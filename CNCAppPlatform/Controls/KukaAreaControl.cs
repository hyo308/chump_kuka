using Chump_kuka.Controller;
using CookComputing.XmlRpc;
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

namespace Chump_kuka.Controls
{
    internal partial class KukaAreaControl : UserControl
    {
        private KukaModel.Area _model = null;
        private KukaModel.Node[] _nodes = new KukaModel.Node[] { };
        private bool _checked = false;
        private int[] _node_status = null;
        private bool _allow_click = true;
        private Color[] _container_colors = new Color[2] { Color.CadetBlue, Color.Orange};

        // 定義事件，使用自定義參數
        public event EventHandler<ControlClickEventArgs> ContainerClick;
        public event EventHandler<ControlClickEventArgs> AreaClick;

        public Image[] ContainerImgs;
        public string Type { get { return "NODE_AREA"; } }
        public string AreaCode = "";

        public KukaModel.Area Model 
        { 
            get => _model;
            set
            {
                if (value != null)
                {
                    if (_model != null) _model.PropertyChanged -= _model_PropertyChanged;       // 解除既有綁定事件
                    _model = value;     // 重新指定模型

                    AreaName = _model.AreaName;
                    AreaCode = _model.AreaCode;
                    _nodes = new KukaModel.Node[] { }; ;
                    AreaNode = _model.NodeList;
                    
                    _model.PropertyChanged += _model_PropertyChanged;       // 綁定新事件
                }
            } 
        }

        [Description("區域名稱。"), Category("自訂值")]
        public string AreaName
        {
            get => label1.Text; 
            set 
            { 
                if (Model?.AreaName != value) Model?.Rename(value); 
                label1.Text = value;
            }
        }

        [Description("區域中是否可點擊。"), Category("自訂值")]
        public bool AllowClick 
        { 
            get => _allow_click;
            set
            {
                _allow_click = value;
                if (_allow_click)
                {
                    tableLayoutPanel1.Cursor = containerPanel.Cursor = label1.Cursor = Cursors.Hand;
                    tableLayoutPanel1.Click += flowLayoutPanel1_Click;
                    containerPanel.Click += flowLayoutPanel1_Click;
                    label1.Click += flowLayoutPanel1_Click;
                }
                else
                {
                    tableLayoutPanel1.Cursor = containerPanel.Cursor = label1.Cursor = Cursors.Default;
                    tableLayoutPanel1.Click -= flowLayoutPanel1_Click;
                    containerPanel.Click -= flowLayoutPanel1_Click;
                    label1.Click -= flowLayoutPanel1_Click;
                }
            } 
        }

        [Description("區域中的節點是否可點擊。"), Category("自訂值")]
        public bool AllowContainerClick{ get; set; } = true;

        [Description("區域中的節點是否顯示鎖圖示。"), Category("自訂值")]
        public bool AllowContainerLock { get; set; } = false;

        [Description("區域中的節點。"), Category("自訂值")]
        public KukaModel.Node[] AreaNode
        {
            get => _nodes;
            set
            {
                if (value == null) return;
                if (_nodes.SequenceEqual(value)) return;        // 如果資訊未更新，不處理
                containerPanel.Controls.Clear();
                _nodes = value;
                foreach (KukaModel.Node node in _nodes)
                {
                    Container container = new Container()
                    {
                        ContainerName = node.NodeName,
                        Size = container1.Size,
                        Enabled = AllowContainerClick,
                        BindingModel = node
                    };

                    // 更新貨架狀態圖片
                    UpdateSingleContainerImage(container, node.RackStatus);        
                    container.ImgColor = _container_colors[Math.Max(node.NodeStatus, 0)];
                    // 若 model.NodeStatus == 1，表示節點已上鎖
                    container.ShowLock = node.IsLock;
                    update_time.Text = $"更新時間：{DateTime.Now.ToString("HH:mm:ss")}";

                    node.PropertyChanged += (sender, e) =>
                    {
                        KukaModel.Node model = (sender as KukaModel.Node);
                        if (container == null || container.IsDisposed || !container.IsHandleCreated) return;

                        try
                        {
                            container.Invoke(new Action(() =>
                            {
                                if (container.IsDisposed) return;
                                switch (e.PropertyName)
                                {
                                    // 貨架狀態
                                    case nameof(KukaModel.Node.RackStatus):
                                        UpdateSingleContainerImage(container, model.RackStatus);        // 更新貨架狀態圖片
                                        break;
                                    // 節點狀態
                                    case nameof(KukaModel.Node.NodeStatus):
                                        container.ImgColor = _container_colors[Math.Max(node.NodeStatus, 0)];
                                        break;
                                    // 上鎖狀態
                                    case nameof(KukaModel.Node.IsLock):
                                        container.ShowLock = model.IsLock;
                                        break;
                                }

                                update_time.Text = $"更新時間：{DateTime.Now.ToString("HH:mm:ss")}";
                            }));
                        }
                        catch (ObjectDisposedException) { }
                        catch (InvalidOperationException) { }
                    };

                    // container.ImageIndex = -1;
                    //UpdateSingleContainerImage(container, _nodes[i].RackStatus);
                    container.ContainerClick += Container_ContainerClick;
                    containerPanel.Controls.Add(container);

                    if (AllowContainerLock)
                    {
                        container.ShowLock = true;
                    }
                }
                // _node_status = new int[_nodes.Length];
            }
        }

