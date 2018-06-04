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

int zeroValue;

const bool monitorVoltage = false;

void setup() {
  Serial.begin(57600);
  Serial.setTimeout(100);
  
  // set the slaveSelectPin as an output:
  pinMode (slaveSelectPin, OUTPUT);

  pinMode(readJoy1VoltagePin, INPUT);

  // initialize SPI:

  SPI.begin();

  joystick1Write(128);
  joystick2Write(128);
  Serial.println("Initialized both to 128");

  calibrate();
}

long milliLastStatus;

void loop() {
  if (Serial.available() > 0) {
    int code = Serial.read();
    String input = Serial.readStringUntil('\n');
    if (code == 'x') {
      Serial.print("Resetting both to ");
      Serial.println(zeroValue);
      joystick1Write(zeroValue);
      joystick2Write(zeroValue);
    }
    else if (code == 'f') {
      int inVal = input.toInt();
      int val = convertToRawPos(inVal);
      Serial.print("Forward/back to: ");
      Serial.println(val);
      joystick1Write(val);
    }
    else if (code == 'l') {
      int inVal = input.toInt();
      int val = convertToRawPos(inVal);
      Serial.print("Left/right to: ");
      Serial.println(val);
      joystick2Write(val);
    }
    else if (code == 'c') {
      calibrate();
    }
    else {
      Serial.println("Unknown command");
    }
  }

  
  if (monitorVoltage && millis() > milliLastStatus + 5000) {
    int aread1 = analogRead(readJoy1VoltagePin);
    long aread1Mv = (long) (2 * (long)aread1 * 5000L / 1024L);
    int aread2 = analogRead(readJoy2VoltagePin);
    long aread2Mv = (long) (2 * (long)aread2 * 5000L / 1024L);
    int areadR = analogRead(readReferenceVoltagePin);
    long areadRMv = (long) (2 * (long)areadR * 5000L / 1024L);
    Serial.print("REF: ");
    Serial.print(areadRMv);
    Serial.print(" mV");
    Serial.print("   Joy1: ");
    Serial.print(aread1Mv);
    Serial.print(" mV");
    Serial.print("   Joy2: ");
    Serial.print(aread2Mv);
    Serial.print(" mV");
    Serial.println();

    milliLastStatus = millis();
  }
}

void printVoltage(int analog)
{
    long milliVolts = (long) (2 * (long)analog * 5000L / 1024L);
    Serial.print(milliVolts);
    Serial.print(" mV");
}

// Convert from range 0-200, where 100 is "no movement", into the raw values,
// where "zeroValue" is the calibrated zero position.
int convertToRawPos(int val)
{
  return val - 100 + zeroValue;
}

void calibrate()
{
  int value = 128;
  int adjust = 64;
  int referenceVoltage = analogRead(readReferenceVoltagePin);
  //Serial.print("Reference voltage: ");
  //printVoltage(referenceVoltage);
  //Serial.println();
  do {
    //Serial.print("Adjusting to ");
    //Serial.println(value);
    
    joystick1Write(value);
    joystick2Write(value);
    delayMicroseconds(1000);
    int voltage = analogRead(readJoy1VoltagePin);
    
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
  Serial.print("Calibration complete: value = ");
  Serial.println(value);
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
