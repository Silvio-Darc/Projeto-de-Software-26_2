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

    // Update is called once per frame
    void Update()
    {
        
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
}
