using UnityEngine;

/// <summary>
/// Gerenciador central de áudio do jogo (GoF Singleton).
/// Suporta reprodução de música de fundo em loop e efeitos sonoros (SFX) para
/// jogadas inválidas, posicionamento de peças, troca de turno e cliques na UI.
/// Os componentes AudioSource e slots de AudioClip podem ser configurados diretamente no Inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música de Fundo (BGM)")]
    [Tooltip("Clipe de áudio da música de fundo contínua do jogo.")]
    [SerializeField] private AudioClip backgroundMusicClip;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Efeitos Sonoros de Tabuleiro (SFX)")]
    [Tooltip("Som reproduzido ao posicionar uma peça com sucesso no tabuleiro.")]
    [SerializeField] private AudioClip tilePlacedClip;

    [Tooltip("Som reproduzido ao tentar uma jogada inválida (encaixe incompatível, clique incorreto, etc.).")]
    [SerializeField] private AudioClip invalidMoveClip;

    [Tooltip("Som reproduzido na troca ou início de turno de um novo jogador.")]
    [SerializeField] private AudioClip turnChangedClip;

    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    [Header("Efeitos Sonoros de Interface (UI)")]
    [Tooltip("Som reproduzido ao selecionar/clicar botões nos menus e no HUD.")]
    [SerializeField] private AudioClip buttonClickClip;

    [Range(0f, 1f)] [SerializeField] private float uiVolume = 1f;

    [Header("Fontes de Áudio (AudioSources)")]
    [Tooltip("AudioSource responsável pela trilha sonora (Loop ativado).")]
    [SerializeField] private AudioSource bgmSource;

    [Tooltip("AudioSource responsável pelos efeitos sonoros do tabuleiro.")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("AudioSource responsável pelos sons de botões da interface.")]
    [SerializeField] private AudioSource uiSource;

    public const string PrefKeyMusicVolume = "CARCASSONNE_BGM_VOLUME";
    public const string PrefKeySfxVolume = "CARCASSONNE_SFX_VOLUME";

    public event System.Action<float> OnMusicVolumeChanged;
    public event System.Action<float> OnSfxVolumeChanged;

    // Propriedades públicas para acesso e configuração
    public AudioSource BgmSource => bgmSource;
    public AudioSource SfxSource => sfxSource;
    public AudioSource UiSource => uiSource;

    public AudioClip BackgroundMusicClip
    {
        get => backgroundMusicClip;
        set => backgroundMusicClip = value;
    }

    public AudioClip TilePlacedClip
    {
        get => tilePlacedClip;
        set => tilePlacedClip = value;
    }

    public AudioClip InvalidMoveClip
    {
        get => invalidMoveClip;
        set => invalidMoveClip = value;
    }

    public AudioClip TurnChangedClip
    {
        get => turnChangedClip;
        set => turnChangedClip = value;
    }

    public AudioClip ButtonClickClip
    {
        get => buttonClickClip;
        set => buttonClickClip = value;
    }

    public float MusicVolume
    {
        get => musicVolume;
        set => SetMusicVolume(value);
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set => SetSfxVolume(value);
    }

    public float UiVolume
    {
        get => uiVolume;
        set => SetSfxVolume(value);
    }

    public void SetMusicVolume(float volume, bool saveToPrefs = true)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = musicVolume;
        if (saveToPrefs)
        {
            PlayerPrefs.SetFloat(PrefKeyMusicVolume, musicVolume);
            PlayerPrefs.Save();
        }
        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    public void SetSfxVolume(float volume, bool saveToPrefs = true)
    {
        sfxVolume = Mathf.Clamp01(volume);
        uiVolume = sfxVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (uiSource != null) uiSource.volume = uiVolume;
        if (saveToPrefs)
        {
            PlayerPrefs.SetFloat(PrefKeySfxVolume, sfxVolume);
            PlayerPrefs.Save();
        }
        OnSfxVolumeChanged?.Invoke(sfxVolume);
    }

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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            musicVolume = PlayerPrefs.GetFloat(PrefKeyMusicVolume, 0.5f);
            sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVolume, 1.0f);
            uiVolume = sfxVolume;
            EnsureAudioSourcesCreated();
        }
        else if (Instance != this)
        {
            Instance.InheritClipsFrom(this);
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Copia referências de clipes de áudio configurados no Inspector de uma duplicata para a instância persistente.
    /// </summary>
    public void InheritClipsFrom(AudioManager other)
    {
        if (other == null) return;

        if (backgroundMusicClip == null && other.backgroundMusicClip != null)
        {
            backgroundMusicClip = other.backgroundMusicClip;
            if (playMusicOnStart && bgmSource != null && !bgmSource.isPlaying)
            {
                PlayBackgroundMusic(backgroundMusicClip);
            }
        }
        if (tilePlacedClip == null && other.tilePlacedClip != null) tilePlacedClip = other.tilePlacedClip;
        if (invalidMoveClip == null && other.invalidMoveClip != null) invalidMoveClip = other.invalidMoveClip;
        if (turnChangedClip == null && other.turnChangedClip != null) turnChangedClip = other.turnChangedClip;
        if (buttonClickClip == null && other.buttonClickClip != null) buttonClickClip = other.buttonClickClip;
    }

    private void Start()
    {
        if (playMusicOnStart && backgroundMusicClip != null)
        {
            PlayBackgroundMusic(backgroundMusicClip);
        }
    }

    /// <summary>
    /// Garante que o AudioManager exista na cena em tempo de execução.
    /// </summary>
    public static AudioManager EnsureExists()
    {
        if (Instance != null) return Instance;

        AudioManager existing = FindFirstObjectByType<AudioManager>();
        if (existing != null)
        {
            Instance = existing;
            existing.transform.SetParent(null);
            DontDestroyOnLoad(existing.gameObject);
            return Instance;
        }

        GameObject audioHost = new GameObject("AudioManager");
        audioHost.transform.SetParent(null);
        AudioManager manager = audioHost.AddComponent<AudioManager>();
        return manager;
    }

    /// <summary>
    /// Cria automaticamente os componentes AudioSource caso não tenham sido associados manualmente no Inspector.
    /// </summary>
    private void EnsureAudioSourcesCreated()
    {
        if (bgmSource == null)
        {
            bgmSource = CreateDedicatedSource("BGM_Source", loop: true, musicVolume);
        }

        if (sfxSource == null)
        {
            sfxSource = CreateDedicatedSource("SFX_Source", loop: false, sfxVolume);
        }

        if (uiSource == null)
        {
            uiSource = CreateDedicatedSource("UI_Source", loop: false, uiVolume);
        }
    }

    private AudioSource CreateDedicatedSource(string sourceName, bool loop, float defaultVolume)
    {
        GameObject child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f; // Som estéreo 2D puro
        source.volume = defaultVolume;
        return source;
    }

    // ==========================================
    // REPRODUÇÃO DE MÚSICA DE FUNDO
    // ==========================================

    /// <summary>
    /// Inicia a reprodução contínua da música de fundo.
    /// Se nenhum clipe for passado por parâmetro, utiliza o backgroundMusicClip configurado.
    /// </summary>
    public void PlayBackgroundMusic(AudioClip clip = null, bool loop = true)
    {
        if (clip != null)
        {
            backgroundMusicClip = clip;
        }

        if (bgmSource == null || backgroundMusicClip == null) return;

        if (bgmSource.clip == backgroundMusicClip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = backgroundMusicClip;
        bgmSource.loop = loop;
        bgmSource.volume = musicVolume;
        bgmSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // ==========================================
    // EVENTOS SONOROS SOLICITADOS
    // ==========================================

    /// <summary>
    /// 1. Som de Jogada Inválida (tentativa de encaixe incompatível, clique fora, etc.).
    /// </summary>
    public void PlayInvalidMove()
    {
        PlayClipOnSource(sfxSource, invalidMoveClip, sfxVolume, "Jogada Inválida");
    }

    /// <summary>
    /// 2. Som de Seleção/Clique em Botões na UI.
    /// </summary>
    public void PlayButtonClick()
    {
        PlayClipOnSource(uiSource, buttonClickClip, uiVolume, "Clique de Botão");
    }

    /// <summary>
    /// 3. Som de Posicionamento de Peça com sucesso no tabuleiro.
    /// </summary>
    public void PlayTilePlaced()
    {
        PlayClipOnSource(sfxSource, tilePlacedClip, sfxVolume, "Posicionamento de Peça");
    }

    /// <summary>
    /// 4. Som de Troca de Turno (novo jogador assume a vez).
    /// </summary>
    public void PlayTurnChanged()
    {
        PlayClipOnSource(sfxSource, turnChangedClip, sfxVolume, "Troca de Turno");
    }

    /// <summary>
    /// Método genérico para disparar um efeito sonoro avulso (PlayOneShot).
    /// </summary>
    public void PlayCustomSFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        if (sfxSource == null) EnsureAudioSourcesCreated();
        sfxSource?.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
    }

    private void PlayClipOnSource(AudioSource source, AudioClip clip, float volume, string actionName)
    {
        if (clip == null)
        {
            // O usuário ainda não atribuiu o clipe no Inspector; não gera erro.
            return;
        }

        if (source == null)
        {
            EnsureAudioSourcesCreated();
        }

        if (source != null)
        {
            source.PlayOneShot(clip, volume);
        }
    }
}
