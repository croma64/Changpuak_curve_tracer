using System;
using System.Collections.Generic;

namespace TanachaiProSuite.Models
{
    public class DataPoint
    {
        public int VcSetMv { get; set; }
        public int VDutMv { get; set; }
        public double IcMa { get; set; }
        public double PowerMw { get; set; }

        public double VDutV => VDutMv / 1000.0;
        public double VcSetV => VcSetMv / 1000.0;

        public DataPoint(int vcSetMv, int vDutMv, double icMa, double powerMw)
        {
            VcSetMv = vcSetMv;
            VDutMv = vDutMv;
            IcMa = icMa;
            PowerMw = powerMw;
        }
    }

    public class CurveData
    {
        public int CurveIndex { get; set; }
        public int VIbMv { get; set; }
        public double IbUa { get; set; }
        public List<DataPoint> Points { get; set; } = new List<DataPoint>();

        public CurveData(int curveIndex, int vIbMv, double ibUa)
        {
            CurveIndex = curveIndex;
            VIbMv = vIbMv;
            IbUa = ibUa;
        }
    }

    public enum DeviceCategory
    {
        BjtNpn,
        BjtPnp,
        MosfetN,
        MosfetP,
        JfetN,
        Diode,
        Zener,
        Resistor,
        Custom
    }

    public class DevicePreset
    {
        public string Name { get; set; }
        public DeviceCategory Category { get; set; }
        public int VcStartMv { get; set; }
        public int VcStopMv { get; set; }
        public int VcStepMv { get; set; }
        public int IbStartMv { get; set; }
        public int IbStopMv { get; set; }
        public int IbStepMv { get; set; }
        public int MaxPowerMw { get; set; }
        public int SettlingDelayMs { get; set; }
        public int RbOhm { get; set; }
        public string PinoutInfo { get; set; }

        public DevicePreset(string name, DeviceCategory cat, int vcStart, int vcStop, int vcStep,
            int ibStart, int ibStop, int ibStep, int pMax, int settle, int rb, string pinout)
        {
            Name = name;
            Category = cat;
            VcStartMv = vcStart;
            VcStopMv = vcStop;
            VcStepMv = vcStep;
            IbStartMv = ibStart;
            IbStopMv = ibStop;
            IbStepMv = ibStep;
            MaxPowerMw = pMax;
            SettlingDelayMs = settle;
            RbOhm = rb;
            PinoutInfo = pinout;
        }

