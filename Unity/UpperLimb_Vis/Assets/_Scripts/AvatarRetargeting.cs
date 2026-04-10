using UnityEngine;

public class AvatarRetargeting : MonoBehaviour {
    [Header("Physical sensors (IMU)")]
    public Transform sourceShoulder; //Sensor1_Model
    public Transform sourceElbow;    //Sensor2_Model

    [Header("3D Avatar Joints")]
    public Transform targetShoulder; // Avatar's LeftArm
    public Transform targetElbow;    // Avatar's LeftForeArm

    private Quaternion offsetShoulder;
    private Quaternion offsetElbow;

    void Start(){
        // Calculate offset between the Avatar's T-Pose and the sensors' N-Pose
        if (sourceShoulder != null && targetShoulder != null){
            offsetShoulder = Quaternion.Inverse(sourceShoulder.rotation) * targetShoulder.rotation;
        }

        if (sourceElbow != null && targetElbow != null){
            offsetElbow = Quaternion.Inverse(sourceElbow.rotation) * targetElbow.rotation;
        }
    }

    void LateUpdate(){
        // Avatar's shoulder rotation = Sensor's shoulder rotation * offset
        if (sourceShoulder != null && targetShoulder != null)
        {
            targetShoulder.rotation = sourceShoulder.rotation * offsetShoulder;
        }
        
        // Avatar's elbow rotation = Sensor's elbow rotation * offset
        if (sourceElbow != null && targetElbow != null)
        {
            targetElbow.rotation = sourceElbow.rotation * offsetElbow;
        }
    }
}
