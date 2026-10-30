using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define uma sub-região interna de uma peça (Cidade, Estrada, Campo ou Mosteiro)
/// usando o modelo topológico de 12 sub-portas de borda (3 por lado em sentido horário):
///   Cima (Norte):     0 (Esq), 1 (Centro), 2 (Dir)
///   Direita (Leste):  3 (Cima), 4 (Centro), 5 (Baixo)
///   Baixo (Sul):      6 (Dir), 7 (Centro), 8 (Esq)
///   Esquerda (Oeste): 9 (Baixo), 10 (Centro), 11 (Cima)
/// </summary>
[Serializable]
public class TileFeatureDefinition
{
    [SerializeField] private int localFeatureId;
    [SerializeField] private string label;
    [SerializeField] private FeatureType featureType;
    [SerializeField] private List<int> connectedPorts = new List<int>();
    [SerializeField] private List<int> adjacentCityFeatureIds = new List<int>();
    [SerializeField] private Vector2 pinLocalOffset;

    public int LocalFeatureId => localFeatureId;
    public string Label => label;
    public FeatureType Type => featureType;
    public IReadOnlyList<int> ConnectedPorts => connectedPorts;
    public IReadOnlyList<int> AdjacentCityFeatureIds => adjacentCityFeatureIds;
    public Vector2 PinLocalOffset => pinLocalOffset;

    public TileFeatureDefinition(
        int id,
        string featureLabel,
        FeatureType type,
        int[] ports,
        Vector2 pinOffset,
        int[] adjacentCities = null)
    {
        localFeatureId = id;
        label = featureLabel;
        featureType = type;
        connectedPorts = ports != null ? new List<int>(ports) : new List<int>();
        adjacentCityFeatureIds = adjacentCities != null ? new List<int>(adjacentCities) : new List<int>();
        pinLocalOffset = pinOffset;
    }

    public bool OccupiesLocalPort(int localPort)
    {
        return connectedPorts != null && connectedPorts.Contains(localPort);
    }

    public MeepleRole GetCorrespondingMeepleRole()
    {
        switch (featureType)
        {
            case FeatureType.Campo: return MeepleRole.FazendeiroCampo;
            case FeatureType.Estrada: return MeepleRole.LadraoEstrada;
            case FeatureType.Cidade: return MeepleRole.CavaleiroCidade;
            case FeatureType.Mosteiro: return MeepleRole.MongeMosteiro;
            default: return MeepleRole.Disponivel;
        }
    }

    // Tabela de portas opostas no vizinho ortogonal (0..11)
    private static readonly int[] OppositePorts = new int[12]
    {
        8, 7, 6,    // Cima (0,1,2) <-> Baixo do vizinho (8,7,6)
        11, 10, 9,  // Direita (3,4,5) <-> Esquerda do vizinho (11,10,9)
        2, 1, 0,    // Baixo (6,7,8) <-> Cima do vizinho (2,1,0)
        5, 4, 3     // Esquerda (9,10,11) <-> Direita do vizinho (5,4,3)
    };

    /// <summary>
    /// Converte uma porta local (sem rotação) para a porta correspondente no mundo após aplicar a rotação horária (0..3).
    /// </summary>
    public static int LocalPortToWorldPort(int localPort, int rotation)
    {
        int normRot = ((rotation % 4) + 4) % 4;
        return (localPort + normRot * 3) % 12;
    }

    /// <summary>
    /// Converte uma porta do mundo (0..11) para o índice de porta local da peça antes da rotação.
    /// </summary>
    public static int WorldPortToLocalPort(int worldPort, int rotation)
    {
        int normRot = ((rotation % 4) + 4) % 4;
        return (worldPort - normRot * 3 + 12) % 12;
    }

    /// <summary>
    /// Retorna a direção (Borda: 0=Cima, 1=Direita, 2=Baixo, 3=Esquerda) à qual pertence uma porta (0..11).
    /// </summary>
    public static Borda GetEdgeFromPort(int port)
    {
        return (Borda)((port / 3) % 4);
    }

    /// <summary>
    /// Retorna a porta correspondente na peça vizinha que faz fronteira com worldPort.
    /// </summary>
    public static int GetOppositeWorldPort(int worldPort)
    {
        return OppositePorts[((worldPort % 12) + 12) % 12];
    }

    /// <summary>
    /// Rotaciona o offset local do marcador visual do Meeple conforme a rotação horária (0..3) da peça.
    /// </summary>
    public static Vector2 RotateLocalOffset(Vector2 localOffset, int rotation)
    {
        int normRot = ((rotation % 4) + 4) % 4;
        switch (normRot)
        {
            case 0: return localOffset;
            case 1: return new Vector2(localOffset.y, -localOffset.x);
            case 2: return new Vector2(-localOffset.x, -localOffset.y);
            case 3: return new Vector2(-localOffset.y, localOffset.x);
            default: return localOffset;
        }
    }
}
