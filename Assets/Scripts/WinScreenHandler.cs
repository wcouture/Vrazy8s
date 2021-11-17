using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenHandler : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject background;

    [SerializeField]
    private Text winnerText;
    // Start is called before the first frame update
    void Start()
    {
        string winner = PlayerPrefs.GetString("winner");
        winnerText.text = winner + " won!!";
        int index = 0;
        foreach(string name in LobbyManager.playerNames)
        {
            if (name.Equals(winner))
            {
                background.GetComponent<Image>().color = gameHandler.colors[index];
                break;
            }
            index++;
        }
    }

    public void returnToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MainMenu");
    }
}
