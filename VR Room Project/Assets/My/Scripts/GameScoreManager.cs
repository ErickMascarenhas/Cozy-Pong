using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum HitType
{
    Miss, // errou
    Bad, // forte demais
    Ok, // fraco/parado
    Perfect // ideal
}

public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance;

    [Header("Modo de Jogo")]
    public bool isImmortal = true;

    [Header("Configuracoes de Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    public UnityEvent onPlayerDeath;

    [Header("Configuracoes de Erros")]
    [HideInInspector] public bool usingErrorBoxes = false;
    private int maxErrors = 5;
    private int currentErrors = 0;
    public GameObject[] errorBoxes;
    public Color activeBoxColor = new Color(0.5f, 0f, 1f, 1f);
    public Color brokenBoxColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Estado Atual")]
    public int totalScore = 0;
    public int currentComboMultiplier = 1;
    public int consecutiveHits = 0;
    public int maxCombo = 0;

    [Header("Estatisticas")]
    public int totalHits = 0;
    public int totalMisses = 0;
    public Dictionary<HitType, int> hitCounts = new Dictionary<HitType, int>();

    [Header("Referencias")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI feedbackText;

    [Header("Referencias de UI (Barras)")]
    public Image healthBarFill;
    public Image comboBarFill;
    public Image comboBarBackground;

    private Coroutine feedbackRoutine;

    [HideInInspector] public string currentSongID;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeHitCounts();
    }

    private void Start()
    {
        ResetGame();
    }

    public void ResetGame()
    {
        currentHealth = maxHealth;
        totalScore = 0;
        currentComboMultiplier = 1;
        consecutiveHits = 0;
        totalHits = 0;
        totalMisses = 0;
        maxCombo = 0;
        currentErrors = 0;
        InitializeHitCounts();
        if (healthBarFill != null) healthBarFill.gameObject.SetActive(!usingErrorBoxes && !isImmortal);
        if (healthText != null) healthText.gameObject.SetActive(!usingErrorBoxes && !isImmortal);
        foreach (var box in errorBoxes)
        {
            if (box != null)
            {
                box.SetActive(usingErrorBoxes && !isImmortal);
                if (usingErrorBoxes)
                {
                    var img = box.GetComponent<Image>();
                    if (img != null) img.color = activeBoxColor;
                }
            }
        }
        UpdateUI();
        if (feedbackText) feedbackText.text = "";
    }

    private void InitializeHitCounts()
    {
        hitCounts.Clear();
        foreach (HitType type in System.Enum.GetValues(typeof(HitType))) hitCounts[type] = 0;

    }

    public void RegisterHit(HitType type)
    {
        if ((!usingErrorBoxes && currentHealth <= 0) || (usingErrorBoxes && currentErrors >= maxErrors)) return;

        ProcessCombo(type);

        int scoreChange = 0;
        float healthChange = 0;

        switch (type)
        {
            case HitType.Miss: // errou
                scoreChange = 0;
                healthChange = -20;
                totalMisses++;
                if (usingErrorBoxes) AddErrorBox();
                break;

            case HitType.Bad: // bateu muito forte
                scoreChange = 50;
                healthChange = 0;
                totalMisses++;
                break;

            case HitType.Ok: // bateu parado
                scoreChange = 100;
                healthChange = 5;
                totalHits++;
                break;

            case HitType.Perfect: // bateu bem
                scoreChange = 200;
                healthChange = 20;
                totalHits++;
                break;
        }

        if (hitCounts.ContainsKey(type)) hitCounts[type]++;

        if (scoreChange > 0) totalScore += scoreChange * currentComboMultiplier;
        else totalScore += scoreChange;

        if (!usingErrorBoxes) ChangeHealth(healthChange);
        UpdateUI();
        ShowFeedback(type);
    }

    private void ProcessCombo(HitType type)
    {
        bool resetsCombo = (type == HitType.Miss || type == HitType.Bad);

        if (resetsCombo)
        {
            consecutiveHits = 0;
            currentComboMultiplier = 1;
        }
        else
        {
            consecutiveHits++;
            if (consecutiveHits > maxCombo) maxCombo = consecutiveHits;
            if (consecutiveHits >= 14) currentComboMultiplier = 8;
            else if (consecutiveHits >= 6) currentComboMultiplier = 4;
            else if (consecutiveHits >= 2) currentComboMultiplier = 2;
            else currentComboMultiplier = 1;
        }
    }

    private void AddErrorBox()
    {
        if (currentErrors < maxErrors)
        {
            if (currentErrors < errorBoxes.Length && errorBoxes[currentErrors] != null)
            {
                var img = errorBoxes[currentErrors].GetComponent<Image>();
                if (img != null) img.color = brokenBoxColor;
            }
            currentErrors++;
        }
        if (currentErrors >= maxErrors && !isImmortal) Die();
    }

    private void ChangeHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0 && !isImmortal) Die();
    }

    private void Die()
    {
        if (feedbackText)
        {
            feedbackText.text = "GAME OVER";
            feedbackText.color = Color.red;
        }
        PlayerPrefs.SetInt(currentSongID + "_Played", 1);
        PlayerPrefs.Save();
        onPlayerDeath.Invoke();
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {totalScore:N0}";
        if (comboText) comboText.text = $"Combo: {currentComboMultiplier}x";
        if (healthText) healthText.text = $"HP: {currentHealth:0}";
        if (healthBarFill) healthBarFill.fillAmount = currentHealth / maxHealth;
        if (comboBarFill && comboBarBackground)
        {
            Color barColor;
            float fillAmount = 0f;
            if (consecutiveHits >= 14) // 8x (dourado)
            {
                barColor = new Color(1f, 0.84f, 0f);
                fillAmount = 1f;
            }
            else if (consecutiveHits >= 6) // 4x (roxo)
            {
                barColor = new Color(0.6f, 0f, 1f);
                fillAmount = (consecutiveHits - 5) / 9f; // 14 - 6 = 8 (+ 1 / + 1)
            }
            else if (consecutiveHits >= 2) // 2x (azul)
            {
                barColor = new Color(0f, 0.4f, 1f);
                fillAmount = (consecutiveHits - 1) / 5f; // 6 - 2 = 4 (+ 1 / + 1)
            }
            else // 1x (cinza)
            {
                barColor = new Color(0.5f, 0.5f, 0.5f);
                fillAmount = (consecutiveHits + 1) / 2f; // 2 - 0 = 2 (+ 1 / + 1)
            }
            comboBarFill.color = barColor;
            comboBarFill.fillAmount = fillAmount;
            Color bgColor = barColor * 0.25f; // mais escuro que a barra
            bgColor.a = 1f;
            comboBarBackground.color = bgColor;
        }
    }

    private void ShowFeedback(HitType type)
    {
        if (feedbackText == null) return;

        string message = "";
        Color color = Color.white;

        switch (type)
        {
            case HitType.Miss: message = "Miss"; color = Color.red; break;
            case HitType.Bad: message = "Bad"; color = new Color(1f, 0.6f, 0f); break;
            case HitType.Ok: message = "Ok"; color = Color.yellow; break;
            case HitType.Perfect: message = "Perfect!"; color = Color.green; break;
        }

        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackRoutine(message, color));
    }

    private IEnumerator FeedbackRoutine(string text, Color color)
    {
        feedbackText.text = text;
        feedbackText.color = color;
        yield return new WaitForSeconds(2f);
        feedbackText.text = "";
    }
}