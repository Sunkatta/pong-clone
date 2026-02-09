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
    private IJoinGameUseCase joinGameUseCase;
    private PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler;

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

    private LocalPvpGameManager gameManager;

    [Inject]
    public void Construct(IObjectResolver objectResolver,
        ICreateGameUseCase createGameUseCase,
        IJoinGameUseCase joinGameUseCase,
        PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler)
    {
        this.objectResolver = objectResolver;
        this.createGameUseCase = createGameUseCase;
        this.joinGameUseCase = joinGameUseCase;
        this.playerJoinedDomainEventHandler = playerJoinedDomainEventHandler;
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

        this.playerJoinedDomainEventHandler.PlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        this.playerJoinedDomainEventHandler.PlayerJoined -= OnPlayerJoined;
    }

    private IEnumerator LocalPvpCoroutine()
    {
        this.localPvpBtn.GetComponent<AudioSource>().Play();

        var localPvpGameManager = this.objectResolver.Instantiate(this.localPvpGameManager);
        this.gameManager = localPvpGameManager.GetComponent<LocalPvpGameManager>();

        yield return new WaitForSeconds(0.1f);

        var createGameCommand = new CreateGameCommand(GameType.LocalPvp,
            (-9, -5),
            (9, -5),
            (9, 5),
            (-9, 5),
            GameManager.Instance.PaddleSpeed,
            2,
            GameManager.Instance.TargetScore,
            GameManager.Instance.BallInitialSpeed,
            GameManager.Instance.BallMaximumSpeed);

        var gameModel = this.createGameUseCase.Execute(createGameCommand);
        GameManager.Instance.SetGameId(gameModel.GameId);
        GameManager.Instance.SetBallId(gameModel.BallId);
        GameManager.Instance.SetGameType(GameType.LocalPvp);

        this.joinGameUseCase.Execute(new JoinGameCommand(gameModel.GameId, Guid.NewGuid().ToString(), "Player 1"));
        this.joinGameUseCase.Execute(new JoinGameCommand(gameModel.GameId, Guid.NewGuid().ToString(), "Player 2"));
        
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

    private void OnPlayerJoined(PlayerJoinedDomainEvent playerJoinedDomainEvent)
    {
        if (playerJoinedDomainEvent.PlayerType == PlayerType.Player1)
        {
            GameManager.Instance.SetPlayer1(playerJoinedDomainEvent.PlayerId, playerJoinedDomainEvent.Username);
        }

        if (playerJoinedDomainEvent.PlayerType == PlayerType.Player2)
        {
            GameManager.Instance.SetPlayer2(playerJoinedDomainEvent.PlayerId, playerJoinedDomainEvent.Username);
        }
    }
}
