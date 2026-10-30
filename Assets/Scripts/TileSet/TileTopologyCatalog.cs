using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catálogo oficial da topologia interna e distribuição das 24 peças do Carcassonne tradicional (72 peças no total).
/// Garante que todas as sub-regiões (Cidades, Estradas, Mosteiros e Campos divididos) estejam mapeadas com precisão.
/// </summary>
public static class TileTopologyCatalog
{
    public static void ConfigureTileTopology(CarcassoneTile tile)
    {
        if (tile == null) return;

        string key = !string.IsNullOrWhiteSpace(tile.name) ? tile.name.Trim() : tile.nomeDaPeca?.Trim();
        List<TileFeatureDefinition> features = new List<TileFeatureDefinition>();
        int quantity = 1;
        bool isStart = false;

        switch (key)
        {
            case "Cidade":
                quantity = 1;
                features.Add(new TileFeatureDefinition(0, "Cidade Central", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Vector2(0f, 0f)));
                break;

            case "Cidade_Cima":
                quantity = 5;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.33f)));
                features.Add(new TileFeatureDefinition(1, "Campo Sul", FeatureType.Campo,
                    new[] { 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Vector2(0f, -0.15f), new[] { 0 }));
                break;

            case "Cidade_Cima_Baixo":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.35f)));
                features.Add(new TileFeatureDefinition(1, "Cidade Sul", FeatureType.Cidade,
                    new[] { 6, 7, 8 }, new Vector2(0f, -0.35f)));
                features.Add(new TileFeatureDefinition(2, "Campo Central", FeatureType.Campo,
                    new[] { 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0f), new[] { 0, 1 }));
                break;

            case "Cidade_Cima_Esq":
                quantity = 2;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.35f)));
                features.Add(new TileFeatureDefinition(1, "Cidade Oeste", FeatureType.Cidade,
                    new[] { 9, 10, 11 }, new Vector2(-0.35f, 0f)));
                features.Add(new TileFeatureDefinition(2, "Campo Sudeste", FeatureType.Campo,
                    new[] { 3, 4, 5, 6, 7, 8 }, new Vector2(0.16f, -0.16f), new[] { 0, 1 }));
                break;

            case "Cidade_Cima_Dir":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Nordeste", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5 }, new Vector2(0.22f, 0.22f)));
                features.Add(new TileFeatureDefinition(1, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 6, 7, 8, 9, 10, 11 }, new Vector2(-0.22f, -0.22f), new[] { 0 }));
                break;

            case "Cidade_Cima_Dir_Estrada_Baixo_Esq_Escudo":
                quantity = 2;
                features.Add(new TileFeatureDefinition(0, "Cidade Nordeste", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5 }, new Vector2(0.22f, 0.22f)));
                features.Add(new TileFeatureDefinition(1, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 6, 7, 8, 9, 10, 11 }, new Vector2(-0.22f, -0.22f), new[] { 0 }));
                break;

            case "Cidade_Cima_Dir_Estrada_Baixo_Esq":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Nordeste", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5 }, new Vector2(0.24f, 0.24f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Sudoeste", FeatureType.Estrada,
                    new[] { 7, 10 }, new Vector2(-0.18f, -0.18f)));
                features.Add(new TileFeatureDefinition(2, "Campo Interno", FeatureType.Campo,
                    new[] { 6, 11 }, new Vector2(-0.02f, -0.02f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Canto Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.34f, -0.34f)));
                break;

            case "Cidade_Cima_Dir_Estrada_Baixo_Esq_Estrada_Escudo":
                quantity = 2;
                features.Add(new TileFeatureDefinition(0, "Cidade Nordeste", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5 }, new Vector2(0.24f, 0.24f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Sudoeste", FeatureType.Estrada,
                    new[] { 7, 10 }, new Vector2(-0.18f, -0.18f)));
                features.Add(new TileFeatureDefinition(2, "Campo Interno", FeatureType.Campo,
                    new[] { 6, 11 }, new Vector2(-0.02f, -0.02f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Canto Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.34f, -0.34f)));
                break;

            case "Cidade_Cima_Esq_Dir":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Grande", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0.16f)));
                features.Add(new TileFeatureDefinition(1, "Campo Sul", FeatureType.Campo,
                    new[] { 6, 7, 8 }, new Vector2(0f, -0.35f), new[] { 0 }));
                break;

            case "Cidade_Cima_Esq_Dir_Escudo":
                quantity = 1;
                features.Add(new TileFeatureDefinition(0, "Cidade Grande", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0.16f)));
                features.Add(new TileFeatureDefinition(1, "Campo Sul", FeatureType.Campo,
                    new[] { 6, 7, 8 }, new Vector2(0f, -0.35f), new[] { 0 }));
                break;

            case "Cidade_Cima_Esq_Dir_Estrada":
                quantity = 1;
                features.Add(new TileFeatureDefinition(0, "Cidade Grande", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0.18f)));
                features.Add(new TileFeatureDefinition(1, "Estrada do Portão", FeatureType.Estrada,
                    new[] { 7 }, new Vector2(0f, -0.32f)));
                features.Add(new TileFeatureDefinition(2, "Campo Sudeste", FeatureType.Campo,
                    new[] { 6 }, new Vector2(0.25f, -0.35f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 8 }, new Vector2(-0.25f, -0.35f), new[] { 0 }));
                break;

            case "Cidade_Cima_Esq_Dir_Estrada_Escudo":
                quantity = 2;
                features.Add(new TileFeatureDefinition(0, "Cidade Grande", FeatureType.Cidade,
                    new[] { 0, 1, 2, 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0.18f)));
                features.Add(new TileFeatureDefinition(1, "Estrada do Portão", FeatureType.Estrada,
                    new[] { 7 }, new Vector2(0f, -0.32f)));
                features.Add(new TileFeatureDefinition(2, "Campo Sudeste", FeatureType.Campo,
                    new[] { 6 }, new Vector2(0.25f, -0.35f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 8 }, new Vector2(-0.25f, -0.35f), new[] { 0 }));
                break;

            case "Cidade_Cima_Estrada_Dir":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.34f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Curva Direita", FeatureType.Estrada,
                    new[] { 4, 7 }, new Vector2(0.15f, -0.15f)));
                features.Add(new TileFeatureDefinition(2, "Campo Oeste", FeatureType.Campo,
                    new[] { 3, 8, 9, 10, 11 }, new Vector2(-0.24f, -0.05f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Canto Sudeste", FeatureType.Campo,
                    new[] { 5, 6 }, new Vector2(0.32f, -0.32f)));
                break;

            case "Cidade_Cima_Estrada_Esq":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.34f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Curva Esquerda", FeatureType.Estrada,
                    new[] { 7, 10 }, new Vector2(-0.15f, -0.15f)));
                features.Add(new TileFeatureDefinition(2, "Campo Leste", FeatureType.Campo,
                    new[] { 3, 4, 5, 6, 11 }, new Vector2(0.24f, -0.05f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Canto Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.32f, -0.32f)));
                break;

            case "Cidade_Esq_Dir":
                quantity = 1;
                features.Add(new TileFeatureDefinition(0, "Cidade Ponte", FeatureType.Cidade,
                    new[] { 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0f)));
                features.Add(new TileFeatureDefinition(1, "Campo Norte", FeatureType.Campo,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.36f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(2, "Campo Sul", FeatureType.Campo,
                    new[] { 6, 7, 8 }, new Vector2(0f, -0.36f), new[] { 0 }));
                break;

            case "Cidade_Esq_Dir_Escudo":
                quantity = 2;
                features.Add(new TileFeatureDefinition(0, "Cidade Ponte", FeatureType.Cidade,
                    new[] { 3, 4, 5, 9, 10, 11 }, new Vector2(0f, 0f)));
                features.Add(new TileFeatureDefinition(1, "Campo Norte", FeatureType.Campo,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.36f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(2, "Campo Sul", FeatureType.Campo,
                    new[] { 6, 7, 8 }, new Vector2(0f, -0.36f), new[] { 0 }));
                break;

            case "Cidade_Estrada_Reto":
                quantity = 4;
                isStart = true;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.34f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Reta", FeatureType.Estrada,
                    new[] { 4, 10 }, new Vector2(0f, -0.02f)));
                features.Add(new TileFeatureDefinition(2, "Campo Norte da Estrada", FeatureType.Campo,
                    new[] { 3, 11 }, new Vector2(-0.28f, 0.12f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(3, "Campo Sul da Estrada", FeatureType.Campo,
                    new[] { 5, 6, 7, 8, 9 }, new Vector2(0f, -0.30f)));
                break;

            case "Cidade_Vila_T":
                quantity = 3;
                features.Add(new TileFeatureDefinition(0, "Cidade Norte", FeatureType.Cidade,
                    new[] { 0, 1, 2 }, new Vector2(0f, 0.34f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Leste", FeatureType.Estrada,
                    new[] { 4 }, new Vector2(0.34f, -0.02f)));
                features.Add(new TileFeatureDefinition(2, "Estrada Sul", FeatureType.Estrada,
                    new[] { 7 }, new Vector2(0f, -0.35f)));
                features.Add(new TileFeatureDefinition(3, "Estrada Oeste", FeatureType.Estrada,
                    new[] { 10 }, new Vector2(-0.34f, -0.02f)));
                features.Add(new TileFeatureDefinition(4, "Campo Norte da Vila", FeatureType.Campo,
                    new[] { 3, 11 }, new Vector2(-0.28f, 0.12f), new[] { 0 }));
                features.Add(new TileFeatureDefinition(5, "Campo Sudeste", FeatureType.Campo,
                    new[] { 5, 6 }, new Vector2(0.28f, -0.28f)));
                features.Add(new TileFeatureDefinition(6, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.28f, -0.28f)));
                break;

            case "Estrada em L":
                quantity = 9;
                features.Add(new TileFeatureDefinition(0, "Estrada em Curva", FeatureType.Estrada,
                    new[] { 7, 10 }, new Vector2(-0.12f, -0.12f)));
                features.Add(new TileFeatureDefinition(1, "Campo Externo Nordeste", FeatureType.Campo,
                    new[] { 0, 1, 2, 3, 4, 5, 6, 11 }, new Vector2(0.18f, 0.18f)));
                features.Add(new TileFeatureDefinition(2, "Campo Interno Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.33f, -0.33f)));
                break;

            case "Estrada Simples":
                quantity = 8;
                features.Add(new TileFeatureDefinition(0, "Estrada Reta", FeatureType.Estrada,
                    new[] { 1, 7 }, new Vector2(0f, 0f)));
                features.Add(new TileFeatureDefinition(1, "Campo Leste", FeatureType.Campo,
                    new[] { 2, 3, 4, 5, 6 }, new Vector2(0.28f, 0f)));
                features.Add(new TileFeatureDefinition(2, "Campo Oeste", FeatureType.Campo,
                    new[] { 0, 8, 9, 10, 11 }, new Vector2(-0.28f, 0f)));
                break;

            case "Mosteiro_Estrada":
                quantity = 2;
                features.Add(new TileFeatureDefinition(0, "Mosteiro", FeatureType.Mosteiro,
                    new int[0], new Vector2(0f, 0.05f)));
                features.Add(new TileFeatureDefinition(1, "Estrada do Mosteiro", FeatureType.Estrada,
                    new[] { 7 }, new Vector2(0f, -0.34f)));
                features.Add(new TileFeatureDefinition(2, "Campo do Mosteiro", FeatureType.Campo,
                    new[] { 0, 1, 2, 3, 4, 5, 6, 8, 9, 10, 11 }, new Vector2(-0.30f, 0.25f)));
                break;

            case "Mosteiro_Sem_Estrada":
                quantity = 4;
                features.Add(new TileFeatureDefinition(0, "Mosteiro", FeatureType.Mosteiro,
                    new int[0], new Vector2(0f, 0.05f)));
                features.Add(new TileFeatureDefinition(1, "Campo do Mosteiro", FeatureType.Campo,
                    new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Vector2(-0.30f, -0.25f)));
                break;

            case "Vila em 3":
                quantity = 4;
                features.Add(new TileFeatureDefinition(0, "Estrada Leste", FeatureType.Estrada,
                    new[] { 4 }, new Vector2(0.34f, -0.02f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Sul", FeatureType.Estrada,
                    new[] { 7 }, new Vector2(0f, -0.35f)));
                features.Add(new TileFeatureDefinition(2, "Estrada Oeste", FeatureType.Estrada,
                    new[] { 10 }, new Vector2(-0.34f, -0.02f)));
                features.Add(new TileFeatureDefinition(3, "Campo Norte", FeatureType.Campo,
                    new[] { 0, 1, 2, 3, 11 }, new Vector2(0f, 0.30f)));
                features.Add(new TileFeatureDefinition(4, "Campo Sudeste", FeatureType.Campo,
                    new[] { 5, 6 }, new Vector2(0.28f, -0.28f)));
                features.Add(new TileFeatureDefinition(5, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.28f, -0.28f)));
                break;

            case "Vila em 4":
                quantity = 1;
                features.Add(new TileFeatureDefinition(0, "Estrada Norte", FeatureType.Estrada,
                    new[] { 1 }, new Vector2(0f, 0.35f)));
                features.Add(new TileFeatureDefinition(1, "Estrada Leste", FeatureType.Estrada,
                    new[] { 4 }, new Vector2(0.35f, 0f)));
                features.Add(new TileFeatureDefinition(2, "Estrada Sul", FeatureType.Estrada,
                    new[] { 7 }, new Vector2(0f, -0.35f)));
                features.Add(new TileFeatureDefinition(3, "Estrada Oeste", FeatureType.Estrada,
                    new[] { 10 }, new Vector2(-0.35f, 0f)));
                features.Add(new TileFeatureDefinition(4, "Campo Nordeste", FeatureType.Campo,
                    new[] { 2, 3 }, new Vector2(0.28f, 0.28f)));
                features.Add(new TileFeatureDefinition(5, "Campo Sudeste", FeatureType.Campo,
                    new[] { 5, 6 }, new Vector2(0.28f, -0.28f)));
                features.Add(new TileFeatureDefinition(6, "Campo Sudoeste", FeatureType.Campo,
                    new[] { 8, 9 }, new Vector2(-0.28f, -0.28f)));
                features.Add(new TileFeatureDefinition(7, "Campo Noroeste", FeatureType.Campo,
                    new[] { 0, 11 }, new Vector2(-0.28f, 0.28f)));
                break;

            default:
                BuildFallbackTopologyFromBorders(tile, features);
                break;
        }

        tile.SetTopologyData(key, quantity, isStart, features);
    }

    private static void BuildFallbackTopologyFromBorders(CarcassoneTile tile, List<TileFeatureDefinition> features)
    {
        int nextId = 0;
        if (tile.temMosteiro)
        {
            features.Add(new TileFeatureDefinition(nextId++, "Mosteiro", FeatureType.Mosteiro, new int[0], Vector2.zero));
        }

        Borda[] edges = { Borda.Cima, Borda.Direita, Borda.Baixo, Borda.Esquerda };
        Vector2[] edgeOffsets = { new Vector2(0f, 0.32f), new Vector2(0.32f, 0f), new Vector2(0f, -0.32f), new Vector2(-0.32f, 0f) };

        for (int i = 0; i < 4; i++)
        {
            TipoTerreno t = tile.obterBordaReal(edges[i], 0);
            int basePort = i * 3;
            if (t == TipoTerreno.Cidade)
            {
                features.Add(new TileFeatureDefinition(nextId++, $"Cidade {edges[i]}", FeatureType.Cidade,
                    new[] { basePort, basePort + 1, basePort + 2 }, edgeOffsets[i]));
            }
            else if (t == TipoTerreno.Estrada)
            {
                features.Add(new TileFeatureDefinition(nextId++, $"Estrada {edges[i]}", FeatureType.Estrada,
                    new[] { basePort + 1 }, edgeOffsets[i]));
            }
        }
    }
}
