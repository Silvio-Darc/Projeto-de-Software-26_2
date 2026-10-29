using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Configuração de Índice")]
    [Tooltip("O número da cena conforme configurado no File > Build Settings")]
    [SerializeField] private int sceneIndexToLoad = 1;

    // Método 1: Usa o índice configurado no Inspector do script
    public void LoadConfiguredScene()
    {
        if (IsValidIndex(sceneIndexToLoad))
        {
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }

    // Método 2: Permite passar o índice diretamente pelo evento do botão (On Click)
    public void LoadSceneByIndex(int index)
    {
        if (IsValidIndex(index))
        {
            SceneManager.LoadScene(index);
        }
    }

    // Função auxiliar para evitar erros de índice fora da lista
    private bool IsValidIndex(int index)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
        {
            return true;
        }

        Debug.LogError($"Não foi possível carregar a cena. O índice {index} não existe no Build Settings!");
        return false;
    }
}