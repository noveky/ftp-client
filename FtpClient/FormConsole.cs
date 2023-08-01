using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using MyFtp2;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace FtpClient
{
	public partial class FormConsole : Form
	{
		FtpService service = new();

		public FormConsole()
		{
			InitializeComponent();
		}

		#region Console I/O
		List<string> cmds = new();
		int cmdIndex = -1;

		void ClearScreen()
		{
			txtOutput.Clear();
			WriteLn("Welcome to FTP Client!");
			HandleInput("?");
		}

		void Input(string inputStr)
		{
			cmds.Add(inputStr);
			txtOutput.Text += $"\n> {inputStr}\n";
			HandleInput(inputStr);
		}

		void Write(string writeStr)
		{
			txtOutput.Text += writeStr;
		}

		void WriteLn(string writeStr)
		{
			Write(writeStr + "\n");
		}
		#endregion

		#region Command interpretation
		void HandleInput(string inputStr)
		{
			try
			{
				inputStr = inputStr.Trim();

				int firstSpaceIndex = inputStr.IndexOf(' ');
				string command;
				string arguments = "";
				if (firstSpaceIndex == -1)
				{
					command = inputStr;
				}
				else
				{
					command = inputStr.Substring(0, firstSpaceIndex).ToLower();
					arguments = inputStr.Substring(firstSpaceIndex + 1);
				}

				switch (command)
				{
					case "?":
						Write("""
							cls: Clear screen
							connect HOST [USER PASS]: Connect to HOST using USER as username and PASS as password
							""");
						break;
					case "cls":
						ClearScreen();
						break;
					case "connect":
						string[] args = arguments.Split(' ');

						/*
						service.FtpServerIP = args[0];
						service.FtpUserID = args[1];
						service.FtpPassword = args[2];
						service.Connect();
						WriteLn($"Connected to {service.FtpServerIP}:{service.FtpServerPort}");
						*/

						// Create a request object
						FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{args[0]}/test");
						if (args.Length > 1)
						{
							request.Credentials = new NetworkCredential(args[1], args[2]);
						}
						request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

						// Get the response object
						FtpWebResponse response = (FtpWebResponse)request.GetResponse();

						// Do something with the response
						WriteLn($"Status: {response.StatusDescription}");

						// Cleanup
						response.Close();
						request.Abort();
						break;
					default:
						throw new InvalidOperationException("Invalid command.");
				}
			}
			catch (Exception ex)
			{
				Write(ex.ToString());
			}
		}
		#endregion

		#region UI events
		private void FormConsole_Load(object sender, EventArgs e)
		{
			ClearScreen();
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			switch (e.KeyCode)
			{
				case Keys.Return:
					Input(txtInput.Text);
					txtInput.Text = string.Empty;
					cmdIndex = -1;
					break;
				case Keys.Up:
					if (cmds.Count != 0)
					{
						if (cmdIndex == -1)
						{
							cmdIndex = cmds.Count - 1;
						}
						txtInput.Text = cmds[cmdIndex--];
					}
					txtInput.SelectionStart = txtInput.Text.Length;
					txtInput.SelectionLength = 0;
					break;
				case Keys.Down:
					if (cmds.Count != 0)
					{
						if (cmdIndex == -1)
						{
							cmdIndex = 0;
						}
						txtInput.Text = cmds[cmdIndex++];
						if (cmdIndex >= cmds.Count)
						{
							cmdIndex = -1;
						}
						txtInput.SelectionStart = txtInput.Text.Length;
						txtInput.SelectionLength = 0;
					}
					break;
				default:
					e.Handled = false;
					break;
			}
		}
		#endregion
	}
}
