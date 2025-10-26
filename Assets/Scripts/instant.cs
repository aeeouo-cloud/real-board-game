using UnityEngine;

public class instant : MonoBehaviour
{
    public GameObject prefab;     // 생성할 프리팹
    public GameObject mapmanager;

    void Start()
    {
        Debug.Log(MapManager.position[0]);

        if (prefab == null || MapManager.position == null)
        {
            Debug.Log("Prefab 또는 Positions 배열이 할당되지 않았습니다!");
            return;
        }

        for (int i = 0; i < MapManager.position.Length; i++)
        {
            Instantiate(prefab, MapManager.position[i], Quaternion.identity);
            // Quaternion.identity → 회전 없이 배치
        }
    }
}