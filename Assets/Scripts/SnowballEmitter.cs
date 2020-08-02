using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class SnowballEmitter : NetworkBehaviour {

    // Snowball forces
    public float m_MinLaunchForce;        // The force given to the snowball if the fire button is not held.
    public float m_MaxLaunchForce;        // The force given to the snowball if the fire button is held for the max charge time.
    public float m_MaxChargeTime;       // How long the snowball can charge for before it is fired at max force.
    private float m_CurrentLaunchForce;     // The force that will be given to the snowball when the fire button is released.
    private float m_ChargeSpeed;            // How fast the launch force increases, based on the max charge time.
    
    // Flags
    private bool m_Fired;                   // If snowball is fired

    // Snowball
    public GameObject snowballPrefab;          // Snowball prefab
    public int loadedSnowballs;             // Number of loaded snowballs
    public Transform snowballPosition;      // Emitter position
    
    // Player animator
    private Animator animator;

    // Pick up snow sound effect
    public AudioSource pickUpSnow;

    // Check if player is in the air
    public bool isInAir = false;

    // Snowball throw cooldown
    [SyncVar]
    private bool delayOn = false;

    // Camera
    public Camera playerCam;

    void Start () 
    {
        animator = GetComponent<Animator>();
        // The rate that the launch force charges up is the range of possible forces by the max charge time.
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

        // Set snowballs to 0
        loadedSnowballs = 0;
    }

	// Update is called once per frame
    [ClientCallback]
	void Update () 
    { 
        // Throw animation
        animator.SetBool("LoadThrow", false);

        // --------------------------- Check if local player -------------------------
        if (isLocalPlayer){
            // Check number of snowballs the players has
            CheckSnowball();

            // If the right mouse key is pressed and the throwing animation is false
            if (Input.GetMouseButtonDown(1) && animator.GetCurrentAnimatorStateInfo(0).IsName("Pick Up") == false && !isInAir)
            {
                Debug.Log(animator.GetCurrentAnimatorStateInfo(0).IsName("Pick Up"));
                // If number of snowballs are less than 5 (max)
                if(loadedSnowballs < 5){
                    // Start the animation
                    animator.SetBool("PickUp", true);

                    // Play sound effect
                    pickUpSnow.Play();

                    // Call load snowball on server
                    CmdLoadSnowball();
                }
            }else{
                // Set animation to false
                animator.SetBool("PickUp", false);
            }
            
            //--------------------- Snowball checks and fire ---------------------------
            if(loadedSnowballs == 0 || delayOn){
                return;
            }
            // If the max force has been exceeded and the shell hasn't yet been launched...
            else if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                animator.SetBool("LoadThrow", true);
                Fire();
                delayOn = true;
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (Input.GetMouseButtonDown(0))
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (Input.GetMouseButton(0) && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (Input.GetMouseButtonUp(0) && !m_Fired)
            {
                // ... launch the shell.
                animator.SetBool("LoadThrow", true);
                Fire();
                delayOn = true;
            }
        }     
    }
    
    [Command]
    public void CmdLoadSnowball()
    {
        RpcLoadSnowball();
    }
     
    [ClientRpc]
    public void RpcDeleteSnowball()
    {
        loadedSnowballs -= 1;
        Debug.Log(gameObject.name + " has " + loadedSnowballs + " snowballs");
    }

    [ClientRpc]
    public void RpcLoadSnowball()
    {
        loadedSnowballs += 1;
        Debug.Log(gameObject.name + " has " + loadedSnowballs + " snowballs");
    }

    private void CheckSnowball()
    {
        // Find UI canvas in the scene
        GameObject UI = GameObject.Find("UI");

        // Find gameobject with snowballs
        GameObject Snowballs = UI.transform.Find("Snowballs").gameObject;

        // Loop trough every snowball in gmaeobject Snowballs
        for(int i = 0; i < Snowballs.transform.GetChildCount(); i++)
        {
            // If number of loaded snowballs are more than index set color to white otherwise black
            if(loadedSnowballs >= (i+1)){
                GameObject snowballGameObject = Snowballs.transform.GetChild(i).gameObject;
                Image snowballImage = snowballGameObject.GetComponent<Image>();
                snowballImage.color = new Color32(255,255,255,255);
            }else{
                GameObject snowballGameObject = Snowballs.transform.GetChild(i).gameObject;
                Image snowballImage = snowballGameObject.GetComponent<Image>();
                snowballImage.color = new Color32(55,55,55,255);
            }
        }
    }


    private void Fire()
    {    
        // Set the fired flag so only Fire is only called once.
        m_Fired = true;

        // Hitpoint vector
        Vector3 hitPoint;

        // Create ray
        Ray shootRay = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit shootHit;  

        // Find point for direction vector
        if(Physics.Raycast(shootRay, out shootHit)){
            Debug.Log("Raycast hit: " + shootHit.transform.name + " hitpoint: " + shootHit.point);
            hitPoint = shootHit.point;
                
            // Player position
            Vector3 playerPosition = gameObject.transform.position;

            // Snowball direction vector
            Vector3 direction = hitPoint-playerPosition;

            Debug.Log("Direction vector: " + direction);

            // Fire snowball on server
            CmdFire(m_CurrentLaunchForce, transform.forward, snowballPosition.position, snowballPosition.rotation, direction);

            // Reset the launch force.
            m_CurrentLaunchForce = m_MinLaunchForce;
            }
    }


    [Command]
    private void CmdFire(float launchForce, Vector3 forward, Vector3 position, Quaternion rotation, Vector3 direction)
    {
        // Set the fired flag so only Fire is only called once.
        m_Fired = true;

        if (snowballPrefab != null)
        {

            // Create snowball instance
            GameObject snowballInstance = Instantiate(snowballPrefab, position, rotation);

            // Set snowball name
            snowballInstance.name = gameObject.name;

            // Scale down snowball
            snowballInstance.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            // Get rigidbody
            Rigidbody shellBody = snowballInstance.GetComponent<Rigidbody>();

            // Set snowball velocity forward
            Vector3 velocity = Vector3.zero + launchForce * direction.normalized;

            // Add velocity and rotation
            shellBody.velocity = velocity;
            shellBody.AddTorque(transform.right * 300);

            Debug.Log("Snowball thrown!");

            // Spawn snowball on server
            NetworkServer.Spawn(snowballInstance);

            // Remove one snowball 
            RpcDeleteSnowball();

            // Start snowball cooldown time
            StartCoroutine(Delay());
            }
    }

    private IEnumerator Delay(){
        // Delay for snowball
        yield return new WaitForSeconds(0.5f);
        RpcDelay();
    }

    [ClientRpc]
    private void RpcDelay(){
        delayOn = false;
    }
}
