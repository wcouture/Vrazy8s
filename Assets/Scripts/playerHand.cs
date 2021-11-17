using System.Collections.Generic;
using UnityEngine;

public class playerHand : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    private List<GameObject> cards;

    private void Awake()
    {
        cards = new List<GameObject>(15);
    }
    public void updateCards(int count)
    {
        foreach (GameObject gm in cards)
        {
            Destroy(gm);
        }
        cards.Clear();

        for(int i = 0; i < count; i++)
        {
            cards.Add(Instantiate(prefab, this.transform));
        }

    }
}
