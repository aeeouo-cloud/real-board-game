// CostMonitor.cs
using UnityEngine;
using TMPro; // TextMeshPro를 사용한다면 이 using이 필요합니다.

public class CostMonitor : MonoBehaviour
{
    // Inspector에서 연결할 TextMeshPro 컴포넌트
    public TextMeshProUGUI CostText;

    // GameManager 참조
    private GameManager gameManager;

    void Start()
    {
        // GameManager 인스턴스 참조
        gameManager = GameManager.Instance;

        if (CostText == null)
        {
            // 이 스크립트가 붙은 오브젝트에서 TextMeshPro 컴포넌트를 찾습니다.
            CostText = GetComponent<TextMeshProUGUI>();
        }

        if (gameManager == null)
        {
            Debug.LogError("CostMonitor: GameManager 인스턴스를 찾을 수 없습니다.");
        }

        // 씬 시작 시 즉시 코스트 업데이트
        UpdateCostDisplay();
    }

    void Update()
    {
        // 코스트가 변경되었는지 확인하고 업데이트 (간단한 방법)
        // 더 효율적인 방법은 이벤트 기반으로 업데이트하는 것이지만, 여기서는 Update를 사용합니다.
        UpdateCostDisplay();
    }

    public void UpdateCostDisplay()
    {
        if (gameManager == null || CostText == null) return;

        // 현재 코스트와 최대 코스트(MAX_COST_CAP)를 표시
        // MAX_COST_CAP는 private const이므로, GameManager에서 값을 가져와야 합니다.
        // 임시로 10을 사용하거나, GameManager에 public const int MAX_COST_CAP을 정의하세요.
        int maxCost = 10; // 🚨 GameManager.cs의 MAX_COST_CAP을 참조하도록 변경 필요 🚨

        CostText.text = $"MP: {gameManager.CurrentCost} / {maxCost}";
    }
}
