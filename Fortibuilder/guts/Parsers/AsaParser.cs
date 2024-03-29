﻿/*
 * Copyright © 2015 by Timothy Anderson
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
 * with the License. You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
 * the specific language governing permissions and limitations under the License.
 */

using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

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

        private bool _check1, _check2, _check3, _check4, _check5, _check6, _check7, _check8; 
        //delete later
        private readonly bool[] _alloptions;

        private int index = 0;
           
        public AsaParser(string filename, bool[]options)
        {
            _filename = filename;
            _check1 = options[0];
            _check2 = options[1];
            _check3 = options[2];
            _check4 = options[3];
            _check5 = options[4];
            _check6 = options[5];
            _check7 = options[6];
            _check8 = options[7];
            _alloptions = options;
        }


       public void ReadConfiguration(object sender, DoWorkEventArgs e, DataGridView polTable, DataGridView nattable)
       // public Task ReadConfiguration(object sender, ProgressChangedEventArgs e1ProgressChangedEventArgs, DoWorkEventArgs e, DataGridView polTable, DataGridView nattable)
        {
                //setup form data
                var progress = CountLinesInFile(_filename);

                string line;
                int
                    objectsParsedTotal = 0,
                    linesIgnoredTotal = 0,
                    networkObjectsTotal = 0,
                    objectGroupTotal = 0,
                    serviceGroupTotal = 0,
                    serviceObjectTotal = 0,
                    staticRouteTotal = 0,
                    unknownObjectTotal = 0,
                    policylinesTotal = 0,
                    natlinesTotal = 0,
                    pindex = 0;

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
                String[] protocolsmade = new string[10];
                try
                {

                var scripter = new Scripter(_alloptions);
                using (var reader = new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.ASCII))
                {
                    

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
                            case "route":
                                scripter.WriteStaticRoute(results);
                                staticRouteTotal++;
                                goto Getmeoutofhere;
                            case "access-list":
                                //policy variables
                                string polinterfacename = null,
                                    poldescription =null,
                                    polsource = null,
                                    poluser = null,
                                    poldestination = null,
                                    polservice = null,
                                    polaction = null,
                                    pollogging = null;

                                polinterfacename = results[0];
                                
                                switch (results[2])
                                {
                                    case "remark":
                                        poldescription = null;
                                        for (var i =2;i<results.Count();i++)
                                        {
                                            poldescription += String.Format("{0} ",results[i]);
                                        }
                                        goto Getmeoutofhere;
                                    case "extended":
                                        polaction = results[3];
                                        var aclindex = 0;
                                        for (var i = 4; i < results.Count(); i++)
                                        {
                                            if ((results[i] == "object") || (results[i] == "object-group"))
                                            {
                                                
                                            }
                                            else
                                            {
                                                switch (aclindex)
                                                {
                                                    case 0:
                                                        polservice = results[i];
                                                        aclindex++;
                                                        break;
                                                    case 1:
                                                        polsource = results[i];
                                                        aclindex++;
                                                        break;
                                                    case 2:
                                                        poldestination = results[i];
                                                        aclindex = 0;
                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                }

                                
                                Object[] addmepol = 
                                {
                                    polinterfacename,
                                    poldescription,
                                    polsource,
                                    poluser,
                                    poldestination,
                                    polservice,
                                    polaction,
                                    pollogging
                                };

                                polTable.Rows.Add(addmepol);
                                policylinesTotal++;
                                goto Getmeoutofhere;
                                //break;
                            case "nat":

                                //NAT variables
                                string natsourceinterface = null,
                                    natdestinationinterface = null,
                                    natoriginaltype = null,
                                    natoriginalsource = null,
                                    natoriginaldestination = null,
                                    nattranslatedtype = null,
                                    nattranslatedsource = null,
                                    nattranslateddest = null,
                                    natnoproxyarp = null,
                                    natroutelookup = null,
                                    natdescription = null;

                                var natindex = 0;
                                string[] intfces = results[1].Split(',');
                                natsourceinterface = intfces[0].TrimStart('(');
                                natdestinationinterface = intfces[1].TrimEnd(')');

                                //begin determining whether or not we have an IP pool/dynamic or VIP/static NAT
                                switch (results[3])
                                {
                                    case "dynamic":
                                        goto Getmeoutofhere;
                                        break;
                                    case "static":
                                        switch (results[2])
                                        {
                                            case "source":
                                                natoriginaltype = results[3];
                                                natoriginalsource = results[4];
                                                natoriginaldestination = results[5];
                                                if (results[6] == "destination")
                                                {
                                                    nattranslatedtype = results[7];
                                                    nattranslatedsource = results[8];
                                                    nattranslateddest = results[9];
                                                }
                                                var des = false;

                                                for (var i = 10; i < results.Count(); i++)
                                                {
                                                    if (des == true)
                                                    {
                                                        natdescription += String.Format("{0} ", results[i]);
                                                    }
                                                    else
                                                    {
                                                        switch (results[i])
                                                        {
                                                            case "description":
                                                                des = true;
                                                                break;
                                                            case "no-proxy-arp":
                                                                natnoproxyarp = "no-proxy-arp";
                                                                break;
                                                            case "route-lookup":
                                                                natroutelookup = "route-lookup";
                                                                break;
                                                        }
                                                    }
                                                }
                                                Object[] addmenat =
                                                {
                                                    natsourceinterface,
                                                    natdestinationinterface,
                                                    natoriginaltype,
                                                    natoriginalsource,
                                                    natoriginaldestination,
                                                    nattranslatedtype,
                                                    nattranslatedsource,
                                                    natoriginaldestination,
                                                    natnoproxyarp,
                                                    natroutelookup,
                                                    natdescription
                                                };

                                                nattable.Rows.Add(addmenat);
                                                natlinesTotal++;
                                                goto Getmeoutofhere;
                                        }
                                        break;
                                }
                                break;
                            case "object":
                                switch (isanobject)
                                {
                                    case true:
                                    AddnetworkobjecttoDB:
                                        //close and add to db.
                                        string[] addme2 = { objectname, ip, mask, objecttype, description };
                                        scripter.WriteNetworkObject(addme2);
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
                                        networkObjectsTotal++;
                                        objectsParsedTotal++;
                                        break;
                                }

                                switch (isaservice)
                                {
                                    case true:
                                    AddserviceobjecttoDB:
                                        string[] addme3 = { objectname, protocolsource, protocoltype, portrange };
                                        scripter.WriteServiceObject(addme3);
                                        //write to console
                                        writetoconsole = true;
                                        consoleoutput = String.Format("{0}{1} {2}{3} {4}{5}","Adding service:",objectname, "type:", protocoltype, "Portrange:", portrange);
                                        //reset values
                                        isaservice = false;
                                        objectname = null;
                                        objecttype = null;
                                        protocoltype = null;
                                        portrange = null;
                                        objectsParsedTotal++;
                                        serviceObjectTotal++;
                                        break;
                                }

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
                                        networkObjectsTotal++;
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
                                                string[] addme3 = {objectgroupname, groupmembers, description};
                                                scripter.WriteServiceObjectGroup(addme3);
                                                objectgroupname = null;
                                                objectname = null;
                                                protocoltype = null;
                                                groupmembers = null;
                                                portrange = null;
                                                description = null;
                                                serviceGroupTotal++;
                                                break;
                                        }
                                        break;
                                }
                                isingroup = true;
                                objectgrouptype = results[1];
                                objectgroupname = results[2];
                                break;

                            default:
                                linesIgnoredTotal++;
                                //TODO break out of unknown types
                                break;
                        }
                    Outer:
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
                                                        objectname += String.Format("{0}/{1},", results[3], "255.255.255.255");
                                                        goto Getmeoutofhere;
                                                    case "object":
                                                        objectname += String.Format("{0},", results[3]);
                                                        goto Getmeoutofhere;
                                                }
                                                break;
                                        }
                                        objectname += String.Format("{0}{1}", results[3], "|");
                                    }
                                    break;
                                case "group-object":
                                    objectname += String.Format("{0}{1}", results[2], ",");
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
                                            if (results.Count() < 2) { 
                                                protocoltype = results[3];
                                            }
                                            break;
                                    }
                                    break;
                                case "protocol-object":
                                    if (isingroup)
                                    {
                                        var isdonealready = IsServiceCreated(protocolsmade, results[2]);
                                        groupmembers += String.Format("{0}{1}", results[2], "|");
                                        switch (results[2])
                                        {
                                            case "udp":
                                                if (isdonealready) 
                                                {
                                                    break;
                                                }
                                                    string[] addme = { results[2], protocolsource, "udp|", "1-65535|" };
                                                    scripter.WriteServiceObject(addme);
                                                    protocolsmade[pindex] = results[2];
                                                    pindex++;
                                                break;

                                            case"tcp":
                                                if (isdonealready)
                                                {
                                                    break;
                                                }
                                                    string[] addme2 = { results[2], protocolsource, "tcp|", "1-65535|" };
                                                    scripter.WriteServiceObject(addme2);
                                                    protocolsmade[pindex] = results[2];
                                                    pindex++;
                                                break;
                                        }
                                    }
                                    break;
                                case "port-object":
                                    if (isingroup)
                                    {
                                        objecttype = results[2];
                                        switch (results[2])
                                        {
                                            case "range":
                                                if (results[3] == "snmp")
                                                {
                                                    var isdonealready = IsServiceCreated(protocolsmade, results[4]);
                                                    if (isdonealready)
                                                    {
                                                    
                                                    switch (results[4])
                                                    {
                                                        case "snmptrap":
                                                            string[] addme = { results[4], protocolsource, "udp|", "162|" };
                                                            scripter.WriteServiceObject(addme);
                                                            groupmembers += String.Format("{0}{1}", results[4], "|");
                                                            protocolsmade[pindex] = results[4];
                                                            pindex++;
                                                            break;
                                                        case "snmpread":
                                                            string[] addme2 = { results[4], protocolsource, "udp|", "161|" };
                                                            scripter.WriteServiceObject(addme2);
                                                            groupmembers += String.Format("{0}{1}", results[4], "|");
                                                            protocolsmade[pindex] = results[4];
                                                            pindex++;
                                                            break;
                                                    }
                                                    break;
                                                    }
                                                }
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
                        e.Result = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", index, objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal,serviceGroupTotal, serviceObjectTotal,staticRouteTotal, unknownObjectTotal, policylinesTotal, natlinesTotal);
                        
                        switch (writetoconsole)
                        {
                            case true:
                                e.Result = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", index, objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal, serviceGroupTotal, serviceObjectTotal, staticRouteTotal, unknownObjectTotal, policylinesTotal, natlinesTotal, consoleoutput);
                                break;
                                //return String.Format("{0},{1}", index, counters);
                            case false:
                                e.Result = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", index, objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal, serviceGroupTotal, serviceObjectTotal, staticRouteTotal, unknownObjectTotal, policylinesTotal, natlinesTotal, consoleoutput);
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

                    scripter.RunOnceEnd();
                    consoleoutput = Throwindir();
                    e.Result = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", "completed!", objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal, serviceGroupTotal, serviceObjectTotal, staticRouteTotal, unknownObjectTotal, policylinesTotal, natlinesTotal, consoleoutput);
                    //Dispose();
                    return;
                }
            }

            catch (Exception ex)
            {
                //MessageBox.Show();
                e.Result = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", index, objectsParsedTotal, linesIgnoredTotal, networkObjectsTotal, objectGroupTotal, serviceGroupTotal, serviceObjectTotal, staticRouteTotal, unknownObjectTotal, policylinesTotal, natlinesTotal, ex);
                Writetolog(e.Result.ToString());
                Writetolog(Throwindir());
                //Dispose();
                //TODO close files after exception?
                //return "Error!,";
                //backgroundWorker1.Dispose();
            }
        }

        private static void Writetolog(string s)
        {
            var fStream2 = new FileStream(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log.txt"), FileMode.Append);
            var p = DateTime.Now.ToString();

            using (var outfile = new StreamWriter(fStream2))
            {
                outfile.WriteLine( p.ToString() + ":" + s.ToString());
            }
        }

        private string Throwindir()
        {
            //Simple method to throw files in directories as needed.
            try
            {
                //string sourcePath = "/var/lib/tftpboot/";
                var sourcePath = String.Format("{0}temp\\", AppDomain.CurrentDomain.BaseDirectory);

                var p = String.Format("{0}_{1}", _filename.Split('\\').Last(), DateTime.Now.ToString());
                //sanitize invalid filename chars
                p = p.Replace('/', '-');
                p = p.Replace('\\', '_');
                p = p.Replace(':', '_');
                p = p.Replace('*', '_');
                p = p.Replace('?', '_');
                p = p.Replace('"', '_');
                p = p.Replace('<', '_');
                p = p.Replace('>', '_');
                p = p.Replace('|', '_');

                //Combine info into the new dir
                var targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p);

                if (Directory.Exists(sourcePath))
                {
                    string[] files = Directory.GetFiles(sourcePath);
                    // Copy the files and overwrite destination files if they already exist.
                    Directory.CreateDirectory(targetPath);
                    foreach (var s in files)
                    {
                        if (CheckIfFileIsBeingUsed(s)) { ; }
                        File.Move(Path.Combine(s), Path.Combine(targetPath, s.Split('\\').Last()));
                    }
                    Process.Start(@targetPath);
                    return String.Format("{0}", "save successful!");
                }
                else
                {
                   return  String.Format("{0} {1} {2}","Source path", sourcePath,"does not exist!");
                }
            }
            catch (Exception ex)
            {
               return  String.Format("{0} {1}","Error attempting to copy to dir:" ,ex);
            }
        }

        private static bool CheckIfFileIsBeingUsed(string s)
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

        private static bool IsServiceCreated(string[] servicelist, string servicetocheck)
        {
            switch (servicelist.Contains(servicetocheck)) 
            {
                case true:
                    return true; 
            }
            return false;
        }
    }
}
