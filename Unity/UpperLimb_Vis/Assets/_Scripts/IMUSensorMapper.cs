using UnityEngine;
using System.IO.Ports;

public class IMUSensorMapper : MonoBehaviour
{
    public string comPort = "COM8"; 
    SerialPort serialPort;

    [Header("Modelos 3D")]
    public Transform sensor1_Model;
    public Transform sensor2_Model;

    [Header("Kinematic Parameters (meters)")]
    public float upperArmLength = 0.30f; // l_u: Distance from Acromion to Lateral Epicondyle
    public float forearmLength = 0.25f;  // l_f: Distance from Lateral Epicondyle to Ulnar Styloid

    // Calibración individual
    private Quaternion calibrationPose1 = Quaternion.identity;
    private Quaternion calibrationPose2 = Quaternion.identity;
    private bool isCalibrated = false;

    void Start()
    {
        serialPort = new SerialPort(comPort, 115200);
        serialPort.ReadTimeout = 15; 
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;

        try {
            serialPort.Open();
            Debug.Log("Puerto abierto. Pulsa 'C' para calibrar AMBOS sensores.");
        } catch (System.Exception e) {
            Debug.LogError("Error abriendo puerto: " + e.Message);
        }
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try {
                string rawData = serialPort.ReadLine();
                string[] values = rawData.Split(',');

                // Verificamos que llegan los 8 números del Arduino
                if (values.Length >= 8) {
                    
                    // --- PARSEO SENSOR 1 (Tórax) ---
                    float w1 = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                    float x1 = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    float y1 = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    float z1 = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Fórmula perfecta que calculamos: (-x, -z, -y, w)
                    Quaternion rawRot1 = new Quaternion(-z1, -y1, -x1, w1);

                    // --- PARSEO SENSOR 2 (Brazo) ---
                    float w2 = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    float x2 = float.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);
                    float y2 = float.Parse(values[6], System.Globalization.CultureInfo.InvariantCulture);
                    float z2 = float.Parse(values[7], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Aplicamos la misma fórmula al segundo sensor
                    Quaternion rawRot2 = new Quaternion(-z2, -y2, -x2, w2);

                    // --- CALIBRACIÓN DE LA POSE CERO ---
                    if (!isCalibrated) {
                        calibrationPose1 = rawRot1;
                        calibrationPose2 = rawRot2;
                        isCalibrated = true;
                        Debug.Log("¡Calibración exitosa para los dos sensores!");
                    }

                    // --- APLICAR ROTACIÓN MATEMÁTICA CORRECTA (CADENA CINEMÁTICA) ---
                    
                    // 1. Calculamos la orientación de cada sensor en el espacio global
                    Quaternion unmirroredRot1 = Quaternion.Inverse(calibrationPose1) * rawRot1;
                    Quaternion unmirroredRot2 = Quaternion.Inverse(calibrationPose2) * rawRot2;

                    // --- NUEVA LÍNEA DE CORRECCIÓN: ESPEJADO DE EJE X ---
                    //zhu et al 2013, equation 19 relative orientation calculation
                    // Para un brazo izquierdo, espejamos el eje X global (horizontal) 
                    // zhu et al 2006, 3.4 relative rotation matrix computation
                    // zhu et al 2013, table 1 standard world coordinate definition
                    Quaternion worldRot1 = new Quaternion(-unmirroredRot1.x, unmirroredRot1.y, unmirroredRot1.z, unmirroredRot1.w);
                    Quaternion worldRot2 = new Quaternion(-unmirroredRot2.x, unmirroredRot2.y, unmirroredRot2.z, unmirroredRot2.w);
                    // 2. El hombro (padre) rota respecto al mundo
                    if (sensor1_Model != null) {
                        sensor1_Model.rotation = worldRot1;
                    }

                    
                    // 3. The elbow (child) rotates relative to the shoulder
                    if (sensor2_Model != null && sensor1_Model != null) {
                        // Relative rotation: q_relative = inverse(q_proximal) * q_distal
                        sensor2_Model.localRotation = Quaternion.Inverse(worldRot1) * worldRot2;
                        
                        // Apply D-H link length (l_u) to separate the joints
                        // Note: We use the X-axis assuming your arm points along the local X-axis. 
                        // Change to (0, -upperArmLength, 0) or (0, 0, upperArmLength) if your specific 3D model points along Y or Z.
                        sensor2_Model.localPosition = new Vector3(0f, -upperArmLength, 0f);
                    }
                }
            } 
            catch (System.TimeoutException) { }
            catch (System.Exception) { }
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            isCalibrated = false; 
            Debug.Log("Recalibrando...");
        }
    }

    void OnApplicationQuit() {
        if (serialPort != null && serialPort.IsOpen) serialPort.Close();
    }
}