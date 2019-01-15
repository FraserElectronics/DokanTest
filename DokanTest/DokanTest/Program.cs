using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokanTest
{
    class Program
    {
        static void Main( string[] args )
        {
            new DokanTest().Run();
        }
    }

    public class DokanTest
    {
        public DokanTest()
        {            
            _root = new DFolder( "." );
            _root.Root = _root;
            _root.Files.Add( new DFile( "desktop.ini", 123 ) );
            _root.Files.Add( new DFile( "InRoot.txt", 678 ) );
            _root.Children.Add( new DFolder( "SubDir1" ) { Root = _root } );
            _root.Children[ 0 ].Files.Add( new DFile( "desktop.ini", 123 ) );
            _root.Children[ 0 ].Files.Add( new DFile( "file1.txt", 9987 ) );
            _root.Children[ 0 ].Files.Add( new DFile( "file2.txt", 765432 ) );
            _root.Children[ 0 ].Files.Add( new DFile( "file3.txt", 123456 ) );
            _root.Children.Add( new DFolder( "SubDir2" ) { Root = _root } );
            _root.Children[ 1 ].Files.Add( new DFile( "desktop.ini", 123 ) );
            _root.Children[ 1 ].Files.Add( new DFile( "file5.txt", 11223344 ) );
        }

        public void Run()
        {
            using ( TextWriter tw = new StreamWriter( @"c:\temp\dokantestoutput.txt" ) )
            {
                //Console.SetOut( tw );

                var dokanfs = new DFS() { Root = _root };
                dokanfs.Mount( "R:\\", DokanOptions.DebugMode, 1 );
            }
        }

        private DFolder _root;
    }
}
