using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Deck : MonoBehaviour   //in game deck data
{   
    public static Action LastCardCancel;
    public static Deck instance;
    public Action LastActive;
    public PlayerDeck playerdeck;
    public AssetReferenceGameObject handprefab;
    GameObject parentcanvas;
    GameObject newhand;
    int handlimit = 10;
    int cardindex;
    public List<string> idlist = new List<string>();

    void Awake()
    {
        if(instance == null) instance = this;
        Loadasset();
        LastCardCancel += () => {Debug.Log("lastcardcancel called");};
        //playerdeck.Load();
    }
    async void Loadasset()
    {
        var task = Addressables.LoadAssetAsync<GameObject>(handprefab);
        await task.Task;
        parentcanvas = task.Result;
    }

    async public void DrawCard()  // need card empty logic
    {   
        float timeout =  0;
        while(timeout < 3f && parentcanvas == null)
        {
            await Task.Yield();
            timeout += Time.deltaTime;
        }
        if(newhand == null)
        {
            newhand = Instantiate(parentcanvas);
        }
        if (idlist == null || idlist.Count == 0)
        {
            Debug.Log("playerdeck empty");
            return;
        }
        ReoderActive();
        if(cardindex < handlimit)
        {
            int rand = UnityEngine.Random.Range(0, idlist.Count);
            GameObject nextcard = newhand.transform.GetChild(cardindex).gameObject;
            nextcard.SetActive(true);
            nextcard.GetComponent<CardMono>().cardid = idlist[rand];
            idlist.RemoveAt(rand);
        }
        else
        {
            Debug.Log($"hand count overs {handlimit}");
        }
    }
    void ReoderActive()
    {
        int activeindex = 0;
        for (int i = 0; i < newhand.transform.childCount; i++)
        {
            Transform child = newhand.transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                child.SetSiblingIndex(activeindex);
                activeindex++;
            }
        }
        cardindex = activeindex;
    }
}
