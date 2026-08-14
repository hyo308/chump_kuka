using Chump_kuka;
using Chump_kuka.Controls;
using CookComputing.XmlRpc;
using iCAPS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Xml.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using CefSharp.DevTools.CSS;
using System.Runtime.CompilerServices;
using static Chump_kuka.KukaModel;

/// <summary>
/// KukaParm 類別 (全域設定管理)
/// 
/// 這是一個靜態類別 (static class)，用來存放關於 Kuka api 的全域參數。
/// 當參數變更時，透過 PropertyChanged 事件通知所有訂閱者 (例如 UI) 來更新顯示。
/// 
/// - 主要功能:
///   1. 當設定變更時，會觸發 PropertyChanged 事件，以便 UI 或其他物件即時更新。
///   2. 使用 static 來確保全域唯一，不需要透過 Instance 存取。
/// 
/// - 使用方式:
///   直接透過 `KukaParm.參數名稱` 存取。
/// </summary>

internal static class KukaParm
{
    private static KukaModel.CarryModel _start_node;
    private static KukaModel.CarryModel _goal_node;
    private static string _robot_status_feedback_time = "--";
    private static RobotInfo[] _robot_status_infos = null;
    private static List<KukaModel.Area> _raw_area_models = new List<KukaModel.Area>();     // 原始 API 回應區域資料
    private static List<KukaModel.Area> _area_models = new List<KukaModel.Area>();
    private static KukaModel.Area _bind_area = null;
    private static KukaModel.Area _target_area = null;

    // public static List<KukaAreaControl> AreaControls = new List<KukaAreaControl>();     // 已記錄的區域控制項
    public static string ParamPath = Path.Combine(Application.StartupPath, "config\\param.ini");
    public static event PropertyChangedEventHandler RobotStatusChanged;
    public static event PropertyChangedEventHandler AreaChanged;
    //public static event PropertyChangedEventHandler AreaStatusChanged;
    //public static event PropertyChangedEventHandler CarryChanged;
    public static event PropertyChangedEventHandler BindChanged;        // 當綁定區域改變後

    public static string GetTodayTaskPath()
    {
        string file_name = "task" + DateTime.Today.ToString(@"yyyyMMdd") + ".ini";
        return Path.Combine(Application.StartupPath, "tasks\\" + file_name);
    }

    public static string GetParamHistory { get => INiReader.ReadINIFile(ParamPath, "kuka", "area_models", 2550); }

    public static void WriteParamHistory()
    {
        if (_area_models.Count == 0) return;
        INiReader.WriteINIFile(ParamPath, "kuka", "area_models", JsonConvert.SerializeObject(_area_models));
    }

    //public static KukaModel.CarryNode StartNode       // 手動派車起點
    //{
    //    get => _start_node;
    //    set
    //    {
    //        if (_start_node != value)
    //        {
    //            _start_node = value;
    //            OnCarryChanged(nameof(StartNode));
    //        }
    //    }
    //}
    //public static KukaModel.CarryNode GoalNode      // 手動派車終點
    //{
    //    get => _goal_node;
    //    set
    //    {
    //        if (_goal_node != value)
    //        {
    //            _goal_node = value;
    //            OnCarryChanged(nameof(GoalNode));
    //        }
    //    }
    //}

    //public static string RobotStatusFeedbackTime
    //{
    //    get => _robot_status_feedback_time;
    //    set
    //    {
    //        if (!_robot_status_feedback_time.Equals(value))
    //        {
    //            _robot_status_feedback_time = value;
    //            OnRobotChanged(nameof(RobotStatusFeedbackTime));
    //        }
    //    }
    //}

    public static RobotInfo[] RobotStatusInfos
    {
        get => _robot_status_infos;
        set
        {
            //if (!JToken.DeepEquals(_robot_status_infos, value)) 
            //{
            //    _robot_status_infos = value;
            //    OnRobotChanged(nameof(RobotStatusInfos));
            //}

            _robot_status_infos = value;
            OnRobotChanged(nameof(RobotStatusInfos));
        }
    }

