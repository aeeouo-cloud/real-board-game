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
    public static int Distance(Hex a, Hex b)
    {
        // 육각형 거리 계산 공식: max(|q1-q2|, |r1-r2|, |s1-s2|)
        // 여기서 s = -q-r 이므로, q, r만으로 거리를 계산합니다.

        int dq = Mathf.Abs((int)a.qr.x - (int)b.qr.x);
        int dr = Mathf.Abs((int)a.qr.y - (int)b.qr.y);
        int ds = Mathf.Abs((int)(-a.qr.x - a.qr.y) - (int)(-b.qr.x - b.qr.y));

        return Mathf.Max(dq, dr, ds);
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
