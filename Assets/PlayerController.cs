using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public Transform camera;
    float yaw = 0f;
    public float sensitivity;
    float pitch = 0f;
    [SerializeField] float accel;
    [SerializeField] float friction;
    [SerializeField] float air_friction;
    [SerializeField] float ground_max_angle = 48f;
    bool jump_buffer = false;
    [SerializeField] int coyote_time = 10;
    int coyote_frames = 0;
    int slide_coyote_frames = 0;
    [SerializeField] float jump_force;
    List<Collision> currentCollisions;
    [SerializeField] float slide_force = 600f;
    [SerializeField] int slide_cooldown_time = 30;
    int slide_cooldown = 0;
    CapsuleCollider standing_collider;
    CapsuleCollider sliding_collider;
    bool slide_buffer = false;
    bool sliding = false;
    [SerializeField] LayerMask ground;
    Vector3 spawnPos;
    [SerializeField] float air_base_max_speed = 12f;
    float dynamic_air_max_speed;
    int jump_timer = 0;
    public bool canMove = true;
    public static PlayerController Instance;
    [SerializeField] AudioSource noise;
    [SerializeField] GameObject jump_sfx;
    GameObject jump_sfx_instance;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        dynamic_air_max_speed = air_base_max_speed;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        CapsuleCollider[] cols = GetComponents<CapsuleCollider>();
        standing_collider = cols[0];
        sliding_collider = cols[1];
        spawnPos = rb.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            yaw = yaw % 360;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -89f, 89f);
            camera.rotation = Quaternion.Slerp(camera.rotation, Quaternion.Euler(new Vector3(pitch, yaw, 0)), 0.95f);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            jump_buffer = true;
        }

        if (Input.GetKeyUp(KeyCode.Space)) {
            jump_buffer = false;
        }

        if (Input.GetKeyDown(KeyCode.R)){
            rb.position = spawnPos;
        }

        float noiseStrength = Mathf.Max(Mathf.Abs(transform.position.x/100f)-2.5f, Mathf.Abs(transform.position.z/100f)-2.5f);
        noise.volume = noiseStrength;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        slide_buffer = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.C);
        
        bool grounded = OnGround();

        if (sliding) ApplyFriction(friction * 0.25f);
        else if (grounded) ApplyFriction(friction);
        else ApplyFriction(air_friction);

        jump_timer--;
        coyote_frames--;
        slide_coyote_frames--;
        if (grounded && jump_timer < 0) {
            coyote_frames = coyote_time;
            slide_coyote_frames = coyote_time;
        }
        slide_cooldown--;
        Vector3 input_dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        input_dir = Quaternion.AngleAxis(yaw, Vector3.up) * input_dir;
        input_dir = input_dir.normalized;
        if (UIManager.Instance.pauseMenu.activeSelf) {
            input_dir = Vector3.zero;
        }
        rb.AddForce(sliding ? input_dir * accel * 0.2f * Time.deltaTime : !grounded ? AirAccelerate(input_dir) : input_dir * accel * Time.deltaTime, ForceMode.VelocityChange);

        if (coyote_frames > 0 && jump_buffer) {
            if (rb.velocity.y < 0) {
                rb.velocity = new Vector3(rb.velocity.x, 0 , rb.velocity.z);
            }
            rb.AddForce((GroundNormal() + Vector3.up * 2).normalized * jump_force, ForceMode.Impulse);
            jump_buffer = false;
            coyote_frames = 0;
            jump_timer = 1;
            if (jump_sfx_instance != null) Destroy(jump_sfx_instance);
            jump_sfx_instance = Instantiate(jump_sfx,transform.position,Quaternion.identity);
        }

        if (slide_buffer && slide_coyote_frames > 0 && slide_cooldown <= 0 && !sliding) {
            sliding = true;
            Vector3 slideForce = input_dir * Time.deltaTime * slide_force;
            Quaternion upToNormal = !grounded ? Quaternion.identity : Quaternion.FromToRotation(Vector3.up, GroundNormal());
            rb.AddForce(upToNormal * slideForce, ForceMode.VelocityChange);
            standing_collider.center = sliding_collider.center;
            standing_collider.height = sliding_collider.height;
            camera.localPosition = Vector3.down * 0.8f;
            //standing_collider.enabled = false;
            //sliding_collider.enabled = true;
        }

        if (sliding) {
            slide_cooldown = slide_cooldown_time;
            if (!slide_buffer && CanStopSliding())
            {
                sliding = false;
                standing_collider.center = Vector3.zero;
                standing_collider.height = 2f;
                camera.localPosition = Vector3.zero;
                //standing_collider.enabled = true;
                //sliding_collider.enabled = false;
            }
        }

        if (grounded || sliding) {
            Vector3 horizontal = rb.velocity;
            horizontal.y = 0;
            dynamic_air_max_speed = Mathf.Max(air_base_max_speed, horizontal.magnitude - 3);
        }

        //Debug.Log("Air Max Speed: " + dynamic_air_max_speed + " u/s");

        currentCollisions = new List<Collision>();
    }

    bool CanStopSliding() {
        return !Physics.CheckCapsule(transform.position - Vector3.up * 0.4f, transform.position + Vector3.up * 0.5f, 0.5f, ground, QueryTriggerInteraction.Ignore);
    }

    void ApplyFriction(float friction) {
        Vector3 horizontal_velocity = rb.velocity;
        horizontal_velocity.y = 0;
        float friction_factor = 1f - (friction * Time.deltaTime);
        Vector3 new_horizontal_velocity = horizontal_velocity * friction_factor;

        rb.AddForce(new_horizontal_velocity-horizontal_velocity,ForceMode.VelocityChange);
    }

    bool IsGround(Vector3 normal) {
        return Vector3.Angle(Vector3.up,normal) < ground_max_angle;
    }

    void OnCollisionStay(Collision collision)
    {
        currentCollisions.Add(collision);
    }

    bool OnGround() {
        if (currentCollisions == null || currentCollisions.Count < 1) return false;
        return currentCollisions.Where(
            c => c.contacts.Where(
                contact => IsGround(contact.normal)
            ).Any()
        ).Any();
    }

    Vector3 GroundNormal() {
        if (!OnGround()) return Vector3.up;

        IEnumerable<Vector3> normals = currentCollisions.SelectMany(
            c => c.contacts.Select(
                contact => contact.normal
            ).Where(
                normal => IsGround(normal)
            )
        );

        Vector3 average = Vector3.zero;

        foreach (Vector3 n in normals) {
            average += n;
        }

        return average.normalized;
    }

    Vector3 AirAccelerate(Vector3 wishdir) {
        Vector3 horizontal_vel = rb.velocity;
        horizontal_vel.y = 0;
        float current_speed = Vector3.Dot(horizontal_vel, wishdir);
        float add_speed = accel * 0.6f * Time.deltaTime;
        if (current_speed + add_speed > dynamic_air_max_speed) add_speed = dynamic_air_max_speed - current_speed;
        return wishdir * add_speed;
    }
}
