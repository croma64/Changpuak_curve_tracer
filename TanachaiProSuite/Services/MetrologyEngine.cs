using System;
using System.Collections.Generic;
using System.Linq;
using TanachaiProSuite.Models;

namespace TanachaiProSuite.Services
{
    public class MetrologyEngine
    {
        public static MetrologyResult Analyze(List<CurveData> curves, DeviceCategory category, double targetVce = 5.0)
        {
            var res = new MetrologyResult();
            if (curves == null || curves.Count == 0) return res;

            // Compute peak current & peak power across all points
            double maxPwr = 0;
            double maxIc = 0;
            foreach (var c in curves)
            {
                foreach (var p in c.Points)
                {
                    if (p.PowerMw > maxPwr) maxPwr = p.PowerMw;
                    if (p.IcMa > maxIc) maxIc = p.IcMa;
                }
            }
            res.PeakPowerMw = maxPwr;
            res.PeakCurrentMa = maxIc;

            switch (category)
            {
                case DeviceCategory.BjtNpn:
                case DeviceCategory.BjtPnp:
                    AnalyzeBjt(curves, targetVce, res);
                    break;

                case DeviceCategory.MosfetN:
                case DeviceCategory.MosfetP:
                case DeviceCategory.JfetN:
                    AnalyzeFet(curves, targetVce, res);
                    break;

                case DeviceCategory.Diode:
                    AnalyzeDiode(curves, res);
                    break;

                case DeviceCategory.Zener:
                    AnalyzeZener(curves, res);
                    break;

                default:
                    AnalyzeGeneric(curves, res);
                    break;
            }

            return res;
        }

        private static void AnalyzeBjt(List<CurveData> curves, double targetVce, MetrologyResult res)
        {
            var validCurves = curves.Where(c => c.IbUa > 1.0 && c.Points.Count >= 5).ToList();
            if (validCurves.Count == 0)
            {
                res.SummaryNotes = "Insufficient data points or Ib is zero.";
                return;
            }

            // Adapt targetVce if it falls outside the actual measured span
            double maxMeasuredV = validCurves.SelectMany(c => c.Points).Max(p => p.VDutV);
            double minMeasuredV = validCurves.SelectMany(c => c.Points).Min(p => p.VDutV);
            if (targetVce <= minMeasuredV || targetVce >= maxMeasuredV)
            {
                targetVce = minMeasuredV + 0.65 * (maxMeasuredV - minMeasuredV);
            }

            // Find operating points at targetVce
            var icAtTarget = new List<(double IbUa, double IcMa)>();
            foreach (var c in validCurves)
            {
                double ic = InterpolateIcAtV(c.Points, targetVce);
                if (!double.IsNaN(ic) && ic > 0.01)
                {
                    icAtTarget.Add((c.IbUa, ic));
                }
            }

            if (icAtTarget.Count > 0)
            {
                // DC gain hFE = Ic / Ib
                var hfeList = icAtTarget.Select(pt => (pt.IcMa * 1000.0) / pt.IbUa).Where(h => h > 1.0 && h < 2000.0).ToList();
                if (hfeList.Count > 0)
                {
                    res.HfeDc = hfeList.Average();
                }

                // AC small-signal gain hfe = dIc / dIb between adjacent curves
                if (icAtTarget.Count >= 2)
                {
                    var hfeAcList = new List<double>();
                    for (int i = 0; i < icAtTarget.Count - 1; i++)
                    {
                        double dIc = (icAtTarget[i + 1].IcMa - icAtTarget[i].IcMa) * 1000.0; // uA
                        double dIb = icAtTarget[i + 1].IbUa - icAtTarget[i].IbUa;
                        if (dIb > 0)
                        {
                            double gain = dIc / dIb;
                            if (gain > 1.0 && gain < 2000.0) hfeAcList.Add(gain);
                        }
                    }
                    if (hfeAcList.Count > 0)
                    {
                        res.HfeAc = hfeAcList.Average();
                    }
                }
            }

            // Early Voltage (Va) & Output Resistance (ro) in active linear region (Vce > 2V)
            var bestCurve = validCurves.OrderByDescending(c => c.IbUa).FirstOrDefault();
            if (bestCurve != null)
            {
                var activePoints = bestCurve.Points.Where(p => p.VDutV >= 2.0 && p.VDutV <= 20.0).ToList();
                if (activePoints.Count >= 5)
                {
                    // Linear regression: Ic = slope * Vce + intercept
                    LinearRegression(activePoints.Select(p => p.VDutV).ToArray(),
                                     activePoints.Select(p => p.IcMa).ToArray(),
                                     out double slope, out double intercept);

                    if (slope > 0.0001)
                    {
                        // ro = dV / dI = 1 / slope (slope is mA/V, so ro is in kOhm)
                        res.OutputResistanceRo = 1.0 / (slope * 0.001); // Ohms
                        // Va = intercept / slope (extrapolated to Ic = 0)
                        res.EarlyVoltageVa = Math.Abs(intercept / slope);
                    }
                }
            }

            res.SummaryNotes = $"BJT Analysis at Vce={targetVce:0.1}V: hFE(DC)={res.HfeDc:0.#}, hfe(AC)={res.HfeAc:0.#}, ro={res.OutputResistanceRo / 1000.0:0.#}kΩ, Va={res.EarlyVoltageVa:0.#}V";
        }

