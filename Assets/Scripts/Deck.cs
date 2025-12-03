using System;
using System.Collections.Generic;
using System.Linq;
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
    Task inittask;
    void Awake()
    {   
        playerdeck.Load();
        idlist = playerdeck.playerdecklist.ToList();
        if(instance == null) instance = this;
        else {Destroy(gameObject); return;}

        inittask = Loadasset().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
        },TaskScheduler.FromCurrentSynchronizationContext()
        );

        LastCardCancel += () => {Debug.Log("lastcardcancel called");};

    }
    async Task Loadasset()
    {
        var task = Addressables.LoadAssetAsync<GameObject>(handprefab);
        await task.Task;
        parentcanvas = task.Result;
        newhand = Instantiate(parentcanvas);
    }
    async public void DrawCard()  // need card empty logic
    {
        if (inittask != null)
        {
            await inittask;
        }
        if (parentcanvas == null || newhand == null)
        {
            Debug.Log("Load hand falied");
            return;
        }
        if (idlist == null || idlist.Count == 0)
        {
            Debug.Log("playerdeck empty");
            return;
        }
        ReoderActive();
        if(cardindex < handlimit)
        {
            string cardID = Deck.instance.idlist[0];
            int rand = UnityEngine.Random.Range(0, idlist.Count);
            GameObject nextcard = newhand.transform.GetChild(cardindex).gameObject;
            nextcard.GetComponent<CardMono>().cardid = idlist[rand];
            nextcard.SetActive(true);
            GameManager.Instance.PlayerHand.Add(idlist[rand]);
            idlist.RemoveAt(rand);
            Debug.Log($"[Draw System] {cardID} 카드 드로우. 남은 덱: {Deck.instance.idlist.Count}, 현재 손패: {GameManager.Instance.PlayerHand.Count}");
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
    void OnDestroy()
    {
        LastCardCancel = null;

        if (instance == this)
        {
            instance = null; // 인스턴스가 파괴될 때 정적 필드를 null로 비워줘야 해!
        }
    }
}
