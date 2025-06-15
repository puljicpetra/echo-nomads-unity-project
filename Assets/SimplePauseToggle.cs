using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimplePauseToggle : MonoBehaviour
{
    [Header("Panel & Main Toggle Button")]
    public GameObject pauseMenuPanel;
    public Button openClosePauseButton;

    [Header("Buttons Inside Panel")]
    public Button resumeButton;
    public Button goToMainMenuButton;

    public string mainMenuSceneName = "MainMenu";

    private bool isPanelVisible = false;

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseMenuPanel nije dodijeljen!");
        }

        if (openClosePauseButton != null)
        {
            openClosePauseButton.gameObject.SetActive(true);
            openClosePauseButton.onClick.AddListener(OpenPausePanel);
        }
        else
        {
            Debug.LogError("OpenClosePauseButton nije dodijeljen!");
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ClosePausePanelFromResume);
        }
        else
        {
            Debug.LogError("ResumeButton (play_alternative_icon_button) nije dodijeljen!");
        }

        if (goToMainMenuButton != null)
        {
            goToMainMenuButton.onClick.AddListener(LoadMainMenuScene);
        }
        else
        {
            Debug.LogError("GoToMainMenuButton (door_arrow_icon_button) nije dodijeljen!");
        }

        isPanelVisible = false;
        Time.timeScale = 1f;
    }

    void OpenPausePanel()
    {
        isPanelVisible = true;
        UpdatePanelAndButtonVisibility();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ClosePausePanelFromResume()
    {
        isPanelVisible = false;
        UpdatePanelAndButtonVisibility();

        Time.timeScale = 1f;
    }

    void UpdatePanelAndButtonVisibility()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPanelVisible);
        }

        if (openClosePauseButton != null)
        {
            openClosePauseButton.gameObject.SetActive(!isPanelVisible);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPanelVisible)
            {
                ClosePausePanelFromResume();
            }
            else
            {
                OpenPausePanel();
            }
        }
    }

    public void LoadMainMenuScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}