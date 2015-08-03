using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace Purppletooth_Jimboree_PC
{
     public partial class Form1 : Form
    {
        static SerialPort _serialPort;
        
         public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _serialPort = new SerialPort();
            Serial_InitialSetting();
            Serial_UpdatePortName();
            DisableSendStringButton();
        }

        private void Serial_InitialSetting()
        {
            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM42";
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Encoding = Encoding.UTF8;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
        }

        private void Serial_UpdatePortName()
        {
            listBox1.Items.Clear();
            foreach (string comport_s in SerialPort.GetPortNames())
            {
                listBox1.Items.Add(comport_s);
            }
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                EnableConnectButton();
                UpdateToConnectButton();
            }
            else
            {
                DisableConnectButton();
                UpdateToConnectButton();
            }
        }

        private Boolean Serial_OpenPort(string PortName)
        {
            Boolean ret = false;
            try
            {
                _serialPort.PortName = PortName;
                _serialPort.Open();
                ret = true;
            }
            catch (Exception ex232)
            {
                ret = false;
            }
            return ret;
        }

        private Boolean Serial_ClosePort()
        {
            Boolean ret = false;
            try
            {
                _serialPort.Close();
                ret = true;
            }
            catch (Exception ex232)
            {
                ret = false;
            }
            return ret;
        }

        private void Serial_WriteStringWithNewLine(string out_str)
        {
            if (_serialPort.IsOpen == true)
            {
                try
                {
                    _serialPort.WriteLine(out_str);
                }
                catch (TimeoutException timeout)
                {
                    MessageBox.Show(timeout.ToString());
                }
            }
            else
            {
                AppendSerialMessageLog("COM port is not connected!\n");
            }
        }

        private void Serial_WriteBufferWithIndexLength(byte[] data_buffer, int offset, int count)
        {
            if (_serialPort.IsOpen == true)
            {
                try
                {
                    _serialPort.Write(data_buffer, offset, count);
                }
                catch (TimeoutException timeout)
                {
                    MessageBox.Show(timeout.ToString());
                }
            }
        }

         //
         // Print Serial Port Message on RichTextBox
         //
        delegate void AppendSerialMessageCallback(string text);
        public void AppendSerialMessageLog(string my_str)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                AppendSerialMessageCallback d = new AppendSerialMessageCallback(AppendSerialMessageLog);
                this.Invoke(d, new object[] { my_str });
            }
            else
            {
                this.richTextBox1.AppendText(my_str);
                this.richTextBox1.ScrollToCaret();
            }
        }

        static bool _continue_serial_read_write=false;
        static Thread readThread = null;

        public void ReadSerialPortThread()
        {
            while (_continue_serial_read_write)
            {
                try
                {
                    int DataLength = _serialPort.BytesToRead;
                    if (DataLength > 0)
                    {
                        string message = _serialPort.ReadExisting();
                        this.AppendSerialMessageLog(message);
                    }
                }
                catch (TimeoutException) { }
            }
        }
        private void Start_SerialReadThread()
        {
            _continue_serial_read_write = true;
            readThread = new Thread(ReadSerialPortThread);
            readThread.Start();
        }
        private void Stop_SerialReadThread()
        {
            _continue_serial_read_write = false;
            if (readThread != null)
            {
                if (readThread.IsAlive)
                {
                    readThread.Join();
                }
            }
        }

        private void EnableSendStringButton()
        {
            btnSendStringToUART.Enabled = true;
            txtStringToSerial.BackColor = Color.White;
        }

        private void DisableSendStringButton()
        {
            btnSendStringToUART.Enabled = false;
            txtStringToSerial.BackColor = Color.Gray;
        }

        private void EnableRefreshCOMButton()
        {
            btnFreshCOMNo.Enabled = true;
        }

        private void DisableRefreshCOMButton()
        {
            btnFreshCOMNo.Enabled = false;
        }

        private void EnableConnectButton()
        {
            btnConnectionControl.Enabled = true;
        }

        private void DisableConnectButton()
        {
            btnConnectionControl.Enabled = false;
        }

        private void UpdateToConnectButton()
        {
            btnConnectionControl.Text = "Connect";
            btnIsConnectionButton = true;
        }

        private void UpdateToDisconnectButton()
        {
            btnConnectionControl.Text = "Disconnect";
            btnIsConnectionButton = false;
        }

        private void RefreshCOMPortNumber_Click(object sender, System.EventArgs e)
        {
            Serial_UpdatePortName();
        }

        Boolean btnIsConnectionButton = true;
        private void ConnectionButton_Click(object sender, System.EventArgs e)
        {
            if (btnIsConnectionButton==true)
            {   // User to connect
                if (_serialPort.IsOpen == false)
                {
                    string curItem = listBox1.SelectedItem.ToString();
                    if (Serial_OpenPort(curItem) == true)
                    {
                        UpdateToDisconnectButton();
                        DisableRefreshCOMButton();
                        EnableSendStringButton();
                        Start_SerialReadThread();
                    }
                    else
                    {
                        richTextBox1.AppendText(DateTime.Now.ToString("h:mm:ss tt") + " - Cannot connect to RS232.\n");
                    }
                }
            }
            else
            {   // User to disconnect
                if (_serialPort.IsOpen == true)
                {
                    Stop_SerialReadThread();
                    if (Serial_ClosePort() == true)
                    {
                        UpdateToConnectButton();
                        EnableRefreshCOMButton();
                        DisableSendStringButton();
                    }
                    else
                    {
                        richTextBox1.AppendText(DateTime.Now.ToString("h:mm:ss tt") + " - Cannot disconnect from RS232.\n");
                    }
                }
            }
        }

        private void btnSendStringToUART_Click(object sender, System.EventArgs e)
        {
            string temp_str = txtStringToSerial.Text;

            if(temp_str != "")
            {
                Serial_WriteStringWithNewLine(temp_str);
            }
        }

        private void txtStringToSerial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string temp_str = txtStringToSerial.Text;

                if (temp_str != "")
                {
                    Serial_WriteStringWithNewLine(temp_str);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serialPort.IsOpen == true)
            {
                Stop_SerialReadThread();
            }
            UpdateToConnectButton();
        }
    }

 
}
