using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Cecil;

public class CardImage : MonoBehaviour      //updateimage를 실행하면 카드 아이디에 맞게 알아서 카드 이미지를 만들어줄겁니다.
{
    public GameObject image;
    public GameObject des;
    public GameObject cardname;
    public GameObject cost;
    public Sprite missingimage;
    TextMeshProUGUI nameUI;
    TextMeshProUGUI desUI;
    TextMeshProUGUI costUI;
    Image imageUI;

    public void UpdateImage(string Id)
    {
        
        imageUI = image.GetComponent<Image>();
        desUI = des.GetComponent<TextMeshProUGUI>();
        nameUI = cardname.GetComponent<TextMeshProUGUI>();
        costUI = cost.GetComponent<TextMeshProUGUI>();
        
        if (DataManager.Instance.CardTable.TryGetValue(Id, out CardData cardData))
        {
            //image
            Sprite loadimage = Resources.Load<Sprite>("CardImage/" + Id);
            if(loadimage != null)
            {
                imageUI.sprite = loadimage;
            }
            else
            {
                imageUI.sprite = missingimage;
            }
            //name
            if(cardData.name != null)
            {
                nameUI.text = cardData.name;
            }
            //des
            if(cardData.Description != null)
            {
                desUI.text = cardData.Description;
            }
            if(cardData.cost != null)
            {
                costUI.text = cardData.cost;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
