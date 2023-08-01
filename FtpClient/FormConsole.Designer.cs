namespace FtpClient
{
	partial class FormConsole
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
			this.txtInput = new System.Windows.Forms.TextBox();
			this.txtOutput = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// txtInput
			// 
			this.txtInput.Location = new System.Drawing.Point(12, 502);
			this.txtInput.Name = "txtInput";
			this.txtInput.PlaceholderText = "输入指令";
			this.txtInput.Size = new System.Drawing.Size(776, 23);
			this.txtInput.TabIndex = 0;
			this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
			// 
			// txtOutput
			// 
			this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtOutput.Location = new System.Drawing.Point(12, 12);
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.Size = new System.Drawing.Size(776, 484);
			this.txtOutput.TabIndex = 1;
			this.txtOutput.Text = "";
			// 
			// FormConsole
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 539);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.txtInput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormConsole";
			this.Text = "FTP 客户端";
			this.Load += new System.EventHandler(this.FormConsole_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private TextBox txtInput;
		private RichTextBox txtOutput;
	}
}