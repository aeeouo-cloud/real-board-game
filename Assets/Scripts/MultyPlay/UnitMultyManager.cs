using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;
public class UnitMultyManager : NetworkBehaviour
{
    public NetworkObject player1;
    public NetworkObject player2;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            
            player1.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // 1. 현재 접속된 클라이언트 수를 확인한다.
        List<ulong> connectedIds = GameNetworkManager.ConnectedClientsID;
        
        // 2. 만약 플레이어가 2명 이상 접속했다면! (ID 0과 ID > 0인 클라이언트)
        if (connectedIds.Count >= 2)
        {
            ulong client2Id = connectedIds.FirstOrDefault(id => id != 0);

            if (client2Id != 0) //플레이어 2가 진짜 접속했는지 확인
            {   
                player2.ChangeOwnership(client2Id);
                player2.GetComponent<MultyUnit>().AssignedOwnerId.Value = client2Id;
                
                // 3. 할당이 끝났으면 이 콜백을 해제한다.
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}
