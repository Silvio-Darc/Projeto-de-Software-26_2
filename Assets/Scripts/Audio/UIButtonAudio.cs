using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Componente auxiliar para disparar o som de clique de botão (AudioManager.PlayButtonClick)
/// quando um botão de UI for acionado ou clicado.
/// Pode ser anexado a GameObjects de UI com componente Button no Inspector
/// ou registrado programaticamente via UIButtonAudio.Attach(button).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerClickHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Caso o botão esteja interativo e clicado diretamente via Pointer
        if (button != null && !button.interactable) return;
        // O listener de button.onClick já cobre o disparo quando interativo
    }

    /// <summary>
    /// Registra programaticamente o efeito sonoro de clique a um botão de UI.
    /// </summary>
    public static void Attach(Button targetButton)
    {
        if (targetButton == null) return;
        targetButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
        });
    }
}
