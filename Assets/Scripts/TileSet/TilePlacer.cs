using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class TilePlacer : MonoBehaviour
{
    public Tilemap tilemap;

    public TileBase validTile;

    public Board board;

    // Array que vai guardar todos os tiles diferentes que você criar
    public CarcassoneTile[] tilePool;

    public List<CarcassoneTile> Baralho = new List<CarcassoneTile>();

    // O tile atual que o jogador está segurando para colocar
    public CarcassoneTile currentTile;

    public Image image;

    public enum Direcao { Cima = 0, Direita = 1, Baixo = 2, Esquerda = 3 }

    private int currentRotationZ = 0;

    private Vector3Int lastPlacedPos = Vector3Int.zero;
    private Vector3Int lastPlacedTile = Vector3Int.zero;

    // Flags que indicam se a rede conectada ao tile recém-colocado já possui meeple
    private bool lastTileEstradaOcupada = false;
    private bool lastTileCidadeOcupada = false;

    public InputActionAsset inputs;

    private InputAction Q;
    private InputAction E; 
    private InputAction R;

    private InputAction Space;

    private InputAction leftClick;

    [SerializeField] private GameObject popupMeeple;

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
        Baralho.Add(tilePool[0]);
        Baralho.Add(tilePool[10]);
        Baralho.Add(tilePool[9]);
        Baralho.Add(tilePool[23]);
        Baralho.Add(tilePool[14]);
        for (int i = 0; i < 2; i++)
        {
            Baralho.Add(tilePool[5]);
            Baralho.Add(tilePool[6]);
            Baralho.Add(tilePool[7]);
            Baralho.Add(tilePool[11]);
            Baralho.Add(tilePool[15]);
            Baralho.Add(tilePool[20]);
        }
        for (int i = 0; i < 3; i++)
        {
            Baralho.Add(tilePool[3]);
            Baralho.Add(tilePool[2]);
            Baralho.Add(tilePool[17]);
            Baralho.Add(tilePool[8]);
            Baralho.Add(tilePool[4]);
            Baralho.Add(tilePool[12]);
            Baralho.Add(tilePool[13]);
        }
        for (int i = 0; i < 4; i++)
        {
            Baralho.Add(tilePool[22]);
            Baralho.Add(tilePool[21]);
            Baralho.Add(tilePool[16]);
        }
        for (int i = 0; i < 5; i++)
        {
            Baralho.Add(tilePool[1]);
        }
        for (int i = 0; i < 8; i++)
        {
            Baralho.Add(tilePool[19]);
            Baralho.Add(tilePool[18]);
        }
        Baralho.Add(tilePool[19]);
        // Sorteia o primeiro tile assim que o jogo começa
        PickRandomTile();
    }



    // Método que escolhe um tile aleatório da nossa lista
    void PickRandomTile()
    {
        if (Baralho.Count > 0)
        {
            int randomIndex = Random.Range(0, Baralho.Count);
            currentTile = Baralho[randomIndex];
            Baralho.RemoveAt(randomIndex); // Remove o tile sorteado do baralho para não repetir

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

    Vector3Int GetCellPos()
    {
        
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0;
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
    
        

        lastPlacedPos = cellPosition; // Atualiza a última posição do tile colocado
        return cellPosition;
    }

    CarcassoneTile GetTile()
    {

        Vector3Int cellPosition = GetCellPos();

        if (tilemap.HasTile(cellPosition))
        {
            TileBase tile = tilemap.GetTile(cellPosition);
            

            if (tile != null && tile is CarcassoneTile carcassoneTile)
            {
                Debug.Log($"Inspecionando tile: {tile.name}");
                Debug.Log($"Borda Cima: {carcassoneTile.bordaCima}");
                Debug.Log($"Borda Direita: {carcassoneTile.bordaDireita}");
                Debug.Log($"Borda Baixo: {carcassoneTile.bordaBaixo}");
                Debug.Log($"Borda Esquerda: {carcassoneTile.bordaEsquerda}");
                Debug.Log($"Tem Escudo: {carcassoneTile.temEscudo}");
                Debug.Log($"Tem Mosteiro: {carcassoneTile.temMosteiro}");
                Debug.Log($"Tem Meeple: {carcassoneTile.temMeeple}");
                return carcassoneTile;
            }
        }

        return null;
    }

    void PlaceTile()
    {
        if (currentTile == null) return; // Proteção caso a lista de tiles esteja vazia

        Vector3Int cellPosition = GetCellPos();

        if (board.IsValidPlacement(currentTile, cellPosition, currentRotationZ))
        {
            CarcassoneTile tileInstance = Instantiate(currentTile);
            tileInstance.temMeeple = false;
            tileInstance.temMeepleEstrada = false;
            tileInstance.temMeepleCidade = false;

            // 1. PRIMEIRO: Coloca a peça no tabuleiro e registra na lista
            tilemap.SetTile(cellPosition, tileInstance);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, currentRotationZ*-90), Vector3.one);
            tilemap.SetTransformMatrix(cellPosition, matrix);
            board.addCellToPlacedCells(cellPosition, currentRotationZ);

            // 2. DEPOIS: Verifica se conectou a redes que já possuem meeple
            //    (agora a peça já existe no tabuleiro, então a busca BFS funciona!)
            lastTileEstradaOcupada = board.RedeJaPossuiMeeple(cellPosition, TipoTerreno.Estrada);
            lastTileCidadeOcupada = board.RedeJaPossuiMeeple(cellPosition, TipoTerreno.Cidade);

            if (lastTileEstradaOcupada)
            {
                tileInstance.temMeepleEstrada = true;
                Debug.Log("Conectado a uma estrada que já possui Meeple!");
            }
            if (lastTileCidadeOcupada)
            {
                tileInstance.temMeepleCidade = true;
                Debug.Log("Conectado a uma cidade que já possui Meeple!");
            }
            
            currentRotationZ = 0;
            image.transform.rotation = Quaternion.Euler(0, 0, 0);
            PickRandomTile();

            lastPlacedPos = cellPosition;
            lastPlacedTile = cellPosition;

            // Abre o popup para o jogador escolher se quer colocar meeple
            if (popupMeeple != null)
            {
                popupMeeple.SetActive(true);
            }
            FinishAction();
        }
        
        else
        {
            Debug.Log("Movimento inválido: As peças não conectam");
        }
    }

    void InspectTile()
    {
        CarcassoneTile carcassoneTile = GetTile();
        if (carcassoneTile == null)
        {
            Debug.Log("Nenhum tile encontrado na posição do mouse.");
            return;
        }

        if(!carcassoneTile.temMeeple)
        {
            popupMeeple.SetActive(true);
        }
        Debug.Log("Tile inspecionado: " + carcassoneTile.name);
    }
    
    public void TryPlaceMeeple()
    {
        // 1. Verifica se o TurnManager e o Jogador existem
        if (Turn.Instance == null)
        {
            Debug.LogError("[Falha Meeple] Turn.Instance é nulo! O script Turn está na cena?");
            return;
        }
        Player current = Turn.Instance.GetCurrentPlayer();
        if (current == null)
        {
            Debug.LogError("[Falha Meeple] Nenhum jogador atual encontrado! A lista de jogadores no Turn está vazia.");
            return;
        }
        // 2. Verifica se o jogador realmente tem meeples no inventário
        if (!current.hasAvailableMeeple())
        {
            Debug.LogWarning($"[Falha Meeple] {current.GetPlayerName()} realmente não possui meeples disponíveis!");
            return;
        }
        // 3. Verifica se a célula do tile colocado é válida
        TileBase tileBase = tilemap.GetTile(lastPlacedTile);
        if (tileBase == null)
        {
            Debug.LogError($"[Falha Meeple] Nenhum tile encontrado na posição gravada: {lastPlacedTile}");
            return;
        }
        if (!(tileBase is CarcassoneTile carcassoneTile))
        {
            Debug.LogError($"[Falha Meeple] O tile em {lastPlacedTile} não é um CarcassoneTile!");
            return;
        }
        // 4. Verifica se o tile já tinha meeple
        if (carcassoneTile.temMeeple)
        {
            Debug.LogWarning($"[Falha Meeple] O tile em {lastPlacedTile} ({carcassoneTile.name}) já possui meeple!");
            return;
        }

        // 5. REGRA DO CARCASSONNE: Verifica se a rede conectada já possui meeple
        //    Usa as flags calculadas em PlaceTile() E refaz a checagem em tempo real
        bool estradaOcupada = lastTileEstradaOcupada || board.RedeJaPossuiMeeple(lastPlacedTile, TipoTerreno.Estrada);
        bool cidadeOcupada = lastTileCidadeOcupada || board.RedeJaPossuiMeeple(lastPlacedTile, TipoTerreno.Cidade);

        // Verifica se TODAS as opções de terreno deste tile estão bloqueadas
        bool temEstrada = carcassoneTile.bordaCima == TipoTerreno.Estrada ||
                          carcassoneTile.bordaDireita == TipoTerreno.Estrada ||
                          carcassoneTile.bordaBaixo == TipoTerreno.Estrada ||
                          carcassoneTile.bordaEsquerda == TipoTerreno.Estrada;

        bool temCidade = carcassoneTile.bordaCima == TipoTerreno.Cidade ||
                         carcassoneTile.bordaDireita == TipoTerreno.Cidade ||
                         carcassoneTile.bordaBaixo == TipoTerreno.Cidade ||
                         carcassoneTile.bordaEsquerda == TipoTerreno.Cidade;

        // Se a peça tem estrada e a estrada está ocupada, E a peça tem cidade e a cidade está ocupada
        // OU a peça só tem estrada e ela está ocupada, OU a peça só tem cidade e ela está ocupada
        bool bloqueado = true;
        if (temEstrada && !estradaOcupada) bloqueado = false;
        if (temCidade && !cidadeOcupada) bloqueado = false;
        if (!temEstrada && !temCidade) bloqueado = false; // Tile só de campo

        if (bloqueado)
        {
            Debug.LogWarning("[Regra Carcassonne] Todos os terrenos deste tile já possuem Meeple na rede! Não é permitido colocar outro.");
            if (popupMeeple != null) popupMeeple.SetActive(false);
            SkipOrFinishMeeplePhase();
            return;
        }


        Debug.Log($"Sucesso: Meeple posicionado pelo {current.GetPlayerName()}!");
        
        carcassoneTile.temMeeple = true;
        carcassoneTile.temMeepleEstrada = true;
        carcassoneTile.temMeepleCidade = true;

        Meeple meeple = current.getAvailableMeeple();
        if (meeple != null)
        {
            meeple.setType(1); // Marca o meeple como ocupado
        }
        // Pinta o tile com a cor do jogador
        tilemap.SetTileFlags(lastPlacedTile, TileFlags.None);
        tilemap.SetColor(lastPlacedTile, Color.Lerp(current.GetPlayerColor(), Color.white, 0.5f));
        if (popupMeeple != null) popupMeeple.SetActive(false);
        // Finaliza o turno e vai para o próximo jogador
        SkipOrFinishMeeplePhase();
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

    public void FinishAction()
    {
        // 1. Registra pontos ou ações do jogador atual se houver
        Player current = Turn.Instance.GetCurrentPlayer();
        // 2. Passa a vez para o prox turno
        if (Turn.Instance.CurrentPhase == Turn.TurnPhase.PosicionarTile)
        {
            Turn.Instance.SetPhase(Turn.TurnPhase.PosicionarMeeple);
            //isMeepling = true;
        }
        else
        {
            //isMeepling = false;
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
    }
    
    
}
