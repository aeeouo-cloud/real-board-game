// Unit.cs
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Unit의 종류를 정의하는 열거형
    public enum UnitType { Player, Enemy }

    // Unit의 종류를 저장하는 변수 (Inspector에서 설정)
    public UnitType Type;

    public string UnitName = "Player";
    public int MaxHP = 20;
    public int CurrentHP = 20;
    public int CurrentPosition = 0; // 맵의 논리적 위치 (타일 인덱스 등)

    // 🚨 [핵심] 공격자(Source) 정보를 받아 최종 피해량을 계산합니다. 🚨
    public void TakeDamage(Unit source, int baseDamage)
    {
        int finalDamage = baseDamage;

        // StatusEffectManager가 있다면 최종 피해량을 계산합니다.
        if (StatusEffectManager.Instance != null)
        {
            // StatusEffectManager에게 공격자와 피해자에게 적용된 모든 버프/디버프를 반영한 최종 피해량을 요청
            finalDamage = StatusEffectManager.Instance.GetModifiedDamage(source, this, baseDamage);
        }

        // 최종 피해량 적용
        CurrentHP -= finalDamage;

        Debug.Log($"[Unit Logic] {UnitName}이 {finalDamage} 피해! (기본 피해: {baseDamage}) 남은 HP: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Debug.Log($"[Unit Logic] {UnitName}이 {amount} 회복! 현재 HP: {CurrentHP}");
    }

    // 🚨 [수정] 월드 위치 업데이트는 MapMovementHelper가 담당하도록 변경 🚨
    public void Move(int distance)
    {
        CurrentPosition += distance;
        Debug.Log($"[Unit Logic] {UnitName}의 논리적 위치 {distance}칸 변경! 현재 위치: {CurrentPosition}");
    }

    private void Die()
    {
        Debug.Log($"[Unit Logic] {UnitName} ({Type})이 사망했습니다!");

        // GameManager에게 사망 정보 전달 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleUnitDeath(this);
        }

        // 오브젝트 비활성화 (씬에서 제거)
        gameObject.SetActive(false);
    }
}