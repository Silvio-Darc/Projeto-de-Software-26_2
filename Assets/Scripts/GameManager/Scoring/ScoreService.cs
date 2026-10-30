using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serviço de pontuação (GRASP Pure Fabrication) que orquestra o FeatureEvaluator e as
/// estratégias polimórficas IFeatureScoringStrategy (GoF Strategy) para premiar jogadores
/// e devolver Meeples ao final de cada turno e ao final da partida.
/// </summary>
public class ScoreService
{
    private readonly Dictionary<FeatureType, IFeatureScoringStrategy> strategies;
    private readonly FeatureEvaluator evaluator;

    public event Action<ConnectedFeatureResult, List<Player>, int> OnFeatureScored;

    public ScoreService(FeatureEvaluator featureEvaluator)
    {
        evaluator = featureEvaluator ?? new FeatureEvaluator();
        strategies = new Dictionary<FeatureType, IFeatureScoringStrategy>
        {
            { FeatureType.Estrada, new RoadScoringStrategy() },
            { FeatureType.Cidade, new CityScoringStrategy() },
            { FeatureType.Mosteiro, new MonasteryScoringStrategy() },
            { FeatureType.Campo, new FarmScoringStrategy() }
        };
    }

    /// <summary>
    /// Avalia e pontua todas as Estradas, Cidades e Mosteiros completados após o posicionamento em lastPlacedTile,
    /// devolvendo os Meeples dessas estruturas aos seus donos.
    /// </summary>
    public void ResolveTurnCompletedFeatures(Board board, PlacedTile lastPlacedTile)
    {
        if (board == null || lastPlacedTile == null) return;

        List<ConnectedFeatureResult> completedFeatures = evaluator.GetCompletedFeaturesAfterPlacement(board, lastPlacedTile.Position);

        for (int i = 0; i < completedFeatures.Count; i++)
        {
            ConnectedFeatureResult feature = completedFeatures[i];
            if (feature == null || !feature.IsComplete) continue;

            if (feature.HasMeeples && strategies.TryGetValue(feature.Type, out IFeatureScoringStrategy strategy))
            {
                int points = strategy.CalculateScore(feature, isEndOfGame: false);
                List<Player> winners = feature.GetMajorityOwners();

                if (points > 0 && winners.Count > 0)
                {
                    for (int w = 0; w < winners.Count; w++)
                    {
                        Player winner = winners[w];
                        winner.AddScore(points);
                        Debug.Log($"[Pontuação Turno] {winner.PlayerName} ganhou +{points} pts por {feature.Type} completa ({feature.Tiles.Count} tiles, {feature.ShieldCount} escudos)! Total: {winner.Score}");
                    }
                    OnFeatureScored?.Invoke(feature, winners, points);
                }
            }

            // Devolve todos os Meeples da estrutura completada para a reserva dos respectivos jogadores
            ReturnMeeplesFromFeature(board, feature);
        }
    }

    /// <summary>
    /// Contabiliza os pontos de Fim de Jogo (Estradas, Cidades e Mosteiros incompletos + Fazendas/Campos).
    /// </summary>
    public void ResolveEndOfGameScoring(Board board)
    {
        if (board == null) return;

        List<ConnectedFeatureResult> endFeatures = evaluator.GetEndOfGameFeaturesWithMeeples(board);

        for (int i = 0; i < endFeatures.Count; i++)
        {
            ConnectedFeatureResult feature = endFeatures[i];
            if (feature == null || !feature.HasMeeples) continue;

            if (strategies.TryGetValue(feature.Type, out IFeatureScoringStrategy strategy))
            {
                int points = strategy.CalculateScore(feature, isEndOfGame: true);
                List<Player> winners = feature.GetMajorityOwners();

                if (points > 0 && winners.Count > 0)
                {
                    for (int w = 0; w < winners.Count; w++)
                    {
                        Player winner = winners[w];
                        winner.AddScore(points);
                        Debug.Log($"[Pontuação Final] {winner.PlayerName} ganhou +{points} pts por {feature.Type} no fim do jogo! Total: {winner.Score}");
                    }
                    OnFeatureScored?.Invoke(feature, winners, points);
                }
            }
        }
    }

    private void ReturnMeeplesFromFeature(Board board, ConnectedFeatureResult feature)
    {
        if (feature.MeepleTiles == null) return;

        for (int i = 0; i < feature.MeepleTiles.Count; i++)
        {
            PlacedTile tile = feature.MeepleTiles[i];
            if (tile == null || !tile.HasMeeple) continue;

            Meeple removedMeeple = tile.RemoveMeeple();
            if (tile.Definition != null)
            {
                tile.Definition.temMeeple = false;
                tile.Definition.temMeepleEstrada = false;
                tile.Definition.temMeepleCidade = false;
            }

            if (removedMeeple != null)
            {
                removedMeeple.Owner?.ReturnMeeple(removedMeeple);
                board.NotifyMeepleRemoved(tile, removedMeeple);
            }
        }
    }
}
