using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public async Task<string> CreateRelay()
    {
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(Constants.MaxPlayersCount - 1);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.NetworkConfig.TickRate = 60;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
				allocation.RelayServer.IpV4,
				(ushort)allocation.RelayServer.Port,
				allocation.AllocationIdBytes,
				allocation.Key,
				allocation.ConnectionData);

			return joinCode;
		}
		catch (RelayServiceException ex)
		{
			Debug.Log(ex);
			return null;
		}
    }

	public async Task JoinRelay(string joinCode)
	{
		try
		{
			JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.NetworkConfig.TickRate = 60;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
				joinAllocation.HostConnectionData);
        }
		catch (RelayServiceException ex)
		{
            Debug.Log(ex);
        }
	}
}
