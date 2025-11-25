// GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;

public class GameManager : MonoBehaviour
{
    public static event Action PlayerTurnStarted;   //턴시작 알리는 이벤트 추가하였습니다
    public static GameManager Instance;
    public Unit PlayerUnit;
    public GameObject TrapPrefab;

    // 턴/상태 관리 변수
    public enum GameState { Setup, PlayerTurn_BaseActions, PlayerTurn_ActionPhase, EnemyTurn, GameEnd }
    public static GameState CurrentState = GameState.Setup; // 다른 스크립트에서 참조하기 쉽게 static으로 변경했습니다.

    // 🚨 턴 카운트 및 코스트 상한 변수 추가 🚨
    public int TurnCount { get; private set; } = 0; // 현재 턴 수 (0에서 시작)
    private const int MAX_COST_CAP = 10; // 코스트의 최대 상한선

    // 코스트 관리 변수
    public int CurrentCost { get; set; }
    // private int BaseCostPerTurn = 10; // 이 변수는 이제 사용하지 않음
    public int InitialDrawAmount = 5;
    public int TurnDrawAmount = 1;
    public int MoveCostPerTile = 1; // 1칸 이동당 1 코스트

    // 커스텀 주사위 면 정의 (1, 1, 1, 2, 2, 3)
    private readonly int[] CustomDiceFaces = new int[] { 1, 1, 1, 2, 2, 3 };

    // 덱/핸드 변수
    public List<string> PlayerDeck = new List<string>();
    public List<string> PlayerHand = new List<string>();

    // 🚨 [새로 추가된 부분] 이동 테스트 변수 및 함수 🚨
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        if (DataManager.Instance != null && DataManager.Instance.CardTable.Count > 0 && PlayerDeck.Count == 0)
        {
            InitializeDeckForTest();
            Debug.Log($"[Draw System] 초기 덱 설정 완료. 총 {PlayerDeck.Count}장.");
        }

        if (CurrentState == GameState.Setup)
        {
            StartGame();
        }
    }

    private void InitializeDeckForTest()
    {
        foreach (var pair in DataManager.Instance.CardTable)
        {
            PlayerDeck.Add(pair.Key);
          
        }
        Deck.instance.idlist = PlayerDeck;
    }

    // ---------------------- 턴 관리 로직 ----------------------

    public void StartGame()
    {
        Debug.Log("--- 게임 시작: 초기 핸드 드로우 ---");

        ProcessDraw(InitialDrawAmount);

        CurrentState = GameState.PlayerTurn_BaseActions;
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        if (CurrentState == GameState.EnemyTurn || CurrentState == GameState.GameEnd) return;

        Debug.Log("--- 플레이어 턴 시작 ---");
        CurrentState = GameState.PlayerTurn_BaseActions;

        // 1. 턴 카운트 증가 및 n코스트 회복
        TurnCount++;
        int recoveredCost = Mathf.Min(TurnCount, MAX_COST_CAP);
        CurrentCost = recoveredCost;
        Debug.Log($"기본 코스트 획득 (턴 {TurnCount}): {CurrentCost} 코스트.");

        // 2. 카드 1장 드로우
        ProcessDraw(TurnDrawAmount);

        PlayerTurnStarted.Invoke();

        // 3. 주사위 굴림 시각화 대기
        Debug.Log($"[Turn Flow] 기본 동작 완료. 주사위 굴림 시각화 대기 중... 현재 코스트: {CurrentCost}");
    }

    public void ApplyDiceResult(int result)
    {
        if (CurrentState != GameState.PlayerTurn_BaseActions)
        {
            Debug.LogWarning("주사위 결과 적용 실패: 턴 흐름이 올바르지 않습니다.");
            return;
        }

        CurrentCost += result;
        Debug.Log($"[Dice] 주사위 {result} 추가 코스트 적용 완료. 총 사용 가능 코스트: {CurrentCost}");

        CurrentState = GameState.PlayerTurn_ActionPhase;
        Debug.Log("--- 액션 페이즈 시작: 카드 사용 및 이동 가능 ---");
    }

    public void EndPlayerTurn()
    {
        if (CurrentState != GameState.PlayerTurn_ActionPhase)
        {
            Debug.LogWarning("액션 페이즈가 아닙니다. 턴 종료를 강제합니다.");
        }

        Debug.Log("--- 플레이어 턴 종료 ---");

        CurrentCost = 0;

        // 턴 종료 효과 처리 (TODO)

        CurrentState = GameState.EnemyTurn;
        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        Debug.Log("--- 적(Enemy) 턴 시작 ---");

        // 시뮬레이션: 3초 후 적 턴 종료
        Invoke("EndEnemyTurn", 3f);
    }

    private void EndEnemyTurn()
    {
        Debug.Log("--- 적(Enemy) 턴 종료 ---");

        CurrentState = GameState.PlayerTurn_BaseActions;
        StartPlayerTurn(); // 다시 플레이어 턴 시작
    }

    // ---------------------- 코스트 및 액션 로직 ----------------------

    public int RollCustomDice()
    {
        int roll = CustomDiceFaces[Random.Range(0, CustomDiceFaces.Length)];
        return roll;
    }

    public bool TryUseCost(int cost)
    {
        if (CurrentState != GameState.PlayerTurn_ActionPhase)
        {
            Debug.LogWarning("액션 페이즈에만 코스트를 사용할 수 있습니다.");
            return false;
        }

        if (CurrentCost >= cost)
        {
            CurrentCost -= cost;
            Debug.Log($"코스트 사용: -{cost}. 남은 코스트: {CurrentCost}");
            return true;
        }

        Debug.LogWarning($"코스트 부족! 필요 코스트: {cost}, 현재 코스트: {CurrentCost}");
        return false;
    }
    public void AddCost(int cost)
    {
        CurrentCost += cost;
    }

    // ---------------------- 기존 카드 효과 로직 (변경 없음) ----------------------

    public void ApplyAttack(Unit target, int damage, int range)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }

    public void ProcessDraw(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Deck.instance.DrawCard();
        }
    }

    public void ProcessHeal(Unit target, int amount)
    {
        if (target != null)
        {
            target.Heal(amount);
        }
    }

    public void ProcessMove(Unit target, int distance)
    {
        if (target != null)
        {
            target.Move(distance);
            Debug.Log($"[Card Effect] 카드 효과로 {distance}만큼 이동.");
        }
    }

    public void PlaceTrap(int range, int slowAmount)
    {
        Vector3 installPosition = PlayerUnit.transform.position;
        GameObject newTrap = Instantiate(TrapPrefab, installPosition, Quaternion.identity);

        Debug.Log($"[Game Logic] 트랩 설치 성공: 위치 {installPosition}, 감속 {slowAmount} 적용. 오브젝트 생성 완료.");
    }
}