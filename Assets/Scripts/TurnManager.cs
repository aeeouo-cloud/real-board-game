using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TurnManager : MonoBehaviour
{
    public AssetReferenceGameObject diceprefab;
    public GameManager gameManager;
    public GameObject dice;
    static IEnumerator WaitSecond(float waittime)
    {
        yield return new WaitForSeconds(waittime);
    }

    void Awake()
    {

        diceprefab.LoadAssetAsync<GameObject>().Completed += handle =>

        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                dice = handle.Result;
                Debug.Log("Dice prefab loaded successfully!");
            }
            else
            {
                Debug.LogError("Failed to load dice prefab!");
            }
        };
    }


    void OnEnable()
    {
        GameManager.PlayerTurnStarted += CallTurn;
    }
    void OnDisable()
    {
        GameManager.PlayerTurnStarted -= CallTurn;
    }

    public void CallTurn()
    {
        StartCoroutine(CallTurnCoroutine());
    }
    public void GetDiceResult(int result)
    {
        gameManager.ApplyDiceResult(result);
    }
    public void EndTurn()
    {
        gameManager.EndPlayerTurn();
    }
    private IEnumerator CallTurnCoroutine() //wait for load
    {
        while (dice == null)
        {
            yield return null;
        }
        GameObject newdice = Instantiate(dice, new Vector3(0, 8, 0), Quaternion.identity);
        newdice.GetComponent<Dice>().turnmanager = this.GetComponent<TurnManager>();
    }
}
