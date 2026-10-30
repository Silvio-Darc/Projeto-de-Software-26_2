using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa um jogador (Humano ou Bot) na partida.
/// Aplica Information Expert (GRASP) para inventário de meeples e pontuação,
/// e Observer (GoF) para notificar a interface gráfica sobre alterações de estado.
/// </summary>
public class Player : MonoBehaviour
{
    public const int InitialMeepleCount = 7;

    [SerializeField] private string id;
    [SerializeField] private new string name;
    [SerializeField] private Color playerColor;
    [SerializeField] private int score = 0;
    [SerializeField] private List<Meeple> meepleInventory = new List<Meeple>();
    [SerializeField] private bool isAI = false;
    [SerializeField] private BotDifficulty botDifficulty = BotDifficulty.Medio;
    [SerializeField] private int connectionStatus = (int)ConnectionStatus.Disconnected;

    // Eventos Observer para atualização reativa da UI
    public event Action<Player, int> OnScoreChanged;
    public event Action<Player, int> OnMeepleCountChanged;

    public string Id => id;
    public string PlayerName => name;
    public Color PlayerColor => playerColor;
    public int Score => score;
    public bool IsBot => isAI;
    public BotDifficulty Difficulty => botDifficulty;
    public ConnectionStatus Status => (ConnectionStatus)connectionStatus;
    public IReadOnlyList<Meeple> Meeples => meepleInventory;

    public int AvailableMeepleCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < meepleInventory.Count; i++)
            {
                if (meepleInventory[i] != null && meepleInventory[i].IsAvailable)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public void Initialize(
        string playerId,
        string playerName,
        Color color,
        bool isBotPlayer = false,
        BotDifficulty difficulty = BotDifficulty.Medio)
    {
        id = playerId;
        name = playerName;
        playerColor = color;
        score = 0;
        isAI = isBotPlayer;
        botDifficulty = difficulty;

        // Limpa meeples antigos caso re-inicializado
        for (int i = 0; i < meepleInventory.Count; i++)
        {
            if (meepleInventory[i] != null)
            {
                Destroy(meepleInventory[i].gameObject);
            }
        }
        meepleInventory.Clear();

        // Inicializa os 7 meeples do jogador como filhos na hierarquia
        for (int i = 0; i < InitialMeepleCount; i++)
        {
            GameObject meepleObj = new GameObject($"Meeple_{playerId}_{i + 1}");
            meepleObj.transform.SetParent(transform, false);
            Meeple meeple = meepleObj.AddComponent<Meeple>();
            meeple.Initialize(this, $"{playerId}_m{i + 1}");
            meepleInventory.Add(meeple);
        }

        OnScoreChanged?.Invoke(this, score);
        OnMeepleCountChanged?.Invoke(this, AvailableMeepleCount);
    }

    public void AddScore(int points)
    {
        if (points == 0) return;
        score += points;
        OnScoreChanged?.Invoke(this, score);
    }

    public Meeple GetAvailableMeeple()
    {
        foreach (Meeple meeple in meepleInventory)
        {
            if (meeple != null && meeple.IsAvailable)
            {
                return meeple;
            }
        }
        return null;
    }

    public void NotifyMeeplePlaced()
    {
        OnMeepleCountChanged?.Invoke(this, AvailableMeepleCount);
    }

    public void ReturnMeeple(Meeple meeple)
    {
        if (meeple == null) return;
        meeple.Release();
        OnMeepleCountChanged?.Invoke(this, AvailableMeepleCount);
    }

    public bool HasAvailableMeeple()
    {
        return GetAvailableMeeple() != null;
    }

    public void SetConnectionStatus(ConnectionStatus status)
    {
        connectionStatus = (int)status;
    }

    // Métodos de compatibilidade com chamadas existentes
    public string GetPlayerName() => PlayerName;
    public Color GetPlayerColor() => PlayerColor;
    public int GetPlayerScore() => Score;
    public int GetPlayerMeeples() => AvailableMeepleCount;
    public void addScore(int points) => AddScore(points);
    public Meeple getAvailableMeeple() => GetAvailableMeeple();
    public void returnMeeple(Meeple meeple) => ReturnMeeple(meeple);
    public bool hasAvailableMeeple() => HasAvailableMeeple();
    public int getConnectionStatus() => connectionStatus;
    public void setConnectionStatus(int status) => connectionStatus = status;
}
