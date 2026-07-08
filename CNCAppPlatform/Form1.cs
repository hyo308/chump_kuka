using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using iCAPS;
using System.Windows.Documents;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Chump_kuka.Controls;
using Chump_kuka.Dispatchers;
using Newtonsoft.Json;
using Chump_kuka.Services;
using System.Drawing.Drawing2D;

namespace Chump_kuka
{
    public partial class Form1 : iCAPS.Form1
    {
        private UdpChatRoom _udp_chat_room = new UdpChatRoom();
        private LogWindow _log_window;

        public Form1()
        {
            InitializeComponent();
            Env.EnableBubble = true;
            Load += Form1_Load;

            //string binPath = Path.Combine(Application.StartupPath, "config\\layout.ini");
            //MessageBox.Show("Bin 資料夾路徑：" + binPath);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Debug模式下，手動開啟 api 連線
            if (!Debugger.IsAttached) 
            { 
                enable_api_btn.Visible = false;
                btnSensorSim.Visible = false;
            }
            //_udp_chat_room.Show();
            //_udp_chat_room.Hide();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(_log_window == null)
            {
                _log_window = new LogWindow() { Anchor = AnchorStyles.Right | AnchorStyles.Top};
                panel1.Controls.Add(_log_window);
                _log_window.Location = new Point(panel1.Width - _log_window.Width - 20, 20);
                _log_window.BringToFront();     // 將視窗至於最上層
                _log_window.Show();
            }
            _log_window.Visible = open_log_button.Checked;
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            //Env.enble_kuka_api = true;
            //KukaApiController.Enable = true;
            //Log.Append("Info", "INFO", "Form1");
            //Log.Append("Test", "TEST", "Form1");
            //MsgBox.Show("Test");

            CarryTaskController.FeedbackFinish(CarryTaskController.CurrentTask.MissionCode);
        }


        private void btnUdpLog_Click(object sender, EventArgs e)
        {
            //if (_udp_chat_room.Visible)
            //{
            //    _udp_chat_room.Hide();
            //}
            //_udp_chat_room.Show();

            new SensorSim().Show();
        }

    }
}
