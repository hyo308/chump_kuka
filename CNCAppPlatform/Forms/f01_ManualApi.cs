using CefSharp.DevTools.CSS;
using Chump_kuka.Controller;
using Chump_kuka.Controls;
using iCAPS;
using Newtonsoft.Json;
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
using System.Web.Management;
using System.Windows.Forms;

namespace Chump_kuka.Forms
{
    public partial class f01_ManualApi : Form
    {
        private KukaModel.CarryModel _start_carry = null;
        private KukaModel.CarryModel _goal_carry = null;
        private KukaModel.CarryModel StartCarry 
        { 
            get => _start_carry;
            set
            {
                _start_carry = value;
                selected_1.Text = _start_carry == null ? "null" : _start_carry.Name;
            } 
        }
        private KukaModel.CarryModel GoalCarry
        {
            get => _goal_carry;
            set
            {
                _goal_carry= value;
                selected_2.Text = _goal_carry == null ? "null" : _goal_carry.Name;
            }
        }
        public f01_ManualApi()
        {
            InitializeComponent();

            Load += F01_ManualApi_Load;

            VisibleChanged += F01_ManualApi_VisibleChanged;
        }

        private void F01_ManualApi_Load(object sender, EventArgs e)
        {
            if(KukaParm.GetAreaArray().Length > 0)
            {
                KukaParm_AreaChanged(this, null);
            }
            
            // 避免重複呼叫導致過度綁定
            KukaParm.AreaChanged -= KukaParm_AreaChanged;
            KukaParm.AreaChanged += KukaParm_AreaChanged;
            //KukaParm_AreaChanged(null, null);
            //KukaParm.CarryChanged -= KukaParm_CarryChanged;
            //KukaParm.CarryChanged += KukaParm_CarryChanged;
            //KukaParm_CarryChanged(null, null);
        }

        private void F01_ManualApi_VisibleChanged(object sender, EventArgs e)
        {
            // 防止表單因為提早開啟導致顯示錯誤
            if (tableLayoutPanel2.Controls.Count == 0 && Visible)
            {
                F01_ManualApi_Load(null, null);
            }

            if (Visible)
            {
                SyncAreasAndRefresh();
            }

            StartCarry = null;
            GoalCarry = null;

            foreach (KukaAreaControl area_ctrl in tableLayoutPanel2.Controls)
            {
                area_ctrl.Checked = false;
                area_ctrl.ResetContainer();
            }
        }

