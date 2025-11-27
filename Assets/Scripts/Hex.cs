using UnityEngine;
using System;

public class Hex : MonoBehaviour
{
    // 🚨 [핵심] Map.cs와 Move.cs에서 사용하는 변수 정의 🚨
    public int cost;
    public bool isselectable;
    public Vector2Int qr;
    public bool iswall; // 벽 여부

    // 🚨 [추가] 맵 생성 시 좌표 및 월드 위치를 설정하는 함수 🚨
    public void SetPosition(int q, int r)
    {
        this.qr = new Vector2Int(q, r);

        // 월드 위치 계산 (Map.cs의 정적 벡터 q, r, hexsize를 사용한다고 가정)
        if (Map.instance != null)
        {
            float x = q * Map.q.x * Map.hexsize + r * Map.r.x * Map.hexsize;
            float z = q * Map.q.z * Map.hexsize + r * Map.r.z * Map.hexsize;

            // Hex 오브젝트의 월드 위치를 설정합니다.
            this.transform.position = new Vector3(x, 0, z);
        }
        else
        {
            Debug.LogError("SetPosition: Map.instance를 찾을 수 없습니다. 맵 초기화 순서를 확인하세요.");
        }
    }

    // 🚨 [수정] 마우스 클릭 감지 함수 (타일 타겟팅 입력 처리) 🚨
    void OnMouseDown()
    {
        // 1. 현재 게임 상태가 타일 타겟팅 대기 상태인지 확인합니다.
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.WaitingForTileTarget)
        {
            // 2. 이 타일이 현재 타겟팅 범위 내에 하이라이트되어 있는지 확인합니다.
            if (isselectable)
            {
                // 3. 유효한 타일이므로 GameManager에게 좌표를 전달하고 타겟팅을 해결하도록 명령합니다.
                GameManager.Instance.ResolveTileTargeting(qr);

                Debug.Log($"[Hex Input] 유효 타일 클릭 감지! 좌표 {qr}를 GameManager에 전달.");
            }
            else
            {
                // 🚨 [추가] 범위 밖의 타일을 클릭하면 타겟팅을 취소합니다. 🚨
                GameManager.Instance.CancelTargeting();
                Debug.LogWarning($"[Hex Input] 선택 불가능한 타일 {qr}이 클릭되었습니다. 타겟팅 취소.");
            }
        }
        // 이 외의 상태(일반 액션 페이즈 등)에서의 클릭은 Move.cs가 처리합니다.
    }
}
