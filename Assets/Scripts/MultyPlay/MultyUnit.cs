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
    public NetworkVariable<ulong> AssignedOwnerId = new NetworkVariable<ulong>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        unit = GetComponent<Unit>();
        AssignedOwnerId.OnValueChanged += SetupUnitMove;
        if (IsOwner)
        {
            UnitHP.Value = unit.CurrentHP;
            GetComponent<Move>().enabled = true;
        }

        if (!IsOwner)
        {
            unit.Type = Unit.UnitType.Enemy;
            GetComponent<Move>().enabled =false;
        }
        UnitPos.OnValueChanged += HandlePosChange;
    }
    public void SetGamemanagerUnit()
    {
        if (IsOwner)
        {
        GameManager.Instance.PlayerUnit = GetComponent<Unit>(); 
        }

    }
    void SetupUnitMove(ulong prev, ulong current)
    {
        if(current == NetworkManager.Singleton.LocalClientId)
        {
            unit.Type = Unit.UnitType.Player;
            GetComponent<Move>().enabled = true;
        }
        else
        {
            unit.Type = Unit.UnitType.Enemy;
            GetComponent<Move>().enabled =false;
        }
        SetGamemanagerUnit();
    }
    void HandlePosChange(Vector2Int previousValue, Vector2Int newValue)
    {
        OnPosChanged.Invoke(previousValue,newValue);
    }

    [Rpc (SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UnitPosChangedServerRpc(Vector2Int pos, ulong objectId)
    {
        Debug.Log("send serverRpc poschange");
        UnitPosChangedClientRpc(pos, objectId);
    }
    [Rpc (SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HPSyncServerRpc(int amount)
    {
        UnitHP.Value = amount;
        unit.CurrentHP = UnitHP.Value;
        HPSyncClientRpc(unit.CurrentHP);
    }
    [ClientRpc]
    public void UnitPosChangedClientRpc(Vector2Int pos, ulong changeobject , ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("recieved client Rpc poschange");
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(changeobject, out NetworkObject targetNetObj))
        {
            targetNetObj.GetComponent<Unit>().CurrentPosition = pos;
        }
    }

    [ClientRpc]
    public void HPSyncClientRpc(int amount, ClientRpcParams clientRpcParams = default)
    {
        unit.CurrentHP = amount;
    }
}
