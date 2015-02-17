using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Fortibuilder.guts;
using Fortibuilder.guts.Parsers;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;

namespace Fortibuilder
{

    public partial class Form1 : Form
    {
        public int linecount =0;

        //Begin serialreader shiat TODO cleanup this horrible mess
        SerialPort sp = new SerialPort();
        static bool _continue;
        TextBox bah;
        Thread readThread;
        string name;
        string message;
        delegate void updateTextDelegate(string newText);
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        public delegate void UpdateTextCallback(string message);
        //end serialreader stuff

        //Datatables for policy view
        DataTable policyTable = new DataTable();
        DataView policyView = new DataView();

        //Datatables for NAT view
        DataTable natTable = new DataTable();
        DataView natView = new DataView();

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            toolStripStatusLabel1.Visible = true;
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Text = String.Format("{0}","Loaded form");

            //Turn on all export options for debugging purposes.
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            checkBox4.Checked = true;
            checkBox5.Checked = true;
        }

        public void InitializePolicyTable()
        {
            //temp collection values for policy 
            /*

            var test1 = new DataColumn();
            var test2 = new DataColumn();
            var test3 = new DataColumn();
            var test4 = new DataColumn();
            var test5 = new DataColumn();
            var test6 = new DataColumn();
            var test7 = new DataColumn();
            var test8 = new DataColumn();

            
            policyTable.TableName = "policy";

            test1.ColumnName = "Interface name";
            test2.ColumnName = "Description";
            test3.ColumnName = "Source";
            test4.ColumnName = "User";
            test5.ColumnName = "Destination";
            test6.ColumnName = "Service";
            test7.ColumnName = "Action";
            test8.ColumnName = "Logging";

            policyTable.Columns.Add(test1);
            policyTable.Columns.Add(test2);
            policyTable.Columns.Add(test3);
            policyTable.Columns.Add(test4);
            policyTable.Columns.Add(test5);
            policyTable.Columns.Add(test6);
            policyTable.Columns.Add(test7);
            policyTable.Columns.Add(test8);
             */
            //policyView.DoubleBuffered(true);
        }

