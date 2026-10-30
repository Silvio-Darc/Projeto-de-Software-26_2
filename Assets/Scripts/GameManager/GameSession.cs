using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia a sessão da partida local, delegando a criação dos jogadores para PlayerFactory (GoF Factory Method)
/// e inicializando o controlador de turnos (Turn) e o placar (UIPlayerScoreboard).
/// </summary>
public class GameSession : MonoBehaviour
{
    [Header("Prefab & Configurações")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Turn turnManager;

    private List<Player> activePlayers = new List<Player>();

    public IReadOnlyList<Player> ActivePlayers => activePlayers;

    private void Start()
    {
        SetupPlayers(GameConfig.SelectedPlayerCount);
    }

    private void SetupPlayers(int fallbackCount)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i] != null)
            {
                Destroy(activePlayers[i].gameObject);
            }
        }
        activePlayers.Clear();

        // 1. Cria os jogadores (Humanos e Bots) via PlayerFactory
        activePlayers = PlayerFactory.CreatePlayers(transform, playerPrefab, fallbackCount);

        // 2. Garante que o HUD / Placar esteja presente e observando os jogadores
        UIPlayerScoreboard scoreboard = FindFirstObjectByType<UIPlayerScoreboard>();
        if (scoreboard == null)
        {
            GameObject hudObj = new GameObject("PlayerScoreboardHUD");
            scoreboard = hudObj.AddComponent<UIPlayerScoreboard>();
        }
        scoreboard.InitializeScoreboard(activePlayers);

        // 3. Inicia a partida no TurnManager
        if (turnManager == null)
        {
            turnManager = Turn.Instance != null ? Turn.Instance : FindFirstObjectByType<Turn>();
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
