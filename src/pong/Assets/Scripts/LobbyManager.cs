using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private bool shouldPing = false;
    private float heartbeatTimer;

    public string LobbyCode
    {
        get
        {
            return this.hostLobby.LobbyCode;
        }
    }

    public int JoinedPlayers
    {
        get
        {
            return this.hostLobby.Players.Count;
        }
    }

    public int MaxPlayers
    {
        get
        {
            return this.hostLobby.MaxPlayers;
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        if (shouldPing)
        {
            HandleLobbyHearbeat();
        }
    }

    public async Task HostPrivateMatch()
    {
        try
        {
            string lobbyName = Guid.NewGuid().ToString();
            int maxPlayers = 2;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            this.hostLobby = lobby;
            this.shouldPing = true;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    public async Task JoinPrivateMatchByCode(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void HandleLobbyHearbeat()
    {
        if (this.hostLobby != null)
        {
            this.heartbeatTimer -= Time.deltaTime;

            if (this.heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                this.heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(this.hostLobby.Id);
            }
        }
    }
}
