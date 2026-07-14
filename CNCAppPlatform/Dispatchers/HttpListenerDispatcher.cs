using iCAPS.Managers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using Chump_kuka.Services.Managers;
using Chump_kuka.Controller;
using System.Messaging;
using iCAPS;
using iCAPS.temp;

namespace Chump_kuka.Dispatchers
{
    internal class HttpListenerDispatcher
    {
        private static HttpListenerManager _kuka_listener;
        private static int _area_step = 0;

        public static event EventHandler<HeardEventArgs> Heard;

        public static async Task<bool> StartKukaListener(string url)
        {
            if (_kuka_listener != null && _kuka_listener.IsRunning) return true;

            _kuka_listener = new HttpListenerManager(url);
            bool success = _kuka_listener.Start();
            if (success)
            {
                await _kuka_listener.WaitForServerToStartAsync();
                _kuka_listener.MessageReceived += _kuka_listener_MessageReceived;
            }

            return _kuka_listener.IsRunning;
        }

        private static void CalcAreaStep(string mission_code, string task_status)
        {
            string status = "INFO";
            string message = "";
            switch (_area_step)
            {
                case 0:
                    if (task_status == "MOVE_BEGIN")
                    {
                        _area_step += 1;
                        message = "接收任務";
                    }
                    else
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[MOVE_BEGIN]，實際狀態[{task_status}]。";
                    }
                    break;
                case 1:
                    if (task_status == "ARRIVE")
                    {
                        _area_step += 1;
                        message = "到達起點";
                    }
                    else
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[ARRIVE]，實際狀態[{task_status}]。";
                    }
                    break;
                case 2:
                    if (task_status == "UP_CONTAINER")
                    {
                        _area_step += 1;
                        message = "頂升貨架";
                    }
                    else
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[UP_CONTAINER]，實際狀態[{task_status}]。";
                    }
                    break;
                case 3:
                    if (task_status == "MOVE_BEGIN")
                    {
                        _area_step += 1;
                        message = "離開起點";
                    }
                    else
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[MOVE_BEGIN]，實際狀態[{task_status}]。";
                    }
                    break;
                case 4:
                    if (task_status == "ARRIVE")
                    {
                        _area_step += 1;
                        message = "到達終點";
                    }
                    else
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[ARRIVE]，實際狀態[{task_status}]。";
                    }
                    break;
                case 5:
                    if (task_status == "DOWN_CONTAINER")
                    {
                        _area_step += 1;
                        message = "放下貨架";
                    }
                    else 
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[DOWN_CONTAINER]，實際狀態[{task_status}]。";
                    }
                    break;
                case 6:
                    if (task_status == "COMPLETED")
                    {
                        _area_step += 1;
                        message = "完成任務";
                    }
                    else 
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[COMPLETED]，實際狀態[{task_status}]。";
                    }
                    break;
                case 7:
                    // 等同於 case 0
                    if (task_status == "MOVE_BEGIN")
                    {
                        _area_step = 1;
                        message = "接收任務";
                    }
                    else
                    {
                        status = "ERROR";
                        message = $"欲接收狀態[MOVE_BEGIN]，實際狀態[{task_status}]。";
                    }
                    break;
            }

            Log.Append(message, status, "HttpListenerDispatcher");
            CarryTaskController.AppendTaskLog(mission_code, message);
        }

        //static string area_code = "";      // 任務起始區域編碼

        private static void _kuka_listener_MessageReceived(object sender, HttpMessageEventArgs e)
        {
            // TODO 當前現在是否於綁定區域
            
            // 將 JSON 解析為 JObject
            JObject jsonObj = JObject.Parse(e.Message);

            //// 設定一個映射字典，鍵是原來的值，值是要替換的值
            //var valueMapping = new Dictionary<string, string>
            //    {
            //        { "MOVE_BEGIN", "开始移动" },
            //        { "ARRIVED", "到达任务节点" },
            //        { "UP_CONTAINER", "顶升完成" },
            //        { "DOWN_CONTAINER", "放下完成" },
            //        { "COMPLETED", "任务完成" },
            //        { "CANCELED", "任务取消完成" },
            //        { "ERROR", "任务执行报错" }
            //    };

            //// 設定一個鍵名映射字典
            //var keyMapping = new Dictionary<string, string>
            //    {
            //        { "missionCode", "作业id " },
            //        { "viewBoardType", "作业类型 " },
            //        { "containerCode", "容器编号" },
            //        { "currentPosition", "容器当前位置 " },
            //        { "slotCode", "当前所在槽位" },
            //        { "robotId", "执行当前任务的机器人id " },
            //        { "missionStatus", "作业当前状态 " },
            //        { "message", "说明信息" },
            //        { "missionData", "需要上报的定制信息对象" }
            //    };

            string task_status = jsonObj["missionStatus"].ToString();
            string mission_code = jsonObj["missionCode"].ToString();

            

            // 從第2步(到達區域)判斷目前區域編碼
            //if (_area_step == 2)
            //{
            //    string current_position = jsonObj["currentPosition"].ToString();
            //    KukaModel.Area area_model = KukaParm.KukaAreaModels.FirstOrDefault(area => area.GetNode(current_position) != null);
            //    area_code = area_model.AreaCode;
            //}

            // 用 mission code 取代原先節點判斷起始位置
            KukaModel.CarryTask receive_task = CarryTaskController.FindCarryTask(mission_code);

            // 若任務取消
            if (task_status == "ERROR")
            {
                CarryTaskController.AppendTaskLog(mission_code, $"任務異常 [{jsonObj["message"]}]");
                iCAPS.MsgBox.Show($"請嘗試：檢查目標格位是否安全後取消當前任務重新派發 \n[{jsonObj["message"]}]","任務異常");
            }
            else if (task_status == "CANCELED")
            {
                CarryTaskController.FeedbackFail(mission_code);     // 回報任務失敗
                LocalAreaController.PubCarryError(receive_task.StartNode.AreaCode);     // 通知報工系統任務失敗
                _area_step = 0;     // 重置步數
                return;
            }
            else
            {
                CalcAreaStep(mission_code, task_status);      // 計算當前步數
            }

            // 觸發接收事件
            if (receive_task != null)
            {
                Heard.Invoke(sender, new HeardEventArgs(mission_code, receive_task.StartNode.AreaCode, _area_step));
            }


            string response_json = "{ \"code\": \"0\", \"message\": \"\", \"success\": true, \"data\":[] }";
            _kuka_listener.MessageResponse(e.Context, response_json);
        }
        public static void ManualHeardEvent(string mission_code, string start_area_code,int step)
        {
            Heard.Invoke(null, new HeardEventArgs(mission_code, start_area_code, step));
        }

        //private static void _kuka_listener_MessageReceived1(object sender, HttpMessageEventArgs e)
        //{
        //    // 將 JSON 解析為 JObject
        //    JObject jsonObj = JObject.Parse(e.Message);

        //    // 設定一個映射字典，鍵是原來的值，值是要替換的值
        //    var valueMapping = new Dictionary<string, string>
        //        {
        //            { "MOVE_BEGIN", "开始移动" },
        //            { "ARRIVED", "到达任务节点" },
        //            { "UP_CONTAINER", "顶升完成" },
        //            { "DOWN_CONTAINER", "放下完成" },
        //            { "COMPLETED", "任务完成" },
        //            { "CANCELED", "任务取消完成" },
        //            { "ERROR", "任务执行报错" }
        //        };

        //    // 設定一個鍵名映射字典
        //    var keyMapping = new Dictionary<string, string>
        //        {
        //            { "missionCode", "作业id " },
        //            { "viewBoardType", "作业类型 " },
        //            { "containerCode", "容器编号" },
        //            { "currentPosition", "容器当前位置 " },
        //            { "slotCode", "当前所在槽位" },
        //            { "robotId", "执行当前任务的机器人id " },
        //            { "missionStatus", "作业当前状态 " },
        //            { "message", "说明信息" },
        //            { "missionData", "需要上报的定制信息对象" }
        //        };

        //    // 使用映射字典進行鍵名與值的轉換
        //    foreach (var key in keyMapping.Keys)
        //    {
        //        if (jsonObj.ContainsKey(key))
        //        {
        //            // 轉換鍵名
        //            jsonObj[keyMapping[key]] = jsonObj[key];
        //            jsonObj.Remove(key); // 刪除舊的鍵

        //            // 如果有值轉換，轉換值
        //            if (jsonObj[keyMapping[key]] != null && valueMapping.ContainsKey(jsonObj[keyMapping[key]].ToString()))
        //            {
        //                jsonObj[keyMapping[key]] = valueMapping[jsonObj[keyMapping[key]].ToString()];
        //            }
        //        }
        //    }

        //    // 格式化 JSON 並顯示
        //    string formattedJson = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

        //    MessageBox.Show(formattedJson, "JSON 格式化顯示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    // MessageBox.Show($"收到 POST 請求: {postData}");
        //}

        public class HeardEventArgs : EventArgs
        {
            public string MissionCode { get; private set; }
            public string StartAreaCode { get; set; }
            public int Step { get; set; }

            public HeardEventArgs(string mission_code, string start_area_code, int step)
            {
                MissionCode = mission_code;
                StartAreaCode = start_area_code;
                Step = step;
            }
        }
    }
}
