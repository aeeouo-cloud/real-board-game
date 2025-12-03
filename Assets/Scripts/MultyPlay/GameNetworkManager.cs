using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkManager : NetworkBehaviour
{
    public enum NetTurnState{waitingforplayer, hostturn, clientturn, offgame};
    public event Action<ulong> NetOnTurnChanged;
    public static List<ulong> ConnectedClientsID => NetworkManager.Singleton.ConnectedClientsIds.ToList();
    private readonly NetworkVariable<ulong> CurrentTurnClientId = new NetworkVariable<ulong>
    (
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<GameNetworkManager.NetTurnState> CurrentNetTurnStat = new NetworkVariable<GameNetworkManager.NetTurnState>
    (
        GameNetworkManager.NetTurnState.waitingforplayer,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CurrentTurnClientId.OnValueChanged += HandleTurnChange;
        if (IsServer)
        {
            if (ConnectedClientsID.Any())
            {
                CurrentTurnClientId.Value = ConnectedClientsID.First();
                CurrentNetTurnStat.Value = NetTurnState.hostturn;
                StartTurnClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams {TargetClientIds = new ulong[] {ConnectedClientsID.First()}}});
            }
        }
    }
    void HandleTurnChange(ulong prev, ulong current)
    {
        NetOnTurnChanged?.Invoke(current);
    }
    [Rpc (SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EndTurnServerRpc()
    {
        if (CurrentNetTurnStat.Value == NetTurnState.hostturn)
        {
            CurrentNetTurnStat.Value = NetTurnState.clientturn;
            CurrentTurnClientId.Value = ConnectedClientsID[1];
            StartTurnClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams {TargetClientIds = new ulong[] {ConnectedClientsID[1]}}});
        }
        else if (CurrentNetTurnStat.Value == NetTurnState.clientturn)
        {
            CurrentNetTurnStat.Value = NetTurnState.hostturn;
            CurrentTurnClientId.Value = ConnectedClientsID.First();
            StartTurnClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams {TargetClientIds = new ulong[] {ConnectedClientsID.First()}}});
        }
    }
    [ClientRpc]
    public void StartTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        GameManager.Instance.StartPlayerTurn();
    }
}
