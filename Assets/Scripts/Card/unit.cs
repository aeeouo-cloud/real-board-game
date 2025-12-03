using UnityEngine;
using System;
using System.Linq;
using Unity.Netcode; // FindObjectsByType 사용을 위해 추가

public class Unit : MonoBehaviour
{
    public enum UnitType { Player, Enemy }

    // Unit의 종류를 저장하는 변수 (Inspector에서 설정)
    public UnitType Type;

    public string UnitName = "Player";
    [Header("Base Stats")]
    public int MaxHP = 20;
    public int CurrentHP = 20;
    
    // 🚨 [핵심 수정] pos는 Get/Set에서 내부적으로만 사용합니다. 🚨
    public Vector2Int pos; 
    
    public Vector2Int CurrentPosition 
    {
        get => pos; 
        set 
        {
            pos = value;
            // 🚨 1. 충돌 유발 코드 제거: Map.instance를 여기서 호출하지 않습니다. 🚨
            // this.transform.position = Map.instance.GetHexAt(pos).transform.position; 
            
            // 2. Move 스크립트에 시각적 동기화를 요청합니다. (Map 준비 완료 후 Move.cs가 처리)
            // Move moveComp = this.GetComponent<Move>();
            // if (moveComp != null && moveComp.isActiveAndEnabled)
            // {
            //      // Move.cs의 SynchronizeWorldPosition을 호출하여 시각적 위치를 업데이트합니다.
            //      moveComp.SynchronizeWorldPosition(); 
            // }
            if (Map.instance != null)transform.position = Map.instance.GetHexAt(pos).transform.position;
            // (Move.cs가 붙어있지 않거나 비활성화된 경우, 이 업데이트는 다음 프레임에 처리됩니다.)
        } 
    } // 맵의 위치 (타일 인덱스 등) unit의 좌표를 바꾸면 자동으로 해당 칸으로 이동합니다.

    public event Action<Unit, int> OnDamageTaken;
    public event Action<Unit, int> OnHealed;
    public event Action<Unit> OnUnitDeath;

    Move move;
    MultyUnit multyunit;

    void Awake()
    {
        CurrentHP = MaxHP; // Awake에서 HP 초기화 (Start보다 먼저)
        multyunit = GetComponent<MultyUnit>();
    }
    public void RpcInvoke(Vector2Int pos)
    {
        multyunit.UnitPosChangedServerRpc(pos, GetComponent<NetworkObject>().NetworkObjectId);
    }
    
    void Start()
    {
        move = this.GetComponent<Move>();
    }
    
    public void TakeDamage(Unit source, int baseDamage)
    {
        int finalDamage = baseDamage;

        // StatusEffectManager가 있다면 최종 피해량을 계산합니다.
        if (StatusEffectManager.Instance != null)
        {
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
            if (this.Type == UnitType.Enemy)
            {
                // 3. GameManager에게 유닛의 현재 좌표를 전달하여 타겟팅을 해결하도록 명령합니다.
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
    
    // Move.cs에서 호출될 수 있도록 CardMove 로직은 그대로 유지
    public void Move(int distance)
    {
        if(move != null)
        {
            move.carddist = distance;
            move.currentmode = global::Move.MoveMode.CardMove;
        }
        else
        {
             Debug.LogError("Move 컴포넌트를 찾을 수 없습니다.");
        }
    }
}