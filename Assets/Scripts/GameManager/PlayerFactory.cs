using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fábrica de criação de jogadores para a sessão de jogo (Padrão GoF Factory Method & GRASP Creator).
/// Instancia jogadores humanos e Bots a partir do MatchConfig ou da configuração padrão.
/// </summary>
public static class PlayerFactory
{
    private static readonly Color[] DefaultColors = new Color[]
    {
        new Color(0.90f, 0.22f, 0.21f), // Vermelho
        new Color(0.12f, 0.53f, 0.90f), // Azul
        new Color(0.26f, 0.63f, 0.28f), // Verde
        new Color(0.98f, 0.75f, 0.18f)  // Amarelo
    };

    public static List<Player> CreatePlayers(Transform parent, GameObject playerPrefab, int fallbackCount = 2)
    {
        List<Player> createdPlayers = new List<Player>();

        if (MatchConfig.HasCustomConfiguration)
        {
            IReadOnlyList<PlayerSetupData> setups = MatchConfig.ConfiguredPlayers;
            for (int i = 0; i < setups.Count; i++)
            {
                PlayerSetupData data = setups[i];
                Player p = InstantiatePlayerObject(parent, playerPrefab, i);
                p.Initialize(
                    playerId: $"p{i + 1}",
                    playerName: data.Name,
                    color: data.Color,
                    isBotPlayer: data.IsBot,
                    difficulty: data.Difficulty);
                createdPlayers.Add(p);
            }
            return createdPlayers;
        }

        // Fallback quando a cena de jogo é iniciada diretamente pelo Editor
        int count = Mathf.Clamp(fallbackCount, 2, 4);
        for (int i = 0; i < count; i++)
        {
            bool isBot = (i > 0); // No modo de teste direto: Jogador 1 Humano vs Bot
            string pName = isBot ? $"Bot {i}" : $"Jogador {i + 1}";
            Player p = InstantiatePlayerObject(parent, playerPrefab, i);
            p.Initialize(
                playerId: $"p{i + 1}",
                playerName: pName,
                color: DefaultColors[i % DefaultColors.Length],
                isBotPlayer: isBot,
                difficulty: BotDifficulty.Medio);
            createdPlayers.Add(p);
        }

        return createdPlayers;
    }

    private static Player InstantiatePlayerObject(Transform parent, GameObject playerPrefab, int index)
    {
        GameObject playerObj;
        if (playerPrefab != null)
        {
            playerObj = Object.Instantiate(playerPrefab, parent);
        }
        else
        {
            playerObj = new GameObject();
            if (parent != null) playerObj.transform.SetParent(parent, false);
        }

        playerObj.name = $"Player_{index + 1}";
        Player player = playerObj.GetComponent<Player>();
        if (player == null)
        {
            player = playerObj.AddComponent<Player>();
        }
        return player;
    }
}
