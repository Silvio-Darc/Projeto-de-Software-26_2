using UnityEngine;
using System.Collections.Generic;

public class GameSession: MonoBehaviour
{
    [Header("Prefab & Configurações")]
    [SerializeField] private GameObject playerPrefab; // Prefab do Player com Player.cs anexado
    [SerializeField] private Turn turnManager;
    // Cores padrão para diferenciar os 4 jogadores (Carcassonne)
    private readonly Color[] defaultColors = new Color[]
    {
        Color.red,      // Jogador 1: Vermelho
        Color.blue,     // Jogador 2: Azul
        Color.green,    // Jogador 3: Verde
        Color.yellow    // Jogador 4: Amarelo
    };
    private List<Player> activePlayers = new List<Player>();
    private void Start()
    {
        SetupPlayers(GameConfig.SelectedPlayerCount);
    }
    private void SetupPlayers(int count)
    {
        activePlayers.Clear();
        for (int i = 0; i < count; i++)
        {
            // 1. Instancia o GameObject do jogador
            
            GameObject playerObj = Instantiate(playerPrefab, gameObject.transform);
            playerObj.name = $"Player_{i + 1}";
            // 2. Configura os dados do Player
            Player player = playerObj.GetComponent<Player>();
            player.Initialize(
                playerId: $"p{i + 1}",
                playerName: $"Jogador {i + 1}",
                color: defaultColors[i]
            );
            activePlayers.Add(player);
        }
        // 3. Entrega os jogadores instanciados ao Turn Manager
        if (turnManager == null)
        {
            turnManager = Turn.Instance;
        }
        if (turnManager != null)
        {
            turnManager.StartGame(activePlayers);
        }
        else
        {
            Debug.LogError("TurnManager não encontrado na cena!");
        }
    }
}
