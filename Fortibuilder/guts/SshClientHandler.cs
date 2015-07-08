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
using System.Drawing;
using System.Windows.Forms;
using Renci.SshNet;


namespace Fortibuilder.guts
{
    class SshClientHandler
    {
        private TabPage _tabPage;
        private ToolStripStatusLabel _toolStripStatusLabel;
        private SshClient _sshClient;
        private TextBox _outputTextBox;
        private String _IP, _username, _password;

        public SshClientHandler()
        {

        }

        public void Connect(SshClient sshClient, ToolStripStatusLabel toolStripStatusLabel, TabPage tabPage, String IP, String username, String password) 
        {
            _sshClient = sshClient;
            _tabPage = tabPage;
            _toolStripStatusLabel = toolStripStatusLabel;
            _IP = IP;
            _username = username;
            _password = password;

            try
            {
                Updatestatus("connecting to server...");
                //sshclient =  Remote.connect("10.4.12.240", bah, sshclient);
                _sshClient = new SshClient(_IP, _username, _password);
                _sshClient.Connect();
                Updatestatus("SSH successful to server");
                Makevis();
                _outputTextBox.Text += String.Format("{0}{1}","Connected...","\r\n");
                Textrefresh();
            }
            catch (Exception ex) { Updatestatus(ex.ToString()); _outputTextBox.Text += (String.Format("{0}{1}",ex.ToString() , "\r\n")); }
        }

        private void Textrefresh()
        {
            _outputTextBox.SelectionStart = _outputTextBox.Text.Length;
            _outputTextBox.ScrollToCaret();
            _outputTextBox.Refresh();
        }

        public void Updatestatus(string s)
        {
            _toolStripStatusLabel.Visible = true;
            _toolStripStatusLabel.Text = s;
        }

        public void tx_txt(string s)
        {
            var terminal = _sshClient.RunCommand(s);
            _outputTextBox.Text += terminal.Result.Replace("\n", "\r\n");
            _sshClient.SendKeepAlive();
            Textrefresh();
            terminal.Dispose();
        }

        public void Makevis()
        {
            _outputTextBox = new TextBox();
            _outputTextBox.Multiline = true;
            _outputTextBox.Location = new Point(168, 6);
            _outputTextBox.Size = new Size(550, 280);
            _outputTextBox.Visible = true;
            _outputTextBox.Enabled = true;
            _outputTextBox.ScrollBars = ScrollBars.Both;
            _outputTextBox.BackColor = Color.Black;
            _outputTextBox.ForeColor = Color.White;
            // bah.Font.SizeInPoints = 12;
            _outputTextBox.Font = new Font("Lucida", 12, _outputTextBox.Font.Style, _outputTextBox.Font.Unit);
            _tabPage.Controls.Add(_outputTextBox);
            var diff1 = _tabPage.Size.Height - _outputTextBox.Size.Height;

            var diff2 = _tabPage.Size.Width - _outputTextBox.Size.Width;
            var res1 = _tabPage.Size.Height - diff1;//189,229
            var res2 = _tabPage.Size.Width - diff2;
            _outputTextBox.SetBounds(215, 25, res2, res1);
            //object bah2 = new object();
            _outputTextBox.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top); //.Equals(bah2.ToString());
            //updatetext(diff1.ToString() + "," + diff2.ToString()+res1.ToString()+res2.ToString()+ bah.Anchor.ToString()+"\r\n");
            
            
        }


        public void disconnect(SshClient sshClient)
        {
            sshClient.Disconnect();
        }
    }
}
