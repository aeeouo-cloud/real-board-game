// CardDisplay.cs
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Data Binding")]
    public string CardID; // 이 카드가 나타내는 실제 카드 ID

    // 🚨 [필수] 이 카드의 코스트 값을 저장할 변수 🚨
    public int CardCost { get; private set; }

    // UI 필드는 나중에 UI 담당자가 추가할 곳

    // HandManager가 이 함수를 호출하여 카드 ID를 주입합니다.
    public void Initialize(string id)
    {
        CardID = id;
        Debug.Log($"[CardDisplay] ID {CardID}로 초기화 완료. 데이터 로딩 시작.");
        LoadCardData(); // 카드의 모든 데이터를 로드하는 함수 호출
    }

    private void LoadCardData()
    {
        if (DataManager.Instance == null || string.IsNullOrEmpty(CardID))
        {
            Debug.LogError($"DataManager 또는 CardID가 준비되지 않았습니다: {CardID}");
            CardCost = 0; // 안전을 위해 코스트 0 할당
            return;
        }

        // 🚨 DataManager의 TryGetCardCost 함수를 호출하여 실제 코스트를 가져옵니다. 🚨
        if (DataManager.Instance.TryGetCardCost(CardID, out int cost))
        {
            CardCost = cost;
            Debug.Log($"[CardDisplay] {CardID} 데이터 로딩 성공. 실제 코스트: {CardCost}");
        }
        else
        {
            CardCost = 0;
            Debug.LogError($"[CardDisplay] {CardID} 코스트 로딩 실패. 기본값 0 할당.");
        }
    }

    // (TODO) 카드를 사용하려는 입력을 감지하는 로직이 여기에 들어갑니다.
    private void OnMouseDown()
    {
        // 🚨 입력 감지 시 HandManager에게 사용을 요청합니다. 🚨
        if (HandManager.Instance != null)
        {
            HandManager.Instance.TryUseCard(CardID);
            Debug.Log($"[Input] {CardID} 카드 사용 요청.");
        }
    }
}