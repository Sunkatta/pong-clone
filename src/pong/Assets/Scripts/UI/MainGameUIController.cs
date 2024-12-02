using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGameUIController : MonoBehaviour
{
    [SerializeField]
    private OnlinePvpGameManager gameManager;

    [SerializeField]
    private RectTransform endGamePanel;

    [SerializeField]
    private RectTransform pauseGamePanel;

    [SerializeField]
    private TMP_Text endGameText;

    [SerializeField]
    private AudioClip btnClickSound;

    [SerializeField]
    private AudioClip gameWonSound;

    [SerializeField]
    private Button endGamePanelBackToMainMenuBtn;

    [SerializeField]
    private Button pauseGamePanelBackToMainMenuBtn;

    [SerializeField]
    private Button resumeGameBtn;

    [SerializeField]
    private Button playAgainBtn;

    private AudioSource endGameAudio;

    private void Start()
    {
        this.endGameAudio = GetComponent<AudioSource>();
        // this.gameManager.MatchEnded += this.OnGameEnded;

        this.endGamePanelBackToMainMenuBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.GoToMainMenuCoroutine());
        });

        this.pauseGamePanelBackToMainMenuBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.GoToMainMenuCoroutine());
        });

        this.resumeGameBtn.onClick.AddListener(() =>
        {
            this.ResumeGame();
        });

        this.playAgainBtn.onClick.AddListener(() =>
        {
            this.PlayAgain();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            this.pauseGamePanel.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void PlayAgain()
    {
        this.endGameAudio.PlayOneShot(this.btnClickSound);
        this.endGamePanel.gameObject.SetActive(false);
        this.gameManager.NewGame();
    }

    public void ResumeGame()
    {
        this.endGameAudio.PlayOneShot(this.btnClickSound);
        this.pauseGamePanel.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    private void OnGameEnded(PlayerType winner)
    {
        this.StartCoroutine(this.ShowEndGamePanelCoroutine(winner));
    }

    private IEnumerator ShowEndGamePanelCoroutine(PlayerType winner)
    {
        yield return new WaitForSeconds(0.2f);
        this.endGameAudio.PlayOneShot(this.gameWonSound);
        var winnerName = winner == PlayerType.Player1 ? "Player 1" : "Player 2";
        var loserName = winner == PlayerType.Player1 ? "Player 2" : "Player 1";

        this.endGameText.text = $"{winnerName} wins!\n {loserName}, want a rematch?";
        this.endGamePanel.gameObject.SetActive(true);
    }

    private IEnumerator GoToMainMenuCoroutine()
    {
        Time.timeScale = 1;
        this.endGameAudio.PlayOneShot(this.btnClickSound);
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("MainMenu");
    }
}
