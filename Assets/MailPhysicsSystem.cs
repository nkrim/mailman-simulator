using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailPhysicsSystem : MonoBehaviour
{
    // Public vars
    public Transform mailPrefab;
    public Camera mailCamera;
    public int mailCount = 1;
    public float grabbedMailHeight = 2f;
    [Range(0f,1f)]
    public float mailGrabEase = 0.25f; // Modifier for velocity lerp for grabbed mail
    [Range(0f,1f)]
    public float mailGrabAngularEase = 0.25f; // Modifer for angular velocity slerp for grabbed mail
    [Range(0f,1f)]
    public float droppedMailDampening = 0.6f;
    [Range(0f,1f)]
    public float droppedVerticalDampening = 0.8f; // Modifier for dampening vertical movement when mail is dropped
    
    // Protected vars
    Transform grabbed = null;
    Vector3 grabbed_velocity;

    Vector3 gizmo_position = Vector3.zero;


    // Private vars
    private RaycastHit[] hitbuffer;

    private float min_x_start = -2f;
    private float max_x_start = 2f;
    private float min_z_start = -2f;
    private float max_z_start = 2f;

    void Start () {
        hitbuffer = new RaycastHit[mailCount];
        if(mailPrefab) {
            for(int i=0;  i<mailCount; i++) {
                Transform m = Instantiate(mailPrefab, this.transform);
                m.localPosition = new Vector3(Random.Range(min_x_start, max_x_start), 0.25f*(i+1), Random.Range(min_z_start, max_z_start));
                Renderer rend = m.GetComponent<Renderer>();
                Material mat = rend.material;
                mat.SetColor("_Color", Random.ColorHSV(0.75f, 1.0f));
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // On left-mouse down
        if (Input.GetMouseButtonDown(0)) {
            // Ungrab item
            if(grabbed != null) {
                Ungrab();
            }
            // Find the top mail item that was clicked
            int numhits = Mathf.Min(mailCount, Physics.RaycastNonAlloc(
                mailCamera.ScreenPointToRay(Input.mousePosition),
                hitbuffer, 100, LayerMask.GetMask(new string[] { "Mail" })));
            int top_hit_index = -1;
            float top_hit_val = float.PositiveInfinity;
            Vector3 cam_pos = mailCamera.transform.position;
            // Iterate through hit objects, and roughly compare distance from camera, if it's shorter than top_hit_val, use
            for (int i = 0; i < numhits; i++) {
                RaycastHit hit = hitbuffer[i];
                float mag = (cam_pos - hit.point).sqrMagnitude;
                if (mag < top_hit_val) {
                    top_hit_index = i;
                    top_hit_val = mag;
                }
            }
            // Grab the top item
            if(top_hit_index >= 0) {
                RaycastHit hit = hitbuffer[top_hit_index];
                Grab(hit);
            }
        }
    }

    private void OnDrawGizmos () {
    }

    private void FixedUpdate () {
        // Grabbed logic
        if (grabbed) {
            // Move the grabbed item with the mouse
            if (Input.GetMouseButton(0)) {
                SetGrabbedMailPosition();
            }
            else {
                Ungrab();
            }
        }
    }

    void SetGrabbedMailPosition() {
        // Compute goal position, which should be exactly aligned with mouse along the plane of this physics system
        Plane mail_height_plane = new Plane(this.transform.up, this.transform.position + grabbedMailHeight*this.transform.up);
        Ray cursor_ray = mailCamera.ScreenPointToRay(Input.mousePosition);
        mail_height_plane.Raycast(cursor_ray, out float ray_distance);
        Vector3 goal_position = cursor_ray.GetPoint(ray_distance);

        // Find lerped position in between current and goal positions, so that velocity cna be set for this midway point 
        Vector3 lerped_position = Vector3.Lerp(grabbed.position, goal_position, mailGrabEase);

        // Compute velocity required to get to lerped_position
        Vector3 vel = (lerped_position - grabbed.position) / Time.fixedDeltaTime;

        // Apply new velocity (if larger than threshold)
        Rigidbody rb = grabbed.GetComponent<Rigidbody>();
        rb.velocity = vel.magnitude > 0.001 ? vel : Vector3.zero;

        // Compute angular velocity to normalize mail
        Quaternion rotation_to_normal = Quaternion.FromToRotation(grabbed.up, this.transform.up)
                                        * Quaternion.FromToRotation(grabbed.right, this.transform.right);
        Quaternion slerped_rotation = Quaternion.Slerp(Quaternion.identity, rotation_to_normal, mailGrabAngularEase);
        slerped_rotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
        Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad;
        Vector3 angularSpeed = angularDisplacement / Time.fixedDeltaTime;
        rb.angularVelocity = angularSpeed;
    }

    void Grab(RaycastHit hit) {
        Transform m = hit.transform;
        grabbed = m;
    }

    void Ungrab() {
        if(!grabbed)
            return;
        // Make changes to rigidbody to polish dropping physics
        Rigidbody rb = grabbed.GetComponent<Rigidbody>();
        // Dampen the velocity by given amount when dropped
        rb.velocity = rb.velocity - rb.velocity * droppedMailDampening;
        // Dampen upwards velocity when dropping
        Vector3 up = this.transform.up;
        Vector3 projected = Vector3.Project(rb.velocity, up);
        Vector3 dampening = (1-droppedVerticalDampening) * projected;
        rb.velocity = rb.velocity - dampening;
        grabbed = null;
    }
}
