using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class testbutton : MonoBehaviour
{
    public AssetReference playerdeckprefab;
    PlayerDeck playerdeck;

    Button button;
    public Deck deck;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(onbuttonclick);
    }

    void Awake()
    {
        playerdeckprefab.LoadAssetAsync<PlayerDeck>().Completed += handle =>

        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                playerdeck = handle.Result;
                Debug.Log("playerdeck prefab loaded successfully!");
            }
            else
            {
                Debug.LogError("Failed to load playerdeck prefab!");
            }
        };
    }

    void onbuttonclick()    //for test
    {
        playerdeck.AddList("12");
        deck.DrawCard();
    }

    void Update()
    {

    }
}
