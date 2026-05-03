using UnityEngine;

public class BoneFollower : MonoBehaviour
{
    [Tooltip(" Sensor Model")]
    public Transform sensorTarget;

    [Tooltip("Degrees to force the arm down")]
    public Vector3 offsetToNPose = new Vector3(0, 0, 0);

    
    public Vector3 compassCorrection = new Vector3(0, 0, 0);
    
    private Quaternion initialAvatarBoneRot;
    private Quaternion initialSensorRot;
    void Start(){
        // Save the initial rotation of the avatar bone (T-Pose)
        initialAvatarBoneRot = transform.rotation;        
        if (sensorTarget != null){
            initialSensorRot = sensorTarget.rotation;
        }
    }

    void Update(){
        if (sensorTarget != null){
            // Calculate the rotation from the initial sensor pose (N-pose)
            Quaternion sensorRotationDelta = sensorTarget.rotation * Quaternion.Inverse(initialSensorRot);
           
            // Use compass to correct axis misalignment
            Quaternion compass = Quaternion.Euler(compassCorrection);
            Quaternion deltaCorrected = compass * sensorRotationDelta * Quaternion.Inverse(compass);
           
            // Calculate the virtual N-Pose: Original T-pose + offset
            Quaternion virtualNPose = initialAvatarBoneRot * Quaternion.Euler(offsetToNPose);
            
            // Apply the rotation to the initial avatar bone rotation
            transform.rotation = deltaCorrected * virtualNPose;
        }
        
        if (Input.GetKeyDown(KeyCode.C)){
            Recalibrate();
        }
        
        // Compass adjustment (for fine-tuning the alignment of the sensor data with the avatar's orientation)
        // Left and Right Arrow keys to adjust the Y-axis of the compass correction
        if (Input.GetKey(KeyCode.RightArrow)){
            compassCorrection.y += 0.5f; 
            Debug.Log($"[BoneFollower {gameObject.name}] Compass Y ajustado a: {compassCorrection.y:F1}º");
        }
        else if (Input.GetKey(KeyCode.LeftArrow)){
            compassCorrection.y -= 0.5f;
            Debug.Log($"[BoneFollower {gameObject.name}] Compass Y ajustado a: {compassCorrection.y:F1}º");
        }
    }
    public void Recalibrate(){
        if (sensorTarget != null){
            initialSensorRot = sensorTarget.rotation;
            Debug.Log($"BoneFollower for {gameObject.name} recalibrated.");
        }
    }
}
