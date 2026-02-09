using System.Collections;
using TMPro;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(AudioSource))]
public class InGameHudController : MonoBehaviour
{
    private PlayerScoredDomainEventHandler playerScoredDomainEventHandler;
    private PlayerWonDomainEventHandler playerWonDomainEventHandler;

    [SerializeField]
    private RectTransform mainMenuPanel;

    [SerializeField]
    private RectTransform endGamePanel;

    [SerializeField]
    private TMP_Text player1ScoreText;

    [SerializeField]
    private TMP_Text player2ScoreText;

    [SerializeField]
    private TMP_Text player1UsernameText;

    [SerializeField]
    private TMP_Text player2UsernameText;

    [SerializeField]
    private TMP_Text endGameText;

    [SerializeField]
    private TMP_Text navigatingToText;

    [SerializeField]
    private TMP_Text countdownTimerText;

    [SerializeField]
    private AudioClip gameWonSound;

    private AudioSource inGameAudioSource;

    private bool shouldBeginCountdown;

    private float remainingCountdownTime;

    private OnlinePvpGameManager gameManager;

    [Inject]
    public void Construct(PlayerScoredDomainEventHandler playerScoredDomainEventHandler,
        PlayerWonDomainEventHandler playerWonDomainEventHandler)
    {
        this.playerScoredDomainEventHandler = playerScoredDomainEventHandler;
        this.playerWonDomainEventHandler = playerWonDomainEventHandler;
    }

    public void OnUiPrepared()
    {
        this.countdownTimerText.gameObject.SetActive(true);
        this.shouldBeginCountdown = true;
        this.remainingCountdownTime = Constants.CountdownTimeInSeconds;
    }

    private void Start()
    {
        this.inGameAudioSource = this.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        this.playerWonDomainEventHandler.PlayerWon += this.OnGameEnded;
        this.playerScoredDomainEventHandler.PlayerScored += this.OnScoreChanged;
        
        if (GameManager.Instance.CurrentGameType == GameType.OnlinePvp)
        {
            var gameManagerGameObject = GameObject.Find("OnlinePvpGameManager(Clone)");
            this.gameManager = gameManagerGameObject.GetComponent<OnlinePvpGameManager>();

            this.gameManager.Player1Score.OnValueChanged += (_, newValue) => this.player1ScoreText.text = newValue.ToString();
            this.gameManager.Player2Score.OnValueChanged += (_, newValue) => this.player2ScoreText.text = newValue.ToString();
            this.gameManager.MatchEnded += this.OnGameEnded;
        }
        
        this.player1ScoreText.text = "0";
        this.player2ScoreText.text = "0";

        this.player1UsernameText.text = GameManager.Instance.CurrentPlayer1Username;
        this.player2UsernameText.text = GameManager.Instance.CurrentPlayer2Username;

        this.countdownTimerText.gameObject.SetActive(true);
        this.shouldBeginCountdown = true;
        this.remainingCountdownTime = Constants.CountdownTimeInSeconds;
    }

    private void OnDisable()
    {
        this.playerWonDomainEventHandler.PlayerWon -= this.OnGameEnded;
        this.playerScoredDomainEventHandler.PlayerScored -= this.OnScoreChanged;

        if (GameManager.Instance.CurrentGameType == GameType.OnlinePvp)
        {
            this.gameManager.Player1Score.OnValueChanged -= (_, newValue) => this.player1ScoreText.text = newValue.ToString();
            this.gameManager.Player2Score.OnValueChanged -= (_, newValue) => this.player2ScoreText.text = newValue.ToString();
            this.gameManager.MatchEnded -= this.OnGameEnded;
        }
    }

    private void Update()
    {
        if (this.shouldBeginCountdown)
        {
            if (this.remainingCountdownTime > 0)
            {
                this.remainingCountdownTime -= Time.deltaTime;
            }
            else if (this.remainingCountdownTime < 0)
            {
                this.remainingCountdownTime = 0;
                this.shouldBeginCountdown = false;
                this.countdownTimerText.gameObject.SetActive(false);
            }

            int seconds = Mathf.FloorToInt(this.remainingCountdownTime % 60);

            this.countdownTimerText.text = $"{seconds}";
        }
    }

    private void OnScoreChanged(PlayerScoredDomainEvent playerScoredDomainEvent)
    {
        if (playerScoredDomainEvent.PlayerType == PlayerType.Player1)
        {
            this.player1ScoreText.text = playerScoredDomainEvent.PlayerNewScore.ToString();
        }
        else
        {
            this.player2ScoreText.text = playerScoredDomainEvent.PlayerNewScore.ToString();
        }
    }

    private void OnMainMenuLoaded()
    {
        this.gameObject.SetActive(false);
        this.endGamePanel.gameObject.SetActive(false);
        this.mainMenuPanel.gameObject.SetActive(true);
    }

    private void OnGameEnded(GameOverStatistics gameOverStatistics)
    {
        this.StartCoroutine(this.ShowEndGamePanelCoroutine(gameOverStatistics));
    }

    private void OnGameEnded(PlayerWonDomainEvent playerWonDomainEvent)
    {
        var gameOverStatistics = new GameOverStatistics
        {
            WinnerName = playerWonDomainEvent.WinnerPlayerUsername,
            LoserName = playerWonDomainEvent.LoserPlayerUsername,
            NavigatingToMessage = Constants.ReturningToMainMenuText,
        };

        this.StartCoroutine(this.ShowEndGamePanelCoroutine(gameOverStatistics));
    }

    private IEnumerator ShowEndGamePanelCoroutine(GameOverStatistics gameOverStatistics)
    {
        yield return new WaitForSeconds(0.2f);
        this.inGameAudioSource.PlayOneShot(this.gameWonSound);

        this.endGameText.text = $"{gameOverStatistics.WinnerName} WINS!\n {gameOverStatistics.LoserName}, WANT A REMATCH?";
        this.navigatingToText.text = gameOverStatistics.NavigatingToMessage;
        this.endGamePanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(5);

        OnMainMenuLoaded();
    }
}
