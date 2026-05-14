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
            Debug.Log($"shoulder flexion: {clinicalAngles[0]:F1}º");
            Debug.Log($"shoulder abduction: {clinicalAngles[1]:F1}º");
            Debug.Log($"elbow flexion: {clinicalAngles[2]:F1}º");
            Debug.Log($"elbow pronosupination: {clinicalAngles[3]:F1}º");
            Debug.Log($"wrist flexion: {clinicalAngles[4]:F1}º");

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
        Vector3 trunkDown = -spine.up; // assuming the spine's up vector points upwards, we take the negative to get a vector pointing downwards along the trunk
        Vector3 trunkForward = spine.forward; // frontal plane
        Vector3 trunkRight = spine.right;     // sagital plane
        
        Vector3 armVector = forearm.position - upperArm.position; // segment from shoulder to elbow
        Vector3 forearmVector = hand.position - forearm.position; // segment from elbow to wrist

        // calculate angle between segments
        float elbowFlexion = Vector3.Angle(armVector, forearmVector);

        Vector3 armSagittal = Vector3.ProjectOnPlane(armVector, trunkRight);
        float shoulderFlexion = Vector3.Angle(trunkDown, armSagittal);

        Vector3 armFrontal = Vector3.ProjectOnPlane(armVector, trunkForward);
        float shoulderAbduction = Vector3.Angle(trunkDown, armFrontal);
        
        Vector3 upperArmRef = Vector3.ProjectOnPlane(upperArm.forward, forearmVector);
        Vector3 handRef = Vector3.ProjectOnPlane(hand.forward, forearmVector);
        float pronosupination = Vector3.SignedAngle(upperArmRef, handRef, forearmVector);

        Vector3 forearmProjected = Vector3.ProjectOnPlane(forearmVector, hand.right);
        float wristFlexion = Vector3.SignedAngle(forearmProjected, hand.up, hand.right);

        return new float[] { shoulderFlexion, shoulderAbduction, elbowFlexion, pronosupination, wristFlexion };
    }

    private void StartRecording(){
        startTime = Time.time;
        csvContent = new StringBuilder();
        
        // Column headers for CSV
        csvContent.AppendLine("Time(s),Shoulder_Flexion_Deg,Shoulder_Abduction_Deg,Elbow_Flexion_Deg");
        Debug.Log("Recording started... Press G again to stop.");
    }

    private void RecordFrame(){
        float currentTime = Time.time - startTime;
        float[] angles = CalculateClinicalAngles();
        // 3 deicmals for time, 2 decimals for angles
        string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0:F3},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}s",
            currentTime, angles[0], angles[1], angles[2], angles[3], angles[4]);
            
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
        string folderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Data_Analysis/IMU_CSV_Results/"));
        
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
