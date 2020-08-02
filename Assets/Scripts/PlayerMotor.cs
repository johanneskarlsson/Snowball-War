using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]

public class PlayerMotor : MonoBehaviour {

    // Player cam
    [SerializeField]
    private Camera cam;

    // Walking sound effect
    public AudioSource walksound;
    private bool walksoundOn = false;

    // Check if player is in the air
    public bool isInAir = false;

    // Vectors
    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 jump = Vector3.zero;
    private Vector3 fall = Vector3.zero;

    private Rigidbody rb;
    private AudioSource audiosource;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        audiosource = GetComponent<AudioSource>();
    }


    // Gets a movement vector
    public void ApplyMovement (Vector3 _velocity)
    {
        velocity = _velocity;
    }

    // Gets a rotational vector
    public void ApplyRotation(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    // Gets a jump vector
    public void ApplyJump(Vector3 _jump)
    {
        jump = _jump;
    }

    // Gets a jump vector
    public void ApplyFall(Vector3 _fall)
    {
        fall = _fall;
    }


    //Run every physics iteration
    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
        PerformJump();
    }


    //Perform movement based on velocity variable
    void PerformMovement ()
    {
        if (velocity != Vector3.zero)
        {
            if(!walksoundOn && !isInAir){

                walksound.Play();
                walksoundOn = true;
            }else if(isInAir){
                walksound.Pause();
                walksoundOn = false;
         }
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }else{
            walksound.Pause();
            walksoundOn = false;
        }
    }


    //Perform jump
    void PerformJump ()
    {
        if (jump != Vector3.zero)
        {
            rb.AddForce(jump, ForceMode.Impulse);
        }
    }

    //Perform rotation
    void PerformRotation ()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler (rotation));
    }

}
