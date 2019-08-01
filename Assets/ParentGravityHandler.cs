using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* IDEAS
 * - Throw this in with the physics system to unify it all
 * - Adjust gravity based on tilt/rotation of car (but shifted 90deg still)
 */

public class ParentGravityHandler : MonoBehaviour
{
    public bool dampenVerticalVelocity = true;
    public float dampenThreshold = 10f;

    private void FixedUpdate () {
        // Artificially add gravity depending on parent's down-axis
        Transform parent = transform.parent;
        Vector3 gravity_adjusted = -Physics.gravity.magnitude * parent.up;
        Rigidbody rb = GetComponent<Rigidbody>();

        // If raising height too quickly, increase the gravity adjustment
        bool moving_up = Vector3.Dot(parent.up, rb.velocity) > 0.1;
        if(moving_up) {
            float mag = Vector3.Scale(parent.up, rb.velocity).sqrMagnitude;
            print(mag);
            if (mag > dampenThreshold) {
                gravity_adjusted *= (mag/dampenThreshold);
            }
        }

        rb.velocity += gravity_adjusted * Time.fixedDeltaTime;
    }
}
