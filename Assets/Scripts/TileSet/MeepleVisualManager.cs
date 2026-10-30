using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gerencia os marcadores visuais clicáveis (MeeplePin) na fase de posicionamento de Meeple
/// e os marcadores visuais permanentes dos Meeples alocados sobre as sub-regiões do tabuleiro.
/// Implementa o padrão GoF Observer reagindo aos eventos do Turn e do Board.
/// </summary>
public class MeepleVisualManager : MonoBehaviour
{
    private class ActivePinMarker
    {
        public GameObject RootObject;
        public TileFeatureDefinition Feature;
        public Vector3 WorldPosition;
        public SpriteRenderer Renderer;
    }

    private Board board;
    private Turn turnManager;

    private readonly List<ActivePinMarker> activePins = new List<ActivePinMarker>();
    private readonly Dictionary<Vector3Int, GameObject> placedMeepleVisuals = new Dictionary<Vector3Int, GameObject>();

    private static Sprite cachedCircleSprite;
    private static Sprite cachedMeepleSprite;
    private static Sprite cachedRingSprite;

    private const float PinClickRadius = 0.20f;
    private const float PinBaseScale = 0.22f;
    private const float MeepleVisualScale = 0.28f;

    [Header("Proteção contra Clique Acidental")]
    [Tooltip("Atraso mínimo (em segundos) após posicionar o tile antes de aceitar clique nos marcadores de Meeple")]
    [SerializeField] private float meepleClickDelay = 0.35f;

    private float pinClickAllowedTime = 0f;
    private bool mouseReleasedSinceSpawn = false;

    private void Awake()
    {
        EnsureProceduralSpritesCreated();
    }

    private void Start()
    {
        BindReferences();
    }

    private void OnEnable()
    {
        BindReferences();
    }

    private void OnDisable()
    {
        UnbindReferences();
    }

    private void BindReferences()
    {
        if (board == null) board = FindFirstObjectByType<Board>();
        if (turnManager == null) turnManager = Turn.Instance != null ? Turn.Instance : FindFirstObjectByType<Turn>();

        if (turnManager != null)
        {
            turnManager.OnMeepleSelectionAvailable -= HandleMeepleSelectionAvailable;
            turnManager.OnMeepleSelectionAvailable += HandleMeepleSelectionAvailable;

            turnManager.OnMeepleSelectionHidden -= ClearActivePins;
            turnManager.OnMeepleSelectionHidden += ClearActivePins;
        }

        if (board != null)
        {
            board.OnMeeplePlaced -= HandleMeeplePlaced;
            board.OnMeeplePlaced += HandleMeeplePlaced;

            board.OnMeepleRemoved -= HandleMeepleRemoved;
            board.OnMeepleRemoved += HandleMeepleRemoved;
        }
    }

    private void UnbindReferences()
    {
        if (turnManager != null)
        {
            turnManager.OnMeepleSelectionAvailable -= HandleMeepleSelectionAvailable;
            turnManager.OnMeepleSelectionHidden -= ClearActivePins;
        }

        if (board != null)
        {
            board.OnMeeplePlaced -= HandleMeeplePlaced;
            board.OnMeepleRemoved -= HandleMeepleRemoved;
        }
    }

    private void Update()
    {
        if (activePins.Count == 0 || Camera.main == null || Mouse.current == null) return;
        if (turnManager == null || turnManager.CurrentPhase != Turn.TurnPhase.PosicionarMeeple)
        {
            ClearActivePins();
            return;
        }

        Player current = turnManager.GetCurrentPlayer();
        if (current != null && current.IsBot) return;

        if (!Mouse.current.leftButton.isPressed)
        {
            mouseReleasedSinceSpawn = true;
        }

        bool canClickPins = Time.unscaledTime >= pinClickAllowedTime && mouseReleasedSinceSpawn;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.10f;
        ActivePinMarker hoveredPin = null;
        float closestDist = PinClickRadius;

        if (canClickPins)
        {
            for (int i = 0; i < activePins.Count; i++)
            {
                ActivePinMarker pin = activePins[i];
                if (pin.RootObject == null) continue;

                float dist = Vector2.Distance(new Vector2(mouseWorld.x, mouseWorld.y), new Vector2(pin.WorldPosition.x, pin.WorldPosition.y));
                if (dist <= closestDist)
                {
                    closestDist = dist;
                    hoveredPin = pin;
                }
            }
        }

        for (int i = 0; i < activePins.Count; i++)
        {
            ActivePinMarker pin = activePins[i];
            if (pin.RootObject == null) continue;

            bool isHovered = canClickPins && (pin == hoveredPin);
            float scale = isHovered ? PinBaseScale * 1.38f : PinBaseScale * pulse;
            pin.RootObject.transform.localScale = new Vector3(scale, scale, 1f);
        }

        if (canClickPins && hoveredPin != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            int chosenFeatureId = hoveredPin.Feature.LocalFeatureId;
            turnManager.ExecuteCommand(new PlaceMeepleCommand(chosenFeatureId));
        }
    }

