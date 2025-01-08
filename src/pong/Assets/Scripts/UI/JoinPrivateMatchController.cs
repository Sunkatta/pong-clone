using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinPrivateMatchController : MonoBehaviour
{
    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private RectTransform lobbyPanel;

    [SerializeField]
    private Button joinPrivateMatchByCodeBtn;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    private void Start()
    {
        this.joinPrivateMatchByCodeBtn.onClick.AddListener(async () =>
        {
            this.joinPrivateMatchByCodeBtn.GetComponent<AudioSource>().Play();

            if (string.IsNullOrWhiteSpace(this.joinCodeInput.text) || !await this.lobbyManager.JoinPrivateMatchByCode(this.joinCodeInput.text))
            {
                this.joinCodeInput.GetComponent<ShakeController>().Shake();
                return;
            }

            this.gameObject.SetActive(false);
            this.lobbyPanel.gameObject.SetActive(true);
        });
    }
}
