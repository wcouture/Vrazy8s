using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// <c>gameHandler<c> is responsible for all logic and functions that take place during the game.
/// Keeps players in sync while executing proper rotation and card placement.
/// </summary>

public class gameHandler : MonoBehaviourPunCallbacks, IOnEventCallback
{

    public RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

    public int startCardAmount = 6;

    private int currentPlayer;
    private Card currentCard;
    private int selectedCardIndex;

    //Used to track when it is currently this player's turn
    private bool isPicking = false;

    private int rotation = 1;

    private byte playerNum;
    private List<Card> playerHand;

    private byte playerCount;

    GameObject card; //Card image used to animate a card placement.

    // Colors used to assign each player.
    public static Color[] colors = {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan,
        new Color(189,20,227),
        new Color(110,73,36),
        new Color(235,134,12)
    };

    [SerializeField, Range(0, 100)]
    private float pointerShift; //The height shift applied to the pointer designating current player.

    [SerializeField]
    Sprite[] playerImages;

    [SerializeField]
    private GameObject playerSquarePrefab;

    [SerializeField]
    private RectTransform playerPointer;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private GridLayoutGroup cardGrid;

    [SerializeField]
    private GridLayoutGroup grid; //Grid used to organize the player squares

    [SerializeField]
    private GameObject imagePrefab;

    [SerializeField]
    private GameObject smallCard;

    [SerializeField]
    private Animator handPanel;

    [SerializeField]
    private GameObject cardAnimPanel;

    private GameObject[] playerSquares;

    GameObject topCard;

    [SerializeField]
    GameObject cardBackPrefab;

    public Sprite cardBack;

    /// <summary>
    /// Unity Awake method callback. Initializes the playerCount, playerNum,
    /// playerSquares, playerHand variables and runs the setPositions() method.
    /// </summary>
    private void Awake()
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        playerNum = (byte)LobbyManager.playerNum;

        playerSquares = new GameObject[playerCount];

        playerHand = new List<Card>();

        setPositions(); // Generates and organizes player squares to represent each player

        for(int i = 0; i < startCardAmount; i++)    // Adds the amount of cards to start with to the player's hand
            drawCard();

        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayer = playerNum - 1; //Randomly sets the first person to player

