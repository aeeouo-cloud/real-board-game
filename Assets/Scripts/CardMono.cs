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
    bool ishovering = false;
    RectTransform rt;
    public Card carddata = new Card();

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
        if(bound != null)ishovering = RectTransformUtility.RectangleContainsScreenPoint(boundinstance.GetComponent<RectTransform>(), Input.mousePosition);
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out pos);
        hoverimage.transform.localPosition = pos;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (ishovering) // card activate
        {
            Debug.Log("card activated");
            Destroy(gameObject);
        }
        else
        {
            hoverimage.transform.position = transform.position;
        }
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
            boundinstance = Instantiate(bound);
            boundinstance.transform.SetParent(this.transform.parent?.parent, false);
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