        public static List<DevicePreset> GetStandardPresets()
        {
            return new List<DevicePreset>
            {
                new DevicePreset(
                    "BJT NPN - Small Signal (BC547, 2N2222, BC847)",
                    DeviceCategory.BjtNpn,
                    vcStart: 0, vcStop: 10000, vcStep: 100,
                    ibStart: 650, ibStop: 8750, ibStep: 2025, // Ib ~ 0uA to ~300uA in 75uA steps with Rb=27k
                    pMax: 500, settle: 8, rb: 27000,
                    "0: GND | 1: EMITTER (Shunt) | 2: BASE (via Rb) | 3: COLLECTOR (+Vc)"
                ),
                new DevicePreset(
                    "BJT NPN - Medium Power (BC141, BD139, 2N3053)",
                    DeviceCategory.BjtNpn,
                    vcStart: 0, vcStop: 15000, vcStep: 150,
                    ibStart: 650, ibStop: 14150, ibStep: 3375, // Ib ~ 0uA to ~500uA in 125uA steps
                    pMax: 2000, settle: 10, rb: 27000,
                    "0: GND | 1: EMITTER (Shunt) | 2: BASE (via Rb) | 3: COLLECTOR (+Vc)"
                ),
                new DevicePreset(
                    "BJT PNP - Small/Medium (BC557, BC140, 2N2907)",
                    DeviceCategory.BjtPnp,
                    vcStart: 0, vcStop: 12000, vcStep: 100,
                    ibStart: 650, ibStop: 14150, ibStep: 3375,
                    pMax: 1500, settle: 10, rb: 27000,
                    "0: GND | 1: COLLECTOR (Shunt) | 2: BASE (via Rb) | 3: EMITTER (+Ve)"
                ),
                new DevicePreset(
                    "MOSFET N-Channel (2N7000, BS170, IRF510)",
                    DeviceCategory.MosfetN,
                    vcStart: 0, vcStop: 15000, vcStep: 150,
                    ibStart: 1500, ibStop: 4500, ibStep: 500, // V_GS from 1.5V to 4.5V in 0.5V steps
                    pMax: 2000, settle: 12, rb: 27000,
                    "0: GND | 1: SOURCE (Shunt) | 2: GATE (+Vgs) | 3: DRAIN (+Vds)"
                ),
                new DevicePreset(
                    "JFET N-Channel (J309, J310, BF245)",
                    DeviceCategory.JfetN,
                    vcStart: 0, vcStop: 15000, vcStep: 150,
                    ibStart: 0, ibStop: 0, ibStep: 0, // Vgs = 0 for Idss curve
                    pMax: 500, settle: 10, rb: 27000,
                    "0: GND | 1: DRAIN (Shunt) | 2: GATE (0V / Bias) | 3: SOURCE (+Vs)"
                ),
                new DevicePreset(
                    "Diode - Silicon / Schottky / LED",
                    DeviceCategory.Diode,
                    vcStart: 0, vcStop: 2500, vcStep: 25,
                    ibStart: 0, ibStop: 0, ibStep: 0,
                    pMax: 300, settle: 5, rb: 27000,
                    "0: GND | 1: CATHODE (Shunt) | 2: N.C. | 3: ANODE (+V)"
                ),
                new DevicePreset(
                    "Zener Diode (< 22V Breakdown)",
                    DeviceCategory.Zener,
                    vcStart: 0, vcStop: 22000, vcStep: 100,
                    ibStart: 0, ibStop: 0, ibStep: 0,
                    pMax: 1000, settle: 8, rb: 27000,
                    "0: GND | 1: ANODE (Shunt) | 2: N.C. | 3: CATHODE (+Vz Reverse)"
                ),
                new DevicePreset(
                    "Resistor / Varistor (< 27V, < 100mA)",
                    DeviceCategory.Resistor,
                    vcStart: 0, vcStop: 15000, vcStep: 200,
                    ibStart: 0, ibStop: 0, ibStep: 0,
                    pMax: 1500, settle: 5, rb: 27000,
                    "0: GND | 1: LEAD 1 (Shunt) | 2: N.C. | 3: LEAD 2 (+V)"
                ),
                new DevicePreset(
                    "Custom Configuration",
                    DeviceCategory.Custom,
                    vcStart: 0, vcStop: 10000, vcStep: 100,
                    ibStart: 0, ibStop: 5000, ibStep: 1000,
                    pMax: 2000, settle: 8, rb: 27000,
                    "Configure terminals according to component datasheet"
                )
            };
        }
    }

    public class MetrologyResult
    {
        public double HfeDc { get; set; } = double.NaN;
        public double HfeAc { get; set; } = double.NaN;
        public double EarlyVoltageVa { get; set; } = double.NaN;
        public double OutputResistanceRo { get; set; } = double.NaN;
        public double TransconductanceGm { get; set; } = double.NaN;
        public double DiodeVf1mA { get; set; } = double.NaN;
        public double DiodeVf10mA { get; set; } = double.NaN;
        public double DiodeDynamicRd { get; set; } = double.NaN;
        public double ZenerVz { get; set; } = double.NaN;
        public double ZenerZz { get; set; } = double.NaN;
        public double PeakPowerMw { get; set; } = 0.0;
        public double PeakCurrentMa { get; set; } = 0.0;
        public string SummaryNotes { get; set; } = string.Empty;
    }
}
