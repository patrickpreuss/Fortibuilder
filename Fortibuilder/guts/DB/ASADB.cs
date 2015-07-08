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
using System.Data.SQLite;
using System.Linq;

namespace Fortibuilder.guts
{
    class ASADB
    {
        private SQLiteConnection _connection;
        private readonly string filename;
        private int objectcount = 1, protocolobjectcount = 1, networkobjeectcount = 1, serviceobjectcount = 1;

        /*
        private enum Object
        {
            Name, Description, Networkaddress, Subnetmask
        }

        private enum ObjectGroup
        {
            Neame, Description, Index
        }

        private enum ServiceObject
        {
            Name, Description, Range, Type
        }
        
        public void CreateTables()
        {

            var conn = new SQLiteConnection("poop.db");

            conn.CreateTable<Object>();
 
            conn.CreateTable<SharpStarPermission>();
            conn.CreateTable<SharpStarGroup>();
            conn.CreateTable<SharpStarGroupPermission>();
            
            conn.Close();
            conn.Dispose();

        }
        */
        public ASADB(string name)
        {
         //   SQLiteConnection
            filename = name;
            using (_connection = new SQLiteConnection(String.Format("{0}{1}", "Data Source=", filename)))
            {
                _connection.Open();
                using (var cmd = _connection.CreateCommand())
                {
               //     CreateTables(cmd);
                    /*
                     * TODO
                     */
                }
                //Tables Created!
            }
        }

        private void RemoveTestDb()
        {
            
        }
        public void WriteToConnection(string s)
        {
            using (_connection = new SQLiteConnection(String.Format("{0}{1};{2};", "Data Source=", filename,"New=True")))
            {

            }
        }

        public void CreateTables(SQLiteCommand cmd)
        {
           
            //Current Tables: objects, protocol-object-group, network-object-group, service-objects
            //cmd.Connection.Open();
            cmd.CommandText = "CREATE TABLE objects (id integer primary key, name TEXT, type TEXT, ip TEXT, smask TEXT, description TEXT);";
            cmd.ExecuteNonQuery();
            //cmd.Transaction.Commit();
            cmd.CommandText = "CREATE TABLE pobjectgroup (pid integer primary key, name TEXT, index TEXT, protocoltype TEXT, description TEXT, ports TEXT);";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE nobjectgroup (nid integer, name TEXT, index TEXT, description TEXT);";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE sobjects (sid integer, name TEXT, description TEXT, netad TEXT, smask TEXT);";
            cmd.ExecuteNonQuery(); 
        
            
        }


        public void WriteObject(string[] ls)
        {
            using (_connection = new SQLiteConnection(String.Format("{0}{1};{2};", "Data Source=", filename,"Version=3")))
            {
               
                _connection.Open();
                var cmd = _connection.CreateCommand();

                if (ls.Count() > 5)
                {
                    for (var i = ls.Count(); i < 5; i++)
                {
                            ls[i] = "filler";

                }
                    }
                cmd.CommandText = String.Format("INSERT INTO objects VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}');", objectcount, ls[0], ls[1], ls[2], ls[3], ls[4]);
                //  cmd.CommandText = "CREATE TABLE objects (id integer primary key, name varchar(100), type varchar(100), ip varchar(100), smask varchar(100), description varchar(100));";
                cmd.ExecuteNonQuery();
                //cmd.Connection.Commit();
                objectcount++;
                //cmd.ExecuteNonQuery();
                cmd.Transaction.Commit();
            }
        }

        public void WriteNetworkObject(string[] ls)
        {
            using (_connection = new SQLiteConnection(String.Format("{0}{1}", "Data Source=", filename)))
            {
                if (ls.Count() > 3)
                {
                    for (var i = ls.Count(); i < 3; i++)
                    {
                        ls[i] = "filler";

                    }
                }

                _connection.Open();
                var cmd = _connection.CreateCommand();
                cmd.Transaction = _connection.BeginTransaction();
                cmd.CommandText = String.Format("INSERT INTO nobjectgroup VALUES({0}, '{1}', '{2}', '{3}');",networkobjeectcount,ls[0],ls[1],ls[2]);
                //cmd.Transaction.Commit();
                cmd.ExecuteNonQuery();
                //SELECT last_insert_rowid();
                networkobjeectcount++;
                cmd.Transaction.Commit();
            }
        }

        public void WriteProtocolObject(string[] ls)
        {
            if (ls.Count() > 5)
            {
                for (var i = ls.Count(); i < 5; i++)
                {
                    ls[i] = "filler";

                }
            }

            using ( _connection = new SQLiteConnection(String.Format("{0}{1}", "Data Source=", filename)))
            {
                _connection.Open();
                var cmd = _connection.CreateCommand();
                cmd.Transaction = _connection.BeginTransaction();
                //cmd.CommandText = "CREATE TABLE if not exists protocolobjectgroup ( [id] INTEGER PRIMARY KEY, [name] TEXT, [index] INTEGER , [protocoltype] TEXT, [ports] TEXT, [description] TEXT)";
                cmd.CommandText = String.Format("INSERT INTO pobjectgroup VALUES({0}, '{1}', '{2}', '{3}', '{4}', '{5}');",protocolobjectcount,ls[0],ls[1],ls[2],ls[3],ls[4]);
                //cmd.Transaction.Commit();
                cmd.ExecuteNonQuery();
                serviceobjectcount++;
                cmd.Transaction.Commit();
            }
        }

        public void WriteServiceObject(string[] ls)
        {
            using ( _connection = new SQLiteConnection(String.Format("{0}{1}", "Data Source=", filename)))
            {
                _connection.Open();
                var cmd = _connection.CreateCommand();
                cmd.Transaction = _connection.BeginTransaction();




                cmd.CommandText =
                    "INSERT INTO sobjects VALUES(@col, @col1, @col2, @col3, @col4);SELECT last_insert_rowid();";

                cmd.Parameters["@col"].Value = serviceobjectcount;
                cmd.Parameters["@col1"].Value = ls[0];
                cmd.Parameters["@col2"].Value = ls[1];
                cmd.Parameters["@col3"].Value = ls[2];

                
                cmd.ExecuteNonQuery();
                serviceobjectcount++;
                cmd.Transaction.Commit();
            }
        }

        public bool ClearTable(String table)
        {
            try
            {
                using (_connection = new SQLiteConnection(String.Format("{0}{1}", "Data Source=", filename)))
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = String.Format("delete from {0};", table);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public object Objects(string[] data)
        {
            return data;
        }

        public object ServiceObject(string[] data)
        {
            return data;
        }

        public object ObjectGroup(string[] data)
        {
            return data;
        }

        public void ReadObjectGroupFromConfig(string[] line)
        {
            
        }

        /*
        boolean isTableExists(SQLiteDatabase db, String tableName)
        {
            if (tableName == null || db == null || !db.isOpen())
            {
                return false;
            }
            Cursor cursor = db.rawQuery("SELECT COUNT(*) FROM sqlite_master WHERE type = ? AND name = ?", new String[] { "table", tableName });
            if (!cursor.moveToFirst())
            {
                return false;
            }
            int count = cursor.getInt(0);
            cursor.close();
            return count > 0;
        }
        */
    }
}
