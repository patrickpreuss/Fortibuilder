using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Fortibuilder.guts.Parsers
{
    class CheckpointParser
    {
        private string _filename;

        public CheckpointParser(string filename)
        {
            _filename = filename;
        }

        public async Task<string> ReadConfiguration()
        {
            string goway = "goaway";
            return goway;
        }
        private static int CountLinesInFile(string f)
        {
            var count = 0;

            using (var r = new StreamReader(f))
            {
                while (r.ReadLine() != null)
                {
                    count++;
                }
            }
            return count;
        }
    }





}
