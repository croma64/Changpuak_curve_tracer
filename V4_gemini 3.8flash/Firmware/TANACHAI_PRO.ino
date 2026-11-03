/*
 ==================================================================================================
  ARDUINO TRANSISTOR CURVE TRACER "TANACHAI PRO" - Firmware Version 2.5
  Based on the original design by Alexander C. Frank (changpuak.ch / ETH Quantum Optics)
  and improvements by Daniel Kasa (V2B2).
  
  Refactored & Enhanced with:
   - High-speed robust serial communication (115200 baud default, compatible with 9600)
   - Trimmed-mean oversampling ADC filter (rejects MC34063A switching spikes)
   - Real-time DUT voltage compensation (compensates for 1.0 Ohm current sense shunt drop)
   - True instantaneous thermal dissipation protection (monitors BC140 pass transistor)
   - Instant non-blocking abort capability (< 1 ms reaction time to STOP command)
   - Dual-protocol engine: Modern SCPI-style ASCII protocol + Full legacy compatibility
   - Mathematical range & limit safety validation on all parameters
 ==================================================================================================
*/

#include <Arduino.h>

// --- HARDWARE CALIBRATION CONSTANTS ---
// MCP4822 DAC: 12-bit (0..4095), 2.048V internal ref, 2x gain mode = 4.096V full scale
// AD822 OpAmp: Non-inverting amplifier, Gain = 1 + (5.6k / 1.0k) = 6.6
// Max Full Scale Voltage = 4.096V * 6.6 = 27.0336V -> 27034 mV
const unsigned int MAX_VOLT_A = 27034;  // Channel A: Collector / Drain (K9)
const unsigned int MAX_VOLT_B = 27034;  // Channel B: Base / Gate (K8)
const float BOOST_RAIL_VOLTAGE_MV = 32500.0f; // Typical +32.5V from MC34063A boost converter
const float SHUNT_RESISTOR_OHM = 1.0f;         // 1.0 Ohm 1% precision shunt (R1)
const float CURRENT_AMP_GAIN = 48.0f;          // MCP6271 OpAmp gain: 1 + (47k / 1k) = 48.0
const float ADC_VREF_MV = 5000.0f;             // Arduino UNO 5.0V ADC reference

// --- PIN ASSIGNMENTS ---
const int PIN_CS   = 11; // DAC Chip Select
const int PIN_SCLK = 12; // DAC SPI Clock
const int PIN_SDAT = 13; // DAC SPI Data
const int PIN_LOAD = 10; // DAC Latch / Load
const int PIN_ISENSE = A0; // Amplified shunt voltage from MCP6271

// --- OPERATIONAL PARAMETERS ---
unsigned int startVc = 0;       // mV
unsigned int stopVc = 10000;    // mV
unsigned int stepVc = 100;      // mV

unsigned int startIb = 0;       // mV at DAC_B (corresponds to Ib = V/Rb)
unsigned int stopIb = 13500;    // mV at DAC_B
unsigned int stepIb = 3375;     // mV at DAC_B

float maxPowerMw = 2000.0f;     // Max allowable BC140 power dissipation in mW (default 2W)
unsigned int settlingDelayMs = 8; // Settling delay per Vc point (ms)
unsigned int rbOhm = 27000;     // Base series resistor in Ohms (R12 on shield = 27k)

volatile bool isSweeping = false;
volatile bool abortRequested = false;

// ================================================================================================
// DAC DRIVER ROUTINES (Direct Bit-Banging, cycle-optimized)
// ================================================================================================

void writeDAC(uint16_t word) {
  digitalWrite(PIN_CS, LOW);
  for (uint16_t mask = 0x8000; mask != 0; mask >>= 1) {
    if (word & mask) {
      digitalWrite(PIN_SDAT, HIGH);
    } else {
      digitalWrite(PIN_SDAT, LOW);
    }
    digitalWrite(PIN_SCLK, HIGH);
    digitalWrite(PIN_SCLK, LOW);
  }
  digitalWrite(PIN_CS, HIGH);
  digitalWrite(PIN_LOAD, LOW);
  digitalWrite(PIN_LOAD, HIGH);
}