        public void SyncAreasAndRefresh()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                this.Invoke(new Action(() =>
                {
                    if (this.IsDisposed) return;

                    var areas = KukaParm.GetAreaArray();
                    if (areas.Length == 0) return;

                    KukaAreaControl[] areaControls = new KukaAreaControl[] { kuka_area1, kuka_area2, kuka_area3 };
                    for (int i = 0; i < areaControls.Length; i++)
                    {
                        if (areaControls[i] == null) continue;

                        KukaModel.Area model = KukaParm.GetAreaModelByIndex(i);
                        if (model != null)
                        {
                            if (areaControls[i].Model != model)
                            {
                                areaControls[i].Model = model;
                            }
                            areaControls[i].RefreshUI();
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncAreasAndRefresh error: {ex.Message}");
            }
        }

        private void KukaParm_AreaChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            SyncAreasAndRefresh();
        }

        private void Area_AreaClick(object sender, ControlClickEventArgs e)
        {
            KukaAreaControl area = e.Control as KukaAreaControl;

            if (area.Checked)
            {
                
                if (StartCarry == null)     //if (selected_1.Tag == null)
                {
                    //selected_1.Tag = area;
                    //selected_1.Text = area.AreaName;

                    // Test
                    StartCarry = new KukaModel.CarryModel(area.AreaName, area.AreaCode, null);
                }
                else if (GoalCarry == null)        // else if (selected_2.Tag == null)
                {
                    //selected_2.Tag = area;
                    //selected_2.Text = area.AreaName;

                    // Test
                    GoalCarry = new KukaModel.CarryModel(area.AreaName, area.AreaCode, null);
                }
                else
                {
                    area.Checked = false;
                    MsgBox.Show("已選擇2個節點");
                }
            }
            else
            {
                // 如果觸發點擊的容器被取消選取，
                // 判斷該容器是否存在於 selected_1 或 selected_2 的 Tag中，
                // 若存在，清空該 selected_1 或 selected_2
                //new List<Label> { selected_1, selected_2 }
                //.Where(c => c.Tag == area)
                //.ToList()
                //.ForEach(c =>
                //{
                //    c.Tag = null;
                //    c.Text = "null";
                //});
                if (StartCarry != null && StartCarry.Name == area.AreaName)
                {
                    StartCarry = null; // 把 StartNode 設為 null
                }

                else if (GoalCarry != null && GoalCarry.Name == area.AreaName)
                {
                    GoalCarry = null; // 把 GoalNode 設為 null
                }
            }
        }

        private void Kuka_area1_ContainerClick(object sender, ControlClickEventArgs e)
        {
            Container container = e.Control as Container;

            if (container.Checked)
            {
                KukaModel.Node click_node = container.BindingModel;
                string stay_area_code = click_node.Parent.AreaCode;
                if (StartCarry == null)     //if (selected_1.Tag == null)
                {
                    //selected_1.Tag = container;
                    //selected_1.Text = container.ContainerName;

                    // Test
                    StartCarry = new KukaModel.CarryModel(container.ContainerName, stay_area_code, click_node.NodeCode);
                }
                else if (GoalCarry == null)     //else if (selected_2.Tag == null)
                {
                    //selected_2.Tag = container;
                    //selected_2.Text = container.ContainerName;

                    // Test
                    GoalCarry = new KukaModel.CarryModel(container.ContainerName, stay_area_code, click_node.NodeCode);
                }
                else
                {
                    container.Checked = false;
                    MsgBox.Show("已選擇2個節點");
                }
            }
            else
            {
                // 如果觸發點擊的容器被取消選取，
                // 判斷該容器是否存在於 selected_1 或 selected_2 的 Tag中，
                // 若存在，清空該 selected_1 或 selected_2
                //new List<Label> { selected_1, selected_2 }
                //.Where(c => c.Tag == container)
                //.ToList()
                //.ForEach(c =>
                //{
                //    c.Tag = null;
                //    c.Text = "null";
                //});
                if (StartCarry != null && StartCarry.Name == container.ContainerName)
                {
                    StartCarry = null; // 把 StartNode 設為 null
                }

                else if (GoalCarry != null && GoalCarry.Name == container.ContainerName)
                {
                    GoalCarry = null; // 把 GoalNode 設為 null
                }
            }
        }

       

        private async void btn_send_carry_Click(object sender, EventArgs e)
        {
            //if (!KukaApiController.Enable)
            //{
            //    MsgBox.Show("尚未開啟 kuka api");
            //    return;
            //}

            if (_start_carry == null || _goal_carry == null)
            {
                MsgBox.Show("派車前，必須先選取兩節點。");
                return;
            }

            //long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //List<string> positions = new List<string>() { GetTagCode(selected_1.Tag), GetTagCode(selected_2.Tag) };
            //List<string> types = new List<string>() { GetTagType(selected_1.Tag), GetTagType(selected_2.Tag) };
            //List<string> texts = new List<string>() { selected_1.Text, selected_2.Text };

            if (go_direction.Change)
            {
                //positions.Reverse();
                //types.Reverse();
                //texts.Reverse();
                //(_start_carry, _goal_carry) = (_goal_carry, _start_carry);
                (StartCarry, GoalCarry) = (GoalCarry, StartCarry);
                go_direction.Change = !go_direction.Change;
            }

            //DialogResult dialogResult = MessageBox.Show($"是否執行派車任務?\n{texts[0]} -> {texts[1]}", "info", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            DialogResult dialogResult = MessageBox.Show($"是否執行派車任務?\n{_start_carry.Name} -> {_goal_carry.Name}", "info", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                // KukaApiController.PubCarryTask();
                await MsgBox.ShowFlash("加入等候任務列表...", "手動派車", 1000);
                ChatController.AppendCarryTask(_start_carry, _goal_carry, false);
            }

            

            //var body = new
            //{
            //    orgId = "chump",     //"9001",
            //    requestId = $"request{timestamp}",
            //    missionCode = $"mission{timestamp}",
            //    missionType = "RACK_MOVE",
            //    viewBoardType = "",
            //    robotType = "LIFT",
            //    robotModels = new string[] { },
            //    robotIds = new string[] { },
            //    priority = 1,
            //    containerType = "",
            //    containerCode = "",
            //    templateCode = "",
            //    lockRobotAfterFinish = false,
            //    unlockRobotId = "",
            //    unlockMissionCode = "",
            //    idleNode = "",
            //    missionData = new[]
            //    {
            //        new
            //        {
            //            sequence = 1,
            //            position = positions[0],     //"A000000002",
            //            type = types[0],     // "NODE_AREA",
            //            putDown = false,
            //            passStrategy = "AUTO",
            //            waitingMillis = 0
            //        },
            //        new
            //        {
            //            sequence = 2,
            //            position = positions[1],     //"A000000003",
            //            type = types[1],     // "NODE_AREA",
            //            putDown = true,
            //            passStrategy = "AUTO",
            //            waitingMillis = 0
            //        }
            //    }
            //};

            //int response_code = await Env.kuka_api.PostRequest("submitMission", body);       // 等待 api 回應
            //if (response_code != 200) return;

            //string responseBody = Env.kuka_api.ResponseMessage;

            //JObject resp_json = JObject.Parse(responseBody);
            //if (!(bool)resp_json["success"])
            //{
            //    MsgBox.Show($"訪問 /submitMission 時發生異常 [{(string)resp_json["code"]}] {(string)resp_json["message"]}");
            //    return;
            //}
            
        }

        private void scaleLabel2_Click(object sender, EventArgs e)
        {
            SyncAreasAndRefresh();

            Form json_form = new Form();
            RichTextBox richTextBox = new RichTextBox() { Dock = DockStyle.Fill };
            json_form.Controls.Add(richTextBox);
            string areas_json = JsonConvert.SerializeObject(KukaParm.GetAreaArray(), Formatting.Indented);
            richTextBox.Text = areas_json;
            json_form.Show();
        }
    }
}
