using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TurnManager : MonoBehaviour
{
    public AssetReferenceGameObject diceprefab;
    public GameManager gameManager; // GameManager 인스턴스가 인스펙터에 연결되어 있다고 가정
    public GameObject dice;

    // 정적 코루틴 헬퍼 함수는 GameManager.Instance의 존재 여부를 알 수 없으므로 제거 (사용하지 않음)
    /*
    static IEnumerator WaitSecond(float waittime)
    {
        yield return new WaitForSeconds(waittime);
    }
    */

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
        StartCoroutine(CallTurnCoroutine());
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
