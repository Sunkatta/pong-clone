using System;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

public class PlayerService : IDisposable
{
    private IMovePlayerUseCase movePlayerUseCase;
    private PlayerMovedDomainEventHandler playerMovedHandler;

    private static readonly Dictionary<ulong, AnticipatedNetworkTransform> clientIdToAnticipatedNetworkTransformMapping = new Dictionary<ulong, AnticipatedNetworkTransform>();
    private static readonly Dictionary<ulong, string> clientIdToPlayerIdMapping = new Dictionary<ulong, string>();

    public event Action<float, ulong> PlayerPositionUpdated;

    [Inject]
    public void Construct(IMovePlayerUseCase movePlayerUseCase, PlayerMovedDomainEventHandler playerMovedHandler)
    {
        this.movePlayerUseCase = movePlayerUseCase;
        this.playerMovedHandler = playerMovedHandler;

        this.playerMovedHandler.PlayerMoved += OnPlayerMoved;
    }

    public void HandleMoveInput(ulong clientId, AnticipatedNetworkTransform anticipatedNetworkTransform, float inputAxis)
    {
        if (!clientIdToPlayerIdMapping.TryGetValue(clientId, out string playerId))
        {
            throw new InvalidOperationException($"Cannot find corresponding Player Id for client with Id {clientId}");
        }

        if (!clientIdToAnticipatedNetworkTransformMapping.ContainsKey(clientId))
        {
            clientIdToAnticipatedNetworkTransformMapping[clientId] = anticipatedNetworkTransform;
        }

        var newY = anticipatedNetworkTransform.transform.position.y + inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;

        var movePlayerCommand = new MovePlayerCommand(
            GameManager.Instance.CurrentGameId,
            playerId,
            newY);

        this.movePlayerUseCase.Execute(movePlayerCommand);
    }

    public void ApplyRemotePosition(float newY)
    {
        //this.PlayerPositionUpdated(newY);
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

            if (!clientIdToAnticipatedNetworkTransformMapping.TryGetValue(kvp.Key, out AnticipatedNetworkTransform anticipatedNetworkTransform))
            {
                throw new InvalidOperationException($"Cannot find corresponding Anticipated Network Transform for client with Id {kvp.Key}");
            }

            anticipatedNetworkTransform.AnticipateMove(new Vector3(anticipatedNetworkTransform.transform.position.x, domainEvent.NewPlayerPosition.Y));
            anticipatedNetworkTransform.Smooth(anticipatedNetworkTransform.AnticipatedState, anticipatedNetworkTransform.AuthoritativeState, 0.1f);

            this.PlayerPositionUpdated(domainEvent.NewPlayerPosition.Y, kvp.Key);
        }
    }
}
