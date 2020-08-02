using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraOnStart : MonoBehaviour
{

    public GameObject playerCam;
    // Start is called before the first frame update
    void Start()
    {
        // Activate Cinemachine on start.
        playerCam.SetActive(true);
        
    }

}
