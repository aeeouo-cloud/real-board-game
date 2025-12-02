using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class MultyUnit : NetworkBehaviour
{
    public event Action<Vector2Int, Vector2Int> OnPosChanged;
    Unit unit;
    public NetworkVariable<Vector2Int> UnitPos = new NetworkVariable<Vector2Int>
    (
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<int> UnitHP = new NetworkVariable<int>
    (
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        unit = GetComponent<Unit>();
        if (!IsOwner)
        {
            unit.Type = Unit.UnitType.Enemy;
            Destroy(GetComponent<Move>());
        }
        UnitPos.OnValueChanged += HandlePosChange;
    }
    void HandlePosChange(Vector2Int previousValue, Vector2Int newValue)
    {
        OnPosChanged.Invoke(previousValue,newValue);
    }
}
