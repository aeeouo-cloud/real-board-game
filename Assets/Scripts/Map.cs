using System.Collections.Generic;
using UnityEngine;



public class Map : MonoBehaviour

{
<<<<<<< HEAD

=======
    public static Map instance;
>>>>>>> origin/main
    static public float hexsize = 1;

    static public Vector3 q = new Vector3(1.7320f, 0f, 0f);

    static public Vector3 r = new Vector3(0.8660f, 0f, 1.5f);

    public GameObject Hex;
<<<<<<< HEAD

    public int widthlength = 0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created

=======
    public int widthlength = 0; //그리드 반지름 길이
    Dictionary<Vector2Int, Hex> hexdic = new Dictionary<Vector2Int, Hex>();

    void Awake()
    {
        if(instance == null) instance = this;
    }
>>>>>>> origin/main
    void Start()
    {
        for (int i = -widthlength; i <= widthlength; i++)
        {
            int r1 = Mathf.Max(-widthlength, -i - widthlength);
            int r2 = Mathf.Min(widthlength, -i + widthlength);
            for (int o = r1; o <= r2; o++)
            {
<<<<<<< HEAD
                // 🚨 1. Hex 프리팹을 인스턴스화하고, 인스턴스를 변수(newHex)에 저장합니다. 🚨
                GameObject newHex = Instantiate(Hex, new Vector3(0, 0, 0), Quaternion.identity);

                // 🚨 2. Hex 프리팹 자체가 아닌, 생성된 오브젝트에 컴포넌트 접근 🚨
                newHex.GetComponent<Hex>().SetPosition(i, o);
            }
        }
=======
                GameObject newhex = Instantiate(Hex, new Vector3(0, 0, 0), Quaternion.identity);
                newhex.GetComponent<Hex>().SetPosition(i, o);
                newhex.transform.SetParent(this.transform);
                
                hexdic.Add(newhex.GetComponent<Hex>().qr, newhex.GetComponent<Hex>());
            }
        }
    }
    static readonly Vector2Int[] hexDirections = new Vector2Int[]
    {
        new Vector2Int(1, 0), 
        new Vector2Int(1, -1),  
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };
    public void SelectReachable(Vector2Int position, int range)
    {
        var selecthex = GetReachableHex(position,range);
        foreach(var hex in selecthex)
        {
            hex.isselectable = true;
        }
    }
    public void UnSelectHex()
    {
        foreach(var hex in hexdic.Values)
        {
            hex.isselectable = false;
        }
    }
    public List<Hex> GetReachableHex(Vector2Int start, int range)
    {
        List<Hex> reachable = new List<Hex>();
        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            if (!hexdic.ContainsKey(current)) continue;
            var currentHex = hexdic[current];
            hexdic[current].cost = dist;
            reachable.Add(currentHex);

            if (dist >= range) continue;

            foreach (var dir in hexDirections)
            {
                Vector2Int next = current + dir;

                if (visited.Contains(next)) continue;
                if (!hexdic.ContainsKey(next)) continue;

                var nextHex = hexdic[next];
                if (nextHex.iswall) continue;

                visited.Add(next);
                queue.Enqueue((next, dist + 1));
            }
        }
        return reachable;
>>>>>>> origin/main
    }



    // Update is called once per frame

    void Update()

    {



    }

}