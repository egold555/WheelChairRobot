/*
  Digital Pot Control for Microchip 4251 dual 10k digital pot (SPI control)
  Darby Hewitt -- 7/11/2016

  based on example code created 10 Aug 2010
  by Tom Igoe
*/

#define W0ADDR 0x00
#define W1ADDR 0x01
#define WRITE  0x00
#define INCR   0x01
#define DECR   0x02
#define READ   0x03
#define WRITE124

// include the SPI library:
#include <SPI.h>


// set pin 10 as the slave select for the digital pot:
const int slaveSelectPin = 10;
const int readReferenceVoltagePin = A0;
const int readJoy1VoltagePin = A1;
const int readJoy2VoltagePin = A2;

// The analog value and milliVolts corresponding to the zero position.
int zeroValue;
int zeroMv;

// Current values and millivolts.
int joy1Value, joy1Mv;
int joy2Value, joy2Mv;

int currentFB = 100;
int currentLR = 100;

const bool monitorVoltage = false;

long milliLastStatus = 0;
long milliLastCommand = 0;

bool emergency = false;

// Convert an analog pin reading to milliVolts.
int analogToMv(int analog)
{
  return (int) (2 * (long)analog * 5000L / 1024L);
}

void printVoltage(int analog)
{
  int milliVolts = analogToMv(analog);
  Serial.print(milliVolts);
  Serial.print(" mV");
}


void setup() {
  Serial.begin(57600);
  Serial.setTimeout(100);

  // set the slaveSelectPin as an output:
  pinMode (slaveSelectPin, OUTPUT);

  pinMode(readJoy1VoltagePin, INPUT);

  // initialize SPI:

  SPI.begin();

  initialize();
}

void initialize()
{
  joystick1Write(200);
  joystick2Write(200);

  Serial.println("Waiting for wheelchair power...");

  while (analogToMv(analogRead(readReferenceVoltagePin)) < 5600) {
    delay(1);
  }

  Serial.println("Wheelchair power detected.");

  delay(50);

  calibrate();

  milliLastCommand = millis();
}


void loop() {
  if (Serial.available() > 0) {
    milliLastCommand = millis();

    int code = Serial.read();
    String input = Serial.readStringUntil('\n');
    if (code == 'x') {
      Serial.println("Resetting both to 100");
      currentLR = 100;
      currentFB = 100;
      setAbstractPositions(currentFB, currentLR);
    }

    else if (code == 'f') {
      int inVal = input.toInt();
      currentFB = inVal;
      Serial.print("Forward/back to: ");
      Serial.println(currentFB);
      if (!emergency) {
        setAbstractPositions(currentFB, currentLR);
      }
    }
    else if (code == 'l') {
      int inVal = input.toInt();
      currentLR = inVal;
      Serial.print("Left/right to: ");
      Serial.println(currentLR);
      if (!emergency) {
        setAbstractPositions(currentFB, currentLR);
      }
    }
    else if (code == 'c') {
      if (!emergency) {
        calibrate();
      }
    }
    else if (code == 'k') {
      // Keep alive. Nothing to do, because we already
      // reset milliLastCommand.
    }
    else {
      Serial.println("Unknown command");
    }
  }


  if (monitorVoltage && millis() > milliLastStatus + 3000) {
    int aread1 = analogRead(readJoy1VoltagePin);
    int aread2 = analogRead(readJoy2VoltagePin);
    int areadR = analogRead(readReferenceVoltagePin);
    Serial.print("REF: ");
    printVoltage(areadR);

    Serial.print("   J1: ");
    printVoltage(aread1);

    Serial.print("   J2: ");
    printVoltage(aread2);
    Serial.println();

    milliLastStatus = millis();
  }

  if (analogToMv(analogRead(readReferenceVoltagePin)) < 4000) {
    Serial.println("Wheelchair power lost. Resetting.");
    initialize();
  }

  // Dead-man switch. If we haven't got a command in 1.5 seconds, then
  // shut it all down.
  if (millis() > milliLastCommand + 1500) {
    // Stop everything!
    setAbstractPositions(100, 100);
    Serial.println("Emergency stop!");
    Serial.println("Reset Arduino to restart.");
    emergency = true;
  }
}


