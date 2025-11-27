using UnityEngine;
using UnityEngine.UI;
public class TurnButton : MonoBehaviour
{
    public GameObject turnmanager;
    public Button button;
    void onbuttonclick()
    {
        if (GameManager.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase)
        {
            turnmanager.GetComponent<TurnManager>().EndTurn(); 
            Debug.Log("turneded");
            return;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(onbuttonclick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
