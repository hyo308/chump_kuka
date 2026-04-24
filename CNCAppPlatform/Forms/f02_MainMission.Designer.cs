using Chump_kuka.Controls;

namespace Chump_kuka.Forms
{
    partial class f02_MainMission
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(f02_MainMission));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.scaleLabel7 = new iCAPS.ScaleLabel();
            this.myPanel1 = new iCAPS.myPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.scaleLabel1 = new iCAPS.ScaleLabel();
            this.scaleLabel2 = new iCAPS.ScaleLabel();
            this.scaleLabel3 = new iCAPS.ScaleLabel();
            this.scaleLabel4 = new iCAPS.ScaleLabel();
            this.scaleLabel5 = new iCAPS.ScaleLabel();
            this.scaleLabel6 = new iCAPS.ScaleLabel();
            this.led_idle = new iCAPS.DoubleImg();
            this.led_turtle_in = new iCAPS.DoubleImg();
            this.led_bot_move = new iCAPS.DoubleImg();
            this.led_bot_in = new iCAPS.DoubleImg();
            this.led_bot_out = new iCAPS.DoubleImg();
            this.led_task_over = new iCAPS.DoubleImg();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.scaleButton1 = new iCAPS.ScaleButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.local_reset = new iCAPS.DoubleImg();
            this.scaleLabel8 = new iCAPS.ScaleLabel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.myPanel2 = new iCAPS.myPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.task_list_reset = new iCAPS.DoubleImg();
            this.scaleLabel9 = new iCAPS.ScaleLabel();
            this.btn_AlertEnable = new System.Windows.Forms.Button();
            this.bind_area_control = new Chump_kuka.Controls.KukaAreaControl();
            this.treeGridView1 = new Chump_kuka.Controls.TreeGridView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.myPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led_idle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_turtle_in)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_bot_move)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_bot_in)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_bot_out)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_task_over)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.local_reset)).BeginInit();
            this.tableLayoutPanel6.SuspendLayout();
            this.myPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.task_list_reset)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btn_AlertEnable, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1159, 691);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel4, 2);
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.scaleLabel7, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.myPanel1, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(37, 23);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1082, 187);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // scaleLabel7
            // 
            this.scaleLabel7.AutoSize = true;
            this.scaleLabel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel7.Factor = 0.4F;
            this.scaleLabel7.Font = new System.Drawing.Font("微軟正黑體", 22.4F, System.Drawing.FontStyle.Bold);
            this.scaleLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.scaleLabel7.Location = new System.Drawing.Point(3, 0);
            this.scaleLabel7.Name = "scaleLabel7";
            this.scaleLabel7.Size = new System.Drawing.Size(1076, 56);
            this.scaleLabel7.TabIndex = 0;
            this.scaleLabel7.Text = "任務流程";
            this.scaleLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.scaleLabel7.Click += new System.EventHandler(this.scaleLabel7_Click);
            // 
            // myPanel1
            // 
            this.myPanel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.myPanel1.BorderColor = System.Drawing.Color.Black;
            this.myPanel1.BorderSize = 0;
            this.myPanel1.Controls.Add(this.tableLayoutPanel3);
            this.myPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myPanel1.Location = new System.Drawing.Point(3, 59);
            this.myPanel1.Name = "myPanel1";
            this.myPanel1.Radius = 10;
            this.myPanel1.Size = new System.Drawing.Size(1076, 125);
            this.myPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 6;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.Controls.Add(this.scaleLabel1, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.scaleLabel2, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.scaleLabel3, 2, 2);
            this.tableLayoutPanel3.Controls.Add(this.scaleLabel4, 3, 2);
            this.tableLayoutPanel3.Controls.Add(this.scaleLabel5, 4, 2);
            this.tableLayoutPanel3.Controls.Add(this.scaleLabel6, 5, 2);
            this.tableLayoutPanel3.Controls.Add(this.led_idle, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.led_turtle_in, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.led_bot_move, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.led_bot_in, 3, 1);
            this.tableLayoutPanel3.Controls.Add(this.led_bot_out, 4, 1);
            this.tableLayoutPanel3.Controls.Add(this.led_task_over, 5, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1076, 125);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // scaleLabel1
            // 
            this.scaleLabel1.AutoSize = true;
            this.scaleLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel1.Factor = 0.35F;
            this.scaleLabel1.Font = new System.Drawing.Font("微軟正黑體", 17.5F, System.Drawing.FontStyle.Bold);
            this.scaleLabel1.Location = new System.Drawing.Point(3, 65);
            this.scaleLabel1.Name = "scaleLabel1";
            this.scaleLabel1.Size = new System.Drawing.Size(173, 50);
            this.scaleLabel1.TabIndex = 0;
            this.scaleLabel1.Text = "區域閒置";
            this.scaleLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // scaleLabel2
            // 
            this.scaleLabel2.AutoSize = true;
            this.scaleLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel2.Factor = 0.35F;
            this.scaleLabel2.Font = new System.Drawing.Font("微軟正黑體", 17.5F, System.Drawing.FontStyle.Bold);
            this.scaleLabel2.Location = new System.Drawing.Point(182, 65);
            this.scaleLabel2.Name = "scaleLabel2";
            this.scaleLabel2.Size = new System.Drawing.Size(173, 50);
            this.scaleLabel2.TabIndex = 0;
            this.scaleLabel2.Text = "物料進站";
            this.scaleLabel2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // scaleLabel3
            // 
            this.scaleLabel3.AutoSize = true;
            this.scaleLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel3.Factor = 0.35F;
            this.scaleLabel3.Font = new System.Drawing.Font("微軟正黑體", 17.5F, System.Drawing.FontStyle.Bold);
            this.scaleLabel3.Location = new System.Drawing.Point(361, 65);
            this.scaleLabel3.Name = "scaleLabel3";
            this.scaleLabel3.Size = new System.Drawing.Size(173, 50);
            this.scaleLabel3.TabIndex = 0;
            this.scaleLabel3.Text = "已叫車";
            this.scaleLabel3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // scaleLabel4
            // 
            this.scaleLabel4.AutoSize = true;
            this.scaleLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel4.Factor = 0.35F;
            this.scaleLabel4.Font = new System.Drawing.Font("微軟正黑體", 17.5F, System.Drawing.FontStyle.Bold);
            this.scaleLabel4.Location = new System.Drawing.Point(540, 65);
            this.scaleLabel4.Name = "scaleLabel4";
            this.scaleLabel4.Size = new System.Drawing.Size(173, 50);
            this.scaleLabel4.TabIndex = 0;
            this.scaleLabel4.Text = "搬運車進站";
            this.scaleLabel4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // scaleLabel5
            // 
            this.scaleLabel5.AutoSize = true;
            this.scaleLabel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel5.Factor = 0.35F;
            this.scaleLabel5.Font = new System.Drawing.Font("微軟正黑體", 17.5F, System.Drawing.FontStyle.Bold);
            this.scaleLabel5.Location = new System.Drawing.Point(719, 65);
            this.scaleLabel5.Name = "scaleLabel5";
            this.scaleLabel5.Size = new System.Drawing.Size(173, 50);
            this.scaleLabel5.TabIndex = 0;
            this.scaleLabel5.Text = "搬運車出站";
            this.scaleLabel5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // scaleLabel6
            // 
            this.scaleLabel6.AutoSize = true;
            this.scaleLabel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel6.Factor = 0.35F;
            this.scaleLabel6.Font = new System.Drawing.Font("微軟正黑體", 17.5F, System.Drawing.FontStyle.Bold);
            this.scaleLabel6.Location = new System.Drawing.Point(898, 65);
            this.scaleLabel6.Name = "scaleLabel6";
            this.scaleLabel6.Size = new System.Drawing.Size(175, 50);
            this.scaleLabel6.TabIndex = 0;
            this.scaleLabel6.Text = "搬運車送達";
            this.scaleLabel6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // led_idle
            // 
            this.led_idle.Change = false;
            this.led_idle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.led_idle.EnableCilck = false;
            this.led_idle.Image = ((System.Drawing.Image)(resources.GetObject("led_idle.Image")));
            this.led_idle.Location = new System.Drawing.Point(3, 25);
            this.led_idle.Name = "led_idle";
            this.led_idle.SetSquare = false;
            this.led_idle.Size = new System.Drawing.Size(173, 37);
            this.led_idle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.led_idle.SubImg = ((System.Drawing.Image)(resources.GetObject("led_idle.SubImg")));
            this.led_idle.TabIndex = 1;
            this.led_idle.TabStop = false;
            this.led_idle.Tag = ((object)(resources.GetObject("led_idle.Tag")));
            this.led_idle.Click += new System.EventHandler(this.led_idle_Click);
            // 
            // led_turtle_in
            // 
            this.led_turtle_in.Change = false;
            this.led_turtle_in.Dock = System.Windows.Forms.DockStyle.Fill;
            this.led_turtle_in.EnableCilck = false;
            this.led_turtle_in.Image = ((System.Drawing.Image)(resources.GetObject("led_turtle_in.Image")));
            this.led_turtle_in.Location = new System.Drawing.Point(182, 25);
            this.led_turtle_in.Name = "led_turtle_in";
            this.led_turtle_in.SetSquare = false;
            this.led_turtle_in.Size = new System.Drawing.Size(173, 37);
            this.led_turtle_in.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.led_turtle_in.SubImg = ((System.Drawing.Image)(resources.GetObject("led_turtle_in.SubImg")));
            this.led_turtle_in.TabIndex = 1;
            this.led_turtle_in.TabStop = false;
            this.led_turtle_in.Tag = ((object)(resources.GetObject("led_turtle_in.Tag")));
            this.led_turtle_in.Click += new System.EventHandler(this.led_turtle_in_Click);
            // 
            // led_bot_move
            // 
            this.led_bot_move.Change = false;
            this.led_bot_move.Dock = System.Windows.Forms.DockStyle.Fill;
            this.led_bot_move.EnableCilck = false;
            this.led_bot_move.Image = ((System.Drawing.Image)(resources.GetObject("led_bot_move.Image")));
            this.led_bot_move.Location = new System.Drawing.Point(361, 25);
            this.led_bot_move.Name = "led_bot_move";
            this.led_bot_move.SetSquare = false;
            this.led_bot_move.Size = new System.Drawing.Size(173, 37);
            this.led_bot_move.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.led_bot_move.SubImg = ((System.Drawing.Image)(resources.GetObject("led_bot_move.SubImg")));
            this.led_bot_move.TabIndex = 1;
            this.led_bot_move.TabStop = false;
            this.led_bot_move.Tag = ((object)(resources.GetObject("led_bot_move.Tag")));
            this.led_bot_move.Click += new System.EventHandler(this.led_bot_move_Click);
            // 
            // led_bot_in
            // 
            this.led_bot_in.Change = false;
            this.led_bot_in.Dock = System.Windows.Forms.DockStyle.Fill;
            this.led_bot_in.EnableCilck = false;
            this.led_bot_in.Image = ((System.Drawing.Image)(resources.GetObject("led_bot_in.Image")));
            this.led_bot_in.Location = new System.Drawing.Point(540, 25);
            this.led_bot_in.Name = "led_bot_in";
            this.led_bot_in.SetSquare = false;
            this.led_bot_in.Size = new System.Drawing.Size(173, 37);
            this.led_bot_in.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.led_bot_in.SubImg = ((System.Drawing.Image)(resources.GetObject("led_bot_in.SubImg")));
            this.led_bot_in.TabIndex = 1;
            this.led_bot_in.TabStop = false;
            this.led_bot_in.Tag = ((object)(resources.GetObject("led_bot_in.Tag")));
            this.led_bot_in.Click += new System.EventHandler(this.led_bot_in_Click);
            // 
            // led_bot_out
            // 
            this.led_bot_out.Change = false;
            this.led_bot_out.Dock = System.Windows.Forms.DockStyle.Fill;
            this.led_bot_out.EnableCilck = false;
            this.led_bot_out.Image = ((System.Drawing.Image)(resources.GetObject("led_bot_out.Image")));
            this.led_bot_out.Location = new System.Drawing.Point(719, 25);
            this.led_bot_out.Name = "led_bot_out";
            this.led_bot_out.SetSquare = false;
            this.led_bot_out.Size = new System.Drawing.Size(173, 37);
            this.led_bot_out.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.led_bot_out.SubImg = ((System.Drawing.Image)(resources.GetObject("led_bot_out.SubImg")));
            this.led_bot_out.TabIndex = 1;
            this.led_bot_out.TabStop = false;
            this.led_bot_out.Tag = ((object)(resources.GetObject("led_bot_out.Tag")));
            this.led_bot_out.Click += new System.EventHandler(this.led_bot_out_Click);
            // 
            // led_task_over
            // 
            this.led_task_over.Change = false;
            this.led_task_over.Dock = System.Windows.Forms.DockStyle.Fill;
            this.led_task_over.EnableCilck = false;
            this.led_task_over.Image = ((System.Drawing.Image)(resources.GetObject("led_task_over.Image")));
            this.led_task_over.Location = new System.Drawing.Point(898, 25);
            this.led_task_over.Name = "led_task_over";
            this.led_task_over.SetSquare = false;
            this.led_task_over.Size = new System.Drawing.Size(175, 37);
            this.led_task_over.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.led_task_over.SubImg = ((System.Drawing.Image)(resources.GetObject("led_task_over.SubImg")));
            this.led_task_over.TabIndex = 1;
            this.led_task_over.TabStop = false;
            this.led_task_over.Tag = ((object)(resources.GetObject("led_task_over.Tag")));
            this.led_task_over.Click += new System.EventHandler(this.led_task_over_Click);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.bind_area_control, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.scaleButton1, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(37, 216);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(503, 436);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // scaleButton1
            // 
            this.scaleButton1.BackColor = System.Drawing.SystemColors.Control;
            this.scaleButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleButton1.Factor = 0.35F;
            this.scaleButton1.Font = new System.Drawing.Font("微軟正黑體", 17.15F);
            this.scaleButton1.Location = new System.Drawing.Point(3, 384);
            this.scaleButton1.Name = "scaleButton1";
            this.scaleButton1.Size = new System.Drawing.Size(497, 49);
            this.scaleButton1.TabIndex = 1;
            this.scaleButton1.Text = "Ready";
            this.scaleButton1.UseVisualStyleBackColor = false;
            this.scaleButton1.Click += new System.EventHandler(this.scaleButton1_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.local_reset);
            this.panel2.Controls.Add(this.scaleLabel8);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(503, 54);
            this.panel2.TabIndex = 2;
            // 
            // local_reset
            // 
            this.local_reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.local_reset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.local_reset.Change = true;
            this.local_reset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.local_reset.EnableCilck = true;
            this.local_reset.Image = ((System.Drawing.Image)(resources.GetObject("local_reset.Image")));
            this.local_reset.Location = new System.Drawing.Point(460, 14);
            this.local_reset.Name = "local_reset";
            this.local_reset.SetSquare = true;
            this.local_reset.Size = new System.Drawing.Size(37, 37);
            this.local_reset.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.local_reset.SubImg = null;
            this.local_reset.TabIndex = 0;
            this.local_reset.TabStop = false;
            this.local_reset.Click += new System.EventHandler(this.doubleImg1_Click);
            // 
            // scaleLabel8
            // 
            this.scaleLabel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel8.Factor = 0.4F;
            this.scaleLabel8.Font = new System.Drawing.Font("微軟正黑體", 21.6F, System.Drawing.FontStyle.Bold);
            this.scaleLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.scaleLabel8.Location = new System.Drawing.Point(0, 0);
            this.scaleLabel8.Name = "scaleLabel8";
            this.scaleLabel8.Size = new System.Drawing.Size(503, 54);
            this.scaleLabel8.TabIndex = 0;
            this.scaleLabel8.Text = "區域狀態";
            this.scaleLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.myPanel2, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(546, 216);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 2;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87.5F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(573, 436);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // myPanel2
            // 
            this.myPanel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.myPanel2.BorderColor = System.Drawing.Color.Black;
            this.myPanel2.BorderSize = 0;
            this.myPanel2.Controls.Add(this.treeGridView1);
            this.myPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myPanel2.Location = new System.Drawing.Point(3, 57);
            this.myPanel2.Name = "myPanel2";
            this.myPanel2.Radius = 10;
            this.myPanel2.Size = new System.Drawing.Size(567, 376);
            this.myPanel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.task_list_reset);
            this.panel1.Controls.Add(this.scaleLabel9);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(573, 54);
            this.panel1.TabIndex = 2;
            // 
            // task_list_reset
            // 
            this.task_list_reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.task_list_reset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.task_list_reset.Change = true;
            this.task_list_reset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.task_list_reset.EnableCilck = false;
            this.task_list_reset.Image = ((System.Drawing.Image)(resources.GetObject("task_list_reset.Image")));
            this.task_list_reset.Location = new System.Drawing.Point(530, 14);
            this.task_list_reset.Name = "task_list_reset";
            this.task_list_reset.SetSquare = true;
            this.task_list_reset.Size = new System.Drawing.Size(37, 37);
            this.task_list_reset.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.task_list_reset.SubImg = null;
            this.task_list_reset.TabIndex = 0;
            this.task_list_reset.TabStop = false;
            this.task_list_reset.Click += new System.EventHandler(this.task_list_reset_Click);
            // 
            // scaleLabel9
            // 
            this.scaleLabel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleLabel9.Factor = 0.4F;
            this.scaleLabel9.Font = new System.Drawing.Font("微軟正黑體", 21.6F, System.Drawing.FontStyle.Bold);
            this.scaleLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.scaleLabel9.Location = new System.Drawing.Point(0, 0);
            this.scaleLabel9.Name = "scaleLabel9";
            this.scaleLabel9.Size = new System.Drawing.Size(573, 54);
            this.scaleLabel9.TabIndex = 0;
            this.scaleLabel9.Text = "任務清單";
            this.scaleLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_AlertEnable
            // 
            this.btn_AlertEnable.BackColor = System.Drawing.Color.LightGreen;
            this.btn_AlertEnable.Font = new System.Drawing.Font("微軟正黑體", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_AlertEnable.Location = new System.Drawing.Point(37, 658);
            this.btn_AlertEnable.Name = "btn_AlertEnable";
            this.btn_AlertEnable.Size = new System.Drawing.Size(75, 30);
            this.btn_AlertEnable.TabIndex = 1;
            this.btn_AlertEnable.Text = "警報燈";
            this.btn_AlertEnable.UseVisualStyleBackColor = false;
            // 
            // bind_area_control
            // 
            this.bind_area_control.AllowClick = false;
            this.bind_area_control.AllowContainerClick = true;
            this.bind_area_control.AllowContainerLock = false;
            this.bind_area_control.AreaName = "No Area Bind";
            this.bind_area_control.AreaNode = new Chump_kuka.KukaModel.Node[0];
            this.bind_area_control.Checked = false;
            this.bind_area_control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bind_area_control.Location = new System.Drawing.Point(0, 54);
            this.bind_area_control.Margin = new System.Windows.Forms.Padding(0);
            this.bind_area_control.Model = null;
            this.bind_area_control.Name = "bind_area_control";
            this.bind_area_control.Size = new System.Drawing.Size(503, 327);
            this.bind_area_control.TabIndex = 0;
            this.bind_area_control.ContainerClick += new System.EventHandler<Chump_kuka.ControlClickEventArgs>(this.bind_area_control_ContainerClick);
            // 
            // treeGridView1
            // 
            this.treeGridView1.AutoIDVisible = false;
            this.treeGridView1.ColumnRatios = new float[0];
            this.treeGridView1.Columns = new Chump_kuka.Controls.TreeColumn[0];
            this.treeGridView1.DataSource = null;
            this.treeGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeGridView1.Location = new System.Drawing.Point(0, 0);
            this.treeGridView1.LogColName = null;
            this.treeGridView1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.treeGridView1.Name = "treeGridView1";
            this.treeGridView1.Size = new System.Drawing.Size(567, 376);
            this.treeGridView1.TabIndex = 0;
            // 
            // f02_MainMission
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1159, 691);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "f02_MainMission";
            this.Text = "交換站任務";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.myPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led_idle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_turtle_in)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_bot_move)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_bot_in)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_bot_out)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led_task_over)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.local_reset)).EndInit();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.myPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.task_list_reset)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Controls.KukaAreaControl bind_area_control;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private iCAPS.ScaleLabel scaleLabel1;
        private iCAPS.ScaleLabel scaleLabel2;
        private iCAPS.ScaleLabel scaleLabel3;
        private iCAPS.ScaleLabel scaleLabel4;
        private iCAPS.ScaleLabel scaleLabel5;
        private iCAPS.ScaleLabel scaleLabel6;
        private iCAPS.DoubleImg led_idle;
        private iCAPS.DoubleImg led_turtle_in;
        private iCAPS.DoubleImg led_bot_move;
        private iCAPS.DoubleImg led_bot_in;
        private iCAPS.DoubleImg led_bot_out;
        private iCAPS.DoubleImg led_task_over;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private iCAPS.ScaleLabel scaleLabel7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private iCAPS.ScaleLabel scaleLabel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private iCAPS.ScaleLabel scaleLabel9;
        private iCAPS.myPanel myPanel1;
        private iCAPS.ScaleButton scaleButton1;
        private iCAPS.myPanel myPanel2;
        private Controls.TreeGridView treeGridView1;
        private System.Windows.Forms.Panel panel1;
        private iCAPS.DoubleImg local_reset;
        private System.Windows.Forms.Panel panel2;
        private iCAPS.DoubleImg task_list_reset;
        private System.Windows.Forms.Button btn_AlertEnable;
    }
}