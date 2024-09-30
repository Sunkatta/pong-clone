using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<PlayerType> PlayerJoined;
    public event Action<GameType> BeginGame;
    public event Action UpdateLobbyUiOnPlayerJoined;

    private Lobby localLobby;
    private bool shouldPing = false;
    private float heartbeatTimer;

    public string LobbyCode => this.localLobby.LobbyCode;

    public ICollection<Player> JoinedPlayers => this.localLobby.Players;

    public string LobbyStatusMessage
    {
        get
        {
            if (this.localLobby.Players.Count < this.localLobby.MaxPlayers)
            {
                return $"{this.localLobby.Players.Count}/{this.localLobby.MaxPlayers} players joined...";
            }
            else
            {
                return "All players have joined! Match will begin shortly!";
            }
        }
    }

    private void Update()
    {
        if (this.shouldPing)
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
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.Profile) },
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, Constants.MaxPlayersCount, createLobbyOptions);

            this.localLobby = lobby;
            this.shouldPing = true;

            await this.SubscribeToLobbyEvents(this.localLobby);

            this.PlayerJoined(PlayerType.Player1);
            this.UpdateLobbyUiOnPlayerJoined();
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
            var lobbyOptions = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.Profile) },
                    }
                }
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, lobbyOptions);

            this.localLobby = lobby;

            await this.SubscribeToLobbyEvents(this.localLobby);

            this.PlayerJoined(PlayerType.Player2);
            this.UpdateLobbyUiOnPlayerJoined();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void HandleLobbyHearbeat()
    {
        if (this.localLobby != null)
        {
            this.heartbeatTimer -= Time.deltaTime;

            if (this.heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                this.heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(this.localLobby.Id);
            }
        }
    }

    private async Task SubscribeToLobbyEvents(Lobby lobby)
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.PlayerJoined += playerChanges =>
        {
            foreach (var playerChange in playerChanges)
            {
                this.localLobby.Players.Add(playerChange.Player);
                this.UpdateLobbyUiOnPlayerJoined();

                if (this.localLobby.Players.Count == Constants.MaxPlayersCount)
                {
                    // TODO: Check that everyone is ready and then begin match
                    // this.BeginGame(GameType.OnlinePvp);
                }
            }
        };

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
    }
}
