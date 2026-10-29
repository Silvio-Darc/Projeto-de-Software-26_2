using UnityEngine;
using UnityEngine.Tilemaps;

public enum TipoTerreno { Campo, Estrada, Cidade }
public enum Borda { Cima = 0, Direita = 1, Baixo = 2, Esquerda = 3 }

[CreateAssetMenu(fileName = "Novo Tile", menuName = "Carcassonne/Tile Customizado")]

public class CarcassoneTile: Tile
{ 
    [Header("Informações do Tile")]
    public string nomeDaPeca;
    public bool temEscudo; // Aqueles escudos que dão pontos extras na cidade
    public bool temMosteiro;
    public bool temMeeple; 
    public int rotation = 0;

    [Header("Bordas (Sentido Horário)")]
    public TipoTerreno bordaCima;
    public TipoTerreno bordaDireita;
    public TipoTerreno bordaBaixo;
    public TipoTerreno bordaEsquerda;


    // Você pode adicionar qualquer variável aqui (pontos, dono do tile, etc.)

    public TipoTerreno obterBordaReal(Borda bordaDesejada, int rotacao)
    {
        int indiceOriginal = ((int)bordaDesejada - rotacao + 4) % 4;

        switch (indiceOriginal)
        {
        case 0: return this.bordaCima;
        case 1: return this.bordaDireita;
        case 2: return this.bordaBaixo;
        case 3: return this.bordaEsquerda;
        default: return this.bordaCima;
        }
    }
}
