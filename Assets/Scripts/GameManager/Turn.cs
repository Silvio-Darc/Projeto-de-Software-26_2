using UnityEngine;
using System.Collections.Generic;
using System;

public class Turn : MonoBehaviour
{
    public static Turn Instance { get; private set; }
    [Header("Jogadores")]
    [SerializeField] private List<Player> players = new List<Player>();
    private int currentPlayerIndex = 0;
    private int roundNumber = 1;

    //fases
    public enum TurnPhase { PosicionarTile, PosicionarMeeple, FimTurno }
    public TurnPhase CurrentPhase { get; private set; }

    // Eventos para UI e outros scripts reagirem
    public event Action<Player> OnTurnChanged;
    public event Action<TurnPhase> OnPhaseChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Inicializa a partida com os 4 jogadores (vindo do GameLobby / GameSession)
    public void StartGame(List<Player> startingPlayers)
    {
        players = startingPlayers;
        currentPlayerIndex = 0;
        roundNumber = 1;
        StartTurn();
    }

    /// Retorna o jogador do turno atual
    public Player GetCurrentPlayer()
    {
        if (players.Count == 0) return null;
        return players[currentPlayerIndex];
    }

    /// Inicia o turno do jogador atual
    private void StartTurn()
    {
        CurrentPhase = TurnPhase.PosicionarTile; // Começa com a fase de comprar tile
        Player current = GetCurrentPlayer();
        Debug.Log($"[Rodada {roundNumber}] Turno de: Jogador {currentPlayerIndex + 1}");
        // Dispara evento avisando a UI e outros sistemas
        OnTurnChanged?.Invoke(current);
        SetPhase(TurnPhase.PosicionarTile);
    }

    /// Altera a fase do turno atual (ex: comprou tile -> posicionar tile)
    public void SetPhase(TurnPhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    /// Avança para o próximo jogador da lista (0 -> 1 -> 2 -> 3 -> 0)
    public void EndTurn()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[Turn] Nenhum jogador cadastrado na lista para passar o turno!");
            return;
        }
        // Avança o índice de forma circular entre os jogadores disponíveis
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        // Se voltou para o primeiro jogador, completou uma rodada inteira
        if (currentPlayerIndex == 0)
        {
            roundNumber++;
        }

        Debug.Log($"Turno passado com sucesso! Vez do: {GetCurrentPlayer()?.GetPlayerName()} (Índice {currentPlayerIndex})");

        StartTurn();
    }
}
