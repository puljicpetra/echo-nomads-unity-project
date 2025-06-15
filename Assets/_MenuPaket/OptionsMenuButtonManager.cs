using UnityEngine;

public class OptionsMenuButtonManager : MonoBehaviour
{
    [SerializeField] private MainMenuManager.OptionsButtons _buttonType;
    public void ButtonClicked()
    {
        MainMenuManager._.OptionsButtonClicked(_buttonType);
    }
}
