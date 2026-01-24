using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PlayerService : IDisposable
{
    private IMovePlayerUseCase movePlayerUseCase;
    private IJoinGameUseCase joinGameUseCase;
    private PlayerMovedDomainEventHandler playerMovedHandler;

    private static readonly Dictionary<ulong, Rigidbody2D> clientIdToRigidbodyMapping = new Dictionary<ulong, Rigidbody2D>();
    private static readonly Dictionary<ulong, string> clientIdToPlayerIdMapping = new Dictionary<ulong, string>();

    public event Action<float> PlayerPositionUpdated;

    [Inject]
    public void Construct(IMovePlayerUseCase movePlayerUseCase, IJoinGameUseCase joinGameUseCase, PlayerMovedDomainEventHandler playerMovedHandler)
    {
        this.movePlayerUseCase = movePlayerUseCase;
        this.joinGameUseCase = joinGameUseCase;
        this.playerMovedHandler = playerMovedHandler;

        this.playerMovedHandler.PlayerMoved += OnPlayerMoved;
    }

    public void HandleJoinGameRequest()
    {
        //var joinGameCommand = new JoinGameCommand();
    }

    public void HandleMoveInput(ulong clientId, Rigidbody2D rb, float inputAxis)
    {
        if (!clientIdToPlayerIdMapping.TryGetValue(clientId, out string playerId))
        {
            throw new InvalidOperationException($"Cannot find corresponding Player Id for client with Id {clientId}");
        }

        if (!clientIdToRigidbodyMapping.ContainsKey(clientId))
        {
            clientIdToRigidbodyMapping[clientId] = rb;
        }

        float newY = rb.position.y + inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;

        var movePlayerCommand = new MovePlayerCommand(
            GameManager.Instance.CurrentGameId,
            playerId,
            newY);

        this.movePlayerUseCase.Execute(movePlayerCommand);
    }

    public void ApplyRemotePosition(float newY)
    {
        this.PlayerPositionUpdated(newY);
    }

    public void RegisterPlayerId(string playerId, ulong clientId)
    {
        if (!clientIdToPlayerIdMapping.TryGetValue(clientId, out _))
        {
            clientIdToPlayerIdMapping[clientId] = playerId;
        }
    }

    public void Dispose()
    {
        if (this.playerMovedHandler != null)
        {
            this.playerMovedHandler.PlayerMoved -= OnPlayerMoved;
        }
    }

    private void OnPlayerMoved(PlayerMovedDomainEvent domainEvent)
    {
        foreach (var kvp in clientIdToPlayerIdMapping)
        {
            if (kvp.Value != domainEvent.PlayerId)
            {
                continue;
            }

            PlayerRpcBridgeService.SendPositionToClient(kvp.Key, domainEvent.NewPlayerPosition.Y);
        }
    }
}
