using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Chump_kuka.Controls
{
    public partial class LogWindow : UserControl
    {
        private List<string> _log_labels = new List<string>() {"INFO", "WARN", "ERROR", "NOTICE", "ALERT" };
        private string _select_label = "";
        private bool _isAutoScroll = true;

        // 拖曳用
        bool dragging = false;
        int dragStartY;

        public LogWindow()
        {
            InitializeComponent();

            Log.UiContext = SynchronizationContext.Current;

            logView.SetDataSource(Log.LogData);
            
            // 檢索所有 Log 訊息，是否存在未登錄標籤
            foreach (Models.LogMsg log in Log.LogData)
            {
                CreateLabel(log.Status);
            }

            Log.LogData.ListChanged += LogData_ListChanged;     // Log 資料變更事件

            checkBox_AutoScroll.Checked = _isAutoScroll; // 預設自動滾動開啟
        }

        private void LogData_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 若有新增資料，判定是否為新的狀態標籤
            // 若發現新狀態標籤，新增篩選按鈕
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                // 用 Index 取出新增的物件
                CreateLabel(Log.LogData[e.NewIndex].Status);        // 判定&建立篩選按鈕
            }

            if (_isAutoScroll) { logView.FirstDisplayedScrollingRowIndex = logView.Rows.Count - 1; }
        }

        private void CreateLabel(string log_label)
        {
            // 將第一次出現的 Log 標籤加入列表中
            if (!_log_labels.Contains(log_label))
            {
                _log_labels.Add(log_label);

                RadioButton new_label = new RadioButton()
                {
                    Appearance = Appearance.Button,
                    Size = checkBox1.Size,
                    Text = log_label,
                    Font = checkBox1.Font,
                    TextAlign = checkBox1.TextAlign,
                    FlatStyle = FlatStyle.Flat,
                    Margin = checkBox1.Margin,
                };
                new_label.Click += checkBox_Click;
                flowLayoutPanel1.Controls.Add(new_label);
            }
        }

        private void checkBox_Click(object sender, EventArgs e)
        {
            // 透過點擊標籤篩選對應資料
            RadioButton select_label = (sender as RadioButton);
            
            // 如果重複點擊，清除選擇，恢復所有資料
            if (select_label.Text == _select_label)
            {
                _select_label = "";
                select_label.Checked = false;
                logView.SetDataSource(Log.LogData);
            }
            else
            {
                _select_label = select_label.Text;
                Log.FilterStatus(_select_label);
                logView.SetDataSource(Log.FilterData);
            }
        }

        #region 拖曳伸長功能
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragStartY = Cursor.Position.Y;

            MaximumSize = new Size(Width, Parent.Height - 20);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                int delta = Cursor.Position.Y - dragStartY;
                this.Height += delta;
                //logView.Height += delta;
                dragStartY += delta;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        #endregion

        private void checkBox_AutoScroll_Click(object sender, EventArgs e)
        {
            if (_isAutoScroll) { _isAutoScroll = false; checkBox_AutoScroll.Checked = false; }
            else { _isAutoScroll = true; checkBox_AutoScroll.Checked = true; }
        }
    }
}
