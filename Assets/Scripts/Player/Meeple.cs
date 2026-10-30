using UnityEngine;

/// <summary>
/// Representa um seguidor (Meeple) pertencente a um jogador.
/// Aplica encapsulamento e tipagem forte (MeepleRole).
/// </summary>
public class Meeple : MonoBehaviour
{
    [SerializeField] private Player owner;
    [SerializeField] private string id;
    [SerializeField] private int type = (int)MeepleRole.Disponivel;
    [SerializeField] private int value = 1;
    [SerializeField] private Vector3Int placedBoardPosition;
    [SerializeField] private int placedFeatureId = -1;

    public Player Owner
    {
        get => owner;
        set => owner = value;
    }

    public string Id
    {
        get => id;
        set => id = value;
    }

    public MeepleRole Role
    {
        get => (MeepleRole)type;
        set => type = (int)value;
    }

    public int Value => value;
    public bool IsAvailable => Role == MeepleRole.Disponivel;
    public Vector3Int PlacedBoardPosition => placedBoardPosition;
    public int PlacedFeatureId => placedFeatureId;

    public void Initialize(Player meepleOwner, string meepleId)
    {
        owner = meepleOwner;
        id = meepleId;
        Role = MeepleRole.Disponivel;
        placedBoardPosition = Vector3Int.zero;
        placedFeatureId = -1;
    }

    public void AssignToFeature(Vector3Int boardPosition, int featureId, MeepleRole role)
    {
        placedBoardPosition = boardPosition;
        placedFeatureId = featureId;
        Role = role;
    }

    public void Release()
    {
        Role = MeepleRole.Disponivel;
        placedBoardPosition = Vector3Int.zero;
        placedFeatureId = -1;
    }

    // Métodos de compatibilidade com código legado
    public int getValue() => Value;
    public Player getOwner() => Owner;
    public int getType() => type;
    public void setType(int newType) => type = newType;
}
