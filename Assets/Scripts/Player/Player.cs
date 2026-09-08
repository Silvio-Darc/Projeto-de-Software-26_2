using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private string id;
    private string name;
    private Color playerColor;
    private int score = 0;

    private List<Meeple> meepleInventory = new List<Meeple>();

    private bool isAI = false;
    private int connectionStatus = 0; //0 = disconnected, 1 = connected, 2 = connecting;

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
}
