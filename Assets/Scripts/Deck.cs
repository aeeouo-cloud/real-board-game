using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour   //in game deck data
{
    public PlayerDeck playerdeck;
    public GameObject parentcanvas;
    public GameObject cardprefab;
    List<string> idlist = new List<string>();

    void Awake()
    {
        playerdeck.Load();
        idlist = playerdeck.playerdecklist;
    }

    public void DrawCard()  // need card empty logic
    {
        if (idlist == null || idlist.Count == 0)
        {
            Debug.Log("playerdeck empty");
            return;
        }
            int rand = Random.Range(0, idlist.Count);
            GameObject newcard = Instantiate(cardprefab, new Vector3(0, 0, 0), Quaternion.identity);
            newcard.GetComponent<CardMono>().carddata.id = idlist[rand];
            newcard.transform.SetParent(parentcanvas.transform, false);
            idlist.RemoveAt(rand);
    }
}
