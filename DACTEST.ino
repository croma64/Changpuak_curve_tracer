// ///////////////////////////////////////////////////////////// 
/*
   
ARDUINO TRANSITOR CURVE TRACER "TANACHAI"
https://www.changpuak.ch/electronics/Arduino-Shield-TANACHAI.php
Software Version 1.0, 
09.05.2016, Alexander C. Frank

TransistorCurveTracer-TANACHAI-DACTEST.ino

*/
// ///////////////////////////////////////////////////////////// 

// GLOBAL LIMITS BY DESIGN
// = GainDAC * GainOpamp * Reference, Millivolt
const unsigned int MaxVoltA = 27034 ;        
const unsigned int MaxVoltB = 27034 ;       
unsigned int Voltage ;   // MILLIVOLT
unsigned int Current ;   // MILLIAMP
// DAC PINs
int CS = 11;
int SCLK = 12;
int SDAT = 13;
int LOAD = 10;

// ///////////////////////////////////////////////////////////// 

void SET_DAC_A(unsigned int VoltageA) {
  unsigned int pointer = 0x8000;
  if (VoltageA > MaxVoltA) VoltageA = MaxVoltA;
  // NO NEED TO CHANGE LINE BELOW, AS DAC's WITH LESS BITS IGNORE THE LSB's
  unsigned int DACA = (int)( VoltageA * pow(2, 12) / MaxVoltA );
  DACA |= 0x1000;   // GAIN = 2, DON'T SLEEP, Write to DACA
  digitalWrite(CS, LOW);
  for (int i=0; i < 16; i++) {
    if ((DACA & pointer) > 0) 
      { digitalWrite(SDAT, HIGH); }
        else { digitalWrite(SDAT, LOW); }
    digitalWrite(SCLK, HIGH);
    digitalWrite(SCLK, LOW);
    pointer = pointer >> 1;
  }
  digitalWrite(CS, HIGH);
  digitalWrite(LOAD, LOW);
  digitalWrite(LOAD, HIGH);  
}

// ///////////////////////////////////////////////////////////// 

void SET_DAC_B(unsigned int VoltageB) {
  unsigned int pointer = 0x8000;
  if (VoltageB > MaxVoltB) VoltageB = MaxVoltB;
  // NO NEED TO CHANGE LINE BELOW, AS DAC's WITH LESS BITS IGNORE THE LSB's
  unsigned int DACB = (int)( VoltageB * pow(2, 12) / MaxVoltB );
  DACB |= 0x9000 ;   // GAIN = 2, DON'T SLEEP, Write to DACB
  digitalWrite(CS, LOW);
  for (int i=0; i < 16; i++) {
    if ((DACB & pointer) > 0) 
      { digitalWrite(SDAT, HIGH); }
        else { digitalWrite(SDAT, LOW); }
    digitalWrite(SCLK, HIGH);
    digitalWrite(SCLK, LOW);
    pointer = pointer >> 1;
  }
  digitalWrite(CS, HIGH);
  digitalWrite(LOAD, LOW);
  digitalWrite(LOAD, HIGH);  
}

// ///////////////////////////////////////////////////////////// 

void Info() {
  Serial.println("\nARDUINO TRANSISTOR CURVE TRACER 'TANACHAI'");
  Serial.println("V3.0, (UNO + SPREADSHEET) 23.05.2016, BY CHANGPUAK.CH");
  Serial.println("DAC TESTROUTINE FOR TROUBLESHOOTING");
}

// ///////////////////////////////////////////////////////////// 

void setup() {
  Serial.begin(9600);
  pinMode(CS, OUTPUT);
  pinMode(SDAT, OUTPUT);
  pinMode(SCLK, OUTPUT);
  pinMode(LOAD, OUTPUT);
  digitalWrite(CS, HIGH);
  digitalWrite(SDAT, LOW);
  digitalWrite(SCLK, LOW);
  digitalWrite(LOAD, HIGH);
}

// ///////////////////////////////////////////////////////////// 

void loop() {
  for (int i=0; i<MaxVoltA; i+=32){
    SET_DAC_A(i) ;
    SET_DAC_B(i) ;
  }
}
// ///////////////////////////////////////////////////////////// 
// END OF FILE.
// ///////////////////////////////////////////////////////////// 
