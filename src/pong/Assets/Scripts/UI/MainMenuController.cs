using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartLocalCoOp()
    {
        SceneManager.LoadScene("MainGame");
    }
}