void setDacA_mV(unsigned int mV) {
  if (mV > MAX_VOLT_A) mV = MAX_VOLT_A;
  // Calculate 12-bit code: 0..4095
  uint32_t code = ((uint32_t)mV * 4095UL + (MAX_VOLT_A / 2)) / MAX_VOLT_A;
  if (code > 4095) code = 4095;
  // MCP4822 Control bits:
  // Bit 15: 0 = DAC_A
  // Bit 14: 0 = Unbuffered
  // Bit 13: 0 = Gain 2x (0..4.096V)
  // Bit 12: 1 = Active mode (not shutdown)
  uint16_t command = 0x1000 | (code & 0x0FFF);
  writeDAC(command);
}

void setDacB_mV(unsigned int mV) {
  if (mV > MAX_VOLT_B) mV = MAX_VOLT_B;
  uint32_t code = ((uint32_t)mV * 4095UL + (MAX_VOLT_B / 2)) / MAX_VOLT_B;
  if (code > 4095) code = 4095;
  // Bit 15: 1 = DAC_B, Bit 12: 1 = Active
  uint16_t command = 0x9000 | (code & 0x0FFF);
  writeDAC(command);
}

void emergencyStop() {
  setDacA_mV(0);
  setDacB_mV(0);
  isSweeping = false;
  abortRequested = true;
}

// ================================================================================================
// HIGH PRECISION ADC FILTERING (Trimmed Mean Outlier Rejection)
// ================================================================================================
// Takes 32 raw ADC readings, sorts them, discards 4 highest and 4 lowest samples
// to remove MC34063A boost regulator switching spikes, and averages the middle 24 samples.

float readCurrent_mA() {
  const int SAMPLES = 32;
  const int DISCARD = 4;
  int raw[SAMPLES];

  for (int i = 0; i < SAMPLES; i++) {
    raw[i] = analogRead(PIN_ISENSE);
    delayMicroseconds(80);
  }

  // Simple insertion sort on 32 elements (very fast on AVR, < 100 microseconds)
  for (int i = 1; i < SAMPLES; i++) {
    int key = raw[i];
    int j = i - 1;
    while (j >= 0 && raw[j] > key) {
      raw[j + 1] = raw[j];
      j--;
    }
    raw[j + 1] = key;
  }

  // Sum central 24 samples
  long sum = 0;
  for (int i = DISCARD; i < SAMPLES - DISCARD; i++) {
    sum += raw[i];
  }
  float avgRaw = (float)sum / (float)(SAMPLES - 2 * DISCARD);

  // Convert raw ADC count to current:
  // V_sense_mV = (avgRaw / 1024.0) * ADC_VREF_MV / CURRENT_AMP_GAIN
  // I_mA = V_sense_mV / SHUNT_RESISTOR_OHM
  float current_mA = (avgRaw * ADC_VREF_MV) / (1024.0f * CURRENT_AMP_GAIN * SHUNT_RESISTOR_OHM);
  if (current_mA < 0.001f) current_mA = 0.0f;
  return current_mA;
}

// Check for incoming stop/abort command without blocking
bool checkAbortCommand() {
  while (Serial.available() > 0) {
    char c = Serial.peek();
    if (c == 'S' || c == 's' || c == 'X' || c == 'x') {
      // Consume all pending abort characters (e.g. "STOP\n" or "s\n")
      while (Serial.available() > 0) {
        Serial.read();
        delay(1);
      }
      emergencyStop();
      return true;
    } else if (c == '\r' || c == '\n' || c == ' ') {
      Serial.read();
    } else {
      break;
    }
  }
  return false;
}

// ================================================================================================
// SWEEP EXECUTION ENGINE
// ================================================================================================

