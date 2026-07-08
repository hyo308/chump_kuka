using Chump_kuka.Controller;
using Chump_kuka.Controls;
using iCAPS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chump_kuka.Forms
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
            Load += Setting_Load;

            Env.EnvChanged += (s, e) => bind_comboBox.Text = Env.BindAreaName;
            KukaParm.AreaChanged += AreaChanged; ;
            // VisibleChanged += (s, e) => comboBox1.Text = KukaParm.BindArea?.AreaName;

            bind_comboBox.Items.Add("Area404");
            checkBox_autoStart.CheckedChanged += checkBox_autoStart_CheckedChanged;
        }

        private void AreaChanged(object sender, PropertyChangedEventArgs e)
        {
            bind_comboBox.Invoke(new Action(() =>
            {
                // 當區域列表出現變化時，同步更新綁定區域的下拉式選單，以便及時更改綁定區域
                bind_comboBox.Items.Clear();
                foreach (KukaModel.Area area in KukaParm.GetAreaArray())
                {
                    bind_comboBox.Items.Add(area);

                    // 若當前選單文字符合加入資料，強制觸發選取事件
                    if (area.AreaName == bind_comboBox.Text)
                    {
                        bind_comboBox.SelectedIndex = bind_comboBox.Items.Count - 1;
                    }
                }
            }));
        }

        private void Setting_Load(object sender, EventArgs e)
        {
            local_ip_combo.Text = Env.LocalIp ?? "";
            switch_client.Checked = !Env.ICapsServer;
            linker_server_ip.Text = Env.IcapsLinkerServerIp ?? "";
            linker_server_port.Text = Env.IcapsLinkerServerPort ?? "5700";
            kuka_request_url.Text = Env.KukaApiUrl;
            modbus_ip.Text = Env.SensorModbusTcp?.Address.ToString();
            modbus_port.Text = Env.SensorModbusTcp?.Port.ToString() ?? "502";
            tcp_record_port.Text = Env.RecordLogTcpPort ?? "6400";
            kuka_response_url.Text = Env.KukaResponseUrl ?? "";

            bind_comboBox.Text = Env.BindAreaName ?? "";
            checkBox_autoStart.Checked = Env.isAutoStart;
            checkBox_autoStart.Visible = Env.isAutoStartBtnVisible;
        }

        private async Task RunTask(int start_val, int end_val, string running_msg, Func<Task> task)
        {
            // 設定任務開始前，進度條描述 & 數值
            Log.SystemInfo("[INIT] " + running_msg);
            progress_msg.Text = running_msg;
            progressBar1.Value = start_val;

            // 等待非同步任務完成
            await task();

            // 任務開始前，進度條數值
            progressBar1.Value = end_val;
        }

        private async Task KukaApiTask()
        {
            if (!Env.ICapsServer)
                return;
            // TODO 關閉現有連線
            bool isconn = await KukaApiController.ConnectAndCheck(kuka_request_url.Text);
            kuka_api_check.Change = isconn;
            kuka_api_check.Visible = true;

            // 若成功連線則加入區域查詢，作為稍後綁定區域的依據
            if (isconn)
            {
                //KukaApiController.GetAreaInfo();
                Env.KukaApiUrl = kuka_request_url.Text;
                Log.SystemInfo("成功");
                KukaApiController.GetRobotStatus();
            }
            else
            {
                Log.SystemInfo("連線異常");
            }


        }
        private async Task ServerTask()
        {
            //bool is_icaps_server = bool.TryParse(radio_button_group.Controls        // 判定是否指定為 iCaps 伺服器
            //                                     .OfType<RadioButton>()
            //                                     .FirstOrDefault(rb => rb.Checked)
            //                                     .Tag.ToString(),
            //                                 out is_icaps_server);

            // SocketDispatcher _icaps_socket = new SocketDispatcher();
            //isconn = await SocketDispatcher.StartRecordListener(int.Parse(tcp_server_port.Text));


            IPEndPoint listen_server_ipep = new IPEndPoint(IPAddress.Parse(linker_server_ip.Text), int.Parse(linker_server_port.Text));       // 開啟 Linker 通訊

            bool isconn = await ChatController.Init(Env.ICapsServer, listen_server_ipep);
            await Task.Delay(500);      // 等待初始化

            Env.IcapsLinkerServerIp = linker_server_ip.Text;
            Env.IcapsLinkerServerPort = linker_server_port.Text;

            server_check.Change = isconn;
            server_check.Visible = true;
            if (isconn)
            {
                Log.SystemInfo("初始化成功");
                // TODO
                // 等待訊息回應，目前透過等待 1.5 秒完成此效果
                await Task.Delay(1500);
            }
            else
            {
                Log.SystemInfo("初始化失敗");
            }
        }

        private async Task RecordLogTask()
        {
            if (!Env.ICapsServer)
                return;

            // SocketDispatcher _icaps_socket = new SocketDispatcher();
            bool isconn = await FeedbackDispatcher.StartRecordListener(int.Parse(tcp_record_port.Text));
            if (isconn)
            {
                Env.RecordLogTcpPort = tcp_record_port.Text;
            }

            record_log_check.Change = isconn;
            record_log_check.Visible = true;
        }

        private async Task SensorModbusTask()
        {
            // Console.WriteLine(KukaParm.KukaAreaModels);
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(modbus_ip.Text), int.Parse(modbus_port.Text));
            bool isconn = await LocalAreaController.ConnectIO_Module(ip);

            sensor_check.Change = isconn;
            sensor_check.Visible = true;
            if (isconn)
            {
                Env.SensorModbusTcp = ip;
                Log.SystemInfo("初始化成功");
            }
            else
            {
                Log.SystemInfo("初始化失敗");
            }
        }

        private async Task KukaResponseTask()
        {
            if (!Env.ICapsServer)
                return;

            bool isconn = await KukaApiController.StartListen(kuka_response_url.Text);
            kuka_response_check.Change = isconn;
            kuka_response_check.Visible = true;

            if (isconn)
            {
                Env.KukaResponseUrl = kuka_response_url.Text;
            }
        }

        private async void connTest_Click(object sender, EventArgs e)
        {
            Env.LocalIp = local_ip_combo.Text;
            kuka_api_check.Visible = kuka_response_check.Visible = record_log_check.Visible = sensor_check.Visible = server_check.Visible = false;
            // 依序執行連線任務
            await RunTask(15, 20, "等待 iCAPS 伺服器開啟...", ServerTask);
            await RunTask(35, 40, "等待 KUKA API 連線...", KukaApiTask);
            await RunTask(55, 60, "等待 Modbus Tcp 連線...", SensorModbusTask);
            await RunTask(75, 80, "等待 KUKA 回應監聽開啟...", KukaResponseTask);
            await RunTask(95, 100, "等待工時監測伺服器開啟...", RecordLogTask);
            //bind_comboBox.SelectedIndex = 0;        // 強制套用當前選項
            progress_msg.Text = "已完成";

            if (switch_sever.Checked)
            {
                bool success = SetStrategy();
                if (success)
                    SyncHistoryData();
            }
            if (bind_comboBox.Items.Contains("Area404"))
            {
                ChatController.SayHi();
                await MsgBox.ShowFlash("未獲取區域資料，嘗試再次連線...", "資料錯誤", 500);
                if (bind_comboBox.Items.Contains("Area404"))
                {
                    MsgBox.Show("未獲取區域資料，請重新連線測試");
                }
            }
        }

        /// <summary>
        /// 設定搬運順序策略
        /// </summary>
        private bool SetStrategy()
        {
            List<string> unmatch_data = new List<string>();
            List<KukaModel.Area> match_models = new List<KukaModel.Area>();

            KukaParm.ResetAreaStrategy();       // 重設策略 (全部區域 index 設為 -1)

            string strategy_string = Env.Strategy;      // 取得策略(搬運區域順序)
            if (strategy_string == "")      // 若找不到歷史搬運順序策略
            {
                // 開啟設定頁面
                station_setting_Click(null, null);
            }

            List<string> area_queue = strategy_string.Split(';').ToList();

            // 嘗試從 area_queue 中，找尋區域模型，並分配到存放區
            foreach (string area_name in area_queue)
            {
                // var match_model = KukaParm.KukaOriginAreaModels.FirstOrDefault(p => p.AreaName == area_name);
                KukaModel.Area match_model = KukaParm.GetRawAreaModel(KukaParm.AreaName2Code(area_name));
                if (match_model != null && match_model.NodeList.Length > 0)     // NodeList 數量需大於 0 才視為模型成立
                {
                    match_models.Add(match_model);
                }
                else
                {
                    unmatch_data.Add(area_name);
                }
            }

            if (unmatch_data.Count > 0)
            {
                Log.Append("策略調整發生異常", "SYSTEM", "Setting");
                MsgBox.Show("找不到以下區域資訊:\n" + String.Join(";", unmatch_data));
                return false;
            }
            else
            {

                KukaParm.InitAreaStrategy(match_models);       // 將多餘部分移除系統區域
                Log.Append("完成策略調整", "SYSTEM", "Setting");
                return true;
            }
        }

        private void SyncHistoryData()
        {
            // ------------
            // 匯入歷史資料
            // ------------
            string history_json = KukaParm.GetParamHistory;

            if (history_json != "")
            {
                List<KukaModel.Area> history_areas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<KukaModel.Area>>(history_json);
                foreach (KukaModel.Area history_area in history_areas)
                {
                    // KukaModel.Area find_area = KukaParm.KukaAreaModels.FirstOrDefault(area => area.AreaCode == history_area.AreaCode); 
                    KukaModel.Area find_area = KukaParm.GetRawAreaModel(history_area.AreaCode);
                    if (find_area != null)
                    {
                        find_area.UpdateNodes(history_area.NodeList);
                    }
                }
            }

            Log.Append("完成歷史資料匯入", "SYSTEM", "Setting");
        }

        private void bind_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if (KukaParm.KukaAreaModels.Count == 0) return;     // 尚未取得 api 資料，暫不處理
            if ((sender as ComboBox).SelectedItem is KukaModel.Area select_model)
            {
                KukaParm.BindAreaModel = select_model;      // 將指定模型淺複製為 BindAreaModel (數值更改會影響原列表)

            }

            LocalAreaController.BuildBindArea();
        }

        private void switch_client_CheckedChanged(object sender, EventArgs e)
        {
            Env.ICapsServer = switch_sever.Checked;
            kuka_request_url.Enabled = tcp_record_port.Enabled = kuka_response_url.Enabled = station_setting.Enabled = Env.ICapsServer;

        }

        private void station_setting_Click(object sender, EventArgs e)
        {
            // List<string> items = new List<string> { "項目1", "項目2", "項目3", "項目4" };
            // List<string> list = KukaParm.KukaOriginAreaModels.Select(m => m.AreaName).ToList();
            List<string> list = KukaParm.GetAreaArray(true).Select(area => area.AreaName).ToList();
            List<string> sortedItems = SortableListForm.ShowDialogAndSort(list);

            Env.Strategy = string.Join(";", sortedItems);
            SetStrategy();
            // MessageBox.Show(KukaParm.KukaAreaModels[0].AreaName);

            // KukaParm.BindAreaModel = KukaModel.Area.Find(Env.BindAreaName, KukaParm.KukaAreaModels);       // 將指定模型淺複製為 BindAreaModel
            KukaParm.BindAreaModel = KukaParm.GetAreaModel(Env.BindAreaName);   // 將指定模型淺複製為 BindAreaModel

            if (KukaParm.BindAreaModel == null)
            {
                Log.Append($"綁定區域({Env.BindAreaName})不存在", "ERROR", "LocalAreaController.cs");
            }
        }

        private void LoadNetworkInterfaces()
        {
            local_ip_combo.Items.Clear();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => ip.Address.ToString())
                .ToList();

            if (interfaces.Count == 0)
            {
                local_ip_combo.Items.Add("沒有可用的 IPv4 網卡");
                local_ip_combo.Enabled = false;
            }
            else
            {
                local_ip_combo.Items.AddRange(interfaces.ToArray());
                local_ip_combo.SelectedIndex = 0;
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            LoadNetworkInterfaces();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Console.WriteLine(KukaParm.KukaAreaModels);
        }

        private void checkBox_autoStart_CheckedChanged(object sender, EventArgs e)
        {
            Env.isAutoStart = checkBox_autoStart.Checked;
        }
    }
}
