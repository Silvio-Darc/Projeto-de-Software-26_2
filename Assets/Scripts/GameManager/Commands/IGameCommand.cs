using UnityEngine;

/// <summary>
/// Interface do padrão GoF Command para encapsular intenções de jogada
/// disparadas tanto por jogadores humanos (via Input/UI) quanto por Bots (IA).
/// </summary>
public interface IGameCommand
{
    bool CanExecute(Turn context);
    bool Execute(Turn context);
}

/// <summary>
/// Comando para posicionar a peça atual do turno em uma coordenada e rotação específicas do tabuleiro.
/// </summary>
public class PlaceTileCommand : IGameCommand
{
    public Vector3Int Position { get; }
    public int Rotation { get; }

    public PlaceTileCommand(Vector3Int position, int rotation)
    {
        Position = position;
        Rotation = ((rotation % 4) + 4) % 4;
    }

    public bool CanExecute(Turn context)
    {
        if (context == null || context.CurrentPhase != Turn.TurnPhase.PosicionarTile) return false;
        if (context.Board == null || context.CurrentTile == null) return false;
        return context.Board.IsValidPlacement(context.CurrentTile, Position, Rotation);
    }

    public bool Execute(Turn context)
    {
        if (!CanExecute(context)) return false;
        return context.PerformTilePlacement(Position, Rotation);
    }
}

/// <summary>
/// Comando para alocar um Meeple do jogador atual em uma sub-região específica da peça recém-colocada.
/// </summary>
public class PlaceMeepleCommand : IGameCommand
{
    public int LocalFeatureId { get; }

    public PlaceMeepleCommand(int localFeatureId)
    {
        LocalFeatureId = localFeatureId;
    }

    public bool CanExecute(Turn context)
    {
        if (context == null || context.CurrentPhase != Turn.TurnPhase.PosicionarMeeple) return false;
        Player current = context.GetCurrentPlayer();
        if (current == null || !current.HasAvailableMeeple()) return false;
        return context.CanPlaceMeepleOnLastTile(LocalFeatureId);
    }

    public bool Execute(Turn context)
    {
        if (!CanExecute(context)) return false;
        return context.PerformMeeplePlacement(LocalFeatureId);
    }
}

/// <summary>
/// Comando para pular opcionalmente o posicionamento de Meeple no turno atual.
/// </summary>
public class SkipMeepleCommand : IGameCommand
{
    public bool CanExecute(Turn context)
    {
        return context != null && context.CurrentPhase == Turn.TurnPhase.PosicionarMeeple;
    }

    public bool Execute(Turn context)
    {
        if (!CanExecute(context)) return false;
        context.PerformSkipMeeple();
        return true;
    }
}
