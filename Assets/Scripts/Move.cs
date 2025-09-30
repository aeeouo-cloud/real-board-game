using UnityEngine;

public class Move : MonoBehaviour
{
    public void OnMouseDrag()
    {
        Debug.Log("호출");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100f, Color.red);
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hit;
    //         Debug.DrawRay(ray.origin, ray.direction*100, Color.red, 30);
    //               if (Physics.Raycast(ray, out hit))
    //     {

    //         Debug.Log(hit.transform.name);
    //     }
    //     }
    // }


}
