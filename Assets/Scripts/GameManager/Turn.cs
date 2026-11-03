using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de Caso de Uso (GRASP Controller) e Contexto da Máquina de Estados (GoF State).
/// Recebe comandos (GoF Command) de jogadores humanos ou Bots e notifica observadores (GoF Observer).
/// </summary>
public class Turn : MonoBehaviour
{
    public static Turn Instance { get; private set; }

    [Header("Jogadores & Referências")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private Board board;
    [SerializeField] private TilePlacer tilePlacer;

    private int currentPlayerIndex = 0;
    private int roundNumber = 1;
    private ITurnState currentState;
    private List<TileFeatureDefinition> currentAvailableMeepleFeatures = new List<TileFeatureDefinition>();

    public FeatureEvaluator Evaluator { get; private set; }
    public ScoreService ScoreService { get; private set; }

    public enum TurnPhase { PosicionarTile, PosicionarMeeple, FimTurno, FimDeJogo }
    public TurnPhase CurrentPhase { get; private set; }

    public int RoundNumber => roundNumber;
    public int CurrentPlayerIndex => currentPlayerIndex;
    public IReadOnlyList<Player> Players => players;

    public Board Board
    {
        get
        {
            if (board == null) board = FindFirstObjectByType<Board>();
            return board;
        }
    }

    public TilePlacer TilePlacerView
    {
        get
        {
            if (tilePlacer == null) tilePlacer = FindFirstObjectByType<TilePlacer>();
            return tilePlacer;
        }
    }

    public CarcassoneTile CurrentTile => TilePlacerView != null ? TilePlacerView.currentTile : null;
    public PlacedTile LastPlacedTile { get; private set; }

    // Eventos Observer para UI, Visualizadores e Controlador de Bot
    public event Action<Player> OnTurnChanged;
    public event Action<TurnPhase> OnPhaseChanged;
    public event Action<CarcassoneTile, int> OnTileDrawn;
    public event Action<PlacedTile, IReadOnlyList<TileFeatureDefinition>> OnMeepleSelectionAvailable;
    public event Action OnMeepleSelectionHidden;
    public event Action<PlacedTile> OnTurnScoringRequested;
    public event Action<IReadOnlyList<Player>> OnFinalScoringRequested;
    public event Action<IReadOnlyList<Player>> OnGameOver;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Evaluator = new FeatureEvaluator();
            ScoreService = new ScoreService(Evaluator);

            if (FindFirstObjectByType<MeepleVisualManager>() == null)
            {
                gameObject.AddComponent<MeepleVisualManager>();
            }
            if (FindFirstObjectByType<BotController>() == null)
            {
                gameObject.AddComponent<BotController>();
            }

            AudioManager.EnsureExists();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Inicializa a partida com a lista de jogadores (vindo do GameSession).
    /// </summary>
    public void StartGame(List<Player> startingPlayers)
    {
        players = startingPlayers ?? new List<Player>();
        currentPlayerIndex = 0;
        roundNumber = 1;
        LastPlacedTile = null;

        if (TilePlacerView != null)
        {
            TilePlacerView.EnsureDeckInitialized();
        }

        StartTurnForCurrentPlayer();
    }

    public Player GetCurrentPlayer()
    {
        if (players == null || players.Count == 0) return null;
        return players[currentPlayerIndex];
    }

    /// <summary>
    /// Ponto único de entrada de ações (Padrão GoF Command + GRASP Controller).
    /// </summary>
    public bool ExecuteCommand(IGameCommand command)
    {
        if (command == null || currentState == null) return false;
        return currentState.HandleCommand(this, command);
    }

    /// <summary>
    /// Transita para um novo estado da máquina de estados do turno (Padrão GoF State).
    /// </summary>
    public void TransitionToState(ITurnState nextState)
    {
        if (nextState == null) return;
        currentState?.Exit(this);
        currentState = nextState;
        CurrentPhase = currentState.Phase;
        currentState.Enter(this);
    }

    public void NotifyPhaseChanged(TurnPhase phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
    }

    public CarcassoneTile DrawNextTileForTurn()
    {
        if (TilePlacerView == null) return null;

        CarcassoneTile drawn = TilePlacerView.DrawTileForTurn();
        int remaining = TilePlacerView.Deck != null ? TilePlacerView.Deck.RemainingCount : 0;
        OnTileDrawn?.Invoke(drawn, remaining);
        return drawn;
    }

    public bool PerformTilePlacement(Vector3Int position, int rotation)
    {
        if (TilePlacerView == null) return false;
        PlacedTile placed = TilePlacerView.ExecuteTilePlacementOnBoard(position, rotation);
        if (placed == null) return false;

        LastPlacedTile = placed;
        currentAvailableMeepleFeatures = ComputeAvailableMeepleFeatures(placed);
        return true;
    }

    public IReadOnlyList<TileFeatureDefinition> GetAvailableMeepleFeaturesForLastTile()
    {
        return currentAvailableMeepleFeatures;
    }

    public bool CanPlaceMeepleOnLastTile(int localFeatureId)
    {
        if (LastPlacedTile == null || LastPlacedTile.HasMeeple) return false;
        for (int i = 0; i < currentAvailableMeepleFeatures.Count; i++)
        {
            if (currentAvailableMeepleFeatures[i].LocalFeatureId == localFeatureId)
            {
                return true;
            }
        }
        return false;
    }

    public bool PerformMeeplePlacement(int localFeatureId)
    {
        if (TilePlacerView == null || LastPlacedTile == null) return false;
        return TilePlacerView.ExecuteMeeplePlacementOnBoard(LastPlacedTile, localFeatureId);
    }

    public void PerformSkipMeeple()
    {
        if (TilePlacerView != null)
        {
            TilePlacerView.CloseMeeplePopup();
        }
    }

    public void NotifyMeepleSelectionAvailable()
    {
        if (LastPlacedTile != null)
        {
            OnMeepleSelectionAvailable?.Invoke(LastPlacedTile, currentAvailableMeepleFeatures);
        }
    }

    public void HideMeepleSelectionUI()
    {
        if (TilePlacerView != null)
        {
            TilePlacerView.CloseMeeplePopup();
        }
        OnMeepleSelectionHidden?.Invoke();
    }

    public void ResolveTurnScoring()
    {
        if (LastPlacedTile != null && Board != null)
        {
            ScoreService?.ResolveTurnCompletedFeatures(Board, LastPlacedTile);
            OnTurnScoringRequested?.Invoke(LastPlacedTile);
        }
    }

    public void ResolveFinalGameScoring()
    {
        if (Board != null)
        {
            ScoreService?.ResolveEndOfGameScoring(Board);
        }
        OnFinalScoringRequested?.Invoke(players);
        OnGameOver?.Invoke(players);
    }

    public void AdvanceToNextPlayerAndStartTurn()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[Turn] Nenhum jogador cadastrado na lista para passar o turno!");
            return;
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        if (currentPlayerIndex == 0)
        {
            roundNumber++;
        }

        Debug.Log($"Turno passado com sucesso! Vez do: {GetCurrentPlayer()?.GetPlayerName()} (Índice {currentPlayerIndex})");
        StartTurnForCurrentPlayer();
    }

