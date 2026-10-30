using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Gerencia a malha espacial de peças posicionadas no tabuleiro.
/// Aplica o padrão GRASP Information Expert com busca O(1) via Dictionary
/// e remove acoplamentos indevidos com namespaces de Editor.
/// </summary>
public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase initialTile;

    private readonly Dictionary<Vector3Int, PlacedTile> placedTiles = new Dictionary<Vector3Int, PlacedTile>();
    private readonly HashSet<Vector3Int> openBorderCells = new HashSet<Vector3Int>();

    public event Action<PlacedTile> OnTilePlaced;
    public event Action<PlacedTile, Meeple> OnMeeplePlaced;
    public event Action<PlacedTile, Meeple> OnMeepleRemoved;

    public IReadOnlyDictionary<Vector3Int, PlacedTile> PlacedTiles => placedTiles;
    public IReadOnlyCollection<Vector3Int> OpenBorderCells => openBorderCells;

    private readonly Vector3Int[] vizinhosDirs = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),  // Cima (0)
        new Vector3Int(1, 0, 0),  // Direita (1)
        new Vector3Int(0, -1, 0), // Baixo (2)
        new Vector3Int(-1, 0, 0)  // Esquerda (3)
    };

    private readonly Borda[] bordasOpostas = new Borda[]
    {
        Borda.Baixo,    // Oposta a Cima
        Borda.Esquerda, // Oposta a Direita
        Borda.Cima,     // Oposta a Baixo
        Borda.Direita   // Oposta a Esquerda
    };

    private void Start()
    {
        if (placedTiles.Count == 0)
        {
            addCellToPlacedCells(Vector3Int.zero, 0);
        }
    }

    public bool HasPlacedTileAt(Vector3Int position)
    {
        return placedTiles.ContainsKey(position);
    }

    public PlacedTile GetPlacedTileAt(Vector3Int position)
    {
        placedTiles.TryGetValue(position, out PlacedTile placed);
        return placed;
    }

    private void UpdateEdges()
    {
        openBorderCells.Clear();

        foreach (Vector3Int pos in placedTiles.Keys)
        {
            for (int i = 0; i < vizinhosDirs.Length; i++)
            {
                Vector3Int target = pos + vizinhosDirs[i];
                if (!placedTiles.ContainsKey(target))
                {
                    openBorderCells.Add(target);
                    if (tilemap != null && initialTile != null)
                    {
                        tilemap.SetTile(target, initialTile);
                    }
                }
            }
        }
    }

    public bool IsValidPlacement(CarcassoneTile currentTile, Vector3Int position, int rotation)
    {
        if (currentTile == null) return false;

        // Regra 1: Verifica se já existe uma peça definitiva posicionada nesta coordenada
        if (placedTiles.ContainsKey(position))
        {
            return false;
        }

        if (tilemap != null && tilemap.HasTile(position))
        {
            TileBase existingTile = tilemap.GetTile(position);
            if (existingTile != null && existingTile != initialTile && !existingTile.name.Equals("PossibleGrid"))
            {
                return false;
            }
        }

        bool hasValidNeighbor = false;

        for (int i = 0; i < 4; i++)
        {
            Vector3Int neighborPos = position + vizinhosDirs[i];
            if (placedTiles.TryGetValue(neighborPos, out PlacedTile neighborPlaced))
            {
                hasValidNeighbor = true;
                CarcassoneTile neighborDef = neighborPlaced.Definition ?? (tilemap != null ? tilemap.GetTile<CarcassoneTile>(neighborPos) : null);
                if (neighborDef == null) continue;

                Borda currentEdge = (Borda)i;
                Borda neighborEdge = bordasOpostas[i];

                if (neighborDef.obterBordaReal(neighborEdge, neighborPlaced.Rotation) !=
                    currentTile.obterBordaReal(currentEdge, rotation))
                {
                    return false;
                }
            }
        }

        if (!hasValidNeighbor && placedTiles.Count > 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica se existe ao menos uma posição de borda aberta e uma rotação (0..3) onde a peça possa ser colocada.
    /// Usado pelo TileDeck para descartar peças impossíveis de encaixar.
    /// </summary>
    public bool HasAnyValidPlacement(CarcassoneTile tile)
    {
        if (tile == null) return false;
        if (placedTiles.Count == 0) return true;

        foreach (Vector3Int candidatePos in openBorderCells)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                if (IsValidPlacement(tile, candidatePos, rot))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Retorna todas as combinações válidas (posição, rotação) para a peça informada.
    /// Utilizado tanto para destaque visual quanto pelas estratégias de Bot (IA).
    /// </summary>
    public List<(Vector3Int position, int rotation)> GetAllValidPlacements(CarcassoneTile tile)
    {
        List<(Vector3Int position, int rotation)> validMoves = new List<(Vector3Int position, int rotation)>();
        if (tile == null) return validMoves;

        if (placedTiles.Count == 0)
        {
            validMoves.Add((Vector3Int.zero, 0));
            return validMoves;
        }

        foreach (Vector3Int candidatePos in openBorderCells)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                if (IsValidPlacement(tile, candidatePos, rot))
                {
                    validMoves.Add((candidatePos, rot));
                }
            }
        }
        return validMoves;
    }

    public PlacedTile RegisterPlacedTile(CarcassoneTile definition, Vector3Int position, int rotation)
    {
        CarcassoneTile resolvedDef = definition;
        if (resolvedDef == null && tilemap != null)
        {
            resolvedDef = tilemap.GetTile<CarcassoneTile>(position);
        }

        PlacedTile placed = new PlacedTile(position, rotation, resolvedDef);
        placedTiles[position] = placed;
        openBorderCells.Remove(position);
        UpdateEdges();
        OnTilePlaced?.Invoke(placed);
        return placed;
    }

    public void addCellToPlacedCells(Vector3Int vector, int rotation)
    {
        CarcassoneTile tileAtPos = tilemap != null ? tilemap.GetTile<CarcassoneTile>(vector) : null;
        RegisterPlacedTile(tileAtPos, vector, rotation);
    }

    public int GetRotationAt(Vector3Int cellPosition)
    {
        if (placedTiles.TryGetValue(cellPosition, out PlacedTile placed))
        {
            return placed.Rotation;
        }
        return 0;
    }

    public void NotifyMeeplePlaced(PlacedTile tile, Meeple meeple)
    {
        OnMeeplePlaced?.Invoke(tile, meeple);
    }

    public void NotifyMeepleRemoved(PlacedTile tile, Meeple meeple)
    {
        OnMeepleRemoved?.Invoke(tile, meeple);
    }

    /// <summary>
    /// Percorre a rede conectada de um terreno (Estrada ou Cidade)
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
            CarcassoneTile tileAtual = GetPlacedTileAt(atualPos)?.Definition ?? (tilemap != null ? tilemap.GetTile<CarcassoneTile>(atualPos) : null);
            if (tileAtual == null) continue;

            if (atualPos != startPos)
            {
                if (tipoProcurado == TipoTerreno.Estrada && (tileAtual.temMeepleEstrada || tileAtual.temMeeple)) return true;
                if (tipoProcurado == TipoTerreno.Cidade && (tileAtual.temMeepleCidade || tileAtual.temMeeple)) return true;
            }

            for (int i = 0; i < 4; i++)
            {
                Borda bordaAtual = (Borda)i;
                if (tileAtual.obterBordaReal(bordaAtual, rotAtual) != tipoProcurado) continue;

                Vector3Int vizinhoPos = atualPos + vizinhosDirs[i];
                if (visitados.Contains(vizinhoPos)) continue;

                CarcassoneTile tileVizinho = GetPlacedTileAt(vizinhoPos)?.Definition ??
                                             (tilemap != null && tilemap.HasTile(vizinhoPos) ? tilemap.GetTile<CarcassoneTile>(vizinhoPos) : null);
                if (tileVizinho != null)
                {
                    int rotVizinho = GetRotationAt(vizinhoPos);
                    Borda bordaConexaoVizinho = bordasOpostas[i];
                    if (tileVizinho.obterBordaReal(bordaConexaoVizinho, rotVizinho) == tipoProcurado)
                    {
                        visitados.Add(vizinhoPos);
                        fila.Enqueue(vizinhoPos);
                    }
                }
            }
        }

        return false;
    }
}
