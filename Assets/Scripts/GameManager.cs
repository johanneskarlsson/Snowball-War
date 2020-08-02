using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
using UnityEngine.UI;
using TMPro;


public class GameManager : NetworkBehaviour
{

    // Game manager instance
    private static GameManager instance;

    public MatchSettings matchSettings;

    // Playerlist prefab
    public GameObject playerItemPrefab;

    // Game over screen
    public GameObject winnerPrefab;
    
    // Player dictionary
    [SerializeField]
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();


  public static void RegisterPlayer(string _netID, Player _player)
  {
    string _playerID = _netID;

    // Add player to dictionary
    players.Add(_playerID, _player);


    // Loop through every player in dictionary
    foreach(KeyValuePair<string,Player> m_player in players){
        Debug.Log("ID: " + m_player.Key);
        Debug.Log("Name: " + m_player.Value.name);
    }

    // Check length of the dictionary
    int length = players.Count;
    Debug.Log("List length: " + length);
  }


    public static void UnRegisterPlayer(string _playerID){
        players.Remove(_playerID);

        instance.RpcUpdatePlayerList();

        // Loop through every player in dictionary
        foreach(KeyValuePair<string,Player> m_player in players){
            Debug.Log("ID: " + m_player.Key);
            Debug.Log("Name: " + m_player.Value.name);   
        }
        // Check length of the dictionary
        int length = players.Count;
        Debug.Log("List length: " + length);
    }

    public static Player GetPlayer(string _playerID){
        return players[_playerID];
    }


    void Awake(){
        if(isServer){
            ResetPlayers();
        }

        if(instance != null){
            Debug.LogError("More than one GameManager in the scene");
        
        }
        else{
            instance = this;
        }
    }


    [ServerCallback]
    private void Start()
    {
        Debug.Log("START!");

        // Start game loop
        StartCoroutine(GameLoop());
    }

     private IEnumerator GameLoop()
    {
        // Continue as long as number of players are less than 2
        while (players.Count < 2)
            yield return null;

        // Delay before round starting
        yield return new WaitForSeconds(2.0f);

        RpcUpdatePlayerList();

        // Start round (don't return until it's finished)
        yield return StartCoroutine(RoundStarting());

        // Start round playing 
        yield return StartCoroutine(RoundPlaying());

        // Start round ending
        yield return StartCoroutine(RoundEnding());

        RpcUpdatePlayerList();

        yield return new WaitForSeconds(2.0f);

        // Check who the winner is
        RpcGameWinner();
        yield return new WaitForSeconds(7.0f);

        // Go back to the lobby
        ResetPlayers();
        LobbyManager.s_Singleton.ServerReturnToLobby();

    }

    
    [ClientRpc]
    void RpcUpdatePlayerList(){

        // Find UI playerlist
        GameObject UI = GameObject.Find("UI");
        Transform playersInGame = UI.transform.Find("PlayersInGame");
        Transform playerList = playersInGame.transform.Find("PlayerList");
        Debug.Log(playerList);
       
        // Empty string
        string playersText = "";

        // Destroy all elements in the list
        foreach(Transform playerItem in playerList){
           Destroy(playerItem.gameObject);
        }

        // Add new playerlist items
        foreach(KeyValuePair<string,Player> m_player in players){
            // Create new playerlist item
            GameObject newPlayerItem = Instantiate(playerItemPrefab);
            GameObject playerName = newPlayerItem.transform.Find("PlayerName").gameObject;
            playerName.GetComponent<TextMeshProUGUI>().text = m_player.Value.name;

            GameObject color = newPlayerItem.transform.Find("Color").gameObject;
            color.GetComponent<Image>().color = m_player.Value.color;

            newPlayerItem.transform.SetParent(playerList);
            newPlayerItem.transform.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }

    [ClientRpc]
    void RpcGameWinner(){
        if(players.Count == 1){
            string winner;
            foreach(KeyValuePair<string,Player> m_player in players){
                Debug.Log("Winner: " + m_player.Value.name);
                winner =  m_player.Value.name;
                GameObject Winner = (GameObject)Instantiate(winnerPrefab);
                GameObject winningPlayer = Winner.transform.Find("Player").gameObject;
                winningPlayer.GetComponent<TextMeshProUGUI>().text = winner + "\n Won the game!";
            }
         }
    }

    void ResetPlayers(){
        players.Clear();
        RpcResetPlayers();
    }

    [ClientRpc]
    void RpcResetPlayers(){
         players.Clear();
    }

    private IEnumerator RoundStarting()
    {
        //we notify all clients that the round is starting
        RpcRoundStarting();

        // Delay before round starting
        yield return new WaitForSeconds(1.0f);
    }

    [ClientRpc]
    void RpcRoundStarting()
    {
        Debug.Log("Game starting!");
    }

    private IEnumerator RoundPlaying()
    {
        //notify clients that the round is now started, they should allow player to move.
        RpcRoundPlaying();

        // While there is not one tank left...
        while (!OneTankLeft())
        {      
            //RpcUpdatePlayerList();
            // ... return on the next frame.
            yield return null;
        }
    }

    [ClientRpc]
    void RpcRoundPlaying()
    {
         Debug.Log("Game playing!");
    }

    private IEnumerator RoundEnding()
    {
        //notify client they should disable tank control
        RpcRoundEnding();

        // Delay before round ending
        yield return new WaitForSeconds(1.0f);
    }

    [ClientRpc]
    private void RpcRoundEnding(){
        Debug.Log("Game ending!");
    }

  private bool OneTankLeft(){
        
        // Start the count of tanks left at zero.
        int numTanksLeft = 0;
        numTanksLeft = players.Count;

        // If there are one or fewer tanks remaining return true, otherwise return false.
        return numTanksLeft <= 1;
    }
}