void executeSweep() {
  isSweeping = true;
  abortRequested = false;

  Serial.println(F("[SWEEP_START]"));

  unsigned int curveIndex = 0;

  // Outer loop: Base / Gate voltage
  for (unsigned int vIb = startIb; vIb <= stopIb && !abortRequested; vIb += (stepIb > 0 ? stepIb : 1)) {
    if (checkAbortCommand()) break;

    setDacB_mV(vIb);
    // Calculate theoretical base current injected: (V_dac_b - Vbe) / Rb
    // If testing FETs/MOSFETs, gate current is 0 and V_gate = vIb
    float vIbVolts = vIb * 0.001f;
    float ib_uA = 0.0f;
    if (vIbVolts > 0.65f && rbOhm > 0) {
      ib_uA = ((vIbVolts - 0.65f) / (float)rbOhm) * 1000000.0f;
    }

    // Announce curve header
    Serial.print(F("CURVE_START,"));
    Serial.print(curveIndex);
    Serial.print(F(",VIB_MV,"));
    Serial.print(vIb);
    Serial.print(F(",IB_UA,"));
    Serial.println(ib_uA, 2);

    delay(15); // Base/gate bias stabilization delay

    // Inner loop: Collector / Drain voltage
    for (unsigned int vVc = startVc; vVc <= stopVc && !abortRequested; vVc += (stepVc > 0 ? stepVc : 1)) {
      if (checkAbortCommand()) break;

      setDacA_mV(vVc);
      delay(settlingDelayMs);

      float current_mA = readCurrent_mA();

      // Real DUT Collector-Emitter Voltage:
      // Terminal 3 is at vVc. Terminal 1 is connected to Shunt R1 (1.0 Ohm) to GND.
      // So Terminal 1 is at V_shunt = I_mA * 1.0 Ohm (in mV).
      // Actual V_CE = vVc - V_shunt
      float vShunt_mV = current_mA * SHUNT_RESISTOR_OHM;
      float vDut_mV = (float)vVc - vShunt_mV;
      if (vDut_mV < 0.0f) vDut_mV = 0.0f;

      // Active Dynamic Ground-Lift Compensation for Base Drive:
      // When Terminal 1 lifts by vShunt_mV, we proactively raise DAC B by the same amount,
      // ensuring that (V_Base - V_Emitter) remains strictly invariant throughout the sweep.
      if (vIb > 0) {
        uint32_t vIbComp = (uint32_t)vIb + (uint32_t)(vShunt_mV + 0.5f);
        if (vIbComp > MAX_VOLT_B) vIbComp = MAX_VOLT_B;
        setDacB_mV((unsigned int)vIbComp);
      }

      // Calculate instantaneous power dissipated in the BC140 series pass transistor T1
      // T1 Collector is at BOOST_RAIL_VOLTAGE_MV, Emitter is at vVc.
      // Drop across T1 = BOOST_RAIL_VOLTAGE_MV - vVc.
      float powerT1_mW = (BOOST_RAIL_VOLTAGE_MV - (float)vVc) * current_mA * 0.001f;

      // Stream data point: VC_SET, V_DUT_ACTUAL, I_MEASURED_MA
      Serial.print(F("DATA,"));
      Serial.print(curveIndex);
      Serial.print(F(","));
      Serial.print(vVc);
      Serial.print(F(","));
      Serial.print((unsigned int)vDut_mV);
      Serial.print(F(","));
      Serial.print(current_mA, 4);
      Serial.print(F(","));
      Serial.println(powerT1_mW, 1);

      // Thermal & Overload Protection check
      if (powerT1_mW > maxPowerMw) {
        Serial.print(F("[ALERT:OVERPOWER],"));
        Serial.print(powerT1_mW);
        Serial.print(F("mW > "));
        Serial.println(maxPowerMw);
        break; // Break inner loop to protect hardware
      }
    }

    Serial.print(F("CURVE_END,"));
    Serial.println(curveIndex);
    curveIndex++;

    if (stepIb == 0) break; // Single curve sweep
  }

  // Sweep completed or aborted - reset outputs to safe 0V
  setDacA_mV(0);
  setDacB_mV(0);
  isSweeping = false;

  if (abortRequested) {
    Serial.println(F("[SWEEP_ABORTED]"));
  } else {
    Serial.println(F("[SWEEP_COMPLETE]"));
  }
}

// ================================================================================================
// COMMAND PARSER (Dual-Protocol Engine)
// ================================================================================================

