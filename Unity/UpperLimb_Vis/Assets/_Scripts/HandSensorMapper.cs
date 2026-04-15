using UnityEngine;
using System.IO.Ports;

public class HandSensorMapper : MonoBehaviour {
    [Header("Serial Connection")]
    public string comPort = "COM6"; // Ensure this matches the Arduino COM port
    private SerialPort serialPort;

    [Header("3D Models")]
    public Transform forearmModel; // Sensor2
    public Transform handModel;    // Sensor3

    private Quaternion calibrationPoseHand = Quaternion.identity;
    private Quaternion calibrationPoseWrist = Quaternion.identity;
    private bool isCalibrated = false;

    void Start() {
        serialPort = new SerialPort(comPort, 115200);
        serialPort.ReadTimeout = 15;
        
        try { 
            serialPort.Open(); 
            Debug.Log("HandManager: COM port opened successfully.");
        } 
        catch (System.Exception e) { 
            Debug.LogError("HandManager Connection Error: " + e.Message); 
        }
    }

    void Update() {
        if (serialPort != null && serialPort.IsOpen) {
            try {
                string[] values = serialPort.ReadLine().Split(',');
                
                // Ensure we receive the full packet (8 values)
                if (values.Length >= 8) {
                    
                    // Extracting Sensor 1 data (Hand)
                    float wHand = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                    float xHand = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    float yHand = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    float zHand = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Extracting Sensor 2 data (Wrist)
                    float wWrist = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    float xWrist = float.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);
                    float yWrist = float.Parse(values[6], System.Globalization.CultureInfo.InvariantCulture);
                    float zWrist = float.Parse(values[7], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Hardware to Unity initial coordinate conversion
                    Quaternion rawRotHand = new Quaternion(-xHand, yHand, -zHand, wHand);
                    Quaternion rawRotWrist = new Quaternion(-xWrist, yWrist, -zWrist, wWrist);

                    // Dynamic N-Pose Calibration
                    if (!isCalibrated) {
                        calibrationPoseHand = rawRotHand;
                        calibrationPoseWrist = rawRotWrist;
                        isCalibrated = true;
                        Debug.Log("Calibration successful.");
                    }

                    // Apply relative matrix calculation
                    Quaternion unmirroredHand = Quaternion.Inverse(calibrationPoseHand) * rawRotHand;
                    Quaternion unmirroredWrist = Quaternion.Inverse(calibrationPoseWrist) * rawRotWrist;

                    // Resolve mirrored axes
                    Quaternion worldRotHand = new Quaternion(-unmirroredHand.x, unmirroredHand.y, -unmirroredHand.z, unmirroredHand.w);
                    Quaternion worldRotWrist = new Quaternion(-unmirroredWrist.x, unmirroredWrist.y, -unmirroredWrist.z, unmirroredWrist.w);
                    
                    // Global rotations
                    if (forearmModel != null) {
                        forearmModel.rotation = worldRotWrist;
                    }
                    if (handModel != null) {
                        handModel.rotation = worldRotHand;
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
