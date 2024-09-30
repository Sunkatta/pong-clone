using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private GameObject playerTilePrefab;

    private AudioSource btnClickSound;
    private List<string> playerNames = new List<string>();

    private void Start()
    {
        this.btnClickSound = GetComponent<AudioSource>();
        this.mainMenuPanel.gameObject.SetActive(true);
        this.lobbyManager.UpdateLobbyUiOnPlayerJoined += this.OnPlayerJoined;
        GameManager.PrepareInGameUi += this.OnUiPrepared;
        GameManager.ScoreChanged += this.OnScoreChanged;

        this.localPvpBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.StartLocalPvpCoroutine());
        });

        this.onlinePvpBtn.onClick.AddListener(() =>
        {
            this.btnClickSound.Play();
            this.mainMenuPanel.gameObject.SetActive(false);
            this.authPanel.gameObject.SetActive(true);
        });

        this.setProfileBtn.onClick.AddListener(async () =>
        {
            this.btnClickSound.Play();
            await this.lobbyManager.SignIn(this.setProfileInput.text);
            this.authPanel.gameObject.SetActive(false);
            this.onlinePvpPanel.gameObject.SetActive(true);
        });

        this.playOnlineBtn.onClick.AddListener(() =>
        {

        });

        this.joinPrivateMatchBtn.onClick.AddListener(() =>
        {
            this.btnClickSound.Play();
            this.onlinePvpPanel.gameObject.SetActive(false);
            this.joinPrivateMatchPanel.gameObject.SetActive(true);
        });

        this.joinPrivateMatchByCodeBtn.onClick.AddListener(async () =>
        {
            this.btnClickSound.Play();
            await this.lobbyManager.JoinPrivateMatchByCode(this.joinCodeInput.text);
            this.joinPrivateMatchPanel.gameObject.SetActive(false);
            this.hostPrivateMatchPanel.gameObject.SetActive(true);
            this.hostCodeInput.text = this.lobbyManager.LobbyCode;
            this.numberOfPlayersTxt.text = this.lobbyManager.LobbyStatusMessage;
        });

        this.hostPrivateMatchBtn.onClick.AddListener(async () =>
        {
            this.btnClickSound.Play();
            await this.lobbyManager.HostPrivateMatch();
            this.onlinePvpPanel.gameObject.SetActive(false);
            this.hostPrivateMatchPanel.gameObject.SetActive(true);
            this.hostCodeInput.text = this.lobbyManager.LobbyCode;
            this.numberOfPlayersTxt.text = this.lobbyManager.LobbyStatusMessage;
        });

        this.backBtn.onClick.AddListener(() =>
        {
            this.btnClickSound.Play();
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
        this.btnClickSound.Play();
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("MainGame");
    }

    private IEnumerator QuitGameCoroutine()
    {
        this.btnClickSound.Play();
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }

    private void OnPlayerJoined()
    {
        this.playerNames.Clear();
        this.playerNames = this.lobbyManager.JoinedPlayers
            .Select(player => player.Data["playerName"].Value)
            .ToList();

        foreach (Transform playerTile in this.lobbyPlayerListPanel.transform)
        {
            Destroy(playerTile.gameObject);
        }

        foreach (var playerName in this.playerNames)
        {
            GameObject playerTile = Instantiate(this.playerTilePrefab, this.lobbyPlayerListPanel);

            TMP_Text itemText = playerTile.GetComponentInChildren<TMP_Text>();

            if (itemText != null)
            {
                itemText.text = playerName;
            }
        }

        this.numberOfPlayersTxt.text = this.lobbyManager.LobbyStatusMessage;
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
}
