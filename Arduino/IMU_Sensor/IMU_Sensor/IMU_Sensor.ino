#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

Adafruit_BNO055 sensor1 = Adafruit_BNO055(1, 0x28);

void setup() {
  Serial.begin(115200);
  delay(2000); 
  Serial.println("Starting IMU connection...");

  if (!sensor1.begin()) {
    Serial.println("Error: Sensor 1 not found");
  } else {
    sensor1.setExtCrystalUse(true);
  }
}

void loop() {
  sensors_event_t event1;
  sensor1.getEvent(&event1);

  Serial.print("X: "); Serial.print(event1.orientation.x, 0); 
  Serial.print(" Y: "); Serial.print(event1.orientation.y, 0); 
  Serial.print(" Z: "); Serial.println(event1.orientation.z, 0); 
  
  delay(100); 
}