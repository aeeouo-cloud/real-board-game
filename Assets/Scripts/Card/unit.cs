// Unit.cs (새 스크립트)
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string UnitName = "Player";
    public int MaxHP = 20;
    public int CurrentHP = 20;
    public int CurrentPosition = 0; // 맵의 위치 (타일 인덱스 등)

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        Debug.Log($"[Unit Logic] {UnitName}이 {amount} 피해! 남은 HP: {CurrentHP}");
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Debug.Log($"[Unit Logic] {UnitName}이 {amount} 회복! 현재 HP: {CurrentHP}");
    }

    public void Move(int distance)
    {
        CurrentPosition += distance;
        Debug.Log($"[Unit Logic] {UnitName}이 {distance}칸 이동! 현재 위치: {CurrentPosition}");
    }
}