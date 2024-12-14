using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackButtonController : MonoBehaviour
{
    [SerializeField]
    private RectTransform sourcePanel;

    [SerializeField]
    private RectTransform targetPanel;

    private AudioSource backButtonAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        this.backButtonAudioSource = GetComponent<AudioSource>();
        var buttonComponent = this.GetComponent<Button>();

        buttonComponent.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.GoBack());
        });
    }

    private IEnumerator GoBack()
    {
        this.backButtonAudioSource.Play();
        yield return new WaitForSeconds(0.1f);
        this.sourcePanel.gameObject.SetActive(false);
        this.targetPanel.gameObject.SetActive(true);
    }
}
