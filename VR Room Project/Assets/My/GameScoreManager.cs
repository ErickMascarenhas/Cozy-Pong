using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Configuracoes de Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    public UnityEvent onPlayerDeath;

    [Header("Estado Atual")]
    public int totalScore = 0;
    public int currentComboMultiplier = 1;
    public int consecutiveHits = 0;

    [Header("Estatisticas")]
    public int totalHits = 0;
    public int totalMisses = 0;
    public Dictionary<HitType, int> hitCounts = new Dictionary<HitType, int>();

    [Header("Referencias")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI feedbackText;

    private Coroutine feedbackRoutine;

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

        InitializeHitCounts();
        UpdateUI();

        if (feedbackText) feedbackText.text = "";
    }

    private void InitializeHitCounts()
    {
        hitCounts.Clear();
        foreach (HitType type in System.Enum.GetValues(typeof(HitType)))
        {
            hitCounts[type] = 0;
        }
    }

    public void RegisterHit(HitType type)
    {
        if (currentHealth <= 0) return;

        ProcessCombo(type);

        int scoreChange = 0;
        float healthChange = 0;

        switch (type)
        {
            case HitType.Miss: // errou
                scoreChange = -100;
                healthChange = -20;
                totalMisses++;
                break;

            case HitType.Bad: // bateu muito forte
                scoreChange = -50;
                healthChange = -10;
                totalMisses++;
                break;

            case HitType.Ok: // bateu parado
                scoreChange = -25;
                healthChange = -5;
                totalMisses++;
                break;

            case HitType.Perfect: // batem bem
                scoreChange = 200;
                healthChange = 10;
                totalHits++;
                break;
        }

        if (hitCounts.ContainsKey(type)) hitCounts[type]++;

        if (scoreChange > 0)
            totalScore += scoreChange * currentComboMultiplier;
        else
            totalScore += scoreChange;

        ChangeHealth(healthChange);
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
            if (consecutiveHits >= 14) currentComboMultiplier = 8;
            else if (consecutiveHits >= 6) currentComboMultiplier = 4;
            else if (consecutiveHits >= 2) currentComboMultiplier = 2;
            else currentComboMultiplier = 1;
        }
    }

    private void ChangeHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (feedbackText)
        {
            feedbackText.text = "GAME OVER";
            feedbackText.color = Color.red;
        }
        onPlayerDeath.Invoke();
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {totalScore:N0}";
        if (comboText) comboText.text = $"Combo: {currentComboMultiplier}x";
        if (healthText) healthText.text = $"HP: {currentHealth:0}";
    }

    private void ShowFeedback(HitType type)
    {
        if (feedbackText == null) return;

        string message = "";
        Color color = Color.white;

        switch (type)
        {
            case HitType.Miss: message = "Miss"; color = Color.red; break;
            case HitType.Bad: message = "Too Hard!"; color = new Color(1f, 0.6f, 0f); break;
            case HitType.Ok: message = "Bad"; color = Color.yellow; break;
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