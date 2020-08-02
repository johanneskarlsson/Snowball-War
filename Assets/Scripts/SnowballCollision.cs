using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnowballCollision : NetworkBehaviour
{
    // Player tag
    private const string PLAYER_TAG = "Player";
    
    // Snowball impact particle systems
    public GameObject snowImpactPrefab;

    // Snowball damage
    private float damage = 10;

    // Collision point and rotation
    private Vector3 destroyPoint;
    private Quaternion destroyRotation;

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
       
        // Name of the collided object
        Debug.Log(collision.gameObject.name);

        // If the collided object is a player
        if(collision.collider.tag == PLAYER_TAG && collision.gameObject.name != gameObject.name ) {

            Debug.Log(collision.gameObject.name + " was hit!");

            // Get rigidbody of the collided object
            Rigidbody targetRigidbody = collision.gameObject.GetComponent<Rigidbody>();
     
            //Debug.Log(targetRigidbody);

            // Find the PlayerHealth script associated with the rigidbody.
            PlayerHealth targetHealth = targetRigidbody.GetComponent<PlayerHealth>();

            // Deal this damage to the tank.
            targetHealth.RpcDamage(damage);
        }
  
        // In case you hit yourself
        if(collision.gameObject.name != gameObject.name){
            Debug.Log("Shell: " + gameObject.name);

            // Collision hit point
            Debug.Log(collision.contacts[0].point);
            
            // Impact position
            destroyPoint = collision.contacts[0].point;

            // Impact rotation
            destroyRotation = Quaternion.LookRotation(collision.contacts[0].normal);

            // Destroy the snowball on clients.
            GameObject snowball = gameObject;
            NetworkServer.Destroy(snowball);

            // Instantiate a snowball impact particle system
            GameObject snow = (GameObject)Instantiate(snowImpactPrefab, destroyPoint, destroyRotation);
            Debug.Log("Snowball impact");

            // Spawn particle system on the server
            NetworkServer.Spawn(snow);

            // Destroy particle system after 3 seconds
            Destroy(snow, 3f);
        }
    }
}




