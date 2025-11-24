using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    static public GameObject canvas;
    void Awake()
    {
        canvas = this.gameObject;
    }
}
