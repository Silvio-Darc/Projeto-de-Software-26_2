using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de execução dos turnos de Bots (IA) em partidas locais.
/// Observa as mudanças de fase do Turn e utiliza polimorfismo via IBotStrategy (Padrão GoF Strategy)
/// para gerar e despachar comandos (PlaceTileCommand, PlaceMeepleCommand, SkipMeepleCommand).
/// </summary>
public class BotController : MonoBehaviour
{
    [SerializeField] private float tilePlacementDelay = 0.55f;
    [SerializeField] private float meepleDecisionDelay = 0.35f;

    private Turn turnManager;
    private Coroutine activeBotCoroutine;

    private readonly Dictionary<BotDifficulty, IBotStrategy> strategies = new Dictionary<BotDifficulty, IBotStrategy>
    {
        { BotDifficulty.Facil, new RandomBotStrategy() },
        { BotDifficulty.Medio, new HeuristicBotStrategy() }
    };

    private void Start()
    {
        BindTurnManager();
    }

    private void OnEnable()
    {
        BindTurnManager();
    }

    private void OnDisable()
    {
        UnbindTurnManager();
    }

    private void BindTurnManager()
    {
        if (turnManager == null)
        {
            turnManager = Turn.Instance != null ? Turn.Instance : FindFirstObjectByType<Turn>();
        }

        if (turnManager != null)
        {
            turnManager.OnPhaseChanged -= HandlePhaseChanged;
            turnManager.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void UnbindTurnManager()
    {
        if (turnManager != null)
        {
            turnManager.OnPhaseChanged -= HandlePhaseChanged;
        }
        if (activeBotCoroutine != null)
        {
            StopCoroutine(activeBotCoroutine);
            activeBotCoroutine = null;
        }
    }

    private void HandlePhaseChanged(Turn.TurnPhase phase)
    {
        if (turnManager == null) return;
        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null || !currentPlayer.IsBot) return;

        if (activeBotCoroutine != null)
        {
            StopCoroutine(activeBotCoroutine);
            activeBotCoroutine = null;
        }

        if (phase == Turn.TurnPhase.PosicionarTile)
        {
            activeBotCoroutine = StartCoroutine(ExecuteBotTilePhase(currentPlayer));
        }
        else if (phase == Turn.TurnPhase.PosicionarMeeple)
        {
            activeBotCoroutine = StartCoroutine(ExecuteBotMeeplePhase(currentPlayer));
        }
    }

    private IEnumerator ExecuteBotTilePhase(Player botPlayer)
    {
        yield return new WaitForSeconds(tilePlacementDelay);

        if (turnManager == null || turnManager.CurrentPhase != Turn.TurnPhase.PosicionarTile) yield break;
        if (turnManager.GetCurrentPlayer() != botPlayer) yield break;

        IBotStrategy strategy = GetStrategy(botPlayer.Difficulty);
        PlaceTileCommand tileCmd = strategy.ChooseTilePlacement(turnManager, turnManager.CurrentTile);

        if (tileCmd != null)
        {
            turnManager.ExecuteCommand(tileCmd);
        }
    }

    private IEnumerator ExecuteBotMeeplePhase(Player botPlayer)
    {
        yield return new WaitForSeconds(meepleDecisionDelay);

        if (turnManager == null || turnManager.CurrentPhase != Turn.TurnPhase.PosicionarMeeple) yield break;
        if (turnManager.GetCurrentPlayer() != botPlayer) yield break;

        IBotStrategy strategy = GetStrategy(botPlayer.Difficulty);
        IReadOnlyList<TileFeatureDefinition> available = turnManager.GetAvailableMeepleFeaturesForLastTile();
        IGameCommand meepleCmd = strategy.ChooseMeepleAction(turnManager, turnManager.LastPlacedTile, available);

        if (meepleCmd != null)
        {
            turnManager.ExecuteCommand(meepleCmd);
        }
        else
        {
            turnManager.ExecuteCommand(new SkipMeepleCommand());
        }
    }

    private IBotStrategy GetStrategy(BotDifficulty difficulty)
    {
        if (strategies.TryGetValue(difficulty, out IBotStrategy strategy))
        {
            return strategy;
        }
        return strategies[BotDifficulty.Medio];
    }
}
