using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField]private string id;
    [SerializeField]private string name;
    [SerializeField]private Color playerColor;
    [SerializeField]private int score = 0;

    [SerializeField]private List<Meeple> meepleInventory = new List<Meeple>();

    [SerializeField]private bool isAI = false;
    [SerializeField]private int connectionStatus = 0; //0 = disconnected, 1 = connected, 2 = connecting, 3 = waiting, 4 = ready;

    public void Initialize(string playerId, string playerName, Color color)
    {
        this.id = playerId;
        this.name = playerName;
        this.playerColor = color;
        this.score = 0;
        // Inicializa os 7 meeples do jogador
        meepleInventory.Clear();
        for (int i = 0; i < 7; i++)
        {
            GameObject meepleObj = new GameObject($"Meeple_{i + 1}");
            Meeple meeple = meepleObj.AddComponent<Meeple>();
            meeple.owner = this;
            meeple.setType(0); // 0 = available
            meepleInventory.Add(meeple);
        }
    }
    public string GetPlayerName() => name;
    public Color GetPlayerColor() => playerColor;

    public void addScore(int points)
    {
        score += points;
        return;
    }

    public Meeple getAvailableMeeple()
    {
        foreach (Meeple meeple in meepleInventory)
        {
            if (meeple.getType() == 0)
            {
                return meeple;
            }
        }
        return null;
    }

    public void returnMeeple(Meeple meeple)
    {
        meeple.setType(0);
        return;
    }   

    public bool hasAvailableMeeple()
    {
        foreach (Meeple meeple in meepleInventory)
        {
            if (meeple.getType() == 0)
            {
                return true;
            }
        }
        return false;
    }

    //adições do Mendes(ver com o grupo)

    public int getConnectionStatus()
    {
        return connectionStatus;
    }

    public void setConnectionStatus(int status) //0 = disconnected, 1 = connected, 2 = connecting, 3 = waiting, 4 = ready;
    {
        connectionStatus = status;
        return;
    }

}