        public void InitializeNatTable()
        {
            /*
            natTable.TableName = "nat table";

            var test1 = new DataColumn();
            var test2 = new DataColumn();
            var test3 = new DataColumn();
            var test4 = new DataColumn();
            var test5 = new DataColumn();
            var test6 = new DataColumn();
            var test7 = new DataColumn();
            var test8 = new DataColumn();
            var test9 = new DataColumn();
            var test10 = new DataColumn();
            var test11 = new DataColumn();

            test1.ColumnName = "Source Interface";
            test2.ColumnName = "Destination Interface";
            test3.ColumnName = "Original NAT Type";
            test4.ColumnName = "Original Source";
            test5.ColumnName = "Original Destination";
            test6.ColumnName = "Translated NAT Type";
            test7.ColumnName = "Translated Source";
            test8.ColumnName = "Translated Destination";
            test9.ColumnName = "No Proxy ARP";
            test10.ColumnName = "Route-lookup";
            test11.ColumnName = "Description";

            policyTable.Columns.Add(test1);
            policyTable.Columns.Add(test2);
            policyTable.Columns.Add(test3);
            policyTable.Columns.Add(test4);
            policyTable.Columns.Add(test5);
            policyTable.Columns.Add(test6);
            policyTable.Columns.Add(test7);
            policyTable.Columns.Add(test8);
            policyTable.Columns.Add(test9);
            policyTable.Columns.Add(test10);
            policyTable.Columns.Add(test11);
             */
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void IPresultButton_Click(object sender, EventArgs e)
        {
          var tool = new Tools();
            tool.ParseIPList(fromIP.Text, toIP.Text);
        }

        private void Showerror(string error)
        {
            MessageBox.Show(error);
            Writelineconsole("Error!: " + error);
        }

        private void Writelineconsole(string s)
        {
            consoletextBox.Text += string.Format("{0}{1}",s,"\r\n");
        }


        private void OpenFunction(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = 100;
            toolStripProgressBar1.Value = 0;

            var options = new bool[]
                      {
                          checkBox1.Checked,
                          checkBox2.Checked,
                          checkBox3.Checked,
                          checkBox4.Checked,
                          checkBox5.Checked
                      };

            var open = new OpenFileDialog();

            open.Filter = String.Format("{0}|{1}", "ASA configuration (*.cfg)|*.cfg", "Checkpoint web export index (index.xml)|*.xml");
            open.Title = String.Format("{0}", "Open Configuration");
            button5.Enabled = true;

            switch (open.ShowDialog() == DialogResult.OK)
            {
                case true:
                    //Make lables visible
                    label4.Visible = true; //objects parsed
                    label6.Visible = true; //lines ignored
                    label12.Visible = true; //network objects
                    label13.Visible = true; //object groups
                    label19.Visible = true; //service groups
                    label14.Visible = true; //service objects
                    label17.Visible = true; //unknown
                    label8.Visible = true; //total lines read

                    var filename = open.FileName;

                    switch (open.FilterIndex)
                    {
                        case 1:
                            //ASA configuration file
                            var asaparser = new AsaParser(filename, options);
                            var worker = new BackgroundWorker();
                            worker.WorkerReportsProgress = true;
                            worker.WorkerSupportsCancellation = true;

                            worker.ProgressChanged += (sender1, eventArgs) =>
                            {
                                RefreshCounters(eventArgs);
                                Textrefresh();
                                //backgroundWorker1_ProgressChanged(sender1, eventArgs);
                            };

                            worker.DoWork += (sender1, e1) =>
                            {
                                asaparser.ReadConfiguration(sender1, e1, policyTable, natdataGridView2);

                                var sw = Stopwatch.StartNew();
                                while (sw.Elapsed.TotalSeconds < 1)
                                {
                                    switch (e1.Result.ToString())
                                    {
                                        case "completed!":
                                            break;
                                        case "error!":
                                            break;
                                        default:
                                            worker.ReportProgress(10, e1);
                                            //worker.ReportProgress(10,"poop"); <- REPORTSSSSSSSSS!!!!!111!!!
                                            break;
                                    }
                                    /*
                                    if ((sw.Elapsed.TotalMilliseconds % 100) == 0)
                                        toolStripProgressBar1.Value = 50;
                                        Writelineconsole(String.Format("{0}{1}", "out:", e1));
                                        ((BackgroundWorker)sender).ReportProgress();
                                    ++i;
                                  */
                                }
                            };
                            worker.RunWorkerCompleted += (sender1, eventArgs) =>
                            {
                                //Writelineconsole("completed!");
                                RefreshCounters(eventArgs);

                                // do something on the UI thread, like
                                // update status or display "result"
                            };
                            worker.RunWorkerAsync();
                            break;
                        case 2:
                            //Checkpoint configuration file
                            var checkpointparser = new CheckpointParser(filename);
                            Task<string> returnedstring2 = checkpointparser.ReadConfiguration();
                            break;

                    }
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = 100;
            toolStripProgressBar1.Value = 0;

            var options = new bool[]
                      {
                          checkBox1.Checked,
                          checkBox2.Checked,
                          checkBox3.Checked,
                          checkBox4.Checked,
                          checkBox5.Checked
                      };

            var open = new OpenFileDialog();

            open.Filter = String.Format("{0}|{1}", "ASA configuration (*.cfg)|*.cfg", "Checkpoint web export index (index.xml)|*.xml");
            open.Title = String.Format("{0}","Open Configuration");
            button5.Enabled = true;

            switch (open.ShowDialog() == DialogResult.OK)
            {
                case true:
                //Make lables visible
                    label4.Visible = true; //objects parsed
                    label6.Visible = true; //lines ignored
                    label12.Visible = true; //network objects
                    label13.Visible = true; //object groups
                    label19.Visible = true; //service groups
                    label14.Visible = true; //service objects
                    label17.Visible = true; //unknown
                    label8.Visible = true; //total lines read

                    var filename = open.FileName;

                    switch (open.FilterIndex)
                    {
                        case 1:
                            //ASA configuration file
                            var asaparser = new AsaParser(filename, options);
                            var worker = new BackgroundWorker();
                            worker.WorkerReportsProgress = true;
                            worker.WorkerSupportsCancellation = true;

                            worker.ProgressChanged += (sender1, eventArgs) =>
                            {
                                RefreshCounters(eventArgs);
                                Textrefresh();
                                //backgroundWorker1_ProgressChanged(sender1, eventArgs);
                            };

                            worker.DoWork += (sender1, e1) =>
                            {
                                asaparser.ReadConfiguration(sender1, e1, policyTable, natdataGridView2);

                                var sw = Stopwatch.StartNew();

                                while (sw.Elapsed.TotalSeconds < 0.2)
                                {
                                    var s = e1.ToString();
                                    string[] res = s.Split(',');
                                    //var progress = res[res.Count()];
                                    //var s1 = Convert.ToInt32(res[9]);
                                    
                                    switch (e1.ToString())
                                    {
                                        case "completed!":
                                            sw.Stop();
                                            break;
                                        case "error!":
                                            break;
                                        default:
                                            //RefreshCounters(res);
                                            worker.ReportProgress(10, e1);
                                            //Textrefresh();
                                            //worker.ReportProgress(10,"poop"); <- REPORTSSSSSSSSS!!!!!111!!!
                                            break;
                                    }
                                }
                            };
                            worker.RunWorkerCompleted += (sender1, eventArgs) =>
                            {
                                Writelineconsole("completed!");
                                RefreshCounters(eventArgs);
                                textrefresh();
                                // do something on the UI thread, like
                                // update status or display "result"
                            };
                            worker.RunWorkerAsync();
                            break;
                        case 2:
                            //Checkpoint configuration file
                            var checkpointparser = new CheckpointParser(filename);
                            Task<string> returnedstring2 = checkpointparser.ReadConfiguration();
                            break;
                            
                    }
                   break;
            }
                             
        }

        private int GetProgress(int index, int totallinecount)
        {
            var progress = index/totallinecount;
            return progress;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            consoletextBox.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // foreach
        }

        /*
        private void backgroundWorker1_DoWork(object sender,  DoWorkEventArgs open)
        {
            //get rid of this later
        }
        */

        private void RefreshCounters(string[] e)
        {
            
            if ((e != null) &&(e.Count()<2))
            {
                
                toolStripStatusLabel1.Text = String.Format("{0}", "Running");

                label8.Text = e[0]; //lines read
                label4.Text = e[1]; //objects parsed - incorrect 
                label6.Text = e[2]; //lines ignored
                label12.Text = e[3]; //network objects
                label13.Text = e[4]; //object groups
                label19.Text = e[5];
                label14.Text = e[6]; //service objects
                label17.Text = e[7]; //unknown objects
                foreach (var c in e)
                {
                    Writelineconsole(c.ToString());
                }
            }
            textrefresh();
        }

        private void RefreshCounters(ProgressChangedEventArgs e)
        {
            var stuff = e.UserState.ToString();
            if ((stuff != null)&&(stuff.Count()<2))
            {
                string[] counters = stuff.Split(',');
                toolStripStatusLabel1.Text = String.Format("{0}", "Running");

                label8.Text = counters[0]; //lines read
                label4.Text = counters[1]; //objects parsed - incorrect 
                label6.Text = counters[2]; //lines ignored
                label12.Text = counters[3]; //network objects
                label13.Text = counters[4]; //object groups
                label19.Text = counters[5];
                label14.Text = counters[6]; //service objects
                label17.Text = counters[7]; //unknown objects
                foreach (var c in counters)
                {
                    Writelineconsole(c.ToString());
                }
            }
            Textrefresh();
        }

        private void RefreshCounters(RunWorkerCompletedEventArgs e)
        {
            var stuff = e.Result as String;
            if (stuff != null)
            {
                string[] counters = stuff.Split(',');
                toolStripStatusLabel1.Text = String.Format("{0}", "Completed");

                label8.Text = counters[0]; //lines read
                label4.Text = counters[1]; //objects parsed - incorrect 
                label6.Text = counters[2]; //lines ignored
                label12.Text = counters[3]; //network objects
                label13.Text = counters[4]; //object groups
                label19.Text = counters[5];
                label14.Text = counters[6]; //service objects
                label17.Text = counters[7]; //unknown objects
                foreach (var c in counters)
                {
                    Writelineconsole(c.ToString());
                }
            }
            Textrefresh();
        }

        private static int CountLinesInFile(string f)
        {
            var count = 0;

            using (var r = new StreamReader(f))
            {
                while (r.ReadLine()!= null)
                {
                    count++;
                }
            }
            return count;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //consoletextBox.Text += String.Format("{0}{1}", "arggshhhhh", "\r\n");
            var stuff = e.UserState as String;
            if (stuff != null)
            {
                string[] counters = stuff.Split(',');
                //TODO - Fix all this shit. labels are all wrong..... grand total label update (label8.Text)
                label8.Text = counters[0]; //lines read
                label4.Text = counters[1]; //objects parsed - incorrect 
                label6.Text = counters[2]; //lines ignored
                label12.Text = counters[3]; //network objects
                label13.Text = counters[4]; //object groups
                label19.Text = counters[5];
                label14.Text = counters[6]; //service objects
                label17.Text = counters[7]; //unknown objects

                for (var i = 9; i < counters.Count(); i++)
                {
                    consoletextBox.Text += String.Format("{0},{1}", counters[i], "\r\n");
                }

                toolStripProgressBar1.Value = Convert.ToInt32(counters[7]);
                Textrefresh();
            }
            //Line to troubleshoot counters  
            //TODO - fix the progress bar.
        }

        public void Textrefresh()
        {
            consoletextBox.SelectionStart = consoletextBox.Text.Length;
            consoletextBox.ScrollToCaret();
            consoletextBox.Refresh();

            //statistics refresh.. dump if causing issues later.
            label4.Refresh();
            label6.Refresh();
            label12.Refresh();
            label13.Refresh();
            label14.Refresh();
            label17.Refresh();
            label19.Refresh();
            label8.Refresh();
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var stuff = e.UserState as String;
            if (stuff != null)
            {
                string[] counters = stuff.Split(',');
                toolStripStatusLabel1.Text = String.Format("{0}", "Completed");
                
                label8.Text = counters[0]; //lines read
                label4.Text = counters[1]; //objects parsed - incorrect 
                label6.Text = counters[2]; //lines ignored
                label12.Text = counters[3]; //network objects
                label13.Text = counters[4]; //object groups
                label19.Text = counters[5];
                label14.Text = counters[6]; //service objects
                label17.Text = counters[7]; //unknown objects
                foreach (var c in counters)
                {
                    Writelineconsole(c.ToString());
                }
            }
            toolStripProgressBar1.Visible = false;
            Textrefresh();
            backgroundWorker1.Dispose();
            button5.Enabled = false;
            //TODO put completed message in later. MessageBox.Show(String.Format("{0}", "Completed. Total processing time:something"));
        }
        
        /*
         * This function is used to peek at files during debugging.
         */
        
        private void DebugSave(string[] stuff)
        {
            var indexme = 0; 
            var fullstring = "";

            foreach (var e in stuff)
            {
                fullstring += String.Format("{0}:[{1}]",indexme,e);
                indexme++;
            }

            File.AppendAllText("results.txt", String.Format("{0}{1}", fullstring,"\r\n"));
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format("{0}\r\n{1}\r\n{2}", "Forti-builder","All code within this application was written by Timothy Anderson - timo691@hotmail.com.","Send all usage questions/comments/bugs that way :)."));
        }

        private void objectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (objectsToolStripMenuItem.Checked)
            {
                case true:
                    objectsToolStripMenuItem.Checked = false;
                    checkBox1.Checked = false;
                    break;                
                case false:
                    objectsToolStripMenuItem.Checked = true;
                    checkBox1.Checked = true;
                    break;
            }
        }

