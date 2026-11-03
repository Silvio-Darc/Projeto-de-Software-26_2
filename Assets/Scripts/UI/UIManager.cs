using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Tela Inicial")]
    [SerializeField] private UIPanel initialPanel;

    // Pilha que guarda o histórico de navegação
    private Stack<UIPanel> panelStack = new Stack<UIPanel>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            AudioManager.EnsureExists();
        }
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (initialPanel != null)
        {
            OpenPanel(initialPanel);
        }
    }

    private void Update()
    {
        // Atalho universal para voltar ou abrir pause
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (panelStack.Count > 1)
            {
                panelStack.Peek().OnBackPressed();
            }
        }
    }

    // Abre um novo submenu (empilha)
    public void OpenPanel(UIPanel newPanel)
    {
        AudioManager.Instance?.PlayButtonClick();
        if (panelStack.Count > 0)
        {
            panelStack.Peek().Close();
        }

        panelStack.Push(newPanel);
        newPanel.Open();

        // Chama a função para debugar a stack atualizada
        LogStackState();
    }

    // Volta para a tela anterior (desempilha)
    public void GoBack()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (panelStack.Count <= 1) return; // Não fecha o menu raiz

        UIPanel current = panelStack.Pop();
        current.Close();

        // Reabre o painel anterior
        UIPanel previous = panelStack.Peek();
        previous.Open();

        // Chama a função para debugar a stack atualizada
        LogStackState();
    }

    private void LogStackState()
    {
        // this.GetInstanceID() mostra um número único para este componente. 
        // Se esse número mudar no log, você tem dois UIManagers rodando.
        string stackInfo = $"--- STACK [Manager ID: {this.GetEntityId()}] | Total de Painéis: {panelStack.Count} ---\n";

        foreach (UIPanel painel in panelStack)
        {
            stackInfo += $"> {painel.gameObject.name}\n";
        }

        Debug.LogWarning(stackInfo); // Usando Warning para ficar amarelo e fácil de ver no Console
    }
}