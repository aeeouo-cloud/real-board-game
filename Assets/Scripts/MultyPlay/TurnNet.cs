using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TurnNet : NetworkBehaviour
{
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnObjectWithOwnerServerRpc(string addresskey, RpcParams rpcParams = default)
    {
        Debug.Log($"spawnobjectowner called {addresskey}");
        ulong clientID = rpcParams.Receive.SenderClientId;
        _= SpawnObjectWithOwner(addresskey,clientID);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DestroyTargetServerRpc(ulong targetobjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetobjectId, out NetworkObject networkObject))
        {
            Destroy(networkObject.gameObject);
        }
    }
    public async Task SpawnObjectWithOwner(string addresskey,ulong clientID)
    {
        var loadHandle = Addressables.LoadAssetAsync<GameObject>(addresskey);
        await loadHandle.Task;
        if (loadHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
        Debug.LogError($"서버 Addressable 로드 실패: {addresskey}");
        return;
        }
        var prefab = loadHandle.Result;
        GameObject newobject = Instantiate(prefab, new Vector3(0, 8, 0), Quaternion.identity);
        newobject.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
        PostSpawnSetupClientRpc(newobject.GetComponent<NetworkObject>().NetworkObjectId); 
    
        Addressables.Release(loadHandle);
    }

    [ClientRpc]
    private void PostSpawnSetupClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject spawnedNetObject))
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObject) && netObject.IsOwner)
            {
                TurnManager manager = FindAnyObjectByType<TurnManager>(); 

                if (manager != null && spawnedNetObject.gameObject.TryGetComponent(out Dice diceComponent))
                {
                    diceComponent.turnmanager = manager;
                }
            }
            else
            {
                netObject.gameObject.GetComponent<Dice>().enabled = false;
            }

        }
    }
}
