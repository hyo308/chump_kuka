using Chump_kuka.Controls;
using iCAPS;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using Chump_kuka.Controller;
using System.Runtime.Serialization;
using CefSharp.DevTools.CSS;
using System.Xml.Linq;
using System.Reflection;

namespace Chump_kuka
{
    internal class KukaModel
    {
        public class Node : INotifyPropertyChanged
        {
            private int _rack_status = -1;      // 貨架狀態 {0: 無貨架, 1: 空貨架, 2: 滿貨架}
            private int _node_status = 0;      // 節點狀態 {0: 普通, 1: 已建立任務}
            private bool _is_lock = false;

            // 建立屬性值發生變化的通知事件
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            public string NodeCode { get; set; }
            public string NodeName { get => NodeCode; }

            public bool IsLock
            {
                get => _is_lock;
                set
                {
                    if (_is_lock == value) return;
                    _is_lock = value;
                    OnPropertyChanged(nameof(IsLock));
                    KukaParm.WriteParamHistory();
                }
            }

            public int RackStatus
            {
                get => _rack_status;
                set
                {
                    if (_rack_status == value) return;
                    _rack_status = value;
                    OnPropertyChanged(nameof(RackStatus));
                }
            }

            /// <summary>
            /// 節點狀態 {0: 普通, 1: 已建立任務}
            /// </summary>
            public int NodeStatus
            {
                get => _node_status;
                set
                {
                    if (_node_status == value) return;

                    if (_node_status == 2)
                    {
                        MsgBox.Show("選用貨架已佔用!");
                        return;
                    }

                    _node_status = value;
                    OnPropertyChanged(nameof(NodeStatus));
                    KukaParm.WriteParamHistory();
                }
            }

            [JsonIgnore]
            public Area Parent 
            { 
                get; 
                set; 
            }

            [JsonConstructor]
            public Node(string nodeCode, bool isLock, int rackStatus, int nodeStatus)
            {
                NodeCode = nodeCode;
                _is_lock = isLock;
                _rack_status = rackStatus;
                _node_status = nodeStatus;
            }

            public Node(string jj)
            {
                NodeCode = jj;
            }

            public bool IsEmpty() => NodeStatus == 0 && RackStatus == 0;

            public override string ToString() =>
                Newtonsoft.Json.JsonConvert.SerializeObject(this);

            public Node Clone()
            {
                return new Node(this.NodeCode)
                {
                    RackStatus = this.RackStatus,
                    NodeStatus = this.NodeStatus
                };
            }

            // 為了比較內容，要實作 Equals 與 GetHashCode
            public override bool Equals(object obj)
            {
                if (obj is Node other)
                    return NodeCode == other.NodeCode && IsLock == other.IsLock && NodeStatus == other.NodeStatus && RackStatus == other.RackStatus;

                return false;
            }

            public override int GetHashCode()
            {
                // 使用 .NET Framework 安全寫法
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (NodeCode?.GetHashCode() ?? 0);
                    hash = hash * 23 + RackStatus.GetHashCode();
                    hash = hash * 23 + NodeStatus.GetHashCode();
                    return hash;
                }
            }

        }

        public class Area : INotifyPropertyChanged
        {
            //public event PropertyChangedEventHandler NodeStatusChanged;
            //public event PropertyChangedEventHandler ModelChanged;

            // 建立屬性值發生變化的通知事件
            public event PropertyChangedEventHandler PropertyChanged;

            #region 屬性
            /// <summary>
            /// 區域所在順序 (從 0 開始; -1 表示為定義)
            /// </summary>
            [JsonProperty]
            public int Index { get; private set; } = -1;

            /// <summary>
            /// 區域列表
            /// </summary>
            [JsonIgnore]
            public List<Area> AreaQueue { get; private set; }

            /// <summary>
            /// 區域編碼 ex: area001
            /// </summary>
            [JsonProperty]
            public string AreaCode { get; private set; }

            /// <summary>
            /// 區域名稱 ex: 加工區
            /// </summary>
            [JsonProperty]
            public string AreaName { get; private set; }

            /// <summary>
            /// 區域類型 {1: 庫區, 2: 作業區, 3: 暫存區, 4: 緩存區}
            /// </summary>
            [JsonProperty]
            public int AreaType { get; private set; }

            /// <summary>
            /// 點位集合
            /// </summary>
            [JsonProperty]
            public Node[] NodeList { get; private set; }

            #endregion 屬性

            public Area(string areaCode, string areaName, int areaType, Node[] nodeList)
            {
                AreaCode = areaCode;
                Rename(areaName);       // AreaName = areaName;
                AreaType = areaType; 
                UpdateNodes(nodeList);      // NodeList = nodeList;
            }

            /// <summary>
            /// 設定區域所在順序
            /// </summary>
            public void SetIndex(List<Area> area_queue, int index)
            {
                AreaQueue = area_queue;
                Index = index;
            }

            public void Rename(string new_area_name)
            {
                AreaName = new_area_name;
                OnPropertyChanged(nameof(AreaName));        // 屬性發生變化
            }

