/*
 * CustomDataGridView - 可擴展的 DataGridView，支援 CSV 資料存取
 * 
 * 功能介紹：
 * 1. 透過 List<string> 初始化欄位。
 * 2. 可動態新增資料列 (AddRow)。
 * 3. 可修改特定儲存格內容 (UpdateCell)。
 * 4. 支援 CSV 檔案的自動儲存與讀取。
 * 5. 透過 AutoSave 屬性決定是否自動儲存變更。
 */

using CefSharp.DevTools.CSS;
using Chump_kuka;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Forms;

public class CustomDataGridView : DataGridView
{
    private bool autoSave = false; // 是否自動儲存 CSV
    private string csvFilePath = "data.csv"; // 預設 CSV 檔案名稱
    // private string[] columns = new string[] {};
    private float[] columnWidthRatios = new float[] { }; // 欄位寬度比例
    private dynamic _col_type;      // 定義資料類型
    private dynamic _rowData;       // 存放列資料 BindingList<資料類型>
    //private BindingSource bs = new BindingSource();

    [Description("資料存放 CSV 位置"), Category("自訂值")]
    public string CsvFilePath
    {
        get => csvFilePath;
        set => csvFilePath = value;
    }

    [Description("是否在資料變更時自動儲存至 CSV"), Category("自訂值")]
    public bool AutoSave
    {
        get => autoSave;
        set => autoSave = value;
    }

    //[Description("自定義欄位列表"), Category("自訂值")]
    //public string[] UserColumns
    //{
    //    get => columns;
    //    set
    //    {
    //        columns = value;
    //        InitializeColumns(columns);
    //    }
    //}
    //[Description("自定義欄位列表"), Category("自訂值")]
    //public dynamic UserColumns
    //{
    //    get => _col_type;
    //    set
    //    {
    //        if (value == null) return;

    //        _col_type = value;
    //        var listType = typeof(BindingList<>).MakeGenericType(_col_type);
    //        _rowData = Activator.CreateInstance(listType);
           
    //        bs.DataSource = _rowData; // 可以是 List<T> 或 BindingList<T>

    //        InitializeColumns(_col_type);
    //    }
    //}

    [Description("設定或取得欄位寬度比例。\n每個數值代表對應欄位所佔的相對寬度。"), Category("自訂值")]
    public float[] UserColumnWidthRatios
    {
        get => columnWidthRatios;
        set
        {
            //if (value.Length != columns.Length)
            //{
            //    MessageBox.Show("設定值與欄位數量不符合");
            //    return;
            //}
            columnWidthRatios = value;
            AdjustColumnWidths();
        }
    }

    /// <summary>
    /// 初始化 CustomDataGridView，並建立指定的欄位。
    /// </summary>
    /// <param name="columns">欄位名稱列表</param>
    public CustomDataGridView()
    {
        AllowUserToAddRows = false;
        // InitializeColumns(columns);
        Resize += CustomDataGridView_Resize;
        CellClick += CustomDataGridView_CellClick;

        AutoGenerateColumns = false;
    }

    /// <summary>
    /// 點擊表格後，複製格位內容文字
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CustomDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
    {

        // 如果點到標題列（header），就不處理
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        var value = Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
        if (!string.IsNullOrEmpty(value))
        {
            Clipboard.SetText(value);
            MessageBox.Show($"已複製: {value}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
    }

    private void CustomDataGridView_Resize(object sender, EventArgs e)
    {
        AdjustColumnWidths();
    }

    public void SetDataSource<T>(BindingList<T> data_source)
    {
        Columns.Clear();


        if (data_source == null) return;

        //Type type = data_source.GetType();
        var properties = typeof(T).GetProperties();

        foreach (var col in properties)
        {
            //Columns.Add(col, col);
            Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = col.Name,
                DataPropertyName = col.Name
            });

        }
        DataSource = data_source;

        AdjustColumnWidths();
    }
 

    /// <summary>
    /// 初始化欄位。
    /// </summary>
    /// <param name="columns">欄位名稱列表</param>
    private void InitializeColumns(dynamic columns)
    {
        Columns.Clear();


        if (columns == null) return;

        //Type type = columns.GetType();
        var properties = columns.GetProperties();
        

        foreach (var col in properties)
        {
            //Columns.Add(col, col);
            Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = col.Name,
                DataPropertyName = col.Name // 對應 string[1]
            });
            
        }
        //DataSource = bs;
    }

    /// <summary>
    /// 調整欄位寬度，使其符合設定的比例。
    /// </summary>
    private void AdjustColumnWidths()
    {
        if (Columns.Count == 0 || columnWidthRatios.Length < Columns.Count) return;

        int totalWidth = ClientSize.Width;
        for (int i = 0; i < Columns.Count; i++)
        {
            Columns[i].Width = (int)(totalWidth * columnWidthRatios[i]);
        }
    }

    /// <summary>
    /// 新增一行資料。
    /// </summary>
    /// <param name="rowData">該行的資料內容</param>
    public void AddRow(dynamic rowData)
    {
        // if (rowData.Length != ColumnCount) return; // 確保欄位數量相符
        
        if (rowData.GetType() == _col_type)
        {
            _rowData.Add(rowData);
        }
        // Rows.Add(rowData.ToArray());
        
        if (AutoSave) SaveToCsv();
    }

    /// <summary>
    /// 修改指定儲存格的內容。
    /// </summary>
    /// <param name="rowIndex">行索引</param>
    /// <param name="colIndex">列索引</param>
    /// <param name="newValue">新值</param>
    public void UpdateCell(int rowIndex, int colIndex, string newValue)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count ||
            colIndex < 0 || colIndex >= ColumnCount) return;

        Rows[rowIndex].Cells[colIndex].Value = newValue;
        if (AutoSave) SaveToCsv();
    }

    /// <summary>
    /// 將目前的資料儲存至 CSV。
    /// </summary>
    public void SaveToCsv()
    {
        int retries = 3; // 最多重試 3 次
        int delay = 500; // 每次等待 500 毫秒

        for (int i = 0; i < retries; i++)
        {
            try
            {
                // 使用 FileStream 與 FileShare.ReadWrite 避免其他程式(如Excel)開啟時導致的寫入鎖定
                using (var fs = new FileStream(csvFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    // 儲存標題列
                    writer.WriteLine(string.Join(",", Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText)));

                    // 儲存資料列
                    foreach (DataGridViewRow row in Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            writer.WriteLine(string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString() ?? "")));
                        }
                    }
                }
                break; // 成功寫入就跳出迴圈
            }
            catch (IOException)
            {
                if (i == retries - 1)
                    throw; // 如果最後一次還是失敗，就拋出例外

                System.Threading.Thread.Sleep(delay); // 等待後重試
            }
        }
    }

    /// <summary>
    /// 從 CSV 讀取資料，並載入至表格。
    /// </summary>
    public void LoadFromCsv()
    {
        //if (!File.Exists(csvFilePath)) return;

        //using (StreamReader reader = new StreamReader(csvFilePath))
        //{
        //    var headers = reader.ReadLine()?.Split(',');
        //    if (headers == null) return;

        //    //InitializeColumns(headers.ToArray()); // 初始化欄位

        //    while (!reader.EndOfStream)
        //    {
        //        var rowData = reader.ReadLine()?.Split(',').ToArray();
        //        if (rowData != null) AddRow(rowData);
        //    }
        //}
    }
}
