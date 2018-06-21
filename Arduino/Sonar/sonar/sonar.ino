#include <NewPing.h>
 

#define MAX_DISTANCE 280
#define NUMSENSORS 4

int triggerPins[NUMSENSORS] = {20, 18, 16, 14};
int echoPins[NUMSENSORS] = {21, 19, 17, 15};
NewPing *sonar[NUMSENSORS];
 
void setup() {
  for (int i = 0; i < NUMSENSORS; ++i) {
    sonar[i] = new NewPing(triggerPins[i], echoPins[i], MAX_DISTANCE);
  }
  Serial.begin(57600);
}

int distance[NUMSENSORS];
 
void loop() {
  for (int i = 0; i < NUMSENSORS; ++i) {
    distance[i] = sonar[i]->ping_cm();
  }

  for (int i = 0; i < NUMSENSORS; ++i) {
    Serial.print(distance[i]);
    Serial.print(",");
  }
  Serial.println();
}
