using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa um cartão de jogador (Humano ou Bot) na tela de configuração de partida local.
/// Aplica encapsulamento mantendo os nomes de campos serializados pelo Unity.
/// </summary>
public class UIPlayer : MonoBehaviour
{
    [SerializeField] public string playername;
    [SerializeField] public Color playerColor;
    [SerializeField] public bool isBot = false;
    [SerializeField] public Image image;
    [SerializeField] public TMP_InputField inputField;

    public string PlayerName => string.IsNullOrWhiteSpace(playername)
        ? (inputField != null && !string.IsNullOrWhiteSpace(inputField.text) ? inputField.text : (isBot ? "Bot" : "Jogador"))
        : playername;

    public Color PlayerColor => playerColor;
    public bool IsBot => isBot;

    private void PickColor()
    {
        if (ConfigurarPartidaLocalPanel.Instance != null &&
            ConfigurarPartidaLocalPanel.Instance.TryRequestColor(out Color chosenColor))
        {
            playerColor = chosenColor;
            if (image != null)
            {
                image.color = playerColor;
            }
        }
    }

    public void CallToRemovePlayer()
    {
        if (ConfigurarPartidaLocalPanel.Instance != null)
        {
            ConfigurarPartidaLocalPanel.Instance.RemovePlayer(this);
        }
    }

    public void Start()
    {
        PickColor();
        updateName();
    }

    public void updateName()
    {
        if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
        {
            playername = inputField.text;
        }
    }
}
