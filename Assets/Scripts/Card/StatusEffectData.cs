// StatusEffectData.cs
using System;
using UnityEngine;

// 효과의 종류를 명확하게 정의하는 열거형
public enum StatusID
{
    NONE,
    DAMAGE_BOOST,
    DAMAGE_RESIST,
    SLOW,
    POISON,
    ATTACK_IMMUNE,
    COST_REDUCTION,

    // 전역 피해 수정 디버프 
    APPLY_DAMAGE_MOD_GLOBAL,

    // TODO: 필요한 다른 StatusID도 여기에 추가해야 합니다.
}


// StatusEffectManager가 리스트로 관리할 데이터 구조체입니다.
[Serializable]
public class StatusEffectData
{
    // 어떤 종류의 효과인지
    public StatusID ID;

    // 효과의 수치 (데미지 +2, 감속 3 등)
    public int Amount;

    // 남은 지속 시간 (턴 수)
    public int DurationRemaining;

    // 이 효과가 적용된 대상
    public Unit TargetUnit;

    public StatusEffectData(StatusID id, int amount, int duration, Unit target)
    {
        this.ID = id;
        this.Amount = amount;
        this.DurationRemaining = duration;
        this.TargetUnit = target;
    }
}