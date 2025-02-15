using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private RectTransform onlinePvpPanel;

    [SerializeField]
    private RectTransform lobbyPlayerListPanel;

    [SerializeField]
    private RectTransform endGamePanel;

    [SerializeField]
    private RectTransform inGameHudPanel;

    [SerializeField]
    private Button readyBtn;

    [SerializeField]
    private Button leaveBtn;

    [SerializeField]
    private TMP_InputField hostCodeInput;

    [SerializeField]
    private TMP_Text numberOfPlayersTxt;

    [SerializeField]
    private TMP_Text countdownTimerText;

    [SerializeField]
    private GameObject playerTilePrefab;

    private bool isReady;
    private bool shouldBeginCountdown;

    private float remainingCountdownTime;

    private GameObject localPlayerTile;
    private IGameManager gameManager;

    private void OnEnable()
    {
        this.hostCodeInput.text = this.lobbyManager.LobbyCode;
        this.readyBtn.GetComponentInChildren<TMP_Text>().text = Constants.PlayerReadyText;

        var gameManagerGameObject = GameObject.Find("OnlinePvpGameManager(Clone)");
        this.gameManager = gameManagerGameObject.GetComponent<OnlinePvpGameManager>();
        var inGameHudController = this.inGameHudPanel.GetComponent<InGameHudController>();

        this.gameManager.PrepareInGameUi += inGameHudController.OnUiPrepared;
        this.gameManager.PrepareInGameUi += this.OnUiPrepared;
        this.gameManager.PlayerDisconnected += this.OnDisconnected;
    }

    private void OnDisable()
    {
        this.gameManager = null;
    }

    private void Start()
    {
        this.lobbyManager.UpdateLobbyUi += this.OnLobbyUiUpdated;
        this.lobbyManager.ShouldShowCountdownUi += this.OnShouldShowCountdownUi;
        OnlinePvpGameManager.LobbyLoaded += this.OnLobbyLoaded;

        this.readyBtn.onClick.AddListener(async () =>
        {
            this.readyBtn.GetComponent<AudioSource>().Play();
            this.isReady = !this.isReady;
            await this.lobbyManager.Ready(isReady);
            this.readyBtn.GetComponentInChildren<TMP_Text>().text = this.isReady ? Constants.PlayerNotReadyText : Constants.PlayerReadyText;
            this.localPlayerTile.GetComponentInChildren<Toggle>().isOn = this.isReady;
        });

        this.leaveBtn.onClick.AddListener(() =>
        {
            StartCoroutine(this.LeaveGameCoroutine());
        });
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
            }

            int seconds = Mathf.FloorToInt(this.remainingCountdownTime % 60);

            this.countdownTimerText.text = $"ALL PLAYERS READY! MATCH BEGINS IN {seconds}";
        }
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

    private void OnLobbyLoaded()
    {
        this.isReady = false;
        this.readyBtn.GetComponentInChildren<TMP_Text>().text = Constants.PlayerReadyText;
        this.inGameHudPanel.gameObject.SetActive(false);
        this.endGamePanel.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
    }

    private void OnShouldShowCountdownUi(bool shouldShowCountdownUi)
    {
        this.remainingCountdownTime = Constants.CountdownTimeInSeconds;

        if (shouldShowCountdownUi)
        {
            this.shouldBeginCountdown = true;
            this.countdownTimerText.gameObject.SetActive(true);
        }
        else
        {
            this.shouldBeginCountdown = false;
            this.countdownTimerText.gameObject.SetActive(false);
        }
    }

    private void OnDisconnected(string _, bool shouldNavigateToOnlinePvpPanel)
    {
        if (shouldNavigateToOnlinePvpPanel)
        {
            this.gameObject.SetActive(false);
            this.inGameHudPanel.gameObject.SetActive(false);
            this.onlinePvpPanel.gameObject.SetActive(true);
        }
    }

    private void OnUiPrepared(List<LocalPlayer> players)
    {
        this.gameObject.SetActive(false);
        this.countdownTimerText.gameObject.SetActive(false);
        this.inGameHudPanel.gameObject.SetActive(true);
    }

    private IEnumerator LeaveGameCoroutine()
    {
        this.leaveBtn.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.1f);
        this.gameManager.LeaveGame();
        this.gameObject.SetActive(false);
        this.onlinePvpPanel.gameObject.SetActive(true);
    }
}
