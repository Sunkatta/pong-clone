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
            this.setProfileBtn.GetComponent<AudioSource>().Play();

            if (string.IsNullOrWhiteSpace(this.setProfileInput.text))
            {
                this.setProfileInput.GetComponent<ShakeController>().Shake();
                return;
            }

            await this.lobbyManager.SignIn(this.setProfileInput.text);
            this.gameObject.SetActive(false);
            this.onlinePvpPanel.gameObject.SetActive(true);
        });
    }
}
