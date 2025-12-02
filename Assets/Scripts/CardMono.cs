using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;


public class CardMono : MonoBehaviour, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler //using card, card image generate script
{
    public AssetReferenceGameObject dropbound;
    static AsyncOperationHandle<GameObject> loadhandle;
    static GameObject boundcach;
    static GameObject boundinstance;
    static int referencecount = 0;
    public GameObject hoverui;
    public GameObject hoverimage;
    Vector3 originalscale;
    Canvas imagecanvas;
    public float sacleamount = 3f;
    public bool ishovering = false;
    public string cardid;
    void Awake()
    {
        referencecount ++;
        if(boundcach != null || referencecount > 1)
        {
            return;
        }
        loadhandle = dropbound.LoadAssetAsync<GameObject>();
        loadhandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                boundcach = handle.Result;
                if(boundinstance == null)
                {
                boundinstance = Instantiate(boundcach, CanvasManager.canvas.GetComponent<RectTransform>(), false);                    
                }
            }
            else
            {
                Debug.Log("failed to load bound prefab");
            }
        };
    }
    void OnEnable()
    {
        hoverimage.GetComponent<CardImage>().UpdateImage(cardid);
    }

    public void OnDrag(PointerEventData eventData)
    {   
        if(boundinstance != null) ishovering = RectTransformUtility.RectangleContainsScreenPoint(boundinstance.GetComponent<RectTransform>(), Input.mousePosition);
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out pos);
        hoverimage.transform.localPosition = pos;
    }
    void ActionAdd()
    {
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
        
        if (ishovering && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn_ActionPhase) // card activate
        {
        GetComponent<CardDisplay>().Initialize(cardid);
        
        CardDisplay display = GetComponent<CardDisplay>();

        if (display == null)
        {
            Debug.LogError($"[Use] 카드 사용 실패: {cardid}에서 CardDisplay 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 1. 코스트 조회 (CardDisplay는 GameManager의 GetFinalCost를 참조하여 코스트를 가져옴)
        int actualCost = display.CardCost;

        // 2. 턴 상태 체크
        if (GameManager.Instance.CurrentState != GameManager.GameState.PlayerTurn_ActionPhase)
        {
            GameManager.Instance.ShowWarning("카드는 액션 페이즈에만 사용할 수 있습니다.");
            return;
        }

        // 🚨 3. 코스트 체크 (차감하지 않고 순수하게 체크만)
        if (GameManager.Instance.TryUseCost(actualCost))
        {
            // 🚨 4. 사용 전 최종 유효성 검사 (사거리 내 타겟 유무 체크) 🚨
            if (CardEffectResolver.Instance.NeedsTargetValidation(cardid))
            {
                if (!CardEffectResolver.Instance.IsActionValid(cardid))
                {
                    GameManager.Instance.ShowWarning("사용 불가: 유효한 타겟이 사거리 내에 없습니다!");
                    return; // 코스트 소모 및 효과 실행을 막습니다.
                }
            }

            // 5. 코스트 체크 성공 -> 실제로 코스트 차감
            GameManager.Instance.ConsumeCost(actualCost);

            // 6. 효과 실행
            CardEffectResolver.Instance.ExecuteCardEffect(cardid);

            // 7. PlayerHand 리스트에서 해당 카드 ID 제거 (UI 제거 동기화)
            GameManager.Instance.PlayerHand.Remove(cardid);

            Debug.Log($"[Use] 카드 사용 성공: {cardid} (Cost: {actualCost})");
            this.gameObject.SetActive(false);
        }
        else
        {
            // 8. 코스트 부족 실패 -> 경고 메시지 출력
            GameManager.Instance.ShowWarning("코스트가 모자랍니다!");
        }
        }
        hoverimage.transform.position = transform.position;
    }
    // private IEnumerator InstantiateBound() //wait for load
    // {
    //     while (bound == null)
    //     {
    //         yield return null;
    //     }
    //     if (boundinstance == null)
    //     {
    //         boundinstance = Instantiate(bound,CanvasManager.canvas.GetComponent<RectTransform>(),false);
    //     }
    // }
    // Update is called once per frame

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalscale = hoverimage.transform.localScale;
        hoverimage.transform.localScale = new Vector3(sacleamount, sacleamount, sacleamount);
        imagecanvas = hoverimage.GetComponent<Canvas>();
        imagecanvas.overrideSorting = true;
        imagecanvas.sortingOrder = 50;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        imagecanvas.overrideSorting = false;
        imagecanvas.sortingOrder = 0;
        hoverimage.transform.localScale = originalscale;
    }   
}
