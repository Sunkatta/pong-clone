using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OnlinePvpController : MonoBehaviour
{
    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private RectTransform lobbyPanel;

    [SerializeField]
    private RectTransform joinPrivateMatchPanel;

    [SerializeField]
    private Button hostPrivateMatchBtn;

    [SerializeField]
    private Button joinPrivateMatchBtn;

    private void Start()
    {
        this.hostPrivateMatchBtn.onClick.AddListener(async () =>
        {
            this.hostPrivateMatchBtn.GetComponent<AudioSource>().Play();
            await this.lobbyManager.HostPrivateMatch();
            this.gameObject.SetActive(false);
            this.lobbyPanel.gameObject.SetActive(true);
        });

        this.joinPrivateMatchBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.JoinPrivateMatchCoroutine());
        });
    }

    private IEnumerator JoinPrivateMatchCoroutine()
    {
        this.joinPrivateMatchBtn.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.1f);
        this.gameObject.SetActive(false);
        this.joinPrivateMatchPanel.gameObject.SetActive(true);
    }
}
