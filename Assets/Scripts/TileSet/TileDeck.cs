using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Especialista na Informação (GRASP Information Expert & Creator) para o baralho de 72 peças do Carcassonne.
/// Gerencia a construção da pilha embaralhada, separação da peça inicial e descarte automático de peças sem encaixe válido.
/// </summary>
public class TileDeck
{
    private readonly List<CarcassoneTile> drawPile = new List<CarcassoneTile>();

    public event Action<int> OnDeckCountChanged;
    public event Action<CarcassoneTile> OnUnplayableTileDiscarded;

    public int RemainingCount => drawPile.Count;
    public bool IsEmpty => drawPile.Count == 0;
    public CarcassoneTile StartingTile { get; private set; }

    /// <summary>
    /// Inicializa o baralho a partir do catálogo de 24 tipos de peças, embaralhando as 71 peças de compra
    /// e reservando 1 cópia da peça inicial (Cidade_Estrada_Reto) para a origem (0,0).
    /// </summary>
    public void Initialize(IEnumerable<CarcassoneTile> tileCatalog, bool reserveStartingTile = true)
    {
        drawPile.Clear();
        StartingTile = null;

        if (tileCatalog == null)
        {
            OnDeckCountChanged?.Invoke(0);
            return;
        }

        HashSet<string> processedTileNames = new HashSet<string>();

        foreach (CarcassoneTile tile in tileCatalog)
        {
            if (tile == null) continue;
            if (!processedTileNames.Add(tile.name)) continue;

            TileTopologyCatalog.ConfigureTileTopology(tile);
            int copies = tile.quantidadeNoBaralho;

            if (reserveStartingTile && StartingTile == null && tile.ehPecaInicial)
            {
                StartingTile = tile;
                copies = Mathf.Max(0, copies - 1);
            }

            for (int i = 0; i < copies; i++)
            {
                drawPile.Add(tile);
            }
        }

        Shuffle();
        OnDeckCountChanged?.Invoke(drawPile.Count);
    }

    /// <summary>
    /// Embaralhamento Fisher-Yates uniforme.
    /// </summary>
    public void Shuffle()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            CarcassoneTile temp = drawPile[i];
            drawPile[i] = drawPile[j];
            drawPile[j] = temp;
        }
    }

    /// <summary>
    /// Sorteia a próxima peça do baralho. Caso um Board seja informado, aplica a regra oficial do Carcassonne:
    /// descarta automaticamente qualquer peça que não possua ao menos um encaixe válido nas 4 rotações.
    /// </summary>
    public CarcassoneTile DrawNextTile(Board board = null)
    {
        while (drawPile.Count > 0)
        {
            int lastIndex = drawPile.Count - 1;
            CarcassoneTile drawn = drawPile[lastIndex];
            drawPile.RemoveAt(lastIndex);

            if (board == null || board.HasAnyValidPlacement(drawn))
            {
                OnDeckCountChanged?.Invoke(drawPile.Count);
                return drawn;
            }

            Debug.LogWarning($"[TileDeck] Peça '{drawn.name}' descartada automaticamente por não possuir encaixe válido no tabuleiro.");
            OnUnplayableTileDiscarded?.Invoke(drawn);
        }

        OnDeckCountChanged?.Invoke(0);
        return null;
    }
}
