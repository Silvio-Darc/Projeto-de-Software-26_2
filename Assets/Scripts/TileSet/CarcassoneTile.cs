using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TipoTerreno { Campo = 0, Estrada = 1, Cidade = 2 }
public enum Borda { Cima = 0, Direita = 1, Baixo = 2, Esquerda = 3 }

/// <summary>
/// Definição de um tipo de peça do Carcassonne (Padrão GoF Flyweight & GRASP Information Expert).
/// Armazena tanto as 4 bordas ortogonais quanto as sub-regiões internas (12 sub-portas) para Cidades, Estradas, Mosteiros e Campos.
/// </summary>
[CreateAssetMenu(fileName = "Novo Tile", menuName = "Carcassonne/Tile Customizado")]
public class CarcassoneTile : Tile
{
    [Header("Informações do Tile")]
    public string nomeDaPeca;
    public int quantidadeNoBaralho = 1;
    public bool ehPecaInicial = false;
    public bool temEscudo;
    public bool temMosteiro;

    // Mantidos para compatibilidade com código legado
    [HideInInspector] public bool temMeeple = false;
    [HideInInspector] public bool temMeepleEstrada = false;
    [HideInInspector] public bool temMeepleCidade = false;
    [HideInInspector] public int rotation = 0;

    [Header("Bordas (Sentido Horário)")]
    public TipoTerreno bordaCima;
    public TipoTerreno bordaDireita;
    public TipoTerreno bordaBaixo;
    public TipoTerreno bordaEsquerda;

    [Header("Topologia Interna (Sub-regiões e 12 Sub-portas)")]
    [SerializeField] private List<TileFeatureDefinition> features = new List<TileFeatureDefinition>();

    public IReadOnlyList<TileFeatureDefinition> Features
    {
        get
        {
            EnsureTopologyInitialized();
            return features;
        }
    }

    public void EnsureTopologyInitialized()
    {
        if (features == null || features.Count == 0)
        {
            TileTopologyCatalog.ConfigureTileTopology(this);
        }
    }

    public void SetTopologyData(string tileName, int quantity, bool isStartingTile, List<TileFeatureDefinition> newFeatures)
    {
        if (!string.IsNullOrWhiteSpace(tileName))
        {
            nomeDaPeca = tileName;
        }
        quantidadeNoBaralho = Mathf.Max(1, quantity);
        ehPecaInicial = isStartingTile;
        features = newFeatures ?? new List<TileFeatureDefinition>();
    }

    public TipoTerreno obterBordaReal(Borda bordaDesejada, int rotacao)
    {
        int indiceOriginal = (((int)bordaDesejada - rotacao) % 4 + 4) % 4;

        switch (indiceOriginal)
        {
            case 0: return bordaCima;
            case 1: return bordaDireita;
            case 2: return bordaBaixo;
            case 3: return bordaEsquerda;
            default: return bordaCima;
        }
    }

    /// <summary>
    /// Retorna a sub-região interna pelo seu ID local na peça.
    /// </summary>
    public TileFeatureDefinition GetFeatureById(int localFeatureId)
    {
        EnsureTopologyInitialized();
        for (int i = 0; i < features.Count; i++)
        {
            if (features[i].LocalFeatureId == localFeatureId)
            {
                return features[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Retorna a sub-região interna que ocupa a sub-porta do mundo (0..11) considerando a rotação da peça.
    /// </summary>
    public TileFeatureDefinition GetFeatureAtWorldPort(int worldPort, int tileRotation)
    {
        EnsureTopologyInitialized();
        int localPort = TileFeatureDefinition.WorldPortToLocalPort(worldPort, tileRotation);
        for (int i = 0; i < features.Count; i++)
        {
            if (features[i].OccupiesLocalPort(localPort))
            {
                return features[i];
            }
        }
        return null;
    }

    public CarcassoneTile GetTile()
    {
        EnsureTopologyInitialized();
        CarcassoneTile clone = ScriptableObject.CreateInstance<CarcassoneTile>();
        clone.name = this.name;
        clone.sprite = this.sprite;
        clone.color = this.color;
        clone.transform = this.transform;
        clone.flags = this.flags;
        clone.colliderType = this.colliderType;
        clone.nomeDaPeca = this.nomeDaPeca;
        clone.quantidadeNoBaralho = this.quantidadeNoBaralho;
        clone.ehPecaInicial = this.ehPecaInicial;
        clone.temEscudo = this.temEscudo;
        clone.temMosteiro = this.temMosteiro;
        clone.temMeeple = this.temMeeple;
        clone.rotation = this.rotation;
        clone.bordaCima = this.bordaCima;
        clone.bordaDireita = this.bordaDireita;
        clone.bordaBaixo = this.bordaBaixo;
        clone.bordaEsquerda = this.bordaEsquerda;
        clone.features = new List<TileFeatureDefinition>(this.features);

        return clone;
    }
}
