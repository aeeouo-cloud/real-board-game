// CardDisplay.cs
using UnityEngine;
using UnityEngine.EventSystems; // OnPointerClick을 위해 필수

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("Data Binding")]
    public string CardID; // 이 카드가 나타내는 실제 카드 ID

    // 이 카드의 코스트 값을 저장하는 유일한 속성입니다. 
    public int CardCost { get; private set; }

    // UI 필드는 나중에 UI 담당자가 추가할 곳

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
            CardCost = 0;
            return;
        }

        // 1. DataManager에서 기본 코스트를 가져옵니다.
        if (DataManager.Instance.TryGetCardCost(CardID, out int baseCost))
        {
            CardCost = baseCost;

            // 2. GameManager에서 수정치 확인 및 합산
            if (GameManager.Instance != null && GameManager.Instance.HandCostModifiers.ContainsKey(CardID))
            {
                int modifier = GameManager.Instance.HandCostModifiers[CardID];
                CardCost += modifier; // 기본 코스트에 수정치 합산

                // 코스트가 음수가 되는 것을 방지 (최소 0)
                CardCost = Mathf.Max(0, CardCost);
            }

            Debug.Log($"[CardDisplay] {CardID} 데이터 로딩 성공. 최종 코스트: {CardCost} (기본: {baseCost})");
        }
        else
        {
            CardCost = 0;
            Debug.LogError($"[CardDisplay] {CardID} 코스트 로딩 실패. 기본값 0 할당.");
        }
    }

    // OnMouseDown() 함수를 대체하는 표준 UI 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (HandManager.Instance == null || GameManager.Instance == null)
            return;

        // ------------------ 타겟팅 모드 처리 ------------------
        // 🚨 [수정 적용] WaitingForCardTarget을 사용하여 오류 해결 🚨
        if (GameManager.Instance.CurrentState == GameManager.GameState.WaitingForCardTarget)
        {
            // 타겟팅 모드라면, 이 카드를 목표물로 지정하고 효과를 실행합니다.
            GameManager.Instance.ResolveTargeting(CardID);

            return;
        }
        // ------------------ 일반 카드 사용 처리 ------------------
        else if (GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase)
        {
            // 일반 카드 사용을 HandManager에게 요청합니다.
            HandManager.Instance.TryUseCard(CardID);
            Debug.Log($"[Input] {CardID} 카드 사용 요청 (PointerClick).");
        }
    }
}