/* 
 * Log - Log 訊息管理
 *
 * 這是一個靜態類別(static class)，用來處理全域 Log 訊息。
 * 當訊息新增時，透過 LogChanged 事件通知所有訂閱者 (例如 UI) 來更新資訊。
 * 注意，更新訊息時，會直接覆蓋先前消息!
 * 
 * - 主要功能:
 *   1. 當設定變更時，會觸發 LogChanged 事件，以便 UI 或其他物件即時更新。
 *   2. 使用 static 來確保全域唯一。
 * 
 * - 使用方式:
 *   上傳時:
       Log.Append("Log 訊息", "Log 狀態", "發布 Log 的程序");
 *   讀取時:  
       Log.LogChanged += LogChanged;
 
       private void LogChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
       {
           List<string> log_info = Log.LogInfo;
       }
 */

using CefSharp;
using Chump_kuka.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using WebSocketSharp;

namespace Chump_kuka
{
    public static class Log 
    {
        private static int _add_index = 1;      // 已記錄的 Log 數量
        private static string _filter_status = "";      // 紀錄當前篩選標籤名稱
        private static string _current_log_csv_path = "";       // 自動記錄檔地址 
        private static readonly object _csvLock = new object(); // 用來鎖定寫入動作，避免跨執行緒存取衝突

        public static BindingList<LogMsg> LogData = new BindingList<LogMsg>();      // Log 列表
        public static BindingList<LogMsg> FilterData = new BindingList<LogMsg>();      // Log 列表
        public static SynchronizationContext UiContext { get; set; }        // 加入控制項的 SynchronizationContext.Current，防止跨執行續問題

        public static event AppendLogEventHandler LogAppended;      // 
        static Log()
        {
            string file_name = "log" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            _current_log_csv_path =  Path.Combine(Application.StartupPath, "logs\\" + file_name);
        }

        /// <summary>
        /// 加入一筆普通訊息
        /// </summary>
        public static void SystemInfo(
            string info_msg,
            [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNumber = 0,
            string log_method = "") 
            => Append(info_msg, "SYSTEM", $"{System.IO.Path.GetFileName(filePath)}:{lineNumber}", log_method);

        /// <summary>
        /// 加入一筆除錯訊息
        /// </summary>
        public static void DebugInfo(
            string info_msg,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0, 
            string log_method = "")
            => Append(info_msg, "Debug", $"{System.IO.Path.GetFileName(filePath)}:{lineNumber}", log_method);

        public static void LogTemplate(string message, string status, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => Append(message, "TTEST", $"{System.IO.Path.GetFileName(filePath)}:{lineNumber}");


        public static void Append(string message, string status, string section, string log_method = "")
        {
            LogMsg new_msg = new LogMsg(_add_index++, message, status, section);

            // 防止跨執行續問題
            if (UiContext != null)
            {
                UiContext.Post(_ => 
                {
                    LogData.Add(new_msg);

                    // 更新篩選資料
                    if (status == _filter_status)
                    {
                        FilterData.Add(new_msg);
                    }
                }, null);
            }
            else
            {
                // 如果 context 沒設，直接加（但會有跨執行緒風險）
                LogData.Add(new_msg);

                // 更新篩選資料
                if (status == _filter_status)
                {
                    FilterData.Add(new_msg);
                }
            }

            AppendCsv(new_msg);     // 寫入本地端資料

            if (log_method != "") LogAppended?.Invoke(null, new_msg);
        }

        /// <summary>
        /// 利用 Status 篩選資料，並記錄於 FilterData
        /// </summary>
        /// <param name="filter_status"></param>
        public static void FilterStatus(string filter_status)
        {
            _filter_status = filter_status;
            var aa = LogData.Where(c => c.Status == filter_status).ToList();
            FilterData = new BindingList<LogMsg>(aa);
        }

        /// <summary>
        /// 將目前的資料儲存至 CSV。
        /// </summary>
        public static void AppendCsv(LogMsg log_msg)
        {
            lock (_csvLock)
            {
                try
                {
                    // 使用 FileShare.ReadWrite 允許其他程序(如Excel)開啟檔案時也能夠寫入
                    using (var fs = new FileStream(_current_log_csv_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {
                        string safeMessage = log_msg.Message?.Replace('\n', ' ') ?? "";
                        writer.WriteLine($"{log_msg.ID},{safeMessage},{log_msg.Status},{log_msg.Section},{log_msg.CreateDate}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Log Error] 無法寫入 Log 檔案: {ex.Message}");
                }
            }
        }
    }
}
