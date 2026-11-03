using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Painel de Configurações de Áudio (Menu de Opções).
/// Permite ajustar em tempo real os volumes da Música de Fundo (BGM)
/// e dos Efeitos Sonoros (SFX/UI), persistindo as escolhas do jogador via PlayerPrefs.
/// Integrado ao sistema de navegação UIPanel / UIManager.
/// </summary>
public class AudioOptionsPanel : UIPanel
{
    [Header("Controles de Volume da Música")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicPercentText;

    [Header("Controles de Volume de Efeitos")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxPercentText;

    [Header("Navegação")]
    [SerializeField] private Button backButton;

    protected override void Awake()
    {
        base.Awake();
        BindControls();
    }

    private void Start()
    {
        SyncValuesFromAudioManager();
    }

    public override void Open()
    {
        base.Open();
        SyncValuesFromAudioManager();
    }

    private void BindControls()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void SyncValuesFromAudioManager()
    {
        float musicVol = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 0.5f;
        float sfxVol = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1.0f;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVol);
        }
        UpdateMusicLabel(musicVol);

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVol);
        }
        UpdateSfxLabel(sfxVol);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        UpdateMusicLabel(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
        UpdateSfxLabel(value);
    }

    private void UpdateMusicLabel(float value)
    {
        if (musicPercentText != null)
        {
            musicPercentText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }

    private void UpdateSfxLabel(float value)
    {
        if (sfxPercentText != null)
        {
            sfxPercentText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }

    private void OnBackButtonClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        OnBackPressed();
    }
}
