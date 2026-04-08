using UnityEngine;
using System.IO.Ports;

public class IMUSensorMapper : MonoBehaviour
{
    public string comPort = "COM8"; 
    SerialPort serialPort;

    [Header("Modelos 3D")]
    public Transform sensor1_Model;
    public Transform sensor2_Model;

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
                    Quaternion rawRot1 = new Quaternion(-x1, -z1, -y1, w1);

                    // --- PARSEO SENSOR 2 (Brazo) ---
                    float w2 = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    float x2 = float.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);
                    float y2 = float.Parse(values[6], System.Globalization.CultureInfo.InvariantCulture);
                    float z2 = float.Parse(values[7], System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Aplicamos la misma fórmula al segundo sensor
                    Quaternion rawRot2 = new Quaternion(-x2, -z2, -y2, w2);

                    // --- CALIBRACIÓN DE LA POSE CERO ---
                    if (!isCalibrated) {
                        calibrationPose1 = rawRot1;
                        calibrationPose2 = rawRot2;
                        isCalibrated = true;
                        Debug.Log("¡Calibración exitosa para los dos sensores!");
                    }

                    // --- APLICAR ROTACIÓN MATEMÁTICA CORRECTA ---
                    if (sensor1_Model != null) {
                        sensor1_Model.rotation = Quaternion.Inverse(calibrationPose1) * rawRot1;
                    }
                    if (sensor2_Model != null) {
                        sensor2_Model.rotation = Quaternion.Inverse(calibrationPose2) * rawRot2;
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