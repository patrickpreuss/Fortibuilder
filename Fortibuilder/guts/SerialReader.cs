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
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace Fortibuilder.guts
{
    class SerialReader
    {
        SerialPort sp = new SerialPort();
        static bool _continue;
        TextBox bah;
        Thread readThread;
        string name;
        string message;
        delegate void updateTextDelegate(string newText);
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        public delegate void UpdateTextCallback(string message);
        //Thread readThread = new Thread(Read);

        // Create a new SerialPort object with default settings.
    }
}