    private void StartTurnForCurrentPlayer()
    {
        Player current = GetCurrentPlayer();
        Debug.Log($"[Rodada {roundNumber}] Turno de: {current?.GetPlayerName() ?? $"Jogador {currentPlayerIndex + 1}"}");
        OnTurnChanged?.Invoke(current);
        AudioManager.Instance?.PlayTurnChanged();
        TransitionToState(new DrawTileState());
    }

    private List<TileFeatureDefinition> ComputeAvailableMeepleFeatures(PlacedTile placed)
    {
        List<TileFeatureDefinition> valid = new List<TileFeatureDefinition>();
        if (placed == null || placed.Definition == null || Board == null) return valid;

        if (Evaluator == null) Evaluator = new FeatureEvaluator();

        IReadOnlyList<TileFeatureDefinition> allFeatures = placed.Definition.Features;
        for (int i = 0; i < allFeatures.Count; i++)
        {
            TileFeatureDefinition feat = allFeatures[i];
            if (Evaluator.CanPlaceMeeple(Board, placed.Position, feat.LocalFeatureId))
            {
                valid.Add(feat);
            }
        }
        return valid;
    }

    // Métodos legados mantidos por compatibilidade
    public void SetPhase(TurnPhase newPhase)
    {
        NotifyPhaseChanged(newPhase);
    }

    public void EndTurn()
    {
        TransitionToState(new ResolveScoreState());
    }
}
