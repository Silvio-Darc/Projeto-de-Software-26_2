using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Configurações de Panning (Arrasto)")]
    [Tooltip("Velocidade do arrasto com o rato")]
    [SerializeField] private float mousePanSpeed = 0.5f;
    [Tooltip("Velocidade do movimento com as teclas (WASD)")]
    [SerializeField] private float keyboardPanSpeed = 15f;

    [Header("Limites de Distância (Bounds)")]
    [SerializeField] private bool useBounds = true;
    [Tooltip("Limite mínimo e máximo no eixo Horizontal (X)")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [Tooltip("Limite mínimo e máximo no eixo Vertical (Y)")]
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    [Header("Configurações de Zoom")]
    [SerializeField] private float zoomSpeed = 0.05f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    private Camera cam;

    private InputAction panToggleAction;
    private InputAction pointerDeltaAction;
    private InputAction keyboardMoveAction;
    private InputAction zoomAction;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        panToggleAction = new InputAction("PanToggle", binding: "<Mouse>/middleButton");
        pointerDeltaAction = new InputAction("PointerDelta", binding: "<Mouse>/delta");

        keyboardMoveAction = new InputAction("KeyboardMove");
        keyboardMoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        zoomAction = new InputAction("Zoom", binding: "<Mouse>/scroll/y");
    }

    private void OnEnable()
    {
        panToggleAction.Enable();
        pointerDeltaAction.Enable();
        keyboardMoveAction.Enable();
        zoomAction.Enable();
    }

    private void OnDisable()
    {
        panToggleAction.Disable();
        pointerDeltaAction.Disable();
        keyboardMoveAction.Disable();
        zoomAction.Disable();
    }

    private void Update()
    {
        HandlePanning();
        HandleZoom();

        // Aplica as restrições de distância no final de cada frame
        ClampPosition();
    }

    private void HandlePanning()
    {
        // Panning com o rato
        if (panToggleAction.IsPressed())
        {
            Vector2 delta = pointerDeltaAction.ReadValue<Vector2>();
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * mousePanSpeed * Time.deltaTime;
            transform.Translate(move, Space.Self);
        }

        // Panning com o teclado
        Vector2 keyboardMove = keyboardMoveAction.ReadValue<Vector2>();
        if (keyboardMove != Vector2.zero)
        {
            Vector3 keyMove = new Vector3(keyboardMove.x, keyboardMove.y, 0) * keyboardPanSpeed * Time.deltaTime;
            transform.Translate(keyMove, Space.Self);
        }
    }

    private void HandleZoom()
    {
        float scroll = zoomAction.ReadValue<float>();

        if (scroll != 0)
        {
            float zoomAmount = scroll * zoomSpeed;

            if (cam.orthographic)
            {
                cam.orthographicSize -= zoomAmount;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
            else
            {
                cam.fieldOfView -= zoomAmount;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
            }
        }
    }

    private void ClampPosition()
    {
        if (!useBounds) return;

        Vector3 currentPos = transform.position;

        // Limita a posição X entre minX e maxX, e a Y entre minY e maxY
        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
        currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

        transform.position = currentPos;
    }

    // Desenha uma caixa azul na aba Scene para ajudar a visualizar os limites configurados
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1f);

        Gizmos.DrawWireCube(center, size);
    }
}