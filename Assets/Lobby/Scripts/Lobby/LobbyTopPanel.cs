using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Prototype.NetworkLobby
{
    public class LobbyTopPanel : MonoBehaviour
    {
        public bool isInGame = false;

        protected bool isDisplayed = false;
        protected Image panelImage;


        void Start()
        {
            panelImage = GetComponent<Image>();
       
        }


        void Update()
        {
            if (!isInGame){
                return;
                }
             


            if (Input.GetKeyDown(KeyCode.Escape))
            {
               // Debug.Log(isDisplayed);
                ToggleVisibility(!isDisplayed);
            }

        }

        public void ToggleVisibility(bool visible)
        {
            isDisplayed = visible;
            foreach (Transform t in transform)
            {
                //Debug.Log(isDisplayed);
                t.gameObject.SetActive(isDisplayed);
            }

            if (panelImage != null)
            {
            
                panelImage.enabled = isDisplayed;
            }
            
        }
    }
}