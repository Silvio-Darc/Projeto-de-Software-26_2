using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface do padrão GoF Strategy para definir algoritmos de tomada de decisão dos Bots.
/// </summary>
public interface IBotStrategy
{
    PlaceTileCommand ChooseTilePlacement(Turn context, CarcassoneTile tile);
    IGameCommand ChooseMeepleAction(Turn context, PlacedTile placedTile, IReadOnlyList<TileFeatureDefinition> availableFeatures);
}

/// <summary>
/// Estratégia Fácil (GoF Strategy): Escolhe uma posição/rotação válida aleatória e posiciona Meeple de forma probabilística.
/// </summary>
public class RandomBotStrategy : IBotStrategy
{
    public PlaceTileCommand ChooseTilePlacement(Turn context, CarcassoneTile tile)
    {
        if (context == null || context.Board == null || tile == null) return null;

        List<(Vector3Int position, int rotation)> validMoves = context.Board.GetAllValidPlacements(tile);
        if (validMoves.Count == 0) return null;

        int chosenIndex = Random.Range(0, validMoves.Count);
        var chosen = validMoves[chosenIndex];
        return new PlaceTileCommand(chosen.position, chosen.rotation);
    }

    public IGameCommand ChooseMeepleAction(Turn context, PlacedTile placedTile, IReadOnlyList<TileFeatureDefinition> availableFeatures)
    {
        Player bot = context != null ? context.GetCurrentPlayer() : null;
        if (bot == null || !bot.HasAvailableMeeple() || availableFeatures == null || availableFeatures.Count == 0)
        {
            return new SkipMeepleCommand();
        }

        // Prioriza estruturas que devolvem meeple durante o jogo (Mosteiro, Cidade, Estrada)
        List<TileFeatureDefinition> nonFarmFeatures = new List<TileFeatureDefinition>();
        for (int i = 0; i < availableFeatures.Count; i++)
        {
            if (availableFeatures[i].Type != FeatureType.Campo)
            {
                nonFarmFeatures.Add(availableFeatures[i]);
            }
        }

        if (nonFarmFeatures.Count > 0 && Random.value <= 0.60f)
        {
            TileFeatureDefinition pick = nonFarmFeatures[Random.Range(0, nonFarmFeatures.Count)];
            return new PlaceMeepleCommand(pick.LocalFeatureId);
        }

        return new SkipMeepleCommand();
    }
}

