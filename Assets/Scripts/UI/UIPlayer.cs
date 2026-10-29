using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    public string playername;
    public bool isBot = false;

    // MUDANÇA: Referencie o InputField diretamente, e não o texto que fica dentro dele
    public TMP_InputField inputField;

    public void CallToRemovePlayer()
    {
        if (ConfigurarPartidaLocalPanel.Instance != null)
        {
            ConfigurarPartidaLocalPanel.Instance.RemovePlayer(this);
        }
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