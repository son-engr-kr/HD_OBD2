#include <SoftwareSerial.h>
// SoftwareSerial mySerial(2,3); //RX, TX

void setup() {
   Serial.begin(9600);
   Serial1.begin(115200);
   
}

void loop() {
  if(Serial1.available())
  {
    Serial.write(Serial1.read());
  }

  if(Serial.available())
  {
    Serial1.write(Serial.read());
  }
}