/// <summary>
/// Estratégia Média/Heurística (GoF Strategy):
/// Avalia todas as jogadas válidas priorizando conexões de Cidades e Estradas e gerencia o estoque de Meeples estrategicamente.
/// </summary>
public class HeuristicBotStrategy : IBotStrategy
{
    private static readonly Vector3Int[] Directions = new Vector3Int[4]
    {
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0)
    };

    private static readonly Borda[] OppositeEdges = new Borda[4]
    {
        Borda.Baixo,
        Borda.Esquerda,
        Borda.Cima,
        Borda.Direita
    };

    public PlaceTileCommand ChooseTilePlacement(Turn context, CarcassoneTile tile)
    {
        if (context == null || context.Board == null || tile == null) return null;

        List<(Vector3Int position, int rotation)> validMoves = context.Board.GetAllValidPlacements(tile);
        if (validMoves.Count == 0) return null;

        Player bot = context.GetCurrentPlayer();
        float bestScore = float.MinValue;
        (Vector3Int position, int rotation) bestMove = validMoves[0];

        for (int i = 0; i < validMoves.Count; i++)
        {
            var candidate = validMoves[i];
            float score = EvaluatePlacementHeuristic(context.Board, bot, tile, candidate.position, candidate.rotation);
            // Pequeno ruído para desempatar jogadas equivalentes de forma natural
            score += Random.Range(0f, 0.25f);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = candidate;
            }
        }

        return new PlaceTileCommand(bestMove.position, bestMove.rotation);
    }

    private float EvaluatePlacementHeuristic(Board board, Player bot, CarcassoneTile tile, Vector3Int pos, int rot)
    {
        float score = 0f;

        for (int dir = 0; dir < 4; dir++)
        {
            Vector3Int neighborPos = pos + Directions[dir];
            PlacedTile neighbor = board.GetPlacedTileAt(neighborPos);
            if (neighbor == null || neighbor.Definition == null) continue;

            score += 1.0f; // Bônus por encaixar em múltiplos vizinhos (ajuda a fechar estruturas e mosteiros)

            TipoTerreno edgeType = tile.obterBordaReal((Borda)dir, rot);
            if (edgeType == TipoTerreno.Cidade)
            {
                score += tile.temEscudo ? 5.0f : 3.5f;
            }
            else if (edgeType == TipoTerreno.Estrada)
            {
                score += 2.0f;
            }

            if (neighbor.HasMeeple && neighbor.PlacedMeeple != null && neighbor.PlacedMeeple.Owner == bot)
            {
                score += 4.0f; // Prioriza expandir/fechar estruturas onde o próprio Bot já tem Meeple
            }
        }

        // Bônus se colocar a peça adjacente a um Mosteiro do próprio Bot
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                PlacedTile around = board.GetPlacedTileAt(pos + new Vector3Int(dx, dy, 0));
                if (around != null && around.Definition != null && around.Definition.temMosteiro &&
                    around.HasMeeple && around.PlacedMeeple.Owner == bot)
                {
                    score += 3.0f;
                }
            }
        }

        return score;
    }

    public IGameCommand ChooseMeepleAction(Turn context, PlacedTile placedTile, IReadOnlyList<TileFeatureDefinition> availableFeatures)
    {
        Player bot = context != null ? context.GetCurrentPlayer() : null;
        if (bot == null || !bot.HasAvailableMeeple() || availableFeatures == null || availableFeatures.Count == 0)
        {
            return new SkipMeepleCommand();
        }

        int availableMeeples = bot.AvailableMeepleCount;
        int remainingTiles = context.TilePlacerView != null && context.TilePlacerView.Deck != null
            ? context.TilePlacerView.Deck.RemainingCount
            : 30;

        // 1. Sempre aloca em Mosteiro se disponível
        for (int i = 0; i < availableFeatures.Count; i++)
        {
            if (availableFeatures[i].Type == FeatureType.Mosteiro)
            {
                return new PlaceMeepleCommand(availableFeatures[i].LocalFeatureId);
            }
        }

        // 2. Aloca em Cidade se tiver ao menos 2 meeples (ou se a cidade tiver escudo)
        for (int i = 0; i < availableFeatures.Count; i++)
        {
            if (availableFeatures[i].Type == FeatureType.Cidade)
            {
                bool hasShield = placedTile != null && placedTile.Definition != null && placedTile.Definition.temEscudo;
                if (availableMeeples >= 2 || hasShield || remainingTiles < 15)
                {
                    return new PlaceMeepleCommand(availableFeatures[i].LocalFeatureId);
                }
            }
        }

        // 3. Aloca em Estrada se tiver boa reserva de meeples (>= 3)
        if (availableMeeples >= 3)
        {
            for (int i = 0; i < availableFeatures.Count; i++)
            {
                if (availableFeatures[i].Type == FeatureType.Estrada)
                {
                    return new PlaceMeepleCommand(availableFeatures[i].LocalFeatureId);
                }
            }
        }

        // 4. Aloca Fazendeiro no Campo na metade final do jogo se tiver reserva folgada (>= 4)
        if (remainingTiles <= 25 && availableMeeples >= 4)
        {
            for (int i = 0; i < availableFeatures.Count; i++)
            {
                if (availableFeatures[i].Type == FeatureType.Campo &&
                    availableFeatures[i].AdjacentCityFeatureIds != null &&
                    availableFeatures[i].AdjacentCityFeatureIds.Count > 0)
                {
                    return new PlaceMeepleCommand(availableFeatures[i].LocalFeatureId);
                }
            }
        }

        return new SkipMeepleCommand();
    }
}
