using Chump_kuka.Dispatchers;
using iCAPS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Chump_kuka.KukaModel;

namespace Chump_kuka.Controller
{
    internal class ChatController
    {
        private static MqttDispatcher _mqtt;
        private static bool _is_master = false;

        public static event EventHandler<HttpListenerDispatcher.HeardEventArgs> StepChanged;
        public static event CarryTasksEventHandler CarryTaskUpdated;

        static ChatController()
        {

        }

        public async static Task<bool> Init(bool is_server, IPEndPoint listen_server_info)
        {
            _is_master = is_server;

            // 初始化，移除所有綁定事件
            KukaParm.RobotStatusChanged -= KukaParm_RobotStatusChanged;
            HttpListenerDispatcher.Heard -= HttpListenerDispatcher_Heard;

            // 重新建立新的 MQTT 實例
            if (_mqtt != null)
            {
                await _mqtt?.StopBroker();
                await _mqtt?.CloseClient();
            }

            _mqtt = new MqttDispatcher();

            if (_is_master)
                await _mqtt.StartBroker(listen_server_info.Port);

            bool success = await _mqtt.InitClient(listen_server_info.Address.ToString(), listen_server_info.Port);
            if (!success) 
                return success;
            
            if (_is_master)
            {
                _mqtt.Subscriber("carry", CarryCb);
                _mqtt.Subscriber("carry/auto", CarryAutoCb);
                _mqtt.Subscriber("feedback", FeedCb);
                _mqtt.Subscriber("del_task", DelTaskCb);
                _mqtt.Subscriber("cancel_task", CancelTaskCb);
                _mqtt.Subscriber("update_task", UpdateTaskCb);
                _mqtt.Subscriber("lock_request", LockCb);

                KukaParm.RobotStatusChanged += KukaParm_RobotStatusChanged;          // 伺服器機器人資訊更新時，發佈到客戶端
                HttpListenerDispatcher.Heard += HttpListenerDispatcher_Heard;
            }

            _mqtt.Subscriber("log", LogCb);
            _mqtt.Subscriber("hello", HelloCb);
            _mqtt.Subscriber("robot", RobotCb, 0);
            _mqtt.Subscriber("area", AreaCb);
            _mqtt.Subscriber("area/rack_status", RackCb);
            _mqtt.Subscriber("area/node_status", NodesCb);
            _mqtt.Subscriber("carry/finish", CarryFinishCb);
            _mqtt.Subscriber("carry/list", CarryListCb);
            _mqtt.Subscriber("heard", HeardCb);
            _mqtt.Subscriber("log/error", ErrorCb);

             SayHi();        // 初次上線，通知取得區域資料
            return true;
        }

        public static void PubLog(string message)
        {
            Log.Append(message, "ASYNC", "ChatController");
            _mqtt?.Publisher("log", message);
        }

        public static void PubError(string message)
        {
            ErrorCb(message);
            _mqtt.Publisher("log/error", message);
        }

        private static void LogCb(string message)
        {
            Log.Append(message, "ASYNC", "ChatController");
        }

        private static void ErrorCb(string message)
        {
            // MsgBox.Show(message, "錯誤");
            Log.Append(message, "ERROR", "ChatController");
        }

        private static void HelloCb(string message)
        {
            if (!_is_master)        // 非伺服端不要傳遞訊息
                return;

            try
            {
                Log.Append("接收客戶端訊號...", "INFO", "ChatController");
                string jsonOutput = JsonConvert.SerializeObject(KukaParm.GetAreaArray(), Formatting.Indented);

                _mqtt.Publisher("area", jsonOutput);
                Log.Append("回應當前區域狀態", "INFO", "ChatController");
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON 序列化失敗：" + ex.Message, "錯誤");
            }
        }

        private static void HeardCb(string message)
        {
            HttpListenerDispatcher.HeardEventArgs data = JsonConvert.DeserializeObject<HttpListenerDispatcher.HeardEventArgs>(message);
            if (data.StartAreaCode == KukaParm.BindAreaModel.AreaCode)
            {
                PubToLocalController(null, data);     // 傳送至下一階段
            }
        }

        private static void RobotCb(string message)
        {
            KukaParm.RobotStatusInfos = JsonConvert.DeserializeObject<KukaModel.RobotInfo[]>(message);
        }

