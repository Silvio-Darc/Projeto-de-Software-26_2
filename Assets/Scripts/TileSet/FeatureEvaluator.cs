using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serviço de avaliação topológica do tabuleiro (GRASP Pure Fabrication).
/// Executa busca em largura (BFS) sobre as 12 sub-portas de cada peça para:
/// 1. Validar se uma sub-região pode receber Meeple (nenhum Meeple na componente conexa);
/// 2. Detectar fechamento de Estradas, Cidades e Mosteiros ao final de cada turno;
/// 3. Contabilizar estruturas incompletas e Fazendas (Campos) ao final da partida.
/// </summary>
public class FeatureEvaluator
{
    private static readonly Vector3Int[] EdgeDirections = new Vector3Int[4]
    {
        new Vector3Int(0, 1, 0),  // Borda.Cima (0)
        new Vector3Int(1, 0, 0),  // Borda.Direita (1)
        new Vector3Int(0, -1, 0), // Borda.Baixo (2)
        new Vector3Int(-1, 0, 0)  // Borda.Esquerda (3)
    };

    /// <summary>
    /// Verifica se a sub-região (localFeatureId) da peça em startPos está livre de Meeples em toda a sua rede conectada.
    /// </summary>
    public bool CanPlaceMeeple(Board board, Vector3Int startPos, int localFeatureId)
    {
        ConnectedFeatureResult result = AnalyzeConnectedFeature(board, startPos, localFeatureId, evaluateFarmCities: false);
        return result != null && !result.HasMeeples;
    }

    /// <summary>
    /// Analisa a componente conexa a partir de (startPos, localFeatureId).
    /// </summary>
    public ConnectedFeatureResult AnalyzeConnectedFeature(
        Board board,
        Vector3Int startPos,
        int localFeatureId,
        bool evaluateFarmCities = false)
    {
        if (board == null) return null;
        PlacedTile startPlaced = board.GetPlacedTileAt(startPos);
        if (startPlaced == null || startPlaced.Definition == null) return null;

        TileFeatureDefinition startFeature = startPlaced.Definition.GetFeatureById(localFeatureId);
        if (startFeature == null) return null;

        if (startFeature.Type == FeatureType.Mosteiro)
        {
            return AnalyzeMonastery(board, startPlaced, startFeature);
        }

        Queue<(Vector3Int pos, int featureId)> queue = new Queue<(Vector3Int pos, int featureId)>();
        HashSet<(Vector3Int pos, int featureId)> visitedNodes = new HashSet<(Vector3Int pos, int featureId)>();
        HashSet<Vector3Int> uniqueTiles = new HashSet<Vector3Int>();
        HashSet<Vector3Int> shieldTiles = new HashSet<Vector3Int>();
        List<Meeple> meeples = new List<Meeple>();
        List<PlacedTile> meepleTiles = new List<PlacedTile>();

        int openPortsCount = 0;

        queue.Enqueue((startPos, localFeatureId));
        visitedNodes.Add((startPos, localFeatureId));

        while (queue.Count > 0)
        {
            var (currentPos, currentFeatId) = queue.Dequeue();
            PlacedTile currentTile = board.GetPlacedTileAt(currentPos);
            if (currentTile == null || currentTile.Definition == null) continue;

            TileFeatureDefinition currentFeat = currentTile.Definition.GetFeatureById(currentFeatId);
            if (currentFeat == null) continue;

            uniqueTiles.Add(currentPos);

            if (currentFeat.Type == FeatureType.Cidade && currentTile.Definition.temEscudo)
            {
                shieldTiles.Add(currentPos);
            }

            if (currentTile.HasMeeple && currentTile.PlacedFeatureId == currentFeatId)
            {
                meeples.Add(currentTile.PlacedMeeple);
                meepleTiles.Add(currentTile);
            }

            IReadOnlyList<int> localPorts = currentFeat.ConnectedPorts;
            for (int i = 0; i < localPorts.Count; i++)
            {
                int localPort = localPorts[i];
                int worldPort = TileFeatureDefinition.LocalPortToWorldPort(localPort, currentTile.Rotation);
                Borda edge = TileFeatureDefinition.GetEdgeFromPort(worldPort);
                Vector3Int neighborPos = currentPos + EdgeDirections[(int)edge];

                PlacedTile neighborTile = board.GetPlacedTileAt(neighborPos);
                if (neighborTile == null || neighborTile.Definition == null)
                {
                    openPortsCount++;
                    continue;
                }

                int neighborWorldPort = TileFeatureDefinition.GetOppositeWorldPort(worldPort);
                TileFeatureDefinition neighborFeat = neighborTile.Definition.GetFeatureAtWorldPort(neighborWorldPort, neighborTile.Rotation);

                if (neighborFeat != null && neighborFeat.Type == currentFeat.Type)
                {
                    var neighborNode = (neighborPos, neighborFeat.LocalFeatureId);
                    if (visitedNodes.Add(neighborNode))
                    {
                        queue.Enqueue(neighborNode);
                    }
                }
                else
                {
                    openPortsCount++;
                }
            }
        }

        bool isComplete = (startFeature.Type == FeatureType.Estrada || startFeature.Type == FeatureType.Cidade) && openPortsCount == 0;
        int completedAdjacentCities = 0;

        if (startFeature.Type == FeatureType.Campo && evaluateFarmCities)
        {
            completedAdjacentCities = CountCompletedCitiesTouchingFarm(board, visitedNodes);
        }

        return new ConnectedFeatureResult(
            startFeature.Type,
            isComplete,
            visitedNodes,
            uniqueTiles,
            shieldTiles.Count,
            meeples,
            meepleTiles,
            completedAdjacentCities);
    }

