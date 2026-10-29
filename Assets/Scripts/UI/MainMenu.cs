using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public static class GameConfig
{
    public static int SelectedPlayerCount = 2; // Padrão
}
public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown playerCountDropdown; // Opcional se usar dropdown
    [SerializeField] private string gameSceneName = "SampleScene"; // Nome da cena de jogo
    // Opção A: Chamado por botões individuais no Canvas (On Click)
    // Botão 2: OnClick -> SetPlayerCount(2)
    // Botão 3: OnClick -> SetPlayerCount(3)
    // Botão 4: OnClick -> SetPlayerCount(4)
    public void SetPlayerCount(int count)
    {
        GameConfig.SelectedPlayerCount = Mathf.Clamp(count, 2, 4);
    }
    // Opção B: Chamado quando o jogador muda a opção de um TMP_Dropdown
    public void OnDropdownChanged()
    {
        // Se as opções forem: 0 = "2 Jogadores", 1 = "3 Jogadores", 2 = "4 Jogadores"
        GameConfig.SelectedPlayerCount = playerCountDropdown.value + 2;
    }
    // Chamado pelo botão "Iniciar Jogo"
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}