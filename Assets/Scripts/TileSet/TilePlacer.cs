using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Atua como a camada de Visão e Input do Tabuleiro (BoardInputView).
/// Captura interações de teclado/mouse do jogador humano e despacha objetos IGameCommand
/// (PlaceTileCommand, PlaceMeepleCommand, SkipMeepleCommand) para o controlador Turn.
/// </summary>
public class TilePlacer : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase validTile;
    public Board board;

    // Catálogo dos 24 tipos de tiles do jogo
    public CarcassoneTile[] tilePool;

    public List<CarcassoneTile> Baralho = new List<CarcassoneTile>();

    public TileDeck Deck { get; private set; } = new TileDeck();

    // O tile atual que o jogador está segurando para colocar
    public CarcassoneTile currentTile;

    public Image image;

    public enum Direcao { Cima = 0, Direita = 1, Baixo = 2, Esquerda = 3 }

    private int currentRotationZ = 0;
    private bool deckInitialized = false;

    private Vector3Int lastPlacedPos = Vector3Int.zero;
    private Vector3Int lastPlacedTile = Vector3Int.zero;

    private bool lastTileEstradaOcupada = false;
    private bool lastTileCidadeOcupada = false;

    public InputActionAsset inputs;

    private InputAction Q;
    private InputAction E;
    private InputAction R;
    private InputAction Space;
    private InputAction leftClick;

    [SerializeField] private GameObject popupMeeple;

    public int CurrentRotation => currentRotationZ;

    private void Awake()
    {
        if (inputs == null) return;
        InputActionMap actionMap = inputs.FindActionMap("Teclado");
        if (actionMap == null) return;

        Q = actionMap.FindAction("Q");
        E = actionMap.FindAction("E");
        R = actionMap.FindAction("R");
        Space = actionMap.FindAction("Espaco");
        leftClick = actionMap.FindAction("LeftClick");
    }

    private void OnEnable()
    {
        if (inputs == null) return;
        inputs.FindActionMap("Teclado")?.Enable();
        if (Q != null) Q.performed += OnQ;
        if (E != null) E.performed += OnE;
        if (R != null) R.performed += OnR;
        if (Space != null) Space.performed += OnSpace;
        if (leftClick != null) leftClick.performed += OnLeftClick;
    }

    private void OnDisable()
    {
        if (inputs == null) return;
        if (Q != null) Q.performed -= OnQ;
        if (E != null) E.performed -= OnE;
        if (R != null) R.performed -= OnR;
        if (Space != null) Space.performed -= OnSpace;
        if (leftClick != null) leftClick.performed -= OnLeftClick;
        inputs.FindActionMap("Teclado")?.Disable();
    }

    private void Start()
    {
        EnsureDeckInitialized();

        // Caso Turn não tenha iniciado um tile ainda (ex.: cena de teste isolada), sorteia o primeiro
        if (currentTile == null)
        {
            DrawTileForTurn();
        }
    }

    public void EnsureDeckInitialized()
    {
        if (deckInitialized) return;
        deckInitialized = true;

        Deck.Initialize(tilePool, reserveStartingTile: true);

        if (Deck.StartingTile != null && tilemap != null)
        {
            CarcassoneTile startInstance = Instantiate(Deck.StartingTile);
            startInstance.EnsureTopologyInitialized();
            tilemap.SetTile(Vector3Int.zero, startInstance);
            if (board != null)
            {
                board.RegisterPlacedTile(startInstance, Vector3Int.zero, 0);
            }
        }
    }

    public CarcassoneTile DrawTileForTurn()
    {
        EnsureDeckInitialized();
        if (currentTile != null)
        {
            UpdatePreviewSprite();
            return currentTile;
        }

        PickRandomTile();
        return currentTile;
    }

    public void PickRandomTile()
    {
        EnsureDeckInitialized();
        currentRotationZ = 0;
        if (image != null)
        {
            image.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        currentTile = Deck.DrawNextTile(board);
        UpdatePreviewSprite();
    }

    private void UpdatePreviewSprite()
    {
        if (currentTile != null)
        {
            TileData tileData = new TileData();
            currentTile.GetTileData(Vector3Int.zero, null, ref tileData);

            if (image != null)
            {
                image.sprite = tileData.sprite;
                image.enabled = true;
            }
        }
        else if (image != null)
        {
            image.enabled = false;
        }
    }

    private void rotateCurrentTile()
    {
        currentRotationZ = (currentRotationZ + 1) % 4;
        if (image != null)
        {
            image.transform.rotation = Quaternion.Euler(0, 0, currentRotationZ * -90f);
        }
    }

    private Vector3Int GetCellPos()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0;
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        lastPlacedPos = cellPosition;
        return cellPosition;
    }

    /// <summary>
    /// Chamado pelo PlaceTileCommand (via Turn) para efetivar o posicionamento no Tilemap e no Board.
    /// </summary>
    public PlacedTile ExecuteTilePlacementOnBoard(Vector3Int cellPosition, int rotation)
    {
        if (currentTile == null || board == null || !board.IsValidPlacement(currentTile, cellPosition, rotation))
        {
            return null;
        }

        CarcassoneTile tileInstance = Instantiate(currentTile);
        tileInstance.EnsureTopologyInitialized();
        tileInstance.temMeeple = false;
        tileInstance.temMeepleEstrada = false;
        tileInstance.temMeepleCidade = false;

        tilemap.SetTile(cellPosition, tileInstance);
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation * -90f), Vector3.one);
        tilemap.SetTransformMatrix(cellPosition, matrix);
        PlacedTile placed = board.RegisterPlacedTile(tileInstance, cellPosition, rotation);

        lastTileEstradaOcupada = board.RedeJaPossuiMeeple(cellPosition, TipoTerreno.Estrada);
        lastTileCidadeOcupada = board.RedeJaPossuiMeeple(cellPosition, TipoTerreno.Cidade);

        if (lastTileEstradaOcupada) tileInstance.temMeepleEstrada = true;
        if (lastTileCidadeOcupada) tileInstance.temMeepleCidade = true;

        lastPlacedPos = cellPosition;
        lastPlacedTile = cellPosition;

        // Consome o tile posicionado para que o próximo turno compre um novo
        currentTile = null;
        currentRotationZ = 0;
        if (image != null)
        {
            image.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        return placed;
    }

    public bool IsFeatureOccupiedInConnectedGraph(PlacedTile placed, TileFeatureDefinition feature)
    {
        if (placed == null || feature == null || board == null) return true;
        if (feature.Type == FeatureType.Estrada)
        {
            return board.RedeJaPossuiMeeple(placed.Position, TipoTerreno.Estrada);
        }
        if (feature.Type == FeatureType.Cidade)
        {
            return board.RedeJaPossuiMeeple(placed.Position, TipoTerreno.Cidade);
        }
        return false;
    }

    /// <summary>
    /// Chamado pelo PlaceMeepleCommand (via Turn) para alocar um Meeple na sub-região escolhida.
    /// </summary>
    public bool ExecuteMeeplePlacementOnBoard(PlacedTile placed, int localFeatureId)
    {
        if (Turn.Instance == null || placed == null) return false;
        Player current = Turn.Instance.GetCurrentPlayer();
        if (current == null || !current.HasAvailableMeeple()) return false;

        TileFeatureDefinition feature = placed.Definition?.GetFeatureById(localFeatureId);
        if (feature == null) return false;

        Meeple meeple = current.GetAvailableMeeple();
        if (meeple == null) return false;

        placed.AssignMeeple(meeple, localFeatureId, feature.GetCorrespondingMeepleRole());

        if (placed.Definition != null)
        {
            placed.Definition.temMeeple = true;
            if (feature.Type == FeatureType.Estrada) placed.Definition.temMeepleEstrada = true;
            if (feature.Type == FeatureType.Cidade) placed.Definition.temMeepleCidade = true;
        }

        board.NotifyMeeplePlaced(placed, meeple);
        CloseMeeplePopup();
        return true;
    }

    public void CloseMeeplePopup()
    {
        if (popupMeeple != null)
        {
            popupMeeple.SetActive(false);
        }
    }

    // Dispara PlaceTileCommand via clique do jogador humano
    private void PlaceTile()
    {
        if (currentTile == null || Turn.Instance == null) return;
        Player current = Turn.Instance.GetCurrentPlayer();
        if (current != null && current.IsBot) return;

        Vector3Int cellPosition = GetCellPos();
        PlaceTileCommand cmd = new PlaceTileCommand(cellPosition, currentRotationZ);

        if (!Turn.Instance.ExecuteCommand(cmd))
        {
            Debug.Log("Movimento inválido: As peças não conectam");
        }
    }

    // Mantido para botões legados do popup Confirma
    public void TryPlaceMeeple()
    {
        if (Turn.Instance == null) return;
        IReadOnlyList<TileFeatureDefinition> available = Turn.Instance.GetAvailableMeepleFeaturesForLastTile();
        if (available != null && available.Count > 0)
        {
            Turn.Instance.ExecuteCommand(new PlaceMeepleCommand(available[0].LocalFeatureId));
        }
        else
        {
            Turn.Instance.ExecuteCommand(new SkipMeepleCommand());
        }
    }

    public void SkipOrFinishMeeplePhase()
    {
        if (Turn.Instance != null)
        {
            Turn.Instance.ExecuteCommand(new SkipMeepleCommand());
        }
    }

    public void OnQ(InputAction.CallbackContext context)
    {
        Player current = Turn.Instance != null ? Turn.Instance.GetCurrentPlayer() : null;
        if (current != null && current.IsBot) return;
        rotateCurrentTile();
    }

    public void OnE(InputAction.CallbackContext context)
    {
        if (currentTile != null && board != null && !board.HasAnyValidPlacement(currentTile))
        {
            PickRandomTile();
        }
    }

    public void OnR(InputAction.CallbackContext context)
    {
        Player current = Turn.Instance != null ? Turn.Instance.GetCurrentPlayer() : null;
        if (current != null && current.IsBot) return;
        rotateCurrentTile();
    }

    public void OnSpace(InputAction.CallbackContext context)
    {
        if (Turn.Instance != null && Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarMeeple)
        {
            Player current = Turn.Instance.GetCurrentPlayer();
            if (current != null && current.IsBot) return;
            Turn.Instance.ExecuteCommand(new SkipMeepleCommand());
        }
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Turn.Instance != null && Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarTile)
        {
            PlaceTile();
        }
    }
}
