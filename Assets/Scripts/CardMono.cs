using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;


public class CardMono : MonoBehaviour, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler //using card, card image generate script
{
    public AssetReferenceGameObject dropbound;
    GameObject bound;
    static GameObject boundinstance;
    public GameObject hoverui;
    public GameObject hoverimage;
    Vector3 originalscale;
    public float sacleamount = 3f;
    public bool ishovering = false;
    RectTransform rt;
    public string cardid;
    void Awake()
    {
        dropbound.LoadAssetAsync<GameObject>().Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                bound = handle.Result;
            }
            else
            {
                Debug.Log("failed to load bound prefab");
            }
        };
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (boundinstance != null) ishovering = RectTransformUtility.RectangleContainsScreenPoint(boundinstance.GetComponent<RectTransform>(), Input.mousePosition);
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out pos);
        hoverimage.transform.localPosition = pos;
    }
    void ActionAdd()
    {
        // Deck.instance가 null인지 확인하여 오류 방지
        if (Deck.instance != null)
        {
            Deck.LastCardCancel -= Deck.instance.LastActive;
            Action ActiveThis = () =>
            {
                Debug.Log("LastcardActived");
                this.gameObject.SetActive(true);
            };
            Deck.instance.LastActive = ActiveThis;
            Deck.LastCardCancel += Deck.instance.LastActive;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        // [수정된 부분] GameManager.Instance를 통해 CurrentState에 접근합니다.
        if (ishovering && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase) // card activate
        {
            this.gameObject.SetActive(false);
            ActionAdd();
            CardEffectResolver.Instance.ExecuteCardEffect(cardid);
        }
        hoverimage.transform.position = transform.position;
    }
    void Start()
    {
        rt = hoverui.GetComponent<RectTransform>();
        StartCoroutine(InstantiateBound());
    }
    private IEnumerator InstantiateBound() //wait for load
    {
        while (bound == null)
        {
            yield return null;
        }
        if (boundinstance == null)
        {
            boundinstance = Instantiate(bound, CanvasManager.canvas.GetComponent<RectTransform>(), false);
        }
    }
    // Update is called once per frame

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalscale = hoverimage.transform.localScale;
        hoverimage.transform.localScale = new Vector3(sacleamount, sacleamount, sacleamount);
        hoverimage.transform.SetAsLastSibling();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hoverimage.transform.localScale = originalscale;
    }
}
