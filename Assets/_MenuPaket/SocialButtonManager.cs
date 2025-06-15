using UnityEngine;

public class SocialButtonManager : MonoBehaviour
{
    [SerializeField] private MainMenuManager.SocialButtons _buttonType;
    public void ButtonClicked()
    {
        MainMenuManager._.SocialButtonClicked(_buttonType);
    }
}