        private static void AreaCb(string message)
        {
            // 若字串為區域類別，解析資料訊息後，將比較後差異處，更新為接收資料

            List<KukaModel.Area> areas = JsonConvert.DeserializeObject<List<KukaModel.Area>>(message);

            // 如果接收列表資訊與當前不同，更新當前列表
            //if (KukaParm.KukaAreaModels.Count == 0)
            //    KukaParm.KukaAreaModels = areas;
            string[] origin_code_array = KukaParm.GetAreaArray().Select(m => m.AreaCode).ToArray();       // 將所有代號取出為陣列
            string[] source_code_array = areas.Select(m => m.AreaCode).ToArray();       // 將所有代號取出為陣列
            if (origin_code_array != source_code_array)     // 只判定區域代號是否修正
            {
                KukaParm.UpdateAreaModels(areas);
                //foreach (KukaModel.Area source_area in areas)
                //{
                //    var base_model = KukaParm.KukaAreaModels.FirstOrDefault(b => b.AreaName == source_area.AreaName);
                //    base_model.CompareAndUpdate(source_area);
                //}
            }
        }

        private static void RackCb(string message)
        {
            KukaModel.Area receive_area = JsonConvert.DeserializeObject<KukaModel.Area>(message);

            // 找到區域模型並修改節點貨架狀態
            KukaModel.Area find_area = KukaParm.GetAreaModel(receive_area.AreaCode);
            if (find_area != null)
            {
                // find_area.NodeStatus = receive_area.NodeStatus;
                for (int i = 0; i < receive_area.NodeList.Length; i++)
                {
                    find_area.NodeList[i].RackStatus = receive_area.NodeList[i].RackStatus;
                }
            }
            else
            {
                Log.DebugInfo($"收到貨架更新訊息但找不到區域[{receive_area.AreaCode}]");
            }
        }

        private static void NodesCb(string message)
        {
            KukaModel.Area receive_area = JsonConvert.DeserializeObject<KukaModel.Area>(message);

            KukaModel.Area find_area = KukaParm.GetAreaModel(receive_area.AreaCode);
            if (find_area != null)
            {
                for (int i = 0; i < receive_area.NodeList.Length; i++)
                {
                    find_area.NodeList[i].IsLock = receive_area.NodeList[i].IsLock;
                    find_area.NodeList[i].NodeStatus = receive_area.NodeList[i].NodeStatus;
                }
            }
        }

        private static void LockCb(string message)
        {
            KukaModel.Area receive_area = JsonConvert.DeserializeObject<KukaModel.Area>(message);

            KukaModel.Area find_area = KukaParm.GetAreaModel(receive_area.AreaCode);
            if (find_area != null)
            {
                for (int i = 0; i < receive_area.NodeList.Length; i++)
                {
                    find_area.NodeList[i].IsLock = receive_area.NodeList[i].IsLock;
                }
            }
            
            NodesCb(message);
            SyncNodeStatus1(find_area);
        }

        private static void FeedCb(string message)
        {
            if (_is_master)
            {
                //Log.Append("接收到報工任務", "INFO", "ChatController");
                PubLog("接收到報工任務");
                SendFeedbackInfo(message);
            }
        }

        private static void CarryCb(string message)
        {
            try
            {
                PubLog("接收排程搬運任務");

                KukaModel.CarryModel[] nodes = JsonConvert.DeserializeObject<KukaModel.CarryModel[]>(message);
                AppendCarryTask(nodes[0], nodes[1], true);
            }
            catch (Exception _e)
            {
                PubLog("錯誤: 接收排程搬運任務" + _e.ToString());
            }
        }
        private static void CarryAutoCb(string message)
        {
            try
            {
                PubLog("接收基本搬運任務");
                KukaModel.CarryModel[] nodes = JsonConvert.DeserializeObject<KukaModel.CarryModel[]>(message);
                AppendCarryTask(nodes[0], nodes[1], false);
            }
            catch (Exception _e)
            {
                PubLog("錯誤: 接收基本搬運任務" + _e.ToString());
            }
        }

        private static void CarryFinishCb(string message)
        {
            // 解析訊息為 [ 任務編號, 區域標號 ]
            List<string> mission_area = JsonConvert.DeserializeObject<List<string>>(message);
            SendCarryFinish(mission_area[0], mission_area[1]);
        }

        private static void CarryListCb(string message)
        {
            KukaModel.SimpleCarryTask[] tasks = JsonConvert.DeserializeObject<List<KukaModel.SimpleCarryTask>>(message).ToArray();
            CarryTaskUpdated?.Invoke(null, tasks);
        }

        private static void DelTaskCb(string message)
        {
            Log.Append($"已接收刪除任務[{message}]", "CHAT", "ChatController");
            int.TryParse(message, out int task_id);
            CarryTaskController.RemoveTask(task_id);
        }

        private static void CancelTaskCb(string message)
        {
            Log.Append($"已接收強制取消任務[{message}]", "CHAT", "ChatController");
            CarryTaskController.CancelTask(message);
        }

        private static void UpdateTaskCb(string message)
        {
            SyncCarryTask(CarryTaskController.GetQueueArray());
        }

