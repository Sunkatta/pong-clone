public interface IGetBallDirectionQuery
{
    public (float X, float Y) Execute(string gameId, string ballId);    
}
