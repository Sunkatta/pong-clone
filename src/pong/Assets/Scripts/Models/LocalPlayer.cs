public class LocalPlayer
{
    public LocalPlayer(string id, string username, PlayerType playerType)
    {
        this.Id = id;
        this.Username = username;
        this.PlayerType = playerType;
    }

    public string Id { get; }

    public string Username { get; }

    public PlayerType PlayerType { get; }
}
