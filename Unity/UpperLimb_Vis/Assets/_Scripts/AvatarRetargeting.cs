using UnityEngine;

public class AvatarRetargeting : MonoBehaviour {
    [Header("Physical sensors (IMU)")]
    public Transform sourceShoulder; // Sensor1_Model
    public Transform sourceElbow;    // Sensor2_Model

    [Header("3D Avatar Joints")]
    public Transform targetShoulder; // Avatar's LeftArm
    public Transform targetElbow;    // Avatar's LeftForeArm

    private Quaternion offsetShoulder;
    private Quaternion offsetElbow;
    
    private bool isCalibrated = false; 

    void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            CalibrateRetargeting();
        }
    }

    private void CalibrateRetargeting() {
        // Calculate offsets between source sensors and target avatar joints
        if (sourceShoulder != null && targetShoulder != null) {
            offsetShoulder = Quaternion.Inverse(sourceShoulder.rotation) * targetShoulder.rotation;
        }

        if (sourceElbow != null && targetElbow != null) {
            offsetElbow = Quaternion.Inverse(sourceElbow.rotation) * targetElbow.rotation;
        }

        isCalibrated = true;
        Debug.Log("¡Retargeting del Avatar sincronizado y calibrado!");
    }

    void LateUpdate() {
        if (!isCalibrated) return; 

        // Avatar's shoulder rotation = Sensor's shoulder rotation * offset
        if (sourceShoulder != null && targetShoulder != null) {
            targetShoulder.rotation = sourceShoulder.rotation * offsetShoulder;
        }
        
        // Avatar's elbow rotation = Sensor's elbow rotation * offset
        if (sourceElbow != null && targetElbow != null) {
            targetElbow.rotation = sourceElbow.rotation * offsetElbow;
        }
    }
}