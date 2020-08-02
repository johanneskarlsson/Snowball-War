using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMotor))]

public class PlayerControllerGame : MonoBehaviour {

    // Player variables
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float jump = 250f;
    [SerializeField]
    private float fall = 2.5f;
    [SerializeField]
    private float lookSensitivity = 1f;

    // Jump sound effect
    public AudioSource jumpsound;

    //Component Caching
    private Animator animator;
    private PlayerMotor motor;
    private SnowballEmitter snowballEmitter;

    // If player is on the ground
    public bool isGrounded = true;


    void Awake()
    {
        // Lock mouse cursor on awake
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start ()
    {
        motor = GetComponent<PlayerMotor>();
        animator = GetComponent<Animator>();
        snowballEmitter = GetComponent<SnowballEmitter>();
    }

    void Update(){
        // Toggles mouse cursor on and off when clicking escape
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(Cursor.lockState == CursorLockMode.None){
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log("Locked!");
            }else if(Cursor.lockState == CursorLockMode.Locked){
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Debug.Log("Not Locked!");
            }
        }
    }

    void FixedUpdate()
    {

        //Calculate movement velocity as a 3D vector
        float _xRot = Input.GetAxis("Mouse X");
        float _zMov = Input.GetAxis("Vertical");
        float _xMov = Input.GetAxis("Horizontal");

        //Animate movement
        animator.SetFloat("VelX", _xMov);
        animator.SetFloat("VelY", _zMov);

        Vector3 _MovVertical = transform.forward * _zMov;
        Vector3 _MovHorizontal = transform.right * _xMov / 2;

        //Final movement vector
        Vector3 _velocity = (_MovVertical + _MovHorizontal) * speed;

        //Apply movement
        motor.ApplyMovement(_velocity);

        Vector3 _rotation = new Vector3(0f, _xRot * lookSensitivity, 0f);

        //Apply rotation
        motor.ApplyRotation(_rotation);

        //Calculate jump
        Vector3 _jump = Vector3.zero;
        Vector3 _fall = Vector3.zero;
  
        if (Input.GetButton("Jump") && isGrounded)
        {
            _jump = Vector3.up * jump;
            _fall = Vector3.up * fall;
            var rb = GetComponent<Rigidbody>();
            rb.AddForce(_jump * 130);
            isGrounded = false;

            // Play animation and sound effect
            animator.SetBool("IsInAir", true);
            jumpsound.Play();

            // Set isInAir variables to true
            motor.isInAir = true;
            snowballEmitter.isInAir = true;
        }


        if(isGrounded){
            // Stop animation
            animator.SetBool("IsInAir", false);

            // Set isInAir variables to false
            motor.isInAir = false;
            snowballEmitter.isInAir = false;
        }
    }

    void OnCollisionStay (Collision other)
    {

        Vector3 hit = other.contacts[0].normal;
        float angle = Vector3.Angle(hit, Vector3.up);

        // If the collision is with the ground
        if (Mathf.Approximately(angle, 0))
        {
            // Down
            isGrounded = true;
            if (other.gameObject.transform.root.gameObject.name != "Terrain")
            {
                isGrounded = false;
                if (isGrounded = false && other.gameObject.transform.root.gameObject.name != "Terrain")

                    // Disable the jumping and wait for when the player is on the ground.
                    if (other.gameObject.transform.root.gameObject.name == "Terrain")
                    {
                        isGrounded = true; //Add the jumping mechanic back in.
                        animator.SetBool("IsInAir", false);
                    }
            }
        }
        
        // If the collision is with a wall
        if (Mathf.Approximately(angle, 90) && Mathf.Approximately(angle, 0) == false)
        {
            isGrounded = false;
        }
    }

    void OnCollisionExit(){
            isGrounded = false;
    }
}