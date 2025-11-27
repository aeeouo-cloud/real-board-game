using UnityEngine;
using System;

public class Unit : MonoBehaviour
{
    // Unit의 종류를 정의하는 열거형
    public enum UnitType { Player, Enemy }

    // Unit의 종류를 저장하는 변수 (Inspector에서 설정)
    public UnitType Type;

    public string UnitName = "Player";
    [Header("Base Stats")]
    public int MaxHP = 20;
    public int CurrentHP = 20;

    // Hex 좌표계 위치를 나타내는 Vector2Int 타입
    // UnityEngine.Vector2Int 대신 Vector2Int만 사용합니다.
    public Vector2Int CurrentPosition;

    // 유닛 상태를 외부에서 구독할 수 있는 이벤트 (기존 코드 유지)
    public event Action<Unit, int> OnDamageTaken;
    public event Action<Unit, int> OnHealed;
    public event Action<Unit> OnUnitDeath;

    void Awake()
    {
        // MaxHP가 20으로 정의되어 있으므로 CurrentHP를 MaxHP로 초기화합니다.
        CurrentHP = MaxHP;
    }

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
        if (finalDamage <= 0)
        {
            Debug.Log($"[Unit Logic] {UnitName}에게 적용될 피해가 0이하이므로 면역 처리되었습니다.");
            return;
        }

        CurrentHP -= finalDamage;

        Debug.Log($"[Unit Logic] {UnitName}이 {finalDamage} 피해! (기본 피해: {baseDamage}) 남은 HP: {CurrentHP}");
        OnDamageTaken?.Invoke(this, finalDamage);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        Debug.Log($"[Unit Logic] {UnitName}이 {amount} 회복! 현재 HP: {CurrentHP}");

        OnHealed?.Invoke(this, amount);
    }

    // 위치를 강제로 설정하는 함수 (Move.cs에서 이 함수 대신 CurrentPosition을 직접 할당할 수 있습니다.)
    public void SetPosition(Vector2Int newPos)
    {
        CurrentPosition = newPos;
        // 실제 게임 오브젝트의 위치는 Move.cs 또는 MapMovementHelper에서 설정되어야 합니다.
        Debug.Log($"[Unit Logic] {UnitName}의 논리적 위치가 {newPos}로 설정되었습니다.");
    }

    private void Die()
    {
        Debug.Log($"[Unit Logic] {UnitName} ({Type})이 사망했습니다!");
        OnUnitDeath?.Invoke(this);

        // GameManager에게 사망 정보 전달 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleUnitDeath(this);
        }

        // 오브젝트 비활성화 (씬에서 제거)
        gameObject.SetActive(false);
    }

    // 🚨 [추가] 유닛 클릭 감지 함수 (공격 타겟팅 입력 처리) 🚨
    void OnMouseDown()
    {
        // 1. 현재 게임 상태가 타일 타겟팅 대기 상태인지 확인합니다.
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.WaitingForTileTarget)
        {
            // 2. 이 유닛이 공격할 수 있는 대상(적 유닛)인지 확인합니다.
            //    공격 대상은 Enemy 타입이어야 합니다.
            if (this.Type == UnitType.Enemy)
            {
                // 3. GameManager에게 유닛의 현재 좌표를 전달하여 타겟팅을 해결하도록 명령합니다.
                //    공격 범위 검사 및 유효성 검사는 ResolveTileTargeting에서 Map.cs를 통해 수행됩니다.

                GameManager.Instance.ResolveTileTargeting(CurrentPosition);

                Debug.Log($"[Unit Input] 공격 유효 타겟 ({UnitName}) 클릭 감지! 좌표 {CurrentPosition}를 GameManager에 전달.");
            }
            else
            {
                // 아군 유닛 클릭 시
                Debug.LogWarning($"[Unit Input] 아군 유닛 ({UnitName})은 타겟으로 선택할 수 없습니다.");
            }
        }
        // 이 외의 상태에서의 클릭은 Hex.cs나 Move.cs에서 처리합니다.
    }
}