    private ConnectedFeatureResult AnalyzeMonastery(Board board, PlacedTile monasteryTile, TileFeatureDefinition monasteryFeature)
    {
        HashSet<(Vector3Int position, int featureId)> nodes = new HashSet<(Vector3Int position, int featureId)>
        {
            (monasteryTile.Position, monasteryFeature.LocalFeatureId)
        };
        HashSet<Vector3Int> occupiedMooreNeighborhood = new HashSet<Vector3Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector3Int checkPos = monasteryTile.Position + new Vector3Int(dx, dy, 0);
                if (board.HasPlacedTileAt(checkPos))
                {
                    occupiedMooreNeighborhood.Add(checkPos);
                }
            }
        }

        List<Meeple> meeples = new List<Meeple>();
        List<PlacedTile> meepleTiles = new List<PlacedTile>();
        if (monasteryTile.HasMeeple && monasteryTile.PlacedFeatureId == monasteryFeature.LocalFeatureId)
        {
            meeples.Add(monasteryTile.PlacedMeeple);
            meepleTiles.Add(monasteryTile);
        }

        bool isComplete = occupiedMooreNeighborhood.Count == 9;
        return new ConnectedFeatureResult(
            FeatureType.Mosteiro,
            isComplete,
            nodes,
            occupiedMooreNeighborhood,
            0,
            meeples,
            meepleTiles);
    }

    /// <summary>
    /// Conta quantas Cidades Completas distintas fazem fronteira com os nós de um Campo (Fazenda).
    /// Cada Cidade Completa adjacente vale 3 pontos para a Fazenda no fim do jogo.
    /// </summary>
    private int CountCompletedCitiesTouchingFarm(Board board, HashSet<(Vector3Int pos, int featureId)> farmNodes)
    {
        HashSet<(Vector3Int position, int featureId)> distinctCompletedCityKeys = new HashSet<(Vector3Int position, int featureId)>();

        foreach (var (pos, fieldFeatureId) in farmNodes)
        {
            PlacedTile placed = board.GetPlacedTileAt(pos);
            TileFeatureDefinition fieldFeat = placed?.Definition?.GetFeatureById(fieldFeatureId);
            if (fieldFeat == null || fieldFeat.AdjacentCityFeatureIds == null) continue;

            for (int i = 0; i < fieldFeat.AdjacentCityFeatureIds.Count; i++)
            {
                int cityLocalId = fieldFeat.AdjacentCityFeatureIds[i];
                ConnectedFeatureResult cityResult = AnalyzeConnectedFeature(board, pos, cityLocalId, evaluateFarmCities: false);
                if (cityResult != null && cityResult.Type == FeatureType.Cidade && cityResult.IsComplete)
                {
                    distinctCompletedCityKeys.Add(cityResult.GetCanonicalKey());
                }
            }
        }

        return distinctCompletedCityKeys.Count;
    }

    /// <summary>
    /// Retorna todas as estruturas (Estradas, Cidades e Mosteiros na vizinhança 3x3) que foram completadas
    /// após o posicionamento da peça em lastPlacedPos.
    /// </summary>
    public List<ConnectedFeatureResult> GetCompletedFeaturesAfterPlacement(Board board, Vector3Int lastPlacedPos)
    {
        List<ConnectedFeatureResult> completed = new List<ConnectedFeatureResult>();
        if (board == null) return completed;

        HashSet<(Vector3Int position, int featureId)> processedKeys = new HashSet<(Vector3Int position, int featureId)>();

        PlacedTile lastTile = board.GetPlacedTileAt(lastPlacedPos);
        if (lastTile != null && lastTile.Definition != null)
        {
            IReadOnlyList<TileFeatureDefinition> features = lastTile.Definition.Features;
            for (int i = 0; i < features.Count; i++)
            {
                TileFeatureDefinition feat = features[i];
                if (feat.Type == FeatureType.Estrada || feat.Type == FeatureType.Cidade)
                {
                    ConnectedFeatureResult res = AnalyzeConnectedFeature(board, lastPlacedPos, feat.LocalFeatureId);
                    if (res != null && res.IsComplete && processedKeys.Add(res.GetCanonicalKey()))
                    {
                        completed.Add(res);
                    }
                }
            }
        }

        // Verifica se a peça recém-colocada completou algum Mosteiro na vizinhança 3x3 (incluindo ela própria)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector3Int checkPos = lastPlacedPos + new Vector3Int(dx, dy, 0);
                PlacedTile neighbor = board.GetPlacedTileAt(checkPos);
                if (neighbor == null || neighbor.Definition == null || !neighbor.Definition.temMosteiro) continue;

                IReadOnlyList<TileFeatureDefinition> neighborFeatures = neighbor.Definition.Features;
                for (int i = 0; i < neighborFeatures.Count; i++)
                {
                    if (neighborFeatures[i].Type == FeatureType.Mosteiro)
                    {
                        ConnectedFeatureResult monasteryRes = AnalyzeConnectedFeature(board, checkPos, neighborFeatures[i].LocalFeatureId);
                        if (monasteryRes != null && monasteryRes.IsComplete && processedKeys.Add(monasteryRes.GetCanonicalKey()))
                        {
                            completed.Add(monasteryRes);
                        }
                    }
                }
            }
        }

        return completed;
    }

    /// <summary>
    /// Coleta todas as estruturas com Meeples (Estradas/Cidades/Mosteiros incompletos e Fazendas/Campos)
    /// restantes no tabuleiro para a pontuação de Fim de Jogo.
    /// </summary>
    public List<ConnectedFeatureResult> GetEndOfGameFeaturesWithMeeples(Board board)
    {
        List<ConnectedFeatureResult> results = new List<ConnectedFeatureResult>();
        if (board == null) return results;

        HashSet<(Vector3Int position, int featureId)> processedKeys = new HashSet<(Vector3Int position, int featureId)>();

        foreach (var kvp in board.PlacedTiles)
        {
            PlacedTile placed = kvp.Value;
            if (placed == null || !placed.HasMeeple || placed.Definition == null) continue;

            ConnectedFeatureResult res = AnalyzeConnectedFeature(
                board,
                placed.Position,
                placed.PlacedFeatureId,
                evaluateFarmCities: true);

            if (res != null && res.HasMeeples && processedKeys.Add(res.GetCanonicalKey()))
            {
                results.Add(res);
            }
        }

        return results;
    }
}
