using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface do padrão GoF State para representar cada fase do ciclo de vida do turno.
/// Elimina condicionais espalhadas e centraliza as regras de transição de estado.
/// </summary>
public interface ITurnState
{
    Turn.TurnPhase Phase { get; }
    void Enter(Turn context);
    bool HandleCommand(Turn context, IGameCommand command);
    void Exit(Turn context);
}

/// <summary>
/// Estado 1: Compra de peça no início do turno.
/// Verifica automaticamente se há jogada possível ou se o baralho acabou (transição para GameOverState).
/// </summary>
public class DrawTileState : ITurnState
{
    public Turn.TurnPhase Phase => Turn.TurnPhase.PosicionarTile;

    public void Enter(Turn context)
    {
        CarcassoneTile drawn = context.DrawNextTileForTurn();
        if (drawn == null)
        {
            context.TransitionToState(new GameOverState());
            return;
        }

        context.TransitionToState(new PlaceTileState());
    }

    public bool HandleCommand(Turn context, IGameCommand command) => false;

    public void Exit(Turn context) { }
}

/// <summary>
/// Estado 2: Aguardando o jogador atual (Humano ou Bot) posicionar a peça sorteada no tabuleiro.
/// </summary>
public class PlaceTileState : ITurnState
{
    public Turn.TurnPhase Phase => Turn.TurnPhase.PosicionarTile;

    public void Enter(Turn context)
    {
        context.NotifyPhaseChanged(Turn.TurnPhase.PosicionarTile);
    }

    public bool HandleCommand(Turn context, IGameCommand command)
    {
        if (command is PlaceTileCommand placeTileCommand)
        {
            if (placeTileCommand.Execute(context))
            {
                Player current = context.GetCurrentPlayer();
                IReadOnlyList<TileFeatureDefinition> availableFeatures = context.GetAvailableMeepleFeaturesForLastTile();

                if (current != null && current.HasAvailableMeeple() && availableFeatures != null && availableFeatures.Count > 0)
                {
                    context.TransitionToState(new PlaceMeepleState());
                }
                else
                {
                    context.TransitionToState(new ResolveScoreState());
                }
                return true;
            }
        }
        return false;
    }

    public void Exit(Turn context) { }
}

/// <summary>
/// Estado 3: Aguardando o jogador atual decidir em qual sub-região livre colocará um Meeple ou pular a fase.
/// </summary>
public class PlaceMeepleState : ITurnState
{
    public Turn.TurnPhase Phase => Turn.TurnPhase.PosicionarMeeple;

    public void Enter(Turn context)
    {
        context.NotifyPhaseChanged(Turn.TurnPhase.PosicionarMeeple);
        context.NotifyMeepleSelectionAvailable();
    }

    public bool HandleCommand(Turn context, IGameCommand command)
    {
        if (command is PlaceMeepleCommand || command is SkipMeepleCommand)
        {
            if (command.Execute(context))
            {
                context.TransitionToState(new ResolveScoreState());
                return true;
            }
        }
        return false;
    }

    public void Exit(Turn context)
    {
        context.HideMeepleSelectionUI();
    }
}

/// <summary>
/// Estado 4: Resolução de estruturas completas (Estradas, Cidades, Mosteiros), devolução de Meeples e avanço de turno.
/// </summary>
public class ResolveScoreState : ITurnState
{
    public Turn.TurnPhase Phase => Turn.TurnPhase.FimTurno;

    public void Enter(Turn context)
    {
        context.NotifyPhaseChanged(Turn.TurnPhase.FimTurno);
        context.ResolveTurnScoring();
        context.AdvanceToNextPlayerAndStartTurn();
    }

    public bool HandleCommand(Turn context, IGameCommand command) => false;

    public void Exit(Turn context) { }
}

/// <summary>
/// Estado 5: Fim de Jogo (Baralho esgotado).
/// Contabiliza pontos finais de estruturas incompletas e Fazendas (Campos) e exibe o placar final.
/// </summary>
public class GameOverState : ITurnState
{
    public Turn.TurnPhase Phase => Turn.TurnPhase.FimDeJogo;

    public void Enter(Turn context)
    {
        Debug.Log("[Turn] Partida encerrada! Calculando pontuação final (estruturas incompletas e fazendas)...");
        context.NotifyPhaseChanged(Turn.TurnPhase.FimDeJogo);
        context.ResolveFinalGameScoring();
    }

    public bool HandleCommand(Turn context, IGameCommand command) => false;

    public void Exit(Turn context) { }
}
