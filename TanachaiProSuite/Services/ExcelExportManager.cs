using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using TanachaiProSuite.Models;

namespace TanachaiProSuite.Services
{
    public class ExcelExportManager
    {
        static ExcelExportManager()
        {
#pragma warning disable CS0618
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
        }

        public static void ExportToExcel(string filePath, List<CurveData> curves, DevicePreset preset, MetrologyResult metrology, string deviceName, string operatorName)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var fileInfo = new FileInfo(filePath);
            using (var package = new ExcelPackage(fileInfo))
            {
                // ==========================================
                // SHEET 1: METROLOGY SUMMARY
                // ==========================================
                var wsSummary = package.Workbook.Worksheets.Add("Summary & Metrology");
                wsSummary.View.ShowGridLines = true;

                // Header Banner
                wsSummary.Cells["A1:G1"].Merge = true;
                wsSummary.Cells["A1"].Value = "TANACHAI CURVE TRACER PRO - MEASUREMENT REPORT";
                wsSummary.Cells["A1"].Style.Font.Size = 16;
                wsSummary.Cells["A1"].Style.Font.Bold = true;
                wsSummary.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsSummary.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 42, 56));
                wsSummary.Cells["A1"].Style.Font.Color.SetColor(Color.White);
                wsSummary.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                wsSummary.Row(1).Height = 35;

                // Metadata Table
                int row = 3;
                void AddMeta(string key, string val)
                {
                    wsSummary.Cells[row, 1].Value = key;
                    wsSummary.Cells[row, 1].Style.Font.Bold = true;
                    wsSummary.Cells[row, 2].Value = val;
                    row++;
                }

                AddMeta("Timestamp:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                AddMeta("Device Name:", string.IsNullOrWhiteSpace(deviceName) ? "DUT" : deviceName);
                AddMeta("Device Category:", preset.Category.ToString());
                AddMeta("Operator:", string.IsNullOrWhiteSpace(operatorName) ? "Lab Technician" : operatorName);
                AddMeta("Max Safe Power Limit:", $"{preset.MaxPowerMw} mW");
                AddMeta("Current Sense Shunt (R1):", "1.00 Ω (1%)");
                AddMeta("Base Series Resistor (Rb):", $"{preset.RbOhm} Ω");

                row++;
                // Parameter Table Header
                wsSummary.Cells[row, 1, row, 3].Merge = true;
                wsSummary.Cells[row, 1].Value = "EXTRACTED SEMICONDUCTOR PARAMETERS";
                wsSummary.Cells[row, 1].Style.Font.Bold = true;
                wsSummary.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsSummary.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(70, 90, 115));
                wsSummary.Cells[row, 1].Style.Font.Color.SetColor(Color.White);
                row++;

                void AddParam(string paramName, string symbol, object val, string unit)
                {
                    wsSummary.Cells[row, 1].Value = paramName;
                    wsSummary.Cells[row, 2].Value = symbol;
                    wsSummary.Cells[row, 2].Style.Font.Italic = true;
                    wsSummary.Cells[row, 3].Value = val;
                    wsSummary.Cells[row, 4].Value = unit;
                    row++;
                }

                if (!double.IsNaN(metrology.HfeDc))
                    AddParam("DC Current Gain (Typical)", "hFE", Math.Round(metrology.HfeDc, 1), "-");
                if (!double.IsNaN(metrology.HfeAc))
                    AddParam("AC Small-Signal Gain", "hfe", Math.Round(metrology.HfeAc, 1), "-");
                if (!double.IsNaN(metrology.EarlyVoltageVa))
                    AddParam("Early Voltage", "Va", Math.Round(metrology.EarlyVoltageVa, 1), "V");
                if (!double.IsNaN(metrology.OutputResistanceRo))
                    AddParam("Output Resistance", "ro", Math.Round(metrology.OutputResistanceRo / 1000.0, 1), "kΩ");
                if (!double.IsNaN(metrology.TransconductanceGm))
                    AddParam("Transconductance", "gm", Math.Round(metrology.TransconductanceGm, 2), "mS");
                if (!double.IsNaN(metrology.DiodeVf1mA))
                    AddParam("Forward Voltage @ 1mA", "VF(1mA)", Math.Round(metrology.DiodeVf1mA, 3), "V");
                if (!double.IsNaN(metrology.DiodeVf10mA))
                    AddParam("Forward Voltage @ 10mA", "VF(10mA)", Math.Round(metrology.DiodeVf10mA, 3), "V");
                if (!double.IsNaN(metrology.DiodeDynamicRd))
                    AddParam("Dynamic Resistance", "rd", Math.Round(metrology.DiodeDynamicRd, 1), "Ω");
                if (!double.IsNaN(metrology.ZenerVz))
                    AddParam("Zener Voltage", "Vz", Math.Round(metrology.ZenerVz, 2), "V");
                if (!double.IsNaN(metrology.ZenerZz))
                    AddParam("Dynamic Zener Impedance", "Zz", Math.Round(metrology.ZenerZz, 1), "Ω");

                AddParam("Peak Current Measured", "Ic_max", Math.Round(metrology.PeakCurrentMa, 2), "mA");
                AddParam("Peak Power Dissipated", "P_max", Math.Round(metrology.PeakPowerMw, 1), "mW");

                wsSummary.Columns[1].Width = 32;
                wsSummary.Columns[2].Width = 18;
                wsSummary.Columns[3].Width = 16;
                wsSummary.Columns[4].Width = 10;

