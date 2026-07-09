using Chump_kuka.Controller;
using Chump_kuka.Dispatchers;
using iCAPS;
using IniParser.Model;
using IniParser;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static Chump_kuka.KukaModel;
using System.Collections.Generic;
using System.Web.Caching;
using System.Windows;
using System.Reflection;

namespace Chump_kuka
{
    internal class CarryTaskController
    {
        private static bool _agv_running = false;
        private static int _task_id = 1;
        private static KukaModel.CarryTask _current_task = null;

        private static BindingList<KukaModel.CarryTask> _task_queue = new BindingList<KukaModel.CarryTask>();      // 搬運任務佇列

        private static System.Timers.Timer _task_timer;
        // private static System.Timers.Timer _task_asker;     // 當系統無主動回報任務狀態時，強制監控狀態
        private static bool _current_task_running = false;

        private static List<string> _node_fill_missions = new List<string>();       // 目標交換站滿的任務清單
        private static List<string> _area_fill_missions = new List<string>();       // 目標區域滿的任務清單
        private static List<string> _traced_missions = new List<string>();          // 已印過尋找邏輯的任務清單

        private enum TaskStatus
        {
            Normal = 0,
            PlanA = 1,
            PlanB = 2,
        }

        public static event Action<bool> OnTimerAlive;     // 計時器啟用事件

        public static KukaModel.CarryTask CurrentTask { get => _current_task; }

        static CarryTaskController()
        {
            FeedbackDispatcher.Called += FeedbackDispatcher_Called;

            if (!Env.ICapsServer) return;
            InitRecordTasks();
            _task_queue.ListChanged += task_queue_ListChanged;
        }

        public static KukaModel.CarryTask FindCarryTask(string mission_code) => _task_queue.FirstOrDefault(task => task.MissionCode == mission_code);

        /*/// <summary>
        /// 取得當天 InI 檔案內紀錄的任務，並實例
        /// </summary>
        private static void InitRecordTasks()
        {
            string file_path = KukaParm.GetTodayTaskPath();
            int.TryParse(INiReader.ReadINIFile(file_path, "tasks", "task_last_id"), out int record_count);        // 任務數量
            if (record_count > 0)
            {
                for (int index = 1; index <= record_count; index++)
                {
                    string task_json = INiReader.ReadINIFile(file_path, "tasks", $"{index}", 65535);
                    
                    CarryTask read_task = Newtonsoft.Json.JsonConvert.DeserializeObject<CarryTask>(task_json);

                    // 若 DeletedAt == string.Empty 表示任務已刪除，跳過此筆
                    if (read_task.IsDeleted) continue;

                    // 未完成任務需綁定模型
                    if(read_task.FinishTime == null)
                    {
                        if (read_task?.StartNode.NodeModel != null)
                        {
                            foreach (Area area in KukaParm.GetAreaArray())
                            {
                                KukaModel.Node node = area.GetNode(read_task?.StartNode.NodeModel.NodeCode);
                                if (node != null)
                                {
                                    read_task.StartNode.NodeModel = node;
                                    break;
                                }
                            }
                        }
                        if (read_task?.GoalNode.NodeModel != null)
                        {
                            foreach (Area area in KukaParm.GetAreaArray())
                            {
                                KukaModel.Node node = area.GetNode(read_task?.StartNode.NodeModel.NodeCode);
                                if (node != null)
                                {
                                    read_task.GoalNode.NodeModel = node;
                                    break;
                                }
                            }
                        }
                    }
                    
                    _task_queue.Add(read_task);
                }

                _task_id = record_count + 1;

                initTimer();        // 自動開始流程
            }

            
        }*/

        /// <summary>
        /// 讀取 ini檔案，並取得 section 內容
        /// </summary>
        public static Dictionary<string, string> ReadBySection(string filePath, string sectionName)
        {
            // 1. 建立解析器
            var parser = new FileIniDataParser();

            Dictionary<string, string> output = new Dictionary<string, string>();

            // 2. 讀取 INI 檔案
            try
            {
                IniData data = parser.ReadFile(filePath);

                // 檢查 Section 是否存在
                if (data.Sections.ContainsSection(sectionName))
                {
                    KeyDataCollection databaseSection = data.Sections[sectionName];

                    // 迭代該 Section 中的所有 Key-Value
                    foreach (KeyData key in databaseSection)
                    {
                        output[key.KeyName] = key.Value;
                        // Console.WriteLine($"{key.KeyName} = {key.Value}");
                    }

                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message);
                return output;
            }
}

