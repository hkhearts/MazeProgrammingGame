using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject CreditScene;
    public GameObject MainMenu;
    public void PlayGame()
    {
        SceneManager.LoadScene(1); 
    }

    public void LoadCredits()
    {
        CreditScene.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void BackBTNclick(){
        CreditScene.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void RestartBTNClick(){
        SceneManager.LoadScene(0);
    }

    // This function quits the application.
    public void QuitGame()
    {
        Debug.Log("Quit Game button was pressed!");
        Application.Quit();
    }
}