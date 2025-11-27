using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

// 네임스페이스 충돌을 방지하기 위해 필요한 경우,
// 아래에 팀원의 Unit, Map, Hex 클래스가 정의된 네임스페이스를 추가합니다.
// 예시: using ProjectName.Core; 

// StatusID는 이제 팀원의 파일에서 찾도록 합니다.

public class CardEffectResolver : MonoBehaviour
{
    public static CardEffectResolver Instance;

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

    // Inspector 필드
    public Unit EnemyTarget;
    public string TestCardID = "N002";

    // 효과의 주 대상 (현재 턴 플레이어)
    private Unit PlayerSource => GameManager.Instance.PlayerUnit;

    // -------------------------------------------------------------
    // 🚨 헬퍼 함수 정의 (public 접근자로 수정) 🚨
    // -------------------------------------------------------------

    // Helper 함수: 파라미터 딕셔너리에서 키를 찾고, 찾지 못하거나 형식이 틀리면 0을 반환
    private int GetIntParam(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out string valueStr) && int.TryParse(valueStr, out int value))
        {
            return value;
        }
        return 0;
    }

    // 🚨 [추가] 임시 유닛 검색 도우미 함수 (팀원 코드를 건드리지 않기 위해) 🚨
    private Unit FindUnitAt(Vector2Int coord)
    {
        // 씬의 모든 Unit 컴포넌트를 찾습니다.
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit u in units)
        {
            if (u.CurrentPosition == coord)
            {
                return u;
            }
        }
        return null;
    }

    // -------------------------------------------------------------
    // 🚨 외부 호출용 유효성 검사 로직 (public으로 선언) 🚨
    // -------------------------------------------------------------

    // 🚨 [추가] 타겟팅이 필요한 카드인지 외부에 알리는 함수 🚨
    public bool NeedsTargetValidation(string cardID)
    {
        if (!DataManager.Instance.CardTable.TryGetValue(cardID, out CardData cardData)) return false;
        DataManager.Instance.EffectSequenceTable.TryGetValue(cardData.EffectGroup_ID, out List<CardEffectSequenceData> sequenceList);
        if (sequenceList == null || sequenceList.Count == 0) return false;

        string firstEffectCode = sequenceList[0].EffectCode;

        // 타일 타겟팅이 필요한 모든 카드 (공격, 트랩)
        return firstEffectCode == "ATTACK_SINGLE" ||
               firstEffectCode == "MOVE_ATTACK" ||
               firstEffectCode == "PLACE_TRAP";
    }

    // 🚨 [추가] 카드 사용 전, 유효한 타겟이 있는지 확인하는 함수 🚨
    public bool IsActionValid(string cardID)
    {
        if (!DataManager.Instance.CardTable.TryGetValue(cardID, out CardData cardData)) return false;
        DataManager.Instance.EffectSequenceTable.TryGetValue(cardData.EffectGroup_ID, out List<CardEffectSequenceData> sequenceList);
        if (sequenceList == null || sequenceList.Count == 0) return true; // 효과가 없으면 유효하다고 가정

        CardEffectSequenceData step = sequenceList[0];
        string effectCode = step.EffectCode;

        if (effectCode == "ATTACK_SINGLE" || effectCode == "MOVE_ATTACK")
        {
            // 공격 카드인 경우: 사거리 내에 적 유닛이 있는지 확인합니다.
            DataManager.Instance.ParameterDetailTable.TryGetValue(step.EffectStep_PK, out List<CardParameterDetailsData> parameters);
            Dictionary<string, string> paramDict = parameters?.ToDictionary(p => p.ParameterKey, p => p.ParameterValue) ?? new Dictionary<string, string>();

            int range = GetIntParam(paramDict, "MAX_RANGE");

            if (Map.instance == null || GameManager.Instance.PlayerUnit == null) return false;

            // 1. 공격 범위 내 모든 타일을 가져옵니다.
            List<Hex> reachableHexes = Map.instance.GetReachableHex(GameManager.Instance.PlayerUnit.CurrentPosition, range);

            // 2. 공격 범위 내에 Enemy 타입의 유닛이 하나라도 있으면 true
            Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None); // 씬의 모든 유닛을 찾습니다.

            return allUnits.Any(unit =>
                unit.Type == Unit.UnitType.Enemy &&
                reachableHexes.Any(h => h.qr == unit.CurrentPosition));
        }

        // PLACE_TRAP은 맵이 존재하면 항상 유효하다고 가정
        return true;
    }

    // -------------------------------------------------------------
    // 🚨 핵심 로직 함수 정의 🚨
    // -------------------------------------------------------------


    [ContextMenu("Execute Test Card")]
    public void TestManualExecution()
    {
        if (!string.IsNullOrEmpty(TestCardID))
        {
            ExecuteCardEffect(TestCardID);
        }
        else
        {
            Debug.LogError("테스트 카드 ID를 입력해 주세요.");
        }
    }

    // EffectCode에 따라 실제 게임 로직을 실행하는 핵심 함수
    private void ExecuteEffectLogic(string effectCode, List<CardParameterDetailsData> parameters)
    {
        Dictionary<string, string> paramDict = parameters?.ToDictionary(p => p.ParameterKey, p => p.ParameterValue)
                                                            ?? new Dictionary<string, string>();

        if (GameManager.Instance == null || StatusEffectManager.Instance == null)
        {
            Debug.LogError("GameManager 또는 StatusEffectManager 인스턴스가 초기화되지 않았습니다.");
            return;
        }

        Unit target = GameManager.Instance.PlayerUnit;
        Unit source = GameManager.Instance.PlayerUnit;

        // TargetType에 따라 target을 EnemyTarget 또는 PlayerUnit으로 변경하는 로직
        if (paramDict.TryGetValue("TARGET_TYPE", out string targetType) && targetType == "ENEMY")
        {
            target = EnemyTarget;
        }

        // 공통 파라미터 획득
        int amount = GetIntParam(paramDict, "AMOUNT");

        // DISTANCE 키가 있다면, MOVE_SELF를 위해 그 값을 amount에 덮어씁니다.
        if (paramDict.ContainsKey("DISTANCE"))
        {
            amount = GetIntParam(paramDict, "DISTANCE");
        }

        int duration = GetIntParam(paramDict, "DURATION");

        // EffectCode에 따라 로직 분기 및 연결
        switch (effectCode)
        {
            // -------------------- 기본/공격/이동 (기존) --------------------
            case "ATTACK_SINGLE":
            case "ATTACK_CONDITIONAL":
            case "MOVE_ATTACK":
                // ATTACK_SINGLE/MOVE_ATTACK은 타겟팅 모드로 진입하므로 이 일반 실행 로직은 무시됩니다.
                Debug.LogWarning($"[ATTACK] {effectCode}가 일반 실행되었습니다. 타겟팅 모드로 실행되어야 합니다.");
                break;
            case "HEAL_HP":
                GameManager.Instance.ProcessHeal(target, amount);
                break;
            case "DRAW_CARD_SELF":
                GameManager.Instance.ProcessDraw(amount);
                break;
            case "MOVE_SELF":
                // 1. Target Unit에서 Move 컴포넌트를 가져옵니다.
                Move unitMove = target.GetComponent<Move>();

                if (unitMove != null)
                {
                    // 2. 카드 파라미터에서 얻은 거리(amount)를 Move.cs의 carddist에 설정합니다.
                    unitMove.carddist = amount;

                    // 3. 유닛을 'CardMove' 모드로 전환합니다. (맵에 하이라이트 표시 및 유저 클릭 대기)
                    unitMove.currentmode = Move.MoveMode.CardMove;

                    Debug.Log($"[MOVE_SELF] 유닛({target.name})을 거리 {amount}로 이동 가능한 모드로 전환합니다.");
                }
                else
                {
                    Debug.LogError($"[MOVE_SELF] 대상 유닛({target.name})에 Move 컴포넌트가 없습니다.");
                }
                break;
            case "PLACE_TRAP":
                // PLACE_TRAP은 타겟팅 모드로 진입하므로 이 일반 실행 로직은 무시됩니다.
                Debug.LogWarning($"[PLACE_TRAP] 일반 실행되었습니다. 타겟팅 모드로 실행되어야 합니다.");
                break;

            // -------------------- 상태/버프/디버프 로직 (APPLY_* 구현) --------------------
            case "APPLY_DAMAGE_RESIST":
            case "APPLY_DAMAGE_IMMUNE":
            case "APPLY_TARGET_IMMUNE":
            case "APPLY_DEBUFF":
                // StatusID가 enum으로 정의되어 있고, Enum.TryParse가 작동한다고 가정
                if (paramDict.TryGetValue("DEBUFF_ID", out string statusIdStr) && Enum.TryParse(statusIdStr, true, out StatusID statusID))
                {
                    StatusEffectManager.Instance.ApplyEffect(statusID, amount, duration, target);
                }
                break;
            case "REMOVE_STATUS":
                if (paramDict.TryGetValue("STATUS_ID", out string statusRemoveIdStr) && Enum.TryParse(statusRemoveIdStr, true, out StatusID statusRemoveID))
                {
                    StatusEffectManager.Instance.RemoveStatus(target, statusRemoveID);
                }
                break;

            // -------------------- 흐름 제어/특수 효과 (구현) --------------------
            case "REFUND_COST_IMMEDIATE":
                GameManager.Instance.RefundCost(amount);
                break;

            case "TAKE_EXTRA_TURN":
            case "CHECK_BRANCHING":
            case "MOVE_TO_OBJECT_RANGE":
            case "DISCARD_DECK_ENEMY":
            case "MODIFY_HAND_STAT":
            case "MODIFY_ENEMY_HAND_STAT":
                Debug.LogWarning($"[Flow/Board] {effectCode} 로직 구현 필요.");
                break;

            default:
                Debug.LogWarning($"알 수 없는 EffectCode: {effectCode}. 해당 로직 구현이 필요합니다.");
                break;
        }
    }


    // 카드 사용 시 호출되는 주 진입점 함수 (HandManager에서 호출)
    public void ExecuteCardEffect(string cardID)
    {
        if (DataManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("DataManager 또는 GameManager가 초기화되지 않았습니다.");
            return;
        }

        if (!DataManager.Instance.CardTable.TryGetValue(cardID, out CardData cardData))
        {
            Debug.LogError($"Card ID를 찾을 수 없음: {cardID}");
            return;
        }

        string effectGroupID = cardData.EffectGroup_ID;

        if (!DataManager.Instance.EffectSequenceTable.TryGetValue(effectGroupID, out List<CardEffectSequenceData> sequenceList))
        {
            Debug.LogError($"EffectGroup ID를 찾을 수 없음: {effectGroupID}");
            return;
        }

        // 🚨 [핵심] 타겟팅이 필요한 카드(REDUCE_COST_SINGLE, PLACE_TRAP, ATTACK_SINGLE)인지 확인 🚨
        if (sequenceList.Count > 0)
        {
            string firstEffectCode = sequenceList[0].EffectCode;

            if (firstEffectCode == "REDUCE_COST_SINGLE")
            {
                // 카드 타겟팅 (손패의 다른 카드를 타겟)
                GameManager.Instance.EnterTargetingMode(cardID);
                Debug.Log($"[Targeting Flow] {cardID}는 카드 타겟팅이 필요하여 모드로 진입합니다.");
                return;
            }
            else if (firstEffectCode == "ATTACK_SINGLE" || firstEffectCode == "MOVE_ATTACK" || firstEffectCode == "PLACE_TRAP")
            {
                // 타일 타겟팅에 필요한 파라미터(범위)를 가져옵니다.
                DataManager.Instance.ParameterDetailTable.TryGetValue(sequenceList[0].EffectStep_PK, out List<CardParameterDetailsData> parameters);
                Dictionary<string, string> paramDict = parameters?.ToDictionary(p => p.ParameterKey, p => p.ParameterValue)
                                                            ?? new Dictionary<string, string>();

                int range = GetIntParam(paramDict, "MAX_RANGE");

                // 🚨 [새로운 로직] 공격 카드인 경우, 타겟이 없으면 사용 불가 🚨
                if (firstEffectCode == "ATTACK_SINGLE" || firstEffectCode == "MOVE_ATTACK")
                {
                    if (Map.instance == null || GameManager.Instance.PlayerUnit == null)
                    {
                        GameManager.Instance.ShowWarning("공격 실패: 맵 또는 플레이어 유닛이 준비되지 않았습니다.");
                        return;
                    }

                    // 1. 공격 범위 내 모든 타일을 가져옵니다.
                    List<Hex> reachableHexes = Map.instance.GetReachableHex(GameManager.Instance.PlayerUnit.CurrentPosition, range);

                    // 2. 공격 범위 내에 적 유닛이 있는지 확인합니다.
                    bool enemyFound = false;
                    Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None); // 씬의 모든 유닛을 찾습니다.

                    foreach (Unit unit in allUnits)
                    {
                        if (unit.Type == Unit.UnitType.Enemy && reachableHexes.Any(h => h.qr == unit.CurrentPosition))
                        {
                            enemyFound = true;
                            break;
                        }
                    }

                    if (!enemyFound)
                    {
                        GameManager.Instance.ShowWarning($"공격 실패: 사거리({range}) 내에 타겟 가능한 적이 없습니다.");
                        return;
                    }
                }

                // 타일 타겟팅 모드로 진입합니다.
                GameManager.Instance.EnterTileTargetingMode(cardID, range);
                Debug.Log($"[Targeting Flow] {cardID}는 타일 타겟팅이 필요하여 모드로 진입합니다. (범위: {range})");
                return;
            }
        }
        // -------------------------------------------------------------

        Debug.Log($"--- {cardData.name} 카드의 일반 효과 실행 시작 (ID: {cardID}) ---");

        foreach (var step in sequenceList)
        {
            DataManager.Instance.ParameterDetailTable.TryGetValue(step.EffectStep_PK, out List<CardParameterDetailsData> parameters);
            ExecuteEffectLogic(step.EffectCode, parameters);
        }

        Debug.Log($"--- {cardData.name} 카드의 일반 효과 실행 완료 ---");
    }

    // 🚨 [핵심] 카드 타겟팅 완료 후 호출 (REDUCE_COST_SINGLE 처리) 🚨
    public void ExecuteTargetedEffect(string sourceCardID, string targetCardID)
    {
        if (DataManager.Instance == null || GameManager.Instance == null) return;

        if (!DataManager.Instance.CardTable.TryGetValue(sourceCardID, out CardData cardData)) return;

        DataManager.Instance.EffectSequenceTable.TryGetValue(cardData.EffectGroup_ID, out List<CardEffectSequenceData> sequenceList);
        if (sequenceList == null || sequenceList.Count == 0) return;

        Debug.Log($"--- 타겟팅 효과 실행 시작 (원천: {sourceCardID}, 대상: {targetCardID}) ---");

        CardEffectSequenceData step = sequenceList[0];
        string effectCode = step.EffectCode;

        DataManager.Instance.ParameterDetailTable.TryGetValue(step.EffectStep_PK, out List<CardParameterDetailsData> parameters);
        Dictionary<string, string> paramDict = parameters?.ToDictionary(p => p.ParameterKey, p => p.ParameterValue)
                                                            ?? new Dictionary<string, string>();

        if (effectCode == "REDUCE_COST_SINGLE")
        {
            int modifierAmount = GetIntParam(paramDict, "AMOUNT");
            GameManager.Instance.ApplyHandCostModifier(targetCardID, modifierAmount);

            Debug.Log($"[Targeted Effect] {sourceCardID}가 {targetCardID}의 코스트를 {modifierAmount}만큼 수정했습니다.");
        }
        else
        {
            Debug.LogWarning($"ExecuteTargetedEffect: 예상치 못한 EffectCode ({effectCode})가 타겟팅 효과로 실행되었습니다.");
        }

        Debug.Log($"--- 타겟팅 효과 실행 완료 ---");
    }

    // 🚨 [핵심 추가] 타일 타겟팅 완료 후 호출 (PLACE_TRAP, ATTACK_SINGLE 처리) 🚨
    public void ExecuteTileTargetedEffect(string cardID, Vector2Int targetPos)
    {
        if (DataManager.Instance == null || GameManager.Instance == null) return;

        if (!DataManager.Instance.CardTable.TryGetValue(cardID, out CardData cardData)) return;

        DataManager.Instance.EffectSequenceTable.TryGetValue(cardData.EffectGroup_ID, out List<CardEffectSequenceData> sequenceList);
        if (sequenceList == null || sequenceList.Count == 0) return;

        Debug.Log($"--- 타일 타겟팅 효과 실행 시작 (카드: {cardID}, 타일: {targetPos}) ---");

        // 첫 번째 스텝의 효과 코드와 파라미터를 가져옵니다.
        CardEffectSequenceData step = sequenceList[0];
        string effectCode = step.EffectCode;

        DataManager.Instance.ParameterDetailTable.TryGetValue(step.EffectStep_PK, out List<CardParameterDetailsData> parameters);
        Dictionary<string, string> paramDict = parameters?.ToDictionary(p => p.ParameterKey, p => p.ParameterValue)
                                                            ?? new Dictionary<string, string>();

        int damage = GetIntParam(paramDict, "DAMAGE_AMOUNT"); // 공격용
        int slowAmount = GetIntParam(paramDict, "SLOW_AMOUNT"); // 트랩용

        // 🚨 [핵심] 이 지점에서 공격과 트랩을 분기합니다. 🚨
        if (effectCode == "PLACE_TRAP")
        {
            // 트랩 설치 최종 로직
            GameManager.Instance.PlaceTrapAt(targetPos, slowAmount);
            Debug.Log($"[PLACE_TRAP] 트랩이 좌표 {targetPos}에 설치되었습니다. (Slow: {slowAmount})");
        }
        else if (effectCode == "ATTACK_SINGLE" || effectCode == "MOVE_ATTACK")
        {
            // 공격 최종 로직
            // 1. 타겟 타일에 있는 유닛을 찾습니다. (EnemyTarget을 타겟팅된 좌표에서 찾음)
            Unit targetUnit = FindUnitAt(targetPos);

            if (targetUnit != null)
            {
                // 2. 공격 실행 (사거리 검사는 이미 Map.instance.SelectReachable에서 완료됨)
                GameManager.Instance.ApplyAttack(GameManager.Instance.PlayerUnit, targetUnit, damage, GameManager.Instance.TileTargetingRange);
                Debug.Log($"[ATTACK_SINGLE] {targetUnit.name}에게 {damage} 피해를 입혔습니다.");
            }
            else
            {
                // 공격 카드인데 유닛이 없는 타일을 클릭하면 공격 실패 로그만 남깁니다.
                Debug.LogWarning($"[ATTACK_SINGLE] 타일 {targetPos}에 공격할 유닛이 없습니다. 공격 실패.");
            }
        }

        Debug.Log($"--- 타일 타겟팅 효과 실행 완료 ---");
    }
}
