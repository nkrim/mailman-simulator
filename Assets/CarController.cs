using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* IDEAS TO FIX SOME OF THE ISSUES WITH THIS
 * - Virtual motor, to mimic gears and chugginess
 * - Max speeds and speed dampening
 * - Harsher brakes / Hand brakes
 */

public class CarController : MonoBehaviour {
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque = 1000; // maximum torque the motor can apply to wheel
    public float maxBrakeTorque = 3000;
    public float brakeStopThreshold = 0.01f;
    public float maxSteeringAngle = 30; // maximum steer angle the wheel can have

    public void FixedUpdate () {
        Rigidbody p_rb = GetComponent<Rigidbody>();
        bool moving_forward = Vector3.Dot(p_rb.velocity, transform.forward) > 0;
        bool faster_than_brake_threshold = p_rb.velocity.sqrMagnitude > brakeStopThreshold;

        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float brake = maxBrakeTorque * -Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        bool should_brake = faster_than_brake_threshold && (moving_forward ? motor < 0 : motor > 0);
        if(should_brake)
            motor = 0;
        else
            brake = 0;

        foreach (AxleInfo axleInfo in axleInfos) {
            // Steering
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            // Motor
            if (axleInfo.motor) { 
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            // Brakes
            axleInfo.leftWheel.brakeTorque = brake;
            axleInfo.rightWheel.brakeTorque = brake;
        }
    }
}

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}