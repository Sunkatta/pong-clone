using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private RectTransform mainMenuPanel;

    [SerializeField]
    private RectTransform onlinePvpPanel;

    [SerializeField]
    private RectTransform hostPrivateMatchPanel;

    [SerializeField]
    private RectTransform joinPrivateMatchPanel;

    [SerializeField]
    private RectTransform lobbyPlayerListPanel;

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
    private Button playOnlineBtn;

    [SerializeField]
    private Button joinPrivateMatchBtn;

    [SerializeField]
    private Button joinPrivateMatchByCodeBtn;

    [SerializeField]
    private Button hostPrivateMatchBtn;

    [SerializeField]
    private Button quitGameBtn;

    [SerializeField]
    private Button backBtn;

    [SerializeField]
    private Button setProfileBtn;

    [SerializeField]
    private Button readyBtn;

    [SerializeField]
    private TMP_InputField setProfileInput;

    [SerializeField]
    private TMP_InputField hostCodeInput;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    [SerializeField]
    private TMP_Text numberOfPlayersTxt;

    [SerializeField]
    private TMP_Text player1ScoreText;

    [SerializeField]
    private TMP_Text player2ScoreText;

    [SerializeField]
    private TMP_Text endGameText;

    [SerializeField]
    private GameObject playerTilePrefab;

    [SerializeField]
    private AudioClip gameWonSound;

    [SerializeField]
    private AudioClip btnClickSound;

    private AudioSource mainMenuAudioSource;

    private bool isReady;

    private GameObject localPlayerTile;

    private void Start()
    {
        this.mainMenuAudioSource = GetComponent<AudioSource>();
        this.mainMenuPanel.gameObject.SetActive(true);
        this.lobbyManager.UpdateLobbyUi += this.OnLobbyUiUpdated;
        GameManager.PrepareInGameUi += this.OnUiPrepared;
        GameManager.ScoreChanged += this.OnScoreChanged;
        GameManager.MatchEnded += this.OnGameEnded;
        GameManager.LobbyLoaded += this.OnLobbyLoaded;

        this.localPvpBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.StartLocalPvpCoroutine());
        });

        this.onlinePvpBtn.onClick.AddListener(() =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            this.mainMenuPanel.gameObject.SetActive(false);
            this.authPanel.gameObject.SetActive(true);
        });

        this.setProfileBtn.onClick.AddListener(async () =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            await this.lobbyManager.SignIn(this.setProfileInput.text);
            this.authPanel.gameObject.SetActive(false);
            this.onlinePvpPanel.gameObject.SetActive(true);
        });

        this.playOnlineBtn.onClick.AddListener(() =>
        {

        });

        this.joinPrivateMatchBtn.onClick.AddListener(() =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            this.onlinePvpPanel.gameObject.SetActive(false);
            this.joinPrivateMatchPanel.gameObject.SetActive(true);
        });

        this.joinPrivateMatchByCodeBtn.onClick.AddListener(async () =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            await this.lobbyManager.JoinPrivateMatchByCode(this.joinCodeInput.text);
            this.joinPrivateMatchPanel.gameObject.SetActive(false);
            this.hostPrivateMatchPanel.gameObject.SetActive(true);
            this.hostCodeInput.text = this.lobbyManager.LobbyCode;
        });

        this.hostPrivateMatchBtn.onClick.AddListener(async () =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            await this.lobbyManager.HostPrivateMatch();
            this.onlinePvpPanel.gameObject.SetActive(false);
            this.hostPrivateMatchPanel.gameObject.SetActive(true);
            this.hostCodeInput.text = this.lobbyManager.LobbyCode;
        });

        this.readyBtn.onClick.AddListener(async () =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            this.isReady = !this.isReady;
            await this.lobbyManager.Ready(isReady);
            this.readyBtn.GetComponentInChildren<TMP_Text>().text = this.isReady ? Constants.PlayerNotReadyText : Constants.PlayerReadyText;
            this.localPlayerTile.GetComponentInChildren<Toggle>().isOn = this.isReady;
        });

        this.backBtn.onClick.AddListener(() =>
        {
            this.mainMenuAudioSource.PlayOneShot(this.btnClickSound);
            this.mainMenuPanel.gameObject.SetActive(true);
            this.onlinePvpPanel.gameObject.SetActive(false);
        });

        this.quitGameBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.QuitGameCoroutine());
        });
    }

    private IEnumerator StartLocalPvpCoroutine()
    {
        this.mainMenuAudioSource.Play();
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("MainGame");
    }

    private IEnumerator QuitGameCoroutine()
    {
        this.mainMenuAudioSource.Play();
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }

    // This gets called every second when polling for lobby changes, so it might not be performant for
    // more than 2 players, but it gets the job done for the time being.
    private void OnLobbyUiUpdated()
    {
        foreach (Transform playerTile in this.lobbyPlayerListPanel.transform)
        {
            Destroy(playerTile.gameObject);
        }

        foreach (var player in this.lobbyManager.JoinedPlayers)
        {
            GameObject playerTile = Instantiate(this.playerTilePrefab, this.lobbyPlayerListPanel);

            if (player.Id == this.lobbyManager.LocalPlayer.Id)
            {
                this.localPlayerTile = playerTile;
            }

            TMP_Text itemText = playerTile.GetComponentInChildren<TMP_Text>();

            if (itemText != null)
            {
                itemText.text = player.Data["playerName"].Value;
            }

            playerTile.GetComponentInChildren<Toggle>().isOn = bool.Parse(player.Data["isReady"].Value);
        }

        this.numberOfPlayersTxt.text = $"{this.lobbyManager.JoinedPlayers.Count}/{this.lobbyManager.MaxPlayers}";
    }

    private void OnUiPrepared()
    {
        this.hostPrivateMatchPanel.gameObject.SetActive(false);
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

    private void OnGameEnded(string winnerName, string loserName)
    {
        this.StartCoroutine(this.ShowEndGamePanelCoroutine(winnerName, loserName));
    }

    private IEnumerator ShowEndGamePanelCoroutine(string winnerName, string loserName)
    {
        yield return new WaitForSeconds(0.2f);
        this.mainMenuAudioSource.PlayOneShot(this.gameWonSound);

        this.endGameText.text = $"{winnerName} wins!\n {loserName}, want a rematch?";
        this.endGamePanel.gameObject.SetActive(true);
    }

    private void OnLobbyLoaded()
    {
        this.isReady = false;
        this.readyBtn.GetComponentInChildren<TMP_Text>().text = Constants.PlayerReadyText;
        this.lobbyManager.Ready(this.isReady).GetAwaiter();
        this.inGameHudPanel.gameObject.SetActive(false);
        this.endGamePanel.gameObject.SetActive(false);
        this.hostPrivateMatchPanel.gameObject.SetActive(true);
    }
}
