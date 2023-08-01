namespace FtpClient
{
	partial class FormMain
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
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.txtStatus = new System.Windows.Forms.RichTextBox();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.btnConnect = new System.Windows.Forms.Button();
			this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.txtPass = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.txtHost = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.txtUser = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.txtWorkDir = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.btnUp = new System.Windows.Forms.Button();
			this.lstWorkDir = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.listView1 = new System.Windows.Forms.ListView();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.txtLocalPath = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cmsDirList = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsiDirList_Download = new System.Windows.Forms.ToolStripMenuItem();
			this.tsiDirList_Rename = new System.Windows.Forms.ToolStripMenuItem();
			this.tsiDirList_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			this.tableLayoutPanel9.SuspendLayout();
			this.tableLayoutPanel8.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel12.SuspendLayout();
			this.tableLayoutPanel13.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.cmsDirList.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.txtStatus, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel7, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(855, 584);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// txtStatus
			// 
			this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.txtStatus.Location = new System.Drawing.Point(3, 487);
			this.txtStatus.Name = "txtStatus";
			this.txtStatus.ReadOnly = true;
			this.txtStatus.Size = new System.Drawing.Size(849, 94);
			this.txtStatus.TabIndex = 0;
			this.txtStatus.TabStop = false;
			this.txtStatus.Text = "";
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.AutoSize = true;
			this.tableLayoutPanel7.ColumnCount = 4;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel7.Controls.Add(this.btnConnect, 3, 0);
			this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel9, 0, 0);
			this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel8, 0, 0);
			this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel4, 0, 0);
			this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 1;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(642, 34);
			this.tableLayoutPanel7.TabIndex = 0;
			// 
			// btnConnect
			// 
			this.btnConnect.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnConnect.Location = new System.Drawing.Point(545, 3);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(94, 28);
			this.btnConnect.TabIndex = 3;
			this.btnConnect.Text = "连接";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// tableLayoutPanel9
			// 
			this.tableLayoutPanel9.AutoSize = true;
			this.tableLayoutPanel9.ColumnCount = 2;
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel9.Controls.Add(this.label5, 0, 0);
			this.tableLayoutPanel9.Controls.Add(this.txtPass, 1, 0);
			this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Left;
			this.tableLayoutPanel9.Location = new System.Drawing.Point(389, 3);
			this.tableLayoutPanel9.Name = "tableLayoutPanel9";
			this.tableLayoutPanel9.RowCount = 1;
			this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel9.Size = new System.Drawing.Size(150, 28);
			this.tableLayoutPanel9.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(44, 29);
			this.label5.TabIndex = 0;
			this.label5.Text = "密码：";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtPass
			// 
			this.txtPass.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtPass.Location = new System.Drawing.Point(53, 3);
			this.txtPass.Name = "txtPass";
			this.txtPass.Size = new System.Drawing.Size(94, 23);
			this.txtPass.TabIndex = 0;
			this.txtPass.UseSystemPasswordChar = true;
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.AutoSize = true;
			this.tableLayoutPanel8.ColumnCount = 2;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanel8.Controls.Add(this.label4, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.txtHost, 1, 0);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Left;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 1;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel8.Size = new System.Drawing.Size(212, 28);
			this.tableLayoutPanel8.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 29);
			this.label4.TabIndex = 0;
			this.label4.Text = "服务器：";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtHost
			// 
			this.txtHost.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtHost.Location = new System.Drawing.Point(65, 3);
			this.txtHost.Name = "txtHost";
			this.txtHost.Size = new System.Drawing.Size(144, 23);
			this.txtHost.TabIndex = 0;
			this.txtHost.Text = "localhost";
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.AutoSize = true;
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.txtUser, 1, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Left;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(221, 3);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.Size = new System.Drawing.Size(162, 28);
			this.tableLayoutPanel4.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 29);
			this.label1.TabIndex = 0;
			this.label1.Text = "用户名：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtUser
			// 
			this.txtUser.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtUser.Location = new System.Drawing.Point(65, 3);
			this.txtUser.Name = "txtUser";
			this.txtUser.Size = new System.Drawing.Size(94, 23);
			this.txtUser.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel12, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.panel2, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 43);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(849, 438);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// tableLayoutPanel12
			// 
			this.tableLayoutPanel12.ColumnCount = 1;
			this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel12.Controls.Add(this.tableLayoutPanel13, 0, 0);
			this.tableLayoutPanel12.Controls.Add(this.lstWorkDir, 0, 1);
			this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel12.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel12.Name = "tableLayoutPanel12";
			this.tableLayoutPanel12.RowCount = 2;
			this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel12.Size = new System.Drawing.Size(543, 432);
			this.tableLayoutPanel12.TabIndex = 0;
			// 
			// tableLayoutPanel13
			// 
			this.tableLayoutPanel13.ColumnCount = 4;
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel13.Controls.Add(this.btnRefresh, 3, 0);
			this.tableLayoutPanel13.Controls.Add(this.txtWorkDir, 1, 0);
			this.tableLayoutPanel13.Controls.Add(this.label7, 0, 0);
			this.tableLayoutPanel13.Controls.Add(this.btnUp, 1, 0);
			this.tableLayoutPanel13.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel13.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel13.Name = "tableLayoutPanel13";
			this.tableLayoutPanel13.RowCount = 1;
			this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel13.Size = new System.Drawing.Size(537, 29);
			this.tableLayoutPanel13.TabIndex = 0;
			// 
			// btnRefresh
			// 
			this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnRefresh.Location = new System.Drawing.Point(480, 3);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(54, 23);
			this.btnRefresh.TabIndex = 2;
			this.btnRefresh.Text = "刷新";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// txtWorkDir
			// 
			this.txtWorkDir.AcceptsReturn = true;
			this.txtWorkDir.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtWorkDir.Location = new System.Drawing.Point(93, 3);
			this.txtWorkDir.Name = "txtWorkDir";
			this.txtWorkDir.Size = new System.Drawing.Size(381, 23);
			this.txtWorkDir.TabIndex = 1;
			this.txtWorkDir.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtWorkDir_KeyDown);
			this.txtWorkDir.Leave += new System.EventHandler(this.txtWorkDir_Leave);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label7.Location = new System.Drawing.Point(3, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(54, 29);
			this.label7.TabIndex = 0;
			this.label7.Text = "远程";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnUp
			// 
			this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnUp.Location = new System.Drawing.Point(63, 3);
			this.btnUp.Name = "btnUp";
			this.btnUp.Size = new System.Drawing.Size(24, 23);
			this.btnUp.TabIndex = 0;
			this.btnUp.Text = "↑";
			this.btnUp.UseVisualStyleBackColor = true;
			this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
			// 
			// lstWorkDir
			// 
			this.lstWorkDir.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
			this.lstWorkDir.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstWorkDir.FullRowSelect = true;
			this.lstWorkDir.GridLines = true;
			this.lstWorkDir.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstWorkDir.Location = new System.Drawing.Point(3, 38);
			this.lstWorkDir.Name = "lstWorkDir";
			this.lstWorkDir.Size = new System.Drawing.Size(537, 391);
			this.lstWorkDir.TabIndex = 1;
			this.lstWorkDir.UseCompatibleStateImageBehavior = false;
			this.lstWorkDir.View = System.Windows.Forms.View.Details;
			this.lstWorkDir.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstWorkDir_MouseClick);
			this.lstWorkDir.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstWorkDir_MouseDoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "名称";
			this.columnHeader1.Width = 120;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "修改日期";
			this.columnHeader2.Width = 160;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "大小";
			this.columnHeader3.Width = 80;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Controls.Add(this.panel1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(552, 3);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
			this.panel2.Size = new System.Drawing.Size(294, 432);
			this.panel2.TabIndex = 1;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.listView1);
			this.panel3.Controls.Add(this.label3);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(0, 89);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(291, 340);
			this.panel3.TabIndex = 0;
			// 
			// listView1
			// 
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.Location = new System.Drawing.Point(0, 23);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(291, 317);
			this.listView1.TabIndex = 2;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// label3
			// 
			this.label3.Dock = System.Windows.Forms.DockStyle.Top;
			this.label3.Location = new System.Drawing.Point(0, 0);
			this.label3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(291, 23);
			this.label3.TabIndex = 0;
			this.label3.Text = "传输列表";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.button2);
			this.panel1.Controls.Add(this.button1);
			this.panel1.Controls.Add(this.txtLocalPath);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(291, 89);
			this.panel1.TabIndex = 1;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(148, 3);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(140, 29);
			this.button2.TabIndex = 1;
			this.button2.Text = "新建目录";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(3, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(140, 29);
			this.button1.TabIndex = 0;
			this.button1.Text = "上传文件";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// txtLocalPath
			// 
			this.txtLocalPath.AcceptsReturn = true;
			this.txtLocalPath.Location = new System.Drawing.Point(0, 60);
			this.txtLocalPath.Name = "txtLocalPath";
			this.txtLocalPath.PlaceholderText = "未设置";
			this.txtLocalPath.Size = new System.Drawing.Size(291, 23);
			this.txtLocalPath.TabIndex = 2;
			this.txtLocalPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtLocalPath_KeyDown);
			this.txtLocalPath.Leave += new System.EventHandler(this.txtLocalPath_Leave);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(0, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 17);
			this.label2.TabIndex = 9;
			this.label2.Text = "本地默认路径";
			// 
			// cmsDirList
			// 
			this.cmsDirList.DropShadowEnabled = false;
			this.cmsDirList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsiDirList_Download,
            this.tsiDirList_Rename,
            this.tsiDirList_Delete});
			this.cmsDirList.Name = "DirItemDropDown";
			this.cmsDirList.Size = new System.Drawing.Size(113, 70);
			// 
			// tsiDirList_Download
			// 
			this.tsiDirList_Download.Name = "tsiDirList_Download";
			this.tsiDirList_Download.Size = new System.Drawing.Size(112, 22);
			this.tsiDirList_Download.Text = "下载";
			// 
			// tsiDirList_Rename
			// 
			this.tsiDirList_Rename.Name = "tsiDirList_Rename";
			this.tsiDirList_Rename.Size = new System.Drawing.Size(112, 22);
			this.tsiDirList_Rename.Text = "重命名";
			// 
			// tsiDirList_Delete
			// 
			this.tsiDirList_Delete.Name = "tsiDirList_Delete";
			this.tsiDirList_Delete.Size = new System.Drawing.Size(112, 22);
			this.tsiDirList_Delete.Text = "删除";
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(855, 584);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "FormMain";
			this.Text = "FTP 客户端";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel7.ResumeLayout(false);
			this.tableLayoutPanel7.PerformLayout();
			this.tableLayoutPanel9.ResumeLayout(false);
			this.tableLayoutPanel9.PerformLayout();
			this.tableLayoutPanel8.ResumeLayout(false);
			this.tableLayoutPanel8.PerformLayout();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel4.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel12.ResumeLayout(false);
			this.tableLayoutPanel13.ResumeLayout(false);
			this.tableLayoutPanel13.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.cmsDirList.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel7;
		private Button btnConnect;
		private TableLayoutPanel tableLayoutPanel9;
		private Label label5;
		private TextBox txtPass;
		private TableLayoutPanel tableLayoutPanel8;
		private Label label4;
		private TextBox txtHost;
		private TableLayoutPanel tableLayoutPanel4;
		private Label label1;
		private TextBox txtUser;
		private RichTextBox txtStatus;
		private TableLayoutPanel tableLayoutPanel2;
		private TableLayoutPanel tableLayoutPanel12;
		private TableLayoutPanel tableLayoutPanel13;
		private Button btnRefresh;
		private TextBox txtWorkDir;
		private Label label7;
		private Button btnUp;
		private ListView lstWorkDir;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private Panel panel2;
		private Panel panel3;
		private ListView listView1;
		private Label label3;
		private Panel panel1;
		private Button button2;
		private Button button1;
		private TextBox txtLocalPath;
		private Label label2;
		private ContextMenuStrip cmsDirList;
		private ToolStripMenuItem tsiDirList_Download;
		private ToolStripMenuItem tsiDirList_Rename;
		private ToolStripMenuItem tsiDirList_Delete;
	}
}