    public static void SetRawAreaModels(List<KukaModel.Area> input_areas)
    {
        _raw_area_models = input_areas;
        Log.Append("修改原始區域資料", "SYSTEM", "KukaParam");
    }

    /// <summary>
    /// 透過區域編碼找尋原始 API 回應的區域模型
    /// </summary>
    /// <param name="area_code"></param>
    /// <returns></returns>
    public static KukaModel.Area GetRawAreaModel(string area_code) => _raw_area_models.FirstOrDefault(p => p.AreaCode == area_code);

    /// <summary>
    /// 透過區域名稱查詢區域編碼，若無結果則返回 null
    /// </summary>
    public static string AreaName2Code(string area_name)
    {
        if (_raw_area_models.Count == 0) return null;
        
        // 建立字典
        Dictionary<string, string> dict = _raw_area_models.ToDictionary(p => p.AreaName, p => p.AreaCode);

        // 若搜尋到對應資料返回編碼資訊，反之返回 null
        if ( dict.TryGetValue(area_name, out string area_code))
        {
            return area_code;
        }
        else
        {
            return null;
        }
    }

    /*
    /// <summary>
    /// 手動排序後區域資料
    /// </summary>
    public static List<KukaModel.Area> KukaAreaModels
    {
        get => _kuka_area_models;
        set
        {
            bool change = false;

            List<KukaModel.Area> rm_temp = new List<KukaModel.Area>();
            // 遍歷現有列表資料，將不存在於輸入列表的物件移除，並更新存在物件
            // 當物件存在且修改後，從輸入列表中移除
            foreach (KukaModel.Area model in _kuka_area_models)
            {
                // 判段原始區域列表是否需要增減
                KukaModel.Area find_in_value = value.FirstOrDefault(m => m.AreaCode == model.AreaCode);
                if ( find_in_value == null)
                {
                    rm_temp.Add(model);
                    change = true;      // 紀錄需更新
                    continue;
                }

                if ( find_in_value.NodeList != null)
                {
                    change |= model.CheckAndUpdate(find_in_value);      // 判定資料內容是否變更
                }

                value.Remove(find_in_value);        // 從輸入列表中移除
            }
            if (value.Count > 0)
            {
                foreach (KukaModel.Area model in value)
                {
                    // 新資料，尚未處理控制項
                    _kuka_area_models.Add(model);
                }
                change = true;
            }
            foreach(KukaModel.Area model in rm_temp)
            {
                _kuka_area_models.Remove(model);
            }

            if (change) 
            {
                AreaChanged?.Invoke(_kuka_area_models, new PropertyChangedEventArgs(nameof(KukaAreaModels)));
                WriteParamHistory();
            }

            // origin
            //if (KukaAreaModel.CompareData(_kuka_area_models, value)) return;
            
            //_kuka_area_models = value;
            ////if (_kuka_area_models[0].NodeList.Count != 0)
            ////    AreaChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(KukaAreaModels)));
            
            //foreach (KukaAreaModel model in _kuka_area_models)
            //{
            //    model.ModelChanged += (sender, e) =>
            //    {
            //        AreaChanged?.Invoke(sender, e);  // sender 直接就是 Model
            //    };

            //    // 訂閱模型的 PropertyChanged 事件，並轉發給 ModelManager 的事件
            //    model.NodeStatusChanged += (sender, e) =>
            //    {
            //        AreaStatusChanged?.Invoke(sender, e);  // sender 直接就是 Model
            //    };

            //}
            //try
            //{
            //    AreaChanged?.Invoke(null, null);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
        }
    }
    */

