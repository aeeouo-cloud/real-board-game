using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    static public float hexsize = 1;
    static public Vector3 q = new Vector3(1.7320f, 0f, 0f);
    static public Vector3 r = new Vector3(0.8660f, 0f, 1.5f);
    public GameObject Hex;
    public int widthlength = 0; //그리드 반지름 길이

    Dictionary<Vector2Int, Hex> hexdic = new Dictionary<Vector2Int, Hex>();

    void Start()
    {
        for (int i = -widthlength; i <= widthlength; i++)
        {
            int r1 = Mathf.Max(-widthlength, -i - widthlength);
            int r2 = Mathf.Min(widthlength, -i + widthlength);
            for (int o = r1; o <= r2; o++)
            {
                GameObject newhex = Instantiate(Hex, new Vector3(0, 0, 0), Quaternion.identity);
                newhex.GetComponent<Hex>().SetPosition(i, o);
                
                hexdic.Add(newhex.GetComponent<Hex>().qr, newhex.GetComponent<Hex>());
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
