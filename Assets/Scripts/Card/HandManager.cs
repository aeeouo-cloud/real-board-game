using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HandManager : MonoBehaviour
{
    // 정적 인스턴스 (다른 스크립트에서 쉽게 접근 가능)
    public static HandManager Instance { get; private set; }

    // --- 설정 필드 (Inspector에서 연결) ---
    [Header("Dependencies")]
    public GameManager GameManager;       // GameManager 인스턴스 (데이터 접근)
    public GameObject CardUIPrefab;       // 실제 카드 UI 프리팹 (CardDisplay 스크립트가 붙어 있어야 함)
    public Transform HandContainer;        // 카드를 배치할 부모 트랜스폼 (손패의 중심 위치)

    [Header("Layout Settings")]
    public float CardSpacing = 0.5f;        // 카드 사이의 간격
    public float MaxWidth = 10f;             // 손패 영역의 최대 너비
    public float FanAngle = 5f;             // 카드를 부채꼴로 배열할 각도 (0이면 직선)

    // --- 내부 상태 ---
    // Key: 카드 ID (string), Value: 생성된 카드 UI 오브젝트
    private Dictionary<string, GameObject> activeCardObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // GameManager 인스턴스 자동 연결
        if (GameManager == null)
        {
            GameManager = GameManager.Instance;
        }
        if (GameManager == null)
        {
            Debug.LogError("HandManager: GameManager 인스턴스를 찾을 수 없습니다. 스크립트를 비활성화합니다.");
            enabled = false;
        }
    }

    // Update()에서 프레임마다 동기화 및 레이아웃을 확인하여 부드러운 움직임을 만듭니다.
    void Update()
    {
        if (GameManager == null || GameManager.PlayerHand == null) return;

        // 데이터와 화면 UI의 개수가 다르면 동기화 함수 호출
        if (activeCardObjects.Count != GameManager.PlayerHand.Count)
        {
            SynchronizeHandVisuals();
        }

        UpdateHandLayout();
    }

    // 1. 데이터 리스트와 화면 UI를 동기화합니다. (카드 생성 및 제거)
    private void SynchronizeHandVisuals()
    {
        // 1. 사라진 카드 제거 (손패 데이터에는 없고 화면에만 있는 카드)
        var cardsToRemove = activeCardObjects.Keys
            .Where(id => !GameManager.PlayerHand.Contains(id))
            .ToList();

        foreach (var id in cardsToRemove)
        {
            Destroy(activeCardObjects[id]);
            activeCardObjects.Remove(id);
            Debug.Log($"[HandManager] 카드 UI 제거: {id}");
        }

        // 2. 새로 추가된 카드 생성 (손패 데이터에는 있고 화면에는 없는 카드)
        var cardsToInstantiate = GameManager.PlayerHand
            .Where(id => !activeCardObjects.ContainsKey(id))
            .ToList();

        foreach (var id in cardsToInstantiate)
        {
            if (CardUIPrefab != null)
            {
                // 프리팹을 HandContainer 아래에 생성
                GameObject cardObj = Instantiate(CardUIPrefab, HandContainer);
                cardObj.name = "Card_" + id;

                // CardDisplay 스크립트를 찾아 ID 전달 및 초기화
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display != null)
                {
                    // PlayerHand는 CardID의 리스트이므로, 해당 ID를 사용합니다.
                    display.Initialize(id);
                }
                else
                {
                    Debug.LogError($"CardUIPrefab에 CardDisplay 스크립트가 없습니다: {id}");
                }

                activeCardObjects.Add(id, cardObj);
                Debug.Log($"[HandManager] 새 카드 UI 생성 및 초기화: {id}");
            }
        }
    }

    // 2. 손패의 카드를 부채꼴/직선 형태로 배치합니다.
    private void UpdateHandLayout()
    {
        int cardCount = activeCardObjects.Count;
        if (cardCount == 0) return;

        float totalWidth = Mathf.Min(MaxWidth, cardCount * CardSpacing);
        float startX = -totalWidth / 2f + CardSpacing / 2f;

        List<string> currentHandIDs = GameManager.PlayerHand;

        for (int i = 0; i < cardCount; i++)
        {
            string cardID = currentHandIDs[i];

            // 🚨 [핵심 수정] Dictionary에서 TryGetValue로 안전하게 오브젝트를 가져와 키 오류 방지 🚨
            if (!activeCardObjects.TryGetValue(cardID, out GameObject cardObj))
            {
                // 이 카드는 아직 SynchronizeHandVisuals()에 의해 생성 중이므로, 이 프레임은 건너뜁니다.
                continue;
            }

            float xPos = startX + i * CardSpacing;

            float t = (cardCount > 1) ? (float)i / (cardCount - 1) : 0.5f;
            float rotation = (t - 0.5f) * FanAngle;
            float yPos = -Mathf.Abs(rotation) * 0.05f;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);
            Quaternion targetRot = Quaternion.Euler(0, 0, rotation);

            // 부드러운 이동 (Lerp 사용)
            cardObj.transform.localPosition = Vector3.Lerp(cardObj.transform.localPosition, targetPos, Time.deltaTime * 10f);
            cardObj.transform.localRotation = Quaternion.Lerp(cardObj.transform.localRotation, targetRot, Time.deltaTime * 10f);
        }
    }

    // 3. 카드 사용 요청 (CardDisplay가 클릭 시 호출할 함수)
    public void TryUseCard(string cardID)
    {
        if (!activeCardObjects.ContainsKey(cardID))
        {
            Debug.LogWarning($"[Use] 손패에 없는 카드 ID 사용 요청: {cardID}");
            return;
        }

        GameObject cardObject = activeCardObjects[cardID];
        CardDisplay display = cardObject.GetComponent<CardDisplay>();

        if (display == null)
        {
            Debug.LogError($"[Use] 카드 사용 실패: {cardID}에서 CardDisplay 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 1. 코스트 조회 (CardDisplay는 GameManager의 GetFinalCost를 참조하여 코스트를 가져옴)
        int actualCost = display.CardCost;

        // 2. 턴 상태 체크
        if (GameManager.Instance.CurrentState != GameManager.GameState.PlayerTurn_ActionPhase)
        {
            GameManager.Instance.ShowWarning("카드는 액션 페이즈에만 사용할 수 있습니다.");
            return;
        }

        // 🚨 3. 코스트 체크 (차감하지 않고 순수하게 체크만)
        if (GameManager.Instance.TryUseCost(actualCost))
        {
            // 🚨 4. 사용 전 최종 유효성 검사 (사거리 내 타겟 유무 체크) 🚨
            if (CardEffectResolver.Instance.NeedsTargetValidation(cardID))
            {
                if (!CardEffectResolver.Instance.IsActionValid(cardID))
                {
                    GameManager.Instance.ShowWarning("사용 불가: 유효한 타겟이 사거리 내에 없습니다!");
                    return; // 코스트 소모 및 효과 실행을 막습니다.
                }
            }

            // 5. 코스트 체크 성공 -> 실제로 코스트 차감
            GameManager.Instance.ConsumeCost(actualCost);

            // 6. 효과 실행
            CardEffectResolver.Instance.ExecuteCardEffect(cardID);

            // 7. PlayerHand 리스트에서 해당 카드 ID 제거 (UI 제거 동기화)
            GameManager.Instance.PlayerHand.Remove(cardID);

            Debug.Log($"[Use] 카드 사용 성공: {cardID} (Cost: {actualCost})");
        }
        else
        {
            // 8. 코스트 부족 실패 -> 경고 메시지 출력
            GameManager.Instance.ShowWarning("코스트가 모자랍니다!");
        }
    }
}
