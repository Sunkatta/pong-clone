using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class GameManager : MonoBehaviour
{
    private IObjectResolver resolver;

    [SerializeField]
    private float paddleSpeed;

    [SerializeField]
    private int targetScore;

    [SerializeField]
    private float ballInitialSpeed;

    [SerializeField]
    private float ballMaximumSpeed;

    [SerializeField]
    private GameObject localPlayerPrefab;

    public float PaddleSpeed => paddleSpeed;

    public int TargetScore => targetScore;

    public float BallInitialSpeed => ballInitialSpeed;

    public float BallMaximumSpeed => ballMaximumSpeed;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Global game state
    public string CurrentGameId { get; private set; }

    public GameType CurrentGameType { get; private set; }

    public string CurrentBallId { get; private set; }

    public string CurrentPlayer1Id { get; private set; }

    public string CurrentPlayer1Username { get; private set; }

    public string CurrentPlayer2Id { get; private set; }

    public string CurrentPlayer2Username { get; private set; }

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        this.resolver = resolver;
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetGameId(string newGameId)
    {
        if (this.CurrentGameId == newGameId)
        {
            return;
        }

        this.CurrentGameId = newGameId;
    }

    public void SetGameType(GameType newGameType)
    {
        if (this.CurrentGameType == newGameType)
        {
            return;
        }

        this.CurrentGameType = newGameType;
    }

    public void SetBallId(string newBallId)
    {
        if (this.CurrentGameId == newBallId)
        {
            return;
        }

        this.CurrentBallId = newBallId;
    }

    public void SetPlayer1(string player1Id, string player1Username)
    {
        if (this.CurrentPlayer1Id == player1Id)
        {
            return;
        }

        this.CurrentPlayer1Id = player1Id;
        this.CurrentPlayer1Username = player1Username;

        if (this.CurrentGameType == GameType.LocalPvp)
        {
            this.InstantiateLocalPlayer(player1Id, PlayerType.Player1);
        }
    }

    public void SetPlayer2(string player2Id, string player2Username)
    {
        if (this.CurrentPlayer2Id == player2Id)
        {
            return;
        }

        this.CurrentPlayer2Id = player2Id;
        this.CurrentPlayer2Username = player2Username;

        if (this.CurrentGameType == GameType.LocalPvp)
        {
            this.InstantiateLocalPlayer(player2Id, PlayerType.Player2);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("GameManager: Scene name is empty!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void ResetGame()
    {
        CurrentGameId = null;
        Debug.Log("GameManager: ResetGame called");
    }

    private void InstantiateLocalPlayer(string playerId, PlayerType playerType)
    {
        var playerGameObject = this.resolver.Instantiate(this.localPlayerPrefab);
        var playerInstanceController = playerGameObject.GetComponent<LocalPlayerController>();
        playerInstanceController.Type = playerType;
        playerInstanceController.Id = playerId;
        playerGameObject.transform.position = this.GetPlayerPosition(playerType);
    }

    private Vector3 GetPlayerPosition(PlayerType playerType)
    {
        Vector3 screenLeftSide = Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height / 2));
        Vector3 screenRightSide = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2));

        return playerType == PlayerType.Player1 ? new Vector3(screenLeftSide.x + .5f, 0) : new Vector3(screenRightSide.x - .5f, 0);
    }
}
