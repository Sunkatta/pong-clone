using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameController : MonoBehaviour
{
    public GameManager gameManager;
    public RectTransform endGamePanel;
    public RectTransform pauseGamePanel;
    public TMP_Text endGameText;

    private void Start()
    {
        this.gameManager.GameEnded += this.OnGameEnded;
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
        this.endGamePanel.gameObject.SetActive(false);
        this.gameManager.NewGame();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
    }

    public void ResumeGame()
    {
        this.pauseGamePanel.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    private void OnGameEnded(PlayerType winner)
    {
        var winnerName = winner == PlayerType.Player1 ? "Player 1" : "Player 2";
        var loserName = winner == PlayerType.Player1 ? "Player 2" : "Player 1";

        this.endGameText.text = $"{winnerName} wins!\n {loserName}, want a rematch?";
        this.endGamePanel.gameObject.SetActive(true);
    }
}
