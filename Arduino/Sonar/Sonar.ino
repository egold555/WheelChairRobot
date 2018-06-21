#include <NewPing.h>
 
#define TRIGGER_PIN  8
#define ECHO_PIN     9
#define MAX_DISTANCE 500
 
NewPing sonar(TRIGGER_PIN, ECHO_PIN, MAX_DISTANCE);
 
void setup() {
  Serial.begin(57600);
}
 
void loop() {
  delay(50);
  Serial.print("Ping: ");
  Serial.print(sonar.ping_cm());
  Serial.println("cm");
}
