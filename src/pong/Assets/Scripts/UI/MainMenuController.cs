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
    private TMP_InputField hostCodeInput;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    [SerializeField]
    private TMP_Text numberOfPlayersTxt;

    private AudioSource btnClickSound;

    private void Start()
    {
        this.btnClickSound = GetComponent<AudioSource>();
        this.mainMenuPanel.gameObject.SetActive(true);

        this.localPvpBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.StartLocalPvpCoroutine());
        });

        this.onlinePvpBtn.onClick.AddListener(() =>
        {
            this.btnClickSound.Play();
            this.mainMenuPanel.gameObject.SetActive(false);
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
            Debug.Log($"{this.lobbyManager.JoinedPlayers}/{this.lobbyManager.MaxPlayers}");
        });

        this.hostPrivateMatchBtn.onClick.AddListener(async () =>
        {
            this.btnClickSound.Play();
            await this.lobbyManager.HostPrivateMatch();
            this.onlinePvpPanel.gameObject.SetActive(false);
            this.hostPrivateMatchPanel.gameObject.SetActive(true);
            this.hostCodeInput.text = this.lobbyManager.LobbyCode;
            this.numberOfPlayersTxt.text = $"{this.lobbyManager.JoinedPlayers}/{this.lobbyManager.MaxPlayers}";
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
}
