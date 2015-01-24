using System;
using System.IO;
using System.Linq;

namespace Fortibuilder.guts
{
    class Scripter
    {
        //filename to write to
        private string _filename;
        private int _staticroutecounter =0;
        //Run once varibles
        private bool[] _options;
        private bool runonceobjects = true, 
        runonceobjectgroups = true,
        runonceservicegroups = true,
        runservices = true,
        runoncstaticroutes = true;

        public Scripter(bool[]options) //TODO - bool[]useroptions
        {
            _options = options;
            RunOnceStart();
        }
        
        public void RunOnceStart()
        {
            var index = 0;
            var spacer = "  ";

            foreach (var option in _options)
            {
                switch (option)
                {
                    case true:
                        switch (index)
                        {
                            case 0:
                                _filename = "objects.txt";
                                 File.AppendAllText(_filename, String.Format("config firewall address\r\n"));
                                 spacer = "    ";
                                 File.AppendAllText(_filename, String.Format("{0}\"{1}\"\r\n", spacer, "edit all"));
                                 File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
                                break;
                            case 1:
                                _filename = "object_groups.txt";
                                File.AppendAllText(_filename, String.Format("config firewall addrgrp\r\n"));
                                break;
                            case 2:
                                _filename = "services.txt";
                                File.AppendAllText(_filename, String.Format("config firewall service custom\r\n"));
                                break;
                            case 3:
                                _filename = "service_groups.txt";
                                File.AppendAllText(_filename, String.Format("config firewall service group\r\n"));
                                break;
                            case 4:
                                _filename = "static_routes.txt";
                                File.AppendAllText(_filename, String.Format("config router static\r\n"));
                                break;
                        }
                        break;
                }
                index++;
            }
        }

        public void RunOnceEnd()
        {
            var index = 0;

            foreach (var option in _options)
            {
                switch (option)
                {
                    case true:
                        switch (index)
                        {
                                //TODO get rid of filler ends :)
                            case 0:
                                _filename = "objects.txt";
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 1:
                                _filename = "object_groups.txt";
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 2:
                                _filename = "services.txt";
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 3:
                                _filename = "service_groups.txt";
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 4:
                                _filename = "static_routes.txt";
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;                            
                        }   
                        break;
                }
                index++;
            }
        }
        public void WriteNetworkObject(string[] input)
        {
            _filename = "objects.txt";
            
            var objectname = input[0];
            var ip = input[1];
            var smask = input[2];
            var objecttype = input[3];
            string description = null;

            for (var i = 4; i < input.Count(); i++)
            {
                description += String.Format("{0} ",input[i]);
            }
            
            var spacer ="    ";
            
            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectname));

            switch(objecttype){
                case "network":
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set type", objecttype));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2} {3}\r\n", spacer, "set subnet", ip, smask));
                break;
                case "range":
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set type", objecttype));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1}{2}",spacer,"set end-ip"));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1}{2}", spacer, "set start-ip"));
                    break;
            }
               

            switch (input.Count()>=5)
            {
                case true:
                    if (description.TrimEnd() == "")
                    {
                        //do nothing
                    }
                    else { 
                            File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment", description.TrimEnd(' ')));
                         }
                    break;
            }

            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));

        }

        public void WriteNetworkObjectGroup(string[] input)
        {
            _filename = "object_groups.txt";

            var spacer= "    "; 
            var objectgroupname = input[0];
            var objectmembers=input[1].Split('|');
            string objectmemberoutput = null;
            
            //parse object names passed.
            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectgroupname));
            //TODO set uuid if needed. i don't think its neccessary though.
            switch (input.Count() > 3)
            {
                case true:
                    string description = null;
                    for(var i=2;i<input.Count();i++)
                    {
                        description += String.Format("{0} ", input[i]);
                    }
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n",spacer,"set comment ", description.TrimEnd(' ')));
                    break;
            }

            foreach (var name in objectmembers)
            {
                objectmemberoutput += String.Format("\"{0}\" ", name);
            }

            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set member ", objectmemberoutput));
            File.AppendAllText(_filename, String.Format("{0}{0}{1}\r\n", spacer, "next"));
        }


        public void WriteServiceObjectGroup(string[] input)
        {
            //NOTE TO SELF. The only real thing that belongs in this is members and comments for the groups.
            _filename = "service_groups.txt";
            var spacer = "    ";
            var objectgroupname = input[0];
            var groupmembers = input[1];

            //var protocoltypes = input[2];
            //string portrangeoutput = null;

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectgroupname));
            switch (input.Count() > 3)
            {
                case true:
                    string description = null;
                    for (var i = 2; i < input.Count(); i++)
                    {
                        description += String.Format("{0} ", input[i]);
                    }
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment ", description.TrimEnd(' ')));
                    break;
            }

            string groupmembers2 = null;
            foreach (var groupmember in groupmembers)
            {
                groupmembers2 += String.Format("\"{0}\"",groupmember);
            }

            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set member", groupmembers2));
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteStaticRoute(string networkinterface, string route, string nexthop, int metric, int routenumber)
        {
            _filename = "static_routes.txt";
            var spacer = "  ";

            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer,"edit",_staticroutecounter));
            _staticroutecounter++; //increment route counter
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "set device", networkinterface));
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "set dst", route));
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "set gateway", nexthop));
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n",spacer,"set distance",metric));
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n",spacer,"next"));
        }

        public void WriteServiceObject(string[] input)
        {
            _filename="services.txt";
            var servicename = input[0];
            var catagory = input[1];
            var protocol = input[2];
            string[] portrange = input[3].Split('|');
            
            //var editcount = 1;
            var spacer = "    ";

            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n",spacer,"edit",servicename));
            //File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer,"set category",catagory));
            if (protocol.Contains("ICMP"))
            {
                var protocolsused = protocol.Split('|');
                for (var i = 0; i < protocolsused.Count(); i++)
                {
                    var icmptypecode = protocolsused[i].Split('\\');
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}",spacer,"set protocol",icmptypecode[0]));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}", spacer, "set icmptype", icmptypecode[1]));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1}", spacer, "unset icmpcode"));
                }
            }
            else
            {
                var protocolsused = protocol.Split('|');
                string portout = null;
                for(var i=0;i<protocolsused.Count()-1;i++)
                {
                    foreach (var port in portrange)
                    { 
                        portout += String.Format("{0}{1}", port, ',');
                        File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}{3} {4}\r\n", spacer, "set", protocolsused[i], "-portrange", portout.TrimEnd(',')));
                    }
                }
            }
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n",spacer,"next"));
        }

        //Function is for debugging purposes.
        public void DebugSave(string[] stuff)
        {
            var indexme = 0;
            var fullstring = "";

            foreach (var e in stuff)
            {
                fullstring += String.Format("{0}:[{1}]", indexme, e);
                indexme++;
            }
            File.AppendAllText(_filename, String.Format("{0}{1}", fullstring, "\r\n"));
        }
    }
}