        [Description("表示元件是否為已核取狀態。"), Category("自訂值")]
        public bool Checked
        {
            get => _checked; 
            set
            {
                // 當控制項被點擊後，點亮外框
                _checked = value;
                switch (_checked)
                {
                    case true:
                        custom_border.BackColor = Color.MediumSpringGreen;
                        break;
                    case false:
                        custom_border.BackColor = SystemColors.ControlLight;
                        break;
                }
            }
        }

        public KukaAreaControl()
        {
            InitializeComponent();
            ContainerImgs = new Image[3] { null, doubleImg1.Image, doubleImg1.SubImg };

            Controls.Remove(samplePanel);
            custom_border.Dock = DockStyle.Fill;
            SizeChanged += Kuka_area_SizeChanged;
        }

        private void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            KukaModel.Area model = (sender as KukaModel.Area);
            switch (e.PropertyName)
            {
                // 區域名稱
                case nameof(KukaModel.Area.AreaName):
                    AreaName = model.AreaName;
                    break;
                // 節點內容
                case nameof(KukaModel.Area.NodeList):
                    AreaNode = model.NodeList;
                    break;
            }
        }

        public void UpdateSingleContainerImage(Container container, int container_status)
        {
            int i = 0;

            try
            {
                switch (container_status)
                {
                    case 0:
                        // 無交換站
                        container.ContainerImage = null;
                        break;
                    case 1:
                        // 有交換站 & 無料
                        container.ContainerImage = doubleImg1.Image;
                        break;
                    case 2:
                        // 有交換站 & 有料
                        container.ContainerImage = doubleImg1.SubImg;
                        break;
                }
                
            }
            catch
            {
                // 輸入 container_status 數量少於區域內的容器
            }
            
        }

        /// <summary>
        /// 更新容器圖片
        /// </summary>
        /// <param name="container_texts"></param>
        public void UpdateContainerText(string[] container_texts)
        {
            int i = 0;
            foreach (Container _container in containerPanel.Controls)
            {
                _container.Text = container_texts[i++];
            }
        }

        private void Container_ContainerClick(object sender, ControlClickEventArgs e)
        {
            ContainerClick?.Invoke(this, e);
        }

        public void ResetContainer()
        {
            foreach(Container rack in containerPanel.Controls)
            {
                rack.Checked = false;
            }
        }

        private void Kuka_area_SizeChanged(object sender, EventArgs e)
        {
            container1.Height = (int)(containerPanel.Height * 0.47);
            container1.Width = (int)(container1.Height * 0.6);

            foreach (Container container in containerPanel.Controls)
            {
                container.Size = container1.Size;
            }
        }

        private void flowLayoutPanel1_Click(object sender, EventArgs e)
        {
            if (!AllowClick) return;
            Checked = !Checked;
            // 觸發事件，並傳遞按鈕資訊
            AreaClick?.Invoke(this, new ControlClickEventArgs(Name, this));

        }

    }
}
