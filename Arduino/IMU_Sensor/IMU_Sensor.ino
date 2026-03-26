#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

Adafruit_BNO055 sensor1 = Adafruit_BNO055(1, 0x28);

void setup() {
  Serial.begin(115200);
  delay(2000); 
  if (sensor1.begin()) sensor1.setExtCrystalUse(true);
}

void loop() {
  imu::Quaternion q1 = sensor1.getQuat();
  
  // Sending data in CSV format for Unity: W, X, Y, Z
  Serial.print(q1.w(), 4); Serial.print(",");
  Serial.print(q1.x(), 4); Serial.print(",");
  Serial.print(q1.y(), 4); Serial.print(",");
  Serial.println(q1.z(), 4); 

  delay(50); 
}