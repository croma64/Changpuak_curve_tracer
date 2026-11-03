using System;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace TanachaiProSuite.Services
{
    public class SerialManager : IDisposable
    {
        private SerialPort _serialPort;
        private readonly StringBuilder _rxBuffer = new StringBuilder();
        private readonly object _lockObj = new object();

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
        public string CurrentPort => _serialPort?.PortName ?? string.Empty;
        public int CurrentBaudrate => _serialPort?.BaudRate ?? 115200;

        // Events
        public event Action<string> OnLogMessage;
        public event Action OnConnectionChanged;
        public event Action OnSweepStarted;
        public event Action<int, int, double> OnCurveStarted; // curveIdx, vIbMv, ibUa
        public event Action<int, int, int, double, double> OnDataPointReceived; // curveIdx, vcSet, vDutMv, icMa, pwrMw
        public event Action<int> OnCurveEnded;
        public event Action OnSweepCompleted;
        public event Action<string> OnSweepAborted;
        public event Action<double, double> OnOverpowerAlert;
        public event Action<string> OnDeviceInfoReceived;

        private int _currentCurveIndex = 0;
        private double _currentIbUa = 0.0;

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool Connect(string portName, int baudrate = 115200)
        {
            Disconnect();
            try
            {
                _serialPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000,
                    DtrEnable = true, // Required for Arduino reset / comms
                    RtsEnable = true
                };

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                OnLogMessage?.Invoke($"[INFO] Connected to {portName} at {baudrate} baud.");
                OnConnectionChanged?.Invoke();

                // Wait 1.5s for Arduino bootloader, then query IDN
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(1600);
                    if (IsConnected)
                    {
                        SendLine("*IDN?");
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[ERROR] Connection failed: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        public void Disconnect()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        SendStop();
                        Thread.Sleep(50);
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                }
                catch { }
                finally
                {
                    _serialPort = null;
                }
                OnLogMessage?.Invoke("[INFO] Disconnected.");
                OnConnectionChanged?.Invoke();
            }
        }

        public void StartModernSweep(int vcStart, int vcStop, int vcStep,
                                     int ibStart, int ibStop, int ibStep,
                                     int maxPowerMw, int settleMs, int rbOhm)
        {
            if (!IsConnected) return;
            string cmd = string.Format(CultureInfo.InvariantCulture,
                "SWEEP {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                vcStart, vcStop, vcStep, ibStart, ibStop, ibStep, maxPowerMw, settleMs, rbOhm);

            SendLine(cmd);
            OnLogMessage?.Invoke($"[TX] {cmd}");
        }

        public void SendStop()
        {
            if (!IsConnected) return;
            try
            {
                _serialPort.Write("STOP\n");
                OnLogMessage?.Invoke("[TX] STOP command sent.");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[ERROR] Failed to send STOP: {ex.Message}");
            }
        }

        public void SendLine(string text)
        {
            if (!IsConnected) return;
            try
            {
                _serialPort.Write(text + "\n");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[ERROR] Write error: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;
            try
            {
                int bytes = _serialPort.BytesToRead;
                byte[] buffer = new byte[bytes];
                int read = _serialPort.Read(buffer, 0, bytes);
                if (read <= 0) return;

                string chunk = Encoding.ASCII.GetString(buffer, 0, read);

                lock (_lockObj)
                {
                    _rxBuffer.Append(chunk);
                    while (true)
                    {
                        string str = _rxBuffer.ToString();
                        int idx = str.IndexOf('\n');
                        if (idx < 0) break;

                        string line = str.Substring(0, idx).Trim('\r', '\n', ' ');
                        _rxBuffer.Remove(0, idx + 1);

                        if (line.Length > 0)
                        {
                            ParseIncomingLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[ERROR] Read error: {ex.Message}");
            }
        }

        private void ParseIncomingLine(string line)
        {
            // 1. Sweep Start
            if (line == "[SWEEP_START]")
            {
                _currentCurveIndex = 0;
                OnSweepStarted?.Invoke();
                return;
            }

            // 2. Modern Curve Start: CURVE_START,<idx>,VIB_MV,<val>,IB_UA,<val>
            if (line.StartsWith("CURVE_START,"))
            {
                var parts = line.Split(',');
                if (parts.Length >= 6)
                {
                    int.TryParse(parts[1], out _currentCurveIndex);
                    int.TryParse(parts[3], out int vIbMv);
                    double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out _currentIbUa);
                    OnCurveStarted?.Invoke(_currentCurveIndex, vIbMv, _currentIbUa);
                }
                return;
            }

            // 3. Modern Data Point: DATA,<idx>,<vcSet>,<vDut>,<icMa>,<pwrMw>
            if (line.StartsWith("DATA,"))
            {
                var parts = line.Split(',');
                if (parts.Length >= 6)
                {
                    int.TryParse(parts[1], out int curveIdx);
                    int.TryParse(parts[2], out int vcSet);
                    int.TryParse(parts[3], out int vDut);
                    double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double icMa);
                    double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double pwrMw);
                    OnDataPointReceived?.Invoke(curveIdx, vcSet, vDut, icMa, pwrMw);
                }
                return;
            }

            // 4. Modern Curve End
            if (line.StartsWith("CURVE_END,"))
            {
                var parts = line.Split(',');
                int cIdx = _currentCurveIndex;
                if (parts.Length >= 2) int.TryParse(parts[1], out cIdx);
                OnCurveEnded?.Invoke(cIdx);
                return;
            }

            // 5. Sweep Complete / Aborted
            if (line == "[SWEEP_COMPLETE]")
            {
                OnSweepCompleted?.Invoke();
                return;
            }
            if (line == "[SWEEP_ABORTED]")
            {
                OnSweepAborted?.Invoke("Aborted by user.");
                return;
            }
            if (line.StartsWith("[ALERT:OVERPOWER]"))
            {
                OnOverpowerAlert?.Invoke(0, 0);
                OnSweepAborted?.Invoke("Hardware Protection Triggered: Maximum Power Limit Exceeded!");
                return;
            }

            // 6. Device IDN Response
            if (line.StartsWith("TANACHAI:") || line.StartsWith("TANACHAI_PRO_READY"))
            {
                OnDeviceInfoReceived?.Invoke(line);
                OnLogMessage?.Invoke($"[DEVICE] {line}");
                return;
            }

            // 7. Legacy Format Compatibility:
            // "Ib = <mV>"
            if (line.StartsWith("Ib ="))
            {
                string valStr = line.Substring(4).Trim();
                if (double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double dacMv))
                {
                    _currentIbUa = (dacMv / 27000.0) * 1000000.0;
                    OnCurveStarted?.Invoke(_currentCurveIndex, (int)dacMv, _currentIbUa);
                }
                return;
            }
            if (line.Equals("New", StringComparison.OrdinalIgnoreCase))
            {
                OnCurveEnded?.Invoke(_currentCurveIndex);
                _currentCurveIndex++;
                return;
            }
            // Single legacy reading: "<Ic_mA>"
            if (double.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out double legacyIcMa))
            {
                OnDataPointReceived?.Invoke(_currentCurveIndex, 0, 0, legacyIcMa, 0);
                return;
            }

            // Unhandled text line: log to console
            OnLogMessage?.Invoke($"[RX] {line}");
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
