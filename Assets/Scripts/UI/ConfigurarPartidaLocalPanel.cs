using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Painel de configuração de partida local (Humanos e Bots).
/// Encapsula o gerenciamento de cores disponíveis e a lista de participantes.
/// </summary>
public class ConfigurarPartidaLocalPanel : UIPanel
{
    [Header("Configuração de Jogadores")]
    [SerializeField] private Transform playersGroup;
    [SerializeField] private UIPlayer playerPrefab;
    [SerializeField] private UIPlayer botPrefab;
    [SerializeField] private GameObject addButtonsContainer;

    private const int MaxPlayers = 4;
    private const int MinPlayers = 2;

    public List<UIPlayer> activePlayers = new List<UIPlayer>();
    public static ConfigurarPartidaLocalPanel Instance { get; private set; }
    public List<Color> avaibleColors = new List<Color>();

    public IReadOnlyList<UIPlayer> ActivePlayers => activePlayers;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    /// <summary>
    /// Corrige o bug de off-by-one em Random.Range e encapsula a retirada de cor disponível.
    /// </summary>
    public bool TryRequestColor(out Color color)
    {
        if (avaibleColors != null && avaibleColors.Count > 0)
        {
            int colorIndex = Random.Range(0, avaibleColors.Count);
            color = avaibleColors[colorIndex];
            avaibleColors.RemoveAt(colorIndex);
            return true;
        }

        color = Color.white;
        return false;
    }

    public void ReleaseColor(Color color)
    {
        if (avaibleColors != null && !avaibleColors.Contains(color))
        {
            avaibleColors.Add(color);
        }
    }

    public void AddPlayer()
    {
        if (activePlayers.Count < MaxPlayers && playerPrefab != null)
        {
            UIPlayer newPlayer = Instantiate(playerPrefab, playersGroup, false);
            newPlayer.isBot = false;
            activePlayers.Add(newPlayer);
            UpdateAddButtonsState();
        }
    }

    public void AddBot()
    {
        if (activePlayers.Count < MaxPlayers && botPrefab != null)
        {
            UIPlayer newBot = Instantiate(botPrefab, playersGroup, false);
            newBot.isBot = true;
            activePlayers.Add(newBot);
            UpdateAddButtonsState();
        }
    }

    public void RemovePlayer(UIPlayer playerToRemove)
    {
        if (playerToRemove != null && activePlayers.Count > MinPlayers && activePlayers.Contains(playerToRemove))
        {
            ReleaseColor(playerToRemove.playerColor);
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
        // Garante no mínimo 2 participantes na partida local
        while (activePlayers.Count < MinPlayers)
        {
            if (botPrefab != null) AddBot();
            else if (playerPrefab != null) AddPlayer();
            else break;
        }

        MatchConfig.SetPlayersFromUI(activePlayers);
        SceneManager.LoadScene(MatchConfig.MainGameSceneName);
    }
}
