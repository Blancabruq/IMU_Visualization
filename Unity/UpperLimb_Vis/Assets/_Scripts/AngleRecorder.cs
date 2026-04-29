using UnityEngine;

public class AngleRecorder : MonoBehaviour
{
    [Header("Avatar Joints")]
    public Transform spine;      
    public Transform upperArm;   
    public Transform forearm;  
    public Transform hand;  

    void Update()
    {
        // When pressing P, angles are calculated and displayed
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Calculate shoulder angle (Upper Arm relative to Trunk)
            Quaternion shoulderRelativeRot = Quaternion.Inverse(spine.rotation) * upperArm.rotation;
            Vector3 shoulderAngles = shoulderRelativeRot.eulerAngles; //show in euler

            //Calculate the elbow angle (Forearm relative to Upper Arm)
            Quaternion elbowRelativeRot = Quaternion.Inverse(upperArm.rotation) * forearm.rotation;
            Vector3 elbowAngles = elbowRelativeRot.eulerAngles;
            
            //Calculate the wrist angle (Hand relative to Forearm)
            Quaternion wristRelativeRot = Quaternion.Inverse(forearm.rotation) * hand.rotation; 
            Vector3 wristAngles = wristRelativeRot.eulerAngles;
            
            // Format angles to be between -180 and 180 degrees
            Vector3 shoulderFormatted = FormatAngles(shoulderAngles);
            Vector3 elbowFormatted = FormatAngles(elbowAngles);
            Vector3 wristFormatted = FormatAngles(wristAngles);

            //Print to console
            Debug.Log($"Angle data");
            Debug.Log($"Shoulder (X: {shoulderFormatted.x:F1}º, Y: {shoulderFormatted.y:F1}º, Z: {shoulderFormatted.z:F1}º)");
            Debug.Log($"Elbow   (X: {elbowFormatted.x:F1}º, Y: {elbowFormatted.y:F1}º, Z: {elbowFormatted.z:F1}º)");
            Debug.Log($"Wrist   (X: {wristFormatted.x:F1}º, Y: {wristFormatted.y:F1}º, Z: {wristFormatted.z:F1}º)");
        }
    }

    //function to convert angles from 0-360 to -180 to 180 range
    private Vector3 FormatAngles(Vector3 euler)
    {
        return new Vector3(
            euler.x > 180 ? euler.x - 360 : euler.x,
            euler.y > 180 ? euler.y - 360 : euler.y,
            euler.z > 180 ? euler.z - 360 : euler.z
        );
    }
}
