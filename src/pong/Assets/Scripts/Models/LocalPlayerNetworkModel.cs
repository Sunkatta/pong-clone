using Unity.Netcode;

public class LocalPlayerNetworkModel : INetworkSerializable
{
    private string id;
    private string username;
    private PlayerType playerType;

    /// <summary>
    /// NGO requires this.
    /// </summary>
    public LocalPlayerNetworkModel()
    {
    }

    public LocalPlayerNetworkModel(string id, string username, PlayerType playerType)
    {
        this.id = id;
        this.username = username;
        this.playerType = playerType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.id);
        serializer.SerializeValue(ref this.username);
        serializer.SerializeValue(ref this.playerType);
    }

    public string GetId() => this.id;

    public string GetUsername() => this.username;

    public PlayerType GetPlayerType() => this.playerType;
}
