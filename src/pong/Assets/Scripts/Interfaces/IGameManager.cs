using System;

public interface IGameManager
{
    public event Action PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;

    void BeginGame();

    void OnPlayerJoined(PlayerEntity player);

    void LeaveGame();
}