        private static void AnalyzeFet(List<CurveData> curves, double targetVds, MetrologyResult res)
        {
            var validCurves = curves.Where(c => c.Points.Count >= 5).ToList();
            if (validCurves.Count < 2) return;

            var idList = new List<(double VgsV, double IdMa)>();
            foreach (var c in validCurves)
            {
                double id = InterpolateIcAtV(c.Points, targetVds);
                if (!double.IsNaN(id))
                {
                    idList.Add((c.VIbMv / 1000.0, id));
                }
            }

            // Transconductance gm = dId / dVgs (mA/V = mS)
            var gmList = new List<double>();
            for (int i = 0; i < idList.Count - 1; i++)
            {
                double dId = idList[i + 1].IdMa - idList[i].IdMa;
                double dVgs = idList[i + 1].VgsV - idList[i].VgsV;
                if (Math.Abs(dVgs) > 0.05)
                {
                    gmList.Add(Math.Abs(dId / dVgs));
                }
            }

            if (gmList.Count > 0)
            {
                res.TransconductanceGm = gmList.Average(); // mS
            }

            res.SummaryNotes = $"FET Analysis at Vds={targetVds:0.1}V: Transconductance gm={res.TransconductanceGm:0.##} mS";
        }

        private static void AnalyzeDiode(List<CurveData> curves, MetrologyResult res)
        {
            if (curves.Count == 0 || curves[0].Points.Count < 5) return;
            var pts = curves[0].Points.OrderBy(p => p.VDutV).ToList();

            double vf1 = InterpolateVAtIc(pts, 1.0);
            double vf10 = InterpolateVAtIc(pts, 10.0);

            res.DiodeVf1mA = vf1;
            res.DiodeVf10mA = vf10;

            if (!double.IsNaN(vf1) && !double.IsNaN(vf10))
            {
                // Dynamic resistance rd = dV / dI = (Vf10 - Vf1) / (10mA - 1mA)
                res.DiodeDynamicRd = ((vf10 - vf1) / 0.009); // Ohms
            }

            res.SummaryNotes = $"Diode Analysis: Vf@1mA={res.DiodeVf1mA:0.###}V, Vf@10mA={res.DiodeVf10mA:0.###}V, Dynamic rd={res.DiodeDynamicRd:0.#}Ω";
        }

        private static void AnalyzeZener(List<CurveData> curves, MetrologyResult res)
        {
            if (curves.Count == 0 || curves[0].Points.Count < 5) return;
            var pts = curves[0].Points.OrderBy(p => p.VDutV).ToList();

            // Find Zener voltage Vz at 5mA and 10mA
            double vz5 = InterpolateVAtIc(pts, 5.0);
            double vz10 = InterpolateVAtIc(pts, 10.0);

            res.ZenerVz = !double.IsNaN(vz5) ? vz5 : InterpolateVAtIc(pts, 2.0);

            if (!double.IsNaN(vz5) && !double.IsNaN(vz10))
            {
                // Dynamic impedance Zz = (Vz10 - Vz5) / 5mA
                res.ZenerZz = (vz10 - vz5) / 0.005; // Ohms
            }

            res.SummaryNotes = $"Zener Analysis: Vz@5mA={res.ZenerVz:0.##}V, Dynamic Impedance Zz={res.ZenerZz:0.#}Ω";
        }

        private static void AnalyzeGeneric(List<CurveData> curves, MetrologyResult res)
        {
            res.SummaryNotes = $"Max Peak Current: {res.PeakCurrentMa:0.##} mA, Max Peak Power: {res.PeakPowerMw:0.#} mW";
        }

        private static double InterpolateIcAtV(List<DataPoint> pts, double targetV)
        {
            if (pts.Count == 0) return double.NaN;
            if (targetV < pts[0].VDutV || targetV > pts[^1].VDutV) return double.NaN;

            for (int i = 0; i < pts.Count - 1; i++)
            {
                if (pts[i].VDutV <= targetV && pts[i + 1].VDutV >= targetV)
                {
                    double v0 = pts[i].VDutV;
                    double v1 = pts[i + 1].VDutV;
                    if (Math.Abs(v1 - v0) < 1e-6) return pts[i].IcMa;
                    double t = (targetV - v0) / (v1 - v0);
                    return pts[i].IcMa + t * (pts[i + 1].IcMa - pts[i].IcMa);
                }
            }
            return pts[^1].IcMa;
        }

        private static double InterpolateVAtIc(List<DataPoint> pts, double targetIcMa)
        {
            if (pts.Count == 0) return double.NaN;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                if (pts[i].IcMa <= targetIcMa && pts[i + 1].IcMa >= targetIcMa)
                {
                    double i0 = pts[i].IcMa;
                    double i1 = pts[i + 1].IcMa;
                    if (Math.Abs(i1 - i0) < 1e-6) return pts[i].VDutV;
                    double t = (targetIcMa - i0) / (i1 - i0);
                    return pts[i].VDutV + t * (pts[i + 1].VDutV - pts[i].VDutV);
                }
            }
            return double.NaN;
        }

        private static void LinearRegression(double[] x, double[] y, out double slope, out double intercept)
        {
            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumX2 = x.Select(v => v * v).Sum();
            double sumXY = x.Zip(y, (a, b) => a * b).Sum();

            double denom = n * sumX2 - sumX * sumX;
            if (Math.Abs(denom) < 1e-9)
            {
                slope = 0;
                intercept = 0;
                return;
            }
            slope = (n * sumXY - sumX * sumY) / denom;
            intercept = (sumY - slope * sumX) / n;
        }
    }
}
