using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TilePlacer : MonoBehaviour
{
    public Tilemap tilemap;

    // Array que vai guardar todos os tiles diferentes que você criar
    public TileBase[] tilePool;

    // O tile atual que o jogador está segurando para colocar
    private TileBase currentTile;

    void Start()
    {
        // Sorteia o primeiro tile assim que o jogo começa
        PickRandomTile();
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentTile == null) return; // Proteção caso a lista de tiles esteja vazia

            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            worldPosition.z = 0;

            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

            // Coloca o tile sorteado na célula
            tilemap.SetTile(cellPosition, currentTile);

            // Sorteia um novo tile para a próxima jogada
            PickRandomTile();
        }
    }

    // Método que escolhe um tile aleatório da nossa lista
    void PickRandomTile()
    {
        if (tilePool.Length > 0)
        {
            // Random.Range com números inteiros inclui o mínimo (0) e exclui o máximo (Length)
            int randomIndex = Random.Range(0, tilePool.Length);
            currentTile = tilePool[randomIndex];

            Debug.Log("Novo tile sorteado: " + currentTile.name);
        }
    }
}