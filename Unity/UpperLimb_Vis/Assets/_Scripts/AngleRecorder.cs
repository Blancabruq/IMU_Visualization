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
    private string fileNameInput = ""; 
    
    private float startTime;
    private StringBuilder csvContent;

    void Update(){
        // Prevent recording or angle display while writing filename
        if (isPromptingSave) return;

        //STATIC RECORDING
        // When pressing P, angles are calculated and displayed
        if (Input.GetKeyDown(KeyCode.P)){
            float[] clinicalAngles = CalculateClinicalAngles();
            
            Debug.Log("<color=cyan>--- Virtual goniometer ---</color>");
            Debug.Log($"shoulder elevation: {clinicalAngles[0]:F1}º");
            Debug.Log($"elbow flexion: {clinicalAngles[1]:F1}º");

        }
        //DYNAMIC RECORDING
        // When pressing G, recording starts/stops and angles are saved to CSV
        if (Input.GetKeyDown(KeyCode.G)){
            isRecording = !isRecording;
            
            if (isRecording) { 
                StartRecording(); 
            }else { 
                // Stop recording and prompt for filename
                isPromptingSave = true;
            }
        }

        if (isRecording){
            RecordFrame();
        }
    }

    //VIRTUAL GONIOMETER
    private float[] CalculateClinicalAngles(){
        
        // calculates the angle between the trunk and the upper arm (shoulder elevation) and the angle between the upper arm and forearm (elbow flexion)
        Vector3 trunkVector = -spine.up; // assuming the spine's up vector points upwards, we take the negative to get a vector pointing downwards along the trunk
        Vector3 armVector = forearm.position - upperArm.position; // segment from shoulder to elbow
        Vector3 forearmVector = hand.position - forearm.position; // segment from elbow to wrist

        // calculate angle between segments
        float shoulderElevation = Vector3.Angle(trunkVector, armVector);
        float elbowFlexion = Vector3.Angle(armVector, forearmVector);

        return new float[] { shoulderElevation, elbowFlexion };
    }

    private void StartRecording(){
        startTime = Time.time;
        csvContent = new StringBuilder();
        
        // Column headers for CSV
        csvContent.AppendLine("Time(s),Shoulder_Elevation_Deg,Elbow_Flexion_Deg");
        Debug.Log("Recording started... Press G again to stop.");
    }

    private void RecordFrame(){
        float currentTime = Time.time - startTime;
        float[] angles = CalculateClinicalAngles();
        // 3 deicmals for time, 2 decimals for angles
        string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0:F3},{1:F2},{2:F2}",
            currentTime, angles[0], angles[1]);
            
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
                fileNameInput = ""; // Reset to default for next recording
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
        Debug.Log($"<color=green>Csv saved to {finalPath}</color>");
    }
}
