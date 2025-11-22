using System;
using System.Collections.Generic;

public interface IGameManager
{
    public event Action<List<PlayerEntity>> PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;

    void BeginGame();

    void OnPlayerJoined(PlayerEntity player);

    void LeaveGame();
}
