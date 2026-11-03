using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ScottPlot.WinForms;
using TanachaiProSuite.Models;
using TanachaiProSuite.Services;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Label = System.Windows.Forms.Label;
using DataPoint = TanachaiProSuite.Models.DataPoint;

namespace TanachaiProSuite
{
    public class MainForm : Form
    {
        private readonly SerialManager _serialManager = new SerialManager();
        private readonly List<CurveData> _curves = new List<CurveData>();
        private CurveData _activeCurve = null;
        private DevicePreset _currentPreset;

        // UI Controls
        private ComboBox _cbPort;
        private ComboBox _cbBaud;
        private Button _btnConnect;
        private Button _btnRefreshPorts;
        private Label _lblStatus;
        private Panel _pnlStatusLed;

        private ComboBox _cbPresets;
        private Label _lblPinout;
        private NumericUpDown _numVcStart;
        private NumericUpDown _numVcStop;
        private NumericUpDown _numVcStep;
        private NumericUpDown _numIbStart;
        private NumericUpDown _numIbStop;
        private NumericUpDown _numIbStep;
        private NumericUpDown _numPMax;
        private NumericUpDown _numSettle;
        private NumericUpDown _numRb;
        private TextBox _txtDeviceName;
        private TextBox _txtOperator;

        private Button _btnStart;
        private Button _btnStop;
        private ProgressBar _progressBar;

        private FormsPlot _plotControl;
        private Label _lblCursorInfo;

        private Label _lblHfeDc;
        private Label _lblHfeAc;
        private Label _lblRo;
        private Label _lblEarlyVa;
        private Label _lblPeakPower;
        private TextBox _txtLog;

        private Button _btnExportExcel;
        private Button _btnExportCsv;
        private Button _btnSaveImage;
        private Button _btnClear;

        // Plotting series tracking
        private readonly List<ScottPlot.Plottables.Scatter> _plotScatters = new List<ScottPlot.Plottables.Scatter>();
        private readonly List<List<ScottPlot.Coordinates>> _curveCoords = new List<List<ScottPlot.Coordinates>>();
        private readonly System.Windows.Forms.Timer _renderTimer = new System.Windows.Forms.Timer { Interval = 33 };
        private ScottPlot.Plottables.Crosshair _crosshair;

        public MainForm()
        {
            InitializeComponent();
            SetupSerialEvents();
            LoadPresets();
            RefreshComPorts();
            _renderTimer.Tick += (s, e) =>
            {
                if (_curves.Count > 0)
                {
                    _plotControl.Plot.Axes.AutoScale();
                    _plotControl.Refresh();
                }
            };
        }

        private void InitializeComponent()
        {
            Text = "TANACHAI CURVE TRACER PRO v2.5 - Laboratory Workstation";
            Size = new Size(1360, 860);
            MinimumSize = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(24, 28, 34);
            ForeColor = Color.FromArgb(224, 230, 237);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // ==========================================
            // TOP HEADER BAR
            // ==========================================
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(32, 38, 48),
                Padding = new Padding(15, 10, 15, 10)
            };

            var lblTitle = new Label
            {
                Text = "⚡ TANACHAI CURVE TRACER PRO",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 255),
                AutoSize = true,
                Location = new Point(15, 16)
            };

            _pnlStatusLed = new Panel
            {
                Size = new Size(14, 14),
                Location = new Point(340, 23),
                BackColor = Color.FromArgb(200, 50, 50)
            };

