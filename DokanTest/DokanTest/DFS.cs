using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DokanTest
{
    public class DFS : IDokanOperations
    {
        public DFolder Root { get; set; }

        public void Cleanup( string fileName, DokanFileInfo info )
        {
            Console.WriteLine( $"Cleanup {fileName}" );
        }

        public void CloseFile( string fileName, DokanFileInfo info )
        {
            Console.WriteLine( $"CloseFile {fileName}" );
        }

        public NtStatus CreateFile( string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info )
        {
            NtStatus ret = DokanResult.Error;

            Console.WriteLine( $"CreateFile {fileName},{access.ToString()},{share.ToString()},{mode.ToString()},{options.ToString()},{attributes.ToString()}" );

            if ( fileName.Equals( "InRoot.txt" ) )
            {
                Console.WriteLine();
            }
            if ( info.IsDirectory == true )
            {
                var dir = Root.FindDirectoryFrom( fileName );
                switch ( mode )
                {
                    case FileMode.Append:
                        ret = DokanResult.AccessDenied;
                        break;
                    case FileMode.Create:
                        ret = DokanResult.AccessDenied;
                        break;
                    case FileMode.CreateNew:
                        ret = DokanResult.AccessDenied;
                        break;
                    case FileMode.Open:
                        if ( access == DokanNet.FileAccess.Delete )
                        {
                            ret = DokanResult.AccessDenied;
                        }
                        else
                        {
                            if ( dir == null )
                            {
                                ret = DokanResult.NotADirectory;
                            }
                        }
                        break;
                    case FileMode.OpenOrCreate:
                        ret = DokanResult.AccessDenied;
                        break;
                    case FileMode.Truncate:
                        ret = DokanResult.AccessDenied;
                        break;
                }
            }
            else
            {
                var readWriteAttributes = (access & DataAccess) == 0;
                var readAccess = (access & DataWriteAccess) == 0;

                var dir = Root.FindDirectoryFrom( fileName );
                var file = Root.FindFileFrom( fileName );

                switch ( mode )
                {
                    case FileMode.Append:
                        break;
                    case FileMode.Create:
                        if ( dir != null )
                        {
                            ret = DokanResult.AccessDenied;
                        }
                        break;
                    case FileMode.CreateNew:
                        if ( dir != null || file != null )
                        {
                            ret = DokanResult.FileExists;
                        }
                        break;
                    case FileMode.Open:
                        if ( dir != null || file != null )
                        {
                            if ( readWriteAttributes || dir != null )
                            {
                                if ( ( access & DokanNet.FileAccess.Delete ) == DokanNet.FileAccess.Delete )
                                {
                                    ret = DokanResult.AccessDenied;
                                }
                                else
                                {
                                    info.IsDirectory = ( dir != null );
                                    info.Context = new object();

                                    ret = DokanResult.Success;
                                }
                            }
                        }
                        else
                        {
                            ret = DokanResult.FileNotFound;
                        }
                        break;
                    case FileMode.OpenOrCreate:
                        break;
                    case FileMode.Truncate:
                        if ( dir == null && file == null )
                        {
                            ret = DokanResult.FileNotFound;
                        }
                        break;
                }

                try
                {
                    info.Context = new object();
                    if ( ( dir != null || file != null ) && ( mode == FileMode.OpenOrCreate || mode == FileMode.Create ) )
                    {
                        ret = DokanResult.AlreadyExists;
                    }
                }
                catch ( UnauthorizedAccessException )
                {
                    ret = DokanResult.AccessDenied;
                }
                catch ( DirectoryNotFoundException )
                {
                    ret = DokanResult.PathNotFound;
                }
                catch ( Exception ex )
                {
                    var hr = (uint)Marshal.GetHRForException(ex);
                    switch ( hr )
                    {
                        case 0x80070020: //Sharing violation
                            ret = DokanResult.SharingViolation;
                            break;
                        default:
                            throw;
                    }
                }
            }

            return ret;
        }

        public NtStatus DeleteDirectory( string fileName, DokanFileInfo info )
        {
            Console.WriteLine( $"DeleteDirectory {fileName}" );
            return DokanResult.AccessDenied;
        }

        public NtStatus DeleteFile( string fileName, DokanFileInfo info )
        {
            Console.WriteLine( $"DeleteFile {fileName}" );
            return DokanResult.AccessDenied;
        }
        public NtStatus FindFiles( string fileName, out IList<FileInformation> files, DokanFileInfo info )
        {
            Console.WriteLine( $"FindFiles {fileName}" );

            files = new List<FileInformation>();

            //
            // Root ?
            //
            if ( fileName == "\\" )
            {
                foreach ( var dir in Root.Children )
                {
                    var finfo = new FileInformation
                    {
                        FileName = dir.Name,
                        Attributes = FileAttributes.Directory,
                        LastAccessTime = DateTime.Now,
                        LastWriteTime = DateTime.Now,
                        CreationTime = DateTime.Now
                    };

                    files.Add( finfo );
                }

                foreach ( var file in Root.Files )
                {
                    Console.WriteLine( $"    Adding file {file.Name} as READ-ONLY" );
                    var finfo = new FileInformation
                    {
                        FileName = file.Name,
                        Attributes = FileAttributes.ReadOnly,
                        LastAccessTime = DateTime.Now,
                        LastWriteTime = DateTime.Now,
                        CreationTime = DateTime.Now,
                        Length = file.Size
                    };

                    files.Add( finfo );
                }

                return DokanResult.Success;
            }
            else
            {
                //
                // Find requested directory
                //
                var dir = Root.FindDirectoryFrom( fileName );
                if ( dir == null )
                {
                    return DokanResult.Error;
                }

                foreach ( var file in dir.Children )
                {
                    var finfo = new FileInformation
                    {
                        FileName = dir.Name,
                        Attributes = FileAttributes.Directory,
                        LastAccessTime = DateTime.Now,
                        LastWriteTime = DateTime.Now,
                        CreationTime = DateTime.Now,
                    };

                    files.Add( finfo );
                }

                foreach ( var file in dir.Files )
                {
                    var finfo = new FileInformation
                    {
                        FileName = file.Name,
                        Attributes = FileAttributes.Normal,
                        LastAccessTime = DateTime.Now,
                        LastWriteTime = DateTime.Now,
                        CreationTime = DateTime.Now,
                        Length = file.Size
                    };

                    files.Add( finfo );
                }
                return DokanResult.Success;
            }
        }

        public NtStatus FindFilesWithPattern( string fileName, string searchPattern, out IList<FileInformation> files, DokanFileInfo info )
        {
            Console.WriteLine( $"FindFilesWithPattern {fileName}, {searchPattern}" );

            files = new List<FileInformation>();

            var dir = Root.FindDirectoryFrom( fileName );
            if ( dir != null )
            {
                foreach ( var subdir in dir.Children )
                {
                    if ( DokanHelper.DokanIsNameInExpression( searchPattern, subdir.Name, true ) == true )
                    {
                        files.Add( new FileInformation()
                        {
                            Attributes = FileAttributes.Directory,
                            CreationTime = DateTime.Now,
                            LastAccessTime = DateTime.Now,
                            LastWriteTime = DateTime.Now,
                            Length = 0,
                            FileName = subdir.Name
                        }
                        );
                    }
                }

                foreach ( var file in dir.Files )
                {
                    if ( DokanHelper.DokanIsNameInExpression( searchPattern, file.Name, true ) == true )
                    {
                        files.Add( new FileInformation()
                        {
                            Attributes = FileAttributes.ReadOnly,
                            CreationTime = DateTime.Now,
                            LastAccessTime = DateTime.Now,
                            LastWriteTime = DateTime.Now,
                            Length = file.Size,
                            FileName = file.Name
                        }
                        );
                    }
                }

                return DokanResult.Success;
            }

            return DokanResult.Error;
        }

        public NtStatus FindStreams( string fileName, out IList<FileInformation> streams, DokanFileInfo info )
        {
            Console.WriteLine( $"FindStream {fileName}" );
            streams = new FileInformation[ 0 ];
            return DokanResult.NotImplemented;
        }

        public NtStatus FlushFileBuffers( string fileName, DokanFileInfo info )
        {
            Console.WriteLine( $"FlushFileBuffers {fileName}" );
            return DokanResult.NotImplemented;
        }

        public NtStatus GetDiskFreeSpace( out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info )
        {
            Console.WriteLine( "GetDiskFreeSpace" );

            freeBytesAvailable = 512 * 1024 * 1024;
            totalNumberOfBytes = 1024 * 1024 * 1024;
            totalNumberOfFreeBytes = 512 * 1024 * 1024;

            return DokanResult.Success;
        }

        public NtStatus GetFileInformation( string fileName, out FileInformation fileInfo, DokanFileInfo info )
        {

            Console.WriteLine( $"GetFileInformation {fileName}" );

            fileInfo = new FileInformation { FileName = fileName };
            if ( fileName == "\\" )
            {
                fileInfo.Attributes = FileAttributes.Directory;
                fileInfo.LastAccessTime = DateTime.Now;
                fileInfo.LastWriteTime = DateTime.Now;
                fileInfo.CreationTime = DateTime.Now;
                fileInfo.Length = 1234567;
                return DokanResult.Success;
            }
            else
            {
                var dir = Root.FindDirectoryFrom( fileName );
                if ( dir == null )
                {
                    var file = Root.FindFileFrom( fileName );
                    if ( file != null )
                    {
                        fileInfo.Attributes = FileAttributes.Normal;
                        fileInfo.LastAccessTime = DateTime.Now;
                        fileInfo.LastWriteTime = DateTime.Now;
                        fileInfo.CreationTime = DateTime.Now;
                        fileInfo.Length = file.Size;

                        return DokanResult.Success;
                    }
                    else
                    {
                        return DokanResult.Error;
                    }
                }
                else
                {
                    fileInfo.Attributes = FileAttributes.Directory;
                    fileInfo.LastAccessTime = DateTime.Now;
                    fileInfo.LastWriteTime = DateTime.Now;
                    fileInfo.CreationTime = DateTime.Now;

                    return DokanResult.Success;
                }
            }
        }

        public NtStatus GetFileSecurity( string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info )
        {
            Console.WriteLine( $"GetFileSecurity {fileName}" );
            security = null;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation( out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, DokanFileInfo info )
        {
            Console.WriteLine( "GetVolumeInformation" );

            volumeLabel = "Altair";
            features = FileSystemFeatures.None;
            fileSystemName = "AltairFS";
            maximumComponentLength = 256;

            return DokanResult.Success;
        }

        public NtStatus LockFile( string fileName, long offset, long length, DokanFileInfo info )
        {
            Console.WriteLine( $"LockFile {fileName}" );
            return DokanResult.Success;
        }

        public NtStatus Mounted( DokanFileInfo info )
        {
            Console.WriteLine( "Mounted" );
            return DokanResult.Success;
        }

        public NtStatus MoveFile( string oldName, string newName, bool replace, DokanFileInfo info )
        {
            Console.WriteLine( $"MoveFile [from {oldName} to {newName}]" );
            return DokanResult.AccessDenied;
        }

        public NtStatus ReadFile( string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info )
        {
            bytesRead = buffer.Length;

            for ( int i = 0 ; i < buffer.Length ; i++ )
            {
                buffer[ i ] = ( byte )'A';
            }

            return DokanResult.Success;
        }

        public NtStatus SetAllocationSize( string fileName, long length, DokanFileInfo info )
        {
            Console.WriteLine( $"SeAllocationSize {fileName}" );
            return DokanResult.Error;
        }

        public NtStatus SetEndOfFile( string fileName, long length, DokanFileInfo info )
        {
            Console.WriteLine( $"SetEndOfFile {fileName},{length}" );
            return DokanResult.Error;
        }

        public NtStatus SetFileAttributes( string fileName, FileAttributes attributes, DokanFileInfo info )
        {
            Console.WriteLine( $"SetFileAttributes {fileName},{attributes.ToString()}" );
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity( string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info )
        {
            Console.WriteLine( $"SetFileSecurity {fileName},{sections.ToString()},{sections.ToString()}" );
            return DokanResult.Error;
        }

        public NtStatus SetFileTime( string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info )
        {
            Console.WriteLine( $"SetFileTime {fileName},{ ( ( creationTime != null ) ? creationTime.ToString() : "None" )},{( ( lastWriteTime != null ) ? lastWriteTime.ToString() : "None" )}" );
            return DokanResult.Error;
        }

        public NtStatus UnlockFile( string fileName, long offset, long length, DokanFileInfo info )
        {
            Console.WriteLine( $"UnlockFile {fileName},{offset},{length}" );
            return DokanResult.Success;
        }

        public NtStatus Unmounted( DokanFileInfo info )
        {
            Console.WriteLine( "Unmounted" );
            return DokanResult.Success;
        }

        public NtStatus WriteFile( string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info )
        {
            bytesWritten = buffer.Length;

            Console.WriteLine( $"WriteFile {fileName},{buffer.Length},{offset}" );
            return DokanResult.Success;
        }

        private const DokanNet.FileAccess DataAccess = DokanNet.FileAccess.ReadData | DokanNet.FileAccess.WriteData | DokanNet.FileAccess.AppendData |
                                              DokanNet.FileAccess.Execute |
                                              DokanNet.FileAccess.GenericExecute | DokanNet.FileAccess.GenericWrite |
                                              DokanNet.FileAccess.GenericRead;

        private const DokanNet.FileAccess DataWriteAccess = DokanNet.FileAccess.WriteData | DokanNet.FileAccess.AppendData |
                                                   DokanNet.FileAccess.Delete |
                                                   DokanNet.FileAccess.GenericWrite;
    }
}
