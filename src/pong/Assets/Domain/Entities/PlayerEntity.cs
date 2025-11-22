using System;

public class PlayerEntity
{
    public PlayerEntity(string id, string username, PlayerType playerType)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException("Id cannot be null, empty or whitespace");
        }

        this.Id = id;
        
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException("Username cannot be null, empty or whitespace");
        }
        
        this.Username = username;
        this.PlayerType = playerType;
    }

    public string Id { get; }

    public string Username { get; }

    public PlayerType PlayerType { get; set; }

    public int Score { get; private set; }

    public void ScorePoint() => this.Score++;
}