void setAbstractPositions(int forwardBack, int leftRight)
{
  setPositions(zeroMv - (forwardBack - 100) * 10, zeroMv + (leftRight - 100) * 10);
}

// Set the resistor values to reach the given voltages.
void setPositions(int mvJoy1, int mvJoy2)
{
  //  Serial.print("Attempting to get Joy1=");
  //  Serial.print(mvJoy1);
  //  Serial.print("  Joy2=");
  //  Serial.print(mvJoy2);
  //  Serial.println();

  // There is a slight amount of coupling between the voltages,
  // so we do both twice.
  setPositionJoy1(mvJoy1);
  setPositionJoy2(mvJoy2);
  setPositionJoy1(mvJoy1);
  setPositionJoy2(mvJoy2);
}

void setPositionJoy1(int newMv1)
{
  for (int i = 0; i < 10; ++i) {
    int expectedNewValue = joy1Value - ((newMv1 - joy1Mv) / 10);
    if (expectedNewValue == joy1Value && i >= 1) {
      break;
    }
    joystick1Write(expectedNewValue);
    delayMicroseconds(1000);
    int analogVoltage = analogRead(readJoy1VoltagePin);
    int newMv = analogToMv(analogVoltage);

    joy1Mv = newMv;
    joy1Value = expectedNewValue;
  }
}


void setPositionJoy2(int newMv2)
{
  for (int i = 0; i < 10; ++i) {
    int expectedNewValue = joy2Value - ((newMv2 - joy2Mv) / 10);
    if (expectedNewValue == joy2Value && i >= 1) {
      break;
    }
    joystick2Write(expectedNewValue);
    delayMicroseconds(1000);
    int analogVoltage = analogRead(readJoy2VoltagePin);
    int newMv = analogToMv(analogVoltage);

    joy2Mv = newMv;
    joy2Value = expectedNewValue;
  }
}

void calibrate()
{
  int value = 128;
  int adjust = 64;
  int referenceVoltage = analogRead(readReferenceVoltagePin);
  int voltage;
  //Serial.print("Reference voltage: ");
  //printVoltage(referenceVoltage);
  //Serial.println();
  do {
    //Serial.print("Adjusting to ");
    //Serial.println(value);

    joystick1Write(value);
    joystick2Write(value);
    delayMicroseconds(1000);
    voltage = analogRead(readJoy1VoltagePin);

    //Serial.print("Measured voltage: ");
    //printVoltage(voltage);
    //Serial.println();
    if (voltage == referenceVoltage) {
      //Serial.println("Adjustment stopped");
      break;
    }
    else if (voltage < referenceVoltage) {
      value -= adjust;
    }
    else {
      value += adjust;
    }
    adjust = adjust / 2;

  } while (adjust > 0);

  zeroValue = value;
  zeroMv = analogToMv(voltage);
  joy1Value = joy2Value = zeroValue;
  joy1Mv = joy2Mv = zeroMv;

  Serial.print("Calibration complete: value = ");
  Serial.print(value);
  Serial.print("  voltage = ");
  Serial.print(zeroMv);
  Serial.println();
}


void joystick1Write(int output)
{
  if (output < 0) output = 0;
  if (output > 255) output = 255;
  digitalPotWrite((W0ADDR << 12) | (WRITE << 8) | output);
}


void joystick2Write(int output)
{
  if (output < 0) output = 0;
  if (output > 255) output = 255;
  digitalPotWrite((W1ADDR << 12) | (WRITE << 8) | output);
}

void digitalPotWrite(int value) {
  // take the SS pin low to select the chip:
  digitalWrite(slaveSelectPin, LOW);
  delayMicroseconds(20);

  SPI.beginTransaction(SPISettings(1000000, MSBFIRST, SPI_MODE0));
  //  send in the address and value via SPI:
  //SPI.transfer(address);
  SPI.transfer(value >> 8);
  //delay(5);
  SPI.transfer(value);
  SPI.endTransaction();
  // take the SS pin high to de-select the chip:
  delayMicroseconds(20);
  digitalWrite(slaveSelectPin, HIGH);

}
