using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
 using UnityEngine.UI;


[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour 
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    string netID;

    Camera sceneCamera;

    // Start is called before the first frame update
    void Start()
    {
        // If not local player disable components and assign to the remote player layer.
        if(!isLocalPlayer){
           DisableComponents();
           AssignRemoteLayer();
        }
        else{
            sceneCamera = Camera.main;
            if(sceneCamera != null){
             sceneCamera.gameObject.SetActive(false);
            }
        }
    }

    // Get net ID and register player in game manaher
    public override void OnStartClient(){
        base.OnStartClient();
        netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player player = GetComponent<Player>();
        player.localID = netID;
        GameManager.RegisterPlayer(netID, player);
    }
  
    // Assign remote players to the remote player layer
    void AssignRemoteLayer(){
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    // Disable components
    void DisableComponents(){
         for(int i = 0; i < componentsToDisable.Length; i++){
                componentsToDisable[i].enabled = false;
            }
    }


    // On disable activate scene camera and unregister player if neccessary
    void OnDisable(){
        if(sceneCamera != null){
            sceneCamera.gameObject.SetActive(true);
        }

        GameManager.UnRegisterPlayer(netID);
        Debug.Log("OnDisable!");
    }


}
