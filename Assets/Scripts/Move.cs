using System.Collections;

using UnityEngine;

public class Move : MonoBehaviour
{
    public bool active = false;
    public bool click = false;
    public bool onmouse = false;

    Renderer rend;

    GameObject beforelocation;

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
    }

    void Update()
    {
        rend.material.color = active ? Color.red : Color.green; //for debug

        if (Input.GetMouseButtonDown(0))
        {
            if (active && GameManager.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 30);
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log(hit.transform.name);
                }

                if (Physics.Raycast(ray, out hit))
                {
                    if (beforelocation == hit.collider.gameObject)
                    {
                        Debug.Log("same location!");
                    }

                    if (hit.collider.gameObject == this.gameObject)
                    {
                        Debug.Log("selfhit");
                    }
                    this.transform.position = hit.transform.position;
                    beforelocation = hit.collider.gameObject;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (active && true && GameManager.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase)
            {
                active = false;
                return;
            }
            if (click && onmouse && GameManager.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase)
            {
                active = true;
            }
            click = false;
        }

    }
}