        /// <summary>
        /// 監聽 http 訊息後，觸發事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HttpListenerDispatcher_Heard(object sender, HttpListenerDispatcher.HeardEventArgs e)
        {
            string target_area_code;

            // 如果 step 為 7 代表搬運任務已完成
            if (e.Step == 7)
            {
                CarryTaskController.FeedbackFinish(e.MissionCode);
                //int index = KukaParm.KukaAreaModels.FindIndex(m => m.AreaCode == e.AreaCode);       // 找到起點區域的 index
                //int next_index = (index+1) % KukaParm.KukaAreaModels.Count;     // 使用「模運算」達到環狀效果
                // KukaModel.Area heard_area = KukaParm.KukaAreaModels.FirstOrDefault(area => area.AreaCode == e.StartAreaCode);
                KukaModel.Area heard_area = KukaParm.GetAreaModel(e.StartAreaCode);
                //SendCarryFinish(e.MissionCode, heard_area.Next().AreaCode);         // 通知目標區域更新(起點區域index+1)
                /* 上行錯誤訊息
   HttpListener發生錯誤[System.NullReferenceException: 並未將物件參考設定為物件的執行個體。
   於 Chump_kuka.Controller.ChatController.HttpListenerDispatcher_Heard(Object sender, HeardEventArgs e) 於 C:\Users\11228\OneDrive - 財團法人精密機械研究發展中心\chump_kuka\CNCAppPlatform\Controller\ChatController.cs: 行 295
   於 Chump_kuka.Dispatchers.HttpListenerDispatcher._kuka_listener_MessageReceived(Object sender, HttpMessageEventArgs e) 於 C:\Users\11228\OneDrive - 財團法人精密機械研究發展中心\chump_kuka\CNCAppPlatform\Dispatchers\HttpListenerDispatcher.cs: 行 217
   於 iCAPS.Managers.HttpListenerManager.<HandleClientAsync>d__16.MoveNext() 於 C:\Users\11228\OneDrive - 財團法人精密機械研究發展中心\chump_kuka\CNCAppPlatform\Services\Managers\HttpListenerManager.cs: 行 99
--- 先前擲回例外狀況之位置中的堆疊追蹤結尾 ---
   於 System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   於 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   於 System.Runtime.CompilerServices.TaskAwaiter.GetResult()
   於 iCAPS.Managers.HttpListenerManager.<<Start>b__14_0>d.MoveNext() 於 C:\Users\11228\OneDrive - 財團法人精密機械研究發展中心\chump_kuka\CNCAppPlatform\Services\Managers\HttpListenerManager.cs: 行 63]
                 */

                // 修復方法：如果找不到對應區域，則不進行後續動作，並記錄警告訊息
                if (heard_area != null)
                {
                    var nextArea = heard_area.Next();
                    if (nextArea != null)
                    {
                        SendCarryFinish(e.MissionCode, nextArea.AreaCode);        // 通知目標區域更新(起點區域index+1)
                    }
                    else
                    {
                        PubLog($"警告：Area_{e.StartAreaCode} 的 Next() 為 null");
                    }
                }
                else
                {
                    PubLog($"警告：找不到 StartAreaCode={e.StartAreaCode} 對應的區域");
                }

                e.Step = 0;
            }

            // 若監聽目標為綁定區域
            if (e.StartAreaCode == KukaParm.BindAreaModel.AreaCode)
            {
                PubToLocalController(sender, e);     // 傳送至下一階段
                
            }
            else
            {
                // 若為其他區域，則傳送至其他模組中
                string message = JsonConvert.SerializeObject(e, Formatting.Indented);

                // Send("heard", message);
                _mqtt.Publisher("heard", message);
            }

            PubLog($"Area_{e.StartAreaCode}:in step [{e.Step}]");
        }

        private static void PubToLocalController(object sender, HttpListenerDispatcher.HeardEventArgs e)
        {
            switch (e.Step)
            {
                case 1:     // 
                    break;
                case 2:     // 機器人進站
                    LocalAreaController.PubRobotFunc();       // station_agv_star
                    break;
                case 4:     // 機器人出站
                    LocalAreaController.PubRobotOut();      // station_agv_begin
                    break;
                case 5:     // 搬運任務完成
                    LocalAreaController.PubCarryOver();
                    break;
                case 7:
                    break;
            }

            StepChanged?.Invoke(sender, e);     // 通知任務步驟事件更新
        }

        private static void KukaParm_RobotStatusChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                string jsonOutput = JsonConvert.SerializeObject(KukaParm.RobotStatusInfos, Formatting.Indented);

                _mqtt.Publisher("robot", jsonOutput, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON 序列化失敗：" + ex.Message, "錯誤");
            }
        }

        public static void SayHi()
        {
            Log.Append("呼叫伺服器，取得區域狀態", "INFO", "ChatController");
            _mqtt.Publisher("hello", "Hi");
        }       