            _lblStatus = new Label
            {
                Text = "DISCONNECTED",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 170, 185),
                AutoSize = true,
                Location = new Point(360, 21)
            };

            var lblPort = new Label { Text = "Port:", AutoSize = true, Location = new Point(500, 21), ForeColor = Color.LightGray };
            _cbPort = new ComboBox { Location = new Point(540, 17), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(42, 50, 62), ForeColor = Color.White };
            _btnRefreshPorts = new Button { Text = "🔄", Location = new Point(655, 16), Width = 36, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(42, 50, 62), ForeColor = Color.White };
            _btnRefreshPorts.Click += (s, e) => RefreshComPorts();

            var lblBaud = new Label { Text = "Baud:", AutoSize = true, Location = new Point(710, 21), ForeColor = Color.LightGray };
            _cbBaud = new ComboBox { Location = new Point(755, 17), Width = 95, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(42, 50, 62), ForeColor = Color.White };
            _cbBaud.Items.AddRange(new object[] { "115200", "9600" });
            _cbBaud.SelectedIndex = 0;

            _btnConnect = new Button
            {
                Text = "CONNECT",
                Location = new Point(870, 15),
                Width = 120,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _btnConnect.Click += BtnConnect_Click;

            topPanel.Controls.AddRange(new Control[] { lblTitle, _pnlStatusLed, _lblStatus, lblPort, _cbPort, _btnRefreshPorts, lblBaud, _cbBaud, _btnConnect });

            // ==========================================
            // LEFT SIDEBAR (CONTROL & PARAMETERS PANEL)
            // ==========================================
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 360,
                BackColor = Color.FromArgb(28, 33, 42),
                Padding = new Padding(12),
                AutoScroll = true
            };

            int y = 10;
            Label MakeSectionHeader(string text)
            {
                var lbl = new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 190, 240),
                    Location = new Point(12, y),
                    AutoSize = true
                };
                leftPanel.Controls.Add(lbl);
                y += 26;
                return lbl;
            }

            MakeSectionHeader("DEVICE & PRESET CONFIGURATION");

            _cbPresets = new ComboBox
            {
                Location = new Point(12, y),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(42, 50, 62),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _cbPresets.SelectedIndexChanged += CbPresets_SelectedIndexChanged;
            leftPanel.Controls.Add(_cbPresets);
            y += 34;

            var pnlPinout = new Panel
            {
                Location = new Point(12, y),
                Size = new Size(320, 65),
                BackColor = Color.FromArgb(36, 43, 54),
                BorderStyle = BorderStyle.FixedSingle
            };
            _lblPinout = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(255, 200, 80),
                Font = new Font("Consolas", 8.5f, FontStyle.Regular),
                Padding = new Padding(6),
                Text = "PINOUT HELPER"
            };
            pnlPinout.Controls.Add(_lblPinout);
            leftPanel.Controls.Add(pnlPinout);
            y += 75;

            MakeSectionHeader("SWEEP PARAMETERS");

            NumericUpDown AddNumRow(string labelText, decimal min, decimal max, decimal val, decimal step, string unit)
            {
                var lbl = new Label { Text = labelText, Location = new Point(12, y + 2), AutoSize = true, ForeColor = Color.Gainsboro };
                var num = new NumericUpDown
                {
                    Location = new Point(160, y),
                    Width = 110,
                    Minimum = min,
                    Maximum = max,
                    Value = val,
                    Increment = step,
                    DecimalPlaces = (step < 1 ? 2 : 0),
                    BackColor = Color.FromArgb(42, 50, 62),
                    ForeColor = Color.White
                };
                var lblUnit = new Label { Text = unit, Location = new Point(275, y + 2), AutoSize = true, ForeColor = Color.DarkGray };
                leftPanel.Controls.AddRange(new Control[] { lbl, num, lblUnit });
                y += 28;
                return num;
            }

            _numVcStart = AddNumRow("Vc / Vds Start:", 0, 27034, 0, 100, "mV");
            _numVcStop = AddNumRow("Vc / Vds Stop:", 1, 27034, 10000, 500, "mV");
            _numVcStep = AddNumRow("Vc Step (ΔV):", 1, 5000, 100, 50, "mV");

            y += 5;
            _numIbStart = AddNumRow("Ib / Vgs Start:", 0, 27034, 650, 100, "mV");
            _numIbStop = AddNumRow("Ib / Vgs Stop:", 0, 27034, 14150, 500, "mV");
            _numIbStep = AddNumRow("Ib / Vgs Step:", 0, 10000, 3375, 250, "mV");

            y += 5;
            _numPMax = AddNumRow("Max Power (Pmax):", 50, 5000, 2000, 100, "mW");
            _numSettle = AddNumRow("Settling Delay:", 2, 500, 8, 2, "ms");
            _numRb = AddNumRow("Base Resistor (Rb):", 100, 500000, 27000, 1000, "Ω");

            y += 8;
            MakeSectionHeader("TEST METADATA");
            var lblDev = new Label { Text = "Component Name:", Location = new Point(12, y + 2), AutoSize = true, ForeColor = Color.Gainsboro };
            _txtDeviceName = new TextBox { Location = new Point(160, y), Width = 172, Text = "BC141-16", BackColor = Color.FromArgb(42, 50, 62), ForeColor = Color.White };
            leftPanel.Controls.AddRange(new Control[] { lblDev, _txtDeviceName });
            y += 28;

            var lblOp = new Label { Text = "Operator:", Location = new Point(12, y + 2), AutoSize = true, ForeColor = Color.Gainsboro };
            _txtOperator = new TextBox { Location = new Point(160, y), Width = 172, Text = "Lab", BackColor = Color.FromArgb(42, 50, 62), ForeColor = Color.White };
            leftPanel.Controls.AddRange(new Control[] { lblOp, _txtOperator });
            y += 38;

            // ACTION BUTTONS
            _btnStart = new Button
            {
                Text = "▶ START SWEEP",
                Location = new Point(12, y),
                Size = new Size(155, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 160, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnStart.Click += BtnStart_Click;

            _btnStop = new Button
            {
                Text = "⏹ STOP / ABORT",
                Location = new Point(177, y),
                Size = new Size(155, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            _btnStop.Click += BtnStop_Click;

            leftPanel.Controls.AddRange(new Control[] { _btnStart, _btnStop });
            y += 50;

            _progressBar = new ProgressBar
            {
                Location = new Point(12, y),
                Size = new Size(320, 14),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            leftPanel.Controls.Add(_progressBar);

            // ==========================================
            // BOTTOM METROLOGY & EXPORT PANEL
            // ==========================================
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.FromArgb(32, 38, 48),
                Padding = new Padding(12, 8, 12, 8)
            };

            var pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 720,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };

            Panel CreateMetricCard(string title, out Label valLabel)
            {
                var card = new Panel
                {
                    Size = new Size(135, 60),
                    BackColor = Color.FromArgb(42, 50, 64),
                    Margin = new Padding(4)
                };
                var lblT = new Label
                {
                    Text = title,
                    Dock = DockStyle.Top,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 190, 240)
                };
                valLabel = new Label
                {
                    Text = "-",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.White
                };
                card.Controls.AddRange(new Control[] { valLabel, lblT });
                return card;
            }

            pnlCards.Controls.Add(CreateMetricCard("hFE (DC GAIN)", out _lblHfeDc));
            pnlCards.Controls.Add(CreateMetricCard("hfe (AC GAIN)", out _lblHfeAc));
            pnlCards.Controls.Add(CreateMetricCard("OUTPUT ro", out _lblRo));
            pnlCards.Controls.Add(CreateMetricCard("EARLY VOLT (Va)", out _lblEarlyVa));
            pnlCards.Controls.Add(CreateMetricCard("PEAK POWER", out _lblPeakPower));

            // Log Console (collapsible text)
            _txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(4, 70),
                Size = new Size(710, 68),
                BackColor = Color.FromArgb(20, 24, 30),
                ForeColor = Color.FromArgb(170, 185, 205),
                Font = new Font("Consolas", 8f)
            };
            pnlCards.Controls.Add(_txtLog);

            // Export Buttons Flow
            var pnlExport = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 260,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.Transparent
            };

            Button MakeExportBtn(string text, Color bg, EventHandler handler)
            {
                var btn = new Button
                {
                    Text = text,
                    Size = new Size(240, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = bg,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Margin = new Padding(2)
                };
                btn.Click += handler;
                pnlExport.Controls.Add(btn);
                return btn;
            }

            _btnExportExcel = MakeExportBtn("📊 EXPORT TO EXCEL (.xlsx)", Color.FromArgb(30, 120, 70), BtnExportExcel_Click);
            _btnExportCsv = MakeExportBtn("📄 EXPORT TO CSV (.csv)", Color.FromArgb(50, 80, 110), BtnExportCsv_Click);
            _btnSaveImage = MakeExportBtn("📷 SAVE PLOT IMAGE (PNG)", Color.FromArgb(90, 70, 130), BtnSaveImage_Click);
            _btnClear = MakeExportBtn("🧹 CLEAR CHART", Color.FromArgb(120, 50, 50), (s, e) => ClearChart());

            bottomPanel.Controls.AddRange(new Control[] { pnlCards, pnlExport });

            // ==========================================
            // CENTER: SCOTTPLOT 5 PLOTTER CONTROL
            // ==========================================
            var centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 22, 28),
                Padding = new Padding(10)
            };

            _lblCursorInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = "Move mouse over curve to inspect instant operating point (V_CE, I_C, Ib, hFE)...",
                ForeColor = Color.FromArgb(0, 210, 255),
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _plotControl = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            ConfigurePlotTheme();

            centerPanel.Controls.AddRange(new Control[] { _plotControl, _lblCursorInfo });

            // ADD ALL MAIN PANELS
            Controls.AddRange(new Control[] { centerPanel, bottomPanel, leftPanel, topPanel });
        }

        private void ConfigurePlotTheme()
        {
            var plot = _plotControl.Plot;

            // Modern Dark Instrument styling
            plot.FigureBackground.Color = ScottPlot.Color.FromHex("#12161C");
            plot.DataBackground.Color = ScottPlot.Color.FromHex("#181D24");

            plot.Axes.Color(ScottPlot.Color.FromHex("#A0B0C0"));
            plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#262F3C");
            plot.Grid.MinorLineColor = ScottPlot.Color.FromHex("#1E242E");

            plot.XLabel("Collector-Emitter Voltage V_CE [V]");
            plot.YLabel("Collector Current I_C [mA]");
            plot.Title("CHARACTERISTIC CURVES");

            plot.ShowLegend(ScottPlot.Edge.Right);
            plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#202632");
            plot.Legend.FontColor = ScottPlot.Color.FromHex("#E0E6ED");
            plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#3A4555");

            // Crosshair
            _crosshair = plot.Add.Crosshair(0, 0);
            _crosshair.IsVisible = false;
            _crosshair.LineColor = ScottPlot.Color.FromHex("#FF9900");

            _plotControl.MouseMove += PlotControl_MouseMove;
            _plotControl.Refresh();
        }

        private void PlotControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_curves.Count == 0) return;

            var pixel = new ScottPlot.Pixel(e.X, e.Y);
            var coord = _plotControl.Plot.GetCoordinates(pixel);

            _crosshair.IsVisible = true;
            _crosshair.Position = coord;

            double mouseV = coord.X;
            double mouseI = coord.Y;

            // Find closest curve and point
            CurveData closestCurve = null;
            DataPoint closestPoint = null;
            double minDistance = double.MaxValue;

            foreach (var curve in _curves)
            {
                foreach (var pt in curve.Points)
                {
                    double dist = Math.Sqrt(Math.Pow(pt.VDutV - mouseV, 2) + Math.Pow(pt.IcMa - mouseI, 2));
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestCurve = curve;
                        closestPoint = pt;
                    }
                }
            }

            if (closestPoint != null && closestCurve != null)
            {
                double hfe = (closestCurve.IbUa > 0.01) ? (closestPoint.IcMa * 1000.0) / closestCurve.IbUa : 0;
                _lblCursorInfo.Text = string.Format(CultureInfo.InvariantCulture,
                    "📍 Point: V_CE = {0:0.000} V | I_C = {1:0.00} mA | Ib = {2:0.#} µA | Instantaneous hFE = {3:0.#} | Dissipation = {4:0} mW",
                    closestPoint.VDutV, closestPoint.IcMa, closestCurve.IbUa, hfe, closestPoint.PowerMw);
            }

            _plotControl.Refresh();
        }

        private void SetupSerialEvents()
        {
            _serialManager.OnLogMessage += msg =>
            {
                if (InvokeRequired) { BeginInvoke((Action)(() => AppendLog(msg))); }
                else { AppendLog(msg); }
            };

            _serialManager.OnConnectionChanged += () =>
            {
                if (InvokeRequired) { BeginInvoke((Action)UpdateConnectionUi); }
                else { UpdateConnectionUi(); }
            };

            _serialManager.OnSweepStarted += () =>
            {
                if (InvokeRequired) { BeginInvoke((Action)HandleSweepStarted); }
                else { HandleSweepStarted(); }
            };

            _serialManager.OnCurveStarted += (idx, vIb, ibUa) =>
            {
                if (InvokeRequired) { BeginInvoke((Action)(() => HandleCurveStarted(idx, vIb, ibUa))); }
                else { HandleCurveStarted(idx, vIb, ibUa); }
            };

            _serialManager.OnDataPointReceived += (idx, vcSet, vDut, icMa, pwrMw) =>
            {
                if (InvokeRequired) { BeginInvoke((Action)(() => HandleDataPoint(idx, vcSet, vDut, icMa, pwrMw))); }
                else { HandleDataPoint(idx, vcSet, vDut, icMa, pwrMw); }
            };

            _serialManager.OnCurveEnded += (idx) =>
            {
                if (InvokeRequired) { BeginInvoke((Action)(() => HandleCurveEnded(idx))); }
                else { HandleCurveEnded(idx); }
            };

            _serialManager.OnSweepCompleted += () =>
            {
                if (InvokeRequired) { BeginInvoke((Action)HandleSweepCompleted); }
                else { HandleSweepCompleted(); }
            };

            _serialManager.OnSweepAborted += (reason) =>
            {
                if (InvokeRequired) { BeginInvoke((Action)(() => HandleSweepAborted(reason))); }
                else { HandleSweepAborted(reason); }
            };
        }

        private void AppendLog(string text)
        {
            if (_txtLog.IsDisposed) return;
            _txtLog.AppendText(text + Environment.NewLine);
        }

        private void UpdateConnectionUi()
        {
            bool conn = _serialManager.IsConnected;
            _pnlStatusLed.BackColor = conn ? Color.FromArgb(40, 200, 80) : Color.FromArgb(200, 50, 50);
            _lblStatus.Text = conn ? $"CONNECTED ({_serialManager.CurrentPort})" : "DISCONNECTED";
            _lblStatus.ForeColor = conn ? Color.FromArgb(0, 230, 120) : Color.FromArgb(160, 170, 185);
            _btnConnect.Text = conn ? "DISCONNECT" : "CONNECT";
            _btnConnect.BackColor = conn ? Color.FromArgb(140, 50, 50) : Color.FromArgb(0, 120, 215);
            _btnStart.Enabled = conn;
        }

        private void RefreshComPorts()
        {
            _cbPort.Items.Clear();
            var ports = SerialManager.GetAvailablePorts();
            if (ports.Length > 0)
            {
                _cbPort.Items.AddRange(ports);
                _cbPort.SelectedIndex = 0;
            }
        }

        private void LoadPresets()
        {
            var presets = DevicePreset.GetStandardPresets();
            _cbPresets.DataSource = presets;
            _cbPresets.DisplayMember = "Name";
            _cbPresets.SelectedIndex = 0;
        }

        private void CbPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cbPresets.SelectedItem is DevicePreset preset)
            {
                _currentPreset = preset;
                _numVcStart.Value = preset.VcStartMv;
                _numVcStop.Value = preset.VcStopMv;
                _numVcStep.Value = preset.VcStepMv;
                _numIbStart.Value = preset.IbStartMv;
                _numIbStop.Value = preset.IbStopMv;
                _numIbStep.Value = preset.IbStepMv;
                _numPMax.Value = preset.MaxPowerMw;
                _numSettle.Value = preset.SettlingDelayMs;
                _numRb.Value = preset.RbOhm;
                _lblPinout.Text = preset.PinoutInfo;

                // Adjust Chart labels according to category
                if (preset.Category == DeviceCategory.MosfetN || preset.Category == DeviceCategory.MosfetP)
                {
                    _plotControl.Plot.XLabel("Drain-Source Voltage V_DS [V]");
                    _plotControl.Plot.YLabel("Drain Current I_D [mA]");
                }
                else if (preset.Category == DeviceCategory.Diode || preset.Category == DeviceCategory.Zener)
                {
                    _plotControl.Plot.XLabel("Forward/Reverse Voltage V_DUT [V]");
                    _plotControl.Plot.YLabel("Device Current I [mA]");
                }
                else
                {
                    _plotControl.Plot.XLabel("Collector-Emitter Voltage V_CE [V]");
                    _plotControl.Plot.YLabel("Collector Current I_C [mA]");
                }
                _plotControl.Refresh();
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (_serialManager.IsConnected)
            {
                _serialManager.Disconnect();
            }
            else
            {
                if (_cbPort.SelectedItem == null)
                {
                    MessageBox.Show("No COM port selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string port = _cbPort.SelectedItem.ToString();
                int baud = int.Parse(_cbBaud.SelectedItem.ToString());
                _serialManager.Connect(port, baud);
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!_serialManager.IsConnected) return;

            ClearChart();
            _curves.Clear();

            _btnStart.Enabled = false;
            _btnStop.Enabled = true;
            _progressBar.Visible = true;
            _progressBar.Value = 0;

            _serialManager.StartModernSweep(
                (int)_numVcStart.Value,
                (int)_numVcStop.Value,
                (int)_numVcStep.Value,
                (int)_numIbStart.Value,
                (int)_numIbStop.Value,
                (int)_numIbStep.Value,
                (int)_numPMax.Value,
                (int)_numSettle.Value,
                (int)_numRb.Value
            );
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _serialManager.SendStop();
        }

        private void HandleSweepStarted()
        {
            _curves.Clear();
            _curveCoords.Clear();
            _plotControl.Plot.Clear();
            _plotScatters.Clear();
            ConfigurePlotTheme();
            _progressBar.Visible = true;
            _renderTimer.Start();
        }

        private readonly string[] _palette = new[]
        {
            "#00E5FF", "#FF9100", "#76FF03", "#FF1744", "#D500F9",
            "#FFEA00", "#00B0FF", "#00E676", "#FF3D00", "#F50057"
        };

        private void HandleCurveStarted(int curveIdx, int vIb, double ibUa)
        {
            _activeCurve = new CurveData(curveIdx, vIb, ibUa);
            _curves.Add(_activeCurve);

            // Add new ScottPlot scatter series using List<Coordinates>
            var coords = new List<ScottPlot.Coordinates>();
            _curveCoords.Add(coords);

            string label = (_currentPreset.Category == DeviceCategory.MosfetN || _currentPreset.Category == DeviceCategory.MosfetP)
                ? $"Vgs = {vIb / 1000.0:0.##}V"
                : (_currentPreset.Category == DeviceCategory.Diode || _currentPreset.Category == DeviceCategory.Zener)
                    ? "V-I Curve"
                    : $"Ib = {ibUa:0.#} µA";

            var color = ScottPlot.Color.FromHex(_palette[curveIdx % _palette.Length]);
            var scatter = _plotControl.Plot.Add.Scatter(coords, color);
            scatter.LegendText = label;
            scatter.LineWidth = 2.2f;

            _plotScatters.Add(scatter);
        }

        private void HandleDataPoint(int curveIdx, int vcSet, int vDut, double icMa, double pwrMw)
        {
            if (_activeCurve == null)
            {
                _activeCurve = new CurveData(curveIdx, 0, 0);
                _curves.Add(_activeCurve);
                var coords = new List<ScottPlot.Coordinates>();
                _curveCoords.Add(coords);
            }

            var pt = new DataPoint(vcSet, vDut, icMa, pwrMw);
            _activeCurve.Points.Add(pt);

            // Live update of current curve (rendered smoothly at 30 FPS by _renderTimer)
            if (curveIdx < _curveCoords.Count)
            {
                _curveCoords[curveIdx].Add(new ScottPlot.Coordinates(pt.VDutV, pt.IcMa));
            }
        }

        private void HandleCurveEnded(int curveIdx)
        {
        }

        private void HandleSweepCompleted()
        {
            _renderTimer.Stop();
            _plotControl.Plot.Axes.AutoScale();
            _plotControl.Refresh();

            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _progressBar.Visible = false;

            // Run metrology analysis
            var result = MetrologyEngine.Analyze(_curves, _currentPreset.Category);
            DisplayMetrologyResults(result);

            AppendLog($"[METROLOGY] {result.SummaryNotes}");
            MessageBox.Show("Sweep completed successfully!\n" + result.SummaryNotes, "Measurement Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleSweepAborted(string reason)
        {
            _renderTimer.Stop();
            _plotControl.Plot.Axes.AutoScale();
            _plotControl.Refresh();

            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _progressBar.Visible = false;
            AppendLog($"[ABORT] {reason}");
            MessageBox.Show($"Sweep aborted: {reason}", "Sweep Halted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void DisplayMetrologyResults(MetrologyResult r)
        {
            _lblHfeDc.Text = !double.IsNaN(r.HfeDc) ? r.HfeDc.ToString("0.#") : "-";
            _lblHfeAc.Text = !double.IsNaN(r.HfeAc) ? r.HfeAc.ToString("0.#") : "-";
            _lblRo.Text = !double.IsNaN(r.OutputResistanceRo) ? (r.OutputResistanceRo / 1000.0).ToString("0.#") + " kΩ" : "-";
            _lblEarlyVa.Text = !double.IsNaN(r.EarlyVoltageVa) ? r.EarlyVoltageVa.ToString("0.#") + " V" : "-";
            _lblPeakPower.Text = $"{r.PeakPowerMw:0} mW";
        }

        private void ClearChart()
        {
            _curves.Clear();
            _curveCoords.Clear();
            _plotScatters.Clear();
            _plotControl.Plot.Clear();
            ConfigurePlotTheme();
            _lblHfeDc.Text = "-";
            _lblHfeAc.Text = "-";
            _lblRo.Text = "-";
            _lblEarlyVa.Text = "-";
            _lblPeakPower.Text = "-";
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (_curves.Count == 0)
            {
                MessageBox.Show("No measurement curves to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                sfd.FileName = $"{_txtDeviceName.Text}_Curves_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var metrology = MetrologyEngine.Analyze(_curves, _currentPreset.Category);
                        ExcelExportManager.ExportToExcel(sfd.FileName, _curves, _currentPreset, metrology, _txtDeviceName.Text, _txtOperator.Text);
                        MessageBox.Show($"Report exported successfully with native Excel Scatter Chart to:\n{sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Excel export error: {ex.Message}", "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            if (_curves.Count == 0) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Data File (*.csv)|*.csv";
                sfd.FileName = $"{_txtDeviceName.Text}_Data.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExcelExportManager.ExportToCsv(sfd.FileName, _curves);
                        MessageBox.Show("CSV export successful!", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"CSV export error: {ex.Message}", "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSaveImage_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg";
                sfd.FileName = $"{_txtDeviceName.Text}_Plot.png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _plotControl.Plot.SavePng(sfd.FileName, 1920, 1080);
                        MessageBox.Show("High-resolution chart saved!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _serialManager.Dispose();
            base.OnFormClosing(e);
        }
    }
}
