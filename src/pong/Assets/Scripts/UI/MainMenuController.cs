using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject localPvpGameManager;

    [SerializeField]
    private RectTransform mainMenuPanel;

    [SerializeField]
    private RectTransform onlinePvpPanel;

    [SerializeField]
    private RectTransform lobbyPanel;

    [SerializeField]
    private RectTransform authPanel;

    [SerializeField]
    private RectTransform inGameHudPanel;

    [SerializeField]
    private RectTransform endGamePanel;

    [SerializeField]
    private Button localPvpBtn;

    [SerializeField]
    private Button onlinePvpBtn;

    [SerializeField]
    private Button quitGameBtn;

    [SerializeField]
    private TMP_Text player1ScoreText;

    [SerializeField]
    private TMP_Text player2ScoreText;

    [SerializeField]
    private TMP_Text endGameText;

    [SerializeField]
    private TMP_Text navigatingToText;

    [SerializeField]
    private AudioClip gameWonSound;

    [SerializeField]
    private AudioClip btnClickSound;

    private AudioSource mainMenuAudioSource;
    private IGameManager gameManager;

    private void Start()
    {
        this.mainMenuAudioSource = GetComponent<AudioSource>();
        this.mainMenuPanel.gameObject.SetActive(true);
        OnlinePvpGameManager.PrepareInGameUi += this.OnUiPrepared;
        OnlinePvpGameManager.ScoreChanged += this.OnScoreChanged;
        OnlinePvpGameManager.MatchEnded += this.OnGameEnded;
        LocalPvpGameManager.ScoreChanged += this.OnScoreChanged;
        LocalPvpGameManager.MatchEnded += this.OnGameEnded;
        LocalPvpGameManager.MainMenuLoaded += this.OnMainMenuLoaded;

        this.localPvpBtn.onClick.AddListener(() =>
        {
            this.mainMenuAudioSource.Play();
            var onlinePvpGameManager = Instantiate(this.localPvpGameManager);
            this.gameManager = onlinePvpGameManager.GetComponent<IGameManager>();

            var player1 = new LocalPlayer(Guid.NewGuid().ToString(), "Player 1", PlayerType.Player1);
            this.gameManager.OnPlayerJoined(player1);

            var player2 = new LocalPlayer(Guid.NewGuid().ToString(), "Player 2", PlayerType.Player2);
            this.gameManager.OnPlayerJoined(player2);

            this.gameManager.BeginGame();

            this.mainMenuPanel.gameObject.SetActive(false);
            this.inGameHudPanel.gameObject.SetActive(true);
        });

        this.onlinePvpBtn.onClick.AddListener(() =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            this.mainMenuPanel.gameObject.SetActive(false);
            this.authPanel.gameObject.SetActive(true);
        });

        this.quitGameBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.QuitGameCoroutine());
        });
    }

    private void OnMainMenuLoaded()
    {
        this.inGameHudPanel.gameObject.SetActive(false);
        this.endGamePanel.gameObject.SetActive(false);
        this.mainMenuPanel.gameObject.SetActive(true);
    }

    private IEnumerator QuitGameCoroutine()
    {
        this.mainMenuAudioSource.Play();
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }

    private void OnUiPrepared()
    {
        this.lobbyPanel.gameObject.SetActive(false);
        this.inGameHudPanel.gameObject.SetActive(true);
    }

    private void OnScoreChanged(int score, PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
        {
            this.player1ScoreText.text = score.ToString();
        }
        else
        {
            this.player2ScoreText.text = score.ToString();
        }
    }

    private void OnGameEnded(GameOverStatistics gameOverStatistics)
    {
        this.StartCoroutine(this.ShowEndGamePanelCoroutine(gameOverStatistics));
    }

    private IEnumerator ShowEndGamePanelCoroutine(GameOverStatistics gameOverStatistics)
    {
        yield return new WaitForSeconds(0.2f);
        this.mainMenuAudioSource.PlayOneShot(this.gameWonSound);

        this.endGameText.text = $"{gameOverStatistics.WinnerName} WINS!\n {gameOverStatistics.LoserName}, WANT A REMATCH?";
        this.navigatingToText.text = gameOverStatistics.NavigatingToMessage;
        this.endGamePanel.gameObject.SetActive(true);
    }
}
