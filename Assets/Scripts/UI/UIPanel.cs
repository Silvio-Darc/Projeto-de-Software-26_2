using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanel: MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Open()
    {
        // Torna visível e interativo
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void Close()
    {
        // Esconde e desativa interação
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Chamado quando o jogador aperta ESC / Voltar
    public virtual void OnBackPressed()
    {
        UIManager.Instance.GoBack();
    }
}
