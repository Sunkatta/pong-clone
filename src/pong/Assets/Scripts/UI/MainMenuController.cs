using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private AudioSource btnClickSound;

    private void Start()
    {
        this.btnClickSound = GetComponent<AudioSource>();
    }

    public void OnStartLocalCoOpClicked()
    {
        this.StartCoroutine(this.StartLocalCoOpCoroutine());
    }

    public void OnQuitGameClicked()
    {
        this.StartCoroutine(this.QuitGameCoroutine());
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
        yield return new WaitForSeconds(0.1f);
        Application.Quit();
    }
}
