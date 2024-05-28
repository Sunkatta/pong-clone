using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameController : MonoBehaviour
{
    public GameManager gameManager;
    public RectTransform endGamePanel;
    public TMP_Text endGameText;

    void Start()
    {
        this.gameManager.GameEnded += this.OnGameEnded;
    }

    public void PlayAgain()
    {
        this.endGamePanel.gameObject.SetActive(false);
        this.gameManager.NewGame();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnGameEnded(PlayerType winner)
    {
        var winnerName = winner == PlayerType.Player1 ? "Player 1" : "Player 2";
        var loserName = winner == PlayerType.Player1 ? "Player 2" : "Player 1";

        this.endGameText.text = $"{winnerName} wins!\n {loserName}, want a rematch?";
        this.endGamePanel.gameObject.SetActive(true);
    }
}
