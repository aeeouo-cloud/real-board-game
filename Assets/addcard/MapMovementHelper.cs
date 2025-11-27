// MapMovementHelper.cs
using UnityEngine;

// Map.cs 파일이 같은 네임스페이스에 있거나 접근 가능하다고 가정
// using YourProject.MapSystem; // 만약 Map 클래스가 특정 네임스페이스에 있다면 추가 필요

public static class MapMovementHelper
{
    // GameManager에서 호출되어 유닛을 이동시키는 핵심 함수
    // 이 로직은 현재 타일에서 한 방향(Q축)으로 distance만큼 이동하는 것을 시뮬레이션합니다.
    public static bool TryMovePlayerUnit(Unit unit, int distance)
    {
        if (unit == null)
        {
            Debug.LogError("[MapHelper] 이동할 유닛 오브젝트가 null입니다.");
            return false;
        }

        // 1. 현재 유닛이 어떤 Hex 타일에 있는지 참조합니다.
        Hex currentHex = unit.GetComponent<Hex>();
        if (currentHex == null)
        {
            Debug.LogError("[MapHelper] 유닛에 Hex 컴포넌트가 없어 맵 좌표 계산이 불가능합니다.");
            return false;
        }

        // 🚨 2. Map.cs의 정적 벡터(q, hexsize)를 사용하여 월드 위치 변화량을 계산합니다. 🚨
        // (Map 클래스가 static public 필드(q, hexsize)를 가지고 있다고 가정)
        // 이 로직은 유닛을 Q축 양의 방향으로만 이동시킨다고 가정하는 임시 이동 로직입니다.

        // q 벡터를 distance와 hexsize만큼 곱하여 실제 월드 변화량을 구합니다.
        Vector3 worldOffset = Map.q * distance * Map.hexsize;

        // 3. 최종 위치 계산 및 할당
        // 현재 유닛의 위치 + 계산된 Hex 오프셋 + 유닛이 타일 위로 띄워진 높이(0.5f)
        Vector3 newPosition = currentHex.transform.position + worldOffset + new Vector3(0f, 0.5f, 0f);

        unit.transform.position = newPosition;

        // 4. (TODO) 유닛의 Hex.cs 컴포넌트에 현재 Q/R 좌표를 업데이트하는 로직 필요
        // unit.GetComponent<Hex>().UpdateQR(currentHex.qr.x + distance, currentHex.qr.y);

        unit.Move(distance); // 유닛의 논리적 위치 (CurrentPosition) 업데이트

        Debug.Log($"[MapHelper] {unit.UnitName}이(가) {distance}칸 이동했습니다. New Position: {newPosition}");
        return true;
    }
}