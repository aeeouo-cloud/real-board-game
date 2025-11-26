using UnityEngine;

public class Hex : MonoBehaviour
{
    public Vector2Int qr;
    public Vector3 position;
    public int cost;
    public bool iswall;
    public bool isselectable = false; 
    Renderer rend;
    public void SetPosition(int q, int r)
    {
        qr = new Vector2Int(q, r);
        position = new Vector3(0, 0, 0) + Map.q * q * Map.hexsize + Map.r * r * Map.hexsize;

        this.transform.position = position;
    }
    void Start()
    {
        rend = this.GetComponent<Renderer>();
    }

    void Update()
    {
        rend.material.color = isselectable ? Color.red : Color.green;
    }
}
