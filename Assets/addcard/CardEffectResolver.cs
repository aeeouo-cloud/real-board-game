// CardEffectResolver.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

        // 타겟팅이 필요한 카드(REDUCE_COST_SINGLE)인지 확인 
        if (sequenceList.Count > 0 && sequenceList[0].EffectCode == "REDUCE_COST_SINGLE")
        {
            GameManager.Instance.EnterTargetingMode(cardID);
            Debug.Log($"[Targeting Flow] {cardID}는 타겟팅이 필요하여 모드로 진입합니다.");
            return; // 일반 효과 실행을 중단하고 타겟팅을 기다립니다.
        }

        Debug.Log($"--- {cardData.name} 카드의 일반 효과 실행 시작 (ID: {cardID}) ---");

        foreach (var step in sequenceList)
        {
            DataManager.Instance.ParameterDetailTable.TryGetValue(step.EffectStep_PK, out List<CardParameterDetailsData> parameters);
            ExecuteEffectLogic(step.EffectCode, parameters);
        }

        Debug.Log($"--- {cardData.name} 카드의 일반 효과 실행 완료 ---");
    }

    // 타겟팅이 완료된 후 호출되는 함수 (RCS 처리)
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

    // Helper 함수: 파라미터 딕셔너리에서 키를 찾고, 찾지 못하거나 형식이 틀리면 0을 반환
    private int GetIntParam(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out string valueStr) && int.TryParse(valueStr, out int value))
        {
            return value;
        }
        return 0;
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
        int duration = GetIntParam(paramDict, "DURATION");

        // EffectCode에 따라 로직 분기 및 연결
        switch (effectCode)
        {
            // -------------------- 기본/공격/이동 (기존) --------------------
            case "ATTACK_SINGLE":
            case "ATTACK_CONDITIONAL":
            case "MOVE_ATTACK": // 이동 후 공격
                int range = GetIntParam(paramDict, "MAX_RANGE");
                int damage = GetIntParam(paramDict, "DAMAGE_AMOUNT");

                Unit attackTarget = EnemyTarget; // 임시 대상 지정

                if (attackTarget != null && source != null)
                {
                    GameManager.Instance.ApplyAttack(source, attackTarget, damage, range);
                }
                else
                {
                    Debug.LogError("[Attack] 공격 대상 또는 공격자 유닛이 연결되지 않았습니다.");
                }
                break;
            case "HEAL_HP":
                GameManager.Instance.ProcessHeal(target, amount);
                break;
            case "DRAW_CARD_SELF":
                GameManager.Instance.ProcessDraw(amount);
                break;
            case "MOVE_SELF": // <<<<<<<<<<<<<<< 여기에 로직을 적용합니다. >>>>>>>>>>>>>>>
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
                int trapRange = GetIntParam(paramDict, "MAX_RANGE");
                GameManager.Instance.PlaceTrap(trapRange, amount); // Amount를 Slow/Damage로 사용
                break;

            // -------------------- 상태/버프/디버프 로직 (APPLY_* 구현) --------------------
            case "APPLY_DAMAGE_RESIST":
            case "APPLY_DAMAGE_IMMUNE":
            case "APPLY_TARGET_IMMUNE":
            case "APPLY_DEBUFF":
            case "APPLY_DAMAGE_MOD_GLOBAL":
                if (paramDict.TryGetValue("DEBUFF_ID", out string statusIdStr) && Enum.TryParse(statusIdStr, true, out StatusID statusID))
                {
                    StatusEffectManager.Instance.ApplyEffect(statusID, amount, duration, target);
                }
                else
                {
                    Debug.LogError($"[Status] DEBUFF_ID 파라미터가 누락되었거나 유효하지 않습니다.");
                }
                break;
            case "REMOVE_STATUS":
                if (paramDict.TryGetValue("STATUS_ID", out string statusRemoveIdStr) && Enum.TryParse(statusRemoveIdStr, true, out StatusID statusRemoveID))
                {
                    StatusEffectManager.Instance.RemoveStatus(target, statusRemoveID);
                }
                break;

            // -------------------- 덱/패 조작 로직 (구현) --------------------
            case "INSERT_DECK":
            case "INSERT_HAND":
                if (paramDict.TryGetValue("CARD_ID_TARGET", out string cardIdTarget))
                {
                    if (effectCode == "INSERT_DECK")
                        GameManager.Instance.InsertCardIntoDeck(cardIdTarget, amount);
                    else
                        GameManager.Instance.InsertCardIntoHand(cardIdTarget, amount);
                }
                break;
            case "DISCARD_HAND_ENEMY":
                GameManager.Instance.DiscardEnemyHand(amount);
                break;
            case "DISCARD_DECK_ENEMY":
                Debug.LogWarning($"[Deck] DISCARD_DECK_ENEMY 로직 구현 필요.");
                break;

            // -------------------- 코스트/스탯 수정 로직 (구현) --------------------
            case "REDUCE_COST_ALL":
                GameManager.Instance.ApplyAllHandCostModifier(amount);
                break;
            case "REDUCE_COST_SINGLE":
                Debug.LogError("REDUCE_COST_SINGLE: 타겟팅 로직을 통해서만 실행되어야 합니다.");
                break;
            case "MODIFY_HAND_STAT":
                Debug.LogWarning($"[Modifier] MODIFY_HAND_STAT 로직 구현 필요.");
                break;
            case "MODIFY_ENEMY_HAND_STAT":
                Debug.LogWarning($"[Modifier] MODIFY_ENEMY_HAND_STAT 로직 구현 필요.");
                break;

            // -------------------- 흐름 제어/특수 효과 (구현) --------------------
            case "TAKE_EXTRA_TURN":
            case "REFUND_COST_IMMEDIATE":
            case "CHECK_BRANCHING":
            case "MOVE_TO_OBJECT_RANGE":
                Debug.LogWarning($"[Flow/Board] {effectCode} 로직 구현 필요.");
                break;

            default:
                Debug.LogWarning($"알 수 없는 EffectCode: {effectCode}. 해당 로직 구현이 필요합니다.");
                break;
        }
    }
}