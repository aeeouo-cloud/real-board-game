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
        // 유닛 오브젝트에 Hex 컴포넌트가 아닌, Map에 Hex 정보가 있으므로 Unit의 CurrentPosition을 사용해야 합니다.
        // 현재 유닛의 위치를 기반으로 맵에서 Hex 객체를 찾아야 합니다.
        // 이 로직은 Map.cs에 GetHexAt(Vector2Int coord) 함수가 필요합니다.

        // *임시 수정: Unit 오브젝트가 World Position과 Hex 좌표를 모두 가지고 있다고 가정*

        // Hex currentHex = unit.GetComponent<Hex>(); // 👈 이 코드는 유닛에 Hex 컴포넌트가 붙어있지 않으면 오류

        // 🚨 2. Map.cs의 정적 벡터(q, hexsize)를 사용하여 월드 위치 변화량을 계산합니다. 🚨
        // (Map 클래스가 static public 필드(q, hexsize)를 가지고 있다고 가정)
        // 이 로직은 유닛을 Q축 양의 방향으로만 이동시킨다고 가정하는 임시 이동 로직입니다.

        // Hex 타일 시스템에서는 월드 위치가 아닌, 헥스 좌표(qr)를 기반으로 이동해야 합니다.
        // MapMovementHelper는 이 부분을 담당하기 어렵습니다. Unit.cs의 Move(int distance) 함수가 제거되었으므로,
        // 이 함수 자체의 로직을 안전하게 수정해야 합니다.

        // *이동 계산 로직을 월드 위치 대신 헥스 좌표로 대체하겠습니다.*

        // 현재 좌표와 이동 거리를 사용하여 목표 헥스 좌표를 계산 (임시: Q축으로만 이동)
        Vector2Int newHexCoord = unit.CurrentPosition + new Vector2Int(distance, 0);

        // 3. Map 인스턴스를 통해 목표 Hex의 월드 위치를 얻습니다. (Map.cs에 GetWorldPositionAt 함수가 필요함)
        // **팀원 코드를 사용하지 않기 위해 임시로 월드 위치를 계산하는 로직은 제거하고, 논리적 좌표만 업데이트합니다.**

        // **************** 안전하게 주석 처리 및 논리적 위치 업데이트 ****************
        // 맵 이동은 Move.cs가 유저 클릭 시 이미 처리하므로, 여기서는 논리적 좌표만 업데이트합니다.

        // unit.Move(distance); // 👈 제거된 함수 호출 제거

        // 유닛의 논리적 위치(CurrentPosition) 업데이트
        unit.SetPosition(newHexCoord);

        // **************** (Move.cs가 실제 이동을 처리하므로 이 코드는 단순 기록용으로 남깁니다) ****************

        Debug.Log($"[MapHelper] {unit.UnitName}이(가) {distance}칸 이동 명령을 받았습니다. 목표 논리적 좌표: {newHexCoord}");
        return true;
    }
}
