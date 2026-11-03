using UnityEngine;

/// <summary>
/// Painel Principal do Menu inicial.
/// </summary>
public class MainMenuPanel : UIPanel
{
    [SerializeField] private UIPanel optionsPanel;

    public void AbrirOpcoes()
    {
        if (optionsPanel != null)
        {
            UIManager.Instance?.OpenPanel(optionsPanel);
        }
        else
        {
            AudioOptionsPanel found = FindFirstObjectByType<AudioOptionsPanel>();
            if (found != null)
            {
                UIManager.Instance?.OpenPanel(found);
            }
        }
    }

    public void Sair()
    {
        AudioManager.Instance?.PlayButtonClick();
        Application.Quit();
    }
}
