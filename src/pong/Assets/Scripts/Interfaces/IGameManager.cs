using System;
using System.Collections.Generic;

public interface IGameManager
{
    public event Action<List<LocalPlayer>> PrepareInGameUi;

    void BeginGame();

    void OnPlayerJoined(LocalPlayer player);

    void OnPlayerLeft(string playerId);
}
