using System.Collections;
using UnityEngine;

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
                case MoveMode.CardMove: Map.instance.SelectReachable(unit.CurrentPosition, carddist); break;

                case MoveMode.CostMove: Map.instance.SelectReachable(unit.CurrentPosition, GameManager.Instance.CurrentCost); break;

                case MoveMode.Inactive: Map.instance.UnSelectHex(); break;
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
    void Start()
    {
        rend = this.GetComponent<Renderer>();
        unit = this.GetComponent<Unit>();
    }

    void Update()
    {
        // rend가 null이거나 GameManager.Instance가 null일 수 있으므로 체크
        if (rend != null && GameManager.Instance != null)
        {
            rend.material.color = currentmode == MoveMode.CostMove ? Color.red : Color.green; //for debug
        }

        if (Input.GetMouseButtonDown(0))
        {
            // [오류 수정] static이 아닌 필드 접근 오류 수정: GameManager.CurrentState -> GameManager.Instance.CurrentState
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase && currentmode != MoveMode.Inactive)
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
                            unit.CurrentPosition = hexComponent.qr;

                            if (currentmode == MoveMode.CostMove)
                            {
                                // 🚨 [핵심 추가] 코스트 체크 후, 실제로 코스트를 소모합니다. 🚨
                                if (GameManager.Instance.TryUseCost(hexComponent.cost))
                                {
                                    GameManager.Instance.ConsumeCost(hexComponent.cost);
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
                        Deck.LastCardCancel.Invoke();
                    }
                }
            }
            currentmode = MoveMode.Inactive;
        }
        if (Input.GetMouseButtonUp(0))
        {
            // [오류 수정] static이 아닌 필드 접근 오류 수정: GameManager.CurrentState -> GameManager.Instance.CurrentState
            if (click && onmouse && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase && currentmode == MoveMode.Inactive)
            {
                currentmode = MoveMode.CostMove;
            }
            click = false;
        }
    }
}