        /// <summary>
        /// 所有主/從站同步搬運任務
        /// </summary>
        public static void SyncCarryTask(KukaModel.SimpleCarryTask[] tasks)
        {
            CarryTaskUpdated?.Invoke(null, tasks);

            string task_list_json = JsonConvert.SerializeObject(tasks, Formatting.Indented);

            _mqtt?.Publisher("carry/list", task_list_json);
        }

        /*/// <summary>
        /// 所有主/從站同步所有節點狀態
        /// </summary>
        public static void SyncNodeStatus(KukaModel.Area update_model)
        {
            string nodes_json = JsonConvert.SerializeObject(update_model, Formatting.Indented);

            _mqtt.Publisher("area/nodes", nodes_json);
        }*/

        /// <summary>
        /// 同步貨架狀態
        /// </summary>
        /// <param name="area_rack">字串陣列，第一字串為區域代號</param>
        public static void SyncRackStatus(KukaModel.Area area)
        {
            string nodes_json = JsonConvert.SerializeObject(area, Formatting.Indented);

            _mqtt.Publisher($"area/rack_status", nodes_json);
        }

        /// <summary>
        /// 更新節點狀態(僅server)
        /// </summary>
        /// <param name="area_rack">字串陣列，第一字串為區域代號</param>
        public static void SyncNodeStatus1(KukaModel.Area area)
        {
            if (!_is_master)        // 非伺服端不要傳遞訊息
                return;

            string nodes_json = JsonConvert.SerializeObject(area, Formatting.Indented);

            _mqtt.Publisher($"area/node_status", nodes_json);
        }

        /// <summary>
        /// 請求鎖定節點
        /// </summary>
        /// <param name="area_code"></param>
        public static void RequestLockNode(KukaModel.Area request_area)
        {
            string area_json = JsonConvert.SerializeObject(request_area, Formatting.Indented);

            if (_is_master)
            {
                LockCb(area_json);
            }
            else
            {
                _mqtt.Publisher("lock_request", area_json);
            }
        }

        /// <summary>
        /// 當完成搬運任務時，通知目標區域更新狀態
        /// </summary>
        /// <param name="area_code"></param>
        public static void SendCarryFinish(string mission_code, string area_code)
        {
            // 如果目標是當前模組，直接觸發步驟 0，通知工作站更新狀態
            if (area_code == KukaParm.BindAreaModel.AreaCode)
            {
                HttpListenerDispatcher.HeardEventArgs _e = new HttpListenerDispatcher.HeardEventArgs(mission_code, area_code, 0);

                StepChanged.Invoke(null, _e);
                Log.Append("Get finish", "INFO", "ChatController");
            }
            else
            {
                if (_is_master)
                {
                    string[] send_info = new string[2] { mission_code, area_code };
                    _mqtt.Publisher("carry/finish", JsonConvert.SerializeObject(send_info));
                }
                    
            }
        }

        public static void SendFeedbackInfo(string feedback_msg)
        {
            if (_is_master)
            {
                FeedbackDispatcher.SendToRecordSystem(feedback_msg);
                Log.Append($"發送報工訊息{feedback_msg}", "INFO", "ChatController");
            }
            else
            {
                // Send("feedback", feedback_msg);
                _mqtt.Publisher("feedback", feedback_msg);
            }
        }

        public static void AppendCarryTask(KukaModel.CarryModel start_carry_info, KukaModel.CarryModel goal_carry_info, bool wait=true)
        {
            if (_is_master)        
            {
                // 若為 master 端，將任務加入等候區
                CarryTaskController.AddToQueue(start_carry_info, goal_carry_info, out _, wait);
            }
            else
            {
                // 若為 slave 端，傳送節點資訊，讓伺服器處理
                // 若 wait = true，透過 "carry" 主題傳遞資料，代表需要等待叫車訊號。
                string topic_name = wait ? "carry" : "carry/auto";

                KukaModel.CarryModel[] nodes = new KukaModel.CarryModel[2] { start_carry_info, goal_carry_info };
                string task_node_string = JsonConvert.SerializeObject(nodes, Formatting.Indented);

                _mqtt.Publisher(topic_name, task_node_string);
            }
        }

        public static void DelTask(string task_id)
        {
            if (_is_master)
            {
                DelTaskCb(task_id);
            }
            else
            {
                _mqtt?.Publisher("del_task", task_id);
            }
        }

        public static void UpdateTaskList()
        {
            if (_is_master)
            {
                UpdateTaskCb("update_task");
            }
            else
            {
                _mqtt?.Publisher("update_task", "update_task");
            }
        }

        public static void CancelTask(string task_id)
        {
            if (_is_master)
            {
                CancelTaskCb(task_id);
            }
            else
            {
                _mqtt.Publisher("cancel_task", task_id);
            }
            
        }
    }
}
