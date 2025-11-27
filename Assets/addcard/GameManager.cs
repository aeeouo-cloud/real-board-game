// GameManager.cs
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Unit PlayerUnit;
    public GameObject TrapPrefab;

    // 턴/상태 관리 변수
    public enum GameState { Setup, PlayerTurn_BaseActions, PlayerTurn_ActionPhase, WaitingForCardTarget, WaitingForTileTarget, EnemyTurn, GameEnd }
    public GameState CurrentState = GameState.Setup;

    // 턴 카운트 및 코스트 상한 변수
    public int TurnCount { get; private set; } = 0;
    private const int MAX_COST_CAP = 10;

    // 코스트 관리 변수
    public int CurrentCost { get; private set; }

    // 🚨 [핵심 수정] CurrentCost에 값을 할당하는 유일한 함수 (중복 제거) 🚨
    public void SetCurrentCost(int newCost)
    {
        CurrentCost = newCost;
    }

    public int InitialDrawAmount = 5;
    public int TurnDrawAmount = 1;
    public int MoveCostPerTile = 1;

    // 커스텀 주사위 면 정의
    private readonly int[] CustomDiceFaces = new int[] { 1, 1, 1, 2, 2, 3 };

    // 덱/핸드 변수
    public List<string> PlayerDeck = new List<string>();
    public List<string> PlayerHand = new List<string>();
    public List<string> PlayerDiscard = new List<string>();
    public List<string> EnemyHand = new List<string>();
    public List<string> EnemyDeck = new List<string>();


    // 손패 코스트 수정량 저장소
    public Dictionary<string, int> HandCostModifiers = new Dictionary<string, int>();

    // 타겟팅 상태 저장 변수
    public string TargetingCardID { get; private set; }


    // UI Feedback 필드
    [Header("UI Feedback")]
    public GameObject CostWarningPanel;

    // 이동 테스트 변수 및 함수
    public int TestMoveDistance = 2;

    [ContextMenu("Test_TryMovePlayer")]
    public void TestTryMovePlayerExecution()
    {
        if (CurrentState != GameState.PlayerTurn_ActionPhase)
        {
            Debug.LogWarning("[Move Test] 코스트 소모 이동 실패: 액션 페이즈가 아닙니다. 주사위 결과를 먼저 적용해 주세요.");
            return;
        }
        Debug.Log($"[Move Test] {TestMoveDistance}칸 이동 시도. (필요 코스트: {TestMoveDistance})");
        TryMovePlayer(TestMoveDistance);
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        if (CurrentState == GameState.Setup)
        {
            StartGame();
        }
    }

    // --- 초기화 및 셔플 로직 ---

    private void InitializeDeckForTest()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager 인스턴스를 찾을 수 없습니다. 덱 초기화 실패.");
            return;
        }

        PlayerDeck.Clear();
        if (DataManager.Instance.CardTable.Count == 0)
        {
            Debug.LogWarning("DataManager의 CardTable에 로드된 카드가 없습니다. CSV 파일을 확인해주세요.");
            return;
        }

        foreach (var pair in DataManager.Instance.CardTable)
        {
            PlayerDeck.Add(pair.Key);
        }
        ShuffleDeck(PlayerDeck);
        Debug.Log($"[Deck Initialized] {PlayerDeck.Count}장의 카드로 덱 초기화 완료.");
    }

    private void ShuffleDeck(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // ---------------------- 턴 관리 로직 ----------------------

    public void StartGame()
    {
        Debug.Log("--- 게임 시작: 초기 핸드 드로우 ---");
        InitializeDeckForTest();
        ProcessDraw(InitialDrawAmount);

        // Player Unit 배치 로직
        PlacePlayerUnitOnMap();

        CurrentState = GameState.PlayerTurn_BaseActions;
        StartPlayerTurn();
    }

    // Player Unit 배치 로직
    private void PlacePlayerUnitOnMap()
    {
        Hex[] hexTiles = FindObjectsByType<Hex>(FindObjectsSortMode.None);

        if (hexTiles.Length > 0 && PlayerUnit != null)
        {
            Vector3 startPosition = hexTiles[0].transform.position;
            // Y축을 1.5f 높여 타일 위에 배치
            PlayerUnit.transform.position = startPosition + new Vector3(0f, 1.5f, 0f);

            Debug.Log($"[Unit Placement] PlayerUnit이 시작 타일 ({startPosition})에 배치되었습니다.");
        }
        else
        {
            Debug.LogError("[Unit Placement] 맵 타일 또는 PlayerUnit이 씬에 없습니다. 배치 실패.");
        }
    }

    public void StartPlayerTurn()
    {
        if (CurrentState == GameState.GameEnd) return;

        if (CurrentState == GameState.EnemyTurn || CurrentState == GameState.GameEnd) return;

        Debug.Log("--- 플레이어 턴 시작 ---");
        CurrentState = GameState.PlayerTurn_BaseActions;

        // 턴 카운트 증가 및 n코스트 회복
        TurnCount++;
        int recoveredCost = Mathf.Min(TurnCount, MAX_COST_CAP);
        SetCurrentCost(recoveredCost);
        Debug.Log($"기본 코스트 획득 (턴 {TurnCount}): {CurrentCost} 코스트.");

        ProcessDraw(TurnDrawAmount);

        Debug.Log($"[Turn Flow] 기본 동작 완료. 주사위 굴림 시각화 대기 중... 현재 코스트: {CurrentCost}");
    }

    public void ApplyDiceResult(int result)
    {
        if (CurrentState != GameState.PlayerTurn_BaseActions)
        {
            Debug.LogWarning("주사위 결과 적용 실패: 턴 흐름이 올바르지 않습니다.");
            return;
        }

        SetCurrentCost(CurrentCost + result);
        Debug.Log($"[Dice] 주사위 {result} 추가 코스트 적용 완료. 총 사용 가능 코스트: {CurrentCost}");

        CurrentState = GameState.PlayerTurn_ActionPhase;
        Debug.Log("--- 액션 페이즈 시작: 카드 사용 및 이동 가능 ---");
    }

    public void EndPlayerTurn()
    {
        if (CurrentState == GameState.GameEnd) return;
        if (CurrentState != GameState.PlayerTurn_ActionPhase)
        {
            Debug.LogWarning("액션 페이즈가 아닙니다. 턴 종료를 강제합니다.");
        }

        Debug.Log("--- 플레이어 턴 종료 ---");

        StatusEffectManager.Instance?.UpdateEffectsOnTurnEnd();

        SetCurrentCost(0);

        CurrentState = GameState.EnemyTurn;
        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        if (CurrentState == GameState.GameEnd) return;

        Debug.Log("--- 적(Enemy) 턴 시작 ---");
        CurrentState = GameState.EnemyTurn;
        Invoke("EndEnemyTurn", 3f);
    }

    private void EndEnemyTurn()
    {
        if (CurrentState == GameState.GameEnd) return;

        Debug.Log("--- 적(Enemy) 턴 종료 ---");

        StatusEffectManager.Instance?.UpdateEffectsOnTurnEnd();

        CurrentState = GameState.PlayerTurn_BaseActions;
        StartPlayerTurn();
    }

    // ---------------------- 타겟팅 로직 ----------------------

    public void EnterTargetingMode(string sourceCardID)
    {
        if (CurrentState != GameState.PlayerTurn_ActionPhase) return;

        TargetingCardID = sourceCardID;
        CurrentState = GameState.WaitingForCardTarget; // 카드 타겟팅 모드

        Debug.Log($"[Targeting] 모드 진입. 목표 카드 선택 대기 중 (원천 카드: {sourceCardID})");
    }

    public void ResolveTargeting(string targetCardID)
    {
        if (CurrentState != GameState.WaitingForCardTarget) return;

        CardEffectResolver.Instance.ExecuteTargetedEffect(TargetingCardID, targetCardID);

        PlayerHand.Remove(TargetingCardID);

        CurrentState = GameState.PlayerTurn_ActionPhase;
        TargetingCardID = null;

        Debug.Log($"[Targeting] 완료. {targetCardID}에 효과 적용 후 액션 페이즈 복귀.");
    }

    public void CancelTargeting()
    {
        if (CurrentState == GameState.WaitingForCardTarget)
        {
            CurrentState = GameState.PlayerTurn_ActionPhase;
            TargetingCardID = null;
            Debug.Log("[Targeting] 취소됨. 액션 페이즈 복귀.");
        }
    }


    // ---------------------- 코스트 및 액션 로직 ----------------------

    public int RollCustomDice()
    {
        int roll = CustomDiceFaces[Random.Range(0, CustomDiceFaces.Length)];
        return roll;
    }

    // 순수 코스트 체크 함수
    public bool TryUseCost(int cost)
    {
        if (CurrentState != GameState.PlayerTurn_ActionPhase)
        {
            Debug.LogWarning("액션 페이즈에만 코스트를 사용할 수 있습니다.");
            return false;
        }

        if (CurrentCost >= cost)
        {
            Debug.Log($"코스트 사용 체크: 필요 {cost}, 현재 {CurrentCost}");
            return true;
        }

        Debug.LogWarning($"코스트 부족! 필요 코스트: {cost}, 현재 코스트: {CurrentCost}");
        return false;
    }

    // 코스트를 실제로 소모하는 함수
    public void ConsumeCost(int cost)
    {
        CurrentCost -= cost;
        Debug.Log($"코스트 소모 완료: -{cost}. 남은 코스트: {CurrentCost}");
    }

    // 카드 ID의 코스트에 수정량을 반영하는 함수
    public void ApplyHandCostModifier(string cardID, int amount)
    {
        if (HandCostModifiers.ContainsKey(cardID))
        {
            HandCostModifiers[cardID] += amount;
            Debug.Log($"[Modifier] {cardID} 코스트 수정: {amount}. 현재 수정량: {HandCostModifiers[cardID]}");
        }
        else
        {
            HandCostModifiers.Add(cardID, amount);
            Debug.Log($"[Modifier] {cardID} 코스트 수정치 초기 적용: {amount}");
        }
    }

    // 손패 전체 코스트에 수정량을 반영하는 함수
    public void ApplyAllHandCostModifier(int amount)
    {
        foreach (string cardID in PlayerHand)
        {
            if (HandCostModifiers.ContainsKey(cardID))
            {
                HandCostModifiers[cardID] += amount;
            }
            else
            {
                HandCostModifiers.Add(cardID, amount);
            }
        }
        Debug.Log($"[Modifier] 손패 전체 코스트 {amount} 일괄 수정 적용됨.");
    }


    public bool TryMovePlayer(int distance)
    {
        if (CurrentState != GameState.PlayerTurn_ActionPhase)
        {
            Debug.LogWarning("이동 실패: 현재는 액션 페이즈가 아닙니다.");
            return false;
        }

        int requiredCost = distance * MoveCostPerTile;

        if (TryUseCost(requiredCost))
        {
            ConsumeCost(requiredCost); // 코스트 차감

            // MapMovementHelper를 통해 유닛 이동 실행
            if (MapMovementHelper.TryMovePlayerUnit(PlayerUnit, distance))
            {
                Debug.Log($"[Action] {distance}칸 이동 성공. 코스트 {requiredCost} 소모.");
                return true;
            }
            else
            {
                Debug.LogError("PlayerUnit이 연결되지 않았거나 이동에 실패했습니다.");
                SetCurrentCost(CurrentCost + requiredCost); // 코스트 되돌리기
                return false;
            }
        }
        return false;
    }

    // ---------------------- 카드 및 덱 조작 로직 ----------------------

    public void InsertCardIntoDeck(string cardID, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            PlayerDeck.Add(cardID);
        }
        ShuffleDeck(PlayerDeck);
        Debug.Log($"[Deck] {cardID} 카드 {amount}장을 덱에 추가했습니다.");
    }

    public void InsertCardIntoHand(string cardID, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            PlayerHand.Add(cardID);
        }
        Debug.Log($"[Hand] {cardID} 카드 {amount}장을 패에 추가했습니다.");
    }

    public void DiscardEnemyHand(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (EnemyHand.Count > 0)
            {
                int randomIndex = Random.Range(0, EnemyHand.Count);
                string discardedID = EnemyHand[randomIndex];
                EnemyHand.RemoveAt(randomIndex);
                Debug.Log($"[Deck] 상대방 패에서 {discardedID}를 버렸습니다.");
            }
        }
        Debug.Log($"[Deck] 상대방 패에서 카드 {amount}장 버리기 시뮬레이션 완료.");
    }

    // ---------------------- 기존 카드 효과 로직 ----------------------

    public void ApplyAttack(Unit source, Unit target, int damage, int range)
    {
        if (target != null) target.TakeDamage(source, damage);
    }

    public void ProcessDraw(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (PlayerDeck.Count > 0)
            {
                string cardID = PlayerDeck[0];
                PlayerDeck.RemoveAt(0);
                PlayerHand.Add(cardID);
                Debug.Log($"[Draw System] {cardID} 카드 드로우. 남은 덱: {PlayerDeck.Count}, 현재 손패: {PlayerHand.Count}");
            }
            else
            {
                Debug.LogWarning("[Draw System] 덱이 비어 카드를 더 드로우할 수 없습니다. (피로도 로직 구현 필요)");
                break;
            }
        }
    }

    public void ProcessHeal(Unit target, int amount)
    {
        if (target != null) target.Heal(amount);
    }

    // ProcessMove 로직을 MapMovementHelper로 위임
    public void ProcessMove(Unit target, int distance)
    {
        if (target != null)
        {
            if (MapMovementHelper.TryMovePlayerUnit(target, distance))
            {
                Debug.Log($"[Card Effect] 카드 효과로 {distance}칸 이동 명령 실행.");
            }
        }
    }

    public void PlaceTrap(int range, int slowAmount)
    {
        if (PlayerUnit == null)
        {
            Debug.LogError("트랩 설치 실패: PlayerUnit이 연결되지 않았습니다.");
            return;
        }
        if (TrapPrefab == null)
        {
            Debug.LogError("트랩 설치 실패: TrapPrefab이 연결되지 않았습니다.");
            return;
        }

        Vector3 installPosition = PlayerUnit.transform.position;
        GameObject newTrap = Instantiate(TrapPrefab, installPosition, Quaternion.identity);

        Debug.Log($"[Game Logic] 트랩 설치 성공: 위치 {installPosition}, 감속 {slowAmount} 적용. 오브젝트 생성 완료.");
    }

    // ---------------------- UI 피드백 로직 ----------------------

    public void ShowWarning(string message)
    {
        if (CostWarningPanel == null)
        {
            Debug.LogError("CostWarningPanel이 GameManager에 연결되지 않았습니다.");
            return;
        }
        CancelInvoke("HideWarning");

        TextMeshProUGUI tmp = CostWarningPanel.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            Debug.LogError("CostWarningPanel 오브젝트에 TextMeshProUGUI 컴포넌트가 없습니다. 자식 오브젝트에 있다면 코드를 수정해야 합니다.");
            return;
        }

        tmp.text = message;
        CostWarningPanel.SetActive(true);

        Invoke("HideWarning", 2.0f);
    }

    private void HideWarning()
    {
        if (CostWarningPanel != null)
        {
            CostWarningPanel.SetActive(false);
        }
    }

    // ---------------------- 승패 판정 로직 ----------------------

    public void HandleUnitDeath(Unit deadUnit)
    {
        if (CurrentState == GameState.GameEnd) return;

        if (deadUnit.Type == Unit.UnitType.Player)
        {
            EndGame(false); // Player 사망: 패배
        }
        else if (deadUnit.Type == Unit.UnitType.Enemy)
        {
            EndGame(true); // Enemy 사망: 승리
        }
    }

    private void EndGame(bool isWin)
    {
        if (CurrentState == GameState.GameEnd) return;

        CurrentState = GameState.GameEnd;

        string result = isWin ? "승리" : "패배";
        Debug.Log($"--- 게임 종료: {result} ---");

        ShowWarning($"게임 종료! 당신은 {result}했습니다!");
    }
}