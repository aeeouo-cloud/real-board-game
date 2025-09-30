using UnityEngine;

public class SIZEZEGI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
if (rend != null)
{
    Vector3 size = rend.bounds.size;
    Debug.Log("World size: " + size);
}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
