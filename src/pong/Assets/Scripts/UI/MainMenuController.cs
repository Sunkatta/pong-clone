using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject localPvpGameManager;

    [SerializeField]
    private RectTransform authPanel;

    [SerializeField]
    private RectTransform inGameHudPanel;

    [SerializeField]
    private RectTransform optionsPanel;

    [SerializeField]
    private Button localPvpBtn;

    [SerializeField]
    private Button onlinePvpBtn;

    [SerializeField]
    private Button optionsBtn;

    [SerializeField]
    private Button quitGameBtn;

    private IGameManager gameManager;

    private void Start()
    {
        this.localPvpBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.LocalPvpCoroutine());
        });

        this.onlinePvpBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.OnlinePvpCoroutine());
        });

        this.optionsBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.OptionsCoroutine());
        });

        this.quitGameBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.QuitGameCoroutine());
        });
    }

    private IEnumerator LocalPvpCoroutine()
    {
        this.localPvpBtn.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.1f);
        var onlinePvpGameManager = Instantiate(this.localPvpGameManager);
        this.gameManager = onlinePvpGameManager.GetComponent<IGameManager>();

        var player1 = new LocalPlayer(Guid.NewGuid().ToString(), "Player 1", PlayerType.Player1);
        this.gameManager.OnPlayerJoined(player1);

        var player2 = new LocalPlayer(Guid.NewGuid().ToString(), "Player 2", PlayerType.Player2);
        this.gameManager.OnPlayerJoined(player2);

        this.gameManager.BeginGame();

        this.gameObject.SetActive(false);
        this.inGameHudPanel.gameObject.SetActive(true);
    }

    private IEnumerator OnlinePvpCoroutine()
    {
        this.onlinePvpBtn.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.1f);
        this.gameObject.SetActive(false);
        this.authPanel.gameObject.SetActive(true);
    }

    private IEnumerator OptionsCoroutine()
    {
        this.optionsBtn.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.1f);
        this.gameObject.SetActive(false);
        this.optionsPanel.gameObject.SetActive(true);
    }

    private IEnumerator QuitGameCoroutine()
    {
        this.quitGameBtn.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }
}
