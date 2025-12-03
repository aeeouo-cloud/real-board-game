using System.Collections;
using UnityEngine;
using System.Linq; 

public class Move : MonoBehaviour
{
    public enum MoveMode { CardMove, CostMove, Inactive }
    public MoveMode mod = MoveMode.Inactive;
    public int carddist;
    public MoveMode currentmode //if current mode set select and unselect hex (except card move)
    {
        get => mod;
        set
        {
            if (mod == value)
                return;
            mod = value;
            switch (mod)
            {
                case MoveMode.CardMove: 
                    // 🚨 이동 시작 전 유닛 위치를 현재 논리적 위치에 강제 동기화 🚨
                    SynchronizeWorldPosition();
                    if (Map.instance != null) Map.instance.SelectReachable(unit.CurrentPosition, carddist); 
                    break;

                case MoveMode.CostMove: 
                    // 🚨 이동 시작 전 유닛 위치를 현재 논리적 위치에 강제 동기화 🚨
                    SynchronizeWorldPosition();
                    if (GameManager.Instance != null && Map.instance != null) Map.instance.SelectReachable(unit.CurrentPosition, GameManager.Instance.CurrentCost); 
                    break;

                case MoveMode.Inactive: 
                    if (Map.instance != null) Map.instance.UnSelectHex(); 
                    break;
            }
        }
    }
    public bool click = false;
    public bool onmouse = false;
    Renderer rend;
    Hex hexComponent;
    Unit unit;

    public void OnMouseDown()
    {
        click = true;
    }

    public void OnMouseEnter()
    {
        onmouse = true;
    }
    public void OnMouseExit()
    {
        onmouse = false;
    }
    
    // 🚨 [수정] Start()와 LateInitialization() 코루틴을 Awake()로 통합 및 제거 🚨
    void Awake()
    {
        rend = this.GetComponent<Renderer>();
        unit = this.GetComponent<Unit>();
        // Move.Start() 로직 제거
    }
    
    // 🚨 [수정] 유닛의 논리적 좌표를 기준으로 월드 위치를 강제 설정하는 함수 (public으로 변경) 🚨
    // Unit.cs의 CurrentPosition setter에서도 호출됩니다.
    public void SynchronizeWorldPosition()
    {
        if (unit == null || Map.instance == null) return;

        // 1. 유닛의 CurrentPosition (논리적 좌표)와 일치하는 Hex 타일을 찾습니다.
        Hex targetHex = FindObjectsByType<Hex>(FindObjectsSortMode.None)
                            .FirstOrDefault(h => h.qr == unit.CurrentPosition);

        if (targetHex != null)
        {
            // 2. 해당 타일의 월드 위치 + Y 오프셋(0.6f)으로 유닛의 Transform 위치를 덮어씁니다.
            Vector3 targetPosition = targetHex.transform.position + new Vector3(0f, 0.6f, 0f);
            this.transform.position = targetPosition;
            Debug.Log($"[Move Sync] 유닛 {unit.UnitName}의 월드 위치를 {unit.CurrentPosition}에 동기화 완료.");
        }
        else
        {
            // 이 오류는 Map에서 타일을 못 찾았을 때 발생하므로, 유닛 배치가 실패했음을 의미합니다.
            Debug.LogError($"[Move Sync] 동기화 실패: 논리적 좌표 {unit.CurrentPosition}에 해당하는 Hex를 씬에서 찾을 수 없습니다.");
        }
    }


    void Update()
    {
        // NullReferenceException 방지
        if (rend != null && GameManager.Instance != null) 
        {
            rend.material.color = currentmode == MoveMode.CostMove ? Color.red : Color.green; //for debug
        }

        if (Input.GetMouseButtonDown(0))
        {
            // GameManager.Instance가 null이면 실행하지 않습니다.
            if (GameManager.Instance == null) return;
            
            if (GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase && currentmode != MoveMode.Inactive)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 30);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.TryGetComponent<Hex>(out hexComponent))
                    {
                        if (hexComponent.isselectable)
                        {
                            this.transform.position = hit.transform.position;
                            unit.CurrentPosition = hexComponent.qr; // 👈 이 시점에 Unit.cs의 setter가 호출되고 위치가 동기화됩니다.
                            unit.RpcInvoke(hexComponent.qr);    //네트워크에 동기화

                            if (currentmode == MoveMode.CostMove)
                            {
                                // 🚨 [핵심 수정] 코스트 체크 후, 실제로 소모합니다. 🚨
                                if (GameManager.Instance.TryUseCost(hexComponent.cost))
                                {
                                    GameManager.Instance.ConsumeCost(hexComponent.cost); // <<-- 이 줄이 코스트를 차감합니다!
                                }
                            }
                            Debug.Log(hexComponent.qr);

                            currentmode = MoveMode.Inactive;
                        }
                        else
                        {
                            Debug.Log("unselectable");
                        }
                    }
                    else
                    {
                        Debug.Log("Not Map object");
                    }
                    if (currentmode == MoveMode.CardMove)
                    {
                        // Deck.LastCardCancel.Invoke(); // Deck에 대한 직접 참조는 위험하여 주석 처리
                    }
                }
            }
            currentmode = MoveMode.Inactive;
        }
        if (Input.GetMouseButtonUp(0))
        {
            // NullReferenceException 방지
            if (GameManager.Instance == null) return;
            
            if (click && onmouse && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase && currentmode == MoveMode.Inactive)
            {
                currentmode = MoveMode.CostMove;
            }
            click = false;
        }
    }
}