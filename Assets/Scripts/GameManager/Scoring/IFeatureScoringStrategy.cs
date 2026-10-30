/// <summary>
/// Interface do padrão GoF Strategy para cálculo polimórfico da pontuação de cada tipo de estrutura
/// do Carcassonne tradicional (tanto durante a partida quanto na contagem final).
/// </summary>
public interface IFeatureScoringStrategy
{
    FeatureType SupportedType { get; }
    int CalculateScore(ConnectedFeatureResult feature, bool isEndOfGame);
}

/// <summary>
/// Regra Oficial de Pontuação de Estrada:
/// - Completa (em jogo) ou Incompleta (fim de jogo): 1 ponto por peça (tile) que compõe a estrada.
/// </summary>
public class RoadScoringStrategy : IFeatureScoringStrategy
{
    public FeatureType SupportedType => FeatureType.Estrada;

    public int CalculateScore(ConnectedFeatureResult feature, bool isEndOfGame)
    {
        if (feature == null || feature.Type != FeatureType.Estrada) return 0;
        if (!isEndOfGame && !feature.IsComplete) return 0;
        return feature.Tiles.Count * 1;
    }
}

/// <summary>
/// Regra Oficial de Pontuação de Cidade:
/// - Completa (em jogo): 2 pontos por peça + 2 pontos por escudo (flâmula).
/// - Incompleta (fim de jogo): 1 ponto por peça + 1 ponto por escudo.
/// </summary>
public class CityScoringStrategy : IFeatureScoringStrategy
{
    public FeatureType SupportedType => FeatureType.Cidade;

    public int CalculateScore(ConnectedFeatureResult feature, bool isEndOfGame)
    {
        if (feature == null || feature.Type != FeatureType.Cidade) return 0;
        if (!isEndOfGame && !feature.IsComplete) return 0;

        int pointsPerTile = feature.IsComplete && !isEndOfGame ? 2 : 1;
        int pointsPerShield = feature.IsComplete && !isEndOfGame ? 2 : 1;

        return (feature.Tiles.Count * pointsPerTile) + (feature.ShieldCount * pointsPerShield);
    }
}

/// <summary>
/// Regra Oficial de Pontuação de Mosteiro:
/// - Completo (em jogo, 9 peças na grade 3x3): 9 pontos.
/// - Incompleto (fim de jogo): 1 ponto pelo mosteiro + 1 ponto por cada peça adjacente na grade 3x3 (1 a 8 pontos).
/// </summary>
public class MonasteryScoringStrategy : IFeatureScoringStrategy
{
    public FeatureType SupportedType => FeatureType.Mosteiro;

    public int CalculateScore(ConnectedFeatureResult feature, bool isEndOfGame)
    {
        if (feature == null || feature.Type != FeatureType.Mosteiro) return 0;
        if (!isEndOfGame && !feature.IsComplete) return 0;
        return feature.Tiles.Count;
    }
}

/// <summary>
/// Regra Oficial de Pontuação de Fazenda / Campo (Camponeses):
/// - Durante o jogo: 0 pontos (fazendeiros permanecem no campo até o fim da partida).
/// - Fim de jogo: 3 pontos por cada Cidade Completa distinta que faz fronteira com o campo conectado.
/// </summary>
public class FarmScoringStrategy : IFeatureScoringStrategy
{
    public FeatureType SupportedType => FeatureType.Campo;

    public int CalculateScore(ConnectedFeatureResult feature, bool isEndOfGame)
    {
        if (feature == null || feature.Type != FeatureType.Campo || !isEndOfGame) return 0;
        return feature.CompletedAdjacentCities * 3;
    }
}