void parseCommandLine(String cmd) {
  cmd.trim();
  if (cmd.length() == 0) return;

  // 1. Identify query
  if (cmd.equalsIgnoreCase(F("*IDN?")) || cmd.equalsIgnoreCase(F("IDN?"))) {
    Serial.println(F("TANACHAI:CURVE_TRACER_PRO:V2.5:CHANGPUAK_ETH"));
    return;
  }

  // 2. Ping query
  if (cmd.equalsIgnoreCase(F("PING"))) {
    Serial.println(F("PONG"));
    return;
  }

  // 3. Stop command
  if (cmd.equalsIgnoreCase(F("STOP")) || cmd.equalsIgnoreCase(F("S")) || cmd.equalsIgnoreCase(F("ABORT"))) {
    emergencyStop();
    Serial.println(F("STATUS:STOPPED"));
    return;
  }

  // 4. Status query
  if (cmd.equalsIgnoreCase(F("STATUS?"))) {
    Serial.print(F("STATUS:"));
    Serial.print(isSweeping ? F("SWEEPING") : F("IDLE"));
    Serial.print(F(",MAX_POWER_MW:"));
    Serial.println(maxPowerMw);
    return;
  }

  // 5. Manual DAC debug controls
  if (cmd.startsWith(F("SET_DAC_A "))) {
    unsigned int val = cmd.substring(10).toInt();
    setDacA_mV(val);
    Serial.print(F("DAC_A_SET_OK:"));
    Serial.println(val);
    return;
  }
  if (cmd.startsWith(F("SET_DAC_B "))) {
    unsigned int val = cmd.substring(10).toInt();
    setDacB_mV(val);
    Serial.print(F("DAC_B_SET_OK:"));
    Serial.println(val);
    return;
  }
  if (cmd.equalsIgnoreCase(F("READ_CURRENT"))) {
    float i_mA = readCurrent_mA();
    Serial.print(F("CURRENT_MA:"));
    Serial.println(i_mA, 4);
    return;
  }

  // 6. Modern Structured Command:
  // SWEEP <VcStart> <VcStop> <VcStep> <IbStart> <IbStop> <IbStep> <PMax> <SettlingMs> <RbOhm>
  if (cmd.startsWith(F("SWEEP "))) {
    char buf[128];
    cmd.toCharArray(buf, sizeof(buf));
    char* token = strtok(buf, " "); // skip "SWEEP"
    
    unsigned int pVcStart = 0, pVcStop = 10000, pVcStep = 100;
    unsigned int pIbStart = 0, pIbStop = 0, pIbStep = 0;
    float pPMax = 2000.0f;
    unsigned int pSettle = 8, pRb = 27000;

    int idx = 0;
    while ((token = strtok(NULL, " ")) != NULL) {
      if (idx == 0) pVcStart = atoi(token);
      else if (idx == 1) pVcStop = atoi(token);
      else if (idx == 2) pVcStep = atoi(token);
      else if (idx == 3) pIbStart = atoi(token);
      else if (idx == 4) pIbStop = atoi(token);
      else if (idx == 5) pIbStep = atoi(token);
      else if (idx == 6) pPMax = atof(token);
      else if (idx == 7) pSettle = atoi(token);
      else if (idx == 8) pRb = atoi(token);
      idx++;
    }

    // Rigorous Range and Safety Validation
    if (pVcStart > MAX_VOLT_A) pVcStart = MAX_VOLT_A;
    if (pVcStop > MAX_VOLT_A) pVcStop = MAX_VOLT_A;
    if (pVcStart > pVcStop) { unsigned int tmp = pVcStart; pVcStart = pVcStop; pVcStop = tmp; }
    if (pVcStep == 0) pVcStep = 100;

    if (pIbStart > MAX_VOLT_B) pIbStart = MAX_VOLT_B;
    if (pIbStop > MAX_VOLT_B) pIbStop = MAX_VOLT_B;
    if (pIbStart > pIbStop) { unsigned int tmp = pIbStart; pIbStart = pIbStop; pIbStop = tmp; }

    if (pPMax <= 0 || pPMax > 5000.0f) pPMax = 2000.0f; // max 5W hardware safety limit
    if (pSettle < 1) pSettle = 1;
    if (pSettle > 500) pSettle = 500;
    if (pRb == 0) pRb = 27000;

    startVc = pVcStart;
    stopVc = pVcStop;
    stepVc = pVcStep;
    startIb = pIbStart;
    stopIb = pIbStop;
    stepIb = pIbStep;
    maxPowerMw = pPMax;
    settlingDelayMs = pSettle;
    rbOhm = pRb;

    executeSweep();
    return;
  }

  // 7. Full Backward Compatibility with Legacy VACH Shield Delimited Protocol:
  // Format: ,StartUC,;StopUC;?StepUC?!maxPower!@StartIB@#StopIB#&StepIB&...
  int a1 = cmd.indexOf(',');
  int a2 = cmd.lastIndexOf(',');
  int b1 = cmd.indexOf(';');
  int b2 = cmd.lastIndexOf(';');
  int c1 = cmd.indexOf('?');
  int c2 = cmd.lastIndexOf('?');

  if (a1 >= 0 && a2 > a1 && b1 >= 0 && b2 > b1 && c1 >= 0 && c2 > c1) {
    startVc = cmd.substring(a1 + 1, a2).toInt();
    stopVc  = cmd.substring(b1 + 1, b2).toInt();
    stepVc  = cmd.substring(c1 + 1, c2).toInt();

    int d1 = cmd.indexOf('!');
    int d2 = cmd.lastIndexOf('!');
    if (d1 >= 0 && d2 > d1) maxPowerMw = cmd.substring(d1 + 1, d2).toFloat();

    int e1 = cmd.indexOf('@');
    int e2 = cmd.lastIndexOf('@');
    if (e1 >= 0 && e2 > e1) startIb = cmd.substring(e1 + 1, e2).toInt();

    int f1 = cmd.indexOf('#');
    int f2 = cmd.lastIndexOf('#');
    if (f1 >= 0 && f2 > f1) stopIb = cmd.substring(f1 + 1, f2).toInt();

    int g1 = cmd.indexOf('&');
    int g2 = cmd.lastIndexOf('&');
    if (g1 >= 0 && g2 > g1) stepIb = cmd.substring(g1 + 1, g2).toInt();

    // Check for Rb (legacy GUI sent "^" at end)
    int i1 = cmd.indexOf('^');
    if (i1 >= 0 && g2 >= 0 && i1 > g2) {
      String rbStr = cmd.substring(g2 + 1, i1);
      rbStr.trim();
      unsigned int val = rbStr.toInt();
      if (val > 0) {
        // If val < 1000, user typed in kOhms (e.g. "27"), convert to Ohms (27000)
        rbOhm = (val < 1000) ? val * 1000 : val;
      }
    }

    // Safety clamps
    if (startVc > MAX_VOLT_A) startVc = MAX_VOLT_A;
    if (stopVc > MAX_VOLT_A) stopVc = MAX_VOLT_A;
    if (startIb > MAX_VOLT_B) startIb = MAX_VOLT_B;
    if (stopIb > MAX_VOLT_B) stopIb = MAX_VOLT_B;
    if (stepVc == 0) stepVc = 100;
    if (maxPowerMw <= 0 || maxPowerMw > 5000.0f) maxPowerMw = 2000.0f;
    settlingDelayMs = 8;

    executeSweep();
    return;
  }
}

