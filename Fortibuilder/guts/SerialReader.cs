using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Timers;
using System.Threading;
using Microsoft.Win32;
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
