using System;
using System.Threading.Tasks;
using Unity.Netcode;
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

    private void Update()
    {
        if (shouldPing)
        {
            HandleLobbyHearbeat();
        }
    }

    public async Task SignIn(string profileName)
    {
        var options = new InitializationOptions();
        options.SetProfile(profileName);

        Debug.Log($"Set profile with name {profileName}");

        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
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

            NetworkManager.Singleton.StartHost();
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
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            this.hostLobby = lobby;

            NetworkManager.Singleton.StartClient();
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