        private void serviceObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (serviceObjectsToolStripMenuItem.Checked)
            {
                case true:
                    serviceObjectsToolStripMenuItem.Checked = false;
                    checkBox2.Checked = false;
                    break;                
                case false:
                    serviceObjectsToolStripMenuItem.Checked = true;
                    checkBox2.Checked = true;
                    break;
            }
        }

        private void serviceGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (serviceGroupsToolStripMenuItem.Checked)
            {
                case true:
                    serviceGroupsToolStripMenuItem.Checked = false;
                    checkBox3.Checked = false;
                    break;                
                case false:
                    serviceGroupsToolStripMenuItem.Checked = true;
                    checkBox3.Checked = true;
                    break;
            }
        }

        private void addressGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (addressGroupsToolStripMenuItem.Checked)
            {
                case true:
                    addressGroupsToolStripMenuItem.Checked = false;
                    checkBox4.Checked = false;
                    break;                
                case false:
                    addressGroupsToolStripMenuItem.Checked = true;
                    checkBox4.Checked = true;
                    break;
            }
        }

        private void staticRoutesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (staticRoutesToolStripMenuItem.Checked)
            {
                case true:
                    staticRoutesToolStripMenuItem.Checked = false;
                    checkBox5.Checked = false;
                    break;
                case false:
                    staticRoutesToolStripMenuItem.Checked = true;
                    checkBox5.Checked = true;
                    break;
            }
        }

        public void Throwindir()
        {
            //Simple method to throw files in directories as needed.
            try
            {
                //string sourcePath = "/var/lib/tftpboot/";
                var sourcePath = AppDomain.CurrentDomain.BaseDirectory;
                var p = DateTime.Now.ToShortDateString();
                p = p.Replace('/', '-');
                var targetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,p);

                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);
                    // Copy the files and overwrite destination files if they already exist.
                    
                    Directory.CreateDirectory(targetPath);
                    foreach (var s in files)
                    {
                        if (CheckIfFileIsBeingUsed(s)) { }
                        else
                        {
                            string[] filenames = { "object_groups.txt", "objects.txt", "service_groups.txt", "services.txt", "static_routes.txt" };

                            for (var i = 0; i > filenames.Count(); i++)
                            {
                                var destfilename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p, filenames[i]);
                                File.Move(System.IO.Path.Combine(sourcePath, filenames[i]), destfilename);
                            }

                            /*
                            // Use static Path methods to extract only the file name from the path.
                            fileName = System.IO.Path.GetFileName(s);
                            destFile = System.IO.Path.Combine(targetPath, fileName);

                            if (File.Exists(destFile))
                                File.Delete(destFile);
                            System.IO.File.Move(s, destFile);
                            */
                        }
                    }
                     
                }
                else
                {
                    Writelineconsole("Source path " + sourcePath + " does not exist!");
                }
            }
            catch (Exception ex)
            {
                Writelineconsole("EX: " + ex.ToString());
            }
        }

        public static bool CheckIfFileIsBeingUsed(string s)
        {
            try
            {
                var fs = File.Open(s, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fs.Dispose();
            }
            catch (Exception exp)
            {
                return true;
            }
            return false;

        }

        private void openASAConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFunction(sender,e);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var bah = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM");
            var i = 1;
            get_comsettings(sp);

            foreach (var s in bah.GetValueNames())
            {
                consoletextBox.Text += String.Format("{0}{1}", bah.GetValue(s),"\r\n");
                comporttextBox9.Text = String.Format("{0}{1}",bah.GetValue(s), "\r\n");
                i++;
            }

            
            baudratetextBox5.Text = sp.BaudRate.ToString();
            stopbitstextBox3.Text = sp.StopBits.ToString();
            paritytextBox4.Text = sp.Parity.ToString();
            databitstextBox6.Text = sp.DataBits.ToString();
            readtimeouttextBox7.Text = sp.ReadTimeout.ToString();
        }
    
    private void get_comsettings(SerialPort a) 
        {
            consoletextBox.Text += String.Format("{0},{1},{2},{3},{4},{5},{6}{7}", "Comm port settings: ",a.PortName,  a.BaudRate , a.StopBits , a.Parity , a.DataBits , a.ReadTimeout,"\r\n"); 
        }

    private void button9_Click(object sender, System.EventArgs e)
    {
        try
        {
            var a = Convert.ToInt32(baudratetextBox5.Text);
            var b = Convert.ToInt32(databitstextBox6.Text);

            sp.PortName = comporttextBox9.Text;
            sp.BaudRate = a;
            sp.DataBits = b;
            //TryToParse(textBox2.Text, a);
            get_comsettings(sp);
        }
        catch (System.Exception ex)
        {
            consoletextBox.Text += String.Format("{0}{1}{2}", "Error: ", ex.Message, "\r\n");
        }
    }

    private void button11_Click(object sender, System.EventArgs e)
    {
        try
        {
            _continue = true;
            sp.ReadTimeout = 500;
            sp.PortName = comporttextBox9.Lines[0];
            sp.Open();
            readThread = new Thread(Read);
            readThread.IsBackground = true;
            readThread.Start();
            consoletextBox.Text += String.Format("{0}{1}{2}", sp.PortName, " opened", "\r\n");
            updatetext(sp.PortName + " opened.");
        }
        catch (System.Exception ex)
        {
            consoletextBox.Text += String.Format("{0}{1}{2}","Initial Error: ",ex.Message,"\r\n");
        }
    }

    private void button10_Click(object sender, System.EventArgs e)
    {
        try
        {
            _continue = false;
            readThread.Suspend();
            sp.Close();
            consoletextBox.Text += String.Format("{0}{1}{2}",sp.PortName," closed","\r\n");
            updatetext(sp.PortName + " closed.");
        }
        catch (System.Exception ex)
        {
            consoletextBox.Text += String.Format("{0}{1}{2}","Error: ",ex,"\r\n");
        }
    }

    private void updatetext(string s)
    {
        if (bah.InvokeRequired)
        {
            // this is worker thread 

           var del = new updateTextDelegate(updatetext);
            bah.Invoke(del, new object[] { s });
        }
        else
        {
            // this is UI thread
            bah.Text += String.Format("{0}{1}", s, "\r\n");
            textrefresh();
        }
        //bah.Text += s = "\r\n";
    }

    private void textrefresh()
    {
        bah.SelectionStart = bah.Text.Length;
        bah.ScrollToCaret();
        bah.Refresh();

        //audit refresh
        consoletextBox.SelectionStart = bah.Text.Length;
        consoletextBox.ScrollToCaret();
        consoletextBox.Refresh();
    }

    private void button12_Click(object sender, System.EventArgs e)
    {
        _continue = false;

        try
        {
            //write line to serial port
            sp.WriteLine(textBox8.Text);
            //clear the text box
            textBox8.Text = "";
            _continue = true;
        }
        catch (System.Exception ex)
        {
            consoletextBox.Text += String.Format("{0}{1}{2}", "Error: ", ex, "\r\n");
            _continue = true;
        }
    }

    private void button14_Click(object sender, System.EventArgs e)
    {

        try
        {
            sp.Open();
            sp.ReadTimeout = 500;
            consoletextBox.Text += String.Format("{0}{1}{2}", sp.PortName, " opened", "\r\n");
        }
        catch (System.Exception ex)
        {
            consoletextBox.Text += String.Format("{0}{1}{2}", "Error: ", ex, "\r\n");
        }

        sp.Close();
        consoletextBox.Text += String.Format("{0}{1}{2}", sp.PortName, " closed","\r\n");
    }

    private void button15_Click(object sender, System.EventArgs e)
    {
        consoletextBox.Text = "";
        consoletextBox.Update();
    }

    private void textBox8_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        switch (e.KeyValue)
        {
            case 13:
                send(textBox8.Text);
                break;
        }
    }

    private void Read()
    {
        while (_continue)
        {

            try
            {

                Thread.Sleep(100);
                //foreach (char s in sp.ReadExisting()) { updatetext(s.ToString());  }
                //bah.Text += message + "\r\n";
                //string message = sp.ReadLine();
                var bytes = sp.BytesToRead;

                byte[] buffer = new byte[bytes];

                sp.Read(buffer, 0, bytes);

                var message = "";
                foreach (var b in buffer)
                {
                    message += b;
                }
                message = Encoding.ASCII.GetString(buffer);
                if (message == "") { }
                else
                {
                    bah.BeginInvoke(new UpdateTextCallback(updatetext), new object[] { message.ToString() });
                }

                //string message = sp.Read(buffer);

                //bah.Show();
                //textrefresh();

            }
            catch (TimeoutException) { }
            catch (Exception ex) { updatetext(ex.ToString()); }
        }
    }

    private void send(string s)
    {
        _continue = false;
        try
        {
            //write line to serial port
            sp.WriteLine(s);
            //clear the text box
            textBox8.Text = "";
            //textrefresh();
            //string s = sp.ReadLine();
            //Zupdatetext(s);
            _continue = true;
        }
        catch (System.Exception ex)
        {
            consoletextBox.Text += String.Format("{0}{1}{2}", "Error: ", ex, "\r\n");
        }
    }

    private void serialConnectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        tabControl1.SelectedTab = tabPage3;
    }

    private void sSHConnectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        tabControl1.SelectedTab = tabPage4;
    }

    }
    public class DownloadStringTaskAsyncExProgress
    {
        public int ProgressPercentage { get; set; }
        public string Text { get; set; }
    }

}