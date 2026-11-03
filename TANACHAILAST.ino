// GLOBAL LIMITS BY DESIGN
// = GainDAC * GainOpamp * Reference, Millivolt
const unsigned int MaxVoltA= 27034 ;        
const unsigned int MaxVoltB= 27034 ;       
unsigned int Voltage ;
unsigned int Current ;

unsigned int StartIB = 0; //= 650;     //  0 uA * 27 kR + 0.65V = 0.65V = 650 mV
unsigned int StopIB = 0; //= 14150;    // 500 uA * 27 kR + 0.65V = 2V = 2'000 mV
unsigned int StepIB = 0; //= 3375;     // 3.375 V / 27 kR = 125 uA

unsigned int StartUC=0; //= 0;
unsigned int StopUC=1; //= 10000;    // 10'000 mV = 10V
unsigned int StepUC=1; //= 100;      // 100 mV 

unsigned int Rb;

unsigned int Neviem;

float MaxPower= 2000 ;         // mW
float Power ;
float aux;
float n;
float I ;
// DAC PINs
int CS = 11;
int SCLK = 12;
int SDAT = 13;
int LOAD = 10;

float ncounter = false;



String C;

bool stopFlag = false;

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

void stopProgram() {

  StartUC = 0;
  StopUC = 0;
  StepUC = 0;

  StartIB = 0; 
  StopIB = 0; 
  StepIB = 0;

  SET_DAC_A(0) ;
  SET_DAC_B(0) ;
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
  //Info();
  // TITLE

}
// ///////////////////////////////////////////////////////////// 

void loop() {
  start:

  if (ncounter) {
          Serial.print("New");
          delay(2000);
          Serial.print('\n');
          ncounter = false;
          }
  
  //Serial.print("0");
   if (Serial.available() > 0) {
  String data = Serial.readString(); // Read the incoming data until a newline character is received

    int AIndex = data.indexOf(",");          //Min Volt UC
    int lastAIndex = data.lastIndexOf(",");
    
    int BIndex = data.indexOf(";");         //Max Volt UC
    int lastBIndex = data.lastIndexOf(";");
    
    int CIndex = data.indexOf("?");          //Step Volt UC   
    int lastCIndex = data.lastIndexOf("?");
    
    int DIndex = data.indexOf("!");           //Max I WARNING
    int lastDIndex = data.lastIndexOf("!");
    
    int EIndex = data.indexOf("@");           //Min Curent IB
    int lastEIndex = data.lastIndexOf("@");
    
    int FIndex = data.indexOf("#");          //Max Curent IB
    int lastFIndex = data.lastIndexOf("#");
    
    int GIndex = data.indexOf("&");         //Step IB
    int lastGIndex = data.lastIndexOf("&");

    int HIndex = data.indexOf("~");          //????????
    int lastHIndex = data.lastIndexOf("~");
    
    int IIndex = data.indexOf("^");           //Prememnna na vzpocet napetia z odporu a prudu
    int lastIIndex = data.lastIndexOf("^");

    char command = Serial.read();
        if (command == 'S') {
          stopProgram();
          ncounter = true;
          goto start;
    }

    // Check if both delimiters are found
    if (AIndex >= 0 && BIndex >= 0 && lastAIndex > AIndex && lastBIndex > BIndex) {
      // Separate the string into two parts
      String aString = data.substring(AIndex + 1, lastAIndex);
      String bString = data.substring(BIndex + 1, lastBIndex);
      String cString = data.substring(CIndex + 1, lastCIndex);
      String dString = data.substring(DIndex + 1, lastDIndex);
      String eString = data.substring(EIndex + 1, lastEIndex);
      String fString = data.substring(FIndex + 1, lastFIndex);
      String gString = data.substring(GIndex + 1, lastGIndex);
      String hString = data.substring(HIndex + 1, lastHIndex);
      String iString = data.substring(IIndex + 1, lastIIndex);
//Serial.print("1");
      // Convert the parts to integers
      StartUC = aString.toInt();
      StopUC = bString.toInt();
      StepUC = cString.toInt();
      int maxPower = dString.toInt();
      StartIB = eString.toInt();
      StopIB = fString.toInt();
      StepIB = gString.toInt();
      Neviem = hString.toInt() ;
      Rb = iString.toInt();
    }
  while ( MaxVoltA > StartUC > -1 && StartUC < StopUC < MaxVoltA && MaxVoltB > StartIB > -1 && -1 < StopIB < MaxVoltB&&StepUC > 0) {
for (unsigned int j = StartIB; j <= StopIB; j += StepIB){

      Serial.print("Ib = ");
      Serial.print(j);
      delay(2000);
      Serial.print('\n');
      SET_DAC_B(j) ;

      for (unsigned int i = StartUC; i <= StopUC; i += StepUC){
        //aux = i * 0.001;
      SET_DAC_A(i) ;
      delay(2000);
      
      Current = 0;
      for (int k=0; k<16; k++) {
        Current = Current + analogRead(A0);
      }
      aux = 5000.0 * Current / ( 16.0 * 1024.0 * 48.0 );
      C = String(aux);
      Serial.print(C); 
      Serial.print('\n');
      // OVERLOAD PROTECTION, POWER SUPPLY DELIVERS 33 V
      Power = (float)(33000 - i) * aux * 0.001 ;
      // Serial.print(" , ");Serial.println(Power); 
      if (Power > MaxPower) break ;

      if (Serial.available() > 0) {
          char command = Serial.read();
          if (command == 'S') {
            stopProgram();
            ncounter = true;
            goto start;
          }
        }
  }
         

Serial.print("New");
delay(2000);
Serial.print('\n');
ncounter = false;

if (Serial.available() > 0) {
          char command = Serial.read();
          if (command == 'S' && stopFlag) {
            stopProgram();
            goto start;
          }
          }

}
stopProgram();
goto start;

}
SET_DAC_A(0) ;
SET_DAC_B(0) ;
delay(10);
}

}
// ///////////////////////////////////////////////////////////// 
// END OF FILE.
// ///////////////////////////////////////////////////////////// 
