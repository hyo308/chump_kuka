using CefSharp.DevTools.CSS;
using Chump_kuka.Controller;
using Chump_kuka.Controls;
using iCAPS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace Chump_kuka.Forms
{
    public partial class f02_MainMission : Form
    {
        //SidePanel1 sidePanel = new SidePanel1();
        private string _bind_area = "";
        private Timer _idle_timer;
        private bool enable_area_reset = false;     // 若為 true，允許區域狀態重設為當前 
        private bool _stay = false;     // 判斷是否停留在此頁

        private Timer _easter_egg_timer = new Timer() { Interval = 3000 };
        private string _easter_password = "";

        public f02_MainMission()
        {
            InitializeComponent();

            //Controls.Add(sidePanel);

            Load += F02_MainMission_Load;
            VisibleChanged += F02_MainMission_VisibleChanged;
        }

        private void F02_MainMission_Load(object sender, EventArgs e)
        {
            // SetupDataGridView();
            // BindAreaController.BindArea?.UserControls.Add(bind_area_control);
            // BindAreaController.UpdateControl(bind_area_control);

            // SetupDataGridView();

            ChatController.CarryTaskUpdated += ChatController_CarryTaskUpdated;
            LocalAreaController.StepChanged += LocalAreaController_StepChanged;

            // 當綁定區域更新時，同步更新控制項 UI
            KukaParm.BindChanged += KukaParm_BindChanged;
            LocalAreaController.BindControl = bind_area_control;
            bind_area_control.Model = KukaParm.BindAreaModel;
            // LocalAreaController.UpdateBindControl();

            LocalAreaController.ButtonPush += (_s, _e) => scaleButton1_Click(_s, _e);

            InitIdleTimer();        // 閒置判斷計時器
            InitTreeGridView();     // 任務列表初始化

            CarryTaskController.OnTimerAlive -= CarryTaskController_OnTimerAlive;       // 初始化

            _easter_egg_timer.Tick += _easter_egg_timer_Tick;

            LocalAreaController.InitAreaStatus();   // 初始化區域狀態

            btn_AlertEnable.Click += (_s, _e) => AlertBtnStatus();
            btn_AlertEnable.Visible = Env.isAlertBtnVisible;     // 僅泡泡模式顯示警示按鈕
        }

        private void AlertBtnStatus()
        {
            Env.isAlertEnable = !Env.isAlertEnable;
            btn_AlertEnable.BackColor = Env.isAlertEnable ? Color.LightGreen : Color.LightGray;
        }

        private void CarryTaskController_OnTimerAlive(bool obj)
        {
            throw new NotImplementedException();
        }

        private void InitTreeGridView()
        {
            treeGridView1.AutoIDVisible = false;
            // 設定 TreeGridView 資料欄
            treeGridView1.Columns = new TreeColumn[6]
            {
                new TreeColumn() { Name = "ID", Text = "ID" },
                new TreeColumn() { Name = "StartNode", Text = "起點" },
                new TreeColumn() { Name = "GoalNode", Text = "終點" },
                new TreeColumn() { Name = "CreateTime", Text = "建立日期" },
                new TreeColumn() { Name = "FinishTime", Text = "完成日期" },
                new TreeColumn() { Name = "Called", Text = "🔔" },
            };
            treeGridView1.ColumnRatios = new float[6] {0.1f, 0.15f, 0.15f, 0.24f, 0.24f, 0.12f };      // 設定 TreeGridView 資料欄寬度係數
            treeGridView1.LogColName = "LogMsg";      // 設定 TreeGridView Log 資料欄位名稱

            // 加入 DataSource
            /*treeGridView1.DataSource = new object[2] {
                new {ID="20", Start="37", Goal="組裝區", CreateDate=DateTime.Now.ToString(@"MM/dd HH:mm"), FinishDate = "06/19 17:50", Called = "N", Log = "這是一筆測試資料\n這是第二行" },
                new {ID="22", Start="48", Goal="成品區", CreateDate=DateTime.Now.ToString(@"MM/dd HH:mm"), FinishDate = "06/19 17:52", Called = "N" } };*/
        }

        private void KukaParm_BindChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                //LocalAreaController.UpdateBindControl();
                bind_area_control.Model = KukaParm.BindAreaModel;
                LocalAreaController.InitAreaStatus();   // 初始化區域狀態
            }));
        }

        private void ChatController_CarryTaskUpdated(object sender, KukaModel.SimpleCarryTask[] e)
        {
            //dataGridView1.Invoke(new Action(() => {
            //    dataGridView1.DataSource = e;
            //    InitDataGridView();
            //}));

            treeGridView1.Invoke(new Action(() =>
            {
                treeGridView1.DataSource = e;

                int running_index = e
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => x.value.RunningState == 1)?.index ?? -1;
                if (running_index > -1)
                {
                    treeGridView1.RowControls[running_index].BackColor = Color.SpringGreen;
                }
            
            }));
        }

        private void InitIdleTimer()
        {
            _idle_timer = new Timer();
            _idle_timer.Interval = 10000; // 設定間隔為 10000 毫秒 (10 秒)
            _idle_timer.Tick += (s, e) =>
            {
                Light(0);
                _idle_timer.Enabled = false;
            };
            _idle_timer.Enabled = true;
        }

        private void LocalAreaController_StepChanged(object sender, Dispatchers.HttpListenerDispatcher.HeardEventArgs e)
        {
            this.Invoke(new Action(async ()=>
            {
                try
                {
                    switch (e.Step)
                    {
                        case 0:
                            LocalAreaController.InitAreaStatus();
                            LocalAreaController.TryCreateCarryTask();      // 更新區域狀態
                            break;
                        case 1:
                            Light(2);
                            break;
                        case 2:
                            LocalAreaController.PubReady();     // 回報搬運車進站
                            Light(3);
                            break;
                        case 4:
                            Light(4);       // 搬運車出站
                            await Task.Delay(3000);
                            LocalAreaController.InitAreaStatus();
                            LocalAreaController.TryCreateCarryTask();      // 更新區域狀態
                            break;
                        case 5:
                            Light(5);                            
                            _idle_timer.Enabled = true;
                            break;
                        case 7:
                            Light(0);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }));
        }

        private void F02_MainMission_VisibleChanged(object sender, EventArgs e)
        {
            // 切換視窗時，更新區域控制項內容
            // LocalAreaController.UpdateControl();

            _stay = !Env.IsBubble && Visible;       // 若不為泡泡模式且 Visible=true 判定為停留此頁


        }
        
        private void KukaApiController_CarryTaskPub(object sender, PropertyChangedEventArgs e)
        {
            // dataGridView1.Rows.Insert(0, new object[] { dataGridView1.Rows.Count, KukaParm.StartNode.Name, KukaParm.GoalNode.Name, DateTime.Now.ToString(@"yyyy/MM/dd HH:mm") });
        }

        private async void scaleButton1_Click(object sender, EventArgs e)
        {
            // 若 enable_area_reset 為 true，重設區域狀態
            if (enable_area_reset)
            {
                LocalAreaController.InitAreaStatus();
                return;
            }

            if (_stay)        // 防止設定綁定區域時，不斷跳出錯誤訊息
            {
                await MsgBox.ShowFlash("準備按鈕已按下", "", 1000);
                Log.Append("按下綠色按鈕", "INFO", "f02");

                // 取得可搬運貨架位置
                KukaModel.Node can_carry_node = LocalAreaController.TryCreateCarryTask();

                if (can_carry_node == null)
                {
                    Log.Append("當前狀態不可搬運", "WARN", "f02");
                    return;     // 不可搬運狀態，跳過
                }

                // 透過點擊區域控制項，確認是否等待呼叫任務
                // 未點擊則等待呼叫
                bool wait_call = !bind_area_control.Checked;

                Light(1);       // 表示物料已進站
                LocalAreaController.AreaReadyFunc();

                // 建立搬運任務
                KukaModel.CarryModel start_carry_node = new KukaModel.CarryModel(can_carry_node.NodeName, KukaParm.BindAreaModel.AreaCode, can_carry_node.NodeCode);
                KukaModel.Area next_area = can_carry_node.Parent.Next();
                KukaModel.CarryModel goal_carry_node = new KukaModel.CarryModel(next_area.AreaName, next_area.AreaCode, null);

                ChatController.AppendCarryTask(start_carry_node, goal_carry_node, wait_call);
            }
        }

        /// <summary>
        /// 流程燈號
        /// </summary>
        /// <param name="index"></param>
        public void Light(int index)
        {
            DoubleImg[] list = new DoubleImg[] { led_idle, led_turtle_in, led_bot_move, led_bot_in, led_bot_out, led_task_over};
            tableLayoutPanel3.Invoke(new Action(() =>
            {
                for (int i = 0; i < list.Length; i++)
                {
                    if (i == index)
                    {
                        list[i].Change = true;
                        continue;
                    }
                    list[i].Change = false;
                }
            }));
        }
        private void scaleLabel7_Click(object sender, EventArgs e)
        {
            //sidePanel.Start = true;
        }

        #region 燈號事件
        private void led_idle_Click(object sender, EventArgs e)
        {
            // ChatController.Send("Hi");
            //LocalAreaController.TurnOffLight();
            _easter_password += "1";
            EasterEgg();
        }

        private void led_bot_in_Click(object sender, EventArgs e)
        {
            //LocalAreaController.PubReady();
        }

        private async void led_bot_out_Click(object sender, EventArgs e)
        {
            //LocalAreaController.PubRobotFunc();
            //await Task.Delay(1000);
            //LocalAreaController.PubRobotOut();
        }

        private void led_task_over_Click(object sender, EventArgs e)
        {
            // LocalAreaController.PubCarryOver();
        }

        private void led_turtle_in_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("確定發送 UDP?");
            //ChatController.SayHi();
            _easter_password += "2";
            EasterEgg();
        }

        private void led_bot_move_Click(object sender, EventArgs e)
        {
            _easter_password += "3";
            EasterEgg();
        }

        

        private void EasterEgg()
        {
            if (!_easter_egg_timer.Enabled)
            {
                _easter_egg_timer.Start();
            }
        }

        private void _easter_egg_timer_Tick(object sender, EventArgs e)
        {
            if (_easter_password == "2123")
            {
                MsgBox.ShowFlash("You find the easter egg !", "恭喜", 1500);
                bind_area_control.AllowClick = !bind_area_control.AllowClick;
                if (!bind_area_control.AllowClick)
                {
                    bind_area_control.Checked = false;
                }
            }
            _easter_password = "";
            _easter_egg_timer.Stop();       // 只執行一次
        }
        #endregion

        private void doubleImg1_Click(object sender, EventArgs e)
        {
            Timer reset_timer = new Timer();
            reset_timer.Interval = 3000;
            reset_timer.Tick += (_s, _e) =>
            {
                //MsgBox.Show("Click");
                local_reset.Change = true;
                enable_area_reset = false;
                reset_timer.Stop();
                reset_timer.Dispose();
            };
            reset_timer.Start();
            enable_area_reset = true;

        }

        private void task_list_reset_Click(object sender, EventArgs e)
        {
            ChatController.UpdateTaskList();
        }

        private void bind_area_control_ContainerClick(object sender, ControlClickEventArgs e)
        {
            Container container = e.Control as Container;
            container.Checked = !container.Checked;     // 停用預設點擊事件

            // 如果貨架已上鎖，解鎖；反之上鎖
            KukaModel.Node click_node = container.BindingModel as KukaModel.Node;

            if (click_node.NodeStatus == 1)
            {
                MsgBox.Show("選用貨架已佔用，無法上/解鎖!");
                return;
            }

            if (!click_node.IsLock)
            {
                if (click_node.RackStatus != 1)
                {
                    // 若不是空貨架無法上鎖
                    MsgBox.Show("僅能上鎖空貨架!");
                    return;
                }
            }
            click_node.IsLock = !click_node.IsLock;
            ChatController.RequestLockNode(click_node.Parent);
        }
    }
}
