using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            await this.OnSetProfile();
        }
    }

    private async Task OnSetProfile()
    {
        var btnAudioSource = this.setProfileBtn.GetComponent<AudioSource>();
        btnAudioSource.Play();

        // Need this explicit delay, otherwise sound does not play after profile is set for the first time.
        await Task.Delay((int)(btnAudioSource.clip.length * 1000));

        if (string.IsNullOrWhiteSpace(this.setProfileInput.text))
        {
            this.setProfileInput.GetComponent<ShakeController>().Shake();
            return;
        }

        await this.lobbyManager.SignIn(this.setProfileInput.text);
        this.gameObject.SetActive(false);
        this.onlinePvpPanel.gameObject.SetActive(true);
    }
}
