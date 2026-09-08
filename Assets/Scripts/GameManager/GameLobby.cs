using UnityEngine;
using System.Collections.Generic;

public class GameLobby : MonoBehaviour
{
    private string lobbyCode;
    private Player host;
    private List<Player> connectedPlayers = new List<Player>();
    private int maxPlayers;
    private bool isPrivate;

    public void addPlayer(Player player)
    {
        connectedPlayers.Add(player);
    }

    public void removePlayer(Player player)
    {
        connectedPlayers.Remove(player);
    }

    public GameSession createGameSession()
    {
        GameSession newSession = new GameSession();
        return newSession;
    }

    //modificado por qfaxas
    public void setPlayerStatus(Player player, bool ready)//0 = disconnected, 1 = connected, 2 = connecting, 3 = waiting, 4 = ready;
    {
        if (ready)
        {
            player.setConnectionStatus(4); // Set player status to ready
        }
        else
        {
            player.setConnectionStatus(3); // Set player status to waiting
        }
    }
}
