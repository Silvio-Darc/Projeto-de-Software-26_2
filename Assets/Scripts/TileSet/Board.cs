using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using System;


public class Board : MonoBehaviour
{
    public Tilemap tilemap;

    public TileBase initialTile;

    //posição e rotação do tile
    private List<(Vector3Int, int)> placedCells = new List<(Vector3Int, int)>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        addCellToPlacedCells(Vector3Int.zero, 0);
        updateEdges();
    }

    // Método para atualizar os edges do mapa
    void updateEdges()
    {
        // As 4 direções válidas (Cruz: Cima, Baixo, Esquerda, Direita)
        Vector3Int[] directions = new Vector3Int[] {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0)
        };

        // Use "var (pos, valor)" para já extrair a posição e não precisar usar .Item1 o tempo todo
        foreach (var (pos, valor) in placedCells)
        {
            // Verifica apenas as 4 posições vizinhas ortogonais
            foreach (Vector3Int dir in directions)
            {
                Vector3Int target = pos + dir;

                // Mudei o nome da variável no lambda de "cell" para "c" para evitar o conflito
                if (!placedCells.Exists(c => c.Item1 == target))
                {
                    tilemap.SetTile(target, initialTile);
                }
            }
        }
    }

    public bool IsValidPlacement(CarcassoneTile currentTile, Vector3Int position, int rotation)
    {
        // Regra 1: Verifica se já existe uma peça definitiva aqui
        if (tilemap.HasTile(position) && !tilemap.GetTile(position).name.Equals("PossibleGrid"))
        {
            Debug.Log("Movimento inválido: Já existe uma peça aqui!");
            return false;
        }

        Vector3Int up = position + new Vector3Int(0, 1, 0);
        Vector3Int down = position + new Vector3Int(0, -1, 0);
        Vector3Int left = position + new Vector3Int(-1, 0, 0);
        Vector3Int right = position + new Vector3Int(1, 0, 0);

        bool hasValidNeighbor = false; // Garante que encostou em pelo menos 1 peça real

        // Cima
        int index = placedCells.FindIndex(c => c.Item1 == up);
        if (index != -1)
        {
            hasValidNeighbor = true;
            CarcassoneTile tile = (CarcassoneTile)tilemap.GetTile(up);
            if (tile.obterBordaReal(Borda.Baixo, placedCells[index].Item2) != currentTile.obterBordaReal(Borda.Cima, rotation)) return false;
        }

        // Baixo
        index = placedCells.FindIndex(c => c.Item1 == down);
        if (index != -1)
        {
            hasValidNeighbor = true;
            CarcassoneTile tile = (CarcassoneTile)tilemap.GetTile(down);
            if (tile.obterBordaReal(Borda.Cima, placedCells[index].Item2) != currentTile.obterBordaReal(Borda.Baixo, rotation)) return false;
        }

        // Esquerda
        index = placedCells.FindIndex(c => c.Item1 == left);
        if (index != -1)
        {
            hasValidNeighbor = true;
            CarcassoneTile tile = (CarcassoneTile)tilemap.GetTile(left);
            if (tile.obterBordaReal(Borda.Direita, placedCells[index].Item2) != currentTile.obterBordaReal(Borda.Esquerda, rotation)) return false;
        }

        // Direita
        index = placedCells.FindIndex(c => c.Item1 == right);
        if (index != -1)
        {
            hasValidNeighbor = true;
            CarcassoneTile tile = (CarcassoneTile)tilemap.GetTile(right); // Consertado! Estava up
            if (tile.obterBordaReal(Borda.Esquerda, placedCells[index].Item2) != currentTile.obterBordaReal(Borda.Direita, rotation)) return false;
        }

        if (!hasValidNeighbor && placedCells.Count > 0)
        {
            Debug.Log("Movimento inválido: A peça precisa estar conectada a outra!");
            return false;
        }

        return true;
    }

    public void addCellToPlacedCells(Vector3Int vector, int rotation)
    {
        placedCells.Add((vector, rotation));
        //foreach ((Vector3Int, int) v in placedCells) Debug.Log(v);
        updateEdges();
    }

    public int GetRotationAt(Vector3Int cellPosition)
    {
        int index = placedCells.FindIndex(c => c.Item1 == cellPosition);
        if (index != -1)
        {
            return placedCells[index].Item2;
        }
        return 0;
    }
    private readonly Vector3Int[] vizinhosDirs = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),  // Cima (0)
        new Vector3Int(1, 0, 0),  // Direita (1)
        new Vector3Int(0, -1, 0), // Baixo (2)
        new Vector3Int(-1, 0, 0)  // Esquerda (3)
    };
    // Bordas opostas: Cima <-> Baixo, Direita <-> Esquerda
    private readonly Borda[] bordasOpostas = new Borda[]
    {
        Borda.Baixo,   // Oposta a Cima
        Borda.Esquerda,// Oposta a Direita
        Borda.Cima,    // Oposta a Baixo
        Borda.Direita  // Oposta a Esquerda
    };
    /// <summary>
    /// Percorre toda a rede conectada de um terreno (Estrada ou Cidade)
    /// e retorna 'true' se encontrar algum Meeple já posicionado nela.
    /// </summary>
    public bool RedeJaPossuiMeeple(Vector3Int startPos, TipoTerreno tipoProcurado)
    {
        Queue<Vector3Int> fila = new Queue<Vector3Int>();
        HashSet<Vector3Int> visitados = new HashSet<Vector3Int>();
        fila.Enqueue(startPos);
        visitados.Add(startPos);
        while (fila.Count > 0)
        {
            Vector3Int atualPos = fila.Dequeue();
            int rotAtual = GetRotationAt(atualPos);
            CarcassoneTile tileAtual = tilemap.GetTile<CarcassoneTile>(atualPos);
            if (tileAtual == null) continue;
            // Se encontrou um tile nesta rede que tem meeple (ignorando a peça que acabou de ser colocada na startPos)
            if (atualPos != startPos)
            {
                if (tipoProcurado == TipoTerreno.Estrada && (tileAtual.temMeepleEstrada || tileAtual.temMeeple)) return true;
                if (tipoProcurado == TipoTerreno.Cidade && (tileAtual.temMeepleCidade || tileAtual.temMeeple)) return true;
                
                // Caso esteja usando o temMeeple comum:
                // if (tileAtual.temMeeple) return true;
            }
            // Checa os 4 vizinhos conectados
            for (int i = 0; i < 4; i++)
            {
                Borda bordaAtual = (Borda)i;
                // Se a borda atual não for do terreno procurado, pula
                if (tileAtual.obterBordaReal(bordaAtual, rotAtual) != tipoProcurado) continue;
                Vector3Int vizinhoPos = atualPos + vizinhosDirs[i];
                if (visitados.Contains(vizinhoPos)) continue;
                if (tilemap.HasTile(vizinhoPos) && tilemap.GetTile(vizinhoPos) is CarcassoneTile tileVizinho)
                {
                    int rotVizinho = GetRotationAt(vizinhoPos);
                    Borda bordaConexaoVizinho = bordasOpostas[i];
                    // Verifica se a borda do vizinho também conecta com o mesmo tipo de terreno
                    if (tileVizinho.obterBordaReal(bordaConexaoVizinho, rotVizinho) == tipoProcurado)
                    {
                        visitados.Add(vizinhoPos);
                        fila.Enqueue(vizinhoPos);
                    }
                }
            }
        }
        return false; // Nenhuma peça da rede conectada possui meeple
    }

}
