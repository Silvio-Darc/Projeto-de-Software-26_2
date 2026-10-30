using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dados de configuração de um participante (Humano ou Bot) definidos no menu de partida local.
/// </summary>
public struct PlayerSetupData
{
    public string Name;
    public Color Color;
    public bool IsBot;
    public BotDifficulty Difficulty;

    public PlayerSetupData(string name, Color color, bool isBot, BotDifficulty difficulty = BotDifficulty.Medio)
    {
        Name = name;
        Color = color;
        IsBot = isBot;
        Difficulty = difficulty;
    }
}

/// <summary>
/// Objeto de transferência de configuração entre a cena de Menu (Menu.unity) e a cena de Jogo (GRASP Pure Fabrication).
/// </summary>
public static class MatchConfig
{
    public const string MenuSceneName = "Menu";
    public const string MainGameSceneName = "teste_faxas";

    private static readonly List<PlayerSetupData> configuredPlayers = new List<PlayerSetupData>();

    public static IReadOnlyList<PlayerSetupData> ConfiguredPlayers => configuredPlayers;
    public static bool HasCustomConfiguration => configuredPlayers.Count >= 2;

    public static void SetPlayers(IEnumerable<PlayerSetupData> players)
    {
        configuredPlayers.Clear();
        if (players != null)
        {
            configuredPlayers.AddRange(players);
        }
    }

    public static void SetPlayersFromUI(IReadOnlyList<UIPlayer> uiPlayers)
    {
        configuredPlayers.Clear();
        if (uiPlayers == null) return;

        int humanIndex = 1;
        int botIndex = 1;

        for (int i = 0; i < uiPlayers.Count; i++)
        {
            UIPlayer ui = uiPlayers[i];
            if (ui == null) continue;

            ui.updateName();
            string defaultName = ui.IsBot ? $"Bot {botIndex++}" : $"Jogador {humanIndex++}";
            string resolvedName = !string.IsNullOrWhiteSpace(ui.playername) ? ui.playername.Trim() : defaultName;

            configuredPlayers.Add(new PlayerSetupData(
                resolvedName,
                ui.PlayerColor,
                ui.IsBot,
                BotDifficulty.Medio));
        }
    }
}