            public void UpdateNodes(Node[] node_list)
            {
                if (node_list == null) return;
                if (NodeList == null || !NodeList.SequenceEqual(node_list))
                {
                    NodeList = node_list;

                    foreach (Node node in node_list)
                    {
                        node.Parent = this;
                    }

                    OnPropertyChanged(nameof(NodeList));        // 屬性發生變化
                    KukaParm.WriteParamHistory();
                }
            }

            /// <summary>
            /// 取得指定節點模型
            /// </summary>
            public Node GetNode(string node_code)
            {
                Node node = NodeList.FirstOrDefault(_node => _node.NodeCode == node_code);
                return node;
            }

            public bool CheckAndUpdate(Area param)
            {
                bool change = false;
                if (AreaName != param.AreaName)
                {
                    AreaName = param.AreaName;
                    change = true;
                }
                if (AreaType != param.AreaType)
                {
                    AreaType = param.AreaType;
                    change = true;
                }
                if (NodeList == null && param.NodeList != null)
                {
                    NodeList = param.NodeList;
                    foreach (var n in NodeList) n.Parent = this;
                    change = true;
                }
                else if (NodeList != null && param.NodeList != null)
                {
                    if (NodeList.Length == param.NodeList.Length && NodeList.Select(n => n.NodeCode).SequenceEqual(param.NodeList.Select(n => n.NodeCode)))
                    {
                        for (int i = 0; i < NodeList.Length; i++)
                        {
                            Node current = NodeList[i];
                            Node incoming = param.NodeList[i];

                            if (current.RackStatus != incoming.RackStatus)
                            {
                                current.RackStatus = incoming.RackStatus;
                                change = true;
                            }
                            if (current.NodeStatus != incoming.NodeStatus)
                            {
                                current.NodeStatus = incoming.NodeStatus;
                                change = true;
                            }
                            if (current.IsLock != incoming.IsLock)
                            {
                                current.IsLock = incoming.IsLock;
                                change = true;
                            }
                        }
                    }
                    else if (!Enumerable.SequenceEqual(NodeList, param.NodeList))
                    {
                        NodeList = param.NodeList;
                        foreach (var n in NodeList) n.Parent = this;
                        change = true;
                    }
                }

                return change;
            }

            public void CompareAndUpdate(Area source_model)
            {
                this.AreaName = source_model.AreaName;
                this.AreaType = source_model.AreaType;
                this.NodeList = source_model.NodeList;
                //this.NodeStatus = source_model.NodeStatus;
            }

            public static bool CompareData(List<Area> sourceData, List<Area> targetData) => sourceData.Select(m => m.AreaName).SequenceEqual(targetData.Select(m => m.AreaName));

            public Area Next()
            {
                //int index = KukaParm.KukaAreaModels.IndexOf(this);
                //if (index == -1)
                //{
                //    return this;
                //}
                //else if (index == KukaParm.KukaAreaModels.Count - 1)
                //{
                //    // 該筆資料為最後一筆，下一筆回到首筆資料
                //    return KukaParm.KukaAreaModels[0];
                //}
                //else
                //{
                //    return KukaParm.KukaAreaModels[index + 1];
                //}
                // return KukaParm.GetAreaModelByIndex(Index+1);
                if(AreaQueue == null)
                {
                    MsgBox.Show("尚未設定區域列表", "ERROR");
                    return null;
                }
                int next_index = Index + 1;
                if (next_index >= AreaQueue.Count) next_index = 0;        // 若大於列表數量，從第一筆循環
                return AreaQueue.FirstOrDefault(area => area.Index == next_index);
            }

            public Node GetEmptyNode() => NodeList.FirstOrDefault(node => node.IsEmpty());
            public Node GetLockNode() => NodeList.FirstOrDefault(node => node.IsLock);

            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
                

            public override string ToString() => AreaName;
        }

        public class CarryModel
        {
            public string Name { get; set; } = "null";

            public string AreaCode { get; set; }

            public string NodeCode { get; set; }

            public CarryModel(string name, string area_code, string node_code)
            {
                Name = name;
                AreaCode = area_code;
                NodeCode = node_code;
                //IsArea = node_model == null ? true : false;
            }
        }

        public class SimpleCarryTask
        {
            public string Called { get; set; }
            public int ID { get; set; }
            public string StartNode { get; set; }
            public string GoalNode { get; set; }
            public string CreateTime { get; set; }
            public string FinishTime { get; set; }
            public string LogMsg { get; set; }
            public int RunningState { get; set; } = 0;      // {1: 執行中}

            public SimpleCarryTask() { }
            public SimpleCarryTask(CarryTask task)
            {
                ID = task.ID;
                StartNode = task.StartNode.Name;
                GoalNode = task.GoalNode.Name;
                CreateTime = task.CreateTime.ToString(@"MM/dd tt hh:mm");
                Called = task.IsCalled ? "🔔" : "🔕";
                LogMsg = task.LogMsg;

                if (task.FinishTime == null)
                {
                    FinishTime = "";
                }
                else if (task.FinishTime == DateTime.MinValue)
                {
                    FinishTime = "已取消";
                }
                else
                {
                    FinishTime = task.FinishTime?.ToString(@"MM/dd tt hh:mm");
                }
            }
        }

