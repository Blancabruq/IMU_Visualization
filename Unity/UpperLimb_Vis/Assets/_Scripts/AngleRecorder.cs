using UnityEngine;
using System.IO;
using System.Text;

public class AngleRecorder : MonoBehaviour
{
    [Header("Avatar Joints")]
    public Transform spine;      
    public Transform upperArm;   
    public Transform forearm;  
    public Transform hand;  

    private bool isRecording = false;
    private bool isPromptingSave = false; // Flag to indicate if we're asking for a filename
    private string fileNameInput = "Test_subject1"; // Default filename
    
    private float startTime;
    private StringBuilder csvContent;

    void Update()
    {
        // Prevent recording or angle display while writing filename
        if (isPromptingSave) return;

        //STATIC RECORDING
        // When pressing P, angles are calculated and displayed
        if (Input.GetKeyDown(KeyCode.P))
        {
            Vector3[] formattedAngles = CalculateAllAngles();
            
            //Print to console
            Debug.Log($"Angle data");
            Debug.Log($"Shoulder (X: {formattedAngles[0].x:F1}º, Y: {formattedAngles[0].y:F1}º, Z: {formattedAngles[0].z:F1}º)");
            Debug.Log($"Elbow   (X: {formattedAngles[1].x:F1}º, Y: {formattedAngles[1].y:F1}º, Z: {formattedAngles[1].z:F1}º)");
            Debug.Log($"Wrist   (X: {formattedAngles[2].x:F1}º, Y: {formattedAngles[2].y:F1}º, Z: {formattedAngles[2].z:F1}º)");
        }
        //DYNAMIC RECORDING
        // When pressing G, recording starts/stops and angles are saved to CSV
        if (Input.GetKeyDown(KeyCode.G))
        {
            isRecording = !isRecording;
            
            if (isRecording) { 
                StartRecording(); 
            }else { 
                // Stop recording and prompt for filename
                isPromptingSave = true;
            }
        }

        if (isRecording)
        {
            RecordFrame();
        }
    }


    private Vector3[] CalculateAllAngles()
    {
        //Calculate shoulder angle (Upper Arm relative to Trunk)
        Quaternion shoulderRelative = Quaternion.Inverse(spine.rotation) * upperArm.rotation;
        //Calculate the elbow angle (Forearm relative to Upper Arm)
        Quaternion elbowRelative = Quaternion.Inverse(upperArm.rotation) * forearm.rotation;
        //Calculate the wrist angle (Hand relative to Forearm)
        Quaternion wristRelative = Quaternion.Inverse(forearm.rotation) * hand.rotation; 

        return new Vector3[] {
            FormatAngles(shoulderRelative.eulerAngles),
            FormatAngles(elbowRelative.eulerAngles),
            FormatAngles(wristRelative.eulerAngles)
        };
    }

    //function to convert angles from 0-360 to -180 to 180 range
    private Vector3 FormatAngles(Vector3 euler){
        return new Vector3(
            euler.x > 180 ? euler.x - 360 : euler.x,
            euler.y > 180 ? euler.y - 360 : euler.y,
            euler.z > 180 ? euler.z - 360 : euler.z
        );
    }

    private void StartRecording(){
        startTime = Time.time;
        csvContent = new StringBuilder();
        
        // Column headers for CSV
        csvContent.AppendLine("Time(s),Shoulder_X,Shoulder_Y,Shoulder_Z,Elbow_X,Elbow_Y,Elbow_Z,Wrist_X,Wrist_Y,Wrist_Z");
        Debug.Log("Recording started... Press G again to stop.");
    }

    private void RecordFrame(){
        float currentTime = Time.time - startTime;
        Vector3[] angles = CalculateAllAngles();

        string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0:F3},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2},{6:F2},{7:F2},{8:F2},{9:F2}",
            currentTime,
            angles[0].x, angles[0].y, angles[0].z,
            angles[1].x, angles[1].y, angles[1].z,
            angles[2].x, angles[2].y, angles[2].z);

        csvContent.AppendLine(line);
    }


    // Function to save the CSV file with the provided filename
    void OnGUI(){
        if (isPromptingSave){
            // Open window to ask for filename
            GUI.Box(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 60, 300, 120), "Save Recording");
            GUI.Label(new Rect(Screen.width / 2 - 140, Screen.height / 2 - 30, 280, 20), "Enter filename:");

            // Text field for filename input
            fileNameInput = GUI.TextField(new Rect(Screen.width / 2 - 140, Screen.height / 2 - 10, 280, 20), fileNameInput);

            // Button to confirm saving
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 20, 100, 30), "Save")){
                SaveFile(fileNameInput);
                isPromptingSave = false; // Close window
                fileNameInput = "prueba_sujetoX"; // Reset to default for next recording
            }
        }
    }

    private void SaveFile(string customName)
    {
        //Define the path where CSV files will be saved - IMU_Results folder inside Assets
        string folderPath = Application.dataPath + "/IMU_Results/";
        
        // Create the folder
        if (!Directory.Exists(folderPath)) { 
            Directory.CreateDirectory(folderPath); 
        }

        // Adapt filename
        string safeName = customName.Replace(" ", "_");
        string finalPath = folderPath + safeName + ".csv";

        //Save the CSV 
        File.WriteAllText(finalPath, csvContent.ToString());
        Debug.Log($"<color=green>ÉXITO: Archivo guardado en {finalPath}</color>");
    }
}