    private void HandleMeepleSelectionAvailable(PlacedTile placedTile, IReadOnlyList<TileFeatureDefinition> availableFeatures)
    {
        ClearActivePins();

        if (placedTile == null || availableFeatures == null || availableFeatures.Count == 0) return;
        if (turnManager != null && turnManager.GetCurrentPlayer() != null && turnManager.GetCurrentPlayer().IsBot)
        {
            return;
        }

        pinClickAllowedTime = Time.unscaledTime + meepleClickDelay;
        mouseReleasedSinceSpawn = false;

        Vector3 cellCenter = GetCellWorldCenter(placedTile.Position);
        Player currentPlayer = turnManager != null ? turnManager.GetCurrentPlayer() : null;
        Color playerColor = currentPlayer != null ? currentPlayer.PlayerColor : Color.white;

        for (int i = 0; i < availableFeatures.Count; i++)
        {
            TileFeatureDefinition feature = availableFeatures[i];
            Vector2 rotatedOffset = TileFeatureDefinition.RotateLocalOffset(feature.PinLocalOffset, placedTile.Rotation);
            Vector3 pinPos = cellCenter + new Vector3(rotatedOffset.x, rotatedOffset.y, -0.2f);

            GameObject pinObj = new GameObject($"MeeplePin_{feature.Type}_{feature.LocalFeatureId}");
            pinObj.transform.SetParent(transform, true);
            pinObj.transform.position = pinPos;
            pinObj.transform.localScale = new Vector3(PinBaseScale, PinBaseScale, 1f);

            // Anel externo indicando o tipo de estrutura (Cidade/Estrada/Mosteiro/Campo)
            SpriteRenderer outerRing = pinObj.AddComponent<SpriteRenderer>();
            outerRing.sprite = cachedRingSprite;
            outerRing.color = GetFeatureAccentColor(feature.Type);
            outerRing.sortingOrder = 25;

            // Núcleo interno com a cor do jogador atual
            GameObject innerDot = new GameObject("InnerPlayerDot");
            innerDot.transform.SetParent(pinObj.transform, false);
            innerDot.transform.localScale = new Vector3(0.68f, 0.68f, 1f);
            SpriteRenderer innerRenderer = innerDot.AddComponent<SpriteRenderer>();
            innerRenderer.sprite = cachedCircleSprite;
            Color innerColor = Color.Lerp(playerColor, Color.white, 0.15f);
            innerColor.a = 0.92f;
            innerRenderer.color = innerColor;
            innerRenderer.sortingOrder = 26;

            activePins.Add(new ActivePinMarker
            {
                RootObject = pinObj,
                Feature = feature,
                WorldPosition = pinPos,
                Renderer = innerRenderer
            });
        }
    }

    public void ClearActivePins()
    {
        for (int i = 0; i < activePins.Count; i++)
        {
            if (activePins[i].RootObject != null)
            {
                Destroy(activePins[i].RootObject);
            }
        }
        activePins.Clear();
    }

