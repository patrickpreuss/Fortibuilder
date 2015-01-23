using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Fortibuilder.guts.DB
{
    class Object
    {
        
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Type { get; set; }
        
        public string Ip { get; set; }

        public string Smask { get; set; }

    }
}
