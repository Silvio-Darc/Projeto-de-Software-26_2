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
    public CarcassoneTile currentTile;

    public Image image;

    public enum Direcao { Cima = 0, Direita = 1, Baixo = 2, Esquerda = 3 }

    private int currentRotationZ = 0;

    public InputActionAsset inputs;

    private InputAction Q;
    private InputAction E; 
    private InputAction R;

    private InputAction Space;

    private InputAction leftClick;

    private void Awake()
    {
        Q = inputs.FindActionMap("Teclado").FindAction("Q");
        E = inputs.FindActionMap("Teclado").FindAction("E");
        R = inputs.FindActionMap("Teclado").FindAction("R");
        Space = inputs.FindActionMap("Teclado").FindAction("Espaco");
        leftClick = inputs.FindActionMap("Teclado").FindAction("LeftClick");

        Q.performed += OnQ;
        E.performed += OnE;
        R.performed += OnR;
        Space.performed += OnSpace;
        leftClick.performed += OnLeftClick;
    }

    void OnEnable()
    {
        inputs.FindActionMap("Teclado").Enable();
        Q.performed += OnQ;
        E.performed += OnE;
        R.performed += OnR;
        Space.performed += OnSpace;
        leftClick.performed += OnLeftClick;
    }

    void OnDisable()
    {
        inputs.FindActionMap("Teclado").Disable();
        Q.performed -= OnQ;
        E.performed -= OnE;
        R.performed -= OnR;
        Space.performed -= OnSpace;
        leftClick.performed -= OnLeftClick;
    }

    void Start()
    {
        // Sorteia o primeiro tile assim que o jogo começa
        PickRandomTile();
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

    void PlaceTile()
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

                FinishAction();
            }
            else
            {
                Debug.Log("Movimento inválido: As peças não conectam");
            }
    }

    public void FinishAction()
    {
        // 1. Registra pontos ou ações do jogador atual se houver
        Player current = Turn.Instance.GetCurrentPlayer();
        // 2. Passa a vez para o prox turno
        if (Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarTile)
        {
            Turn.Instance.SetPhase(Turn.TurnPhase.PosicionarMeeple);
        }
        else
        {
            Turn.Instance.EndTurn();
        }
    }

    public void OnQ(InputAction.CallbackContext context)
    {
        rotateCurrentTile();
    }
    public void OnE(InputAction.CallbackContext context)
    {
        currentTile = null;
        PickRandomTile();
    }
    public void OnR(InputAction.CallbackContext context)
    {
        rotateCurrentTile();
    }
    public void OnSpace(InputAction.CallbackContext context)
    {
        if (Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarMeeple)
        {
            SkipOrFinishMeeplePhase();
        }
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        // Se a fase for de colocar Tile:
        if (Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarTile)
        {
            PlaceTile();
        }
        // Se a fase for de colocar Meeple:
        else if (Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarMeeple)
        {
            TryPlaceMeeple();
        }

    }
    
    public void TryPlaceMeeple()
    {
        Player current = Turn.Instance.GetCurrentPlayer();
        if (current != null && current.hasAvailableMeeple())
        {
            // Aqui entra sua lógica de instanciar o meeple sobre a 'lastPlacedCell'
            Debug.Log($"Meeple posicionado pelo {current.GetPlayerName()}!");
            
            // Finaliza o turno após colocar o meeple
            SkipOrFinishMeeplePhase();
        }
        else
        {
            Debug.Log("Jogador não possui meeples disponíveis!");
        }
    }
    // Chame essa função pelo clique de um Botão UI "Pular Meeple" ou tecla Espaço
    public void SkipOrFinishMeeplePhase()
    {
        if (Turn.Instance.CurrentPhase != Turn.TurnPhase.PosicionarMeeple) return;
        // 1. Sorteia o tile para a próxima rodada
        PickRandomTile();
        // 2. Passa o turno para o próximo jogador
        Turn.Instance.EndTurn();
    }
}