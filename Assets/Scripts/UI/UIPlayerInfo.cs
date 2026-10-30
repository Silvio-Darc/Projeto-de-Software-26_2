using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cartão de HUD de um jogador individual.
/// Implementa o padrão GoF Observer inscrevendo-se nos eventos de Player e Turn.
/// </summary>
public class UIPlayerInfo : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image meepleImage;
    [SerializeField] private TextMeshProUGUI meepleQty;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image scoreImage;
    [SerializeField] private Outline outline;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private bool isPlayerTurn;

    public Player BoundPlayer => player;

    private void Start()
    {
        if (player != null)
        {
            BindPlayer(player);
        }
    }

    private void OnDestroy()
    {
        UnbindPlayer();
    }

    public void ConfigureVisualReferences(
        TextMeshProUGUI nameLabel,
        TextMeshProUGUI scoreLabel,
        TextMeshProUGUI meepleLabel,
        Image bgImage,
        Outline borderOutline)
    {
        playerNameText = nameLabel;
        scoreText = scoreLabel;
        meepleQty = meepleLabel;
        backgroundImage = bgImage;
        outline = borderOutline;
    }

    public void BindPlayer(Player targetPlayer)
    {
        UnbindPlayer();
        player = targetPlayer;
        if (player == null) return;

        player.OnScoreChanged += HandleScoreChanged;
        player.OnMeepleCountChanged += HandleMeepleCountChanged;

        if (Turn.Instance != null)
        {
            Turn.Instance.OnTurnChanged -= HandleTurnChanged;
            Turn.Instance.OnTurnChanged += HandleTurnChanged;
        }

        RefreshStaticVisuals();
        HandleScoreChanged(player, player.Score);
        HandleMeepleCountChanged(player, player.AvailableMeepleCount);

        if (Turn.Instance != null)
        {
            HandleTurnChanged(Turn.Instance.GetCurrentPlayer());
        }
    }

    private void UnbindPlayer()
    {
        if (player != null)
        {
            player.OnScoreChanged -= HandleScoreChanged;
            player.OnMeepleCountChanged -= HandleMeepleCountChanged;
        }

        if (Turn.Instance != null)
        {
            Turn.Instance.OnTurnChanged -= HandleTurnChanged;
        }
    }

    private void RefreshStaticVisuals()
    {
        if (player == null) return;
        Color c = player.PlayerColor;

        if (playerNameText != null)
        {
            string tag = player.IsBot ? " [BOT]" : "";
            playerNameText.text = $"{player.PlayerName}{tag}";
            playerNameText.color = Color.Lerp(c, Color.white, 0.25f);
        }
        if (meepleImage != null) meepleImage.color = c;
        if (scoreImage != null) scoreImage.color = c;
        if (outline != null) outline.effectColor = c;
    }

    private void HandleScoreChanged(Player p, int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Pts: {newScore}";
        }
    }

    private void HandleMeepleCountChanged(Player p, int availableMeeples)
    {
        if (meepleQty != null)
        {
            meepleQty.text = $"Meeples: {availableMeeples}";
        }
    }

    private void HandleTurnChanged(Player activeTurnPlayer)
    {
        isPlayerTurn = (player != null && activeTurnPlayer == player);

        if (outline != null)
        {
            outline.enabled = isPlayerTurn;
            outline.effectDistance = isPlayerTurn ? new Vector2(3f, -3f) : new Vector2(1f, -1f);
        }

        if (backgroundImage != null && player != null)
        {
            backgroundImage.color = isPlayerTurn
                ? new Color(0.18f, 0.22f, 0.30f, 0.96f)
                : new Color(0.10f, 0.12f, 0.16f, 0.82f);
        }

        transform.localScale = isPlayerTurn ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
    }
}
