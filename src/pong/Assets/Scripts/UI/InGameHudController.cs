using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InGameHudController : MonoBehaviour
{
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

    public void OnUiPrepared(List<LocalPlayer> players)
    {
        this.player1UsernameText.text = players.FirstOrDefault(player => player.PlayerType == PlayerType.Player1).Username;
        this.player2UsernameText.text = players.FirstOrDefault(player => player.PlayerType == PlayerType.Player2).Username;

        this.countdownTimerText.gameObject.SetActive(true);
        this.shouldBeginCountdown = true;
        this.remainingCountdownTime = Constants.CountdownTimeInSeconds;
    }

    private void Start()
    {
        OnlinePvpGameManager.ScoreChanged += this.OnScoreChanged;
        OnlinePvpGameManager.MatchEnded += this.OnGameEnded;
        LocalPvpGameManager.ScoreChanged += this.OnScoreChanged;
        LocalPvpGameManager.MatchEnded += this.OnGameEnded;
        LocalPvpGameManager.MainMenuLoaded += this.OnMainMenuLoaded;

        this.inGameAudioSource = this.GetComponent<AudioSource>();
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

    private IEnumerator ShowEndGamePanelCoroutine(GameOverStatistics gameOverStatistics)
    {
        yield return new WaitForSeconds(0.2f);
        this.inGameAudioSource.PlayOneShot(this.gameWonSound);

        this.endGameText.text = $"{gameOverStatistics.WinnerName} WINS!\n {gameOverStatistics.LoserName}, WANT A REMATCH?";
        this.navigatingToText.text = gameOverStatistics.NavigatingToMessage;
        this.endGamePanel.gameObject.SetActive(true);
    }
}
