using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{   
    // Protected vars 
    readonly float parked_velocity_threshold = 0.5f;
    readonly float parking_distance = 4f;
    readonly float time_til_park = 1f;
    bool was_recently_parked = false;
    float time_of_last_park = float.PositiveInfinity;
    Transform mailbox = null;

    private void Awake () {
        mailbox = transform.Find("Mailbox");
        if(!mailbox)
            Debug.LogWarning("CAN'T FIND MAILBOX");
    }

    // Trigger event for car parking in front of house
    private void OnTriggerStay (Collider other) {
        Rigidbody car_rb = other.GetComponentInParent<Rigidbody>();
        bool below_parked_threshold = car_rb.velocity.sqrMagnitude < parked_velocity_threshold;

        // If at parked speed and not recently parked, perform parking procedure
        if(below_parked_threshold && !was_recently_parked) {
            // If time_of_last_park is greater than current time, parking has just started
            if(time_of_last_park > Time.time) {
                print("Starting parking");
                time_of_last_park = Time.time;
            }
            else if(Time.time - time_of_last_park >= time_til_park) {
                Park(car_rb);
            }
        }
        // If was recently parked but above parked threshold speed, repark
        else if(!below_parked_threshold && was_recently_parked) {
           Unpark();
        }
    }
    private void OnTriggerExit (Collider other) {
        Unpark();
    }

    void Park(Rigidbody car_rb) {
        print("PARKED");
        was_recently_parked = true;

        // Stop the car
        car_rb.velocity = Vector3.zero;
        // Position in front of mailbox (will be smoother in the future)
        Vector3 parked_position = mailbox.position + parking_distance * mailbox.forward;
        parked_position.y = car_rb.transform.position.y + 0.1f;
        car_rb.transform.position = parked_position;
        // Adjust the camera towards mailbox (will also be smoother)
        Camera.main.transform.LookAt(new Vector3(mailbox.position.x, Camera.main.transform.position.y, mailbox.position.z), Vector3.up);
    }

    void Unpark() {
        if(!was_recently_parked)
            return;
        print("UNPARKED");
        was_recently_parked = false;

        // Re-adjust the camera to normal
        Camera.main.transform.localRotation = Quaternion.identity;

        // Reset time_of_last_park
        time_of_last_park = float.PositiveInfinity;
    }
}
