using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Renci.SshNet;

namespace Fortibuilder.guts
{
    class Scripter
    {
        //filename to write to
        private string _filename;
        private int _staticroutecounter =0;
        //Run once varibles
        private bool[] _options;

        //todo make dynamic
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
                                 _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory+"\\temp\\", "objects.txt");
                                 File.AppendAllText(_filename, String.Format("config firewall address\r\n"));
                                 spacer = "    ";
                                 File.AppendAllText(_filename, String.Format("{0}\"{1}\"\r\n", spacer, "edit all"));
                                 File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
                                break;
                            case 1:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory+"\\temp\\","object_groups.txt");
                                File.AppendAllText(_filename, String.Format("config firewall addrgrp\r\n"));
                                break;
                            case 2:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "services.txt");
                                File.AppendAllText(_filename, String.Format("config firewall service custom\r\n"));
                                break;
                            case 3:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "service_groups.txt");
                                File.AppendAllText(_filename, String.Format("config firewall service group\r\n"));
                                break;
                            case 4:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "static_routes.txt");
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
                            case 0:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "objects.txt");
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 1:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "object_groups.txt");
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 2:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "services.txt");
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 3:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "service_groups.txt");
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;
                            case 4:
                                _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "static_routes.txt");
                                File.AppendAllText(_filename, String.Format("{0}", "end"));
                                break;                            
                        }   
                        break;
                }
                index++;
            }
        }

        public void WriteStaticRoute(string[] input)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "static_routes.txt");
            var interfacename = input[1];
            var networkaddress = input[2];
            var networkmask = input[3];
            var nexthop = input[4];
            var metric = input[5];

            File.AppendAllText(_filename, String.Format(" {0} {1}\r\n", "edit", _staticroutecounter));
            File.AppendAllText(_filename, String.Format("  {0} {1}\r\n", "set device", interfacename));
            File.AppendAllText(_filename, String.Format("  {0} {1} {2}\r\n", "set dst", networkaddress, networkmask));
            File.AppendAllText(_filename, String.Format("  {0} {1}\r\n", "set gateway", nexthop));
            File.AppendAllText(_filename, String.Format("  {0} {1}\r\n", "set distance", metric));
            File.AppendAllText(_filename, String.Format(" {0}\r\n", "next"));
            _staticroutecounter++;
        }

        public void WriteNetworkObject(string[] input)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "objects.txt");
            
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
                    File.AppendAllText(_filename, String.Format("{0}{0}{1}{2}\r\n", spacer, "set end-ip"));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1}{2}\r\n", spacer, "set start-ip"));
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

        public void WriteNetworkObject(string objectname, string ip, string smask, string objecttype, string description)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "objects.txt");

            var spacer = "    ";

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectname));
            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set type", objecttype));
            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2} {3}\r\n", spacer, "set subnet", ip, smask));
            if (description != null)
            {
                File.AppendAllText(_filename,String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment", description.TrimEnd(' ')));
            }
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteNetworkRangeObject(string objectname, string firstip, string lastip, string description)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "objects.txt");

            var spacer = "    ";

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectname));
            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set type", "iprange"));
            File.AppendAllText(_filename, String.Format("{0}{0}{1}{2}\r\n", spacer, "set end-ip",lastip));
            File.AppendAllText(_filename, String.Format("{0}{0}{1}{2}\r\n", spacer, "set start-ip",firstip));
            if (description != null)
            {
                File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment", description.TrimEnd(' ')));
            }
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteHostkObject(string objectname, string ip, string description)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "objects.txt");

            var spacer = "    ";

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectname));
            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2} {3}\r\n", spacer, "set subnet", ip, "255.255.255.255"));
            if (description != null) 
            { 
                File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment", description.TrimEnd(' ')));
            }
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteNetworkObjectGroup(string[] input)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "object_groups.txt");

            var spacer= "    "; 
            var objectgroupname = input[0];
            var objectmembers=input[1].Split(',');
            string objectmemberoutput = null;
            
            //parse object names passed.
            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectgroupname));
            //TODO set uuid if needed. i don't think its neccessary though.
            switch (input.Count() > 2)
            {
                case true:
                    string description = null;
                    for(var i=2;i<input.Count();i++)
                    {
                        description += String.Format("{0} ", input[i]);
                    }
                    switch (description)
                    {
                        case " ":
                            break;
                        default:
                            File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment ", description.TrimEnd(' ')));
                            break;
                    }
                    break;
            }

            foreach (var name in objectmembers)
            {
                if (name == "")
                {
                }
                else 
                { 
                    objectmemberoutput += String.Format("\"{0}\" ", name);
                }
            }

            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set member ", objectmemberoutput));
            File.AppendAllText(_filename, String.Format("{0}{0}{1}\r\n", spacer, "next"));
        }

        public void WriteNetworkObjectGroup(string objectgroupname, string[] objectmembers, string description)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "object_groups.txt");

            var spacer = "    ";
            string objectmemberoutput = null;

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectgroupname));
            //TODO set uuid if needed. i don't think its neccessary though.
            File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment ", description.TrimEnd(' ')));
                           
            foreach (var name in objectmembers)
            {
                if (name == "")
                {
                }
                else
                {
                    objectmemberoutput += String.Format("\"{0}\" ", name);
                }
            }

            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set member ", objectmemberoutput));
            File.AppendAllText(_filename, String.Format("{0}{0}{1}\r\n", spacer, "next"));
        }

        public void WriteServiceObject(string[] input)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "services.txt");
            var servicename = input[0];
            var catagory = input[1];
            var protocol = input[2];
            var portrange = input[3].Replace('|',',');

            //var editcount = 1;
            var spacer = "    ";

            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "edit", servicename));
            //File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer,"set category",catagory));
            if (protocol.Contains("ICMP"))
            {
                var protocolsused = protocol.Split('|');
                for (var i = 0; i < protocolsused.Count(); i++)
                {
                    var icmptypecode = protocolsused[i].Split('\\');
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set protocol", icmptypecode[0]));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set icmptype", icmptypecode[1]));
                    File.AppendAllText(_filename, String.Format("{0}{0}{1}\r\n", spacer, "unset icmpcode"));
                }
            }

            else
            {
                var protocolsused = protocol.Split('|');
                //string portout = null;
                for (var i = 0; i < protocolsused.Count() - 1; i++)
                {
                        //portout += String.Format("{0}{1}", portrange, ',');
                        File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}{3} {4}\r\n", spacer, "set", protocolsused[i], "-portrange", portrange.TrimEnd(',')));
                
                }
            }
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteServiceObjectGroup(string[] input)
        {
            //NOTE TO SELF. The only real thing that belongs in this is members and comments for the groups.
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "service_groups.txt");
            var spacer = "    ";
            var objectgroupname = input[0];
            string[] groupmembers = input[1].Split('|');
            //var protocoltypes = input[2];
            //string portrangeoutput = null;

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", objectgroupname));
            switch (input.Count() > 2)
            {
                case true:
                    string description = null;
                    for (var i = 2; i < input.Count(); i++)
                    {
                        description += String.Format("{0} ", input[i]);
                    }
                    switch (description)
                    {
                        case" ":
                        break;
                        default:
                        File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment ", description.TrimEnd(' ')));
                        break;
                    }
                    break;
            }

            string groupmembers2 = null;

            foreach (var groupmember in groupmembers)
            {
                if (groupmember == "")
                {
                }
                else { 
                    groupmembers2 += String.Format("\"{0}\" ",groupmember);
                }
            }

            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set member", groupmembers2.TrimEnd(' ')));
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteServiceObjectGroup(string servicegroupname, string[] groupmembers, string description)
        {
            //NOTE TO SELF. The only real thing that belongs in this is members and comments for the groups.
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "service_groups.txt");
            var spacer = "    ";

            File.AppendAllText(_filename, String.Format("{0}{1} \"{2}\"\r\n", spacer, "edit", servicegroupname));
            File.AppendAllText(_filename, String.Format("{0}{0}{1} \"{2}\"\r\n", spacer, "set comment ", description.TrimEnd(' ')));

            string groupmembers2 = null;

            foreach (var groupmember in groupmembers)
            {
                if (groupmember == "")
                {
                }
                else
                {
                    groupmembers2 += String.Format("\"{0}\" ", groupmember);
                }
            }

            File.AppendAllText(_filename, String.Format("{0}{0}{1} {2}\r\n", spacer, "set member", groupmembers2.TrimEnd(' ')));
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n", spacer, "next"));
        }

        public void WriteStaticRoute(string networkinterface, string route, string nexthop, int metric, int routenumber)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\", "static_routes.txt");
            var spacer = "  ";

            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer,"edit",_staticroutecounter));
            _staticroutecounter++; //increment route counter
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "set device", networkinterface));
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "set dst", route));
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n", spacer, "set gateway", nexthop));
            File.AppendAllText(_filename, String.Format("{0}{1} {2}\r\n",spacer,"set distance",metric));
            File.AppendAllText(_filename, String.Format("{0}{1}\r\n",spacer,"next"));
        }

        //Function is for debugging purposes.
        public void DebugSave(string[] stuff)
        {
            _filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            var indexme = 0;
            var fullstring = "";

            foreach (var e in stuff)
            {
                fullstring += String.Format("{0}:[{1}]", indexme, e);
                indexme++;
            }
            File.AppendAllText(_filename, String.Format("{0}{1}", fullstring, "\r\n"));
        }

        public void WriteUrlFilter(string url, string name, SshClient sshClient)
        {
            
        }
    }
}