    private void HandleMeeplePlaced(PlacedTile placedTile, Meeple meeple)
    {
        ClearActivePins();
        if (placedTile == null || meeple == null) return;

        if (placedMeepleVisuals.TryGetValue(placedTile.Position, out GameObject existing) && existing != null)
        {
            Destroy(existing);
        }

        TileFeatureDefinition feature = placedTile.Definition?.GetFeatureById(placedTile.PlacedFeatureId);
        Vector2 localOffset = feature != null ? feature.PinLocalOffset : Vector2.zero;
        Vector2 rotatedOffset = TileFeatureDefinition.RotateLocalOffset(localOffset, placedTile.Rotation);
        Vector3 worldPos = GetCellWorldCenter(placedTile.Position) + new Vector3(rotatedOffset.x, rotatedOffset.y, -0.3f);

        GameObject meepleRoot = new GameObject($"PlacedMeeple_{placedTile.Position.x}_{placedTile.Position.y}");
        meepleRoot.transform.SetParent(transform, true);
        meepleRoot.transform.position = worldPos;
        meepleRoot.transform.localScale = new Vector3(MeepleVisualScale, MeepleVisualScale, 1f);

        // Fazendeiros ficam deitados (90 graus) sobre o campo, como no Carcassonne físico
        if (meeple.Role == MeepleRole.FazendeiroCampo)
        {
            meepleRoot.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        // Contorno escuro para legibilidade sobre qualquer tile
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(meepleRoot.transform, false);
        outlineObj.transform.localScale = new Vector3(1.22f, 1.22f, 1f);
        SpriteRenderer outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = cachedMeepleSprite;
        outlineRenderer.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        outlineRenderer.sortingOrder = 30;

        // Corpo colorido do Meeple
        SpriteRenderer bodyRenderer = meepleRoot.AddComponent<SpriteRenderer>();
        bodyRenderer.sprite = cachedMeepleSprite;
        bodyRenderer.color = meeple.Owner != null ? meeple.Owner.PlayerColor : Color.white;
        bodyRenderer.sortingOrder = 31;

        placedMeepleVisuals[placedTile.Position] = meepleRoot;
    }

    private void HandleMeepleRemoved(PlacedTile placedTile, Meeple meeple)
    {
        if (placedTile == null) return;
        if (placedMeepleVisuals.TryGetValue(placedTile.Position, out GameObject visual))
        {
            if (visual != null)
            {
                Destroy(visual);
            }
            placedMeepleVisuals.Remove(placedTile.Position);
        }
    }

    private Vector3 GetCellWorldCenter(Vector3Int cellPos)
    {
        if (board != null && board.tilemap != null)
        {
            return board.tilemap.GetCellCenterWorld(cellPos);
        }
        return new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0f);
    }

    private static Color GetFeatureAccentColor(FeatureType type)
    {
        switch (type)
        {
            case FeatureType.Cidade: return new Color(1f, 0.78f, 0.15f, 0.98f);   // Dourado (Cidade)
            case FeatureType.Estrada: return new Color(0.95f, 0.95f, 0.95f, 0.98f); // Branco (Estrada)
            case FeatureType.Mosteiro: return new Color(0.85f, 0.35f, 1f, 0.98f);  // Roxo/Lilás (Mosteiro)
            case FeatureType.Campo: return new Color(0.30f, 0.95f, 0.40f, 0.98f);  // Verde Claro (Fazenda)
            default: return Color.white;
        }
    }

    private static void EnsureProceduralSpritesCreated()
    {
        if (cachedCircleSprite == null)
        {
            cachedCircleSprite = CreateCircleSprite(64, 0f, 0.95f);
        }
        if (cachedRingSprite == null)
        {
            cachedRingSprite = CreateCircleSprite(64, 0.62f, 0.98f);
        }
        if (cachedMeepleSprite == null)
        {
            cachedMeepleSprite = CreateMeepleSilhouetteSprite(64);
        }
    }

    private static Sprite CreateCircleSprite(int size, float innerRadiusNorm, float outerRadiusNorm)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = (size - 1) * 0.5f;
        float maxR = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / maxR;
                if (dist >= innerRadiusNorm && dist <= outerRadiusNorm)
                {
                    float edgeAlpha = Mathf.Clamp01((outerRadiusNorm - dist) * 16f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, edgeAlpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateMeepleSilhouetteSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            float ny = (y / (float)(size - 1)) * 2f - 1f; // -1 (base) .. +1 (topo)
            for (int x = 0; x < size; x++)
            {
                float nx = (x / (float)(size - 1)) * 2f - 1f; // -1 .. +1
                float ax = Mathf.Abs(nx);

                // Cabeça do Meeple
                bool head = Vector2.Distance(new Vector2(nx, ny), new Vector2(0f, 0.58f)) <= 0.28f;
                // Braços abertos
                bool arms = ny >= 0.02f && ny <= 0.34f && ax <= 0.72f && (ny <= 0.32f - (ax * 0.15f));
                // Tronco central
                bool torso = ny >= -0.35f && ny <= 0.35f && ax <= 0.36f;
                // Pernas abertas na base
                bool legs = ny >= -0.85f && ny <= -0.25f && ax >= (0.06f + (-0.25f - ny) * 0.25f) && ax <= (0.42f + (-0.25f - ny) * 0.42f);

                if (head || arms || torso || legs)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
