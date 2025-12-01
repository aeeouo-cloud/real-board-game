using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Map : MonoBehaviour
{
    public AssetReferenceGameObject hexprefab;
    public static Map instance;
    static public float hexsize = 1;
    static public Vector3 q = new Vector3(1.7320f, 0f, 0f);
    static public Vector3 r = new Vector3(0.8660f, 0f, 1.5f);
   
    public int widthlength = 0; //그리드 반지름 길이
    Dictionary<Vector2Int, Hex> hexdic = new Dictionary<Vector2Int, Hex>();
    GameObject Hex;
    bool isready;
    void Awake()
    {   
        if(instance == null) instance = this;
        else {Destroy(gameObject); return;}

        Loadasset().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
        },TaskScheduler.FromCurrentSynchronizationContext()
        );
    }
    public async Task Loadasset()
    {
        if ( Hex == null)
        {
            var task = Addressables.LoadAssetAsync<GameObject>(hexprefab);
            await task.Task;
            Hex = task.Result;
            Starting();
        }
    }
    void Starting()
    {
        for (int i = -widthlength; i <= widthlength; i++)
        {
            int r1 = Mathf.Max(-widthlength, -i - widthlength);
            int r2 = Mathf.Min(widthlength, -i + widthlength);
            for (int o = r1; o <= r2; o++)
            {
                Hex newhexcom;
                GameObject newhex = Instantiate(Hex, Vector3.zero, Quaternion.identity,this.transform);
                newhexcom = newhex.GetComponent<Hex>();
                newhexcom.SetPosition(i, o);
                
                hexdic.Add(newhexcom.qr, newhexcom);
            }
        }
        isready = true;
        Hex = null;
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
    public Hex GetHex(Vector2Int cord)
    {
        if (!isready)
        {
            throw new System.Exception("Map not ready");
        }
        return hexdic[cord];
    }
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