                // ==========================================
                // SHEET 2: CHARACTERISTIC CURVES DATA
                // ==========================================
                if (curves.Count > 0)
                {
                    var wsData = package.Workbook.Worksheets.Add("Curves Data");
                    wsData.View.ShowGridLines = true;

                    // Header row
                    wsData.Cells[1, 1].Value = "V_DUT [V]";
                    wsData.Cells[1, 1].Style.Font.Bold = true;
                    wsData.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    wsData.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(40, 60, 80));
                    wsData.Cells[1, 1].Style.Font.Color.SetColor(Color.White);

                    for (int c = 0; c < curves.Count; c++)
                    {
                        int col = c + 2;
                        string curveLabel = preset.Category switch
                        {
                            DeviceCategory.MosfetN or DeviceCategory.MosfetP => $"Id (Vgs = {curves[c].VIbMv / 1000.0:0.##}V) [mA]",
                            DeviceCategory.Diode or DeviceCategory.Zener => "I_DUT [mA]",
                            _ => $"Ic (Ib = {curves[c].IbUa:0.#} µA) [mA]"
                        };

                        wsData.Cells[1, col].Value = curveLabel;
                        wsData.Cells[1, col].Style.Font.Bold = true;
                        wsData.Cells[1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        wsData.Cells[1, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(40, 60, 80));
                        wsData.Cells[1, col].Style.Font.Color.SetColor(Color.White);
                    }

                    // Fill data points
                    int maxPoints = curves.Max(c => c.Points.Count);
                    for (int i = 0; i < maxPoints; i++)
                    {
                        int dataRow = i + 2;
                        // Use V_DUT from first curve that has this point
                        var pt0 = curves.FirstOrDefault(c => c.Points.Count > i)?.Points[i];
                        if (pt0 != null)
                        {
                            wsData.Cells[dataRow, 1].Value = Math.Round(pt0.VDutV, 3);
                        }

                        for (int c = 0; c < curves.Count; c++)
                        {
                            int col = c + 2;
                            if (i < curves[c].Points.Count)
                            {
                                wsData.Cells[dataRow, col].Value = Math.Round(curves[c].Points[i].IcMa, 4);
                            }
                        }
                    }

                    wsData.Cells[wsData.Dimension.Address].AutoFitColumns();

                    // ==========================================
                    // EMBEDDED EXCEL SCATTER CHART
                    // ==========================================
                    try
                    {
                        var chart = wsData.Drawings.AddScatterChart("CharacteristicCurvesChart", eScatterChartType.XYScatterLinesNoMarkers);
                        chart.Title.Text = preset.Category switch
                        {
                            DeviceCategory.Diode or DeviceCategory.Zener => "Diode Characteristic Curve I = f(V)",
                            DeviceCategory.MosfetN or DeviceCategory.MosfetP => "Output Characteristics Id = f(Vds, Vgs)",
                            _ => "Output Characteristics Ic = f(Vce, Ib)"
                        };
                        chart.Title.Font.Size = 14;
                        chart.Title.Font.Bold = true;

                        chart.XAxis.Title.Text = "Collector-Emitter Voltage V_CE [V]";
                        chart.YAxis.Title.Text = "Collector Current I_C [mA]";

                        // Add a series for each curve
                        int endRow = maxPoints + 1;
                        for (int c = 0; c < curves.Count; c++)
                        {
                            int col = c + 2;
                            var xRange = wsData.Cells[2, 1, endRow, 1];
                            var yRange = wsData.Cells[2, col, endRow, col];

                            string seriesTitle = preset.Category switch
                            {
                                DeviceCategory.MosfetN or DeviceCategory.MosfetP => $"Vgs = {curves[c].VIbMv / 1000.0:0.##}V",
                                DeviceCategory.Diode or DeviceCategory.Zener => "Forward/Reverse",
                                _ => $"Ib = {curves[c].IbUa:0.#} µA"
                            };

                            var series = chart.Series.Add(yRange, xRange);
                            series.Header = seriesTitle;
                        }

                        // Position chart on right side of data
                        int chartCol = curves.Count + 4;
                        chart.SetPosition(1, 0, chartCol, 0);
                        chart.SetSize(800, 500);
                    }
                    catch { }
                }

                package.Save();
            }
        }

        public static void ExportToCsv(string filePath, List<CurveData> curves)
        {
            using (var sw = new StreamWriter(filePath))
            {
                if (curves.Count == 0) return;
                // Header
                var headers = new List<string> { "V_DUT_V" };
                for (int c = 0; c < curves.Count; c++)
                {
                    headers.Add($"Curve_{c}_Ib_{curves[c].IbUa:0.#}uA_mA");
                }
                sw.WriteLine(string.Join(",", headers));

                int maxPoints = curves.Max(c => c.Points.Count);
                for (int i = 0; i < maxPoints; i++)
                {
                    var lineVals = new List<string>();
                    var pt0 = curves.FirstOrDefault(c => c.Points.Count > i)?.Points[i];
                    lineVals.Add(pt0 != null ? pt0.VDutV.ToString("0.###", CultureInfo.InvariantCulture) : "0");

                    for (int c = 0; c < curves.Count; c++)
                    {
                        if (i < curves[c].Points.Count)
                        {
                            lineVals.Add(curves[c].Points[i].IcMa.ToString("0.####", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            lineVals.Add(string.Empty);
                        }
                    }
                    sw.WriteLine(string.Join(",", lineVals));
                }
            }
        }
    }
}
