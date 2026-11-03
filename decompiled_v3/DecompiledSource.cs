using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OfficeOpenXml;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("TANACHAIarduino")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("TANACHAIarduino")]
[assembly: AssemblyCopyright("Copyright ©  2023")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("b318f420-33c8-4505-9ee8-a9de4f6eaf20")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.7.2", FrameworkDisplayName = ".NET Framework 4.7.2")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace TANACHAIarduino
{
	public class MainForm : Form
	{
		private int graph_scaler = 500;

		private int send_repeat_counter = 0;

		private bool send_data_flag = false;

		private bool plotter_flag = true;

		private StreamWriter out_file;

		private StreamReader in_file;

		private Series newSpline;

		private int[] splineCounters;

		private Series[] dataSeries;

		private StringBuilder receivedData = new StringBuilder();

		private double countb = 0.0;

		private double ic = 0.0;

		private double beta = 0.0;

		private double ib;

		private bool grf = false;

		private string[] variables;

		private string ibValue;

		private bool btnclick = true;

		private bool isUpdatingTextBox1 = false;

		private bool isUpdatingTextBox2 = false;

		private bool isUpdatingTextBox3 = false;

		private Color enabledTextColor = Color.Black;

		private IContainer components = null;

		private Button connect;

		private ComboBox portConfig;

		private ComboBox baudrateConfig;

		private ComboBox databitsConfig;

		private ComboBox stopbitsConfig;

		private ComboBox parityConfig;

		private SerialPort mySerial;

		private GroupBox serial_options_group;

		private Label parity_label;

		private Label stopbits_label;

		private Label databits_label;

		private Label baudrate_label;

		private Label port_label;

		private Label flowconrol_label;

		private ComboBox flowcontrolConfig;

		private StatusStrip statusStrip1;

		private ToolStripStatusLabel toolStripStatusLabel1;

		private TabPage tabPage1;

		private OpenFileDialog openFileDialog1;

		private Button sendData;

		private TextBox tx_textarea;

		private Label label7;

		private NumericUpDown graph_speed;

		private Timer tx_repeater_delay;

		private BackgroundWorker backgroundWorker1;

		private ContextMenuStrip contextMenuStrip1;

		private ToolStripMenuItem clearToolStripMenuItem;

		private ContextMenuStrip graph_menu;

		private ToolStripMenuItem clearToolStripMenuItem1;

		private ToolStripMenuItem saveAsImageToolStripMenuItem;

		private SaveFileDialog saveFileDialog1;

		private NumericUpDown graph_scale;

		private Label label3;

		private GroupBox plotter_option_group;

		private CheckBox set_graph_min_enable;

		private NumericUpDown graph_min;

		private CheckBox set_graph_max_enable;

		private ToolStripStatusLabel toolStripStatusLabel2;

		protected TabControl Set_up;

		private RadioButton write_form_file_radiobutton;

		private RadioButton key_capture_radiobutton;

		private RadioButton send_word_radiobutton;

		private Label label2;

		private Label label1;

		private NumericUpDown send_repeat;

		private NumericUpDown send_delay;

		private GroupBox write_options_group;

		private Panel datalogger_options_panel;

		private Label send_status;

		private NotifyIcon alert_messege;

		private RichTextBox rx_textarea;

		private TabPage tabPage3;

		private RadioButton datalogger_overwrite_radiobutton;

		private RadioButton datalogger_append_radiobutton;

		private CheckBox datalogger_checkbox;

		private GroupBox read_options_group;

		private Panel dsiplayas_options_panel;

		private RadioButton display_hex_radiobutton;

		private Label label4;

		private RadioButton display_string_radiobutton;

		private GroupBox groupBox1;

		private RichTextBox tx_terminal;

		private NumericUpDown graph_max;

		private ToolStripStatusLabel byte_counter_label;

		private ProgressBar progressBar1;

		private Panel tx_num_panel;

		private Panel tx_radiobuttons_panel;

		private ContextMenuStrip contextMenuStrip2;

		private ToolStripMenuItem toolStripMenuItem1;

		private Button save_btn;

		private Label label22;

		private Label label23;

		private TextBox Rb_gate;

		private Label label21;

		private TextBox steps_gate;

		private Label label16;

		private Label label17;

		private TextBox max_gate;

		private TextBox dk_gate;

		private Label label11;

		private Label label12;

		private Label label13;

		private TextBox max_current;

		private TextBox step_volt;

		private Label label9;

		private Label label8;

		private Label label6;

		private Label label5;

		private TextBox max_volt;

		private TextBox min_volt;

		private TextBox textBox4;

		private TextBox textBox3;

		private TextBox textBox2;

		private TextBox textBox1;

		private RichTextBox richTextBox2;

		private RichTextBox richTextBox1;

		private Button send_btn;

		private Label label14;

		private Label label10;

		private Label label15;

		private Chart graph;

		private Button Stop_btn;

		private Label label19;

		private Label label24;

		private Label label25;

		private TextBox dk_gateA;

		private Label label26;

		private Label label27;

		private TextBox maxA_gate;

		private Label label28;

		private TextBox stepsA_gate;

		public string data { get; set; }

		public MainForm()
		{
			InitializeComponent();
			configrations();
		}

		public void configrations()
		{
			ComboBox.ObjectCollection items = portConfig.Items;
			object[] portNames = SerialPort.GetPortNames();
			items.AddRange(portNames);
			baudrateConfig.DataSource = new string[7] { "115200", "19200", "230400", "57600", "38400", "9600", "4800" };
			parityConfig.DataSource = new string[5] { "None", "Odd", "Even", "Mark", "Space" };
			databitsConfig.DataSource = new string[4] { "5", "6", "7", "8" };
			stopbitsConfig.DataSource = new string[3] { "1", "2", "1.5" };
			flowcontrolConfig.DataSource = new string[4] { "None", "RTS", "RTS/X", "Xon/Xoff" };
			baudrateConfig.SelectedIndex = 5;
			parityConfig.SelectedIndex = 0;
			databitsConfig.SelectedIndex = 3;
			stopbitsConfig.SelectedIndex = 0;
			flowcontrolConfig.SelectedIndex = 0;
			openFileDialog1.Filter = "Text|*.txt";
			mySerial.DataReceived += rx_data_event;
			tx_repeater_delay.Tick += send_data;
			backgroundWorker1.DoWork += update_rxtextarea_event;
			dataSeries = new Series[5];
			splineCounters = new int[5];
			ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
			for (int i = 0; i < 1; i++)
			{
				dataSeries[i] = new Series();
				dataSeries[i].ChartType = SeriesChartType.Spline;
				graph.Series.Add(dataSeries[i]);
				dataSeries[i].Name = "VACH " + splineCounters[i];
			}
		}

		private void connect_Click(object sender, EventArgs e)
		{
			if (!mySerial.IsOpen)
			{
				if (!Serial_port_config())
				{
					return;
				}
				try
				{
					mySerial.Open();
				}
				catch
				{
					alert("Can't open " + mySerial.PortName + " port, it might be used in another program");
					return;
				}
				if (datalogger_checkbox.Checked)
				{
					try
					{
						out_file = new StreamWriter(datalogger_checkbox.Text, datalogger_append_radiobutton.Checked);
					}
					catch
					{
						alert("Can't open " + datalogger_checkbox.Text + " file, it might be used in another program");
						return;
					}
				}
				UserControl_state(value: true);
			}
			else
			{
				if (!mySerial.IsOpen)
				{
					return;
				}
				try
				{
					mySerial.Close();
					mySerial.DiscardInBuffer();
					mySerial.DiscardOutBuffer();
				}
				catch
				{
				}
				if (datalogger_checkbox.Checked)
				{
					try
					{
						out_file.Dispose();
					}
					catch
					{
					}
				}
				try
				{
					mySerial.Close();
				}
				catch
				{
				}
				UserControl_state(value: false);
			}
		}

		private void rx_data_event(object sender, SerialDataReceivedEventArgs e)
		{
			if (!mySerial.IsOpen)
			{
				return;
			}
			try
			{
				int bytesToRead = mySerial.BytesToRead;
				byte[] array = new byte[bytesToRead];
				if (mySerial.Read(array, 0, bytesToRead) == 0)
				{
					return;
				}
				string value = Encoding.UTF8.GetString(array);
				receivedData.Append(value);
				int num = receivedData.ToString().LastIndexOf('\n');
				if (num >= 0)
				{
					string packet = receivedData.ToString(0, num + 1);
					receivedData.Remove(0, num + 1);
					ProcessDataPacket(packet);
				}
				if (!datalogger_checkbox.Checked)
				{
					return;
				}
				try
				{
					out_file.Write(data.Replace("\\n", Environment.NewLine));
				}
				catch
				{
					alert("Can't write to " + datalogger_checkbox.Text + " file it might be not exist or it is opennd in another program");
				}
			}
			catch
			{
				alert("Can't read form  " + mySerial.PortName + " port it might be opennd in another program");
			}
		}

		private void ProcessDataPacket(string packet)
		{
			BeginInvoke((Action)(() =>
			{
				double.TryParse(packet, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
				string text = result.ToString();
				if (packet.StartsWith("Ib ="))
				{
					int startIndex = text.IndexOf("Ib =") + 5;
					ibValue = packet.Substring(startIndex).Trim();
					ib = double.Parse(ibValue) / double.Parse(Rb_gate.Text);
					rx_textarea.AppendText("Ib = " + ib.ToString("0.##"));
					rx_textarea.AppendText("\n");
				}
				else if (packet.Contains("New"))
				{
					grf = true;
					if (countb.ToString() == "0")
					{
						countb = 1.0;
					}
					beta = ic / countb;
					beta /= ib;
					rx_textarea.AppendText("Beta = " + beta.ToString("0.####") + "\n");
					rx_textarea.AppendText("\n");
					beta = 0.0;
					countb = 0.0;
					ic = 0.0;
				}
				else
				{
					if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
					{
						rx_textarea.AppendText("[RX]> ");
						rx_textarea.AppendText(text);
						rx_textarea.AppendText("\n");
						rx_textarea.AppendText("");
						data = text;
						ic += result2;
						countb++;
					}
					if (plotter_flag)
					{
						variables = text.Split('\n')[0].Split(':');
						for (int i = 0; i < variables.Length && i < 5; i++)
						{
							if (double.TryParse(variables[i], out var result3))
							{
								if (dataSeries[i].Points.Count > graph_scaler)
								{
									dataSeries[i].Points.RemoveAt(0);
								}
								if (grf)
								{
									dataSeries[i].Name = "VACH " + splineCounters[i];
									Series series = new Series
									{
										ChartType = SeriesChartType.Spline,
										Points = { new double[1] }
									};
									graph.Series.Add(series);
									dataSeries[i] = series;
									splineCounters[i]++;
									dataSeries[i].Name = "VACH " + splineCounters[i];
									grf = false;
								}
								else
								{
									dataSeries[i].Points.Add(result3);
								}
							}
						}
						graph.ResetAutoValues();
					}
				}
			}));
		}

		private void update_rxtextarea_event(object sender, DoWorkEventArgs e)
		{
			BeginInvoke((Action)(() =>
			{
				if (rx_textarea.Lines.Count() > 5000)
				{
					rx_textarea.ResetText();
				}
				rx_textarea.AppendText("[RX]> " + data);
			}));
		}

		private void datalogger_checkbox_CheckedChanged(object sender, EventArgs e)
		{
			if (datalogger_checkbox.Checked)
			{
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					datalogger_checkbox.Text = openFileDialog1.FileName;
					datalogger_append_radiobutton.Enabled = true;
					datalogger_overwrite_radiobutton.Enabled = true;
					datalogger_append_radiobutton.Enabled = true;
					datalogger_overwrite_radiobutton.Enabled = true;
				}
				else
				{
					datalogger_checkbox.Checked = false;
					datalogger_append_radiobutton.Enabled = false;
					datalogger_overwrite_radiobutton.Enabled = false;
					datalogger_append_radiobutton.Enabled = false;
					datalogger_overwrite_radiobutton.Enabled = false;
				}
			}
			else
			{
				datalogger_append_radiobutton.Enabled = false;
				datalogger_overwrite_radiobutton.Enabled = false;
				datalogger_checkbox.Text = "Enable Data logger";
			}
		}

		private void clear_rx_textarea_Click(object sender, EventArgs e)
		{
			rx_textarea.Clear();
		}

		private void sendData_Click(object sender, EventArgs e)
		{
			if (!send_data_flag)
			{
				tx_repeater_delay.Interval = (int)send_delay.Value;
				tx_repeater_delay.Start();
				if (send_word_radiobutton.Checked)
				{
					progressBar1.Maximum = (int)send_repeat.Value;
					progressBar1.Visible = true;
				}
				else if (write_form_file_radiobutton.Checked)
				{
					try
					{
						in_file = new StreamReader(tx_textarea.Text, detectEncodingFromByteOrderMarks: true);
					}
					catch
					{
						alert("Can't open " + tx_textarea.Text + " file, it might be not exist or it is used in another program");
						return;
					}
					progressBar1.Maximum = file_size(tx_textarea.Text);
					progressBar1.Visible = true;
				}
				send_data_flag = true;
				tx_num_panel.Enabled = false;
				tx_textarea.Enabled = false;
				tx_radiobuttons_panel.Enabled = false;
				sendData.Text = "Stop";
				return;
			}
			tx_repeater_delay.Stop();
			progressBar1.Value = 0;
			send_repeat_counter = 0;
			send_data_flag = false;
			progressBar1.Visible = false;
			tx_num_panel.Enabled = true;
			tx_textarea.Enabled = true;
			tx_radiobuttons_panel.Enabled = true;
			sendData.Text = "Send";
			if (!write_form_file_radiobutton.Checked)
			{
				return;
			}
			try
			{
				in_file.Dispose();
			}
			catch
			{
			}
		}

		private void send_data(object sender, EventArgs e)
		{
			string text = "";
			if (send_word_radiobutton.Checked)
			{
				text = tx_textarea.Text.Replace("\n", Environment.NewLine);
				if (send_repeat_counter < (int)send_repeat.Value)
				{
					send_repeat_counter++;
					progressBar1.Value = send_repeat_counter;
					progressBar1.Update();
				}
				else
				{
					send_data_flag = false;
				}
			}
			else if (write_form_file_radiobutton.Checked)
			{
				try
				{
					text = in_file.ReadLine();
				}
				catch
				{
				}
				if (text == null)
				{
					send_data_flag = false;
				}
				else
				{
					progressBar1.Value = send_repeat_counter;
					send_repeat_counter++;
				}
				text += "\\n";
			}
			if (send_data_flag)
			{
				if (mySerial.IsOpen)
				{
					try
					{
						mySerial.Write(text.Replace("\\n", Environment.NewLine));
						tx_terminal.AppendText("[TX]> " + text + "\n");
						return;
					}
					catch
					{
						alert("Can't write to " + mySerial.PortName + " port it might be opennd in another program");
						return;
					}
				}
				return;
			}
			tx_repeater_delay.Stop();
			sendData.Text = "Send";
			send_repeat_counter = 0;
			progressBar1.Value = 0;
			progressBar1.Visible = false;
			tx_radiobuttons_panel.Enabled = true;
			tx_num_panel.Enabled = true;
			tx_textarea.Enabled = true;
			if (!write_form_file_radiobutton.Checked)
			{
				return;
			}
			try
			{
				in_file.Dispose();
			}
			catch
			{
			}
		}

		private void tx_textarea_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (key_capture_radiobutton.Checked && mySerial.IsOpen)
			{
				try
				{
					mySerial.Write(e.KeyChar.ToString());
					tx_terminal.AppendText("[TX]> " + e.KeyChar + "\n");
					tx_textarea.Clear();
				}
				catch
				{
					alert("Can't write to " + mySerial.PortName + " port it might be opennd in another program");
				}
			}
		}

		private void send_word_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
			tx_textarea.Clear();
			send_repeat.Enabled = send_word_radiobutton.Checked;
			send_delay.Enabled = send_word_radiobutton.Checked;
			ActiveControl = tx_textarea;
		}

		private void key_capture_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
			tx_textarea.Clear();
			send_repeat.Enabled = !key_capture_radiobutton.Checked;
			send_delay.Enabled = !key_capture_radiobutton.Checked;
			sendData.Enabled = !key_capture_radiobutton.Checked;
			ActiveControl = tx_textarea;
		}

		private void write_form_file_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
			tx_textarea.Clear();
			send_repeat.Enabled = !write_form_file_radiobutton.Checked;
			send_delay.Enabled = write_form_file_radiobutton.Checked;
			if (write_form_file_radiobutton.Checked)
			{
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					tx_textarea.Text = openFileDialog1.FileName;
					tx_textarea.Cursor = Cursors.Hand;
					tx_textarea.ReadOnly = true;
				}
				else
				{
					send_word_radiobutton.Checked = true;
				}
			}
			else
			{
				tx_textarea.Cursor = Cursors.IBeam;
				tx_textarea.ReadOnly = false;
			}
		}

		private void graph_speed_ValueChanged(object sender, EventArgs e)
		{
			graph.ChartAreas[0].AxisY.Interval = (int)graph_speed.Value;
		}

		private void graph_scale_ValueChanged(object sender, EventArgs e)
		{
			graph_scaler = (int)graph_scale.Value;
		}

		private void set_graph_max_enable_CheckedChanged(object sender, EventArgs e)
		{
			if (set_graph_max_enable.Checked)
			{
				try
				{
					graph_max.Value = (int)graph.ChartAreas[0].AxisY.Maximum;
					graph.ChartAreas[0].AxisY.Maximum = (int)graph_max.Value;
				}
				catch
				{
					alert("Invalid Minimum value");
				}
			}
			else
			{
				graph.ChartAreas[0].AxisY.Maximum = double.NaN;
			}
			graph_max.Enabled = set_graph_max_enable.Checked;
		}

		private void graph_max_ValueChanged(object sender, EventArgs e)
		{
			if (graph_max.Value > graph_min.Value)
			{
				graph.ChartAreas[0].AxisY.Maximum = (int)graph_max.Value;
			}
			else
			{
				alert("Invalid Maximum value");
			}
		}

		private void set_graph_min_enable_CheckedChanged(object sender, EventArgs e)
		{
			if (set_graph_min_enable.Checked)
			{
				try
				{
					graph_min.Value = (int)graph.ChartAreas[0].AxisY.Minimum;
					graph.ChartAreas[0].AxisY.Minimum = (int)graph_min.Value;
				}
				catch
				{
					alert("Invalid Minimum value");
				}
			}
			else
			{
				graph.ChartAreas[0].AxisY.Minimum = double.NaN;
			}
			graph_min.Enabled = set_graph_min_enable.Checked;
		}

		private void graph_min_ValueChanged(object sender, EventArgs e)
		{
			if (graph_min.Value < graph_max.Value)
			{
				graph.ChartAreas[0].AxisY.Minimum = (int)graph_min.Value;
			}
			else
			{
				alert("Invalid Minimum value");
			}
		}

		private void saveAsImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				graph.SaveImage(saveFileDialog1.FileName, ChartImageFormat.Png);
			}
		}

		private void clear_graph_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < variables.Length; i++)
			{
				graph.Series[i].Points.Clear();
			}
		}

		private bool Serial_port_config()
		{
			try
			{
				mySerial.PortName = portConfig.Text;
			}
			catch
			{
				alert("There are no available ports");
				return false;
			}
			mySerial.BaudRate = int.Parse(baudrateConfig.Text);
			mySerial.StopBits = (StopBits)Enum.Parse(typeof(StopBits), (stopbitsConfig.SelectedIndex + 1).ToString(), ignoreCase: true);
			mySerial.Parity = (Parity)Enum.Parse(typeof(Parity), parityConfig.SelectedIndex.ToString(), ignoreCase: true);
			mySerial.DataBits = int.Parse(databitsConfig.Text);
			mySerial.Handshake = (Handshake)Enum.Parse(typeof(Handshake), flowcontrolConfig.SelectedIndex.ToString(), ignoreCase: true);
			return true;
		}

		private void UserControl_state(bool value)
		{
			serial_options_group.Enabled = !value;
			datalogger_options_panel.Enabled = !value;
			write_options_group.Enabled = value;
			if (value)
			{
				connect.Text = "Disconnected";
				toolStripStatusLabel1.Text = "Connected port: " + mySerial.PortName + " @ " + mySerial.BaudRate + " bps";
			}
			else
			{
				connect.Text = "Connected";
				toolStripStatusLabel1.Text = "No Connection";
			}
		}

		private void portConfig_Click(object sender, EventArgs e)
		{
			portConfig.Items.Clear();
			ComboBox.ObjectCollection items = portConfig.Items;
			object[] portNames = SerialPort.GetPortNames();
			items.AddRange(portNames);
		}

		private void alert(string text)
		{
			alert_messege.Icon = Icon;
			alert_messege.Visible = true;
			alert_messege.ShowBalloonTip(5000, "Serial Lab", text, ToolTipIcon.Error);
		}

		private void toolStripStatusLabel2_Click(object sender, EventArgs e)
		{
			AboutBox1 aboutBox = new AboutBox1();
			aboutBox.ShowDialog();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (mySerial.IsOpen)
			{
				mySerial.Close();
			}
		}

		private void tx_textarea_Click(object sender, EventArgs e)
		{
			if (write_form_file_radiobutton.Checked)
			{
				write_form_file_radiobutton_CheckedChanged(sender, e);
			}
		}

		private int file_size(string path)
		{
			string text = new StreamReader(path).ReadToEnd();
			string[] source = text.Split('\n');
			return source.Count();
		}

		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			tx_terminal.Clear();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
		}

		private void SaveAsExcelEPPlus(string filePath)
		{
			try
			{
				FileInfo newFile = new FileInfo(filePath);
				using (ExcelPackage excelPackage = new ExcelPackage(newFile))
				{
					ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
					excelWorksheet.Cells[1, 1].Value = "Received Data";
					string[] lines = rx_textarea.Lines;
					int num = 1;
					int num2 = 2;
					string[] array = lines;
					foreach (string text in array)
					{
						string[] array2 = text.Split(' ');
						string[] array3 = array2;
						foreach (string text2 in array3)
						{
							if (text2.Contains("Beta"))
							{
								num++;
								num2 = 1;
							}
							if (double.TryParse(text2, out var result))
							{
								excelWorksheet.Cells[num2, num].Value = result;
								num2++;
							}
						}
					}
					excelPackage.Save();
				}
				MessageBox.Show("Data has been saved to " + filePath, "Save File");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
		}

		private void save_btn_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "Text Files (*.txt)|*.txt|Excel Files (*.xlsx)|*.xlsx|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
				saveFileDialog.FilterIndex = 1;
				saveFileDialog.RestoreDirectory = true;
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					string fileName = saveFileDialog.FileName;
					if (Path.GetExtension(fileName).Equals(".txt"))
					{
						File.WriteAllText(fileName, rx_textarea.Text);
					}
					else if (Path.GetExtension(fileName).Equals(".xlsx"))
					{
						SaveAsExcelEPPlus(fileName);
					}
					else if (Path.GetExtension(fileName).Equals(".png") && saveFileDialog1.ShowDialog() == DialogResult.OK)
					{
						graph.SaveImage(saveFileDialog1.FileName, ChartImageFormat.Png);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
		}

		private void plotter_option_group_Enter(object sender, EventArgs e)
		{
		}

		private void SendData()
		{
			if (btnclick && mySerial.IsOpen && mySerial.BytesToWrite == 0)
			{
				string text = "," + min_volt.Text + ",;" + max_volt.Text + ";?" + step_volt.Text + "?!" + max_current.Text + "!@" + dk_gate.Text + "@#" + max_gate.Text + "#&" + steps_gate.Text + "&" + Rb_gate.Text + "^";
				mySerial.Write(text);
				send_btn.BackColor = Color.Gray;
				btnclick = false;
			}
		}

		private void send_btn_Click(object sender, EventArgs e)
		{
			SendData();
		}

		private void dk_gate_TextChanged(object sender, EventArgs e)
		{
			if (!isUpdatingTextBox1)
			{
				dk_gateA.BackColor = Color.LightGray;
				dk_gate.BackColor = Color.White;
				isUpdatingTextBox1 = true;
				if (double.TryParse(dk_gate.Text, out var result) && double.TryParse(Rb_gate.Text, out var result2) && result2 != 0.0)
				{
					dk_gateA.Text = (result / result2).ToString("0.##");
				}
				isUpdatingTextBox1 = false;
			}
		}

		private void dk_gateA_TextChanged(object sender, EventArgs e)
		{
			if (!isUpdatingTextBox1)
			{
				dk_gate.BackColor = Color.LightGray;
				dk_gateA.BackColor = Color.White;
				isUpdatingTextBox1 = true;
				if (double.TryParse(dk_gateA.Text, out var result) && double.TryParse(Rb_gate.Text, out var result2) && result2 != 0.0)
				{
					dk_gate.Text = (result * result2).ToString("0.##");
				}
				isUpdatingTextBox1 = false;
			}
		}

		private void steps_gate_TextChanged(object sender, EventArgs e)
		{
			if (!isUpdatingTextBox2)
			{
				isUpdatingTextBox2 = true;
				if (double.TryParse(steps_gate.Text, out var result) && double.TryParse(Rb_gate.Text, out var result2) && result2 != 0.0)
				{
					stepsA_gate.Text = (result / result2).ToString("0.##");
				}
				isUpdatingTextBox2 = false;
			}
		}

		private void stepsA_gate_TextChanged(object sender, EventArgs e)
		{
			if (!isUpdatingTextBox2)
			{
				isUpdatingTextBox2 = true;
				if (double.TryParse(stepsA_gate.Text, out var result) && double.TryParse(Rb_gate.Text, out var result2) && result2 != 0.0)
				{
					steps_gate.Text = (result * result2).ToString("0.##");
				}
				isUpdatingTextBox2 = false;
			}
		}

		private void max_gate_TextChanged(object sender, EventArgs e)
		{
			if (!isUpdatingTextBox3)
			{
				isUpdatingTextBox3 = true;
				if (double.TryParse(max_gate.Text, out var result) && double.TryParse(Rb_gate.Text, out var result2) && result2 != 0.0)
				{
					maxA_gate.Text = (result / result2).ToString("0.##");
				}
				isUpdatingTextBox3 = false;
			}
		}

		private void textBox6_TextChanged(object sender, EventArgs e)
		{
			if (!isUpdatingTextBox3)
			{
				isUpdatingTextBox3 = true;
				if (double.TryParse(maxA_gate.Text, out var result) && double.TryParse(Rb_gate.Text, out var result2) && result2 != 0.0)
				{
					max_gate.Text = (result * result2).ToString("0.##");
				}
				isUpdatingTextBox3 = false;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (mySerial.IsOpen && !btnclick)
			{
				mySerial.Write("S");
				send_btn.BackColor = Color.LightGray;
				btnclick = true;
			}
		}

		private void DisableControl()
		{
			if (!Enabled)
			{
				ForeColor = SystemColors.ControlText;
			}
		}

		private void min_current_TextChanged(object sender, EventArgs e)
		{
		}

		private void label6_Click(object sender, EventArgs e)
		{
		}

		private void label11_Click(object sender, EventArgs e)
		{
		}

		private void statusStrip1_ItemClicked_1(object sender, ToolStripItemClickedEventArgs e)
		{
		}

		private void Rb_gate_TextChanged(object sender, EventArgs e)
		{
		}

		private void label16_Click(object sender, EventArgs e)
		{
		}

		private void graph_Click(object sender, EventArgs e)
		{
		}

		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void richTextBox2_TextChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void portConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void baudrateConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void databitsConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void stopbitsConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void parityConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void serial_options_group_Enter(object sender, EventArgs e)
		{
		}

		private void flowconrol_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void baudrate_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void flowcontrolConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void parity_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void port_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void stopbits_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void databits_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void byte_counter_label_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void toolStripStatusLabel1_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void Set_up_SelectedIndexChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void tabPage1_Click(object sender, EventArgs e)
		{
		}

		private void rx_textarea_TextChanged(object sender, EventArgs e)
		{
		}

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
		}

		private void read_options_group_Enter(object sender, EventArgs e)
		{
		}

		private void dsiplayas_options_panel_Paint(object sender, PaintEventArgs e)
		{
		}

		private void display_hex_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void display_string_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void label4_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void tabPage3_Click(object sender, EventArgs e)
		{
		}

		private void tx_terminal_TextChanged(object sender, EventArgs e)
		{
		}

		private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
		{
		}

		private void write_options_group_Enter(object sender, EventArgs e)
		{
		}

		private void tx_num_panel_Paint(object sender, PaintEventArgs e)
		{
		}

		private void send_delay_ValueChanged(object sender, EventArgs e)
		{
		}

		private void send_repeat_ValueChanged(object sender, EventArgs e)
		{
		}

		private void label1_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label2_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void tx_radiobuttons_panel_Paint(object sender, PaintEventArgs e)
		{
		}

		private void progressBar1_Click(object sender, EventArgs e)
		{
		}

		private void send_status_Click(object sender, EventArgs e)
		{
		}

		private void tx_textarea_TextChanged(object sender, EventArgs e)
		{
		}

		private void label3_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label7_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void graph_menu_Opening(object sender, CancelEventArgs e)
		{
		}

		private void groupBox1_Enter(object sender, EventArgs e)
		{
		}

		private void datalogger_options_panel_Paint(object sender, PaintEventArgs e)
		{
		}

		private void datalogger_overwrite_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void datalogger_append_radiobutton_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
		{
		}

		private void tx_repeater_delay_Tick(object sender, EventArgs e)
		{
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
		}

		private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
		{
		}

		private void alert_messege_MouseDoubleClick(object sender, MouseEventArgs e)
		{
		}

		private void label22_Click(object sender, EventArgs e)
		{
		}

		private void label23_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label21_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label17_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label12_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label13_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void max_current_TextChanged(object sender, EventArgs e)
		{
		}

		private void label9_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label8_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label5_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void max_volt_TextChanged(object sender, EventArgs e)
		{
		}

		private void min_volt_TextChanged(object sender, EventArgs e)
		{
		}

		private void textBox4_TextChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label14_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label10_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label15_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label19_Click_1(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label24_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label25_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		private void label26_Click(object sender, EventArgs e)
		{
			DisableControl();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TANACHAIarduino.MainForm));
			this.connect = new System.Windows.Forms.Button();
			this.portConfig = new System.Windows.Forms.ComboBox();
			this.baudrateConfig = new System.Windows.Forms.ComboBox();
			this.databitsConfig = new System.Windows.Forms.ComboBox();
			this.stopbitsConfig = new System.Windows.Forms.ComboBox();
			this.parityConfig = new System.Windows.Forms.ComboBox();
			this.mySerial = new System.IO.Ports.SerialPort(this.components);
			this.serial_options_group = new System.Windows.Forms.GroupBox();
			this.flowconrol_label = new System.Windows.Forms.Label();
			this.baudrate_label = new System.Windows.Forms.Label();
			this.flowcontrolConfig = new System.Windows.Forms.ComboBox();
			this.parity_label = new System.Windows.Forms.Label();
			this.port_label = new System.Windows.Forms.Label();
			this.stopbits_label = new System.Windows.Forms.Label();
			this.databits_label = new System.Windows.Forms.Label();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.byte_counter_label = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
			this.Set_up = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.rx_textarea = new System.Windows.Forms.RichTextBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.read_options_group = new System.Windows.Forms.GroupBox();
			this.dsiplayas_options_panel = new System.Windows.Forms.Panel();
			this.display_hex_radiobutton = new System.Windows.Forms.RadioButton();
			this.display_string_radiobutton = new System.Windows.Forms.RadioButton();
			this.label4 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tx_terminal = new System.Windows.Forms.RichTextBox();
			this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.write_options_group = new System.Windows.Forms.GroupBox();
			this.tx_num_panel = new System.Windows.Forms.Panel();
			this.send_delay = new System.Windows.Forms.NumericUpDown();
			this.send_repeat = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tx_radiobuttons_panel = new System.Windows.Forms.Panel();
			this.write_form_file_radiobutton = new System.Windows.Forms.RadioButton();
			this.key_capture_radiobutton = new System.Windows.Forms.RadioButton();
			this.send_word_radiobutton = new System.Windows.Forms.RadioButton();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.send_status = new System.Windows.Forms.Label();
			this.sendData = new System.Windows.Forms.Button();
			this.tx_textarea = new System.Windows.Forms.TextBox();
			this.plotter_option_group = new System.Windows.Forms.GroupBox();
			this.graph_max = new System.Windows.Forms.NumericUpDown();
			this.set_graph_min_enable = new System.Windows.Forms.CheckBox();
			this.graph_min = new System.Windows.Forms.NumericUpDown();
			this.set_graph_max_enable = new System.Windows.Forms.CheckBox();
			this.graph_scale = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.graph_speed = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.graph = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.graph_menu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.saveAsImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.datalogger_options_panel = new System.Windows.Forms.Panel();
			this.datalogger_overwrite_radiobutton = new System.Windows.Forms.RadioButton();
			this.datalogger_append_radiobutton = new System.Windows.Forms.RadioButton();
			this.datalogger_checkbox = new System.Windows.Forms.CheckBox();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.tx_repeater_delay = new System.Windows.Forms.Timer(this.components);
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.alert_messege = new System.Windows.Forms.NotifyIcon(this.components);
			this.save_btn = new System.Windows.Forms.Button();
			this.label22 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.Rb_gate = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.steps_gate = new System.Windows.Forms.TextBox();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.max_gate = new System.Windows.Forms.TextBox();
			this.dk_gate = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.max_current = new System.Windows.Forms.TextBox();
			this.step_volt = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.max_volt = new System.Windows.Forms.TextBox();
			this.min_volt = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.richTextBox2 = new System.Windows.Forms.RichTextBox();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.send_btn = new System.Windows.Forms.Button();
			this.label14 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.Stop_btn = new System.Windows.Forms.Button();
			this.label19 = new System.Windows.Forms.Label();
			this.label24 = new System.Windows.Forms.Label();
			this.label25 = new System.Windows.Forms.Label();
			this.dk_gateA = new System.Windows.Forms.TextBox();
			this.label26 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.maxA_gate = new System.Windows.Forms.TextBox();
			this.label28 = new System.Windows.Forms.Label();
			this.stepsA_gate = new System.Windows.Forms.TextBox();
			this.serial_options_group.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.Set_up.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			this.read_options_group.SuspendLayout();
			this.dsiplayas_options_panel.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.contextMenuStrip2.SuspendLayout();
			this.write_options_group.SuspendLayout();
			this.tx_num_panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.send_delay).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.send_repeat).BeginInit();
			this.tx_radiobuttons_panel.SuspendLayout();
			this.plotter_option_group.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.graph_max).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.graph_min).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.graph_scale).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.graph_speed).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.graph).BeginInit();
			this.graph_menu.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.datalogger_options_panel.SuspendLayout();
			base.SuspendLayout();
			this.connect.Location = new System.Drawing.Point(549, 139);
			this.connect.Margin = new System.Windows.Forms.Padding(4);
			this.connect.Name = "connect";
			this.connect.Size = new System.Drawing.Size(235, 49);
			this.connect.TabIndex = 0;
			this.connect.Text = "Connect";
			this.connect.UseVisualStyleBackColor = true;
			this.connect.Click += new System.EventHandler(this.connect_Click);
			this.portConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.portConfig.FormattingEnabled = true;
			this.portConfig.Location = new System.Drawing.Point(119, 42);
			this.portConfig.Margin = new System.Windows.Forms.Padding(4);
			this.portConfig.Name = "portConfig";
			this.portConfig.Size = new System.Drawing.Size(100, 24);
			this.portConfig.TabIndex = 1;
			this.portConfig.SelectedIndexChanged += new System.EventHandler(this.portConfig_SelectedIndexChanged);
			this.portConfig.Click += new System.EventHandler(this.portConfig_Click);
			this.baudrateConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.baudrateConfig.FormattingEnabled = true;
			this.baudrateConfig.Location = new System.Drawing.Point(119, 78);
			this.baudrateConfig.Margin = new System.Windows.Forms.Padding(4);
			this.baudrateConfig.Name = "baudrateConfig";
			this.baudrateConfig.Size = new System.Drawing.Size(99, 24);
			this.baudrateConfig.TabIndex = 2;
			this.baudrateConfig.SelectedIndexChanged += new System.EventHandler(this.baudrateConfig_SelectedIndexChanged);
			this.databitsConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.databitsConfig.FormattingEnabled = true;
			this.databitsConfig.Location = new System.Drawing.Point(119, 113);
			this.databitsConfig.Margin = new System.Windows.Forms.Padding(4);
			this.databitsConfig.Name = "databitsConfig";
			this.databitsConfig.Size = new System.Drawing.Size(100, 24);
			this.databitsConfig.TabIndex = 3;
			this.databitsConfig.SelectedIndexChanged += new System.EventHandler(this.databitsConfig_SelectedIndexChanged);
			this.stopbitsConfig.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
			this.stopbitsConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.stopbitsConfig.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.stopbitsConfig.FormattingEnabled = true;
			this.stopbitsConfig.Location = new System.Drawing.Point(371, 42);
			this.stopbitsConfig.Margin = new System.Windows.Forms.Padding(4);
			this.stopbitsConfig.Name = "stopbitsConfig";
			this.stopbitsConfig.Size = new System.Drawing.Size(97, 24);
			this.stopbitsConfig.TabIndex = 4;
			this.stopbitsConfig.SelectedIndexChanged += new System.EventHandler(this.stopbitsConfig_SelectedIndexChanged);
			this.parityConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.parityConfig.FormattingEnabled = true;
			this.parityConfig.Location = new System.Drawing.Point(371, 78);
			this.parityConfig.Margin = new System.Windows.Forms.Padding(4);
			this.parityConfig.Name = "parityConfig";
			this.parityConfig.Size = new System.Drawing.Size(97, 24);
			this.parityConfig.TabIndex = 5;
			this.parityConfig.SelectedIndexChanged += new System.EventHandler(this.parityConfig_SelectedIndexChanged);
			this.serial_options_group.Controls.Add(this.flowconrol_label);
			this.serial_options_group.Controls.Add(this.baudrate_label);
			this.serial_options_group.Controls.Add(this.flowcontrolConfig);
			this.serial_options_group.Controls.Add(this.parity_label);
			this.serial_options_group.Controls.Add(this.port_label);
			this.serial_options_group.Controls.Add(this.portConfig);
			this.serial_options_group.Controls.Add(this.baudrateConfig);
			this.serial_options_group.Controls.Add(this.parityConfig);
			this.serial_options_group.Controls.Add(this.stopbitsConfig);
			this.serial_options_group.Controls.Add(this.stopbits_label);
			this.serial_options_group.Controls.Add(this.databitsConfig);
			this.serial_options_group.Controls.Add(this.databits_label);
			this.serial_options_group.Location = new System.Drawing.Point(19, 25);
			this.serial_options_group.Margin = new System.Windows.Forms.Padding(4);
			this.serial_options_group.Name = "serial_options_group";
			this.serial_options_group.Padding = new System.Windows.Forms.Padding(4);
			this.serial_options_group.Size = new System.Drawing.Size(513, 164);
			this.serial_options_group.TabIndex = 9;
			this.serial_options_group.TabStop = false;
			this.serial_options_group.Text = "Serial port options";
			this.serial_options_group.Enter += new System.EventHandler(this.serial_options_group_Enter);
			this.flowconrol_label.AutoSize = true;
			this.flowconrol_label.ForeColor = System.Drawing.Color.Black;
			this.flowconrol_label.Location = new System.Drawing.Point(273, 116);
			this.flowconrol_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.flowconrol_label.Name = "flowconrol_label";
			this.flowconrol_label.Size = new System.Drawing.Size(83, 17);
			this.flowconrol_label.TabIndex = 13;
			this.flowconrol_label.Text = "Flow control";
			this.flowconrol_label.Click += new System.EventHandler(this.flowconrol_label_Click);
			this.baudrate_label.AutoSize = true;
			this.baudrate_label.ForeColor = System.Drawing.Color.Black;
			this.baudrate_label.Location = new System.Drawing.Point(35, 81);
			this.baudrate_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.baudrate_label.Name = "baudrate_label";
			this.baudrate_label.Size = new System.Drawing.Size(66, 17);
			this.baudrate_label.TabIndex = 8;
			this.baudrate_label.Text = "Baudrate";
			this.baudrate_label.Click += new System.EventHandler(this.baudrate_label_Click);
			this.flowcontrolConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.flowcontrolConfig.FormattingEnabled = true;
			this.flowcontrolConfig.Location = new System.Drawing.Point(371, 112);
			this.flowcontrolConfig.Margin = new System.Windows.Forms.Padding(4);
			this.flowcontrolConfig.Name = "flowcontrolConfig";
			this.flowcontrolConfig.Size = new System.Drawing.Size(97, 24);
			this.flowcontrolConfig.TabIndex = 12;
			this.flowcontrolConfig.SelectedIndexChanged += new System.EventHandler(this.flowcontrolConfig_SelectedIndexChanged);
			this.parity_label.AutoSize = true;
			this.parity_label.ForeColor = System.Drawing.Color.Black;
			this.parity_label.Location = new System.Drawing.Point(273, 81);
			this.parity_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.parity_label.Name = "parity_label";
			this.parity_label.Size = new System.Drawing.Size(44, 17);
			this.parity_label.TabIndex = 11;
			this.parity_label.Text = "Parity";
			this.parity_label.Click += new System.EventHandler(this.parity_label_Click);
			this.port_label.AutoSize = true;
			this.port_label.ForeColor = System.Drawing.Color.Black;
			this.port_label.Location = new System.Drawing.Point(37, 46);
			this.port_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.port_label.Name = "port_label";
			this.port_label.Size = new System.Drawing.Size(34, 17);
			this.port_label.TabIndex = 7;
			this.port_label.Text = "Port";
			this.port_label.Click += new System.EventHandler(this.port_label_Click);
			this.stopbits_label.AutoSize = true;
			this.stopbits_label.ForeColor = System.Drawing.Color.Black;
			this.stopbits_label.Location = new System.Drawing.Point(272, 46);
			this.stopbits_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.stopbits_label.Name = "stopbits_label";
			this.stopbits_label.Size = new System.Drawing.Size(63, 17);
			this.stopbits_label.TabIndex = 10;
			this.stopbits_label.Text = "Stop bits";
			this.stopbits_label.Click += new System.EventHandler(this.stopbits_label_Click);
			this.databits_label.AutoSize = true;
			this.databits_label.ForeColor = System.Drawing.Color.Black;
			this.databits_label.Location = new System.Drawing.Point(36, 117);
			this.databits_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.databits_label.Name = "databits_label";
			this.databits_label.Size = new System.Drawing.Size(64, 17);
			this.databits_label.TabIndex = 9;
			this.databits_label.Text = "Data bits";
			this.databits_label.Click += new System.EventHandler(this.databits_label_Click);
			this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.byte_counter_label, this.toolStripStatusLabel1, this.toolStripStatusLabel2 });
			this.statusStrip1.Location = new System.Drawing.Point(0, 904);
			this.statusStrip1.MinimumSize = new System.Drawing.Size(759, 31);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
			this.statusStrip1.Size = new System.Drawing.Size(1864, 31);
			this.statusStrip1.TabIndex = 11;
			this.statusStrip1.Text = "statusStrip1";
			this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked_1);
			this.byte_counter_label.Name = "byte_counter_label";
			this.byte_counter_label.Size = new System.Drawing.Size(0, 25);
			this.byte_counter_label.Click += new System.EventHandler(this.byte_counter_label_Click);
			this.toolStripStatusLabel1.BackColor = System.Drawing.Color.Transparent;
			this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 8f);
			this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(6, 3, 0, 2);
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(101, 26);
			this.toolStripStatusLabel1.Text = "No Connection";
			this.toolStripStatusLabel1.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
			this.toolStripStatusLabel2.BackColor = System.Drawing.Color.Transparent;
			this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
			this.toolStripStatusLabel2.Size = new System.Drawing.Size(50, 25);
			this.toolStripStatusLabel2.Text = "About";
			this.toolStripStatusLabel2.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
			this.Set_up.Controls.Add(this.tabPage1);
			this.Set_up.Controls.Add(this.tabPage3);
			this.Set_up.Location = new System.Drawing.Point(19, 197);
			this.Set_up.Margin = new System.Windows.Forms.Padding(4);
			this.Set_up.Name = "Set_up";
			this.Set_up.SelectedIndex = 0;
			this.Set_up.Size = new System.Drawing.Size(513, 693);
			this.Set_up.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
			this.Set_up.TabIndex = 14;
			this.Set_up.SelectedIndexChanged += new System.EventHandler(this.Set_up_SelectedIndexChanged);
			this.tabPage1.BackColor = System.Drawing.Color.White;
			this.tabPage1.Controls.Add(this.rx_textarea);
			this.tabPage1.Controls.Add(this.read_options_group);
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
			this.tabPage1.Size = new System.Drawing.Size(505, 664);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Read";
			this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
			this.rx_textarea.BackColor = System.Drawing.Color.White;
			this.rx_textarea.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.rx_textarea.ContextMenuStrip = this.contextMenuStrip1;
			this.rx_textarea.DetectUrls = false;
			this.rx_textarea.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.rx_textarea.HideSelection = false;
			this.rx_textarea.Location = new System.Drawing.Point(24, 111);
			this.rx_textarea.Margin = new System.Windows.Forms.Padding(4);
			this.rx_textarea.Name = "rx_textarea";
			this.rx_textarea.ReadOnly = true;
			this.rx_textarea.Size = new System.Drawing.Size(449, 532);
			this.rx_textarea.TabIndex = 29;
			this.rx_textarea.Text = "";
			this.rx_textarea.TextChanged += new System.EventHandler(this.rx_textarea_TextChanged);
			this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.clearToolStripMenuItem });
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(113, 28);
			this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
			this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
			this.clearToolStripMenuItem.Size = new System.Drawing.Size(112, 24);
			this.clearToolStripMenuItem.Text = "Clear";
			this.clearToolStripMenuItem.Click += new System.EventHandler(this.clear_rx_textarea_Click);
			this.read_options_group.Controls.Add(this.dsiplayas_options_panel);
			this.read_options_group.Controls.Add(this.label4);
			this.read_options_group.Location = new System.Drawing.Point(24, 20);
			this.read_options_group.Margin = new System.Windows.Forms.Padding(4);
			this.read_options_group.Name = "read_options_group";
			this.read_options_group.Padding = new System.Windows.Forms.Padding(4);
			this.read_options_group.Size = new System.Drawing.Size(449, 83);
			this.read_options_group.TabIndex = 26;
			this.read_options_group.TabStop = false;
			this.read_options_group.Text = "Read Options";
			this.read_options_group.Enter += new System.EventHandler(this.read_options_group_Enter);
			this.dsiplayas_options_panel.Controls.Add(this.display_hex_radiobutton);
			this.dsiplayas_options_panel.Controls.Add(this.display_string_radiobutton);
			this.dsiplayas_options_panel.Location = new System.Drawing.Point(111, 22);
			this.dsiplayas_options_panel.Margin = new System.Windows.Forms.Padding(4);
			this.dsiplayas_options_panel.Name = "dsiplayas_options_panel";
			this.dsiplayas_options_panel.Size = new System.Drawing.Size(302, 39);
			this.dsiplayas_options_panel.TabIndex = 18;
			this.dsiplayas_options_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.dsiplayas_options_panel_Paint);
			this.display_hex_radiobutton.AutoSize = true;
			this.display_hex_radiobutton.Location = new System.Drawing.Point(189, 8);
			this.display_hex_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.display_hex_radiobutton.Name = "display_hex_radiobutton";
			this.display_hex_radiobutton.Size = new System.Drawing.Size(57, 21);
			this.display_hex_radiobutton.TabIndex = 26;
			this.display_hex_radiobutton.Text = "HEX";
			this.display_hex_radiobutton.UseVisualStyleBackColor = true;
			this.display_hex_radiobutton.CheckedChanged += new System.EventHandler(this.display_hex_radiobutton_CheckedChanged);
			this.display_string_radiobutton.AutoSize = true;
			this.display_string_radiobutton.Checked = true;
			this.display_string_radiobutton.Location = new System.Drawing.Point(4, 6);
			this.display_string_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.display_string_radiobutton.Name = "display_string_radiobutton";
			this.display_string_radiobutton.Size = new System.Drawing.Size(66, 21);
			this.display_string_radiobutton.TabIndex = 25;
			this.display_string_radiobutton.TabStop = true;
			this.display_string_radiobutton.Text = "String";
			this.display_string_radiobutton.UseVisualStyleBackColor = true;
			this.display_string_radiobutton.CheckedChanged += new System.EventHandler(this.display_string_radiobutton_CheckedChanged);
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(28, 31);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 17);
			this.label4.TabIndex = 25;
			this.label4.Text = "Display as:";
			this.label4.Click += new System.EventHandler(this.label4_Click);
			this.tabPage3.Controls.Add(this.tx_terminal);
			this.tabPage3.Controls.Add(this.write_options_group);
			this.tabPage3.Location = new System.Drawing.Point(4, 25);
			this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(505, 664);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Write";
			this.tabPage3.UseVisualStyleBackColor = true;
			this.tabPage3.Click += new System.EventHandler(this.tabPage3_Click);
			this.tx_terminal.BackColor = System.Drawing.Color.White;
			this.tx_terminal.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tx_terminal.ContextMenuStrip = this.contextMenuStrip2;
			this.tx_terminal.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.tx_terminal.HideSelection = false;
			this.tx_terminal.Location = new System.Drawing.Point(24, 222);
			this.tx_terminal.Margin = new System.Windows.Forms.Padding(4);
			this.tx_terminal.Name = "tx_terminal";
			this.tx_terminal.ReadOnly = true;
			this.tx_terminal.Size = new System.Drawing.Size(469, 425);
			this.tx_terminal.TabIndex = 30;
			this.tx_terminal.Text = "";
			this.tx_terminal.TextChanged += new System.EventHandler(this.tx_terminal_TextChanged);
			this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.toolStripMenuItem1 });
			this.contextMenuStrip2.Name = "contextMenuStrip1";
			this.contextMenuStrip2.Size = new System.Drawing.Size(113, 28);
			this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(112, 24);
			this.toolStripMenuItem1.Text = "Clear";
			this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
			this.write_options_group.Controls.Add(this.tx_num_panel);
			this.write_options_group.Controls.Add(this.tx_radiobuttons_panel);
			this.write_options_group.Controls.Add(this.progressBar1);
			this.write_options_group.Controls.Add(this.send_status);
			this.write_options_group.Controls.Add(this.sendData);
			this.write_options_group.Controls.Add(this.tx_textarea);
			this.write_options_group.Enabled = false;
			this.write_options_group.Location = new System.Drawing.Point(24, 0);
			this.write_options_group.Margin = new System.Windows.Forms.Padding(4);
			this.write_options_group.Name = "write_options_group";
			this.write_options_group.Padding = new System.Windows.Forms.Padding(4);
			this.write_options_group.Size = new System.Drawing.Size(469, 205);
			this.write_options_group.TabIndex = 25;
			this.write_options_group.TabStop = false;
			this.write_options_group.Text = "Write Options";
			this.write_options_group.Enter += new System.EventHandler(this.write_options_group_Enter);
			this.tx_num_panel.Controls.Add(this.send_delay);
			this.tx_num_panel.Controls.Add(this.send_repeat);
			this.tx_num_panel.Controls.Add(this.label1);
			this.tx_num_panel.Controls.Add(this.label2);
			this.tx_num_panel.ForeColor = System.Drawing.Color.Black;
			this.tx_num_panel.Location = new System.Drawing.Point(15, 123);
			this.tx_num_panel.Margin = new System.Windows.Forms.Padding(4);
			this.tx_num_panel.Name = "tx_num_panel";
			this.tx_num_panel.Size = new System.Drawing.Size(368, 33);
			this.tx_num_panel.TabIndex = 31;
			this.tx_num_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.tx_num_panel_Paint);
			this.send_delay.Location = new System.Drawing.Point(252, 5);
			this.send_delay.Margin = new System.Windows.Forms.Padding(4);
			this.send_delay.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
			this.send_delay.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
			this.send_delay.Name = "send_delay";
			this.send_delay.Size = new System.Drawing.Size(71, 22);
			this.send_delay.TabIndex = 21;
			this.send_delay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.send_delay.Value = new decimal(new int[4] { 1, 0, 0, 0 });
			this.send_delay.ValueChanged += new System.EventHandler(this.send_delay_ValueChanged);
			this.send_repeat.Location = new System.Drawing.Point(72, 5);
			this.send_repeat.Margin = new System.Windows.Forms.Padding(4);
			this.send_repeat.Maximum = new decimal(new int[4] { 1000000, 0, 0, 0 });
			this.send_repeat.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
			this.send_repeat.Name = "send_repeat";
			this.send_repeat.Size = new System.Drawing.Size(71, 22);
			this.send_repeat.TabIndex = 22;
			this.send_repeat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.send_repeat.Value = new decimal(new int[4] { 1, 0, 0, 0 });
			this.send_repeat.ValueChanged += new System.EventHandler(this.send_repeat_ValueChanged);
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1, 9);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 17);
			this.label1.TabIndex = 23;
			this.label1.Text = "Repeats:";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(168, 9);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 17);
			this.label2.TabIndex = 24;
			this.label2.Text = "Delay (ms):";
			this.label2.Click += new System.EventHandler(this.label2_Click);
			this.tx_radiobuttons_panel.Controls.Add(this.write_form_file_radiobutton);
			this.tx_radiobuttons_panel.Controls.Add(this.key_capture_radiobutton);
			this.tx_radiobuttons_panel.Controls.Add(this.send_word_radiobutton);
			this.tx_radiobuttons_panel.ForeColor = System.Drawing.Color.Black;
			this.tx_radiobuttons_panel.Location = new System.Drawing.Point(15, 34);
			this.tx_radiobuttons_panel.Margin = new System.Windows.Forms.Padding(4);
			this.tx_radiobuttons_panel.Name = "tx_radiobuttons_panel";
			this.tx_radiobuttons_panel.Size = new System.Drawing.Size(368, 26);
			this.tx_radiobuttons_panel.TabIndex = 27;
			this.tx_radiobuttons_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.tx_radiobuttons_panel_Paint);
			this.write_form_file_radiobutton.AutoSize = true;
			this.write_form_file_radiobutton.Location = new System.Drawing.Point(261, 0);
			this.write_form_file_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.write_form_file_radiobutton.Name = "write_form_file_radiobutton";
			this.write_form_file_radiobutton.Size = new System.Drawing.Size(83, 21);
			this.write_form_file_radiobutton.TabIndex = 20;
			this.write_form_file_radiobutton.Text = "From file";
			this.write_form_file_radiobutton.UseVisualStyleBackColor = true;
			this.write_form_file_radiobutton.CheckedChanged += new System.EventHandler(this.write_form_file_radiobutton_CheckedChanged);
			this.key_capture_radiobutton.AutoSize = true;
			this.key_capture_radiobutton.Location = new System.Drawing.Point(131, 0);
			this.key_capture_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.key_capture_radiobutton.Name = "key_capture_radiobutton";
			this.key_capture_radiobutton.Size = new System.Drawing.Size(105, 21);
			this.key_capture_radiobutton.TabIndex = 19;
			this.key_capture_radiobutton.Text = "Key capture";
			this.key_capture_radiobutton.UseVisualStyleBackColor = true;
			this.key_capture_radiobutton.CheckedChanged += new System.EventHandler(this.key_capture_radiobutton_CheckedChanged);
			this.send_word_radiobutton.AutoSize = true;
			this.send_word_radiobutton.Checked = true;
			this.send_word_radiobutton.Location = new System.Drawing.Point(0, 0);
			this.send_word_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.send_word_radiobutton.Name = "send_word_radiobutton";
			this.send_word_radiobutton.Size = new System.Drawing.Size(96, 21);
			this.send_word_radiobutton.TabIndex = 18;
			this.send_word_radiobutton.TabStop = true;
			this.send_word_radiobutton.Text = "Send word";
			this.send_word_radiobutton.UseVisualStyleBackColor = true;
			this.send_word_radiobutton.CheckedChanged += new System.EventHandler(this.send_word_radiobutton_CheckedChanged);
			this.progressBar1.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.progressBar1.Cursor = System.Windows.Forms.Cursors.Default;
			this.progressBar1.ForeColor = System.Drawing.Color.Black;
			this.progressBar1.Location = new System.Drawing.Point(15, 106);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(368, 13);
			this.progressBar1.Step = 1;
			this.progressBar1.TabIndex = 26;
			this.progressBar1.Visible = false;
			this.progressBar1.Click += new System.EventHandler(this.progressBar1_Click);
			this.send_status.AutoSize = true;
			this.send_status.Location = new System.Drawing.Point(459, 133);
			this.send_status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.send_status.Name = "send_status";
			this.send_status.Size = new System.Drawing.Size(0, 17);
			this.send_status.TabIndex = 25;
			this.send_status.Click += new System.EventHandler(this.send_status_Click);
			this.sendData.ForeColor = System.Drawing.Color.Black;
			this.sendData.Location = new System.Drawing.Point(391, 34);
			this.sendData.Margin = new System.Windows.Forms.Padding(4);
			this.sendData.Name = "sendData";
			this.sendData.Size = new System.Drawing.Size(65, 135);
			this.sendData.TabIndex = 13;
			this.sendData.Text = "Send";
			this.sendData.UseVisualStyleBackColor = true;
			this.sendData.Click += new System.EventHandler(this.sendData_Click);
			this.tx_textarea.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
			this.tx_textarea.Font = new System.Drawing.Font("Courier New", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.tx_textarea.ForeColor = System.Drawing.Color.Black;
			this.tx_textarea.Location = new System.Drawing.Point(15, 76);
			this.tx_textarea.Margin = new System.Windows.Forms.Padding(4);
			this.tx_textarea.Name = "tx_textarea";
			this.tx_textarea.Size = new System.Drawing.Size(368, 29);
			this.tx_textarea.TabIndex = 14;
			this.tx_textarea.Click += new System.EventHandler(this.tx_textarea_Click);
			this.tx_textarea.TextChanged += new System.EventHandler(this.tx_textarea_TextChanged);
			this.tx_textarea.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tx_textarea_KeyPress);
			this.plotter_option_group.Controls.Add(this.graph_max);
			this.plotter_option_group.Controls.Add(this.set_graph_min_enable);
			this.plotter_option_group.Controls.Add(this.graph_min);
			this.plotter_option_group.Controls.Add(this.set_graph_max_enable);
			this.plotter_option_group.Controls.Add(this.graph_scale);
			this.plotter_option_group.Controls.Add(this.label3);
			this.plotter_option_group.Controls.Add(this.graph_speed);
			this.plotter_option_group.Controls.Add(this.label7);
			this.plotter_option_group.Location = new System.Drawing.Point(1058, 811);
			this.plotter_option_group.Margin = new System.Windows.Forms.Padding(4);
			this.plotter_option_group.Name = "plotter_option_group";
			this.plotter_option_group.Padding = new System.Windows.Forms.Padding(4);
			this.plotter_option_group.Size = new System.Drawing.Size(773, 79);
			this.plotter_option_group.TabIndex = 25;
			this.plotter_option_group.TabStop = false;
			this.plotter_option_group.Text = "Plotter options";
			this.plotter_option_group.Enter += new System.EventHandler(this.plotter_option_group_Enter);
			this.graph_max.AutoSize = true;
			this.graph_max.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.graph_max.Enabled = false;
			this.graph_max.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.graph_max.Location = new System.Drawing.Point(113, 32);
			this.graph_max.Margin = new System.Windows.Forms.Padding(4);
			this.graph_max.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
			this.graph_max.Minimum = new decimal(new int[4] { 100000, 0, 0, -2147483648 });
			this.graph_max.Name = "graph_max";
			this.graph_max.Size = new System.Drawing.Size(101, 22);
			this.graph_max.TabIndex = 28;
			this.graph_max.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.graph_max.Value = new decimal(new int[4] { 1, 0, 0, -2147483648 });
			this.graph_max.ValueChanged += new System.EventHandler(this.graph_max_ValueChanged);
			this.set_graph_min_enable.AutoSize = true;
			this.set_graph_min_enable.Location = new System.Drawing.Point(226, 36);
			this.set_graph_min_enable.Margin = new System.Windows.Forms.Padding(4);
			this.set_graph_min_enable.Name = "set_graph_min_enable";
			this.set_graph_min_enable.Size = new System.Drawing.Size(81, 21);
			this.set_graph_min_enable.TabIndex = 27;
			this.set_graph_min_enable.Text = "Set Min:";
			this.set_graph_min_enable.UseVisualStyleBackColor = true;
			this.set_graph_min_enable.CheckedChanged += new System.EventHandler(this.set_graph_min_enable_CheckedChanged);
			this.graph_min.AutoSize = true;
			this.graph_min.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.graph_min.Enabled = false;
			this.graph_min.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.graph_min.Location = new System.Drawing.Point(315, 35);
			this.graph_min.Margin = new System.Windows.Forms.Padding(4);
			this.graph_min.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
			this.graph_min.Minimum = new decimal(new int[4] { 100000, 0, 0, -2147483648 });
			this.graph_min.Name = "graph_min";
			this.graph_min.Size = new System.Drawing.Size(101, 22);
			this.graph_min.TabIndex = 26;
			this.graph_min.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.graph_min.Value = new decimal(new int[4] { 1, 0, 0, -2147483648 });
			this.graph_min.ValueChanged += new System.EventHandler(this.graph_min_ValueChanged);
			this.set_graph_max_enable.AutoSize = true;
			this.set_graph_max_enable.Location = new System.Drawing.Point(28, 34);
			this.set_graph_max_enable.Margin = new System.Windows.Forms.Padding(4);
			this.set_graph_max_enable.Name = "set_graph_max_enable";
			this.set_graph_max_enable.Size = new System.Drawing.Size(84, 21);
			this.set_graph_max_enable.TabIndex = 25;
			this.set_graph_max_enable.Text = "Set Max:";
			this.set_graph_max_enable.UseVisualStyleBackColor = true;
			this.set_graph_max_enable.CheckedChanged += new System.EventHandler(this.set_graph_max_enable_CheckedChanged);
			this.graph_scale.AutoSize = true;
			this.graph_scale.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.graph_scale.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.graph_scale.Increment = new decimal(new int[4] { 100, 0, 0, 0 });
			this.graph_scale.Location = new System.Drawing.Point(494, 36);
			this.graph_scale.Margin = new System.Windows.Forms.Padding(4);
			this.graph_scale.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
			this.graph_scale.Minimum = new decimal(new int[4] { 100, 0, 0, 0 });
			this.graph_scale.Name = "graph_scale";
			this.graph_scale.Size = new System.Drawing.Size(91, 22);
			this.graph_scale.TabIndex = 19;
			this.graph_scale.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.graph_scale.Value = new decimal(new int[4] { 500, 0, 0, 0 });
			this.graph_scale.ValueChanged += new System.EventHandler(this.graph_scale_ValueChanged);
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.label3.Location = new System.Drawing.Point(424, 37);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(61, 21);
			this.label3.TabIndex = 24;
			this.label3.Text = "Points:";
			this.label3.Click += new System.EventHandler(this.label3_Click);
			this.graph_speed.AllowDrop = true;
			this.graph_speed.AutoSize = true;
			this.graph_speed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.graph_speed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.graph_speed.Location = new System.Drawing.Point(674, 36);
			this.graph_speed.Margin = new System.Windows.Forms.Padding(4);
			this.graph_speed.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
			this.graph_speed.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
			this.graph_speed.Name = "graph_speed";
			this.graph_speed.Size = new System.Drawing.Size(91, 22);
			this.graph_speed.TabIndex = 17;
			this.graph_speed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.graph_speed.Value = new decimal(new int[4] { 1, 0, 0, 0 });
			this.graph_speed.ValueChanged += new System.EventHandler(this.graph_speed_ValueChanged);
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Tahoma", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.label7.Location = new System.Drawing.Point(593, 36);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(73, 21);
			this.label7.TabIndex = 18;
			this.label7.Text = "Interval:";
			this.label7.Click += new System.EventHandler(this.label7_Click);
			this.graph.BackColor = System.Drawing.SystemColors.HighlightText;
			this.graph.BorderlineColor = System.Drawing.SystemColors.Window;
			this.graph.BorderlineWidth = 0;
			chartArea.AlignmentOrientation = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.None;
			chartArea.AxisX.IsMarginVisible = false;
			chartArea.AxisX.LabelStyle.Enabled = false;
			chartArea.AxisX.LineColor = System.Drawing.Color.FromArgb(230, 230, 230);
			chartArea.AxisX.MajorGrid.Interval = 0.0;
			chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
			chartArea.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
			chartArea.AxisX.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.InsideArea;
			chartArea.AxisX.MinorGrid.Enabled = true;
			chartArea.AxisX.MinorGrid.LineColor = System.Drawing.Color.FromArgb(224, 224, 224);
			chartArea.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
			chartArea.AxisY.LineColor = System.Drawing.Color.FromArgb(190, 190, 190);
			chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
			chartArea.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
			chartArea.AxisY.MinorGrid.Enabled = true;
			chartArea.AxisY.MinorGrid.LineColor = System.Drawing.Color.FromArgb(224, 224, 224);
			chartArea.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
			chartArea.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
			chartArea.BorderColor = System.Drawing.Color.Maroon;
			chartArea.BorderWidth = 0;
			chartArea.Name = "ChartArea1";
			this.graph.ChartAreas.Add(chartArea);
			this.graph.ContextMenuStrip = this.graph_menu;
			legend.Alignment = System.Drawing.StringAlignment.Center;
			legend.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
			legend.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Row;
			legend.MaximumAutoSize = 20f;
			legend.Name = "Legend1";
			legend.TitleAlignment = System.Drawing.StringAlignment.Near;
			legend.TitleFont = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.graph.Legends.Add(legend);
			this.graph.Location = new System.Drawing.Point(820, 25);
			this.graph.Margin = new System.Windows.Forms.Padding(4);
			this.graph.Name = "graph";
			this.graph.Size = new System.Drawing.Size(1036, 757);
			this.graph.TabIndex = 16;
			this.graph.Text = "chart2";
			this.graph.Click += new System.EventHandler(this.graph_Click);
			this.graph_menu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.graph_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.saveAsImageToolStripMenuItem, this.clearToolStripMenuItem1 });
			this.graph_menu.Name = "clear_graph";
			this.graph_menu.Size = new System.Drawing.Size(174, 52);
			this.graph_menu.Opening += new System.ComponentModel.CancelEventHandler(this.graph_menu_Opening);
			this.saveAsImageToolStripMenuItem.Name = "saveAsImageToolStripMenuItem";
			this.saveAsImageToolStripMenuItem.Size = new System.Drawing.Size(173, 24);
			this.saveAsImageToolStripMenuItem.Text = "Save as image";
			this.saveAsImageToolStripMenuItem.Click += new System.EventHandler(this.saveAsImageToolStripMenuItem_Click);
			this.clearToolStripMenuItem1.Name = "clearToolStripMenuItem1";
			this.clearToolStripMenuItem1.Size = new System.Drawing.Size(173, 24);
			this.clearToolStripMenuItem1.Text = "Clear";
			this.clearToolStripMenuItem1.Click += new System.EventHandler(this.clear_graph_Click);
			this.groupBox1.Controls.Add(this.datalogger_options_panel);
			this.groupBox1.Location = new System.Drawing.Point(549, 25);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
			this.groupBox1.Size = new System.Drawing.Size(235, 107);
			this.groupBox1.TabIndex = 27;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Data logger";
			this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
			this.datalogger_options_panel.Controls.Add(this.datalogger_overwrite_radiobutton);
			this.datalogger_options_panel.Controls.Add(this.datalogger_append_radiobutton);
			this.datalogger_options_panel.Controls.Add(this.datalogger_checkbox);
			this.datalogger_options_panel.Location = new System.Drawing.Point(12, 31);
			this.datalogger_options_panel.Margin = new System.Windows.Forms.Padding(4);
			this.datalogger_options_panel.Name = "datalogger_options_panel";
			this.datalogger_options_panel.Size = new System.Drawing.Size(205, 66);
			this.datalogger_options_panel.TabIndex = 28;
			this.datalogger_options_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.datalogger_options_panel_Paint);
			this.datalogger_overwrite_radiobutton.AutoSize = true;
			this.datalogger_overwrite_radiobutton.Enabled = false;
			this.datalogger_overwrite_radiobutton.Location = new System.Drawing.Point(99, 37);
			this.datalogger_overwrite_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.datalogger_overwrite_radiobutton.Name = "datalogger_overwrite_radiobutton";
			this.datalogger_overwrite_radiobutton.Size = new System.Drawing.Size(89, 21);
			this.datalogger_overwrite_radiobutton.TabIndex = 29;
			this.datalogger_overwrite_radiobutton.Text = "Overwrite";
			this.datalogger_overwrite_radiobutton.UseVisualStyleBackColor = true;
			this.datalogger_overwrite_radiobutton.CheckedChanged += new System.EventHandler(this.datalogger_overwrite_radiobutton_CheckedChanged);
			this.datalogger_append_radiobutton.AutoSize = true;
			this.datalogger_append_radiobutton.Checked = true;
			this.datalogger_append_radiobutton.Enabled = false;
			this.datalogger_append_radiobutton.Location = new System.Drawing.Point(8, 37);
			this.datalogger_append_radiobutton.Margin = new System.Windows.Forms.Padding(4);
			this.datalogger_append_radiobutton.Name = "datalogger_append_radiobutton";
			this.datalogger_append_radiobutton.Size = new System.Drawing.Size(78, 21);
			this.datalogger_append_radiobutton.TabIndex = 28;
			this.datalogger_append_radiobutton.TabStop = true;
			this.datalogger_append_radiobutton.Text = "Append";
			this.datalogger_append_radiobutton.UseVisualStyleBackColor = true;
			this.datalogger_append_radiobutton.CheckedChanged += new System.EventHandler(this.datalogger_append_radiobutton_CheckedChanged);
			this.datalogger_checkbox.AutoSize = true;
			this.datalogger_checkbox.BackColor = System.Drawing.Color.Transparent;
			this.datalogger_checkbox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.datalogger_checkbox.Cursor = System.Windows.Forms.Cursors.Default;
			this.datalogger_checkbox.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.datalogger_checkbox.Location = new System.Drawing.Point(8, 4);
			this.datalogger_checkbox.Margin = new System.Windows.Forms.Padding(4);
			this.datalogger_checkbox.MaximumSize = new System.Drawing.Size(533, 23);
			this.datalogger_checkbox.Name = "datalogger_checkbox";
			this.datalogger_checkbox.Size = new System.Drawing.Size(160, 23);
			this.datalogger_checkbox.TabIndex = 15;
			this.datalogger_checkbox.Text = "Enable Data logger";
			this.datalogger_checkbox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.datalogger_checkbox.UseVisualStyleBackColor = false;
			this.datalogger_checkbox.CheckedChanged += new System.EventHandler(this.datalogger_checkbox_CheckedChanged);
			this.openFileDialog1.DefaultExt = "txt";
			this.openFileDialog1.FileName = "log_file";
			this.openFileDialog1.RestoreDirectory = true;
			this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
			this.tx_repeater_delay.Tick += new System.EventHandler(this.tx_repeater_delay_Tick);
			this.backgroundWorker1.WorkerSupportsCancellation = true;
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.saveFileDialog1.DefaultExt = "png";
			this.saveFileDialog1.FileName = "figure";
			this.saveFileDialog1.Filter = "PNG|.png| JPEG|.jpg";
			this.saveFileDialog1.RestoreDirectory = true;
			this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
			this.alert_messege.Text = "notifyIcon1";
			this.alert_messege.Visible = true;
			this.alert_messege.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.alert_messege_MouseDoubleClick);
			this.save_btn.Location = new System.Drawing.Point(880, 815);
			this.save_btn.Name = "save_btn";
			this.save_btn.Size = new System.Drawing.Size(171, 75);
			this.save_btn.TabIndex = 28;
			this.save_btn.Text = "Save";
			this.save_btn.UseVisualStyleBackColor = true;
			this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
			this.label22.AutoSize = true;
			this.label22.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label22.ForeColor = System.Drawing.Color.Black;
			this.label22.Location = new System.Drawing.Point(710, 744);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(56, 21);
			this.label22.TabIndex = 63;
			this.label22.Text = "kOhm";
			this.label22.Click += new System.EventHandler(this.label22_Click);
			this.label23.AutoSize = true;
			this.label23.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label23.ForeColor = System.Drawing.Color.Black;
			this.label23.Location = new System.Drawing.Point(594, 746);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(30, 17);
			this.label23.TabIndex = 62;
			this.label23.Text = "Rb:";
			this.label23.Click += new System.EventHandler(this.label23_Click);
			this.Rb_gate.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.Rb_gate.Enabled = false;
			this.Rb_gate.ForeColor = System.Drawing.Color.Black;
			this.Rb_gate.Location = new System.Drawing.Point(630, 741);
			this.Rb_gate.Name = "Rb_gate";
			this.Rb_gate.Size = new System.Drawing.Size(74, 22);
			this.Rb_gate.TabIndex = 61;
			this.Rb_gate.Text = "27";
			this.Rb_gate.TextChanged += new System.EventHandler(this.Rb_gate_TextChanged);
			this.label21.AutoSize = true;
			this.label21.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label21.ForeColor = System.Drawing.Color.Black;
			this.label21.Location = new System.Drawing.Point(575, 656);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(51, 17);
			this.label21.TabIndex = 57;
			this.label21.Text = "StepsI:";
			this.label21.Click += new System.EventHandler(this.label21_Click);
			this.steps_gate.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.steps_gate.ForeColor = System.Drawing.Color.Black;
			this.steps_gate.Location = new System.Drawing.Point(631, 653);
			this.steps_gate.Name = "steps_gate";
			this.steps_gate.Size = new System.Drawing.Size(74, 22);
			this.steps_gate.TabIndex = 55;
			this.steps_gate.Text = "0";
			this.steps_gate.TextChanged += new System.EventHandler(this.steps_gate_TextChanged);
			this.label16.AutoSize = true;
			this.label16.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label16.ForeColor = System.Drawing.Color.Black;
			this.label16.Location = new System.Drawing.Point(582, 581);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(41, 17);
			this.label16.TabIndex = 54;
			this.label16.Text = "Max.:";
			this.label16.Click += new System.EventHandler(this.label16_Click);
			this.label17.AutoSize = true;
			this.label17.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label17.ForeColor = System.Drawing.Color.Black;
			this.label17.Location = new System.Drawing.Point(585, 510);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(38, 17);
			this.label17.TabIndex = 53;
			this.label17.Text = "Min.:";
			this.label17.Click += new System.EventHandler(this.label17_Click);
			this.max_gate.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.max_gate.ForeColor = System.Drawing.Color.Black;
			this.max_gate.Location = new System.Drawing.Point(630, 578);
			this.max_gate.Name = "max_gate";
			this.max_gate.Size = new System.Drawing.Size(74, 22);
			this.max_gate.TabIndex = 52;
			this.max_gate.Text = "0";
			this.max_gate.TextChanged += new System.EventHandler(this.max_gate_TextChanged);
			this.dk_gate.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.dk_gate.ForeColor = System.Drawing.Color.Black;
			this.dk_gate.Location = new System.Drawing.Point(631, 505);
			this.dk_gate.Name = "dk_gate";
			this.dk_gate.Size = new System.Drawing.Size(74, 22);
			this.dk_gate.TabIndex = 51;
			this.dk_gate.Text = "0";
			this.dk_gate.TextChanged += new System.EventHandler(this.dk_gate_TextChanged);
			this.label11.AutoSize = true;
			this.label11.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label11.ForeColor = System.Drawing.Color.Black;
			this.label11.Location = new System.Drawing.Point(711, 454);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(32, 17);
			this.label11.TabIndex = 49;
			this.label11.Text = "mW";
			this.label11.Click += new System.EventHandler(this.label11_Click);
			this.label12.AutoSize = true;
			this.label12.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label12.ForeColor = System.Drawing.Color.Black;
			this.label12.Location = new System.Drawing.Point(584, 451);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(41, 17);
			this.label12.TabIndex = 48;
			this.label12.Text = "Max.:";
			this.label12.Click += new System.EventHandler(this.label12_Click);
			this.label13.AutoSize = true;
			this.label13.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label13.ForeColor = System.Drawing.Color.Black;
			this.label13.Location = new System.Drawing.Point(569, 397);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(51, 17);
			this.label13.TabIndex = 47;
			this.label13.Text = "StepU:";
			this.label13.Click += new System.EventHandler(this.label13_Click);
			this.max_current.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.max_current.ForeColor = System.Drawing.Color.Black;
			this.max_current.Location = new System.Drawing.Point(631, 451);
			this.max_current.Name = "max_current";
			this.max_current.Size = new System.Drawing.Size(74, 22);
			this.max_current.TabIndex = 46;
			this.max_current.TextChanged += new System.EventHandler(this.max_current_TextChanged);
			this.step_volt.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.step_volt.ForeColor = System.Drawing.Color.Black;
			this.step_volt.Location = new System.Drawing.Point(630, 397);
			this.step_volt.Name = "step_volt";
			this.step_volt.Size = new System.Drawing.Size(74, 22);
			this.step_volt.TabIndex = 45;
			this.step_volt.TextChanged += new System.EventHandler(this.min_current_TextChanged);
			this.label9.AutoSize = true;
			this.label9.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label9.ForeColor = System.Drawing.Color.Black;
			this.label9.Location = new System.Drawing.Point(711, 372);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(43, 17);
			this.label9.TabIndex = 44;
			this.label9.Text = "mVolt";
			this.label9.Click += new System.EventHandler(this.label9_Click);
			this.label8.AutoSize = true;
			this.label8.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label8.ForeColor = System.Drawing.Color.Black;
			this.label8.Location = new System.Drawing.Point(711, 341);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(43, 17);
			this.label8.TabIndex = 43;
			this.label8.Text = "mVolt";
			this.label8.Click += new System.EventHandler(this.label8_Click);
			this.label6.AutoSize = true;
			this.label6.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label6.ForeColor = System.Drawing.Color.Black;
			this.label6.Location = new System.Drawing.Point(584, 369);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(41, 17);
			this.label6.TabIndex = 42;
			this.label6.Text = "Max.:";
			this.label6.Click += new System.EventHandler(this.label6_Click);
			this.label5.AutoSize = true;
			this.label5.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label5.ForeColor = System.Drawing.Color.Black;
			this.label5.Location = new System.Drawing.Point(587, 341);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(38, 17);
			this.label5.TabIndex = 41;
			this.label5.Text = "Min.:";
			this.label5.Click += new System.EventHandler(this.label5_Click);
			this.max_volt.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.max_volt.ForeColor = System.Drawing.Color.Black;
			this.max_volt.Location = new System.Drawing.Point(631, 369);
			this.max_volt.Name = "max_volt";
			this.max_volt.Size = new System.Drawing.Size(74, 22);
			this.max_volt.TabIndex = 40;
			this.max_volt.TextChanged += new System.EventHandler(this.max_volt_TextChanged);
			this.min_volt.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.min_volt.ForeColor = System.Drawing.Color.Black;
			this.min_volt.Location = new System.Drawing.Point(631, 341);
			this.min_volt.Name = "min_volt";
			this.min_volt.Size = new System.Drawing.Size(74, 22);
			this.min_volt.TabIndex = 39;
			this.min_volt.TextChanged += new System.EventHandler(this.min_volt_TextChanged);
			this.textBox4.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 238);
			this.textBox4.ForeColor = System.Drawing.Color.Black;
			this.textBox4.Location = new System.Drawing.Point(611, 479);
			this.textBox4.Name = "textBox4";
			this.textBox4.ReadOnly = true;
			this.textBox4.Size = new System.Drawing.Size(125, 20);
			this.textBox4.TabIndex = 38;
			this.textBox4.Text = "BASE/GATE";
			this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			this.textBox3.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 238);
			this.textBox3.ForeColor = System.Drawing.Color.Black;
			this.textBox3.Location = new System.Drawing.Point(617, 425);
			this.textBox3.Name = "textBox3";
			this.textBox3.ReadOnly = true;
			this.textBox3.Size = new System.Drawing.Size(112, 20);
			this.textBox3.TabIndex = 37;
			this.textBox3.Text = "CURRENT";
			this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
			this.textBox2.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 238);
			this.textBox2.ForeColor = System.Drawing.Color.Black;
			this.textBox2.Location = new System.Drawing.Point(621, 309);
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.Size = new System.Drawing.Size(93, 20);
			this.textBox2.TabIndex = 36;
			this.textBox2.Text = "VOLTAGE";
			this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
			this.textBox1.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 238);
			this.textBox1.ForeColor = System.Drawing.Color.Black;
			this.textBox1.Location = new System.Drawing.Point(588, 195);
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(161, 20);
			this.textBox1.TabIndex = 35;
			this.textBox1.Text = "CONNECTIONS";
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox2.ForeColor = System.Drawing.Color.Black;
			this.richTextBox2.Location = new System.Drawing.Point(680, 221);
			this.richTextBox2.Name = "richTextBox2";
			this.richTextBox2.Size = new System.Drawing.Size(133, 82);
			this.richTextBox2.TabIndex = 34;
			this.richTextBox2.Text = " 3 PORT DEVICES\n0: GND\n1: DRAIN\n2: BASE/GATE\n3: SOURCE";
			this.richTextBox2.TextChanged += new System.EventHandler(this.richTextBox2_TextChanged);
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.ForeColor = System.Drawing.Color.Black;
			this.richTextBox1.Location = new System.Drawing.Point(545, 221);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(144, 81);
			this.richTextBox1.TabIndex = 33;
			this.richTextBox1.Text = " 2 PORT DEVICES\n0: GND\n1: CATHODE  \n2: N.C\n3: ANODE";
			this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
			this.send_btn.Location = new System.Drawing.Point(549, 788);
			this.send_btn.Name = "send_btn";
			this.send_btn.Size = new System.Drawing.Size(235, 49);
			this.send_btn.TabIndex = 64;
			this.send_btn.Text = "Send ARDUINO";
			this.send_btn.UseVisualStyleBackColor = true;
			this.send_btn.Click += new System.EventHandler(this.send_btn_Click);
			this.label14.AutoSize = true;
			this.label14.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label14.ForeColor = System.Drawing.Color.Black;
			this.label14.Location = new System.Drawing.Point(711, 400);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(43, 17);
			this.label14.TabIndex = 65;
			this.label14.Text = "mVolt";
			this.label14.Click += new System.EventHandler(this.label14_Click);
			this.label10.AutoSize = true;
			this.label10.BackColor = System.Drawing.Color.Transparent;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f);
			this.label10.Location = new System.Drawing.Point(824, 25);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(124, 25);
			this.label10.TabIndex = 66;
			this.label10.Text = "Current [mA]";
			this.label10.Click += new System.EventHandler(this.label10_Click);
			this.label15.AutoSize = true;
			this.label15.BackColor = System.Drawing.Color.Transparent;
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f);
			this.label15.Location = new System.Drawing.Point(1674, 728);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(157, 25);
			this.label15.TabIndex = 67;
			this.label15.Text = "Sent/Voltage [V]";
			this.label15.Click += new System.EventHandler(this.label15_Click);
			this.Stop_btn.Location = new System.Drawing.Point(549, 843);
			this.Stop_btn.Name = "Stop_btn";
			this.Stop_btn.Size = new System.Drawing.Size(235, 47);
			this.Stop_btn.TabIndex = 68;
			this.Stop_btn.Text = "STOP";
			this.Stop_btn.UseVisualStyleBackColor = true;
			this.Stop_btn.Click += new System.EventHandler(this.button1_Click);
			this.label19.AutoSize = true;
			this.label19.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label19.ForeColor = System.Drawing.Color.Black;
			this.label19.Location = new System.Drawing.Point(711, 656);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(43, 17);
			this.label19.TabIndex = 69;
			this.label19.Text = "mVolt";
			this.label19.Click += new System.EventHandler(this.label19_Click_1);
			this.label24.AutoSize = true;
			this.label24.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label24.ForeColor = System.Drawing.Color.Black;
			this.label24.Location = new System.Drawing.Point(710, 583);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(43, 17);
			this.label24.TabIndex = 70;
			this.label24.Text = "mVolt";
			this.label24.Click += new System.EventHandler(this.label24_Click);
			this.label25.AutoSize = true;
			this.label25.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label25.ForeColor = System.Drawing.Color.Black;
			this.label25.Location = new System.Drawing.Point(713, 510);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(43, 17);
			this.label25.TabIndex = 71;
			this.label25.Text = "mVolt";
			this.label25.Click += new System.EventHandler(this.label25_Click);
			this.dk_gateA.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.dk_gateA.ForeColor = System.Drawing.Color.Black;
			this.dk_gateA.Location = new System.Drawing.Point(630, 533);
			this.dk_gateA.Name = "dk_gateA";
			this.dk_gateA.Size = new System.Drawing.Size(74, 22);
			this.dk_gateA.TabIndex = 72;
			this.dk_gateA.Text = "0";
			this.dk_gateA.TextChanged += new System.EventHandler(this.dk_gateA_TextChanged);
			this.label26.AutoSize = true;
			this.label26.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label26.ForeColor = System.Drawing.Color.Black;
			this.label26.Location = new System.Drawing.Point(710, 538);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(25, 17);
			this.label26.TabIndex = 73;
			this.label26.Text = "uA";
			this.label26.Click += new System.EventHandler(this.label26_Click);
			this.label27.AutoSize = true;
			this.label27.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label27.ForeColor = System.Drawing.Color.Black;
			this.label27.Location = new System.Drawing.Point(710, 611);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(25, 17);
			this.label27.TabIndex = 75;
			this.label27.Text = "uA";
			this.maxA_gate.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.maxA_gate.ForeColor = System.Drawing.Color.Black;
			this.maxA_gate.Location = new System.Drawing.Point(630, 606);
			this.maxA_gate.Name = "maxA_gate";
			this.maxA_gate.Size = new System.Drawing.Size(74, 22);
			this.maxA_gate.TabIndex = 74;
			this.maxA_gate.Text = "0";
			this.maxA_gate.TextChanged += new System.EventHandler(this.textBox6_TextChanged);
			this.label28.AutoSize = true;
			this.label28.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.label28.ForeColor = System.Drawing.Color.Black;
			this.label28.Location = new System.Drawing.Point(711, 690);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(25, 17);
			this.label28.TabIndex = 77;
			this.label28.Text = "uA";
			this.stepsA_gate.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
			this.stepsA_gate.ForeColor = System.Drawing.Color.Black;
			this.stepsA_gate.Location = new System.Drawing.Point(631, 685);
			this.stepsA_gate.Name = "stepsA_gate";
			this.stepsA_gate.Size = new System.Drawing.Size(74, 22);
			this.stepsA_gate.TabIndex = 76;
			this.stepsA_gate.Text = "0";
			this.stepsA_gate.TextChanged += new System.EventHandler(this.stepsA_gate_TextChanged);
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
			base.ClientSize = new System.Drawing.Size(1864, 935);
			base.Controls.Add(this.label28);
			base.Controls.Add(this.stepsA_gate);
			base.Controls.Add(this.label27);
			base.Controls.Add(this.maxA_gate);
			base.Controls.Add(this.label26);
			base.Controls.Add(this.dk_gateA);
			base.Controls.Add(this.label25);
			base.Controls.Add(this.label24);
			base.Controls.Add(this.label19);
			base.Controls.Add(this.Stop_btn);
			base.Controls.Add(this.label15);
			base.Controls.Add(this.label10);
			base.Controls.Add(this.label14);
			base.Controls.Add(this.send_btn);
			base.Controls.Add(this.label22);
			base.Controls.Add(this.label23);
			base.Controls.Add(this.Rb_gate);
			base.Controls.Add(this.label21);
			base.Controls.Add(this.steps_gate);
			base.Controls.Add(this.label16);
			base.Controls.Add(this.label17);
			base.Controls.Add(this.max_gate);
			base.Controls.Add(this.dk_gate);
			base.Controls.Add(this.label11);
			base.Controls.Add(this.label12);
			base.Controls.Add(this.label13);
			base.Controls.Add(this.max_current);
			base.Controls.Add(this.step_volt);
			base.Controls.Add(this.label9);
			base.Controls.Add(this.label8);
			base.Controls.Add(this.label6);
			base.Controls.Add(this.label5);
			base.Controls.Add(this.max_volt);
			base.Controls.Add(this.min_volt);
			base.Controls.Add(this.textBox4);
			base.Controls.Add(this.textBox3);
			base.Controls.Add(this.textBox2);
			base.Controls.Add(this.textBox1);
			base.Controls.Add(this.richTextBox2);
			base.Controls.Add(this.richTextBox1);
			base.Controls.Add(this.plotter_option_group);
			base.Controls.Add(this.graph);
			base.Controls.Add(this.save_btn);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.connect);
			base.Controls.Add(this.Set_up);
			base.Controls.Add(this.statusStrip1);
			base.Controls.Add(this.serial_options_group);
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.Margin = new System.Windows.Forms.Padding(4);
			base.Name = "MainForm";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "VACH Shield";
			base.Load += new System.EventHandler(this.MainForm_Load);
			this.serial_options_group.ResumeLayout(false);
			this.serial_options_group.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.Set_up.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.contextMenuStrip1.ResumeLayout(false);
			this.read_options_group.ResumeLayout(false);
			this.read_options_group.PerformLayout();
			this.dsiplayas_options_panel.ResumeLayout(false);
			this.dsiplayas_options_panel.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			this.contextMenuStrip2.ResumeLayout(false);
			this.write_options_group.ResumeLayout(false);
			this.write_options_group.PerformLayout();
			this.tx_num_panel.ResumeLayout(false);
			this.tx_num_panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this.send_delay).EndInit();
			((System.ComponentModel.ISupportInitialize)this.send_repeat).EndInit();
			this.tx_radiobuttons_panel.ResumeLayout(false);
			this.tx_radiobuttons_panel.PerformLayout();
			this.plotter_option_group.ResumeLayout(false);
			this.plotter_option_group.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this.graph_max).EndInit();
			((System.ComponentModel.ISupportInitialize)this.graph_min).EndInit();
			((System.ComponentModel.ISupportInitialize)this.graph_scale).EndInit();
			((System.ComponentModel.ISupportInitialize)this.graph_speed).EndInit();
			((System.ComponentModel.ISupportInitialize)this.graph).EndInit();
			this.graph_menu.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.datalogger_options_panel.ResumeLayout(false);
			this.datalogger_options_panel.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
	public class AboutBox1 : Form
	{
		private IContainer components = null;

		private RichTextBox richTextBox1;

		private LinkLabel linkLabel1;

		private LinkLabel linkLabel2;

		private RichTextBox richTextBox2;

		private RichTextBox richTextBox3;

		private RichTextBox richTextBox4;

		public AboutBox1()
		{
			InitializeComponent();
		}

		private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://www.changpuak.ch/electronics/Arduino-Shield-TANACHAI.php");
		}

		private void Label2_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void AboutBox1_Load(object sender, EventArgs e)
		{
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
		}

		private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://www.changpuak.ch/electronics/Arduino-Shield-TANACHAI.php");
		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{
		}

		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Close();
		}

		private void richTextBox2_TextChanged(object sender, EventArgs e)
		{
		}

		private void richTextBox4_TextChanged(object sender, EventArgs e)
		{
		}

		private void richTextBox3_TextChanged(object sender, EventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TANACHAIarduino.AboutBox1));
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.richTextBox2 = new System.Windows.Forms.RichTextBox();
			this.richTextBox3 = new System.Windows.Forms.RichTextBox();
			this.richTextBox4 = new System.Windows.Forms.RichTextBox();
			base.SuspendLayout();
			this.richTextBox1.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.Location = new System.Drawing.Point(69, 81);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(371, 151);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 24f);
			this.linkLabel1.Location = new System.Drawing.Point(61, 10);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(389, 46);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Curve Tracer Project";
			this.linkLabel1.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 0, 192);
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked_1);
			this.linkLabel2.AutoSize = true;
			this.linkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
			this.linkLabel2.LinkColor = System.Drawing.Color.Black;
			this.linkLabel2.Location = new System.Drawing.Point(542, 16);
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.Size = new System.Drawing.Size(17, 20);
			this.linkLabel2.TabIndex = 3;
			this.linkLabel2.TabStop = true;
			this.linkLabel2.Text = "x";
			this.linkLabel2.VisitedLinkColor = System.Drawing.Color.Black;
			this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
			this.richTextBox2.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox2.Enabled = false;
			this.richTextBox2.Location = new System.Drawing.Point(69, 274);
			this.richTextBox2.Name = "richTextBox2";
			this.richTextBox2.Size = new System.Drawing.Size(274, 28);
			this.richTextBox2.TabIndex = 4;
			this.richTextBox2.Text = "Software Author: Daniel Kasa";
			this.richTextBox2.TextChanged += new System.EventHandler(this.richTextBox2_TextChanged);
			this.richTextBox3.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox3.Enabled = false;
			this.richTextBox3.ForeColor = System.Drawing.Color.Black;
			this.richTextBox3.Location = new System.Drawing.Point(69, 308);
			this.richTextBox3.Name = "richTextBox3";
			this.richTextBox3.Size = new System.Drawing.Size(340, 28);
			this.richTextBox3.TabIndex = 5;
			this.richTextBox3.Text = "Contact info: daniel2.kasa@gmail.com";
			this.richTextBox3.TextChanged += new System.EventHandler(this.richTextBox3_TextChanged);
			this.richTextBox4.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox4.Enabled = false;
			this.richTextBox4.Location = new System.Drawing.Point(69, 238);
			this.richTextBox4.Name = "richTextBox4";
			this.richTextBox4.Size = new System.Drawing.Size(274, 28);
			this.richTextBox4.TabIndex = 6;
			this.richTextBox4.Text = "Hardware Author:  Alexander C. Frank";
			this.richTextBox4.TextChanged += new System.EventHandler(this.richTextBox4_TextChanged);
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			base.ClientSize = new System.Drawing.Size(571, 339);
			base.Controls.Add(this.richTextBox4);
			base.Controls.Add(this.richTextBox3);
			base.Controls.Add(this.richTextBox2);
			base.Controls.Add(this.linkLabel2);
			base.Controls.Add(this.linkLabel1);
			base.Controls.Add(this.richTextBox1);
			this.DoubleBuffered = true;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Margin = new System.Windows.Forms.Padding(4);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "AboutBox1";
			base.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			base.Load += new System.EventHandler(this.AboutBox1_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new MainForm());
		}
	}
}
namespace TANACHAIarduino.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("TANACHAIarduino.Properties.Resources", typeof(Resources).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal Resources()
		{
		}
	}
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;
	}
}
