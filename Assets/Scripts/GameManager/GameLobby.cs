using System.Collections.Generic;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    [SerializeField] private string lobbyCode;
    [SerializeField] private Player host;
    [SerializeField] private List<Player> connectedPlayers = new List<Player>();
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private bool isPrivate;
    [SerializeField] private GameSession sessionPrefab;

    public string LobbyCode => lobbyCode;
    public Player Host => host;
    public IReadOnlyList<Player> ConnectedPlayers => connectedPlayers;
    public int MaxPlayers => maxPlayers;
    public bool IsPrivate => isPrivate;

    public void AddPlayer(Player player)
    {
        if (player != null && connectedPlayers.Count < maxPlayers && !connectedPlayers.Contains(player))
        {
            connectedPlayers.Add(player);
        }
    }

    public void RemovePlayer(Player player)
    {
        connectedPlayers.Remove(player);
    }

    /// <summary>
    /// Aplica o padrão GRASP Creator respeitando o ciclo de vida de MonoBehaviour do Unity.
    /// </summary>
    public GameSession CreateGameSession()
    {
        if (sessionPrefab != null)
        {
            return Instantiate(sessionPrefab);
        }

        GameSession existingSession = FindFirstObjectByType<GameSession>();
        if (existingSession != null)
        {
            return existingSession;
        }

        GameObject sessionObj = new GameObject("GameSession");
        return sessionObj.AddComponent<GameSession>();
    }

    public void SetPlayerStatus(Player player, bool ready)
    {
        if (player == null) return;
        player.SetConnectionStatus(ready ? ConnectionStatus.Ready : ConnectionStatus.Waiting);
    }

    // Métodos de compatibilidade com código legado
    public void addPlayer(Player player) => AddPlayer(player);
    public void removePlayer(Player player) => RemovePlayer(player);
    public GameSession createGameSession() => CreateGameSession();
    public void setPlayerStatus(Player player, bool ready) => SetPlayerStatus(player, ready);
}
