using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckCreateButton : MonoBehaviour
{
    public PlayerDeck playerDeck;
    public TMP_InputField inputField;
    public void ProcessInputText()
    {
        string inputText = inputField.text;
        playerDeck.AddList(inputText);
        
        inputField.text = ""; 
    }
    public void SaveDeck()
    {
        playerDeck.Save();
    }
}