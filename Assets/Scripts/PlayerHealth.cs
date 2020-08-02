using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : NetworkBehaviour
{
    // Game over screen
    public GameObject gameOverPrefab;

    // Player animator
    private Animator animator;

    // Sound effects
    public AudioSource ouch;
  
    // Health 
    public float currentHealth;   
    private float m_StartingHealth = 100f;   
             

    // This function is called at the start of each round to make sure each tank is set up correctly.
   public void Start()
    {
        animator = GetComponent<Animator>();

        // Set current health to starting health
        currentHealth = m_StartingHealth;
        Debug.Log(gameObject.name + " Health at start: " + currentHealth);
    }

    [ClientRpc]
    public void RpcDamage(float amount)
    {
        //Trigger hit animation
        animator.SetTrigger("Hit");

        // Play ouch sound effect
        ouch.Play();

        // Reduce current health by the amount of damage done.
        currentHealth -= amount;
  
        Debug.Log(transform.name + " now has " + currentHealth + " health.");

        // Update healthbar UI
         UpdatePlayerHealth();

        // If current health is equal or less than zero.
        if (currentHealth <= 0f)
        {
            Die(); 
        }
    }


    void Die()
    {
        // Get net ID for the player
        string netID = GetComponent<NetworkIdentity>().netId.ToString();

        // Start death animation
        animator.SetBool("Dead", true);

        // Disable shooting and movement scripts
        gameObject.GetComponent<PlayerControllerGame>().enabled = false;
        gameObject.GetComponent<SnowballEmitter>().enabled = false;

        // Destroy gameobject and unregister player
        StartCoroutine(RemovePlayer(netID));
   
    }


    private IEnumerator RemovePlayer(string netID){
        
        // Delay for the animation to finish
        yield return new WaitForSeconds(3f);
            
        // Unregister player in game manager
        GameManager.UnRegisterPlayer(netID);

        // Destory player
        Destroy(gameObject);
   
        // Disable UI for player
        if(isLocalPlayer){
            GameObject UI = GameObject.Find("UI");
            GameObject Snowballs = UI.transform.Find("Snowballs").gameObject;
            GameObject HealthBar = UI.transform.Find("LocalHealthBar").gameObject;
            GameObject Wood = UI.transform.Find("Wood").gameObject;
            GameObject Heart = UI.transform.Find("Heart").gameObject;

            Snowballs.SetActive(false);
            HealthBar.SetActive(false);
            Heart.SetActive(false);
            Wood.SetActive(false);

            // Instantiate game over screen
            GameObject GameOver = (GameObject)Instantiate(gameOverPrefab);
        }
         
    }

    private void UpdatePlayerHealth(){
         
        // Max scale health bar
        float maxScale = 8f;

        // New scale after update
        float newScale;

        // Max health
        float maxHealth = m_StartingHealth;

        // -------------------- If local player -----------------------------
        if(!isLocalPlayer){

            // Find gameobject health bar (above player)
            GameObject healthBar = gameObject.transform.Find("Canvas").Find("HealthBar").gameObject;
            
            // Find health bar components
            GameObject bar = healthBar.transform.Find("Bar").gameObject;
            RectTransform rectTransform = bar.GetComponent<RectTransform>();
            Image image = bar.GetComponent<Image>();

            // Width of the health bar
            float width = rectTransform.rect.width;

            // New scale value
            newScale = currentHealth/maxHealth * maxScale;

            if(newScale < 0){
                newScale = 0;
            }

            // Health percentage
            float healthPercentage = currentHealth/maxHealth;

            //Debug.Log(newScale);

            // Green, yellow or red health bar
            if(healthPercentage <= 1 && healthPercentage > 0.5){
                    image.color = new Color32(0,204,70,255);
            }
            else if(healthPercentage <= 0.5 && healthPercentage > 0.3){
                    image.color = new Color32(255,204,70,255);
            }
            else if(healthPercentage < 0.3){
                    image.color = new Color32(255,0,0,255);
            }

            // Calculate health bar offset after update
            float positionOffset = (1 - (currentHealth/maxHealth) )* 400;

            // Set position and rotation of the health bar
            rectTransform.localPosition = new Vector3(positionOffset, 0f,0f);
            rectTransform.localScale = new Vector3(newScale, 2f, 1f);
    
        }else{
        
            // Find UI canvas in the scene
            GameObject UI = GameObject.Find("UI");
    
            // Find gameobject heath bar 
            GameObject localHealthBar = UI.transform.Find("LocalHealthBar").gameObject;
            Debug.Log(localHealthBar);

            // Find bar
            GameObject localBar = localHealthBar.transform.Find("Bar").gameObject;
            RectTransform rectTransform = localBar.GetComponent<RectTransform>();
            Image image = localBar.GetComponent<Image>();

            // Width of the health bar
            float width = rectTransform.rect.width;

            // New scale value
            newScale = currentHealth/maxHealth * maxScale;

            if(newScale < 0){
                newScale = 0;
            }

            // Health percentage
            float healthPercentage = currentHealth/maxHealth;

            Debug.Log(newScale);

            // Green, yellow or red health bar
            if(healthPercentage <= 1 && healthPercentage > 0.5){
                    image.color = new Color32(0,204,70,255);
            }
            else if(healthPercentage <= 0.5 && healthPercentage > 0.3){
                    image.color = new Color32(255,204,70,255);
            }
            else if(healthPercentage < 0.3){
                    image.color = new Color32(255,0,0,255);
            }

            // Calculate healthbar offset
            float positionOffset = (1 - (currentHealth/maxHealth) )* 400;

            // Set position and rotation of the health bar
            rectTransform.localPosition = new Vector3(positionOffset*(-1), 0f,0f);
            rectTransform.localScale = new Vector3(newScale, 2f, 1f);

        }
    }   
}
