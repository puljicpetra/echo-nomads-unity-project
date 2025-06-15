using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager _;
    [SerializeField] private bool _debugMode; // privatna var koja se prikazuje u Inspectoru
    public enum MainMenuButtons { play, options, credits, quit };
    public enum SocialButtons { website, twitter, youtube };
    public enum CreditsButtons { back };
    public enum OptionsButtons { back };
    [SerializeField] GameObject _MainMenuContainer;
    [SerializeField] GameObject _CreditsMenuContainer;
    [SerializeField] GameObject _OptionsMenuContainer;
    [SerializeField] private string _sceneToLoadAfterClickingPlay;
    public void Awake()
    {
        if (_ == null) 
        { 
            _ = this;
        }
        else
        {
            Debug.LogError("Vise od 1 MainMenuManager u scene");
        }
    }
    private void Start()
    {
        OpenMenu(_MainMenuContainer);
    }
    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Button Clicked: " + buttonClicked.ToString());
        switch (buttonClicked)
        {
            case MainMenuButtons.play:
                PlayClicked();
                break;
            case MainMenuButtons.options:
                OptionsClicked();
                break;
            case MainMenuButtons.credits:
                CreditsClicked();
                break;
            case MainMenuButtons.quit:
                QuitGame();
                break;
            default:
                Debug.Log("Kliknut je button koji nije implementiran u MainMenuManager funkc.");
                break;
        }
    }
    public void SocialButtonClicked(SocialButtons buttonClicked)
    {
        string websiteLink = "";
        switch (buttonClicked) 
        {
            case SocialButtons.website:
                websiteLink = "http://www.google.com/";  // dodati link kasnije
                break;
            case SocialButtons.twitter:
                websiteLink = "https://x.com/"; // FF x
                break;
            case SocialButtons.youtube:
                websiteLink = "https://www.youtube.com/"; // FF yt
                break;
            default:
                Debug.LogError("Kliknuti soc button nije implementiran");
                break;
        }
        if (websiteLink != "")
        {
            Application.OpenURL(websiteLink);
        }
    }
    public void CreditsClicked()
    {
        OpenMenu(_CreditsMenuContainer);
    }
    public void OptionsClicked()
    {
        OpenMenu(_OptionsMenuContainer);
    }
    public void ReturnToMainMenu()
    {
        OpenMenu(_MainMenuContainer);
    }
    public void CreditsButtonClicked(CreditsButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case CreditsButtons.back:
                ReturnToMainMenu();
                break;
        }
    }
    public void OptionsButtonClicked(OptionsButtons buttonClicked)
    {
        switch (buttonClicked)
        {
            case OptionsButtons.back:
                ReturnToMainMenu();
                break;
        }
    }
    public void DebugMessage(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }

    public void PlayClicked()
    {
        SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }

    // za quit u play mode jer se nece ugasiti pomocu Application.Quit();
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
    public void OpenMenu(GameObject menuToOpen)
    {
        _MainMenuContainer.SetActive(menuToOpen == _MainMenuContainer);
        _CreditsMenuContainer.SetActive(menuToOpen == _CreditsMenuContainer);
        _OptionsMenuContainer.SetActive(menuToOpen == _OptionsMenuContainer);
    }
}
