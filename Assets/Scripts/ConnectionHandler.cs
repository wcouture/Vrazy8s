using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionHandler : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject connectButton;

    [SerializeField]
    private GameObject connectionText;

    [SerializeField]
    private InputField nameInput;

    [SerializeField]
    private byte maxPlayers = 8;

    private bool connecting = false;

    #region nameGeneration
    private string[] part1 = new string[] {
        "Flying",
        "Domesticated",
        "Euthanized",
        "Deified",
        "Purple",
        "Strange",
        "Mysterious",
        "Promiscuous"
    };

    private string[] part2 = new string[] {
        "Donkey",
        "Soccerball",
        "Potato",
        "Rhombus",
        "Soviet",
        "Communist",
        "Xbox",
        "Dissapointment"
    };
    #endregion

    public void Start()
    {
        connectButton.SetActive(true);
        connectionText.SetActive(false);

        string playerName = "";
        playerName += part1[Random.Range(0, 8)];
        playerName += part2[Random.Range(0, 8)];

        nameInput.text = playerName;
    }

    public void Connect()
    {
        connecting = true;

        connectButton.SetActive(false);
        connectionText.SetActive(true);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        else
        {
            joinCreateRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        joinCreateRoom();
    }

    public void joinCreateRoom()
    {
        if (PhotonNetwork.IsConnected && nameInput.text != null && connecting)
        {
            connecting = false;
            Debug.Log("Joining Random Room");
            PhotonNetwork.JoinRandomRoom();
            PlayerPrefs.SetString("nickname", nameInput.text);
            PhotonNetwork.NickName = PlayerPrefs.GetString("nickname");
        }
        else if (nameInput.text == null)
        {
            Debug.Log("Input a name");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to Join Random Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayers });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Random Room");
        PhotonNetwork.LoadLevel("lobby");
    }
}
