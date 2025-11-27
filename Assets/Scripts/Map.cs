using Unity.Mathematics;

using UnityEngine;



public class Map : MonoBehaviour

{

    static public float hexsize = 1;

    static public Vector3 q = new Vector3(1.7320f, 0f, 0f);

    static public Vector3 r = new Vector3(0.8660f, 0f, 1.5f);

    public GameObject Hex;

    public int widthlength = 0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        for (int i = -widthlength; i <= widthlength; i++)
        {
            int r1 = Mathf.Max(-widthlength, -i - widthlength);
            int r2 = Mathf.Min(widthlength, -i + widthlength);
            for (int o = r1; o <= r2; o++)
            {
                // 🚨 1. Hex 프리팹을 인스턴스화하고, 인스턴스를 변수(newHex)에 저장합니다. 🚨
                GameObject newHex = Instantiate(Hex, new Vector3(0, 0, 0), Quaternion.identity);

                // 🚨 2. Hex 프리팹 자체가 아닌, 생성된 오브젝트에 컴포넌트 접근 🚨
                newHex.GetComponent<Hex>().SetPosition(i, o);
            }
        }
    }



    // Update is called once per frame

    void Update()

    {



    }

}