        public class CarryTask : INotifyPropertyChanged
        {
            private bool _is_loading = false;

            [JsonProperty]
            public int ID { get; private set; }

            [JsonProperty]
            public string MissionCode { get; private set; }

            [JsonProperty]
            public CarryModel StartNode { get; private set; }

            [JsonProperty]
            public CarryModel GoalNode { get; private set; }

            [JsonProperty]
            public DateTime CreateTime { get; private set; }
            
            [JsonProperty]
            public DateTime? FinishTime {  get; private set; }

            [JsonProperty]
            public string LogMsg { get; private set; }

            [JsonProperty]
            public bool IsCalled { get; private set; }

            [JsonProperty]
            public bool IsPlan { get; private set; } = false;

            /// <summary>
            /// 任務是否被軟刪除
            /// </summary>
            [JsonProperty]
            public bool IsDeleted { get; private set; } = false;

            public CarryTask(int task_id, bool called, CarryModel start_node, CarryModel goal_node)
            {
                ID = task_id;
                //MissionCode = $"mission{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                MissionCode = $"mission{DateTimeOffset.UtcNow.Ticks}";  //建立任務中創建的策略任務秒數短不宜以秒為單位，改以Ticks為單位，增加任務編號的唯一性
                IsCalled = called;
                StartNode = start_node;
                GoalNode = goal_node;
                CreateTime = DateTime.Now;
                LogMsg = $"已建立任務[{MissionCode}]\n";

                // 若 task_id != 0，表示非 Json 反序列化觸發
                if (task_id != 0)
                {
                    _is_loading = true;
                    WriteIni();
                }
            }

            private void WriteIni()
            {
                if (!_is_loading) return;
                string file_path = KukaParm.GetTodayTaskPath();
                string task_msg = Newtonsoft.Json.JsonConvert.SerializeObject(this);
                INiReader.WriteINIFile(file_path, "tasks", ID.ToString(), task_msg);       //單筆任務寫入
            }

            public void SetPlanTask()
            {
                IsPlan = true;
                OnPropertyChanged(nameof(IsPlan));
            }

            public void CallTask()
            {
                IsCalled = true;
                OnPropertyChanged(nameof(IsCalled));
            }

            public void TaskComplete(bool success)
            {
                FinishTime = success ? DateTime.Now : DateTime.MinValue;
                OnPropertyChanged(nameof(FinishTime));
            }

            public void SoftDelete()
            {
                IsDeleted = true;
                AppendLog("已刪除任務");
                OnPropertyChanged(nameof(IsDeleted));
            }

            public void AppendLog(string msg)
            {
                LogMsg += $"[{DateTime.Now.ToString(@"MM/dd tt hh:mm:ss")}] {msg}\n";
                OnPropertyChanged(nameof(LogMsg));
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                WriteIni();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            [OnDeserialized]
            internal void OnDeserializedMethod(StreamingContext context)
            {
                // Json 反序列化後，將 _is_loading 設定為 true，防止反序列化時，重複寫入文件
                _is_loading = true;
            }
        }

        public class RobotInfo
        {
            /// <summary>
            /// 機器人編號
            /// </summary>
            public string RobotId { get; set; }

            /// <summary>
            /// 機器人型號
            /// </summary>
            public string RobotType { get; set; }

            /// <summary>
            /// 機器人持有容器編號
            /// </summary>
            public string ContainerCode { get; set; }

            /// <summary>
            /// 地圖編號
            /// </summary>
            public string MapCode { get; set; }

            /// <summary>
            /// 片區編號
            /// </summary>
            public string FloorNumber { get; set; }

            /// <summary>
            /// 工廠或倉庫編號
            /// </summary>
            public string BuildingCode { get; set; }

            /// <summary>
            /// 機器人狀態
            /// </summary>
            public RobotStatus Status { get; set; }

            /// <summary>
            /// 是否占用
            /// </summary>
            public int OccupyStatus { get; set; }

            /// <summary>
            /// 電量
            /// </summary>
            public float BatteryLevel { get; set; }

            /// <summary>
            /// 當前點位
            /// </summary>
            public string NodeCode { get; set; }

            /// <summary>
            /// 當前任務編號
            /// </summary>
            public string MissionCode { get; set; }

            /// <summary>
            /// 更新時間
            /// </summary>
            public string ReceiveTime { get; set; } = DateTime.Now.ToString(@"G");
        }

        public enum RobotStatus
        {
            ///<summary> 離場 </summary>
            Leaving = 1,

            /// <summary> 離線 </summary>
            Offline = 2,

            /// <summary> 閒置 </summary>
            Idle = 3,

            /// <summary> 任務中 </summary>
            Working = 4,

            /// <summary> 充電中 </summary>
            Charging = 5,

            /// <summary> 更新中 </summary>
            Updating = 6,

            /// <summary> 異常 </summary>
            Error = 7
        }
    }
}
