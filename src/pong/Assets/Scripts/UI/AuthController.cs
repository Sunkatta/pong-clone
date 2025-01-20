using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AuthController : MonoBehaviour
{
    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private RectTransform onlinePvpPanel;

    [SerializeField]
    private Button setProfileBtn;

    [SerializeField]
    private TMP_InputField setProfileInput;

    private void Start()
    {
        this.setProfileBtn.onClick.AddListener(async () =>
        {
            await this.OnSetProfile();
        });
    }

    private void OnEnable()
    {
        var eventSystem = EventSystem.current.GetComponent<EventSystem>();
        eventSystem.SetSelectedGameObject(this.setProfileInput.gameObject);
    }

    private async Task OnSetProfile()
    {
        var btnAudioSource = this.setProfileBtn.GetComponent<AudioSource>();
        btnAudioSource.Play();

        // Need this explicit delay, otherwise sound does not play after profile is set for the first time.
        await Task.Delay((int)(btnAudioSource.clip.length * 1000));

        if (string.IsNullOrWhiteSpace(this.setProfileInput.text) || !await this.lobbyManager.SignIn(this.setProfileInput.text))
        {
            this.setProfileInput.GetComponent<ShakeController>().Shake();
            return;
        }

        this.gameObject.SetActive(false);
        this.onlinePvpPanel.gameObject.SetActive(true);
    }
}
