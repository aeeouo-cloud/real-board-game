using System.Collections.Generic;
using UnityEngine;

public class Select : MonoBehaviour
{
    public GameObject mapmanager;
    public float x;
    List<List<int>> set = new List<List<int>>();

    public List<Vector2> GetCoordinate(int range, Vector2 origin)
    {
        for (int i = 0; i < range; range--)
        {
            for (int a = -range; a <= range; a++)
            {
                for (int b = -range; b <= range; b++)
                {
                    if (2 * range == Mathf.Abs(a) + Mathf.Abs(b) + Mathf.Abs(a + b))
                    {
                        set.Add(new List<int> { a, b });
                    }
                }
            }
        }
        List<Vector2> select = new List<Vector2>();
        foreach (List<int> argument in set)
        {
            select.Add(new Vector2(origin.x + argument[0], origin.y + argument[1]));
        }
        Debug.Log(set[0][0]);
        return select;
    }
}
