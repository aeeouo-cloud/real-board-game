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
    public bool isready;
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
    public Hex GetHexAt(Vector2Int coord)
    {
        if (!isready)
        {
            throw new System.Exception("Map not ready");
        }
        return hexdic[coord];
    }
    static readonly Vector2Int[] hexDirections = new Vector2Int[]
    {
        new Vector2Int(1, 0),    // E
        new Vector2Int(0, 1),    // NE
        new Vector2Int(-1, 1),   // NW
        new Vector2Int(-1, 0),   // W
        new Vector2Int(0, -1),   // SW
        new Vector2Int(1, -1)    // SE
    };

    public void SelectReachable(Vector2Int position, int range)
    {
        var selecthex = GetReachableHex(position, range);
        foreach (var hex in selecthex)
        {
            hex.isselectable = true;
        }
    }
    public void UnSelectHex()
    {
        foreach (var hex in hexdic.Values)
        {
            // NullReferenceException 방지를 위해 hex가 null이 아닌지 체크합니다.
            if (hex != null)
            {
                hex.isselectable = false;
            }
        }
    }

    // 🚨 [핵심 수정] GetReachableHex 함수 (6방향 BFS 로직) 🚨
    public List<Hex> GetReachableHex(Vector2Int start, int range)
    {
        List<Hex> reachable = new List<Hex>();
        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // 시작점에서 Map.cs의 hexdic이 이 Hex를 포함하고 있는지 확인 
        if (!hexdic.ContainsKey(start))
        {
            Debug.LogWarning($"[Map] 시작 좌표 {start}는 맵에 존재하지 않아 이동 범위 계산에 실패했습니다.");
            return reachable;
        }

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            if (!hexdic.ContainsKey(current)) continue; // 맵에 없는 좌표는 건너뜁니다.

            // 현재 타일의 cost를 업데이트하고, reachable 리스트에 추가합니다.
            var currentHex = hexdic[current];
            currentHex.cost = dist;
            reachable.Add(currentHex);

            if (dist >= range) continue; // 최대 범위 도달 시 더 이상 탐색하지 않습니다.

            // 6방향 탐색을 보장하는 표준 로직입니다.
            foreach (var dir in hexDirections)
            {
                Vector2Int next = current + dir;

                if (visited.Contains(next)) continue;
                if (!hexdic.ContainsKey(next)) continue; // 맵 밖 타일 제외

                var nextHex = hexdic[next];
                if (nextHex.iswall) continue; // 벽 타일 제외

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