        private static void InitRecordTasks()
        {
            // 取得今日日期以讀取任務清單
            string file_path = KukaParm.GetTodayTaskPath();
            Dictionary<string, string> tasks_array = ReadBySection(file_path, "tasks");

            //if (tasks_array.Count == 0) Task.Run(() => MsgBox.Show("今日尚未建立可執行任務"));

            foreach (KeyValuePair<string, string> task in tasks_array)
            {

                CarryTask read_task = Newtonsoft.Json.JsonConvert.DeserializeObject<CarryTask>(task.Value);

                // 若 DeletedAt == string.Empty 表示任務已刪除，跳過此筆
                if (read_task.IsDeleted) continue;

                //// 未完成任務需綁定模型
                //if (read_task.FinishTime == null)
                //{
                //    if (read_task?.StartNode.NodeModel != null)
                //    {
                //        foreach (Area area in KukaParm.GetAreaArray())
                //        {
                //            KukaModel.Node node = area.GetNode(read_task?.StartNode.NodeModel.NodeCode);
                //            if (node != null)
                //            {
                //                read_task.StartNode.NodeModel = node;
                //                break;
                //            }
                //        }
                //    }
                //    if (read_task?.GoalNode.NodeModel != null)
                //    {
                //        foreach (Area area in KukaParm.GetAreaArray())
                //        {
                //            KukaModel.Node node = area.GetNode(read_task?.GoalNode.NodeModel.NodeCode);
                //            if (node != null)
                //            {
                //                read_task.GoalNode.NodeModel = node;
                //                break;
                //            }
                //        }
                //    }
                //}

                _task_queue.Add(read_task);
            }

            _task_id = tasks_array.Count + 1;

            initTimer();        // 自動開始流程
        }

        /// <summary>
        /// 當任務狀態更改時，自動儲存 InI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void task_queue_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 當任務狀態更改時，自動儲存，防止系統崩潰後，資料消失
            // 任務清單檔案依日期建立&儲存
            
            string file_path = KukaParm.GetTodayTaskPath();
            //int last_id = _task_queue.Count > 0 ? _task_queue[_task_queue.Count - 1].ID : 0;
            //INiReader.WriteINIFile(file_path, "tasks", "task_last_id", last_id.ToString());     // 紀錄最後一筆任務id
            
            int change_index = e.NewIndex;      // 受變更影響的索引

