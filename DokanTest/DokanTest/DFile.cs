using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokanTest
{
    public class DFile
    {
        public string Name { get; set; }
        public long Size { get; set; }

        public DFile( string name )
        {
            Name = name;
        }

        public DFile( string name, long size ) : this( name )
        {
            Size = size;
        }
    }
}
