using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TurnManager : MonoBehaviour
{
    public AssetReferenceGameObject diceprefab;
    public GameManager gameManager;
    public GameObject dice;

    void OnEnable()
    {
        // [수정] GameManager 인스턴스를 통해 OnPlayerTurnStart 이벤트에 구독합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerTurnStart += CallTurn;
        }
    }
    void OnDisable()
    {
        // [수정] GameManager 인스턴스를 통해 OnPlayerTurnStart 이벤트 구독을 해지합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerTurnStart -= CallTurn;
        }
    }

    public void CallTurn()
    {
        // 턴 시작 시 주사위 굴림 코루틴 시작
        Debug.Log($"calltrun activated what??? {diceprefab.RuntimeKey}");
        StartCoroutine(DiceStart());
    }
    public void GetDiceResult(int result)
    {
        // gameManager는 인스펙터에 연결된 GameManager 객체입니다.
        gameManager.ApplyDiceResult(result);
    }
    public void EndTurn()
    {
        gameManager.EndPlayerTurn();
    }
    private IEnumerator DiceStart()
    {
        TurnNet turnNet = GetComponent<TurnNet>();

        if(turnNet != null)
        {   
            while(turnNet.IsSpawned == false)
            {
                yield return null;
            }
            turnNet.SpawnObjectWithOwnerServerRpc(diceprefab.RuntimeKey.ToString());
        }
    }
    void OnDestroy()
    {
        dice = null;
    }
    public void destroyinstance(ulong netObjectid)
    {
        TurnNet turnNet = GetComponent<TurnNet>();
        turnNet.DestroyTargetServerRpc(netObjectid);
    }
}
