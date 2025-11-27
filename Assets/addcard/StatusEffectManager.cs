// StatusEffectManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    // 🚨 [핵심 수정] public으로 변경하여 Inspector에 보이게 함 🚨
    public List<StatusEffectData> activeEffects = new List<StatusEffectData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -----------------------------------------------------------
    // 1. 효과 적용 로직
    // -----------------------------------------------------------

    public void ApplyEffect(StatusID effectID, int amount, int duration, Unit target)
    {
        // TODO: 같은 타입의 효과가 이미 존재하면 Duration/Amount를 갱신 또는 합산하는 로직 필요

        StatusEffectData newEffect = new StatusEffectData(effectID, amount, duration, target);
        activeEffects.Add(newEffect);

        Debug.Log($"[Status] {target.UnitName}에게 {effectID} 효과 적용. Amount: {amount}, Duration: {duration}");
    }

    // -----------------------------------------------------------
    // 2. 턴 종료 시 업데이트 로직 (GameManager가 호출)
    // -----------------------------------------------------------

    public void UpdateEffectsOnTurnEnd()
    {
        // 1. 지속 시간 감소 및 만료된 효과 제거
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectData effect = activeEffects[i];

            // 영구 효과(0)가 아니라면 지속 시간 감소
            if (effect.DurationRemaining > 0)
            {
                effect.DurationRemaining--;
            }

            // 지속 시간이 0이 되어 만료된 효과 제거
            if (effect.DurationRemaining == 0 && effect.ID != StatusID.NONE)
            {
                Debug.Log($"[Status] {effect.TargetUnit.UnitName}에게 적용된 {effect.ID} 효과 만료 및 제거.");
                activeEffects.RemoveAt(i);
            }
        }
    }

    // -----------------------------------------------------------
    // 3. 최종 능력치 계산 로직 (Unit.cs가 조회)
    // -----------------------------------------------------------

    // 유닛이 피해를 받거나 입힐 때 최종 능력치를 계산
    public int GetModifiedDamage(Unit source, Unit target, int baseDamage)
    {
        int finalDamage = baseDamage;

        // 1. 데미지를 입히는 유닛(Source)의 공격력 버프 확인 (DAMAGE_BOOST)
        var sourceBuffs = activeEffects.Where(e => e.TargetUnit == source && e.ID == StatusID.DAMAGE_BOOST);
        finalDamage += sourceBuffs.Sum(e => e.Amount);

        // 2. 데미지를 받는 유닛(Target)의 저항/방어 버프 확인 (DAMAGE_RESIST)
        var targetResists = activeEffects.Where(e => e.TargetUnit == target && e.ID == StatusID.DAMAGE_RESIST);
        finalDamage -= targetResists.Sum(e => e.Amount); // 방어력은 데미지를 감소시킴

        // 3. 데미지를 받는 유닛(Target)의 전역 피해 증가/감소 디버프 확인 (APPLY_DAMAGE_MOD_GLOBAL)
        var targetGlobalMods = activeEffects.Where(e => e.TargetUnit == target && e.ID == StatusID.APPLY_DAMAGE_MOD_GLOBAL);
        finalDamage += targetGlobalMods.Sum(e => e.Amount); // 피해량에 바로 합산

        // 최종 데미지는 최소 0
        return Mathf.Max(0, finalDamage);
    }
    // 특정 상태 이상을 제거하는 로직
    public void RemoveStatus(Unit target, StatusID statusID)
    {
        // 제거된 요소의 개수를 반환합니다.
        int removedCount = activeEffects.RemoveAll(e => e.TargetUnit == target && e.ID == statusID);

        if (removedCount > 0)
        {
            Debug.Log($"[Status] {target.UnitName}에게 적용된 {statusID} 효과 {removedCount}개 제거 완료.");
        }
    }

    // TODO: GetModifiedRange, IsImmuneToAttack 등 다른 조회 함수 필요
}