using UnityEngine;

/// <summary>
/// Representa o estado extrínseco de uma peça já posicionada no tabuleiro (Board).
/// Separa a definição imutável do catálogo (CarcassoneTile - Padrão GoF Flyweight)
/// do estado mutável da partida (posição, rotação e Meeple alocado).
/// </summary>
public class PlacedTile
{
    public Vector3Int Position { get; }
    public int Rotation { get; }
    public CarcassoneTile Definition { get; }
    public Meeple PlacedMeeple { get; private set; }
    public int PlacedFeatureId { get; private set; } = -1;

    public bool HasMeeple => PlacedMeeple != null;

    public PlacedTile(Vector3Int position, int rotation, CarcassoneTile definition)
    {
        Position = position;
        Rotation = ((rotation % 4) + 4) % 4;
        Definition = definition;
    }

    /// <summary>
    /// Retorna o TipoTerreno na borda solicitada do mundo levando em conta a rotação desta instância.
    /// (GRASP: Information Expert)
    /// </summary>
    public TipoTerreno GetEdgeTerrain(Borda worldEdge)
    {
        if (Definition == null) return TipoTerreno.Campo;
        return Definition.obterBordaReal(worldEdge, Rotation);
    }

    public void AssignMeeple(Meeple meeple, int featureId, MeepleRole role)
    {
        PlacedMeeple = meeple;
        PlacedFeatureId = featureId;
        if (meeple != null)
        {
            meeple.AssignToFeature(Position, featureId, role);
            meeple.Owner?.NotifyMeeplePlaced();
        }
    }

    public Meeple RemoveMeeple()
    {
        Meeple removed = PlacedMeeple;
        PlacedMeeple = null;
        PlacedFeatureId = -1;
        return removed;
    }
}
