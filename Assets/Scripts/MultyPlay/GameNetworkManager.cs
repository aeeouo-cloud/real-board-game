using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using System;
public class GameNetworkManager : NetworkBehaviour
{
    public event Action<ulong> OnTurnChanged;
    static  List<ulong> ConnectedClientsID => NetworkManager.Singleton.ConnectedClientsIds.ToList();
    GameManager gameManager;
    private readonly NetworkVariable<ulong> CurrentTurnClientId = new NetworkVariable<ulong>
    (
        default,
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
            }
        }
        gameManager = this.GetComponent<GameManager>();
    }
    void HandleTurnChange(ulong prev, ulong current)
    {
        OnTurnChanged?.Invoke(current);
    }
    [ServerRpc]
    public void EndTurnServerRpc()
    {
        StartTurnClientRpc();
    }
    [ClientRpc]
    public void StartTurnClientRpc()
    {
        
    }
}
