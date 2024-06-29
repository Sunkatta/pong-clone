using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private Button localCoOpBtn;

    [SerializeField]
    private Button quitGameBtn;

    private AudioSource btnClickSound;

    private void Start()
    {
        this.btnClickSound = GetComponent<AudioSource>();

        this.localCoOpBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.StartLocalCoOpCoroutine());
        });

        this.quitGameBtn.onClick.AddListener(() =>
        {
            this.StartCoroutine(this.QuitGameCoroutine());
        });
    }

    private IEnumerator StartLocalCoOpCoroutine()
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
