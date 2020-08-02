using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{
   public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer){
       LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
       Player localPlayer = gamePlayer.GetComponent<Player>();

        Debug.Log("Lobby player: " + lobby.playerName);
    
        // Change player name on server version of the player script
        localPlayer.name = lobby.playerName;
        localPlayer.color = lobby.playerColor;
   }
}
