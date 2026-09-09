using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TilePlacer : MonoBehaviour
{
    public Tilemap tilemap;

    public TileBase validTile;

    public Board board;

    // Array que vai guardar todos os tiles diferentes que você criar
    public CarcassoneTile[] tilePool;

    // O tile atual que o jogador está segurando para colocar
    private CarcassoneTile currentTile;

    public Image image;

    public enum Direcao { Cima = 0, Direita = 1, Baixo = 2, Esquerda = 3 }

    private int currentRotationZ = 0;

    void Start()
    {
        // Sorteia o primeiro tile assim que o jogo começa
        PickRandomTile();
    }

    void Update()
    {
        if(Keyboard.current != null & Keyboard.current.rKey.wasPressedThisFrame)
        {
            rotateCurrentTile();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentTile == null) return; // Proteção caso a lista de tiles esteja vazia

            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            worldPosition.z = 0;

            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

            if (board.IsValidPlacement(currentTile, cellPosition, currentRotationZ))
            {
                tilemap.SetTile(cellPosition, currentTile);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, currentRotationZ*-90), Vector3.one);
                tilemap.SetTransformMatrix(cellPosition, matrix);
                board.addCellToPlacedCells(cellPosition, currentRotationZ);
                currentRotationZ = 0;
                image.transform.rotation = Quaternion.Euler(0, 0, 0);
                PickRandomTile();
            }
            else
            {
                Debug.Log("Movimento inválido: As peças não conectam");
            }
        }
    }


    // Método que escolhe um tile aleatório da nossa lista
    void PickRandomTile()
    {
        if (tilePool.Length > 0)
        {
            int randomIndex = Random.Range(0, tilePool.Length);
            currentTile = tilePool[randomIndex];

            // Cria um pacote vazio de dados de tile
            TileData tileData = new TileData();

            // Pede para o currentTile preencher o pacote (usamos posições vazias pois só queremos o sprite base)
            currentTile.GetTileData(Vector3Int.zero, null, ref tileData);

            // Aplica o sprite extraído do pacote na imagem da tela
            image.sprite = tileData.sprite;

            Debug.Log("Novo tile sorteado: " + currentTile.name);
        }
    }

    void rotateCurrentTile()
    {
        currentRotationZ++;

        if (currentRotationZ > 3)
        {
            currentRotationZ = 0;
        }
        image.transform.Rotate(0, 0, -90f);
    }
}