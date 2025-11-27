// CardEffectResolver.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardEffectResolver : MonoBehaviour
{
    // 🚨 1. 정적(static) Instance 변수 추가 🚨
    public static CardEffectResolver Instance;

    // ... (기존 public string TestCardID 등 변수) ...

    // 🚨 2. Awake 함수 추가 (또는 수정) 🚨
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 이 컴포넌트 자신을 Instance에 할당
        }
        else
        {
            // 중복 생성 방지
            Destroy(gameObject);
        }
    }
    // 🚨 1. Inspector에 테스트 ID를 입력할 수 있는 필드 추가 🚨
    public string TestCardID = "N002"; // 트랩 설치 카드 ID로 초기 설정

    // 🚨 공격 테스트를 위한 임시 적 유닛 변수 추가 🚨
    public Unit EnemyTarget; // Inspector에서 EnemyUnit 오브젝트를 여기에 연결합니다. 

    // 🚨 2. Inspector에 버튼을 생성하는 속성 추가 🚨
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
    Action CancelCostAction;
    // 카드 사용 시 호출되는 주 진입점 함수
    public void ExecuteCardEffect(string cardID)
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager가 초기화되지 않았습니다.");
            return;
        }

        // CardData 클래스 참조
        if (!DataManager.Instance.CardTable.TryGetValue(cardID, out CardData cardData))
        {
            Debug.LogError($"Card ID를 찾을 수 없음: {cardID}");
            return;
        }

        // 🚨 1. 코스트 값을 문자열에서 정수로 안전하게 변환 🚨
        if (!int.TryParse(cardData.cost, out int requiredCost))
        {
            Debug.LogError($"[Resolver] 카드 '{cardData.name}'의 코스트 '{cardData.cost}' 파싱 오류.");
            return; // 파싱 실패 시 카드 사용 중단
        }

        void CancelCost()// I ADDED IT!
        {
            if(CancelCostAction ==null)
            {
                CancelCostAction = () => 
                {
                    GameManager.Instance.AddCost(requiredCost);
                    Debug.Log($"Cost recovered - added {requiredCost}, currentcost {GameManager.Instance.CurrentCost}");
                };
            }
        }
        void Actionadd()
        {
            Deck.LastCardCancel += CancelCostAction;
        }
        void ActionRemove()
        {
            Deck.LastCardCancel -= CancelCostAction;
        }

        ActionRemove();
        // 🚨 2. 정수 타입의 requiredCost를 TryUseCost에 전달 🚨
        if (GameManager.Instance != null && !GameManager.Instance.TryUseCost(requiredCost))
        {
            Debug.LogWarning($"카드 사용 실패: 코스트 부족 ({cardData.name})");
            Deck.LastCardCancel.Invoke();
            return;
        }
        CancelCost();
        Actionadd();

        string effectGroupID = cardData.EffectGroup_ID; 


        // CardEffectSequenceData 클래스 참조
        if (!DataManager.Instance.EffectSequenceTable.TryGetValue(effectGroupID, out List<CardEffectSequenceData> sequenceList))
        {
            Debug.LogError($"EffectGroup ID를 찾을 수 없음: {effectGroupID}");
            return;
        }

        Debug.Log($"--- {cardData.name} 카드의 효과 실행 시작 (ID: {cardID}) ---");

        foreach (var step in sequenceList)
        {
            // CardParameterDetailsData 클래스 참조
            DataManager.Instance.ParameterDetailTable.TryGetValue(step.EffectStep_PK, out List<CardParameterDetailsData> parameters);

            ExecuteEffectLogic(step.EffectCode, parameters);
        }

        Debug.Log($"--- {cardData.name} 카드의 효과 실행 완료 ---");
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

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 인스턴스가 초기화되지 않았습니다.");
            return;
        }

        // 기본 대상은 PlayerUnit으로 설정 (힐/이동 등)
        Unit target = GameManager.Instance.PlayerUnit;
        if (target == null)
        {
            Debug.LogError("PlayerUnit이 GameManager의 Inspector 필드에 연결되지 않았습니다.");
            // return; // 공격 시에는 EnemyTarget을 사용할 것이므로 주석 처리
        }

        // EffectCode에 따라 로직 분기 및 연결
        switch (effectCode)
        {
            case "ATTACK_SINGLE":
            case "ATTACK_CONDITIONAL":
                int range = GetIntParam(paramDict, "MAX_RANGE");
                int damage = GetIntParam(paramDict, "DAMAGE_AMOUNT");

                // 🚨 공격 대상 지정: EnemyTarget 사용 🚨
                Unit attackTarget = EnemyTarget;

                if (attackTarget != null)
                {
                    GameManager.Instance.ApplyAttack(attackTarget, damage, range);
                }
                else
                {
                    Debug.LogError("[Attack Test] EnemyTarget 변수에 공격할 유닛을 연결해 주세요.");
                }
                break;

            case "DRAW_CARD_SELF":
                int drawAmount = GetIntParam(paramDict, "AMOUNT");
                GameManager.Instance.ProcessDraw(drawAmount);
                break;

            case "HEAL_HP":
                int healAmount = GetIntParam(paramDict, "AMOUNT");
                GameManager.Instance.ProcessHeal(target, healAmount);
                break;

            case "MOVE_SELF":
                int moveDistance = GetIntParam(paramDict, "DISTANCE");
                GameManager.Instance.ProcessMove(target, moveDistance);
                break;

            case "PLACE_TRAP":
                int trapRange = GetIntParam(paramDict, "MAX_RANGE");
                int slowAmount = GetIntParam(paramDict, "SLOW_AMOUNT");
                GameManager.Instance.PlaceTrap(trapRange, slowAmount);
                break;

            default:
                Debug.LogWarning($"알 수 없는 EffectCode: {effectCode}. 해당 로직 구현이 필요합니다.");
                break;
        }
    }
}