            // 更新事件
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                KukaModel.CarryTask task = _task_queue[change_index];       // 取得更新項目
                string task_msg = Newtonsoft.Json.JsonConvert.SerializeObject(task);
                INiReader.WriteINIFile(file_path, "tasks", task.ID.ToString(), task_msg);       //單筆任務寫入
            }
            else if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                
            }

            ChatController.SyncCarryTask(GetQueueArray());      // 同步&更新所有 UI
        }

        private static void initTimer()
        {
            Log.Append("候車計時器初始化", "SYSTEM", nameof(CarryTaskController));
            // 設定計時器
            _task_timer = new System.Timers.Timer();
            _task_timer.Interval = 1000; // 每 1 秒請求一次

            _task_timer.Elapsed += ProcessNextApiAsync;
            _task_timer.AutoReset = true; // 是否重複執行（true 表示會一直觸發）
            _task_timer.Start();
            //_task_timer.Enabled = true;
            //_task_timer.Tick += ProcessNextApiAsync;

            //_task_asker = new System.Timers.Timer();
            //_task_asker.Interval = 500; // 每 0.5 秒請求一次

            //_task_asker.Elapsed += TaskAsker;
            //_task_asker.AutoReset = true; // 是否重複執行（true 表示會一直觸發）
        }

        private static async void ProcessNextApiAsync(object sender, EventArgs e)
        {
            // 停止計時器，確保在請求處理中不會再觸發計時器
            _task_timer.Stop();

            OnTimerAlive?.Invoke(true);

            await Task.Delay(800);
            OnTimerAlive?.Invoke(false);
            // AppendRobotStatusTask();        // 機器人狀態查詢為固定行程

            bool build_task = FindAndAssignTask();
            // 如果有派發任務，則停止計時器
            if (!build_task)
            {
                _task_timer.Start();
            }
            //else
            //{
            //    _task_asker.Start();        // 開啟監控狀態
            //}
        }
        /*private static async void TaskAsker(object sender, EventArgs e)
        {
            // AppendTaskLog(_current_task.MissionCode, "未接收回報，主動監控狀態");

            // 取得當前任務
            foreach (var robot_info in KukaParm.RobotStatusInfos)
            {
                if ((string)robot_info["robotId"] == "8503484")
                {
                    if ((string)robot_info["missionCode"] == _current_task?.MissionCode)
                    {
                        if ((string)robot_info["occupyStatus"] == "1")        // 占用中
                        {
                            _current_task_running = true;
                        }
                    }
                    else
                    {
                        if (_current_task_running)
                        {
                            _current_task_running = false;
                            if (_current_task != null)
                            {
                                AppendTaskLog(_current_task?.MissionCode, "監控結束訊號");
                                FeedbackFinish(_current_task?.MissionCode);      // 主動回報結束
                            }
                            // _task_asker.Stop();
                        }
                    }
                }
            } 
        }*/

        private static void FeedbackDispatcher_Called(object sender, TextEventArgs e)
        {
            // TODO 等待上一筆任務結束


            // 接收到叫車命令，尋找可派發任務
            string start_area_code = e.Message;

            string can_carry_mission_code = GetCallTask(start_area_code);
            if (can_carry_mission_code != null)
            {
                // KukaApiController.PubCarryTask();
                ChatController.PubLog($"接收叫車任務，等待執行。");
                HttpListenerDispatcher.ManualHeardEvent(can_carry_mission_code, start_area_code, 1);        // 觸發接收報工系統 call 事件
            }
            else
            {
                ChatController.PubLog($"接收叫車任務，無可執行搬運任務。(起始節點: {start_area_code})");
            }
        }

        /// <summary>
        /// 將搬運任務加入等待列表
        /// </summary>
        /// <param name="wait">若為 true，需等待報工系統通知；反之，直接派發任務。</param>
        public static void AddToQueue(KukaModel.CarryModel start_node, KukaModel.CarryModel goal_node, out string mission_code, bool wait=true, 
                    bool is_plan = false)
        {
            mission_code = "";

            // 初始化計時器
            if (_task_timer == null)
            {
                initTimer();
            }
            // 判斷起始區域是否有代號
            //if (start_node.AreaCode == null)
            //{
            //    // start_node.AreaCode = KukaParm.KukaAreaModels.FirstOrDefault(area => area.GetNode(start_node.NodeModel.NodeCode) != null).AreaCode;
            //    start_node.AreaCode = KukaParm.KukaAreaModels.FirstOrDefault(area => area.GetNode(start_node.NodeModel.NodeCode) != null).AreaCode;
            //}

            // 判定是否建立重複起始點(起始點已在任務列表中，且該任務尚未完成)
            bool exists_task = _task_queue.Any(m => 
                                        m.StartNode.NodeCode == start_node.NodeCode && 
                                        m.FinishTime == null);
            if (exists_task)
            {
                ChatController.PubError("派發重複任務 !");
                return;
            }
            
            // 建立搬運任務資訊
            KukaModel.CarryTask task = new KukaModel.CarryTask(_task_id, !wait, start_node, goal_node);
            mission_code = task.MissionCode;

            if (is_plan) task.SetPlanTask();      // 判斷是否為策略任務

            // 最後一區的任務優先執行
            // if (start_node.AreaCode == KukaParm.KukaAreaModels[KukaParm.KukaAreaModels.Count - 1].AreaCode)
            if (start_node.AreaCode == KukaParm.GetAreaModelByIndex(KukaParm.GetAreaArray().Length - 1).AreaCode)
            {
                task.CallTask();
                ChatController.PubLog($"[{task}]為終點區域任務，系統自動叫車。");
            }
            _task_id++;

            _task_queue.Add(task);      // 加入佇列

            // 如果起點是 node 標記為任務占用
            if(start_node.NodeCode != "")
            {
                KukaParm.GetNodeModel(start_node.NodeCode).NodeStatus = 1;
            }
            ChatController.SyncNodeStatus1(KukaParm.GetAreaModel(start_node.AreaCode));
        }

        /// <summary>
        /// 尋找可執行的搬運任務
        /// </summary>
        /// <returns></returns>
        private static bool FindAndAssignTask()
        {
            try
            {
                // 優先派發策略任務
                CarryTask plan_task = _task_queue.FirstOrDefault(task => task.IsCalled && task.FinishTime == null && task.IsPlan);
                if (plan_task != null)
                {
                    KukaApiController.PubCarryTask(plan_task);
                    // 0818測試
                    // if(Debugger.IsAttached) _current_task = plan_task;
                    ChatController.PubLog($"已派發策略任務，ID: {plan_task.ID}");
                    return true;
                }

                foreach (KukaModel.CarryTask task in _task_queue)
                {
                    if (task.IsCalled && task.FinishTime == null)
                    {
                        bool isFirstTrace = !_traced_missions.Contains(task.MissionCode);
                        if (isFirstTrace)
                        {
                            _traced_missions.Add(task.MissionCode);
                        }

                        if (isFirstTrace) Log.DebugInfo($"任務[{task.ID}] - 尋找目標交換站");
                        KukaModel.Node goal_node = string.IsNullOrEmpty(task.GoalNode.NodeCode) ? null : KukaParm.GetNodeModel(task.GoalNode.NodeCode);
                        
                        // 檢查目標是否為貨架點
                        if (goal_node != null)     // 目標為貨架點
                        {
                            // 檢查目標貨架點是否搬允許搬運

                            if (goal_node.IsEmpty())
                            {
                                // 貨架點無占用，可直接派發任務
                            }
                            else
                            {
                                // 貨架點占用，判斷是否上鎖
                                if (goal_node.IsLock)
                                {
                                    KukaModel.Node start_node = KukaParm.GetNodeModel(task.StartNode.NodeCode);
                                    // 貨架點已鎖定，執行策略
                                    KukaModel.Area start_area = KukaParm.GetAreaModel(task.StartNode.AreaCode);

                                    TaskPlan(task.ID, goal_node, start_area);
                                    return false;
                                }

                                if (!_node_fill_missions.Contains(task.MissionCode))
                                {
                                    // ChatController.PubLog($"當前任務[{task.ID}]無法執行。目標貨架點滿載，優先執行下一筆任務");
                                    string task_json = Newtonsoft.Json.JsonConvert.SerializeObject(task);
                                    ChatController.PubLog($"當前任務[{task.ID}]無法執行。目標貨架點[{goal_node.NodeCode}]滿載{{{goal_node.NodeStatus}, {goal_node.RackStatus}}}，優先執行下一筆任務。\n{task_json}");
                                    _node_fill_missions.Add(task.MissionCode);
                                }
                                continue;
                            }
                        }
                        else       // 目標為區域
                        {
                            if (isFirstTrace) Log.DebugInfo($"任務[{task.ID}] - 尋找目標區域");
                            KukaModel.Area goal_area = KukaParm.GetAreaModel(task.GoalNode.AreaCode);
                            // 先搜尋是否有空貨架點
                            KukaModel.Node empty_node = goal_area?.GetEmptyNode();
                            if (empty_node == null)
                            {
                                // 沒有空貨架點，搜尋是否有上鎖貨架點
                                KukaModel.Node lock_node = goal_area?.GetLockNode();
                                if (lock_node == null)
                                {
                                    if (!_area_fill_missions.Contains(task.MissionCode))
                                    {
                                        // 找不到上鎖貨架，執行下一筆
                                        ChatController.PubLog($"當前任務[{task.ID}]無法執行。目標區域皆滿載，優先執行下一筆任務");
                                        _area_fill_missions.Add(task.MissionCode );
                                    }
                                        
                                    continue;
                                }
                                Log.DebugInfo($"找到可換交換站{lock_node.NodeCode}");
                                // 找到上鎖貨架，執行策略
                                KukaModel.Area start_area = KukaParm.GetAreaModel(task.StartNode.AreaCode);
                                TaskPlan(task.ID, lock_node, start_area);
                                return false;
                            }
                            else
                            {
                                // 加入可搬運貨架點
                                task.GoalNode.NodeCode = empty_node.NodeCode;
                            }
                        }

                        // 派發 API
                        KukaApiController.PubCarryTask(task);
                        _current_task = task;
                        ChatController.PubLog($"已派發任務，ID: {_current_task.ID}");

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Append($"尋找任務完成時發生錯誤[{ex.ToString()}]", "ERROR", "CarryTaskController");
                return false;
            }

        }

        private static async Task<bool> TaskPlan(int task_id, KukaModel.Node lock_node, Area start_area)
        {
            Log.DebugInfo("開始執行策略判斷");
            // 執行搬運策略
            // 需確認已經指定目標貨架點，並且該貨架點已鎖定
            await Task.Delay(500);
            string start_empty_node_code = start_area.GetEmptyNode()?.NodeCode;
            string plan_mission_code = "";
            if (start_empty_node_code != null)       // 策略A => 將上鎖貨架搬運到當前區域無佔用位置
            {
                ChatController.PubLog($"[task_{task_id}] > 啟動策略A，搬運到起始區域。");

                AddToQueue(new CarryModel(lock_node.NodeName, lock_node.Parent.AreaCode, lock_node.NodeCode),
                            new CarryModel(start_empty_node_code, start_area.AreaCode, start_empty_node_code),
                            out plan_mission_code, false, true);
                AppendTaskLog(plan_mission_code, $"[task_{task_id}] 策略A [搬運到起始區域]\n===");
            } 
            else        // 策略B => 將上鎖貨架搬運到下一區域無佔用位置
            {
                // 目前區域無空位，更換策略至下一區域
                KukaModel.Node next_empty_node = lock_node.Parent?.Next().GetEmptyNode();
                if (next_empty_node == null) return false;      // 找不到下一目標

                ChatController.PubLog($"[task_{task_id}] > 啟動策略B，搬運到下一區域。");
                AddToQueue(new CarryModel(lock_node.NodeName, lock_node.Parent.AreaCode, lock_node.NodeCode),
                            new CarryModel(next_empty_node.NodeName, next_empty_node.Parent.AreaCode, next_empty_node.NodeCode),
                            out plan_mission_code, false, true);
                AppendTaskLog(plan_mission_code, $"[task_{task_id}] 策略B [搬運到下一區域]\n===");
            }
            _current_task = _task_queue.FirstOrDefault(task => task.MissionCode == plan_mission_code);
            //_current_plan_task = _task_queue.FirstOrDefault(task => task.ID == task_id);        // 將原任務加入至plan
            return true;
        }

        /// <summary>
        /// 將任務模型列表轉換成簡化模型，方便資料傳輸
        /// </summary>
        /// <returns></returns>
        public static KukaModel.SimpleCarryTask[] GetQueueArray()
        {
            // var simple_queue = _task_queue.Select(queue => new KukaModel.SimpleCarryTask(queue)).ToArray();
            var simple_queue = _task_queue.Select(queue => {
                KukaModel.SimpleCarryTask simple_task = new KukaModel.SimpleCarryTask(queue);

                if(_current_task != null)
                    if (queue.MissionCode == _current_task.MissionCode) simple_task.RunningState = 1;       // 若執行中，標記為 1

                return simple_task;
                }).ToArray();

            return simple_queue;
        }
        
        /// <summary>
        /// 區域被呼叫，找到可執行的搬運任務
        /// </summary>
        /// <param name="start_area_code"></param>
        /// <returns></returns>
        private static string GetCallTask(string start_area_code)
        {
            // 找到符合開始區域且尚未執行的第一筆資料
            KukaModel.CarryTask call_task = _task_queue.FirstOrDefault(task => task.StartNode.AreaCode == start_area_code &&
                                                                                task.IsCalled == false &&
                                                                                task.FinishTime == null);
            if (call_task != null)
            {
                call_task.CallTask();
                return call_task.MissionCode;
            }

            return null;
        }

        //private static bool PubCarryList()
        //{
        //    if (_current_task != null)
        //    {
        //        // Console.WriteLine($"找到的資料: Start = {foundModel.StartNode}, Goal = {foundModel.GoalNode}");
        //        // 自動指派起始終點節點
        //        KukaParm.StartNode = _current_task.StartNode;
        //        KukaParm.GoalNode = _current_task.GoalNode;
        //        // _current_task.Called = true;       // 已呼叫

        //        ChatController.SyncCarryTask(GetQueueArray());      // 同步&更新所有 UI
        //        return true;
        //    }
        //    else
        //    {
        //        Console.WriteLine("找不到指定名稱的資料。");
        //        return false;
        //    }
        //}

        private static void ClearMissionLogs(string mission_code)
        {
            _traced_missions.Remove(mission_code);
            _node_fill_missions.Remove(mission_code);
            _area_fill_missions.Remove(mission_code);
        }

        /// <summary>
        /// 回報任務完成，並重置 _current_task
        /// </summary>
        public static void FeedbackFinish(string mission_code)
        {
            ClearMissionLogs(mission_code);

            //if (_current_task != null) 
            //    _current_task.FinishTime = DateTime.Now;
            KukaModel.CarryTask finish_task = FindCarryTask(mission_code);
            finish_task.TaskComplete(true);

            try
            {
                KukaModel.Node start_node = KukaParm.GetNodeModel(finish_task.StartNode.NodeCode);
                KukaModel.Node goal_node = KukaParm.GetNodeModel(finish_task.GoalNode.NodeCode);
                // 判斷結完成的任務是否為策略任務
                if (finish_task.IsPlan)
                {
                    Log.Append($"完成策略任務[{mission_code}]", "INFO", "CarryTaskController");
                    // 若是策略任務，移轉鎖定狀態
                    if (start_node.IsLock)
                    {
                        start_node.IsLock = false;
                        goal_node.IsLock = true;
                        ChatController.SyncNodeStatus1(goal_node.Parent);
                    }
                }
                start_node.NodeStatus = 0;      // 解除起始交換站的執行預備狀態

                _task_timer.Start();

                ChatController.SyncNodeStatus1(start_node.Parent);
            }
            catch (Exception e)
            {
                Log.Append($"回報任務完成時發生錯誤[{e.ToString()}]", "ERROR", "CarryTaskController");
            }
        }

        /// <summary>
        /// 回報任務失敗，並重置 _current_task
        /// </summary>
        public static void FeedbackFail(string mission_code)
        {
            ClearMissionLogs(mission_code);

            if (mission_code == _current_task?.MissionCode) _current_task = null;

            KukaModel.CarryTask fail_task = FindCarryTask(mission_code);
            fail_task.TaskComplete(false);
            KukaModel.Node start_node = KukaParm.GetNodeModel(fail_task.StartNode.NodeCode);
            start_node.NodeStatus = 0;
            ChatController.SyncNodeStatus1(start_node.Parent);

            _task_timer.Start();
        }

        /// <summary>
        /// 強制取消指定任務
        /// </summary>
        public static void CancelTask(string task_id)
        {
            int.TryParse(task_id, out int cancel_id);
            if (cancel_id == 0)
            {
                ChatController.PubError($"錯誤: 請確認搬運任務編號正確[{cancel_id}]");
                return;
            }

            KukaModel.CarryTask target = _task_queue.FirstOrDefault(m => m.ID == cancel_id);       // 找到 ID 對應任務
            if (target != null)
            {
                KukaApiController.PubCarryCancel(target.MissionCode);
                target.AppendLog("已強制取消搬運任務");
                FeedbackFail(target.MissionCode);

                ChatController.PubLog($"已強制取消搬運任務[{cancel_id}]");
            }
            else
            {
                ChatController.PubLog($"找不到指定任務[{cancel_id}]");
            }
        }

        /// <summary>
        /// 增加任務狀態紀錄
        /// </summary>
        /// <param name="log_message"></param>
        public static void AppendTaskLog(string mission_code, string log_message)
        {
            //if (_current_task != null)
            //    _current_task.LogMsg += $"[{DateTime.Now.ToString(@"MM/dd tt hh:mm:ss")}] {log_message}\n";
            KukaModel.CarryTask task = _task_queue.FirstOrDefault(t => t.MissionCode == mission_code);
            task.AppendLog(log_message);
        }

        /// <summary>
        /// 刪除指定任務
        /// </summary>
        /// <param name="log_message"></param>
        public static void RemoveTask(int task_id)
        {
            // 任務 id 不得為 0
            if (task_id == 0)
            {
                ChatController.PubError($"錯誤: 請確認搬運任務編號正確[{task_id}]");
                return;
            }

            // 執行中任務不得刪除
            if (_current_task?.ID == task_id)
            {
                ChatController.PubError($"搬運任務[{task_id}]運行中，無法移除");
                return;
            }

            KukaModel.CarryTask target = _task_queue.FirstOrDefault(m => m.ID == task_id);       // 找到 ID 對應任務
            if (target != null)
            {
                ClearMissionLogs(target.MissionCode);

                // target.StartNode.NodeModel.NodeStatus = 0;
                // ChatController.SyncNodeStatus(target.StartNode.NodeModel.Parent);
                target.SoftDelete();        // 軟刪除

                // 從當前佇列移除
                _task_queue.Remove(target);
                ChatController.PubLog($"已從任務列表中移除搬運任務[{task_id}]");

                KukaModel.Node start_node = KukaParm.GetNodeModel(target.StartNode.NodeCode);
                start_node.NodeStatus = 0;
                ChatController.SyncNodeStatus1(start_node.Parent);
            }
            else
            {
                ChatController.PubLog($"找不到指定任務[{task_id}]");
            }
        }

        
    }
}