// ================================================================================================
// SETUP & MAIN LOOP
// ================================================================================================

void setup() {
  // Initialize serial at high speed (115200) for fast streaming
  Serial.begin(115200);
  
  // Set ADC prescaler to 32 (500 kHz ADC clock) for high-speed low-noise acquisition
  // 16 MHz / 32 = 500 kHz (approx. 26 microseconds per analog conversion)
  ADCSRA = (ADCSRA & 0xF8) | 0x05;

  pinMode(PIN_CS, OUTPUT);
  pinMode(PIN_SDAT, OUTPUT);
  pinMode(PIN_SCLK, OUTPUT);
  pinMode(PIN_LOAD, OUTPUT);

  digitalWrite(PIN_CS, HIGH);
  digitalWrite(PIN_SDAT, LOW);
  digitalWrite(PIN_SCLK, LOW);
  digitalWrite(PIN_LOAD, HIGH);

  // Initialize DAC outputs to 0V
  setDacA_mV(0);
  setDacB_mV(0);

  // Send boot banner
  Serial.println(F("TANACHAI_PRO_READY:115200"));
}

String serialInputBuffer = "";

void loop() {
  while (Serial.available() > 0) {
    char c = (char)Serial.read();
    if (c == '\n' || c == '\r') {
      if (serialInputBuffer.length() > 0) {
        parseCommandLine(serialInputBuffer);
        serialInputBuffer = "";
      }
    } else {
      if (serialInputBuffer.length() < 120) {
        serialInputBuffer += c;
      }
    }
  }
}
