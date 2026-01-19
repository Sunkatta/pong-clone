using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class VContainerNetworkPrefabHandler : INetworkPrefabInstanceHandler
{
    private readonly IObjectResolver resolver;
    private readonly GameObject prefab;

    public VContainerNetworkPrefabHandler(
        IObjectResolver resolver,
        GameObject prefab)
    {
        this.resolver = resolver;
        this.prefab = prefab;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        var instance = resolver.Instantiate(
            prefab,
            position,
            rotation);

        if (!instance.TryGetComponent<NetworkObject>(out var netObj))
        {
            Debug.LogError($"Prefab {prefab.name} is missing a NetworkObject!");
            return null;
        }

        return netObj;
    }

    public void Destroy(NetworkObject networkObject)
    {
        Object.Destroy(networkObject);
    }
}
