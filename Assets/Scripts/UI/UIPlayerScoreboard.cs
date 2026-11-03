using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gerencia o HUD completo da partida (Placar de Jogadores, Barra de Estado do Turno,
/// Contador de Peças do Baralho, Botão de Pular Meeple e Modal de Fim de Jogo).
/// Aplica o padrão GoF Observer inscrevendo-se nos eventos do controlador Turn.
/// </summary>
public class UIPlayerScoreboard : MonoBehaviour
{
    [SerializeField] private List<UIPlayerInfo> playerCards = new List<UIPlayerInfo>();
    [SerializeField] private TextMeshProUGUI turnStatusText;
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private Button skipMeepleButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverStandingsText;

    [Header("Opções In-Game")]
    [SerializeField] private GameObject inGameOptionsModal;
    [SerializeField] private Slider inGameMusicSlider;
    [SerializeField] private Slider inGameSfxSlider;
    [SerializeField] private TextMeshProUGUI inGameMusicPercentText;
    [SerializeField] private TextMeshProUGUI inGameSfxPercentText;

    private Canvas hudCanvas;
    private RectTransform cardsContainer;

    private void OnDestroy()
    {
        UnbindTurnEvents();
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (gameOverPanel != null && gameOverPanel.activeSelf) return;
            ToggleInGameOptions();
        }
    }

    public void ToggleInGameOptions(bool? forceState = null)
    {
        if (inGameOptionsModal == null) return;

        bool targetState = forceState.HasValue ? forceState.Value : !inGameOptionsModal.activeSelf;
        inGameOptionsModal.SetActive(targetState);

        if (targetState)
        {
            float musicVol = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 0.5f;
            float sfxVol = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1.0f;

            if (inGameMusicSlider != null)
            {
                inGameMusicSlider.SetValueWithoutNotify(musicVol);
            }
            if (inGameMusicPercentText != null)
            {
                inGameMusicPercentText.text = $"{Mathf.RoundToInt(musicVol * 100f)}%";
            }

            if (inGameSfxSlider != null)
            {
                inGameSfxSlider.SetValueWithoutNotify(sfxVol);
            }
            if (inGameSfxPercentText != null)
            {
                inGameSfxPercentText.text = $"{Mathf.RoundToInt(sfxVol * 100f)}%";
            }
        }
    }

    public void InitializeScoreboard(IReadOnlyList<Player> players)
    {
        EnsureHUDConstructed();

        // Limpa cartões antigos gerados dinamicamente
        for (int i = 0; i < playerCards.Count; i++)
        {
            if (playerCards[i] != null)
            {
                Destroy(playerCards[i].gameObject);
            }
        }
        playerCards.Clear();

        if (players != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                UIPlayerInfo card = CreatePlayerCard(players[i]);
                playerCards.Add(card);
            }
        }

        BindTurnEvents();
        RefreshHUDState();
    }

    private void BindTurnEvents()
    {
        UnbindTurnEvents();
        if (Turn.Instance == null) return;

        Turn.Instance.OnTurnChanged += HandleTurnChanged;
        Turn.Instance.OnPhaseChanged += HandlePhaseChanged;
        Turn.Instance.OnTileDrawn += HandleTileDrawn;
        Turn.Instance.OnGameOver += HandleGameOver;
    }

    private void UnbindTurnEvents()
    {
        if (Turn.Instance == null) return;

        Turn.Instance.OnTurnChanged -= HandleTurnChanged;
        Turn.Instance.OnPhaseChanged -= HandlePhaseChanged;
        Turn.Instance.OnTileDrawn -= HandleTileDrawn;
        Turn.Instance.OnGameOver -= HandleGameOver;
    }

    private void HandleTurnChanged(Player currentPlayer)
    {
        RefreshHUDState();
    }

    private void HandlePhaseChanged(Turn.TurnPhase phase)
    {
        RefreshHUDState();
    }

    private void HandleTileDrawn(CarcassoneTile drawnTile, int remainingInDeck)
    {
        if (deckCountText != null)
        {
            deckCountText.text = $"Baralho: {remainingInDeck} peças";
        }
        RefreshHUDState();
    }

    private void RefreshHUDState()
    {
        if (Turn.Instance == null) return;

        Player current = Turn.Instance.GetCurrentPlayer();
        Turn.TurnPhase phase = Turn.Instance.CurrentPhase;
        bool isHumanTurn = current != null && !current.IsBot;

        if (turnStatusText != null && current != null)
        {
            string who = current.IsBot ? $"{current.PlayerName} (Bot pensando...)" : current.PlayerName;
            string actionHint = "";

            if (phase == Turn.TurnPhase.PosicionarTile)
            {
                actionHint = isHumanTurn ? "Clique no tabuleiro para posicionar a peça (Q / R gira)" : "Posicionando peça...";
            }
            else if (phase == Turn.TurnPhase.PosicionarMeeple)
            {
                actionHint = isHumanTurn ? "Clique em um marcador na peça para alocar Meeple ou Pule (Espaço)" : "Avaliando Meeple...";
            }
            else if (phase == Turn.TurnPhase.FimDeJogo)
            {
                actionHint = "Partida Finalizada!";
            }

            turnStatusText.text = $"<b>Turno: {who}</b>  |  {actionHint}";
        }

        if (deckCountText != null && Turn.Instance.TilePlacerView != null && Turn.Instance.TilePlacerView.Deck != null)
        {
            deckCountText.text = $"Baralho: {Turn.Instance.TilePlacerView.Deck.RemainingCount} peças";
        }

        if (skipMeepleButton != null)
        {
            skipMeepleButton.gameObject.SetActive(isHumanTurn && phase == Turn.TurnPhase.PosicionarMeeple);
        }
    }

    private void HandleGameOver(IReadOnlyList<Player> players)
    {
        if (skipMeepleButton != null) skipMeepleButton.gameObject.SetActive(false);
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (gameOverStandingsText != null && players != null && players.Count > 0)
        {
            List<Player> sorted = players.Where(p => p != null).OrderByDescending(p => p.Score).ToList();
            int topScore = sorted.Count > 0 ? sorted[0].Score : 0;
            List<Player> winners = sorted.Where(p => p.Score == topScore).ToList();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (winners.Count == 1)
            {
                sb.AppendLine($"<size=32><color=#FFD54F>Vencedor: {winners[0].PlayerName}!</color></size>\n");
            }
            else
            {
                string tiedNames = string.Join(" & ", winners.Select(w => w.PlayerName));
                sb.AppendLine($"<size=30><color=#FFD54F>Empate na Vitória: {tiedNames}!</color></size>\n");
            }

            sb.AppendLine("<b>Classificação Final (incluindo Fazendas e Estruturas Incompletas):</b>\n");
            for (int i = 0; i < sorted.Count; i++)
            {
                Player p = sorted[i];
                string botTag = p.IsBot ? " [BOT]" : "";
                sb.AppendLine($"{i + 1}º  <b>{p.PlayerName}{botTag}</b>  —  {p.Score} pontos");
            }

            gameOverStandingsText.text = sb.ToString();
        }
    }

    private void EnsureHUDConstructed()
    {
        if (hudCanvas != null) return;

        GameObject canvasObj = new GameObject("GameHUDCanvas");
        canvasObj.transform.SetParent(transform, false);
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Barra Superior de Status e Controles
        GameObject topBar = CreatePanel(canvasObj.transform, "TopStatusBar",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, 56f), new Color(0.08f, 0.10f, 0.14f, 0.90f));

        turnStatusText = CreateText(topBar.transform, "TurnStatusText", "Iniciando partida...", 22, TextAlignmentOptions.MidlineLeft);
        RectTransform statusRect = turnStatusText.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.02f, 0f);
        statusRect.anchorMax = new Vector2(0.68f, 1f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;

        deckCountText = CreateText(topBar.transform, "DeckCountText", "Baralho: 71 peças", 22, TextAlignmentOptions.MidlineRight);
        RectTransform deckRect = deckCountText.GetComponent<RectTransform>();
        deckRect.anchorMin = new Vector2(0.68f, 0f);
        deckRect.anchorMax = new Vector2(0.87f, 1f);
        deckRect.offsetMin = Vector2.zero;
        deckRect.offsetMax = Vector2.zero;


        // 2. Coluna Esquerda com os Cartões de Jogadores
        GameObject cardsHolder = new GameObject("PlayerCardsContainer", typeof(RectTransform));
        cardsHolder.transform.SetParent(canvasObj.transform, false);
        cardsContainer = cardsHolder.GetComponent<RectTransform>();
        cardsContainer.anchorMin = new Vector2(0f, 0.25f);
        cardsContainer.anchorMax = new Vector2(0f, 0.92f);
        cardsContainer.pivot = new Vector2(0f, 1f);
        cardsContainer.anchoredPosition = new Vector2(18f, -12f);
        cardsContainer.sizeDelta = new Vector2(280f, 620f);

        VerticalLayoutGroup vlg = cardsHolder.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // 3. Botão "Pular Meeple (Espaço)" na parte inferior central
        skipMeepleButton = CreateButton(canvasObj.transform, "SkipMeepleButton", "Não colocar Meeple (Espaço)",
            new Color(0.85f, 0.36f, 0.18f, 0.96f), () =>
            {
                if (Turn.Instance != null)
                {
                    Turn.Instance.ExecuteCommand(new SkipMeepleCommand());
                }
            });
        RectTransform skipRect = skipMeepleButton.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0.5f, 0f);
        skipRect.anchorMax = new Vector2(0.5f, 0f);
        skipRect.pivot = new Vector2(0.5f, 0f);
        skipRect.anchoredPosition = new Vector2(0f, 28f);
        skipRect.sizeDelta = new Vector2(360f, 54f);
        skipMeepleButton.gameObject.SetActive(false);

        // 4. Painel Modal de Fim de Jogo (Game Over)
        gameOverPanel = CreatePanel(canvasObj.transform, "GameOverModal",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0.03f, 0.04f, 0.07f, 0.88f));

        GameObject dialogCard = CreatePanel(gameOverPanel.transform, "DialogBox",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(760f, 500f), new Color(0.12f, 0.15f, 0.22f, 0.98f));

        Outline dialogOutline = dialogCard.AddComponent<Outline>();
        dialogOutline.effectColor = new Color(1f, 0.82f, 0.25f, 0.9f);
        dialogOutline.effectDistance = new Vector2(2f, -2f);

        gameOverStandingsText = CreateText(dialogCard.transform, "StandingsText", "Fim de Jogo!", 24, TextAlignmentOptions.Center);
        RectTransform standingsRect = gameOverStandingsText.GetComponent<RectTransform>();
        standingsRect.anchorMin = new Vector2(0.06f, 0.24f);
        standingsRect.anchorMax = new Vector2(0.94f, 0.92f);
        standingsRect.offsetMin = Vector2.zero;
        standingsRect.offsetMax = Vector2.zero;

        Button playAgainBtn = CreateButton(dialogCard.transform, "PlayAgainBtn", "Jogar Novamente",
            new Color(0.20f, 0.65f, 0.32f, 1f), () =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        RectTransform againRect = playAgainBtn.GetComponent<RectTransform>();
        againRect.anchorMin = new Vector2(0.12f, 0.06f);
        againRect.anchorMax = new Vector2(0.47f, 0.18f);
        againRect.offsetMin = Vector2.zero;
        againRect.offsetMax = Vector2.zero;

        Button backToMenuBtn = CreateButton(dialogCard.transform, "BackToMenuBtn", "Voltar ao Menu",
            new Color(0.26f, 0.32f, 0.45f, 1f), () =>
            {
                SceneManager.LoadScene(MatchConfig.MenuSceneName);
            });
        RectTransform backRect = backToMenuBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.53f, 0.06f);
        backRect.anchorMax = new Vector2(0.88f, 0.18f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);

        // 5. Botão "⚙ Opções (Esc)" no canto superior direito
        Button inGameOptionsBtn = CreateButton(canvasObj.transform, "InGameOptionsBtn", "⚙ Opções (Esc)",
            new Color(0.20f, 0.25f, 0.35f, 0.90f), () =>
            {
                ToggleInGameOptions();
            });
        RectTransform optRect = inGameOptionsBtn.GetComponent<RectTransform>();
        optRect.anchorMin = new Vector2(1f, 1f);
        optRect.anchorMax = new Vector2(1f, 1f);
        optRect.pivot = new Vector2(1f, 1f);
        optRect.anchoredPosition = new Vector2(-20f, -18f);
        optRect.sizeDelta = new Vector2(160f, 42f);

        // 6. Painel Modal de Opções de Áudio In-Game
        ConstructInGameOptionsModal(canvasObj);
    }

    private void ConstructInGameOptionsModal(GameObject canvasObj)
    {
        inGameOptionsModal = CreatePanel(canvasObj.transform, "InGameOptionsModal",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0.03f, 0.04f, 0.07f, 0.88f));

        GameObject card = CreatePanel(inGameOptionsModal.transform, "OptionsCard",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(620f, 420f), new Color(0.12f, 0.15f, 0.22f, 0.98f));

        Outline cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.22f, 0.72f, 0.81f, 0.9f);
        cardOutline.effectDistance = new Vector2(2f, -2f);

        TextMeshProUGUI titleTxt = CreateText(card.transform, "TitleText", "CONFIGURAÇÕES DE ÁUDIO", 24, TextAlignmentOptions.Center);
        RectTransform titleRect = titleTxt.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.06f, 0.82f);
        titleRect.anchorMax = new Vector2(0.94f, 0.96f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Seção Música
        TextMeshProUGUI musicLabel = CreateText(card.transform, "MusicLabel", "Música de Fundo", 20, TextAlignmentOptions.MidlineLeft);
        RectTransform musicLabelRect = musicLabel.GetComponent<RectTransform>();
        musicLabelRect.anchorMin = new Vector2(0.08f, 0.64f);
        musicLabelRect.anchorMax = new Vector2(0.68f, 0.76f);
        musicLabelRect.offsetMin = Vector2.zero;
        musicLabelRect.offsetMax = Vector2.zero;

        float currentMusicVol = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 0.5f;
        inGameMusicPercentText = CreateText(card.transform, "MusicPercent", $"{Mathf.RoundToInt(currentMusicVol * 100f)}%", 20, TextAlignmentOptions.MidlineRight);
        RectTransform musicPercentRect = inGameMusicPercentText.GetComponent<RectTransform>();
        musicPercentRect.anchorMin = new Vector2(0.70f, 0.64f);
        musicPercentRect.anchorMax = new Vector2(0.92f, 0.76f);
        musicPercentRect.offsetMin = Vector2.zero;
        musicPercentRect.offsetMax = Vector2.zero;

        inGameMusicSlider = CreateSlider(card.transform, "MusicSlider", 0f, 1f, currentMusicVol, (val) =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
            if (inGameMusicPercentText != null) inGameMusicPercentText.text = $"{Mathf.RoundToInt(val * 100f)}%";
        });
        RectTransform musicSliderRect = inGameMusicSlider.GetComponent<RectTransform>();
        musicSliderRect.anchorMin = new Vector2(0.08f, 0.52f);
        musicSliderRect.anchorMax = new Vector2(0.92f, 0.62f);
        musicSliderRect.offsetMin = Vector2.zero;
        musicSliderRect.offsetMax = Vector2.zero;

        // Seção SFX
        TextMeshProUGUI sfxLabel = CreateText(card.transform, "SFXLabel", "Efeitos Sonoros (SFX / UI)", 20, TextAlignmentOptions.MidlineLeft);
        RectTransform sfxLabelRect = sfxLabel.GetComponent<RectTransform>();
        sfxLabelRect.anchorMin = new Vector2(0.08f, 0.36f);
        sfxLabelRect.anchorMax = new Vector2(0.68f, 0.48f);
        sfxLabelRect.offsetMin = Vector2.zero;
        sfxLabelRect.offsetMax = Vector2.zero;

        float currentSfxVol = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1.0f;
        inGameSfxPercentText = CreateText(card.transform, "SFXPercent", $"{Mathf.RoundToInt(currentSfxVol * 100f)}%", 20, TextAlignmentOptions.MidlineRight);
        RectTransform sfxPercentRect = inGameSfxPercentText.GetComponent<RectTransform>();
        sfxPercentRect.anchorMin = new Vector2(0.70f, 0.36f);
        sfxPercentRect.anchorMax = new Vector2(0.92f, 0.48f);
        sfxPercentRect.offsetMin = Vector2.zero;
        sfxPercentRect.offsetMax = Vector2.zero;

        inGameSfxSlider = CreateSlider(card.transform, "SFXSlider", 0f, 1f, currentSfxVol, (val) =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(val);
            if (inGameSfxPercentText != null) inGameSfxPercentText.text = $"{Mathf.RoundToInt(val * 100f)}%";
        });
        RectTransform sfxSliderRect = inGameSfxSlider.GetComponent<RectTransform>();
        sfxSliderRect.anchorMin = new Vector2(0.08f, 0.24f);
        sfxSliderRect.anchorMax = new Vector2(0.92f, 0.34f);
        sfxSliderRect.offsetMin = Vector2.zero;
        sfxSliderRect.offsetMax = Vector2.zero;

        // Botões Fechar / Menu
        Button resumeBtn = CreateButton(card.transform, "ResumeBtn", "Continuar",
            new Color(0.20f, 0.65f, 0.32f, 1f), () =>
            {
                ToggleInGameOptions(false);
            });
        RectTransform resumeRect = resumeBtn.GetComponent<RectTransform>();
        resumeRect.anchorMin = new Vector2(0.10f, 0.06f);
        resumeRect.anchorMax = new Vector2(0.48f, 0.17f);
        resumeRect.offsetMin = Vector2.zero;
        resumeRect.offsetMax = Vector2.zero;

        Button menuBtn = CreateButton(card.transform, "MenuBtn", "Voltar ao Menu",
            new Color(0.26f, 0.32f, 0.45f, 1f), () =>
            {
                SceneManager.LoadScene(MatchConfig.MenuSceneName);
            });
        RectTransform menuRect = menuBtn.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.52f, 0.06f);
        menuRect.anchorMax = new Vector2(0.90f, 0.17f);
        menuRect.offsetMin = Vector2.zero;
        menuRect.offsetMax = Vector2.zero;

        inGameOptionsModal.SetActive(false);
    }

    private UIPlayerInfo CreatePlayerCard(Player targetPlayer)
    {
        GameObject cardObj = CreatePanel(cardsContainer, $"Card_{targetPlayer.PlayerName}",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            Vector2.zero, new Vector2(270f, 96f), new Color(0.10f, 0.12f, 0.16f, 0.85f));

        Image bgImage = cardObj.GetComponent<Image>();
        Outline borderOutline = cardObj.AddComponent<Outline>();
        borderOutline.effectColor = targetPlayer.PlayerColor;
        borderOutline.effectDistance = new Vector2(2f, -2f);

        // Barra lateral com a cor do jogador
        GameObject colorStrip = CreatePanel(cardObj.transform, "ColorStrip",
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            Vector2.zero, new Vector2(10f, 0f), targetPlayer.PlayerColor);

        TextMeshProUGUI nameTxt = CreateText(cardObj.transform, "PlayerName", targetPlayer.PlayerName, 21, TextAlignmentOptions.MidlineLeft);
        nameTxt.fontStyle = FontStyles.Bold;
        RectTransform nameRect = nameTxt.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.07f, 0.52f);
        nameRect.anchorMax = new Vector2(0.96f, 0.94f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        TextMeshProUGUI scoreTxt = CreateText(cardObj.transform, "ScoreText", "Pts: 0", 19, TextAlignmentOptions.MidlineLeft);
        RectTransform scoreRect = scoreTxt.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.07f, 0.08f);
        scoreRect.anchorMax = new Vector2(0.48f, 0.50f);
        scoreRect.offsetMin = Vector2.zero;
        scoreRect.offsetMax = Vector2.zero;

        TextMeshProUGUI meepleTxt = CreateText(cardObj.transform, "MeepleText", "Meeples: 7", 19, TextAlignmentOptions.MidlineRight);
        RectTransform meepleRect = meepleTxt.GetComponent<RectTransform>();
        meepleRect.anchorMin = new Vector2(0.48f, 0.08f);
        meepleRect.anchorMax = new Vector2(0.95f, 0.50f);
        meepleRect.offsetMin = Vector2.zero;
        meepleRect.offsetMax = Vector2.zero;

        UIPlayerInfo info = cardObj.AddComponent<UIPlayerInfo>();
        info.ConfigureVisualReferences(nameTxt, scoreTxt, meepleTxt, bgImage, borderOutline);
        info.BindPlayer(targetPlayer);

        return info;
    }

    private static GameObject CreatePanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPos,
        Vector2 sizeDelta,
        Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string content,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Color bgColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.GetComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClick());
        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        TextMeshProUGUI txt = CreateText(btnObj.transform, "Label", label, 20, TextAlignmentOptions.Center);
        txt.fontStyle = FontStyles.Bold;
        RectTransform txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btn;
    }

    private static Slider CreateSlider(
        Transform parent,
        string name,
        float minValue,
        float maxValue,
        float initialValue,
        UnityEngine.Events.UnityAction<float> onValueChanged)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject sliderObj = DefaultControls.CreateSlider(res);
        sliderObj.name = name;
        sliderObj.transform.SetParent(parent, false);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = initialValue;

        if (onValueChanged != null)
        {
            slider.onValueChanged.AddListener(onValueChanged);
        }

        return slider;
    }
}
