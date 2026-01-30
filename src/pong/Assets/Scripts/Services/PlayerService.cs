using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PlayerService : IDisposable
{
    private IMovePlayerUseCase movePlayerUseCase;
    private PlayerMovedDomainEventHandler playerMovedHandler;

    private static readonly Dictionary<ulong, string> clientIdToPlayerIdMapping = new Dictionary<ulong, string>();

    public event Action<float, ulong> PlayerPositionUpdated;

    [Inject]
    public void Construct(IMovePlayerUseCase movePlayerUseCase, PlayerMovedDomainEventHandler playerMovedHandler)
    {
        this.movePlayerUseCase = movePlayerUseCase;
        this.playerMovedHandler = playerMovedHandler;

        this.playerMovedHandler.PlayerMoved += OnPlayerMoved;
    }

    public void HandleMoveInput(ulong clientId, Transform transform, float inputAxis)
    {
        if (!clientIdToPlayerIdMapping.TryGetValue(clientId, out string playerId))
        {
            throw new InvalidOperationException($"Cannot find corresponding Player Id for client with Id {clientId}");
        }

        float newY = transform.position.y + inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;

        var movePlayerCommand = new MovePlayerCommand(
            GameManager.Instance.CurrentGameId,
            playerId,
            newY);

        this.movePlayerUseCase.Execute(movePlayerCommand);
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

            this.PlayerPositionUpdated(domainEvent.NewPlayerPosition.Y, kvp.Key);
        }
    }
}
