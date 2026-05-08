using Chump_kuka.Controller;
using iCAPS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Chump_kuka.Controls
{
    public partial class TreeGridView : UserControl
    {

        private TreeColumn[] _columns = new TreeColumn[0];
        private float[] _column_ratios = new float[0];

        [Description("是否顯示自動ID。"), Category("自訂值")]
        public bool AutoIDVisible
        {
            get => scaleLabel1.Visible;
            set
            {
                scaleLabel1.Visible = value;
            }
        }

        [Description("加入資料。"), Category("自訂值")]
        public dynamic[] DataSource 
        { 
            get => null;
            set
            {
                if (value == null) return;

                flowLayoutPanel1.Controls.Clear();
                int mission_index = 1;      // 任務編號

                foreach (dynamic obj in value)
                {
                    if (obj == null) return;

                    Type type = obj.GetType();
                    var properties = type.GetProperties();

                    string[] prop_values = new string[Columns.Length];

                    TreeGridRow data_row = new TreeGridRow()
                    {
                        ID = mission_index++,
                        AutoIDVisible = this.AutoIDVisible,
                        ColumnRatios = _column_ratios,
                        Width = flowLayoutPanel1.Width - 6,
                        Height = panel1.Height,
                        BackColor = Color.FromArgb(224, 224, 224),
                        Margin = new Padding(0, 3, 6, 3)
                    };

                    foreach (var prop in properties)
                    {
                        string name = prop.Name;
                        object prop_value = prop.GetValue(obj);
                        //Console.WriteLine($"{name} = {prop_value}");

                        // 找到對應的欄位名稱並分配資料
                        int index = _columns
                            .Select((model, i) => new { model, i })
                            .FirstOrDefault(m => m.model.Name == name)?.i ?? -1;

                        if (index != -1)        // -1 代表找不到
                        {
                            prop_values[index] = prop_value.ToString();
                        }

                        // 若有指定 LogColName，將 Log 資訊傳入 Row
                        if (name == LogColName)
                        {
                            data_row.LogMsg = prop_value.ToString();
                        }
                    };

                    data_row.Items = prop_values.ToArray();     // 將資料傳入 Row

                    flowLayoutPanel1.Controls.Add(data_row);

                    data_row.RemoveItem += Item_RemoveItem;
                    data_row.TaskCancel += TaskCancel;
                }
                flowLayoutPanel1.AutoScrollPosition = new Point(0, flowLayoutPanel1.VerticalScroll.Maximum);     // 自動捲動到最底部
            }
        }

        [Description("加入資料欄。"), Category("自訂值")]
        public TreeColumn[] Columns
        {
            get => _columns;
            set
            {
                _columns = (TreeColumn[])value.Clone();
                if (value.Length == 0) return;
                Array.Reverse(value);
                Array.Reverse(_column_ratios);

                panel1.Controls.Clear();

                int title_length = panel1.Width - 6;        // 欄位寬度
                // 設定首行自動 ID 寬度
                int auto_id_width = scaleLabel1.Width = scaleLabel1.Visible ? (int)(title_length * 0.08) : 0;

                int index = 0;
                foreach (TreeColumn col in value)
                {
                    string content = col.Text;
                    float ratio = 1.0f / value.Length;
                    if (_column_ratios.Length > index)
                    {
                        ratio = _column_ratios[index++];
                    }

                    ScaleLabel sLabel = new ScaleLabel()
                    {
                        Text = content,
                        Font = scaleLabel1.Font,
                        ForeColor = scaleLabel1.ForeColor,
                        TextAlign = scaleLabel1.TextAlign,
                        Factor = scaleLabel1.Factor,
                        Dock = DockStyle.Left,
                        Width = (int)((title_length - auto_id_width) * ratio),
                    };
                    panel1.Controls.Add(sLabel);
                }

                Array.Reverse(_column_ratios);      // 反轉回來，避免資料錯誤

                panel1.Controls.Add(scaleLabel1);
            }
        }

        [Description("資料欄比例。"), Category("自訂值")]
        public float[] ColumnRatios
        {
            get => _column_ratios;
            set
            {
                _column_ratios = value;

                Columns = _columns;     // 更新欄位顯示

            }
        }

        [Description("Log 欄位名稱。"), Category("自訂值")]
        public string LogColName { get; set; }

        public TreeGridRow[] RowControls { get => flowLayoutPanel1.Controls.Cast<TreeGridRow>().ToArray(); }

        public TreeGridView()
        {
            InitializeComponent();

            // 隱藏下方卷軸
            flowLayoutPanel1.AutoScroll = false;
            flowLayoutPanel1.HorizontalScroll.Maximum = 0;
            flowLayoutPanel1.AutoScroll = true;
            Resize += TreeGridItem_Resize;
        }

        private void TreeGridItem_Resize(object sender, EventArgs e)
        {
            Columns = _columns;
            foreach(TreeGridRow rows in flowLayoutPanel1.Controls)
            {
                rows.Width = flowLayoutPanel1.Width - 6;
            }
        }
        private void Item_RemoveItem(object sender, EventArgs e)
        {
            TreeGridRow item = sender as TreeGridRow;
            DialogResult check = MessageBox.Show($"確認移除任務[{item.Items[0]}] =>\n 從 [{item.Items[1]}] 到 [{item.Items[2]}]", "移除任務確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (check == DialogResult.Yes)
            {
                Log.Append($"已申請刪除搬運任務[{item.Items[0].ToString()}]", "NOTICE", "TreeView");
                ChatController.DelTask(item.Items[0].ToString());
                
                //item.Dispose();
            }
        }

        private void TaskCancel(object sender, EventArgs e)
        {
            TreeGridRow item = sender as TreeGridRow;
            DialogResult check = MessageBox.Show($"確認強制取消任務[{item.Items[0]}] =>\n 從 [{item.Items[1]}] 到 [{item.Items[2]}]", "強制取消任務確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (check == DialogResult.Yes)
            {
                item.Dispose();

                ChatController.CancelTask(item.Items[0].ToString());
            }
        }
    }

    public class TreeColumn
    {
        public string Name {  get; set; }
        public string Text { get; set; }
    }
}
