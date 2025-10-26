// CardEffectResolver.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardEffectResolver : MonoBehaviour
{
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

    // EffectCode에 따라 실제 게임 로직을 실행하는 핵심 함수
    private void ExecuteEffectLogic(string effectCode, List<CardParameterDetailsData> parameters) 
    {
        Dictionary<string, string> paramDict = parameters?.ToDictionary(p => p.ParameterKey, p => p.ParameterValue)
                                                ?? new Dictionary<string, string>();

        // 디버그 로그가 출력될 것
        switch (effectCode)
        {
            case "ATTACK_SINGLE":
                if (!paramDict.TryGetValue("MAX_RANGE", out string rangeStr) || !paramDict.TryGetValue("DAMAGE_AMOUNT", out string damageStr))
                {
                    Debug.LogError($"ATTACK_SINGLE: 필수 파라미터 누락됨.");
                    return;
                }
                if (int.TryParse(rangeStr, out int range) && int.TryParse(damageStr, out int damage))
                {
                    Debug.Log($"[로직 실행] ATTACK_SINGLE: 사거리 {range}, 데미지 {damage} 적용");
                }
                break;

            case "DRAW_CARD_SELF":
                if (!paramDict.TryGetValue("AMOUNT", out string amountStr) || !int.TryParse(amountStr, out int amount))
                {
                    Debug.LogError($"DRAW_CARD_SELF: 필수 파라미터(AMOUNT) 누락 또는 오류.");
                    return;
                }
                Debug.Log($"[로직 실행] DRAW_CARD_SELF: 카드 {amount}장 드로우");
                break;

            case "HEAL_HP":
                if (!paramDict.TryGetValue("AMOUNT", out string healStr) || !int.TryParse(healStr, out int healAmount))
                {
                    Debug.LogError($"HEAL_HP: 필수 파라미터(AMOUNT) 누락 또는 오류.");
                    return;
                }
                Debug.Log($"[로직 실행] HEAL_HP: 체력 {healAmount} 회복");
                break;

            default:
                Debug.LogWarning($"알 수 없는 EffectCode: {effectCode}. 해당 로직 구현이 필요합니다.");
                break;
        }
    }
}

