public interface IGameManager
{
    void BeginGame();

    void OnPlayerJoined(LocalPlayer player);

    void OnPlayerLeft(string playerId);
}
