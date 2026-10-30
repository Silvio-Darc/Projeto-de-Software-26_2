using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resultado da análise de uma componente conexa do tabuleiro (Estrada, Cidade, Mosteiro ou Campo).
/// Responsável também por determinar o(s) jogador(es) com maioria de Meeples na estrutura.
/// </summary>
public class ConnectedFeatureResult
{
    public FeatureType Type { get; }
    public bool IsComplete { get; }
    public HashSet<(Vector3Int position, int featureId)> Nodes { get; }
    public HashSet<Vector3Int> Tiles { get; }
    public int ShieldCount { get; }
    public int CompletedAdjacentCities { get; set; }
    public List<Meeple> Meeples { get; }
    public List<PlacedTile> MeepleTiles { get; }

    public bool HasMeeples => Meeples != null && Meeples.Count > 0;

    public ConnectedFeatureResult(
        FeatureType type,
        bool isComplete,
        HashSet<(Vector3Int position, int featureId)> nodes,
        HashSet<Vector3Int> tiles,
        int shieldCount,
        List<Meeple> meeples,
        List<PlacedTile> meepleTiles,
        int completedAdjacentCities = 0)
    {
        Type = type;
        IsComplete = isComplete;
        Nodes = nodes ?? new HashSet<(Vector3Int position, int featureId)>();
        Tiles = tiles ?? new HashSet<Vector3Int>();
        ShieldCount = shieldCount;
        Meeples = meeples ?? new List<Meeple>();
        MeepleTiles = meepleTiles ?? new List<PlacedTile>();
        CompletedAdjacentCities = completedAdjacentCities;
    }

    /// <summary>
    /// Retorna uma chave canônica única para esta componente conexa (o menor nó lexicográfico),
    /// evitando que a mesma estrutura seja pontuada duas vezes no mesmo turno.
    /// </summary>
    public (Vector3Int position, int featureId) GetCanonicalKey()
    {
        bool first = true;
        (Vector3Int position, int featureId) minNode = (Vector3Int.zero, -1);

        foreach (var node in Nodes)
        {
            if (first || CompareNodes(node, minNode) < 0)
            {
                minNode = node;
                first = false;
            }
        }
        return minNode;
    }

    private static int CompareNodes((Vector3Int pos, int id) a, (Vector3Int pos, int id) b)
    {
        if (a.pos.x != b.pos.x) return a.pos.x.CompareTo(b.pos.x);
        if (a.pos.y != b.pos.y) return a.pos.y.CompareTo(b.pos.y);
        return a.id.CompareTo(b.id);
    }

    /// <summary>
    /// Regra Oficial de Maioria do Carcassonne:
    /// Retorna o jogador com mais seguidores na estrutura. Em caso de empate na maior quantidade,
    /// todos os jogadores empatados recebem os pontos integrais.
    /// </summary>
    public List<Player> GetMajorityOwners()
    {
        List<Player> winners = new List<Player>();
        if (Meeples == null || Meeples.Count == 0) return winners;

        Dictionary<Player, int> counts = new Dictionary<Player, int>();
        int maxCount = 0;

        for (int i = 0; i < Meeples.Count; i++)
        {
            Meeple m = Meeples[i];
            if (m == null || m.Owner == null) continue;

            int weight = Mathf.Max(1, m.Value);
            counts.TryGetValue(m.Owner, out int current);
            int updated = current + weight;
            counts[m.Owner] = updated;

            if (updated > maxCount)
            {
                maxCount = updated;
            }
        }

        foreach (var kvp in counts)
        {
            if (kvp.Value == maxCount && maxCount > 0)
            {
                winners.Add(kvp.Key);
            }
        }

        return winners;
    }
}
