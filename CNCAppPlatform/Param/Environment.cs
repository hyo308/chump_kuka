using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using iCAPS;

namespace Chump_kuka
{
    public class Env : iCAPS.Env
    {
        private static bool? _icaps_server;
        private static string _kuka_api_url;
        private static IPEndPoint _sensor_modbus_tcp;
        private static string _area_name = "";
        private static string _target_name = "";
        private static WebInfo[] _favorite_web;

        public static readonly string LayoutPath = Path.Combine(Application.StartupPath, "config\\layout.ini");        // UI設定檔位址

        public static event PropertyChangedEventHandler EnvChanged;
        public static bool IsDebug
        {
            get
            {
                if (_icaps_server != null)
                    return _icaps_server.Value;
                string text = INiReader.ReadINIFile(LayoutPath, "System", "IsDebug");
                if (text == "") text = "true";
                return bool.Parse(text);
            }
        }

        public static bool ICapsServer
        {
            get
            {
                if (_icaps_server != null)
                    return _icaps_server.Value;
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "icaps_server");
                if (text == "") text = "true";
                return bool.Parse(text);
            }
            set
            {
                _icaps_server = value;
                INiReader.WriteINIFile(LayoutPath, "Control", "icaps_server", value.ToString());
            }
        }

        public static string KukaApiUrl
        {
            get
            {
                string url_txt = INiReader.ReadINIFile(LayoutPath, "Control", "kuka_api_url");
                // KukaApiController.kuka_api_server = new HttpRequest(url_txt);
                return url_txt;
            }
            set
            {
                _kuka_api_url = value;
                // KukaApiController.kuka_api_server = new HttpRequest(value);
                INiReader.WriteINIFile(LayoutPath, "Control", "kuka_api_url", value);
            }
        }

        public static IPEndPoint SensorModbusTcp        // IO 模組 Modbus TCP 連線參數
        {
            get
            {
                if (_sensor_modbus_tcp == null)
                {
                    string address = INiReader.ReadINIFile(LayoutPath, "Control", "sensor_modbus_ip");
                    string port = INiReader.ReadINIFile(LayoutPath, "Control", "sensor_modbus_port");
                    if (address != "" && port != "") _sensor_modbus_tcp = new IPEndPoint(IPAddress.Parse(address), int.Parse(port));
                }

                return _sensor_modbus_tcp;
            }
            set
            {
                _sensor_modbus_tcp = value;

                INiReader.WriteINIFile(LayoutPath, "Control", "sensor_modbus_ip", _sensor_modbus_tcp?.Address.ToString());
                INiReader.WriteINIFile(LayoutPath, "Control", "sensor_modbus_port", _sensor_modbus_tcp?.Port.ToString());
            }
        }

        public static string LocalIp
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "local_ip");
                return string.IsNullOrEmpty(text) ? null : text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "local_ip", value);
            }
        }

        public static string IcapsLinkerServerIp
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "icaps_linker_server_ip");
                return string.IsNullOrEmpty(text) ? null : text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "icaps_linker_server_ip", value);
            }
        }

        public static string IcapsLinkerServerPort
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "icaps_linker_server_port");
                return string.IsNullOrEmpty(text) ? null : text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "icaps_linker_server_port", value);
            }
        }

        public static string RecordLogTcpPort
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "record_log_tcp_port");
                return string.IsNullOrEmpty(text) ? null : text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "record_log_tcp_port", value);
            }
        }

        public static string KukaResponseUrl
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "kuka_response_url");
                return text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "kuka_response_url", value);
            }
        }

        /// <summary>
        /// 綁定區域名稱(僅為字串資訊，供下拉式選單用)
        /// </summary>
        public static string BindAreaName
        {
            get
            {
                // 首次訪問才需要讀取文件，降低文件讀寫頻率
                if (string.IsNullOrEmpty(_area_name))
                    _area_name = INiReader.ReadINIFile(LayoutPath, "Control", "bind_area_name");

                return _area_name;
            }
            set
            {
                // 如果更新資料同原始，不做任動作
                if (_area_name != value) 
                    _area_name = value;

                EnvChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(BindAreaName)));
                INiReader.WriteINIFile(LayoutPath, "Control", "bind_area_name", value);
            }
        }
        
        public static string TargetAreaName
        {
            get
            {
                // 首次訪問才需要讀取文件，降低文件讀寫頻率
                if (string.IsNullOrEmpty(_target_name))
                    _target_name = INiReader.ReadINIFile(LayoutPath, "Control", "target_area_name");

                return _target_name;
            }
            set
            {
                // 如果更新資料同原始，不做任動作
                if (_target_name != value)
                    _target_name = value;

                INiReader.WriteINIFile(LayoutPath, "Control", "target_area_name", value);
            }
        }

        public static WebInfo[] FavoriteWebs
        {
            get
            {
                // 首次訪問才需要讀取文件，降低文件讀寫頻率
                if (_favorite_web == null)
                {
                    string json_txt = INiReader.ReadINIFile(LayoutPath, "Control", "favorite_web");
                    _favorite_web = Newtonsoft.Json.JsonConvert.DeserializeObject<WebInfo[]>(json_txt);
                } 

                return _favorite_web;
            }
            set
            {
                // 如果更新資料同原始，不做任動作
                if (_favorite_web != value)
                {
                    _favorite_web = value;

                    string list_txt = Newtonsoft.Json.JsonConvert.SerializeObject(value);

                    INiReader.WriteINIFile(LayoutPath, "Control", "favorite_web", list_txt);
                }  
            }
        }

        public static string Strategy
        {
            get
            {
                return INiReader.ReadINIFile(LayoutPath, "Control", "strategy");
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "strategy", value);
            }
        }

        public static string FullAreaCount
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "full_area_count");
                return string.IsNullOrEmpty(text) ? null : text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "full_area_count", value);
            }
        }

        public static string AlertStatus
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "alert_status");
                return string.IsNullOrEmpty(text) ? "2" : text;
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "alert_status", value);
            }
        }

        public static bool isAlertEnable{get; set;}
        public static bool isAlertBtnVisible
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "alert_button_visible");
                return string.IsNullOrEmpty(text) ? false : text == "1";
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "alert_button_visible", value ? "1" : "0");
            }
        }

        public static bool isAutoStart
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "auto_start");
                return string.IsNullOrEmpty(text) ? false : text == "1";
            }
            set
            {
                INiReader.WriteINIFile(LayoutPath, "Control", "auto_start", value ? "1" : "0");
            }
        }

        public static bool isAutoStartBtnVisible
        {
            get
            {
                string text = INiReader.ReadINIFile(LayoutPath, "Control", "auto_start_button_visible");
                return string.IsNullOrEmpty(text) ? false : text == "1";
            }
        }

        public class WebInfo
        {
            public string WebName {  get; set; }
            public string Url { get; set; }

            public WebInfo(string webName, string url)
            {
                WebName = webName;
                Url = url;
            }
        }
    }
}
