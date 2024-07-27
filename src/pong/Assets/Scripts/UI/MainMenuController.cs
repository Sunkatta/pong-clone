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
    private RectTransform privateMatchPanel;

    [SerializeField]
    private Button localPvpBtn;

    [SerializeField]
    private Button onlinePvpBtn;

    [SerializeField]
    private Button playOnlineBtn;

    [SerializeField]
    private Button privateMatchBtn;

    [SerializeField]
    private Button quitGameBtn;

    [SerializeField]
    private Button backBtn;

    [SerializeField]
    private TMP_InputField hostCodeInput;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    private AudioSource btnClickSound;

    private void Start()
    {
        this.btnClickSound = GetComponent<AudioSource>();

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

        this.privateMatchBtn.onClick.AddListener(async () =>
        {
            this.btnClickSound.Play();
            await this.lobbyManager.InitPrivateMatch();
            this.onlinePvpPanel.gameObject.SetActive(false);
            this.privateMatchPanel.gameObject.SetActive(true);
            this.hostCodeInput.text = this.lobbyManager.LobbyCode;
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
