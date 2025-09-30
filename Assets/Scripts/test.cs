using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    List<List<int>> set = new List<List<int>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetCoordinate(5);
    }
    public List<List<int>> GetCoordinate(int range)
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

        Debug.Log("0리스트 = " + set[0]);
        return set;
    }

}
