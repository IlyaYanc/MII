using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class GameplayUI : MonoBehaviour
{
    [SerializeField]
    private StatsContainer playerStats;
    [SerializeField]
    private SpellsController spellsController;

    [Header("UI Panels")]
    [Space(10)]
    [SerializeField]
    private GameObject gameplayPanel;
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private GameObject optionsPanel;
    [SerializeField]
    private GameObject levelEndPanel;
    [SerializeField]
    private GameObject gameOverPanel;

    private GameObject activePanel;

    [Header("Stats")]
    [Space(10)]
    [SerializeField]
    private Slider playerHealth;
    [SerializeField]
    private Slider playerMana;

    [Header("Spells")]
    [Space(10)]
    [SerializeField]
    private Image[] spellsImages;
    [SerializeField]
    private SliderTimer[] spellsCooldowns;

    [Header("Other Stats")]
    [Space(10)]
    [SerializeField]
    private Image[] livesImages;
    [SerializeField]
    private TMP_Text timeText;
    [SerializeField]
    private Image[] keysImages;
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_Text enemiesText;

    [Header("Settings")]
    [Space(10)]
    [SerializeField]
    private TMP_Text qualityText;
    [SerializeField]
    private Slider volumeSlider;

    public static GameplayUI instance = null;

    private Image selectedSpellImage;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        activePanel = gameplayPanel;

        selectedSpellImage = spellsImages[0];
        for (int i = 1; i < spellsImages.Length; i++)
        {
            spellsImages[i].color = Color.grey;
        }
    }

    private void Start()
    {
        playerHealth.maxValue = playerStats.GetMaxHealth();
        playerMana.maxValue = playerStats.GetMaxMana();

        for(int i = 0; i < spellsCooldowns.Length; i++)
        {
            spellsCooldowns[i].SetTimerSlider(spellsController.GetCooldownTimer(i));
        }

        UpdateQualityText();
        UpdateSpellImages();
    }

    private void Update()
    {
        playerHealth.value = playerStats.Health;
        playerMana.value = playerStats.Mana;

        timeText.text = GameManager.instance.TimeToString();

    }

    public void UpdateSpellImages()
    {
        selectedSpellImage.color = Color.gray;
        selectedSpellImage = spellsImages[spellsController.SelectedSpellIndex];
        selectedSpellImage.color = Color.white;
    }

    public void UpdateLivesImages()
    {
        for(int i = 0; i < livesImages.Length; i++)
        {
            livesImages[i].enabled = i < GameManager.instance.Lives ? true : false;
        }
    }

    public void UpdateKeysImages()
    {
        for (int i = 0; i < keysImages.Length; i++)
        {
            keysImages[i].color = i < GameManager.instance.KeysFound ? Color.white : Color.gray;
        }
    }

    public void UpdateScore(int value)
    {
        scoreText.text = value.ToString();
    }

    public void UpdateEnemiesKilled(int value)
    {
        enemiesText.text = value.ToString();
    }

    public void ChangeState(GameState state)
    {
        activePanel.SetActive(false);
        switch (state) 
        {
            case GameState.GS_GAME:
                activePanel = gameplayPanel;
                break;
            case GameState.GS_PAUSEMENU:
                activePanel = pausePanel;
                break;
            case GameState.GS_OPTIONS:
                activePanel = optionsPanel;
                break;
            case GameState.GS_LEVELCOMPLETED:
                activePanel = levelEndPanel;
                break;
            case GameState.GS_GAME_OVER:
                activePanel = gameOverPanel;
                break;
            default: break;
        }
        activePanel.SetActive(true);
    }

    public void OptionsButton()
    {
        GameManager.instance.Options();
    }

    public void ResumeButton()
    {
        GameManager.instance.InGame();
    }

    public void RestartButton()
    {
        GameManager.instance.OnRestartButton();
    }

    public void ExitButton()
    {
        GameManager.instance.OnExitButton();
    }

    public void IncreaseQualityButton()
    {
        GameManager.instance.IncreaseQuality();
        UpdateQualityText();
    }

    public void DecreaseQualityButton()
    {
        GameManager.instance.DecreaseQuality();
        UpdateQualityText();
    }

    public void UpdateQualityText()
    {
        qualityText.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    public float VolumeSliderValue()
    {
        return volumeSlider.value;
    }

    public void UpdateVolumeLevel()
    {
        GameManager.instance.SetVolume();
    }
}
