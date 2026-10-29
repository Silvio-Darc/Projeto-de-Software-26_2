using System.Collections.Generic;
using UnityEngine;
public class ConfigurarPartidaLocalPanel : UIPanel
{
    [Header("Configuração de Jogadores")]
    [SerializeField] private Transform playersGroup;
    [SerializeField] private UIPlayer playerPrefab;
    [SerializeField] private UIPlayer botPrefab;
    [SerializeField] private GameObject addButtonsContainer;
    private const int MaxPlayers = 4;
    private const int MinPlayers = 1;
    public List<UIPlayer> activePlayers = new List<UIPlayer>();
    public static ConfigurarPartidaLocalPanel Instance { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }
    public void AddPlayer()
    {
        if (activePlayers.Count < MaxPlayers)
        {
            UIPlayer newPlayer = Instantiate(playerPrefab, playersGroup, false);
            activePlayers.Add(newPlayer);
            UpdateAddButtonsState();
        }
    }
    public void AddBot()
    {
        if (activePlayers.Count < MaxPlayers)
        {
            UIPlayer newBot = Instantiate(botPrefab, playersGroup, false);
            activePlayers.Add(newBot);
            UpdateAddButtonsState();
        }
    }

    public void RemovePlayer(UIPlayer playerToRemove)
    {
        if (activePlayers.Count > MinPlayers && activePlayers.Contains(playerToRemove))
        {
            activePlayers.Remove(playerToRemove);
            Destroy(playerToRemove.gameObject);
            UpdateAddButtonsState();
        }
    }
    private void UpdateAddButtonsState()
    {
        if (addButtonsContainer != null)
        {
            addButtonsContainer.transform.SetAsLastSibling();
            addButtonsContainer.SetActive(activePlayers.Count < MaxPlayers);
        }
    }

    public void Jogar()
    {
        foreach(UIPlayer player in activePlayers)
        {
            Debug.Log(player.playername);
        }
    }
}