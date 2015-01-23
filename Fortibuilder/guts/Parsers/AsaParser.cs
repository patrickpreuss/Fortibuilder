using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;

namespace Fortibuilder.guts.Parsers
{
    class AsaParser
    {
        private enum NamedPorts
        {
            ftp = 21,
            ssh = 22,
            telnet = 23,
            smtp = 25,
            tacacs = 49,
            domain = 53,
            www = 80,
            ldap = 389,
            https = 443,
            ldaps = 636,
            sip = 5060 - 5061
            //echo = icmp
        };
        private readonly string _filename;

        private bool _check1 = false,
                     _check2 = false,
                     _check3 = false, 
                     _check4 = false,
                     _check5 = false; 
        //delete later
        private bool[] alloptions;

        private int index = 0;
           
        public AsaParser(string filename, bool[]options)
        {
            _filename = filename;
            _check1 = options[0];
            _check2 = options[1];
            _check3 = options[2];
            _check4 = options[3];
            _check5 = options[4];
            alloptions = options;
        }

        public void ReadConfiguration(object sender, DoWorkEventArgs e)
        {
            try
            {
                //setup form data
                var progress = CountLinesInFile(_filename);
                using (var reader = new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.ASCII))
                {
                    string line;
                    int 
                        objectsParsedTotal = 0,
                        linesIgnoredTotal = 0,
                        networkObjectsTotal = 0,
                        objectGroupTotal = 0,
                        serviceGroupTotal = 0,
                        serviceObjectTotal = 0,
                        unknownObjectTotal = 0;

                    //Temp holder for console output.
                    string consoleoutput = null;
                    //stats and progress bar ints
                    var db = new ASADB("test.db");
                    //All the ASA and fortigate config variables that will be interchanged. 
                    bool isingroup = false,
                        isanobject = false,
                        isaservice = false,
                        isaservicegroup = false,
                        writetoconsole = false;

                    string objectname = null,
                        objectgroupname = null,
                        objecttype = null,
                        groupmembers = null,
                        objectgrouptype = null,
                        ip = null,
                        mask = null,
                        description = null,
                        protocoltype = null,
                        portrange = null,
                        protocolsource = null;

                    var scripter = new Scripter(alloptions);

                    while ((line = reader.ReadLine()) != null)
                    {
                        String[] results = line.Split(' ');
                        //   DebugSave(results); <-This is a debug line
                        switch (results[0])
                        {
                            case "!":
                                //do nothing
                                linesIgnoredTotal++;
                                break;
                            case "":
                                //do nothing.
                                break;
                            case "object":
                                switch (isanobject)
                                {
                                    case true:
                                    AddnetworkobjecttoDB:
                                        //close and add to db.
                                        string[] addme = { objectname, ip, mask, objecttype, description };
                                        scripter.WriteNetworkObject(addme);
                                        //write to console
                                        writetoconsole = true;
                                        consoleoutput = String.Format("{0}{1} {2}{3} {4}{5} {6}{7} {8}{9}",
                                         "Adding object:",
                                        objectname, "type:", objecttype, "IP:", ip, "SM:", mask, "DES:",
                                        description);
                                        //db.WriteObject(addme);
                                        //Clear all values
                                        isanobject = false;
                                        //index = 0;
                                        objectname = null;
                                        objecttype = null;
                                        ip = null;
                                        mask = null;
                                        description = null;
                                        objectsParsedTotal++;
                                        goto Outer;
                                }
                                switch (isaservice)
                                {
                                    case true:
                                    AddserviceobjecttoDB:
                                        string[] addme2 = { objectname, protocolsource, protocoltype, portrange };
                                        scripter.WriteServiceObject(addme2);
                                        //write to console
                                        writetoconsole = true;
                                        consoleoutput = String.Format("{0}{1} {2}{3} {4}{5}","Adding service:",objectname, "type:", protocoltype, "Portrange:", portrange);
                                        //reset values
                                        isaservice = false;
                                        objectname = null;
                                        objecttype = null;
                                        protocoltype = null;
                                        portrange = null;
                                        goto Outer;
                                }

                            Outer:
                                objecttype = results[1];
                                switch (objecttype)
                                {
                                    case "network":
                                        objectname = results[2];
                                        isanobject = true;
                                        break;

                                    case "service":
                                        objectname = results[2];
                                        isaservice = true;
                                        break;
                                }


                                break;
                            case "object-group":
                                switch (isaservice)
                                {
                                    case true:
                                        string[] addme2 = { objectname, protocolsource, protocoltype, portrange };
                                        scripter.WriteServiceObject(addme2);
                                        //write to console
                                        writetoconsole = true;
                                        consoleoutput = String.Format("{0}{1} {2}{3} {4}{5}","Adding service:",objectname, "type:", protocoltype, "Portrange:", portrange);
                                        //reset values
                                        isaservice = false;
                                        objectname = null;
                                        objecttype = null;
                                        protocoltype = null;
                                        portrange = null;
                                        goto Outer;
                                        break;
                                }
                                switch (isanobject)
                                {
                                    case true:
                                        //close and add to db.
                                        string[] addme = { objectname, ip, mask, objecttype, description };
                                        scripter.WriteNetworkObject(addme);
                                        writetoconsole = true;
                                        consoleoutput = String.Format("{0}{1} {2}{3} {4}{5} {6}{7} {8}{9}", "Adding object:", objectname, "type:", objecttype, "IP:", ip, "SM:", mask, "DES:", description);
                                        //db.WriteObject(addme);
                                        //Clear all values
                                        isanobject = false;
                                        //index = 0;
                                        objectname = null;
                                        objecttype = null;
                                        ip = null;
                                        mask = null;
                                        description = null;
                                        objectsParsedTotal++;
                                        break;
                                }
                                switch (isingroup)
                                {
                                    case true:
                                        switch (objectgrouptype)
                                        {
                                            case "network":
                                                string[] addme2 = { objectgroupname, objectname, description };
                                                scripter.WriteNetworkObjectGroup(addme2);
                                                //reset vars
                                                objectgroupname = null;
                                                objectname = null;
                                                description = null;
                                                objectGroupTotal++;
                                                break;
                                            case "protocol":
                                                string[] addme3 = { objectgroupname, objectname, description };
                                                scripter.WriteServiceObjectGroup(addme3);
                                                objectgroupname = null;
                                                objectname = null;
                                                protocoltype = null;
                                                portrange = null;
                                                description = null;
                                                serviceGroupTotal++;
                                                break;
                                        }
                                        break;
                                }

                                isingroup = true;
                                objectgrouptype = results[1];
                                break;

                            default:
                                linesIgnoredTotal++;
                                //TODO break out of unknown types
                                break;
                        }

                        if (results.Count() > 1)
                        {
                            switch (results[1])
                            {
                                case "subnet":
                                    if (isanobject)
                                    {
                                        ip = results[2];
                                        mask = results[3];
                                    }
                                    break;
                                case "host":
                                    if ((isingroup) || (isanobject))
                                    {
                                        ip = results[2];
                                        mask = "255.255.255.255";
                                    }
                                    break;
                                case "description":
                                    if ((isingroup) || (isanobject))
                                    {
                                        for (var i = 2; i < results.Count(); i++)
                                        {
                                            description += string.Format("{0} ", results[i]);
                                        }
                                    }
                                    break;
                                case "network-object":
                                    if (isingroup)
                                    {
                                        //The object name in Objects Db.
                                        switch (Tools.Isanip(results[2]))
                                        {
                                            case true:
                                                objectname += String.Format("{0}/{1},", results[2], results[3]);
                                                break;
                                            case false:
                                                switch (results[2])
                                                {
                                                    case "host":
                                                        objectname += String.Format("{0}/{1},", results[2], "255.255.255.255");
                                                        goto Getmeoutofhere;
                                                    case "object":
                                                        objectname += String.Format("{0},", results[2]);
                                                        goto Getmeoutofhere;
                                                }
                                                break;
                                        }
                                        objectname += String.Format("{0}{1}", results[3], "|");
                                    }
                                    break;
                                case "icmp-object":
                                    if (isingroup)
                                    {
                                        switch (results[2])
                                        {
                                            case "echo-reply":
                                                protocoltype += "|ICMP\\0|";//echoreply
                                                break;
                                            case "unreachable":
                                                protocoltype += "|ICMP\\3|";
                                                break;
                                            case "echo":
                                                protocoltype += "|ICMP\\8|";//echorequest
                                                break;
                                            case "time-exceeded":
                                                protocoltype += "|ICMP\\11|";//time-exceeded
                                                break;
                                            case "info-reply":
                                                protocoltype += "|ICMP\\16|";
                                                break;
                                        }
                                        // protocoltype += String.Format("icmp-{0}{1}", results[2], "|");
                                    }
                                    break;
                                case "service":
                                    switch(isaservice)
                                    {
                                        case true:
                                            switch (results.Count())
                                            {
                                            case 3:
                                                objectname = results[2];
                                                break;
                                            default:
                                            protocoltype += String.Format("{0}{1}", results[2], '|');
                                            protocolsource = results[3];
                                            portrange += results[5];
                                            break;
                                            }
                                            break;
                                    }
                                    switch (isingroup)
                                    {
                                        case true:
                                            objectgroupname = results[2];
                                            protocoltype = results[3];
                                            break;
                                    }
                                    break;
                                case "protocol-object":
                                    if (isingroup)
                                    {
                                        protocoltype += String.Format("{0}{1}", results[2], "|");
                                    }
                                    break;
                                case "port-object":
                                    if (isingroup)
                                    {
                                        objecttype = results[2];
                                        switch (results[2])
                                        {
                                            case "range":
                                                portrange += String.Format("{0}-{1}|", results[3], results[4]);
                                                break;
                                            case "eq":
                                                portrange += String.Format("{0}|", Tools.Isanint(results[3]));
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }

                    Getmeoutofhere:
                        index++;

                        switch (writetoconsole)
                        {
                            case true:
                                var per = index / progress;
                                var counters = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", index, objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal, serviceObjectTotal, unknownObjectTotal, per, consoleoutput);
                               
                                //Form1.ReportProgress(per, String.Format("{0},{1}", index, counters));
                                //ReportProgress this.
                                //writetoconsole = false;
                                
                                break;
                                //return String.Format("{0},{1}", index, counters);
                            case false:
                                var per2 = index / progress;
                                var counters2 = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", index, objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal, serviceObjectTotal, unknownObjectTotal, per2, consoleoutput);
                                //ProgressChangedEventArgs eventArgs = 1;
                                break;
                                //return counters2;

                        }
                        /*    Omit Everything above to debug line to peek at config file.
                         * 
                         *    index++;
                         *    var per = index / progress;
                         *    backgroundWorker1.ReportProgress(per,index); 
                         */
                    }
                    e.Result = "completed!";
                    Dispose();
                    //  return "completed!,";
                }
            }

            catch (Exception ex)
            {
                //MessageBox.Show();
                e.Result = String.Format("{0}{1}{2}{3}", "----Exception handled at *index:",index,"-----\r\n", ex);
                Writetolog(e.Result.ToString());
                Writetolog(Throwindir());
                Dispose();
                //TODO close files after exception?
                //return "Error!,";
                //backgroundWorker1.Dispose();
            }
        }

        public static void Writetolog(string s)
        {
            var fStream2 = new FileStream(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log.txt"), FileMode.Append);
            var p = DateTime.Now.ToString();

            using (var outfile = new StreamWriter(fStream2))
            {
                outfile.WriteLine( p.ToString() + ":" + s.ToString());
            }
        }

        public string Throwindir()
        {
            //Simple method to throw files in directories as needed.
            try
            {
                //string sourcePath = "/var/lib/tftpboot/";
                var sourcePath = AppDomain.CurrentDomain.BaseDirectory;
                var p = DateTime.Now.ToShortDateString();
                p = p.Replace('/', '-');
                var targetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p);

                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);
                    // Copy the files and overwrite destination files if they already exist.
                    Directory.CreateDirectory(targetPath);
                    foreach (var s in files)
                    {
                        if (CheckIfFileIsBeingUsed(s)) { ; }

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
                            return String.Format("{0}", "save successful!");
                    }
                    return String.Format("{0}", "save successful!");
                }
                else
                {
                   return  String.Format("{0} {1} {2}","Source path", sourcePath,"does not exist!");
                }
            }
            catch (Exception ex)
            {
               return  String.Format("{0} {1}","EX:" ,ex);
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

        private void Dispose()
        {
           // Dispose(true); kinda redundant, need to look into why this is in MSDN for object disposal.
            GC.SuppressFinalize(this);
        }

        private int NamedPortconversion(string input)
        {
            int output;

            switch (NamedPorts.TryParse(input, out output))
            {
                case true:
                    return output;
            }
            int.TryParse(input, out output);

            return output;
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
