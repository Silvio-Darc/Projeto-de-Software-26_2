using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    public string playername;
    public Color playerColor;
    public bool isBot = false;
    public Image image;

    // MUDANÇA: Referencie o InputField diretamente, e não o texto que fica dentro dele
    public TMP_InputField inputField;

    private void PickColor()
    {
        int colorChosed = Random.Range(0, ConfigurarPartidaLocalPanel.Instance.avaibleColors.Count-1);
        playerColor = ConfigurarPartidaLocalPanel.Instance.avaibleColors[colorChosed];
        ConfigurarPartidaLocalPanel.Instance.avaibleColors.RemoveAt(colorChosed);
        image.color = playerColor;
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
    }

    // MUDANÇA: Leia do inputField
    public void updateName()
    {
        if (inputField != null)
        {
            playername = inputField.text;
        }
    }
}