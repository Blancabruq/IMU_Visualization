using UnityEngine;
using System.IO.Ports;

public class IMUSensorMapper : MonoBehaviour
{
    public string comPort = "COM8"; 
    SerialPort serialPort;

    // Variables para la calibración
    private Quaternion calibrationPose = Quaternion.identity;
    private bool isCalibrated = false;

    void Start()
    {
        serialPort = new SerialPort(comPort, 115200);
        serialPort.ReadTimeout = 15; 
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;

        try {
            serialPort.Open();
            Debug.Log("Puerto " + comPort + " abierto. Pulsa 'C' para calibrar el nivel.");
        } catch (System.Exception e) {
            Debug.LogError("Error al abrir el puerto: " + e.Message);
        }
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try {
                string rawData = serialPort.ReadLine();
                string[] values = rawData.Split(',');

                // Leemos los 4 primeros números (Sensor 1)
                if (values.Length >= 4) {
                    float w = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                    float x = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    float y = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    float z = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);

                    // --- LA CORRECCIÓN MATEMÁTICA ---
                    // Cambiamos el orden y el signo para que coincidan los mundos
                    Quaternion rawSensorRotation = new Quaternion(-x, -z, -y, w);

                    // Si no está calibrado (al empezar o pulsar C), guardamos la pose actual como 'cero'
                    if (!isCalibrated) {
                        calibrationPose = rawSensorRotation;
                        isCalibrated = true;
                        Debug.Log("¡Calibración completada!");
                    }

                    // Aplicamos la rotación relativa (Pose Actual x Inversa de la Pose Inicial)
                    transform.rotation = Quaternion.Inverse(calibrationPose) * rawSensorRotation;
                }
            } 
            catch (System.TimeoutException) { }
            catch (System.Exception) { }
        }

        // Si pulsas la tecla C, se resetea la orientación
        if (Input.GetKeyDown(KeyCode.C)) {
            isCalibrated = false; 
            Debug.Log("Recalibrando...");
        }
    }

    void OnApplicationQuit() {
        if (serialPort != null && serialPort.IsOpen) serialPort.Close();
    }
}