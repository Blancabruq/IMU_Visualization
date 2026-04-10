using UnityEngine;
using System.IO.Ports;

public class IMUSensorMapper : MonoBehaviour {
    public string comPort = "COM8"; 
    SerialPort serialPort;

    [Header("3D Models")]
    public Transform sensor1_Model; // Proximal joint (Shoulder)
    public Transform sensor2_Model; // Distal joint (Elbow)

    [Header("Kinematic Parameters (meters)")]
    public float upperArmLength = 0.30f; // l_u: Distance from Acromion to Lateral Epicondyle
    public float forearmLength = 0.25f;  // l_f: Distance from Lateral Epicondyle to Ulnar Styloid

    // Calibration variables
    private Quaternion calibrationPose1 = Quaternion.identity;
    private Quaternion calibrationPose2 = Quaternion.identity;
    private bool isCalibrated = false;

    void Start() {
        serialPort = new SerialPort(comPort, 115200);
        serialPort.ReadTimeout = 15; 
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;

        try {
            serialPort.Open();
            Debug.Log("Serial port opened successfully. Press 'C' to calibrate both sensors (N-Pose).");
        } catch (System.Exception e) {
            Debug.LogError("Error opening serial port: " + e.Message);
        }
    }

    void Update(){
        if (serialPort != null && serialPort.IsOpen){
            try {
                string rawData = serialPort.ReadLine();
                string[] values = rawData.Split(',');

                // Ensure complete data from Arduino (8 quaternion values)
                if (values.Length >= 8) {
                    
                    //PARSE SENSOR 1 (Shoulder)
                    float w1 = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                    float x1 = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    float y1 = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    float z1 = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);
                    
                    //PARSE SENSOR 2 (Elbow)
                    float w2 = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    float x2 = float.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);
                    float y2 = float.Parse(values[6], System.Globalization.CultureInfo.InvariantCulture);
                    float z2 = float.Parse(values[7], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Raw Data Extraction
                    Quaternion rawRot1 = new Quaternion(-x1, y1, -z1, w1);
                    Quaternion rawRot2 = new Quaternion(-x2, y2, -z2, w2);

                    // Calibration
                    if (!isCalibrated) {
                        calibrationPose1 = rawRot1;
                        calibrationPose2 = rawRot2;
                        isCalibrated = true;
                        Debug.Log("Calibration successful.");
                    }

                    // KINEMATIC CHAINS MATRIXES
                    
                    // Apply calibration to raw data
                    Quaternion unmirroredRot1 = Quaternion.Inverse(calibrationPose1) * rawRot1;
                    Quaternion unmirroredRot2 = Quaternion.Inverse(calibrationPose2) * rawRot2;

                    // mirror corrections
                    Quaternion worldRot1 = new Quaternion(-unmirroredRot1.x, unmirroredRot1.y, -unmirroredRot1.z, unmirroredRot1.w);
                    Quaternion worldRot2 = new Quaternion(-unmirroredRot2.x, unmirroredRot2.y, -unmirroredRot2.z, unmirroredRot2.w);
                    
                    // Global rotation for proximal joint (shoulder)
                    if (sensor1_Model != null) {
                        sensor1_Model.rotation = worldRot1;
                    }

                    // Relative rotation for distal joint (elbow)
                    if (sensor2_Model != null && sensor1_Model != null) {
                        // Relative rotation: q_relative = inverse(q_proximal) * q_distal
                        sensor2_Model.localRotation = Quaternion.Inverse(worldRot1) * worldRot2;
                        
                        // Apply D-H link length (l_u) to separate the joints
                        sensor2_Model.localPosition = new Vector3(0f, -upperArmLength, 0f);
                    }
                }
            } 
            catch (System.TimeoutException) { }
            catch (System.Exception) { }
        }

        // Manual recalibration
        if (Input.GetKeyDown(KeyCode.C)) {
            isCalibrated = false; 
            Debug.Log("Recalibrating sensors...");
        }
    }

    void OnApplicationQuit() {
        if (serialPort != null && serialPort.IsOpen) serialPort.Close();
    }
}