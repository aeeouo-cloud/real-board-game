using System.Collections;

using UnityEngine;

public class Move : MonoBehaviour
{
    public enum MoveMode {CardMove, CostMove, Inactive}
    private MoveMode mod;
    public int carddist;
    public MoveMode currentmode //if current mode set select and unselect hex (except card move)
    {
        get => mod;
        set
        {
            if (mod == value)
            return;
            mod = value;
            switch(mod)
            {
                case MoveMode.CardMove : Map.instance.SelectReachable(unit.CurrentPosition, carddist); break;

                case MoveMode.CostMove : Map.instance.SelectReachable(unit.CurrentPosition, GameManager.Instance.CurrentCost); break;

                case MoveMode.Inactive : Map.instance.UnSelectHex(); break;
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
        Debug.Log("mouse in");
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
        rend.material.color = currentmode == MoveMode.CostMove ? Color.red : Color.green; //for debug
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase && currentmode != MoveMode.Inactive )
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

                            if(currentmode == MoveMode.CostMove)
                            {
                                GameManager.Instance.TryUseCost(hexComponent.cost);
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
                    if(currentmode == MoveMode.CardMove)
                    {
                        Deck.LastCardCancel.Invoke();
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (click && onmouse && GameManager.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase && currentmode == MoveMode.Inactive)
            {
                currentmode = MoveMode.CostMove;
            }
            else
            {
                currentmode = MoveMode.Inactive;
            }
            click = false;
        }
    }
}
