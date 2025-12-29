using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class MainMenuController : MonoBehaviour
{
    private IObjectResolver objectResolver;
    private ICreateGameUseCase createGameUseCase;

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

    [Inject]
    public void Construct(IObjectResolver objectResolver, ICreateGameUseCase createGameUseCase)
    {
        this.objectResolver = objectResolver;
        this.createGameUseCase = createGameUseCase;
    }

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

    private void OnEnable()
    {
        var eventSystem = EventSystem.current.GetComponent<EventSystem>();
        eventSystem.SetSelectedGameObject(this.localPvpBtn.gameObject);
    }

    private IEnumerator LocalPvpCoroutine()
    {
        this.localPvpBtn.GetComponent<AudioSource>().Play();

        var localPvpGameManager = this.objectResolver.Instantiate(this.localPvpGameManager);
        this.gameManager = localPvpGameManager.GetComponent<IGameManager>();

        var inGameHudController = this.inGameHudPanel.GetComponent<InGameHudController>();
        this.gameManager.PrepareInGameUi += inGameHudController.OnUiPrepared;

        yield return new WaitForSeconds(0.1f);

        var player1 = new PlayerEntity(Guid.NewGuid().ToString(), "Player 1", PlayerType.Player1);
        this.gameManager.OnPlayerJoined(player1);

        var player2 = new PlayerEntity(Guid.NewGuid().ToString(), "Player 2", PlayerType.Player2);
        this.gameManager.OnPlayerJoined(player2);

        var createGameCommand = new CreateGameCommand("1",
            player1.Id,
            player1.Username,
            player2.Id,
            player2.Username,
            (-10, -5),
            (10, -5),
            (10, 5),
            (-10, 5),
            7,
            2);

        this.createGameUseCase.Execute(createGameCommand);

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
