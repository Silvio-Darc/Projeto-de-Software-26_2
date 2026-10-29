using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfo : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image meepleImage;
    [SerializeField] private TextMeshProUGUI meepleQty;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image scoreImage;
    [SerializeField] private Outline outline;
    private Color playerColor;
    private int playerScore;
    private int playerMeeples;
    private string playerName;

    [SerializeField] private bool isPlayerTurn;

    private void Start()
    {
        playerColor = player.GetPlayerColor();
        //playerScore = player.getPlayerScore();
        //playerMeeples = player.getPlayerMeeples();
        playerName = player.GetPlayerName();
        outline.effectColor = playerColor;
        playerNameText.color = playerColor;
        meepleImage.color = playerColor;
        scoreImage.color = playerColor;
    }

    private void OnEnterTurn()
    {
        isPlayerTurn = true;
    }
}
