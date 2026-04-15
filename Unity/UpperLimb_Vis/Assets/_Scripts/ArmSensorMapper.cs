using UnityEngine;
using System.IO.Ports;

public class ArmSensorMapper : MonoBehaviour {
    [Header("Serial Connection")]
    public string comPort = "COM8"; // Ensure this matches the Arduino COM port
    private SerialPort serialPort;

    [Header("3D Model")]
    public Transform armModel; // Sensor1

    private Quaternion calibrationPoseArm = Quaternion.identity;
    private bool isCalibrated = false;

    void Start() {
        serialPort = new SerialPort(comPort, 115200);
        serialPort.ReadTimeout = 15;
        
        try { 
            serialPort.Open(); 
            Debug.Log("ArmManager: COM port opened successfully.");
        } 
        catch (System.Exception e) { 
            Debug.LogError("ArmManager Connection Error: " + e.Message); 
        }
    }

    void Update() {
        if (serialPort != null && serialPort.IsOpen) {
            try {
                string[] values = serialPort.ReadLine().Split(',');
                
                // Ensure we receive the full packet (8 values)
                if (values.Length >= 8) {
                    
                    // Ignoring Sensor 1 (values[0] to values[3])
                    // Extracting Sensor 2 data (Biceps)
                    float wArm = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    float xArm = float.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);
                    float yArm = float.Parse(values[6], System.Globalization.CultureInfo.InvariantCulture);
                    float zArm = float.Parse(values[7], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Hardware to Unity initial coordinate conversion
                    Quaternion rawRot = new Quaternion(-xArm, yArm, -zArm, wArm);

                    //  N-Pose Calibration
                    if (!isCalibrated) {
                        calibrationPoseArm = rawRot;
                        isCalibrated = true;
                        Debug.Log("Calibration successful.");
                    }

                    // Apply relative matrix and resolve mirrored axes
                    Quaternion unmirroredRot = Quaternion.Inverse(calibrationPoseArm) * rawRot;
                    Quaternion worldRot = new Quaternion(-unmirroredRot.x, unmirroredRot.y, -unmirroredRot.z, unmirroredRot.w);
                    
                    // global rotation for Arm
                    if (armModel != null) {
                        armModel.rotation = worldRot; 
                    }
                }
            } 
            catch (System.TimeoutException) {  } 
            catch (System.Exception) {  }
        }

        // Manual calibration
        if (Input.GetKeyDown(KeyCode.C)) {
            isCalibrated = false;
            Debug.Log("Recalibrating sensors..."); 
        }
    }

    void OnApplicationQuit() { 
        if (serialPort != null && serialPort.IsOpen) {
            serialPort.Close(); 
        }
    }
}