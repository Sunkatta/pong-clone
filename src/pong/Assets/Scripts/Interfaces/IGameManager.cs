using System;
using System.Collections.Generic;

public interface IGameManager
{
    public event Action<List<LocalPlayer>> PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;

    void BeginGame();

    void OnPlayerJoined(LocalPlayer player);

    void LeaveGame();
}
