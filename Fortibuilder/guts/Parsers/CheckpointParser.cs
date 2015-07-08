/*
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Fortibuilder.guts;

namespace Fortibuilder.guts.Parsers
{
    class CheckpointParser
    {
        private string _filename;

        private bool _check1, _check2, _check3, _check4, _check5, _check6, _check7, _check8;
        //delete later
        private readonly bool[] _alloptions;

        public CheckpointParser(string filename, bool[] options)
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

        /*
        public async Task<int> BeginRead(IProgress<int> progress, bool[] options)
        {
            var totalCount = CountLinesInFile(_filename);
            using (
                           int processCount = await Task.Run<int>(() =>
                {
                var reader =
                    new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                        Encoding.ASCII))
                {
 
                    int tempCount = 0;
                    foreach (var image in imageList)
                    {
                        //await the processing and uploading logic here
                        int processed = await ReadConfiguration();
                        if (progress != null)
                        {
                            progress.Report((tempCount * 100 / totalCount));
                        }
                        tempCount++;
                    }
                
                    return tempCount;
                });
            }
            return totalCount;
        }
        
        void ReportProgress(int value)
        {
            //Update the UI to reflect the progress value that is passed back.
        }
        */

        public async Task<int> ReadConfiguration(IProgress<int> progress)
        {
            
            
            var tlines = CountLinesInFile(_filename);
            var index = 0;
            using (
                var reader =
                    new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                        Encoding.ASCII))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    /*
                     * This is simply debug read
                    string[] readline = line.Split(',');
                    
                    scripter.DebugSave(readline);
                    var prognum = (index * 100) % tlines;
                    progress.Report(prognum);
                    index++;
                    */
                    if (line.Contains(":netobj (netobj"))
                    {
                        //begin object parsing
                        await ParseObject(reader, line);

                    }
                    index++;
                    var prognum = (index * 100) % tlines;
                    progress.Report(prognum);
                }
            }
            return 100;
        }

        private async Task ParseObject(StreamReader sr, string line)
        {
            var scripter = new Scripter(_alloptions);
            var objectgroupindex = 0;
            string objectname = null;
            string objecttype = null;
            string ipaddr = null;
            string firstipaddr = null;
            string lastipaddr = null;
            string netmask = null; //temp for sorting
            string description = null;
            string[] groupobjects = null;

            while ((line = sr.ReadLine()) != "			:masters (") //delim?? nope
            {
                if (line.Contains(": ("))
                {
                    switch (objecttype)
                    {
                        case "range":
                            string[] writeme = { objectname, ipaddr, netmask, "range" };
                            scripter.WriteNetworkObject(writeme);
                            //todo sendme to scripter
                            break;
                        case "host":
                            string[] writeme2 = {objectname,ipaddr,"255.255.255.255","host"};
                            scripter.WriteNetworkObject(writeme2);
                            break;
                        case "network":
                            string[] writeme3 = { objectname, ipaddr, netmask, "network" };
                            scripter.WriteNetworkObject(writeme3);
                            break;
                        case "machines_range":
                            scripter.WriteNetworkRangeObject(objectname,firstipaddr,lastipaddr,description);
                            break;
                        case "group":
                            scripter.WriteNetworkObjectGroup( objectname, groupobjects, description);
                            break;
                        case "dynamic_net_obj":
                            //todo figure out how to handle these in fortigate policy
                            break;
                    }

                    objectname = line.TrimStart("		: (".ToCharArray());
                }
                else if (line.Contains(":") && objectname != null && objecttype == "group")
                {
                    //add object to group
                    string[] temp = line.Split(':');
                    groupobjects[objectgroupindex] = temp[1].TrimStart(' ');
                    objectgroupindex++;

                }
                else if (line.Contains(":"))
                {
                    string[] temp = line.Split(':');
                    string[] property = temp[1].Split('(');
                    switch (property[0])
                    {
                        case "add_adtr_rule ":
                            break;
                        case "ipaddr ":
                            ipaddr = property[1].TrimEnd(')');
                            break;
                        case "ipaddr_first ":
                            firstipaddr = property[1].TrimEnd(')');
                            break;
                        case "ipaddr_last ":
                            lastipaddr = property[1].TrimEnd(')');
                            break;
                        case "netmask ":
                            netmask = property[1].TrimEnd(')');
                            break;
                        case "type ":
                            objecttype = property[1].TrimEnd(')');
                            break;
                    }
                }
                else if (line.Contains(')') && objectname != null) //todo fix this statement
                {
                    //close object? need to check
                }
            }
            //return;
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
