using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokanTest
{
    public class DFolder
    {
        public DFolder Root { get; set; }

        public string Name { get; set; }
        public List<DFile> Files { get; protected set; }

        public List<DFolder> Children { get; protected set; }

        public DFolder()
        {
            Files = new List<DFile>();
            Children = new List<DFolder>();
        }

        public DFolder( string name ) : this()
        {
            Name = name;
        }

        public DFile FindFileFrom( string filename )
        {
            DFolder dir = this;
            DFile file = null;

            if ( filename == "\\" )
            {
                dir = Root;
            }

            string [] subdirs = filename.Split( new char [] { '\\' } );
            if ( subdirs.Length > 0 )
            {
                for( int i = 0 ; i < subdirs.Length - 1 ; i++ )
                {
                    var match = dir.Children.FirstOrDefault( x => x.Name == subdirs[ i ] );
                    if ( match == null )
                    {
                        dir = null;
                        break;
                    }
                    else
                    {
                        dir = match;
                    }
                }

                if ( dir != null )
                {
                    file = dir.Files.FirstOrDefault( x => x.Name == subdirs[ subdirs.Length - 1 ] );
                }
            }


            return file;
        }

        public DFolder FindDirectoryFrom( string dirname )
        {
            DFolder dir = this;

            string [] subdirs = dirname.Split( new char [] { '\\' } );
            if ( subdirs.Length > 1 && dirname != "\\" )
            {
                for ( int i = 1 ; i < subdirs.Length ; i++ )
                {
                    var match = dir.Children.FirstOrDefault( x => x.Name == subdirs[ i ] );
                    if ( match == null )
                    {
                        dir = null;
                        break;
                    }
                    else
                    {
                        dir = match;
                    }
                }
            }
            else
            {
                dir = Root;
            }

            return dir;
        }
    }
}
