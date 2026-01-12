using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager to handle global game state like CurrentGameId.
/// Persists across scenes and is safe for multiple accesses.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Global game state
    public string CurrentGameId { get; private set; }

    public string CurrentBallId { get; private set; }

    // Optional: Event to notify when GameId changes
    public delegate void GameIdChanged(string newId);
    public event GameIdChanged OnGameIdChanged;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Only one instance allowed
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    /// <summary>
    /// Sets the current GameId and triggers event.
    /// </summary>
    public void SetGameId(string newGameId)
    {
        if (CurrentGameId == newGameId) return; // No change
        CurrentGameId = newGameId;
        OnGameIdChanged?.Invoke(CurrentGameId);
        Debug.Log($"GameManager: CurrentGameId set to {CurrentGameId}");
    }

    /// <summary>
    /// Sets the current BallId and triggers event.
    /// </summary>
    public void SetBallId(string newBallId)
    {
        if (CurrentGameId == newBallId) return; // No change
        CurrentBallId = newBallId;
        Debug.Log($"GameManager: CurrentBallId set to {CurrentBallId}");
    }

    /// <summary>
    /// Optional helper to load a scene while keeping global state.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("GameManager: Scene name is empty!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Resets the GameManager state if needed.
    /// </summary>
    public void ResetGame()
    {
        CurrentGameId = null;
        Debug.Log("GameManager: ResetGame called");
    }
}
