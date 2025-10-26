using UnityEngine;

public class Hex : MonoBehaviour
{
    public Vector2 qr;
    public Vector3 position;

    public void SetPosition(int q, int r)
    {
        qr = new Vector2(q, r);
        position = new Vector3(0, 0, 0) + Map.q * q  * Map.hexsize + Map.r * r * Map.hexsize;
    }
    void Start()
    {
          transform.position = position;
    }

    void Update()
    {
        
    }
}
