using UnityEngine;

public class Unit : MonoBehaviour
{
    public string UnitName = "Player";
    public int MaxHP = 20;
    public int CurrentHP = 20;
    public Vector2Int CurrentPosition 
    {
        get => pos; 
        set 
        {
            pos = value;
            this.transform.position = Map.instance.GetHex(pos).transform.position;
        }
    } // 맵의 위치 (타일 인덱스 등) unit의 좌표를 바꾸면 자동으로 해당 칸으로 이동합니다.
    MultyUnit multyunit;
    private Vector2Int pos;
    Move move;
    void Awake()
    {
        multyunit = this.GetComponent<MultyUnit>();
        multyunit.OnPosChanged += ServerPosChange;
    }
    void ServerPosChange(Vector2Int prevPos, Vector2Int newPos)
    {
        CurrentPosition = newPos;
    }
    void Start()
    {
        move = this.GetComponent<Move>();
    }
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
        move.carddist = distance;
        move.currentmode = global::Move.MoveMode.CardMove;
    }
}