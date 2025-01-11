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
    private RectTransform lobbyPanel;

    [SerializeField]
    private RectTransform authPanel;

    [SerializeField]
    private RectTransform inGameHudPanel;

    [SerializeField]
    private Button localPvpBtn;

    [SerializeField]
    private Button onlinePvpBtn;

    [SerializeField]
    private Button quitGameBtn;

    [SerializeField]
    private AudioClip btnClickSound;

    private AudioSource mainMenuAudioSource;
    private IGameManager gameManager;

    private void Start()
    {
        this.mainMenuAudioSource = GetComponent<AudioSource>();
        this.mainMenuPanel.gameObject.SetActive(true);

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

    private IEnumerator QuitGameCoroutine()
    {
        this.mainMenuAudioSource.Play();
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }
}
