public interface IGameManager
{
    void BeginGame();

    void OnPlayerJoined(LocalPlayer player);

    void OnPlayerLeft(string playerId);

    void OnPlayerScored(PlayerType scorer);

    void OnBallHit();

    void EndGame(string winnerName, string loserName);
}
