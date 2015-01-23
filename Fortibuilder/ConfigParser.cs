using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Fortibuilder
{
    class ConfigParser
    {
        private readonly string _filename;
      //  private TextBox writeconsoleline;
      //  private OpenFileDialog open;
      //  private ToolStripLabel label;

        public ConfigParser(string filename1, TextBox writeconsoleline1,  ToolStripLabel label1)
        {
            _filename = filename1;
        //    writeconsoleline = writeconsoleline1;
        //    label = label1;
        }

        public void Parse() 
        {
            FileRead(_filename);
        }

        private static void FileRead(String filename) 
        {

            try
            {
                Stream myStream = null;
                    using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.ASCII))
                    {
                        string line;
                        var index =0;

                        while ((line = reader.ReadLine()) != null)
                        {
                          /*
                            writeconsoleline.Text+=index+line+"\r\n";
                            /*
                            foreach (String itemChecked in fightsList.CheckedItems)
                            {
                                iterates through strings. Not needed right now
                            }*/
                         //   label.Text = index.ToString();
                            index++;
                        }
                        myStream.Dispose();
                        return;
                    }
               // }
            }

            catch (Exception ex)
            {
                MessageBox.Show(String.Format("{0}{1}{2}{3}","Exception handled:", ex.Message,"filename:",filename));
            }
        }
    }
}
