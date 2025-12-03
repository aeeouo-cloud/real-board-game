using TMPro;
using UnityEngine;

public class HPUI : MonoBehaviour
{
    public GameObject unit;
    string hp;
    TextMeshProUGUI text;
    Unit unitcom;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        unitcom = unit.GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = unitcom.CurrentHP.ToString();
    }
}
