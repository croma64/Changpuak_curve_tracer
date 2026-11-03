// ///////////////////////////////////////////////////////////// 
/*
   
ARDUINO TRANSITOR CURVE TRACER "TANACHAI"
https://www.changpuak.ch/electronics/Arduino-Shield-TANACHAI.php
Software Version 1.0, 
09.05.2016, Alexander C. Frank

Suiteable for : two port devices like zenerdiodes, resistors, ...

*/
// ///////////////////////////////////////////////////////////// 

// GLOBAL LIMITS BY DESIGN
// = GainDAC * GainOpamp * Reference, Millivolt
const unsigned int MaxVoltA = 27034 ;        
unsigned int Current ;   // MILLIAMP
// DAC PINs
int CS = 11;
int SCLK = 12;
int SDAT = 13;
int LOAD = 10;
// USER DEFINED SETTINGS FOR SWEEP
unsigned int Start = 0;         // ALL VALUES IN MILLIVOLT
unsigned int Stop = 15000;      // EQUALS 15 VOLTS :-)
unsigned int Step = 100 ;       // DELTA VOLTAGE, = 0.1 V
float MaxPower = 2000;          // MILLIWATT
float Power = 0;
float aux ;

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

void Info() {
  Serial.println("\nARDUINO TRANSISTOR CURVE TRACER 'TANACHAI'");
  Serial.println("V3.0, (UNO + SPREADSHEET) 24.05.2016, BY CHANGPUAK.CH");
  Serial.println("FOR TWO PORT DEVICES LIKE ZENERDIODES, RESISTORS, ...");
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

  // RUN THE SWEEP ONCE
  Info();
  // TITLE
  Serial.print("VOLT, CURRENT\n");    
  for (int i=Start; i<=Stop;i+=Step){
    aux = i * 0.001;
    Serial.print(aux,1);
    Serial.print(" , ");
    SET_DAC_A(i) ;
    delay(10);
    Current = 0;
    for (int j=0; j<16; j++) {
      Current = Current + analogRead(A0);
    }
    aux = 5000.0 * Current / ( 16.0 * 1024.0 * 48.0 );
    Serial.print(aux,2); 
    Serial.print("\n");
    // OVERLOAD PROTECTION, POWER SUPPLY DELIVERS 33 V
    Power = (float)(33000 - i) * aux * 0.001 ;
    // Serial.print(" , ");Serial.println(Power); 
    if (Power > MaxPower) break ;
  }
}

// ///////////////////////////////////////////////////////////// 

void loop() {
  SET_DAC_A(0) ;
  delay(999);
}
// ///////////////////////////////////////////////////////////// 
// END OF FILE.
// ///////////////////////////////////////////////////////////// 