            do
            {
                currentCard = new Card();
            } while (currentCard.getType().Equals(type.REVERSE) || currentCard.getType().Equals(type.DRAW)); // Generates a new starting card until it isn't a reverse or a draw two
            playCard(currentCard);
        }
        StartCoroutine("slowLoop"); // Begins the loop to update player card counts visually
    }

    /// <summary>
    /// Unity Start() callback. Initializes the card object used to animate the card placement
    /// and caches the deck card object.
    /// </summary>
    void Start()
    {
        card = Instantiate(new GameObject());
        card.transform.SetParent(GameObject.Find("Canvas").transform);
        card.AddComponent<Image>().sprite = cardBack;
        card.GetComponent<Image>().color = Color.white;
        card.GetComponent<Image>().enabled = false; // Disabled while not being animated
        card.GetComponent<RectTransform>().sizeDelta = new Vector2(39.0625f, 59.67578f);

        topCard = GameObject.Find("DeckCard");
    }


    /// <summary>
    /// Unity Update() callback. 
    /// </summary>
    private void Update()
    {
        try
        {
            // Sets the position of the pointer to above the current active turn player.
            playerPointer.position = playerSquares[currentPlayer].GetComponent<RectTransform>().position + Vector3.up * pointerShift;
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.Log("no current player");
        };
    }

    /// <summary>
    /// Coroutine used for syncing the card counts between players.
    /// Raises event relating to displaying the card count below each player
    /// using minicards as a visual indicator. Called 10 times per second.
    /// </summary>
    /// <returns>
    /// IEnumerator - Waits for .1 seconds after updating player card counts before updating them again.
    /// </returns>
    IEnumerator slowLoop()
    {
        while (true)    // Loops the entirity of the time in this scene after being first started.
        {
            byte[] outData = new byte[] {(byte)(playerNum - 1), (byte)playerHand.Count };   // Serializing the player number and card count into byte array
            PhotonNetwork.RaiseEvent(42,outData, raiseEventOptions, SendOptions.SendReliable);  // Raising event with proper event code to signify it is for updating card counts.
            yield return new WaitForSeconds(.1f);
        }
    }

    /// <summary>
    /// Called when the player draws a card from the deck.
    /// Adds card to player hand and calls event to go to next player using same current card.
    /// </summary>
    public void unableToPlay()
    {
        if(currentPlayer == playerNum - 1)
        {
            drawCard();
            handPanel.SetTrigger("hideHand");
            PhotonNetwork.RaiseEvent((byte)21, null, raiseEventOptions, SendOptions.SendReliable); // Raises event with same card data and extra signifier stating it is the same card
        }
    }

    /// <summary>
    /// Called when a player leaves the room. Causes every other player to leave the room as well.
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Called after player leaves the game, brings them back to the main menu.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Generalized method to play a card and end the current turn.
    /// </summary>
    /// <param name="card">The card being played.</param>
    public void playCard(Card card)
    {
        byte[] data = new byte[] { (byte)card.getColor(), (byte)card.getType(), (byte)card.getNum()};   // Serializes provided card information into byte array to be sent to other clients
        PhotonNetwork.RaiseEvent((byte)currentPlayer, data, raiseEventOptions, SendOptions.SendReliable);   // Raises event to end turn and go to next player.
    }

    /// <summary>
    /// Adds a new card to the players hand.
    /// </summary>
    public void drawCard()
    {
        playerHand.Add(new Card());
    }

    /// <summary>
    /// Executed at the beginning of the game, creates a colored square to represent each player
    /// and adds it to the gridlayout. Randomly sets each player's icon as well.
    /// </summary>
    private void setPositions()
    {
        bool[] playerImgs = new bool[8];    //Used to track which images have already been assigned
        for (int i = 0; i < playerImgs.Length; i++)
            playerImgs[i] = false;

        for(int i = 0; i < playerCount; i++)
        {
            GameObject temp = Instantiate(playerSquarePrefab, grid.transform);
            temp.name = "player" + i;
            temp.GetComponentInChildren<Text>().text = LobbyManager.playerNames[i]; //Accesses list of player names created in the lobby joining step
            temp.GetComponent<Image>().color = colors[i];
            
            int imgNum = -1;
            do
            {
                imgNum = UnityEngine.Random.Range(0, 8);
            } while (playerImgs[imgNum]); // true if the player image has already been used
            playerImgs[imgNum] = true;

            GameObject img = Instantiate(imagePrefab, temp.transform);
            img.GetComponent<Image>().sprite = playerImages[imgNum];

            playerSquares[i] = temp;
        }
    }

    /// <summary>
    /// Identifies the card clicked, checks it is a playable card, plays the card and shifts to next player's turn.
    /// </summary>
    /// <param name="name">name of card button indicating the card's index in player hand</param>
    public void endTurn(string name)
    {
        if (!isPicking) //Return if the player's turn is already over.
        {
            return;
        }
        selectedCardIndex = int.Parse(name); //Finds index of the card using the card object's name
        if (playerHand[selectedCardIndex].Equals(currentCard)) //checks that the card is playable
        {

            isPicking = false; //marks turn is over

            Card tempCard = playerHand[selectedCardIndex];
            playerHand.RemoveAt(selectedCardIndex);
            handPanel.SetTrigger("hideHand");

            if (playerHand.Count <= 0) //Placed down last card
            {
                PhotonNetwork.RaiseEvent((byte)69, (byte)playerNum, raiseEventOptions, SendOptions.SendReliable); //Raises the event signifying the player has won providing the player's number
            }
            else
            {
                playCard(tempCard);
            }
        }
        
    }

    /// <summary>
    /// Executed at the start of the player's turn if they aren't played a draw two.
    /// Clears and re-adds card buttons to the card gridlayout and moves the grid of cards
    /// into view.
    /// </summary>
    public void startTurn()
    {   
        int index = 0;
        foreach(Button gm in cardGrid.GetComponentsInChildren<Button>())
        {
            Destroy(gm.gameObject);
        }

        foreach(Card card in playerHand)
        {
            GameObject temp = Instantiate(cardPrefab, cardGrid.transform);
            temp.name = index.ToString();

            string path = "";
            switch (card.getColor()) {
                case color.RED:
                    temp.GetComponent<Image>().color = Color.red;
                    break;
                case color.GREEN:
                    temp.GetComponent<Image>().color = Color.green;
                    break;
                case color.BLUE:
                    temp.GetComponent<Image>().color = Color.blue;
                    break;
                case color.YELLOW:
                    temp.GetComponent<Image>().color = Color.yellow;
                    break;
            }

            temp.GetComponent<Button>().onClick.AddListener(delegate { endTurn(temp.name); });

            int num = card.getNum();
            if (num == -1)
            {
                if (card.getType().Equals(type.REVERSE))
                {
                    temp.GetComponentInChildren<TextMeshProUGUI>().text = "R";
                }
                else if (card.getType().Equals(type.DRAW))
                {
                    temp.GetComponentInChildren<TextMeshProUGUI>().text = "D";
                }
            }
            else
            {
                temp.GetComponentInChildren<TextMeshProUGUI>().text = num.ToString();
            }

            index++;
        }

        handPanel.SetTrigger("showHand");
    }

    /// <summary>
    /// Used to display each player's card count using minicards under each person's square.
    /// </summary>
    /// <param name="pNum">Player number - 1 signifying which square to place the minicards under</param>
    /// <param name="cardCount">card count of the designated player</param>
    private void updateSquares(byte pNum, byte cardCount)
    {
        GameObject temp = playerSquares[pNum];

        playerHand hand = temp.GetComponentInChildren<playerHand>();
        if(hand != null)
        {
            hand.updateCards(cardCount);
        }
    }

    /// <summary>
    /// Called each time <c>PhotonNetwork.RaiseEvent();<c> is used. Used for syncing player info and handling
    /// rotation across the network.
    /// </summary>
    /// <param name="photonEvent">Event object holding the information specifying the nature of the event</param>
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code) {
            case 69:
                Debug.Log("Winner");
                PlayerPrefs.SetString("winner", LobbyManager.playerNames[(byte)photonEvent.CustomData - 1]);
                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.LoadLevel("WinnerScreen");
                break;
            case 42:
                byte[] inData = (byte[])photonEvent.CustomData;
                updateSquares(inData[0], inData[1]);
                return;
        }
        Debug.Log("Last Player : " + photonEvent.Code);



        byte[] data = (byte[])photonEvent.CustomData;

        if (photonEvent.Code < 21)
        {

            currentPlayer = photonEvent.Code;

            currentCard = new Card((color)data[0], (type)data[1], (int)data[2]);

            StartCoroutine(animateCard(currentPlayer));
        }



        

        if (currentCard.getType() == type.REVERSE)
        {
            rotation *= -1;
        }
        currentPlayer += rotation;

        if (currentPlayer == playerCount)
        {
            currentPlayer = 0;
        }
        else if (currentPlayer == -1)
        {
            currentPlayer = playerCount - 1;
        }

        Debug.Log("CurrentPlayer : " + currentPlayer);

        if(currentPlayer == playerNum - 1)
        {
            
            if (currentCard.getType().Equals(type.DRAW))
            {
                StartCoroutine(drawTwo());
            }
            else
            {
                isPicking = true;
                startTurn();
            }
            
        }
    }

    /// <summary>
    /// Coroutine used to animate a card placement using <c>Vector2.Lerp();<c> before
    /// displaying the placed card.
    /// </summary>
    /// <param name="playerNum">index of the player square the card is originating from.</param>
    /// <returns>IEnumerator - waits till the end of each frame while moving the card.</returns>
    IEnumerator animateCard(int playerNum)
    {
        card.GetComponent<Image>().enabled = true;
        Vector2 startPos = playerSquares[playerNum].transform.position;
        Vector2 endPos = topCard.transform.position;
        float totalTime = 1.0f;
        float timeElapsed = 0;
        card.transform.position = startPos;
        while (timeElapsed < totalTime)
        {
            card.GetComponent<RectTransform>().position = Vector2.Lerp(startPos, endPos, timeElapsed/totalTime);
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        card.GetComponent<Image>().enabled = false;


        switch (currentCard.getColor())
        {
            case color.RED:
                topCard.GetComponent<Image>().color = Color.red;
                break;
            case color.GREEN:
                topCard.GetComponent<Image>().color = Color.green;
                break;
            case color.BLUE:
                topCard.GetComponent<Image>().color = Color.blue;
                break;
            case color.YELLOW:
                topCard.GetComponent<Image>().color = Color.yellow;
                break;
        }
        int num = currentCard.getNum();
        if (num < 0 || num > 9)
        {
            if (currentCard.getType().Equals(type.REVERSE))
            {
                topCard.GetComponentInChildren<TextMeshProUGUI>().text = "R";
            }
            else if (currentCard.getType().Equals(type.DRAW))
            {
                topCard.GetComponentInChildren<TextMeshProUGUI>().text = "D";
            }
        }
        else
        {
            topCard.GetComponentInChildren<TextMeshProUGUI>().text = num.ToString();
        }
    }

    /// <summary>
    /// Coroutine used when a draw two is played to display two card additions to 
    /// the players hand.
    /// </summary>
    /// <returns>IEnumerator - Waits before and inbetween card draws.</returns>
    IEnumerator drawTwo()
    {
        yield return new WaitForSeconds(1.5f);
        drawCard();
        yield return new WaitForSeconds(.5f);
        drawCard();
        Card tempCard;
        do
        {
            tempCard = new Card();
        } while (tempCard.getType().Equals(type.REVERSE) || tempCard.getType().Equals(type.DRAW));
        byte[] data = new byte[] { (byte)tempCard.getColor(), (byte)tempCard.getType(), (byte)tempCard.getNum()};
        PhotonNetwork.RaiseEvent((byte)currentPlayer, data, raiseEventOptions, SendOptions.SendReliable);
    }
}
