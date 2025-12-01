using System;
using Unity.Netcode;
using UnityEngine;

public class MultyUnit : NetworkBehaviour
{
    public event Action<Vector2Int, Vector2Int> OnPosChanged;
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

        UnitPos.OnValueChanged += HandlePosChange;
    }
    void HandlePosChange(Vector2Int previousValue, Vector2Int newValue)
    {
        OnPosChanged.Invoke(previousValue,newValue);
    }
}
