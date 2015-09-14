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
                        _readline_read_oneline = true;
                        _readline_usage = UART_READLINE_USAGE.FREERUN;
                        UART_MSG.Enqueue(message);
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

        private bool Check_Read_OneLine_OK()
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
                while ((Check_Read_OneLine_OK() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get one line
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
                while ((Check_Read_OneLine_OK() == false) && (Get_ReadLine_Timeout_Flag() == false))    // Loop until either Timeout occurs or get one line
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

    }
}