    public static void UpdateAreaModels(List<KukaModel.Area> input_areas)
    {
        bool update_value = false;

        if (input_areas.Count != _area_models.Count)
        {
            update_value = true;
        }

        // 遍歷現有列表資料，將不存在於輸入列表的物件移除，並更新存在物件
        // 當物件存在且修改後，從輸入列表中移除
        foreach (KukaModel.Area model in input_areas)
        {
            model.SetIndex(_area_models, model.Index);

            // 判段原始區域列表是否需要增減
            KukaModel.Area exist_model = GetAreaModel(model.AreaCode);

            if (exist_model == null)      // 若找不到表示將輸入資料作為更新資料
            {
                update_value = true;      // 紀錄需更新
            }
            else
            {
                bool changed = exist_model.CheckAndUpdate(model);      // 判定資料內容是否變更
                if (changed)
                {
                    update_value = true;      // 紀錄需更新
                }
            }
        }

        if (update_value)
        {
            _area_models.Clear();
            _area_models.AddRange(input_areas);
            AreaChanged?.Invoke(_area_models, new PropertyChangedEventArgs("KukaAreaModels"));
            WriteParamHistory();
        }
    }

    /// <summary>
    /// 透過區域編碼找尋系統環境的區域模型
    /// </summary>
    public static KukaModel.Area[] GetAreaArray(bool is_raw = false)
    { 
        if (is_raw) return _raw_area_models.ToArray();
        else return _area_models.ToArray(); 
    }

    /// <summary>
    /// 透過 index 找尋系統環境的區域模型
    /// </summary>
    public static KukaModel.Area GetAreaModelByIndex(int index)
    {
        if (index >= _area_models.Count) index = 0;        // 若大於列表數量，從第一筆循環
        return _area_models.FirstOrDefault(area => area.Index == index);
    }

    /// <summary>
    /// 透過區域編碼找尋系統環境的區域模型
    /// </summary>
    public static KukaModel.Area GetAreaModel(string area_code) => _area_models.FirstOrDefault(area => area.AreaCode == area_code);

    /// <summary>
    /// 透過交換站編碼找尋系統環境的交換站模型
    /// </summary>
    public static KukaModel.Node GetNodeModel(string node_code, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (string.IsNullOrEmpty(node_code)) return null;

        foreach (KukaModel.Area area in _area_models)
        {
            KukaModel.Node get_node = area.GetNode(node_code);
            if (get_node == null) continue;
            else return get_node;
        }
        Log.DebugInfo($"找不到交換站[{node_code}]", filePath, lineNumber);
        return null;
    }

    /// <summary>
    /// 初始化區域搬運策略 ( index 設為 -1 )
    /// </summary>
    public static void ResetAreaStrategy()
    {
        foreach(KukaModel.Area area in _area_models)
        {
            area.SetIndex(null, -1); 
        }
    }

    /// <summary>
    /// 初始化區域搬運策略 ( index 設為 -1 )
    /// </summary>
    public static void InitAreaStrategy(List<KukaModel.Area> init_areas)
    {
        //init_areas.RemoveAll(area => area.Index == -1);

        //// 重新排序
        //_area_models = init_areas.OrderBy(area => area.Index).ToList();

        _area_models.Clear();
        for (int i = 0; i < init_areas.Count; i++)
        {
            init_areas[i].SetIndex(_area_models, i);
        }

        _area_models.AddRange(init_areas);

        AreaChanged?.Invoke(_area_models, new PropertyChangedEventArgs("KukaAreaModels"));
    }


    public static KukaModel.Area BindAreaModel
    {
        get => _bind_area;
        set
        {
            if (value == null) return;
            //if (_bind_area != null && Chump_kuka.Env.BindAreaName == value.AreaName) return;        // 非首次綁定時，跳過資料相同的處理

            Chump_kuka.Env.BindAreaName = value.AreaName;
            _bind_area = value;

            BindChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(BindAreaModel)));
        }
    }

    private static void OnRobotChanged(string propertyName)
    {
        RobotStatusChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }

    //private static void OnCarryChanged(string propertyName)
    //{
    //    CarryChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    //}

   
}




