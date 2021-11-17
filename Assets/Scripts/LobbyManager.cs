using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GridLayoutGroup playerList;

    [SerializeField]
    private GameObject nameTemplate;

    public static int playerNum = -1;

    public static List<string> playerNames;

    private void Awake()
    {
        playerNames = new List<string>();
    }

    private void Start()
    {
        playerNum = PhotonNetwork.CurrentRoom.PlayerCount;
        updatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        updatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        updatePlayerList();
    }

    public void updatePlayerList()
    {
        Debug.Log("Updating List");

        playerNames.Clear();

        Text[] currentNames = playerList.GetComponentsInChildren<Text>();
        foreach(Text gm in currentNames)
        {
            Destroy(gm.gameObject);
        }

        int player = 0;
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            GameObject temp = Instantiate(nameTemplate, playerList.transform);
            temp.name = p.NickName;
            temp.GetComponent<Text>().text = p.NickName;
            temp.GetComponentInChildren<Image>().color = gameHandler.colors[player];
            player++;

            playerNames.Add(p.NickName);
        }
    }

    public void startGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("game");
        }
    }
}
