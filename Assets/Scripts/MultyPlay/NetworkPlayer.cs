using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayer : NetworkBehaviour
{
    NetworkVariable<int> num = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        Debug.Log($"is owner - {IsOwner} Client Id  = {OwnerClientId}");
        num.OnValueChanged += (int prevalue, int newvalue) =>
        {
            Debug.Log($"is owner - {IsOwner} Client Id  = {OwnerClientId} num = {num.Value}");
        };
    }
    void Update()
    {
        if(!IsOwner) return;
        if (Input.GetKeyUp(KeyCode.A))
        {
            num.Value = Random.Range(0,100);
        }
    }
}
