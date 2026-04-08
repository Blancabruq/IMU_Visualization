#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

// Initialization of both sensors 
Adafruit_BNO055 sensor1 = Adafruit_BNO055(1, 0x28);
Adafruit_BNO055 sensor2 = Adafruit_BNO055(2, 0x29);

void setup() {
  Serial.begin(115200);
  delay(2000); 
  
  // Le quitamos el "Adafruit_BNO055::" y dejamos solo el nombre del modo
  if (!sensor1.begin(OPERATION_MODE_IMUPLUS)) {
    Serial.println("Error S1");
  } else {
    sensor1.setExtCrystalUse(true);
  }

  if (!sensor2.begin(OPERATION_MODE_IMUPLUS)) {
    Serial.println("Error S2");
  } else {
    sensor2.setExtCrystalUse(true);
  }
}
void loop() {
  imu::Quaternion q1 = sensor1.getQuat();
  imu::Quaternion q2 = sensor2.getQuat();
  
  // Sending Sensor 1 data 
  Serial.print(q1.w(), 4); Serial.print(",");
  Serial.print(q1.x(), 4); Serial.print(",");
  Serial.print(q1.y(), 4); Serial.print(",");
  Serial.print(q1.z(), 4); Serial.print(","); 

  // Sending Sensor 2 data 
  Serial.print(q2.w(), 4); Serial.print(",");
  Serial.print(q2.x(), 4); Serial.print(",");
  Serial.print(q2.y(), 4); Serial.print(",");
  Serial.println(q2.z(), 4); 

  delay(50); 
}