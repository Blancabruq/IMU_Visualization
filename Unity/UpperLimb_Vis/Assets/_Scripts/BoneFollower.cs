using UnityEngine;

public class BoneFollower : MonoBehaviour
{
    [Tooltip(" Sensor Model")]
    public Transform sensorTarget;
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
            
            // Apply the rotation to the initial avatar bone rotation
            transform.rotation = sensorRotationDelta * initialAvatarBoneRot;
        }
    }
}
