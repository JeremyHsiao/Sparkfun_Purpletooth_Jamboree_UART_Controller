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
            Serial_UpdateBaudRate();
            DisableSendStringButton();
            InitializeProfileView();
        }

        private void Serial_InitialSetting()
        {
            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM42";
            _serialPort.BaudRate = 9600; // as default;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Encoding = Encoding.UTF8;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
        }

        private void Serial_UpdateBaudRate()
        {
            if ((lstBaudRate.SelectedIndex >= 0) && (lstBaudRate.SelectedIndex <= lstBaudRate.Items.Count))
            {
               //
            }
            else
            {
                lstBaudRate.SelectedIndex = 0;
            }
            _serialPort.BaudRate = Convert.ToInt32(lstBaudRate.SelectedItem.ToString());
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
                // listBox1.SelectedIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = 0;     // this can be modified to preferred default
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

        private void Serial_WriteStringWithPause(string out_str)
        {
            if (_serialPort.IsOpen == true)
            {
                try
                {
                    int index;
                    char[] uart_str=new char[out_str.Length];
                    out_str.CopyTo(0, uart_str, 0, out_str.Length);
                    for (index = 0; index < uart_str.Length; index++)
                    {
                        _serialPort.Write(uart_str, index, 1);
                        if(_serialPort.BaudRate==9600)
                        {
                            Thread.Sleep(5);
                        }
                        else
                        {
                            Thread.Sleep(2);
                        }
                    }
                }
                catch (TimeoutException timeout)
                {
                    MessageBox.Show(timeout.ToString());
                }
            }
            else
            {
                AppendSerialMessageLog("COM is closed and cannot send this string " + out_str + "\n");
            }
        }

        private void Serial_WriteBytesWithPause(byte[] data_buffer)
        {
            if (_serialPort.IsOpen == true)
            {
                try
                {
                    int index;
                    for (index = 0; index < data_buffer.Length; index++)
                    {
                        _serialPort.Write(data_buffer, index, 1);
                        if (_serialPort.BaudRate == 9600)
                        {
                            Thread.Sleep(5);
                        }
                        else
                        {
                            //Thread.Sleep(2);
                        }
                    }
                }
                catch (TimeoutException timeout)
                {
                    MessageBox.Show(timeout.ToString());
                }
            }
            else
            {
                AppendSerialMessageLog("COM is closed and cannot send byte data\n");
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

        enum UART_READLINE_USAGE
        {
            FREERUN = 0,
            ENQUEUE_TILL_ACK,
            WAIT_SINGLE_LINE,
            WAIT_ACK,
        }

        static bool _continue_serial_read_write=false;
        static Thread readThread = null;
        private Queue<string> UART_MSG = new Queue<string>();
        static UART_READLINE_USAGE _readline_usage = UART_READLINE_USAGE.FREERUN;
        //static bool _readline_data_enqueue = false;
        static bool _readline_timeout_flag = false;
        static bool _readline_receive_OK = false;
        static bool _readline_read_oneline = false;

        public void ReadSerialPortThread()
        {
            while (_continue_serial_read_write)
            {
                if (_readline_usage == UART_READLINE_USAGE.ENQUEUE_TILL_ACK)
                {
                    try
                    {
                        string message = _serialPort.ReadLine();
                        //this.AppendSerialMessageLog(message);
                        //                        UART_MSG.Enqueue(message);
                        if (message == "\x0dOK")
                        {
                            _readline_receive_OK = true;
                            _readline_usage = UART_READLINE_USAGE.FREERUN;
                        }
                        else
                        {
                            UART_MSG.Enqueue(message);
                        }
                    }
                    catch (TimeoutException)
                    {
                        Set_ReadLine_Timeout_Flag();
                        Disable_ReadLine_Queue();
                    }
                }
                else if (_readline_usage == UART_READLINE_USAGE.WAIT_SINGLE_LINE)
                {
                    try
                    {
                        string message = _serialPort.ReadLine();
                        UART_MSG.Enqueue(message);
                        _readline_read_oneline = true;
                        _readline_usage = UART_READLINE_USAGE.FREERUN;
                    }
                    catch (TimeoutException)
                    {
                        Set_ReadLine_Timeout_Flag();
                    }
                }
                else if (_readline_usage == UART_READLINE_USAGE.WAIT_ACK)
                {
                    try
                    {
                        string message = _serialPort.ReadLine();
                        //this.AppendSerialMessageLog(message);
                        //                        UART_MSG.Enqueue(message);
                        if (message == "\x0dOK")
                        {
                            _readline_receive_OK = true;
                            _readline_usage = UART_READLINE_USAGE.FREERUN;
                        }
                        else
                        {
                        }
                        this.AppendSerialMessageLog(message);
                    }
                    catch (TimeoutException)
                    {
                        Set_ReadLine_Timeout_Flag();
                    }
                }
                else if (_readline_usage == UART_READLINE_USAGE.FREERUN)
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
                    catch (TimeoutException)
                    {
                    }
                }
            }
        }

        private void Set_UART_ReadLine_Timeout_Time(int timeout_time)
        {
            _serialPort.ReadTimeout = timeout_time;
        }

        private void Clear_ReadLine_Timeout_Flag()
        {
            _readline_timeout_flag = false;
        }

        private void Set_ReadLine_Timeout_Flag()
        {
            _readline_timeout_flag = true;
        }

        private bool Get_ReadLine_Timeout_Flag()
        {
            return (_readline_timeout_flag == true ? true : false);
        }

        private void Enable_ReadLine_Queue(int readline_timeout_time = 1000)
        {
            _readline_usage = UART_READLINE_USAGE.ENQUEUE_TILL_ACK;
            _serialPort.ReadTimeout = readline_timeout_time;
        }

        private void Clear_ReadLine_Queue_Rcv_OK()
        {
            _readline_receive_OK = false;
        }

        private bool Check_ReadLine_Queue_Rcv_OK()
        {
            return (_readline_receive_OK == true ? true : false);
        }

        private void Disable_ReadLine_Queue()
        {
            _readline_usage = UART_READLINE_USAGE.FREERUN;
            _serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
        }

        private void Start_Read_OneLine_Queue(int readline_timeout_time = 3000)
        {
            _readline_read_oneline = false;
            _readline_usage = UART_READLINE_USAGE.WAIT_SINGLE_LINE;
            _serialPort.ReadTimeout = readline_timeout_time;
        }

        private bool Check_Read_OneLine_Done()
        {
            return (_readline_read_oneline == true ? true : false);
        }

        /*
        public void ReadSerialPortThread_char()
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
        */

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
                        Serial_UpdateBaudRate(); 
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

        private void ProcessStringSendUart(string inp_str)
        {
            if (inp_str != "")
            {
                inp_str += '\x0d';
                richTextBox1.AppendText("CMD==> "+inp_str);
                Serial_WriteStringWithPause(inp_str);
            }
            else
            {
                // Empty string -> do nothing
            }
        }

        private void btnSendStringToUART_Click(object sender, System.EventArgs e)
        {
            string temp_str = txtStringToSerial.Text;
            ProcessStringSendUart(temp_str);
        }

        private void txtStringToSerial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string temp_str = txtStringToSerial.Text;
                ProcessStringSendUart(temp_str);
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

        private void lstBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            Serial_UpdateBaudRate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Serial_WriteStringWithPause("set buad=115200\x0d");
            _serialPort.BaudRate = 115200;
            lstBaudRate.SelectedItem = "115200";
            //lstBaudRate.SelectedIndex = 1;
        }

        //
        // Melody Audio V5.8 RC1
        //
        class PTJ_Configuration
        {
            public string audio { get; set; }
            public string autoconn { get; set; }
            public string batt_curr { get; set; }
            public string batt_thresh { get; set; }
            public string baud { get; set; }
            public string ble_auto_adv { get; set; }
            public string ble_role { get; set; }
            public string bps { get; set; }
            public string classic_role { get; set; }
            public string cmd_to { get; set; }
            public string cod { get; set; }
            public string codec { get; set; }
            public string conn_to { get; set; }
            public string deep_sleep { get; set; }
            public string device_id { get; set; }
            public string discoverable { get; set; }
//            public string timeout { get; set; }
            public string enable_a2dp { get; set; }
            public string enable_android_ble { get; set; }
            public string enable_avrcp { get; set; }
            public string enable_batt_ind { get; set; }
            public string enable_hfp { get; set; }
            public string enable_hfp_comm { get; set; }
            public string enable_hfp_cvc { get; set; }
            public string enable_hfp_nrec { get; set; }
            public string enable_hfp_wbs { get; set; }
            public string enable_led { get; set; }
            public string enable_map { get; set; }
            public string enable_pbap { get; set; }
            public string enable_raw_data { get; set; } 
            public string enable_siri_status { get; set; }
            public string enable_spp { get; set; }
            public string enable_spp_sniff { get; set; }
            public string flow_ctrl { get; set; }
            public string force_analog_mic { get; set; }
            public string gpiocontrol { get; set; }
            public string hid_role { get; set; }
            public string i2s { get; set; }
            public string ibeacon_major { get; set; }
            public string ibeacon_minor { get; set; }
            public string ibeacon_power { get; set; }
            public string ibeacon_uuid { get; set; }
            public string input_gain { get; set; }
            public string local_addr { get; set; }
            public string max_rec { get; set; }
            public string mm { get; set; }
            public string music_meta_data { get; set; }
            public string name { get; set; }
            public string name_short { get; set; }
            public string parity { get; set; }
            public string pin { get; set; }
            public string remote_addr { get; set; }
            public string rssi_thresh { get; set; }
            public string spp_transparent { get; set; }
            public string ssp_caps { get; set; }
            public string tws_mode { get; set; }
            public string usb_host { get; set; }
            public string uuid_data { get; set; }
            public string uuid_spp { get; set; }
            public string uuid_srv { get; set; }
            public string vreg_role { get; set; }
            public string wired { get; set; }

            /*
                        public int audio { get; set; }
                        public int autoconn{ get; set; }
                        public int batt_thresh_charglvl{ get; set; }
                        public int batt_thresh_crit{ get; set; }
                        public int batt_thresh_low{ get; set; }
                        public int batt_thresh_lvl0{ get; set; }
                        public int batt_thresh_lvl1{ get; set; }
                        public int batt_thresh_lvl2{ get; set; }
                        public int batt_thresh_lvl3{ get; set; }
                        public int baud{ get; set; }
                        public int ble_role{ get; set; }
                        public int bps{ get; set; }
                        public int classic_role{ get; set; }
                        public int cmd_t0{ get; set; }
                        public int cod{ get; set; }
                        public int codec_codec{ get; set; }
                        public int codec_fs{ get; set; }
                        public int codec_mode{ get; set; }
                        public int seep_sleep{ get; set; }
                        public int device_id_vendor_id_source{ get; set; }
                        public int device_id_vendor_id{ get; set; }
                        public int device_id_product_id{ get; set; }
                        public int device_id_bcd_version{ get; set; }
                        public ulong device_id_software_version{ get; set; }
                        public int discoverable_value{ get; set; }
                        public int timeout{ get; set; }
                        public bool enable_a2dp{ get; set; }
                        public bool enable_android_ble{ get; set; }
                        public bool enable_avrcp{ get; set; }
                        public bool enable_battery_ind{ get; set; }
                        public bool enable_hfp{ get; set; }
                        public bool enable_hfp_cvc{ get; set; }
                        public bool enable_hfp_nrec{ get; set; }
                        public bool enable_hfp_wbs{ get; set; }
                        public bool enable_led{ get; set; }
                        public bool enable_map{ get; set; }
                        public bool enable_pbap{ get; set; }
                        public bool enable_spp{ get; set; }
                        public bool enable_spp_sniff_state{ get; set; }
                        public int enable_spp_sniff_min_interval{ get; set; }
                        public int enable_spp_sniff_max_interval{ get; set; }
                        public int enable_spp_sniff_attempt{ get; set; }
                        public int enable_spp_sniff_timeout{ get; set; }
                        public int enable_spp_sniff_duration{ get; set; }
                        public bool flow_ctrl{ get; set; }
                        public int force_analog_mic{ get; set; }
                        public bool gpio_control{ get; set; }
                        public bool i2s{ get; set; }
                        public int intput_gain{ get; set; }
                        public int local_addr{ get; set; }
                        public int max_rec{ get; set; }
                        public bool music_meta_data{ get; set; }
                        public string name{ get; set; }
                        public string name_short{ get; set; }
                        public int parity{ get; set; }
                        public string pin{ get; set; }
                        public long remote_addr{ get; set; }
            */
        }

        private void ParseConfigQueue()
        {
            PTJ_Configuration board_config = new PTJ_Configuration();
            char[] GetConfigCharSeparators = new char[] { '=', '\r' };
            while (UART_MSG.Count>0)
            {
                string str = UART_MSG.Dequeue();
                string[] words = str.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length >= 2)
                {
                    // Combine other splitting word,if any, into one parameter
                        for (int index=2; index<words.Length; index++)
                    {
                        words[1] += words[index];
                    }
                    AppendSerialMessageLog(words[0] + "/" + words[1] + '\x0d');
                    // Use "string content" as "property name" --> easy for extension and coding
                    System.Reflection.PropertyInfo prop = typeof(PTJ_Configuration).GetProperty(words[0].ToLower());
                    prop.SetValue(board_config, words[1], null);
                    // the following code is only for reference
                    //object value = prop.GetValue(yourInstance, null);
                    //prop.SetValue(yourInstance, "value", null);
                }
            }
            //AppendSerialMessageLog("FIN.\r");
        }

        private static bool _btnGetConfig_click_running=false;
        private void btnGetConfig_click(object sender, EventArgs e)
        {

            if (_btnGetConfig_click_running == false)
            {
                btnGetConfig.Enabled = false;
                _btnGetConfig_click_running = true;
                Enable_ReadLine_Queue();
                Clear_ReadLine_Timeout_Flag();
                Serial_WriteStringWithPause("config\x0d");
                Clear_ReadLine_Queue_Rcv_OK();
                while ((Check_ReadLine_Queue_Rcv_OK() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                {
                    Application.DoEvents();                     // let other process goes on
                }
                Disable_ReadLine_Queue();
                Application.DoEvents();                     // let other process goes on

                ParseConfigQueue();
                _btnGetConfig_click_running = false;
                btnGetConfig.Enabled = true;
            }
        }

        private void InitializeProfileView()
        {
            // Populate the rows.
            string[] row1 = new string[] { "SPP",   "OFF" };
            string[] row2 = new string[] { "A2DP",  "OFF" };
            string[] row3 = new string[] { "AVRCP", "OFF" };
            string[] row4 = new string[] { "MAP",   "OFF" };
            string[] row5 = new string[] { "HFP",   "OFF" };
            string[] row6 = new string[] { "PBAP",  "OFF" };
            object[] rows = new object[] { row1, row2, row3, row4, row5, row6 };

            foreach (string[] rowArray in rows)
            {
                dgvProfileView.Rows.Add(rowArray);
            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public class InquiryResult
        {
            public string bt_addr;
            public string device_class;
            public string bt_rssi;
            public InquiryResult(string addr, string dev_class, string rssi) { bt_addr = addr; device_class = dev_class; bt_rssi = rssi; }
        }

        private void ParseInquiryQueue()
        {
            PTJ_Configuration board_config = new PTJ_Configuration();
            char[] GetConfigCharSeparators = new char[] { ' ', '\r' };
            List<InquiryResult> BT_device = new List<InquiryResult>();

            while (UART_MSG.Count > 0)
            {
                string str = UART_MSG.Dequeue();
                string[] words = str.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 4)
                {
                    if(words[0]== "INQUIRY")
                    {
                        BT_device.Add(new InquiryResult(words[1], words[2], words[3]));
                    }
                }
            }
            IEnumerable<string> All_BT_Address = BT_device.Select(x => x.bt_addr).Distinct();
            foreach(string addr in All_BT_Address)
            {
                Serial_WriteStringWithPause("name " + addr + "\x0d");
                Clear_ReadLine_Timeout_Flag();
                Start_Read_OneLine_Queue(); 
                while ((Check_Read_OneLine_Done() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get one line
                {
                    Application.DoEvents();                     // let other process goes on
                }
                if (UART_MSG.Count > 0)
                {
                    AppendSerialMessageLog(UART_MSG.Dequeue() + '\x0d');
                }
            }
        }

        private static bool _btnInquiry_Click_running = false;
        private void btnInquiry_Click(object sender, EventArgs e)
        {
            if (_btnInquiry_Click_running == false)
            {
                btnInquiry.Enabled = false;
                _btnInquiry_Click_running = true;
                Enable_ReadLine_Queue(10000);
                Clear_ReadLine_Timeout_Flag();
                Serial_WriteStringWithPause("inquiry 30\x0d");
                Clear_ReadLine_Queue_Rcv_OK();
                while ((Check_ReadLine_Queue_Rcv_OK() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                {
                    Application.DoEvents();                     // let other process goes on
                }
                Disable_ReadLine_Queue();
                Application.DoEvents();                     // let other process goes on
                ParseInquiryQueue();
                _btnInquiry_Click_running = false;
                btnInquiry.Enabled = true;
            }
        }

        private bool ParseVersionQueueWithReady()
        {
            char[] GetConfigCharSeparators = new char[] { ' ', '\r' };
            bool _ready_found = false;
            while (UART_MSG.Count > 0)
            {
                string str = UART_MSG.Dequeue();
                string[] words = str.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (words[0] == "Ready")
                {
                    _ready_found = true;
                    UART_MSG.Clear();
                }
            }
            return _ready_found;
        }

        private static bool _btnCheckSystem_Click_running = false;
        private void btnCheckSystem_Click(object sender, EventArgs e)
        {
            if (_btnCheckSystem_Click_running == false)
            {
                btnCheckSystem.Enabled = false;
                _btnCheckSystem_Click_running = true;
                pctSystemCheckResult.Image = global::Properties.Resources.Checking;
                Enable_ReadLine_Queue();
                Clear_ReadLine_Timeout_Flag();
                Serial_WriteStringWithPause("version\x0d");
                while (Get_ReadLine_Timeout_Flag() == false)    // Loop until either Timeout occurs or get an OK
                {
                    Application.DoEvents();                     // let other process goes on
                }
                Disable_ReadLine_Queue();
                if(ParseVersionQueueWithReady()== true)
                {
                    pctSystemCheckResult.Image = global::Properties.Resources.OK;
                }
                else
                {
                    pctSystemCheckResult.Image = global::Properties.Resources.NG;
                }
                _btnCheckSystem_Click_running = false;
                btnCheckSystem.Enabled = true;
            }
        }

        public class ListResult
        {
            public string bt_addr;
            public List<string> bt_profile_list;
            public ListResult(string addr, List<string> profile_list) { bt_addr = addr; bt_profile_list = profile_list; }
        }
        private void ParseListQueue()
        {
            char[] GetConfigCharSeparators = new char[] { ' ', '\r' };
            List<ListResult> BT_device = new List<ListResult>();
            List<string> bt_profile = new List<string>();

            while (UART_MSG.Count > 0)
            {
                string str = UART_MSG.Dequeue();
                string[] words = str.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    if (words[0] == "LIST")
                    {
                        // Combine other splitting word,if any, into one parameter
                        for (int index = 2; index < words.Length; index++)
                        {
                            bt_profile.Add(words[index]);
                        }

                        BT_device.Add(new ListResult(words[1], bt_profile));
                    }
                }
                Application.DoEvents();                     // let other process goes on
            }
            //Set_UART_ReadLine_Timeout_Time(10000);
            IEnumerable<string> All_BT_Address = BT_device.Select(x => x.bt_addr).Distinct();
            foreach (string addr in All_BT_Address)
            {
                Application.DoEvents();                     // let other process goes on
                Thread.Sleep(5000);
                Clear_ReadLine_Timeout_Flag();
                Start_Read_OneLine_Queue(5000);
                Serial_WriteStringWithPause("name " + addr + "\x0d");
                while ((Check_Read_OneLine_Done() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get one line
                {
                    Application.DoEvents();                     // let other process goes on
                }
                if (UART_MSG.Count > 0)
                {
                    Application.DoEvents();                     // let other process goes on
                    AppendSerialMessageLog(UART_MSG.Dequeue() + '\x0d');
                }
            }
        }

        private static bool _btnList_Click_running = false;
        private void btnList_Click(object sender, EventArgs e)
        { 
            if (_btnList_Click_running == false)
            {
                btnList.Enabled = false;
                _btnList_Click_running = true;
                Enable_ReadLine_Queue(2000);
                Clear_ReadLine_Timeout_Flag();
                Serial_WriteStringWithPause("list\x0d");
                Clear_ReadLine_Queue_Rcv_OK();
                while ((Check_ReadLine_Queue_Rcv_OK() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                {
                    Application.DoEvents();                     // let other process goes on
                }
                Disable_ReadLine_Queue();
                Application.DoEvents();                     // let other process goes on
                ParseListQueue();
                _btnList_Click_running = false;
                btnList.Enabled = true;
            }
        }

        private static Boolean _SendStringToHIDKeyboard_running = false;
        private Boolean SendStringToHIDKeyboard(String input_str)
        {
            char[] GetConfigCharSeparators = new char[] { ' ', '\r' };
            Boolean ack_all_OK = false;

            if (_SendStringToHIDKeyboard_running == false)
            {
                _SendStringToHIDKeyboard_running = true;

                foreach(char input_char in input_str)
                {
                    UART_MSG.Clear();
                    Clear_ReadLine_Timeout_Flag();
                    Start_Read_OneLine_Queue(2000);
                    Serial_WriteStringWithPause("send 16 8\x0d");
                    while ((Check_Read_OneLine_Done() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                    {
                        Application.DoEvents();                     // let other process goes on
                    }
                    if ((Check_Read_OneLine_Done() == true))         // one line is read
                    {
                        string str1 = UART_MSG.Dequeue();
                        string[] words1 = str1.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                        if (words1[0] == "OK")
                        {
                            AppendSerialMessageLog("1");
                        }
                        Application.DoEvents();                     // let other process goes on
                    }
                    else
                    {
                        goto exit_SendStrinToHIDKeyboard;
                    }
                    
                    // 2nd
                    UART_MSG.Clear();
                    Clear_ReadLine_Timeout_Flag();
                    Start_Read_OneLine_Queue(3000);
                    //Serial_WriteStringWithPause("0000040000000000\x0d");
                    byte[] key_pressed_0 = { hid_key_LUT[Convert.ToByte(input_char), 1], 0x00, hid_key_LUT[Convert.ToByte(input_char),0], 0x00, 0x00, 0x00, 0x00, 0x00 };
                    Serial_WriteBytesWithPause(key_pressed_0);
                    while ((Check_Read_OneLine_Done() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                    {
                        Application.DoEvents();                     // let other process goes on
                    }
                    if ((Check_Read_OneLine_Done() == true))        // one line is read
                    {
                        string str2 = UART_MSG.Dequeue();
                        string[] words2 = str2.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                        if (words2[0] == "OK")
                        {
                            AppendSerialMessageLog("2");
                        }
                        Application.DoEvents();                     // let other process goes on
                    }
                    else
                    {
                        goto exit_SendStrinToHIDKeyboard;
                    }

                    // 3rd
                    UART_MSG.Clear();
                    Clear_ReadLine_Timeout_Flag();
                    Start_Read_OneLine_Queue(5000);
                    Serial_WriteStringWithPause("send 16 8\x0d");
                    while ((Check_Read_OneLine_Done() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                    {
                        Application.DoEvents();                     // let other process goes on
                    }
                    if ((Check_Read_OneLine_Done() == true))         // one line is read
                    {
                        string str3 = UART_MSG.Dequeue();
                        string[] words3 = str3.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                        if (words3[0] == "OK")
                        {
                            AppendSerialMessageLog("3");
                        }
                        Application.DoEvents();                     // let other process goes on
                    }
                    else
                    {
                        goto exit_SendStrinToHIDKeyboard;
                    }

                    // 4th
                    UART_MSG.Clear();
                    Clear_ReadLine_Timeout_Flag();
                    Start_Read_OneLine_Queue(5000);
                    //Serial_WriteStringWithPause("0000000000000000\x0d");
                    byte[] key_released_all = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    Serial_WriteBytesWithPause(key_released_all);
                    while ((Check_Read_OneLine_Done() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get an OK
                    {
                        Application.DoEvents();                     // let other process goes on
                    }
                    if ((Check_Read_OneLine_Done() == true))        // one line is read
                    {
                        string str4 = UART_MSG.Dequeue();
                        string[] words4 = str4.Split(GetConfigCharSeparators, StringSplitOptions.RemoveEmptyEntries);
                        if (words4[0] == "OK")
                        {
                            AppendSerialMessageLog("4");
                        }
                        UART_MSG.Clear();
                        Application.DoEvents();                     // let other process goes on
                    }
                    else
                    {
                        goto exit_SendStrinToHIDKeyboard;
                    }

                }
                ack_all_OK = true;

                exit_SendStrinToHIDKeyboard:

                UART_MSG.Clear();
                _SendStringToHIDKeyboard_running = false;
            }
            return ack_all_OK;
        }



        private static bool _btnTestBLEKeyBoardHID_Click_running = false;
        private void btnTestBLEKeyBoardHID_Click(object sender, EventArgs e)
        {
            Boolean blHIDKDB_Result;

            if (_btnTestBLEKeyBoardHID_Click_running == false)
            {
                btnTestBLEKeyBoardHID.Enabled = false;
                _btnList_Click_running = true;

                blHIDKDB_Result = SendStringToHIDKeyboard("This is a Book.");

                _btnTestBLEKeyBoardHID_Click_running = false;
                btnTestBLEKeyBoardHID.Enabled = true;
            }
        }

        enum HID_KEYBOARD_CODE
        {
            KEY_A = 4,
            KEY_B = 5,
            KEY_C = 6,
            KEY_D = 7,
            KEY_E = 8,
            KEY_F = 9,
            KEY_G = 10,
            KEY_H = 11,
            KEY_I = 12,
            KEY_J = 13,
            KEY_K = 14,
            KEY_L = 15,
            KEY_M = 16,
            KEY_N = 17,
            KEY_O = 18,
            KEY_P = 19,
            KEY_Q = 20,
            KEY_R = 21,
            KEY_S = 22,
            KEY_T = 23,
            KEY_U = 24,
            KEY_V = 25,
            KEY_W = 26,
            KEY_X = 27,
            KEY_Y = 28,
            KEY_Z = 29,
            KEY_1 = 30,             // !
            KEY_2 = 31,             // @
            KEY_3 = 32,             // #
            KEY_4 = 33,             // $
            KEY_5 = 34,             // %
            KEY_6 = 35,             // ^
            KEY_7 = 36,             // &
            KEY_8 = 37,             // *
            KEY_9 = 38,             // (
            KEY_0 = 39,             // )
            KEY_Enter = 40,
            KEY_Escape = 41,
            KEY_Delete_Back = 42,
            KEY_Tab = 43,
            KEY_Space = 44,
            KEY_Minus = 45,         // -/_
            KEY_Equals = 46,        // =/+
            KEY_LeftBracket = 47,   // [/{
            KEY_RightBracket = 48,  // ]/}
            KEY_Backslash = 49,     // BACK_SLASH/|
            KEY_Semicolon = 51,     // ;\:
            KEY_Quote = 52,         // '\"
            KEY_Grave = 53,         // `\~
            KEY_Comma = 54,         // ,/<
            KEY_Period = 55,        // ./>
            KEY_Slash = 56,         // //?
            KEY_CapsLock = 57,
            KEY_F1 = 58,
            KEY_F2 = 59,
            KEY_F3 = 60,
            KEY_F4 = 61,
            KEY_F5 = 62,
            KEY_F6 = 63,
            KEY_F7 = 64,
            KEY_F8 = 65,
            KEY_F9 = 66,
            KEY_F10 = 67,
            KEY_F11 = 68,
            KEY_F12 = 69,
            KEY_PrintScreen = 70,
            KEY_ScrollLock = 71,
            KEY_Pause = 72,
            KEY_Insert = 73,
            KEY_Home = 74,
            KEY_PageUp = 75,
            KEY_DeleteForward = 76,
            KEY_End = 77,
            KEY_PageDown = 78,
            KEY_Right = 79,
            KEY_Left = 80,
            KEY_Down = 81,
            KEY_Up = 82,
            KP_NumLock = 83,
            KP_Divide = 84,
            KP_Multiply = 85,
            KP_Subtract = 86,
            KP_Add = 87,
            KP_Enter = 88,
            KP_1 = 89,
            KP_2 = 90,
            KP_3 = 91,
            KP_4 = 92,
            KP_5 = 93,
            KP_6 = 94,
            KP_7 = 95,
            KP_8 = 96,
            KP_9 = 97,
            KP_0 = 98,
            KP_Point = 99,
            KEY_NonUSBackslash = 100,
            KP_Equals = 103,
            KEY_F13 = 104,
            KEY_F14 = 105,
            KEY_F15 = 106,
            KEY_F16 = 107,
            KEY_F17 = 108,
            KEY_F18 = 109,
            KEY_F19 = 110,
            KEY_F20 = 111,
            KEY_F21 = 112,
            KEY_F22 = 113,
            KEY_F23 = 114,
            KEY_F24 = 115,
            KEY_Help = 117,
            KEY_Menu = 118,
            KEY_Mute = 127,
            KEY_Vol_Up = 128,
            KEY_Vol_Down = 129,
            KEY_LeftControl = 224,
            KEY_LeftShift = 225,
            KEY_LeftAlt = 226,
            KEY_LeftGUI = 227,
            KEY_RightControl = 228,
            KEY_RightShift = 229,
            KEY_RightAlt = 230,
            KEY_RightGUI = 231
        };

        //#define MK_BIT_L_ALT					2
        //#define MK_BIT_L_GUI					3
        //#define MK_BIT_R_CTRL					4
        //#define MK_BIT_R_SHIFT					5
        //#define MK_BIT_R_ALT					6
        //#define MK_BIT_R_GUI					7

        private const byte L_CTRL_MODIFIER = (1 << 0);        //#define MK_BIT_L_CTRL					0
        private const byte L_SHIFT_MODIFIER = (1 << 1);       //#define MK_BIT_L_SHIFT					1

        private byte[,] hid_key_LUT = new byte[128,2] {
            {0, 0},                                                                       // 0x00
            {(byte)HID_KEYBOARD_CODE.KEY_A,              L_CTRL_MODIFIER},                // 0x01
            {(byte)HID_KEYBOARD_CODE.KEY_B,              L_CTRL_MODIFIER},                // 0x02
            {(byte)HID_KEYBOARD_CODE.KEY_C,              L_CTRL_MODIFIER},                // 0x03     ctrl-c
            {(byte)HID_KEYBOARD_CODE.KEY_D,              L_CTRL_MODIFIER},                // 0x04
            {(byte)HID_KEYBOARD_CODE.KEY_E,              L_CTRL_MODIFIER},                // 0x05
            {(byte)HID_KEYBOARD_CODE.KEY_F,              L_CTRL_MODIFIER},                // 0x06
            {(byte)HID_KEYBOARD_CODE.KEY_G,              L_CTRL_MODIFIER},                // 0x07     ctrl-g
            {(byte)HID_KEYBOARD_CODE.KEY_Delete_Back,    0},                              // 0x08     backspace / ctrl-h
            {(byte)HID_KEYBOARD_CODE.KEY_Tab,            0},                              // 0x09     tab / ctrl-i
            {(byte)HID_KEYBOARD_CODE.KEY_J,              L_CTRL_MODIFIER},                // 0x0A     linefeed / ctrl-j / LF
            {(byte)HID_KEYBOARD_CODE.KEY_K,              L_CTRL_MODIFIER},                // 0x0B
            {(byte)HID_KEYBOARD_CODE.KEY_L,              L_CTRL_MODIFIER},                // 0x0C
            {(byte)HID_KEYBOARD_CODE.KEY_M,              L_CTRL_MODIFIER},                // 0x0D     carriage return / ctrl-m / CR
            {(byte)HID_KEYBOARD_CODE.KEY_N,              L_CTRL_MODIFIER},                // 0x0E
            {(byte)HID_KEYBOARD_CODE.KEY_O,              L_CTRL_MODIFIER},                // 0x0F
            {(byte)HID_KEYBOARD_CODE.KEY_P,              L_CTRL_MODIFIER},                // 0x10
            {(byte)HID_KEYBOARD_CODE.KEY_Q,              L_CTRL_MODIFIER},                // 0x11
            {(byte)HID_KEYBOARD_CODE.KEY_R,              L_CTRL_MODIFIER},                // 0x12
            {(byte)HID_KEYBOARD_CODE.KEY_S,              L_CTRL_MODIFIER},                // 0x13
            {(byte)HID_KEYBOARD_CODE.KEY_T,              L_CTRL_MODIFIER},                // 0x14
            {(byte)HID_KEYBOARD_CODE.KEY_U,              L_CTRL_MODIFIER},                // 0x15
            {(byte)HID_KEYBOARD_CODE.KEY_V,              L_CTRL_MODIFIER},                // 0x16
            {(byte)HID_KEYBOARD_CODE.KEY_W,              L_CTRL_MODIFIER},                // 0x17
            {(byte)HID_KEYBOARD_CODE.KEY_X,              L_CTRL_MODIFIER},                // 0x18
            {(byte)HID_KEYBOARD_CODE.KEY_Y,              L_CTRL_MODIFIER},                // 0x19
            {(byte)HID_KEYBOARD_CODE.KEY_Z,              L_SHIFT_MODIFIER},               // 0x1A     ctrl-z
            {(byte)HID_KEYBOARD_CODE.KEY_Escape,         0},                              // 0x1B     esc
            {0, 0},                                                                       // 0x1C
            {0, 0},                                                                       // 0x1D
            {0, 0},                                                                       // 0x1E
            {0, 0},                                                                       // 0x1F
            {(byte)HID_KEYBOARD_CODE.KEY_Space,          0},                              // 0x20
            {(byte)HID_KEYBOARD_CODE.KEY_1,              L_SHIFT_MODIFIER},               // 0x21	  !
            {(byte)HID_KEYBOARD_CODE.KEY_Quote,          L_SHIFT_MODIFIER},               // 0x22     "
            {(byte)HID_KEYBOARD_CODE.KEY_3,              L_SHIFT_MODIFIER},               // 0x23     #
            {(byte)HID_KEYBOARD_CODE.KEY_4,              L_SHIFT_MODIFIER},               // 0x24     $
            {(byte)HID_KEYBOARD_CODE.KEY_5,              L_SHIFT_MODIFIER},               // 0x25     %
            {(byte)HID_KEYBOARD_CODE.KEY_7,              L_SHIFT_MODIFIER},               // 0x26     &
            {(byte)HID_KEYBOARD_CODE.KEY_Quote,          0},                              // 0x27     '
            {(byte)HID_KEYBOARD_CODE.KEY_9,              L_SHIFT_MODIFIER},               // 0x28     (
            {(byte)HID_KEYBOARD_CODE.KEY_0,              L_SHIFT_MODIFIER},               // 0x29     )
            {(byte)HID_KEYBOARD_CODE.KEY_8,              L_SHIFT_MODIFIER},               // 0x2A     *
            {(byte)HID_KEYBOARD_CODE.KEY_Equals,         L_SHIFT_MODIFIER},               // 0x2B     +
            {(byte)HID_KEYBOARD_CODE.KEY_Comma,          0},                              // 0x2C     ,
            {(byte)HID_KEYBOARD_CODE.KEY_Minus,          0},                              // 0x2D     -
            {(byte)HID_KEYBOARD_CODE.KEY_Period,         0},                              // 0x2E     .
            {(byte)HID_KEYBOARD_CODE.KEY_Slash,          0},                              // 0x2F     /
            {(byte)HID_KEYBOARD_CODE.KEY_0,              0},                              // 0x30     0
            {(byte)HID_KEYBOARD_CODE.KEY_1,              0},                              // 0x31     1
            {(byte)HID_KEYBOARD_CODE.KEY_2,              0},                              // 0x32     2
            {(byte)HID_KEYBOARD_CODE.KEY_3,              0},                              // 0x33     3
            {(byte)HID_KEYBOARD_CODE.KEY_4,              0},                              // 0x34     4
            {(byte)HID_KEYBOARD_CODE.KEY_5,              0},                              // 0x35     5
            {(byte)HID_KEYBOARD_CODE.KEY_6,              0},                              // 0x36     6
            {(byte)HID_KEYBOARD_CODE.KEY_7,              0},                              // 0x37     7
            {(byte)HID_KEYBOARD_CODE.KEY_8,              0},                              // 0x38     8
            {(byte)HID_KEYBOARD_CODE.KEY_9,              0},                              // 0x39     9
            {(byte)HID_KEYBOARD_CODE.KEY_Semicolon,      L_SHIFT_MODIFIER},               // 0x3A     :
            {(byte)HID_KEYBOARD_CODE.KEY_Semicolon,      0},                              // 0x3B     ;
            {(byte)HID_KEYBOARD_CODE.KEY_Comma,          L_SHIFT_MODIFIER},               // 0x3C     <
            {(byte)HID_KEYBOARD_CODE.KEY_Equals,         0},                              // 0x3D     =
            {(byte)HID_KEYBOARD_CODE.KEY_Period,         L_SHIFT_MODIFIER},               // 0x3E     >
            {(byte)HID_KEYBOARD_CODE.KEY_Slash,          L_SHIFT_MODIFIER},               // 0x3F     ?
            {(byte)HID_KEYBOARD_CODE.KEY_2,              L_SHIFT_MODIFIER},               // 0x40     @
            {(byte)HID_KEYBOARD_CODE.KEY_A,              L_SHIFT_MODIFIER},               // 0x41     A
            {(byte)HID_KEYBOARD_CODE.KEY_B,              L_SHIFT_MODIFIER},               // 0x42     B
            {(byte)HID_KEYBOARD_CODE.KEY_C,              L_SHIFT_MODIFIER},               // 0x43     C
            {(byte)HID_KEYBOARD_CODE.KEY_D,              L_SHIFT_MODIFIER},               // 0x44     D
            {(byte)HID_KEYBOARD_CODE.KEY_E,              L_SHIFT_MODIFIER},               // 0x45     E
            {(byte)HID_KEYBOARD_CODE.KEY_F,              L_SHIFT_MODIFIER},               // 0x46     F
            {(byte)HID_KEYBOARD_CODE.KEY_G,              L_SHIFT_MODIFIER},               // 0x47     G
            {(byte)HID_KEYBOARD_CODE.KEY_H,              L_SHIFT_MODIFIER},               // 0x48     H
            {(byte)HID_KEYBOARD_CODE.KEY_I,              L_SHIFT_MODIFIER},               // 0x49     I
            {(byte)HID_KEYBOARD_CODE.KEY_J,              L_SHIFT_MODIFIER},               // 0x4A     J
            {(byte)HID_KEYBOARD_CODE.KEY_K,              L_SHIFT_MODIFIER},               // 0x4B     K
            {(byte)HID_KEYBOARD_CODE.KEY_L,              L_SHIFT_MODIFIER},               // 0x4C     L
            {(byte)HID_KEYBOARD_CODE.KEY_M,              L_SHIFT_MODIFIER},               // 0x4D     M
            {(byte)HID_KEYBOARD_CODE.KEY_N,              L_SHIFT_MODIFIER},               // 0x4E     N 
            {(byte)HID_KEYBOARD_CODE.KEY_O,              L_SHIFT_MODIFIER},               // 0x4F     O
            {(byte)HID_KEYBOARD_CODE.KEY_P,              L_SHIFT_MODIFIER},               // 0x50     P
            {(byte)HID_KEYBOARD_CODE.KEY_Q,              L_SHIFT_MODIFIER},               // 0x51     Q
            {(byte)HID_KEYBOARD_CODE.KEY_R,              L_SHIFT_MODIFIER},               // 0x52     R
            {(byte)HID_KEYBOARD_CODE.KEY_S,              L_SHIFT_MODIFIER},               // 0x53     S
            {(byte)HID_KEYBOARD_CODE.KEY_T,              L_SHIFT_MODIFIER},               // 0x54     T
            {(byte)HID_KEYBOARD_CODE.KEY_U,              L_SHIFT_MODIFIER},               // 0x55     U
            {(byte)HID_KEYBOARD_CODE.KEY_V,              L_SHIFT_MODIFIER},               // 0x56     V
            {(byte)HID_KEYBOARD_CODE.KEY_W,              L_SHIFT_MODIFIER},               // 0x57     W
            {(byte)HID_KEYBOARD_CODE.KEY_X,              L_SHIFT_MODIFIER},               // 0x58     X
            {(byte)HID_KEYBOARD_CODE.KEY_Y,              L_SHIFT_MODIFIER},               // 0x59     Y
            {(byte)HID_KEYBOARD_CODE.KEY_Z,              L_SHIFT_MODIFIER},               // 0x5A     Z
            {(byte)HID_KEYBOARD_CODE.KEY_LeftBracket,    0},                              // 0x5B     [
            {(byte)HID_KEYBOARD_CODE.KEY_Backslash,      0},                              // 0x5C     BACK-SLASH
            {(byte)HID_KEYBOARD_CODE.KEY_RightBracket,   0},                              // 0x5D     ]
            {(byte)HID_KEYBOARD_CODE.KEY_6,              L_SHIFT_MODIFIER},               // 0x5E     ^
            {(byte)HID_KEYBOARD_CODE.KEY_Minus,          L_SHIFT_MODIFIER},               // 0x5F     _
            {(byte)HID_KEYBOARD_CODE.KEY_Grave,          0},                              // 0x60     `
            {(byte)HID_KEYBOARD_CODE.KEY_A,              0},                              // 0x61     a
            {(byte)HID_KEYBOARD_CODE.KEY_B,              0},                              // 0x62     b
            {(byte)HID_KEYBOARD_CODE.KEY_C,              0},                              // 0x63     c
            {(byte)HID_KEYBOARD_CODE.KEY_D,              0},                              // 0x64     d
            {(byte)HID_KEYBOARD_CODE.KEY_E,              0},                              // 0x65     e
            {(byte)HID_KEYBOARD_CODE.KEY_F,              0},                              // 0x66     f
            {(byte)HID_KEYBOARD_CODE.KEY_G,              0},                              // 0x67     g
            {(byte)HID_KEYBOARD_CODE.KEY_H,              0},                              // 0x68     h
            {(byte)HID_KEYBOARD_CODE.KEY_I,              0},                              // 0x69     i
            {(byte)HID_KEYBOARD_CODE.KEY_J,              0},                              // 0x6A     j
            {(byte)HID_KEYBOARD_CODE.KEY_K,              0},                              // 0x6B     k
            {(byte)HID_KEYBOARD_CODE.KEY_L,              0},                              // 0x6C     l
            {(byte)HID_KEYBOARD_CODE.KEY_M,              0},                              // 0x6D     m
            {(byte)HID_KEYBOARD_CODE.KEY_N,              0},                              // 0x6E     n 
            {(byte)HID_KEYBOARD_CODE.KEY_O,              0},                              // 0x6F     o
            {(byte)HID_KEYBOARD_CODE.KEY_P,              0},                              // 0x70     p
            {(byte)HID_KEYBOARD_CODE.KEY_Q,              0},                              // 0x71     q
            {(byte)HID_KEYBOARD_CODE.KEY_R,              0},                              // 0x72     r
            {(byte)HID_KEYBOARD_CODE.KEY_S,              0},                              // 0x73     s
            {(byte)HID_KEYBOARD_CODE.KEY_T,              0},                              // 0x74     t
            {(byte)HID_KEYBOARD_CODE.KEY_U,              0},                              // 0x75     u
            {(byte)HID_KEYBOARD_CODE.KEY_V,              0},                              // 0x76     v
            {(byte)HID_KEYBOARD_CODE.KEY_W,              0},                              // 0x77     w
            {(byte)HID_KEYBOARD_CODE.KEY_X,              0},                              // 0x78     x
            {(byte)HID_KEYBOARD_CODE.KEY_Y,              0},                              // 0x79     y
            {(byte)HID_KEYBOARD_CODE.KEY_Z,              0},                              // 0x7A     z
            {(byte)HID_KEYBOARD_CODE.KEY_LeftBracket,    L_SHIFT_MODIFIER},               // 0x7B     {
            {(byte)HID_KEYBOARD_CODE.KEY_Backslash,      L_SHIFT_MODIFIER},               // 0x7C     |
            {(byte)HID_KEYBOARD_CODE.KEY_RightBracket,   L_SHIFT_MODIFIER},               // 0x7D     }
            {(byte)HID_KEYBOARD_CODE.KEY_Grave,          L_SHIFT_MODIFIER},               // 0x7E     ~
            {(byte)HID_KEYBOARD_CODE.KEY_DeleteForward,  0},                              // 0x7F     DELETE
        };

    }
}
