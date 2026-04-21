using CefSharp.DevTools.CSS;
using Chump_kuka.Controls;
using Chump_kuka.Dispatchers;
using iCAPS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chump_kuka.Controller
{
    internal class LocalAreaController
    {
        private static ModbusTCPDispatcher _sensor_dispatcher = null;
        private static int[] _record_node_status;
        private static DateTime _area_update_time = DateTime.Now;       // 區域最後更新時間

        private static int[] _RecordNodeStatus        // 紀錄的區域狀態
        {
            get => _record_node_status;
            set
            {
                _record_node_status = value;
                // 建立對照表
                var map = new Dictionary<int, string>
                {
                    { 0, "無貨架" },
                    { 1, "空載" },
                    { 2, "滿載" }
                };

                // 使用 LINQ 將 int[] 轉換為 string[]
                string[] result = value.Select(n => map.ContainsKey(n) ? map[n] : "unknown").ToArray();

                BindControl.UpdateContainerText(result);
            }
        }      

        private static DateTime? full_time = null;

        // 定義貨架前後狀態轉換對應的動作：0=無動作, 1=入貨, 2=異常
        private static readonly Dictionary<(int, int), int> _carry_rules = new Dictionary<(int, int), int>
            {
                { (0, 0), 0 },      // 無變化
                { (0, 1), 2 },      // 錯誤 (空貨架進站)
                { (0, 2), 0 },      // 貨架進站
                { (1, 0), 2 },      // 錯誤 (空貨架出站)
                { (1, 1), 0 },      // 無變化
                { (1, 2), 1 },      // 入貨
                { (2, 0), 0 },      // 貨架出站
                { (2, 1), 0 },      // 取貨
                { (2, 2), 0 }       // 無變化
            };


        public static KukaAreaControl BindControl { get; set; }

        public static event EventHandler<HttpListenerDispatcher.HeardEventArgs> StepChanged;        // 流程變更事件
        public static event EventHandler<ButtonPushEventArgs> ButtonPush;

        static LocalAreaController()
        {
            _sensor_dispatcher = new ModbusTCPDispatcher();

            ChatController.StepChanged += (s, e) => StepChanged?.Invoke(s, e);
        }

        public async static Task<bool> ConnectIO_Module(IPEndPoint modbus_tco_ip)
        {
            bool isconn = await _sensor_dispatcher.Start(modbus_tco_ip);
            return isconn;
        }

        public static void BuildBindArea()
        {
            // 確認是否已經指定綁定區域名稱
            // 如果未指定名稱，開啟詢問表單
            // 反之，建立模型副本
            if (string.IsNullOrEmpty(Env.BindAreaName))
            {
                Form form = new Form() { StartPosition = FormStartPosition.CenterParent };

                ComboBox comboBox = new ComboBox();
                foreach (KukaModel.Area area in KukaParm.GetAreaArray())
                {
                    comboBox.Items.Add(area.AreaName);
                }

                FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
                flowLayoutPanel.Controls.AddRange(new Control[2] { new Label() { Text = "請選擇綁定區域" }, comboBox });

                form.Controls.Add(flowLayoutPanel);

                comboBox.SelectedValueChanged += (_sender, _e) =>
                    Env.BindAreaName = comboBox.Text;
                    // KukaParm.BindAreaModel = KukaAreaModel.Find(comboBox.Text, KukaParm.KukaAreaModels);

                form.ShowDialog();
            }
            
            //KukaParm.BindAreaModel = KukaAreaModel.Find(Env.BindAreaName, KukaParm.KukaAreaModels);       // 將指定模型淺複製為 BindAreaModel


            if (KukaParm.BindAreaModel == null)
            {
                Log.Append($"綁定區域({Env.BindAreaName})不存在", "Error", "LocalAreaController.cs");
            }
            else
            {
                // 重設區域IO數量，更新綁定區域時需要執行一次

                _sensor_dispatcher.SensorRead -= ModbusTCPDispatcher_SensorRead;
                if (KukaParm.BindAreaModel.NodeList != null)
                {
                    _sensor_dispatcher.RegisterCount = KukaParm.BindAreaModel.NodeList.Length * 2 + 1;      // 每個工作站 2 個 sensor + 按鈕 1 個
                    _sensor_dispatcher.SensorRead += ModbusTCPDispatcher_SensorRead;
                }
            }
        }


        /// <summary>
        /// 接收感測器資訊事件時，更新綁定模型資料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ModbusTCPDispatcher_SensorRead(object sender, SensorDataEventArgs e)
        {
            int[] sensor_node_status = ToNodeStatus(e.Data).ToArray();    // 當前節點狀態
            int[] model_node_status = KukaParm.BindAreaModel.NodeList.Select(n => n.RackStatus).ToArray();
            // 如果節點狀態有變更才執行 ( 或距離上次更新超過5秒 )
            if (!sensor_node_status.SequenceEqual(model_node_status) || ((DateTime.Now - _area_update_time).TotalSeconds > 5))
            {
                //KukaParm.BindAreaModel.NodeStatus = current_node_status;
                //BindControl?.UpdateContainerImage(KukaParm.BindAreaModel.NodeStatus.ToArray());        // 更新圖片

                // 判定感測器數量是否正確
                if(KukaParm.BindAreaModel.NodeList.Length != sensor_node_status.Length)
                {
                    MsgBox.Show("感測器數量與模型不符");
                    return;
                }

                // 更新感測器資訊
                for( int i = 0; i < KukaParm.BindAreaModel.NodeList.Length; i++)
                {
                    KukaParm.BindAreaModel.NodeList[i].RackStatus = sensor_node_status[i];
                }

                // 同步貨架狀態
                ChatController.SyncRackStatus(KukaParm.BindAreaModel);

                _area_update_time = DateTime.Now;       // 儲存更新時間
            }

            // 若區域滿載達指定時數後，觸發亮燈
            bool[] result = e.Data.Take(e.Data.Length - 1).ToArray();
            bool is_alert = CheckFullAreaAndDuration(result.ToList(), 5, Convert.ToInt16(Env.FullAreaCount));        // 等待5秒
            if (is_alert)
            {
                TurnOnLight();
            }
            else
            {
                TurnOffLight();
            }

            // 若按鈕狀態為 true ，觸發訊息
            int data_length = e.Data.Length - 1;
            bool button_state = e.Data[data_length];
            if (button_state)
            {
                ButtonPush?.Invoke(sender, new ButtonPushEventArgs(button_state));
            }

        }

        private static bool CheckFullAreaAndDuration(List<bool> statusList, int requiredSeconds, int? minRequired = null)
        {
            // 如果沒給數量，預設就是 Full (statusList.Count)
            int threshold = minRequired ?? statusList.Count;

            // 檢查 true 的個數是否達標
            bool is_detect = statusList.Count(x => x == true) >= threshold;

            if (is_detect)
            {
                if (full_time == null)
                    full_time = DateTime.Now;
                else if ((DateTime.Now - full_time.Value).TotalSeconds >= requiredSeconds)
                    return true;
            }
            else
            {
                full_time = null;
            }

            return false;
        }

        private static List<int> ToNodeStatus(bool[] input_readers)
        {
            List<int> status = new List<int>();
            int length = input_readers.Length - 3;
            for (int i = 0; i <= length; i += 2)
            {
                bool turtle_sensor = input_readers[i];    // 先取出當前索引的值，並遞增索引
                bool rack_sensor = input_readers[i + 1];    // 取得遞增後的索引值

                /*
                - 若 rack_sensor 為 false，代表無貨架，狀態設定為 0。
                - 若 rack_sensor 為 true 且 turtle_sensor 為 false，代表有貨架但無烏龜車，狀態設定為 1。
                - 若 rack_sensor 為 true 且 turtle_sensor 為 true，代表有貨架且有烏龜車，狀態設定為 2。
                */
                status.Add(!rack_sensor ? 0 : (!turtle_sensor ? 1 : 2));
            }
            
            return status;
        }

        public static void UpdateBindControl()
        {
            // 判定綁定區域是否存在/更新
            if (KukaParm.BindAreaModel == null || BindControl == null) return;


            // 更新控制項為綁定區域資訊
            //bind_control.Dock = DockStyle.Fill;
            //bind_control.Margin = new Padding(10);
            BindControl.AreaName = KukaParm.BindAreaModel.AreaName;
            BindControl.AreaCode = KukaParm.BindAreaModel.AreaCode;
            BindControl.AreaNode = KukaParm.BindAreaModel.NodeList;
            //BindControl.UpdateContainerImage(KukaParm.BindAreaModel.NodeStatus);        // 初次建立，更新圖片

            //BindControl.ContainerClick -= BindControl_ContainerClick;
            //BindControl.ContainerClick += BindControl_ContainerClick;
            // KukaParm.BindAreaModel.ControlUI = bind_control;
        }

        //private static void BindControl_ContainerClick(object sender, ControlClickEventArgs e)
        //{
        //    // 若點擊，將該節點加入鎖定清單，供系統搬運調節策略使用
        //    Container select_node = (e.Control as Container);
        //    if (select_node.Checked)
        //    {
        //        KukaParm.BindAreaModel.LockNodes.Add(select_node.ContainerName);
        //    }
        //    else
        //    {
        //        KukaParm.BindAreaModel.LockNodes.Remove(select_node.ContainerName);
        //    }

        //    ChatController.SyncNodeStatus(KukaParm.BindAreaModel);
        //}

        public static void InitAreaStatus()
        {
            // 當前區域狀態
            int[] current_status = KukaParm.BindAreaModel?.NodeList
               .Select(node => node.RackStatus)
               .ToArray();
            if (current_status == null) return;

            _RecordNodeStatus = current_status;
        }

        public static KukaModel.Node TryCreateCarryTask()
        {
            // 透過與歷史狀態的比對，判定當前區域的動作狀態
            // 若動作狀態為可出貨，執行以下
            // * 返回當前是否可派發任務
            // * 自動設定搬運任務的起點與終點

            // 當前區域狀態
            int[] current_status = KukaParm.BindAreaModel?.NodeList
               .Select(node => node.RackStatus)
               .ToArray();

            if (current_status == null) return null;

            // 第一次執行，初始化歷史狀態
            if (_record_node_status == null)
            {
                _RecordNodeStatus = current_status;
                return null;
            }

            // 若狀態無改變，無需處理
            if (_record_node_status.SequenceEqual(current_status))
            {
                MsgBox.ShowFlash("貨架狀態沒有變化", "區域貨架異常", 1000);
                Log.Append("貨架狀態沒有變化", "WARN", "LocalAreaController");
                return null;
            }

            List<int> node_action = new List<int>();        // 區域動作狀態判定

            // 比對每一格的前後狀態，轉換為動作
            for (int i = 0; i < current_status.Length; i++)
            {
                var key = (_record_node_status[i], current_status[i]);
                node_action.Add(_carry_rules.TryGetValue(key, out var action) ? action : 0);
            }

            // 處理異常狀況
            if (node_action.Contains(2))
            {
                MsgBox.Show("資料異常", "區域貨架異常");
                Log.Append($"資料異常 [{string.Join(",", _record_node_status)}] => [{string.Join(",", current_status)}]", "ERROR", "LocalAreaController");
                return null;
            }
            else if (node_action.Contains(1))
            {
                if (node_action.Count(n => n == 1) >= 2)
                {
                    MsgBox.Show("可派發任務 > 1 筆", "區域貨架異常");
                    Log.Append("可派發任務 > 1 筆", "WARN", "LocalAreaController");
                    return null;
                }
                
                _RecordNodeStatus = current_status;        // 更新歷史狀態

                return KukaParm.BindAreaModel.NodeList[node_action.IndexOf(1)];        // 找到第一個需要入貨的節點;
            }
            _RecordNodeStatus = current_status;        // 更新歷史狀態
            // 沒有可派任務
            return null;
        }

        

        public static void TurnOnLight()
        {
            _sensor_dispatcher.LightControl(true);
        }

        public static void TurnOffLight()
        {
            _sensor_dispatcher.LightControl(false);
        }

        public static int GetStationNo(string area_code="")
        {
            if (area_code == "")
                area_code = KukaParm.BindAreaModel.AreaCode;

            // int index = KukaParm.KukaAreaModels.FindIndex(m => m.AreaCode == KukaParm.GetAreaModel(area_code).AreaCode);
            int index = KukaParm.GetAreaModel(area_code).Index;
            return index == -1 ? 0 : index + 1;
        }

        public static void PubReady()
        {
            int _bind_station_no = GetStationNo();

            string feedback_string = INiReader.ReadINIFile(Env.LayoutPath, "Control", $"station{_bind_station_no}");
            string[] feedback_msgs = feedback_string?.Split(';');

            if (_bind_station_no != 0 && feedback_msgs != null && feedback_msgs.Length > 1)
            {
                //ChatController.SendFeedbackInfo($"station{_bind_station_no}_agv_ready");
                ChatController.SendFeedbackInfo(feedback_msgs[1]);
            }
        }

        public static void AreaReadyFunc()
        {
            int _bind_station_no = GetStationNo();

            string feedback_string = INiReader.ReadINIFile(Env.LayoutPath, "Control", $"station{_bind_station_no}");
            string[] feedback_msgs = feedback_string?.Split(';');

            if (_bind_station_no != 0 && feedback_msgs != null && feedback_msgs.Length > 0)
            {
                ChatController.SendFeedbackInfo(feedback_msgs[0]);
            }

        }

        public static void PubRobotFunc()
        {
            int _bind_station_no = GetStationNo();

            string feedback_string = INiReader.ReadINIFile(Env.LayoutPath, "Control", $"station{_bind_station_no}");
            string[] feedback_msgs = feedback_string?.Split(';');

            if (_bind_station_no != 0 && feedback_msgs != null && feedback_msgs.Length > 2)
            {
                //ChatController.SendFeedbackInfo($"station{_bind_station_no}_agv_star");
                ChatController.SendFeedbackInfo(feedback_msgs[2]);
            }

        }

        public static void PubRobotOut()
        {
            int _bind_station_no = GetStationNo();

            string feedback_string = INiReader.ReadINIFile(Env.LayoutPath, "Control", $"station{_bind_station_no}");
            string[] feedback_msgs = feedback_string?.Split(';');

            if (_bind_station_no != 0 && feedback_msgs != null && feedback_msgs.Length > 3)
            {
                //ChatController.SendFeedbackInfo($"station{_bind_station_no}_agv_begin");
                ChatController.SendFeedbackInfo(feedback_msgs[3]);
            }
                
        }

        public static void PubCarryOver()
        {
            // 頭尾未形成迴圈 (但目前規劃，最後一站搬運到第一站後，無須回報第一站完成，所以不影響)
            int _bind_station_no = GetStationNo() + 1;
            string feedback_string = INiReader.ReadINIFile(Env.LayoutPath, "Control", $"station{_bind_station_no}");
            string[] feedback_msgs = feedback_string?.Split(';');

            if (_bind_station_no != 0 && feedback_msgs != null && feedback_msgs.Length > 4)
            {
                //ChatController.SendFeedbackInfo($"station{_bind_station_no}_agv_end");
                ChatController.SendFeedbackInfo(feedback_msgs[4]);
            }
                
        }

        public static void PubCarryError(string area_code)
        {
            // 頭尾未形成迴圈
            int _bind_station_no = GetStationNo(area_code) + 1;
            string feedback_string = INiReader.ReadINIFile(Env.LayoutPath, "Control", $"station{_bind_station_no}");
            string[] feedback_msgs = feedback_string?.Split(';');

            if (_bind_station_no != 0 && feedback_msgs != null && feedback_msgs.Length > 5)
            {
                //ChatController.SendFeedbackInfo($"station{_bind_station_no}_agv_end");
                ChatController.SendFeedbackInfo(feedback_msgs[5]);
            }

        }
    }

    public class ButtonPushEventArgs : EventArgs
    {
        public bool Status { get; }

        public ButtonPushEventArgs(bool status)
        {
            Status = status;
        }
    }
}
