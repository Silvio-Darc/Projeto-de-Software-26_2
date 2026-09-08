using UnityEngine;


public class Meeple : MonoBehaviour
{
    public Player owner;
    private string id;
    private int type = 0; //0 = available, 1 = farm, 2 = road, 3 = city, 4 = monastery
    private int value = 0;
    
    public int getValue()
    {
        return value;
    }

    public Player getOwner()
    {
        return owner;
    }

    public int getType() //0 = available, 1 = farm, 2 = road, 3 = city, 4 = monastery
    {
        return type;
    }

    public void setType(int newType) //0 = available, 1 = farm, 2 = road, 3 = city, 4 = monastery
    {
        type = newType;
    }
}
