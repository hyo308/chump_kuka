using CookComputing.XmlRpc;
using iCAPS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using static Chump_kuka.KukaModel;


namespace Chump_kuka.Controls
{
    public partial class KukaRobotStatus : UserControl
    {
        // 回應字串範例
        private static JArray robot_infos = (JArray)JObject.Parse($@"{{""code"": ""0"", 
                                                                     ""message"": """", 
                                                                      ""success"": true, 
                                                                      ""data"":[{{ 
                                                                            ""robotId"":""robot001"", 
                                                                            ""robotType"":""test001"", 
                                                                            ""containerCode"" : ""container001"", 
                                                                            ""mapCode"" : ""map01"", 
                                                                            ""floorNumber"" : ""floor01"", 
                                                                            ""buildingCode"" : ""9001"", 
                                                                            ""status"" : 4, 
                                                                            ""occupyStatus"" : 1, 
                                                                            ""batteryLevel"" : 60, 
                                                                            ""nodeCode"" : ""node001"", 
                                                                            ""missionCode"" : ""missionCode111"", 
                                                                            ""updateTime"" : ""{DateTime.Now.ToString(@"g")}""
                                                                           }},
                                                                           {{ 
                                                                            ""robotId"":""robot002"", 
                                                                            ""robotType"":""test002"", 
                                                                            ""containerCode"" : ""container002"", 
                                                                            ""mapCode"" : ""map02"", 
                                                                            ""floorNumber"" : ""floor02"", 
                                                                            ""buildingCode"" : ""9002"", 
                                                                            ""status"" : 4, 
                                                                            ""occupyStatus"" : 1, 
                                                                            ""batteryLevel"" : 60, 
                                                                            ""nodeCode"" : ""node001"", 
                                                                            ""missionCode"" : ""missionCode111"", 
                                                                            ""updateTime"" : ""{DateTime.Now.ToString(@"g")}""
                                                                           }}] 
                                                                    }} ")["data"];

        public KukaRobotStatus()
        {
            InitializeComponent();

            Load += KukaRobotStatus_Load;
            Resize += KukaRobotStatus_Resize;       // 自動調整分頁字體大小

            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;       // 切換分頁時，顯示內容
            
            
            // KukaApiController.GetRobotStatus();     // 加入機器人資訊訪問
        }

        private void KukaParm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                this.Invoke(new Action(() =>
                {
                    if (this.IsDisposed) return;
                    InfoUPdate();
                }));
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// 依據分頁更新表格內容
        /// </summary>
        private void InfoUPdate()
        {
            if (KukaParm.RobotStatusInfos == null || KukaParm.RobotStatusInfos.Length == 0)
            {
                tabControl1.Controls.Clear();
                tabControl1.Controls.Add(tabPage2);
                return; 
            }

            // 將搜尋到的機器人名稱整理成 list
            //List<string> robotIds = KukaParm.RobotStatusInfos
            //                      .Select(item => item["robotId"]?.ToString())
            //                      .Where(id => !string.IsNullOrEmpty(id))
            //                      .ToList();
            List<string> robotIds = KukaParm.RobotStatusInfos
                                  .Select(item => item.RobotId?.ToString())
                                  .Where(id => !string.IsNullOrEmpty(id))
                                  .ToList();

            // 刪除不存在於 List 中的 TabPage
            tabControl1.TabPages
                .Cast<TabPage>()
                .Where(tab => !robotIds.Contains(tab.Text))
                .ToList()
                .ForEach(tab => tabControl1.TabPages.Remove(tab));
            
            if (tabControl1 == null || tabControl1.IsDisposed || !tabControl1.IsHandleCreated) return;
            try
            {
                tabControl1.Invoke(new Action(() =>
                {
                    if (tabControl1.IsDisposed) return;
                    // 新增不存在於 TabControl 中的 TabPage
                robotIds
                    .Where(tabName => !tabControl1.TabPages.Cast<TabPage>().Any(tab => tab.Text == tabName))
                    .ToList()
                    .ForEach(tabName => tabControl1.TabPages.Add(new TabPage(tabName) { BackColor = Color.White, Padding = new Padding(15) }));

                // 將按鈕的 Parent 設置為當前 TabPage
                tableLayoutPanel1.Parent = tabControl1.SelectedTab;

                // 因為 api 返回順序不固定，因此需要有一個尋找順序的機制
                for (int current_index = 0; current_index < robotIds.Count; current_index++)
                {
                    //if (robotIds[current_index] == tabControl1.SelectedTab.Text)
                    //{
                    //    robot_id.Text = (string)KukaParm.RobotStatusInfos[current_index]["robotId"];
                    //    robot_type.Text = (string)KukaParm.RobotStatusInfos[current_index]["robotType"];
                    //    container_code.Text = (string)KukaParm.RobotStatusInfos[current_index]["containerCode"];
                    //    map_code.Text = (string)KukaParm.RobotStatusInfos[current_index]["mapCode"];
                    //    Dictionary<string, string> status_dict = new Dictionary<string, string>() { { "1", "離場" }, { "2", "離線" }, { "3", "空閒" }, { "4", "任務中" }, { "5", "充電中" }, { "6", "更新中" }, { "7", "異常" } };
                    //    status.Text = status_dict[(string)KukaParm.RobotStatusInfos[current_index]["status"]];
                    //    Dictionary<string, string> occupy_dict = new Dictionary<string, string>() { { "0", "未占用" }, { "1", "占用中" } };
                    //    occupy_status.Text = occupy_dict[(string)KukaParm.RobotStatusInfos[current_index]["occupyStatus"]];
                    //    battery_level.Text = (string)KukaParm.RobotStatusInfos[current_index]["batteryLevel"];
                    //    node_code.Text = (string)KukaParm.RobotStatusInfos[current_index]["nodeCode"];
                    //    update_time.Text = (string)KukaParm.RobotStatusInfos[current_index]["updateTime"];
                    //    return;
                    //}
                    if (robotIds[current_index] == tabControl1.SelectedTab.Text)
                    {
                        robot_id.Text = (string)KukaParm.RobotStatusInfos[current_index].RobotId;
                        robot_type.Text = (string)KukaParm.RobotStatusInfos[current_index].RobotType;
                        container_code.Text = (string)KukaParm.RobotStatusInfos[current_index].ContainerCode;
                        mission_code.Text = (string)KukaParm.RobotStatusInfos[current_index].MissionCode;
                        Dictionary<RobotStatus, string> status_dict = new Dictionary<RobotStatus, string>() {
                            { RobotStatus.Leaving, "離場" },
                            { RobotStatus.Offline, "離線" },
                            { RobotStatus.Idle, "空閒" },
                            { RobotStatus.Working, "任務中" },
                            { RobotStatus.Charging, "充電中" },
                            { RobotStatus.Updating, "更新中" },
                            { RobotStatus.Error, "異常" } };
                        status.Text = status_dict[KukaParm.RobotStatusInfos[current_index].Status];
                        Dictionary<int, string> occupy_dict = new Dictionary<int, string>() { { 0, "未占用" }, { 1, "占用中" } };
                        occupy_status.Text = occupy_dict[KukaParm.RobotStatusInfos[current_index].OccupyStatus];
                        battery_level.Text = KukaParm.RobotStatusInfos[current_index].BatteryLevel.ToString();
                        node_code.Text = KukaParm.RobotStatusInfos[current_index].NodeCode;
                        update_time.Text = KukaParm.RobotStatusInfos[current_index].ReceiveTime;
                        return;
                    }
                }
            }));
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            

            
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 取得當前選中的 TabPage
            TabPage currentTab = tabControl1.SelectedTab;

            if (currentTab != null)
            {
                // 將按鈕的 Parent 設置為當前 TabPage
                tableLayoutPanel1.Parent = currentTab;
                InfoUPdate();
            }
        }

        private void KukaRobotStatus_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;     // 若在設計階段，不執行以下內容。( 需在 Load 事件中才有用)

            tabControl1.Controls.RemoveAt(0);
            KukaParm.RobotStatusChanged += KukaParm_PropertyChanged;       // 自動更新機器人資訊
        }

        private void KukaRobotStatus_Resize(object sender, EventArgs e)
        {
            tabControl1.Font = robot_id.Font;
        }

    }
}
