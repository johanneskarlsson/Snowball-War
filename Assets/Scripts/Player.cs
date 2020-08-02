using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Player : NetworkBehaviour
{
 
    // Player color
    [SyncVar]
    public Color color = Color.white;

    // Player name
    [SyncVar]
    public string name = "";

    // Player net id
    [SyncVar]
    public string localID;


    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    // Necessary because of a bug to sync player name and color from lobby
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }


    public override void OnStartLocalPlayer(){
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0,true);   
    }

    public override void PreStartClient(){
         GetComponent<NetworkAnimator>().SetParameterAutoSend(0,true);
         
    }

    [ClientRpc]
    private void RpcChangeName(string newName){

        // Change name on client
        name = newName;
        gameObject.name = newName;
    }

    [ClientRpc]
    private void RpcChangeColor(Color newColor){

        // Change color on client
        color = newColor;

        // Change upperbody color
        Renderer upperbodyRenderer = gameObject.transform.Find("Boy_01_Meshes").Find("Boy01_UpperBody_Geo").GetComponent<Renderer>();
        upperbodyRenderer.material.color = newColor;

        // Change hair color
        Renderer hairRenderer = gameObject.transform.Find("Boy_01_Meshes").Find("Boy01_Hair_Geo").GetComponent<Renderer>();
        hairRenderer.material.color = newColor;
    }
      

    private void Update(){

        // If server update change name on the clients (data from lobby)  
        if(isServer){

            // Only solution since SyncVar hooks do not seem to work
            RpcChangeName(name);
            RpcChangeColor(color);
        }

        // Ref to gameobject transform
        Transform childTrans = gameObject.transform.Find("Canvas").Find("PlayerName");

        // Update gamebject name
        gameObject.name = name;
        string text = name;
        childTrans.gameObject.GetComponent<TextMeshProUGUI>().text = text;

        // If local player disable UI above the player's head
        if(isLocalPlayer){
            childTrans.gameObject.SetActive(false);
            GameObject healthBar = gameObject.transform.Find("Canvas").Find("HealthBar").gameObject;
            healthBar.SetActive(false);
        }
    }

}
