using System;
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
    public event Action<bool> ShouldShowCountdownUi;
    public event Action UpdateLobbyUi;

    private const float LobbyUpdateIntervalInSeconds = 1.1f;

    [SerializeField]
    private RelayManager relayManager;

    [SerializeField]
    private GameObject onlinePvpGameManager;

    private Lobby localLobby;
    private bool shouldPing = false;
    private bool shouldStartBeginGameCountdown = false;
    private bool lobbyInstantiated = false;
    private float heartbeatTimer;
    private float lobbyUpdateTimer = LobbyUpdateIntervalInSeconds;
    private float beginGameCountdownTimer = Constants.CountdownTimeInSeconds;
    private IGameManager gameManager;

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
        
        if (this.shouldStartBeginGameCountdown)
        {
            this.HandleBeginGameCountdown();
        }

        this.HandleLobbyPollForUpdates();
    }

    public async Task<bool> SignIn(string profileName)
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
            {
                return true;
            }

            var options = new InitializationOptions();
            options.SetProfile(profileName);

            Debug.Log($"Set profile with name {profileName}");

            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            return true;
        }
        catch (AuthenticationException ex)
        {
            Debug.Log(ex);
            return false;
        }
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

            var onlinePvpGameManager = Instantiate(this.onlinePvpGameManager);
            this.gameManager = onlinePvpGameManager.GetComponent<IGameManager>();

            this.gameManager.OnPlayerJoined(localPlayer);

            this.gameManager.PlayerDisconnected += async (playerId, _) =>
            {
                await this.LeaveLobby(playerId);
            };

            this.lobbyInstantiated = true;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    public async Task<bool> JoinPrivateMatchByCode(string lobbyCode)
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

            this.localLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, lobbyOptions);
            await this.SubscribeToLobbyEvents(this.localLobby);

            var onlinePvpGameManager = Instantiate(this.onlinePvpGameManager);
            this.gameManager = onlinePvpGameManager.GetComponent<IGameManager>();

            this.gameManager.PlayerDisconnected += async (playerId, _) =>
            {
                await this.LeaveLobby(playerId);
            };

            foreach (var lobbyPlayer in this.localLobby.Players)
            {
                if (lobbyPlayer.Id != this.localLobby.HostId)
                {
                    var localPlayer = new LocalPlayer(lobbyPlayer.Id, lobbyPlayer.Data["playerName"].Value, PlayerType.Player2);

                    await this.relayManager.JoinRelay(this.localLobby.Data["relayCode"].Value);

                    this.gameManager.OnPlayerJoined(localPlayer);
                }
            }

            this.lobbyInstantiated = true;

            return true;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            return false;
        }
    }

    public async Task Ready(bool isReady)
    {
        try
        {
            this.shouldStartBeginGameCountdown = false;
            this.ShouldShowCountdownUi(false);

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

    private async Task LeaveLobby(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(this.localLobby.Id, playerId);
            this.localLobby = null;
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
        if (this.localLobby != null && this.lobbyInstantiated)
        {
            this.lobbyUpdateTimer -= Time.deltaTime;

            if (this.lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = LobbyUpdateIntervalInSeconds;
                this.lobbyUpdateTimer = lobbyUpdateTimerMax;

                try
                {
                    this.localLobby = await LobbyService.Instance.GetLobbyAsync(this.localLobby.Id);
                }
                catch (LobbyServiceException ex)
                {
                    this.localLobby = null;
                    Debug.Log(ex.Message);
                    return;
                }

                this.UpdateLobbyUi();

                if (this.localLobby.Players.Count != Constants.MaxPlayersCount || !this.localLobby.Players.All(player => bool.Parse(player.Data["isReady"].Value)))
                {
                    this.beginGameCountdownTimer = Constants.CountdownTimeInSeconds;
                    this.shouldStartBeginGameCountdown = false;
                    this.ShouldShowCountdownUi(false);
                    return;
                }

                if (!this.shouldStartBeginGameCountdown)
                {
                    this.beginGameCountdownTimer = Constants.CountdownTimeInSeconds;
                    this.shouldStartBeginGameCountdown = true;
                    this.ShouldShowCountdownUi(true);
                }
            }
        }
    }

    private void HandleBeginGameCountdown()
    {
        if (this.localLobby != null)
        {
            this.beginGameCountdownTimer -= Time.deltaTime;

            if (this.beginGameCountdownTimer < 0f)
            {
                float beginGameCountdownTimerMax = Constants.CountdownTimeInSeconds;
                this.beginGameCountdownTimer = beginGameCountdownTimerMax;

                this.gameManager.BeginGame();
                this.Ready(false).GetAwaiter();
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
                this.gameManager.OnPlayerJoined(localPlayer);
                this.UpdateLobbyUi();
            }
        };

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
    }
}
