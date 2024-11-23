using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<LocalPlayer> PlayerJoined;
    public event Action<GameType> BeginGame;
    public event Action ShowCountdownUi;
    public event Action UpdateLobbyUi;

    [SerializeField]
    private RelayManager relayManager;

    private Lobby localLobby;
    private bool shouldPing = false;
    private bool shouldStartBeginGameCountdown = false;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    public string LobbyCode => this.localLobby.LobbyCode;

    public ICollection<Player> JoinedPlayers => this.localLobby.Players;

    public int MaxPlayers => this.localLobby.MaxPlayers;

    public Player LocalPlayer => this.localLobby.Players.FirstOrDefault(player => player.Id == AuthenticationService.Instance.PlayerId);

    private void Update()
    {
        if (this.shouldPing)
        {
            this.HandleLobbyHearbeat();
        }

        this.HandleLobbyPollForUpdates();
    }

    public async Task SignIn(string profileName)
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

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

            var createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.Profile) },
                        { "isReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, false.ToString()) },
                    }
                }
            };

            this.localLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, Constants.MaxPlayersCount, createLobbyOptions);
            this.shouldPing = true;

            await this.SubscribeToLobbyEvents(this.localLobby);

            var localPlayer = new LocalPlayer(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.Profile, PlayerType.Player1);

            string relayJoinCode = await this.relayManager.CreateRelay();

            this.localLobby = await LobbyService.Instance.UpdateLobbyAsync(this.localLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            this.PlayerJoined(localPlayer);
            this.UpdateLobbyUi();
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
                        { "isReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, false.ToString()) },
                    }
                }
            };

            this.localLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, lobbyOptions);
            await this.SubscribeToLobbyEvents(this.localLobby);

            foreach (var lobbyPlayer in this.localLobby.Players)
            {
                if (lobbyPlayer.Id != this.localLobby.HostId)
                {
                    var localPlayer = new LocalPlayer(lobbyPlayer.Id, lobbyPlayer.Data["playerName"].Value, PlayerType.Player2);

                    await this.relayManager.JoinRelay(this.localLobby.Data["relayCode"].Value);

                    this.PlayerJoined(localPlayer);
                }
            }

            this.UpdateLobbyUi();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task Ready(bool isReady)
    {
        try
        {
            var updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "isReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady.ToString()) },
                }
            };

            this.localLobby = await LobbyService.Instance.UpdatePlayerAsync(this.localLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(this.localLobby.Id, AuthenticationService.Instance.PlayerId);
            this.localLobby = null;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task ResetLocalPlayer()
    {
        try
        {
            this.shouldStartBeginGameCountdown = false;

            var updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "isReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, false.ToString()) },
                }
            };

            this.localLobby = await LobbyService.Instance.UpdatePlayerAsync(this.localLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
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

    private async void HandleLobbyPollForUpdates()
    {
        if (this.localLobby != null)
        {
            this.lobbyUpdateTimer -= Time.deltaTime;

            if (this.lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                this.lobbyUpdateTimer = lobbyUpdateTimerMax;

                this.localLobby = await LobbyService.Instance.GetLobbyAsync(this.localLobby.Id);

                this.UpdateLobbyUi();

                if (!this.shouldStartBeginGameCountdown && this.localLobby.Players.Count == Constants.MaxPlayersCount && this.localLobby.Players.All(player => bool.Parse(player.Data["isReady"].Value)))
                {
                    this.shouldStartBeginGameCountdown = true;
                    StartCoroutine(this.BeginGameCountdown());
                }
                // TODO: Figure out how to stop countdown when player switches to Not Ready
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
                var lobbyPlayer = playerChange.Player;
                this.localLobby.Players.Add(lobbyPlayer);
                var localPlayer = new LocalPlayer(lobbyPlayer.Id, lobbyPlayer.Data["playerName"].Value, PlayerType.Player2);
                this.PlayerJoined(localPlayer);
                this.UpdateLobbyUi();
            }
        };

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
    }

    private IEnumerator BeginGameCountdown()
    {
        this.ShowCountdownUi();

        yield return new WaitForSeconds(Constants.CountdownTimeInSeconds);

        this.BeginGame(GameType.OnlinePvp);
    }
}
