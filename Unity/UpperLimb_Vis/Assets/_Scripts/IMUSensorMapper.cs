using UnityEngine;
using System.IO.Ports;

public class IMUSensorMapper : MonoBehaviour
{
    public string comPort = "COM8"; 
    SerialPort serialPort;

    void Start()
    {
        // Recuerda poner aquí tu puerto COM real si no es el 6
        serialPort = new SerialPort(comPort, 115200);
        serialPort.ReadTimeout = 15; 
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;

        try {
            serialPort.Open();
            Debug.Log("Serial port opened successfully on " + comPort);
        } catch (System.Exception e) {
            Debug.LogError("Error opening port: " + e.Message);
        }
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try {
                string rawData = serialPort.ReadLine();
                string[] values = rawData.Split(',');

                // Leemos los 4 primeros números (los del Sensor 1)
                if (values.Length >= 4) {
                    float w = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                    float x = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    float y = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    float z = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);

                    // EL ERROR ESTABA AQUÍ: Mapeo directo sin tener en cuenta 
                    // que Hardware y Unity usan sistemas de coordenadas distintos
                    transform.rotation = new Quaternion(x, y, z, w);
                }
            } 
            catch (System.TimeoutException) { /* Ignoramos si tarda un poco */ }
            catch (System.Exception) { /* Ignoramos datos corruptos */ }
        }
    }

    void OnApplicationQuit() {
        if (serialPort != null && serialPort.IsOpen) serialPort.Close();
    }
}