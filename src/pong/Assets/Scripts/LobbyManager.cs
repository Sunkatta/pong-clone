using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public string LobbyCode { get; private set; }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task InitPrivateMatch()
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
            this.LobbyCode = lobby.LobbyCode;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
