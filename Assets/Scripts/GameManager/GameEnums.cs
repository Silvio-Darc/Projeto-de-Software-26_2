/// <summary>
/// Tipos de estruturas (terrenos/regiões) do Carcassonne tradicional.
/// </summary>
public enum FeatureType
{
    Campo = 0,
    Estrada = 1,
    Cidade = 2,
    Mosteiro = 3
}

/// <summary>
/// Papel assumido por um Meeple quando posicionado no tabuleiro.
/// Substitui os inteiros mágicos anteriormente usados em Meeple.cs.
/// </summary>
public enum MeepleRole
{
    Disponivel = 0,
    FazendeiroCampo = 1,
    LadraoEstrada = 2,
    CavaleiroCidade = 3,
    MongeMosteiro = 4
}

/// <summary>
/// Estado de prontidão/conexão do jogador no lobby.
/// Substitui os inteiros mágicos anteriormente usados em Player.cs e GameLobby.cs.
/// </summary>
public enum ConnectionStatus
{
    Disconnected = 0,
    Connected = 1,
    Connecting = 2,
    Waiting = 3,
    Ready = 4
}

/// <summary>
/// Níveis de dificuldade para estratégias de Bots (Padrão GoF Strategy).
/// </summary>
public enum BotDifficulty
{
    Facil = 0,
    Medio = 1
}
