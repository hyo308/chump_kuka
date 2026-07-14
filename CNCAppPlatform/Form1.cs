using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
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

        private async void Form1_Shown(object sender, EventArgs e)
        {
            await Task.Delay(200); // 確保所有 UI 都已經載入完成
            await InitializeAllPagesAsync(this);
        }

        private async Task InitializeAllPagesAsync(Control parent)
        {
            // 顯示不阻塞的閃爍訊息框，讓使用者知道正在切換頁面
            _ = MsgBox.ShowFlash("正在自動初始化，請稍候...", "啟動中", 3000);

            string[] targetPages = new string[] { "手動派車", "交換站任務", "地圖監控", "API 設定", "作業訊息" };
            List<Control> buttonsToClick = new List<Control>();
            Control settingBtn = null;

            FindTargetButtons(parent, targetPages, buttonsToClick, ref settingBtn);

            foreach (var btn in buttonsToClick)
            {
                InvokeControlClick(btn);
                await Task.Delay(200); // 確保每個頁面有足夠時間觸發 Load/VisibleChanged
            }

            // 最後切換回設定頁
            if (settingBtn != null)
            {
                InvokeControlClick(settingBtn);
            }

            // 觸發 Form1 的 open_log_button 進行初始化
            InitializeOpenLogButton(parent);
        }

        private void InitializeOpenLogButton(Control parent)
        {
            if (parent == null) return;
            
            Control logBtn = FindControlByName(parent, "open_log_button");
            if (logBtn is CheckBox cb)
            {
                bool originalState = cb.Checked;
                cb.Checked = !originalState;
                Application.DoEvents(); 
                cb.Checked = originalState;
            }
        }
        
        private Control FindControlByName(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c;
                if (c.HasChildren)
                {
                    Control found = FindControlByName(c, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void FindTargetButtons(Control parent, string[] targetTexts, List<Control> foundButtons, ref Control settingButton)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button || c is RadioButton || c.GetType().Name.ToLower().Contains("button"))
                {
                    foreach (var text in targetTexts)
                    {
                        if (c.Text != null && c.Text.Contains(text))
                        {
                            if (!foundButtons.Contains(c))
                                foundButtons.Add(c);
                            break;
                        }
                    }
                    if (c.Text != null && (c.Text == "設定" || (c.Text.Contains("設定") && !c.Text.Contains("API"))))
                    {
                        settingButton = c;
                    }
                }
                
                if (c.HasChildren)
                {
                    FindTargetButtons(c, targetTexts, foundButtons, ref settingButton);
                }
            }
        }
        
        private void InvokeControlClick(Control ctrl)
        {
            if (ctrl is Button btn)
            {
                btn.PerformClick();
                return;
            }
            if (ctrl is RadioButton rbtn)
            {
                rbtn.PerformClick();
                return;
            }

            var method = ctrl.GetType().GetMethod("PerformClick");
            if (method != null)
            {
                method.Invoke(ctrl, null);
                return;
            }
            
            var onClick = ctrl.GetType().GetMethod("OnClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (onClick != null)
            {
                onClick.Invoke(ctrl, new object[] { EventArgs.Empty });
            }
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

            Form1_Shown(sender, e);
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
