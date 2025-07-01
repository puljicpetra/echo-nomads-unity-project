using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("IntroCutscene");
    }

    public void QuitGame()
    {
        Debug.Log("Quit button was pressed! The game would close now.");

        Application.Quit();
    }
}