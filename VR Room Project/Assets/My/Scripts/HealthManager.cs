using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public enum GameDifficulty
{
    Easy, Medium, Hard // 5 a 3 erros, ordem decrescente
}

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;
    [Header("Configuracao")]
    public GameDifficulty difficulty = GameDifficulty.Easy;
    [Header("UI")]
    public GameObject[] healthBoxes;
    public Color activeColor = new Color(0.5f, 0f, 1f, 1f);
    public Color brokenColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private int currentErrors = 0;
    private int maxErrors;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupDifficulty();
    }

    void SetupDifficulty()
    {
        currentErrors = 0;
        foreach (var box in healthBoxes) box.SetActive(true);
        switch (difficulty)
        {
            case GameDifficulty.Easy: maxErrors = 5; break;
            case GameDifficulty.Medium: maxErrors = 4; break;
            case GameDifficulty.Hard: maxErrors = 3; break;
        }

        for (int i = 0; i < healthBoxes.Length; i++)
        {
            if (i >= maxErrors)
            {
                healthBoxes[i].SetActive(false);
            }
            else
            {
                healthBoxes[i].GetComponent<Image>().color = activeColor;
            }
        }
    }

    public bool AddError() // retorna true se jogador foi derrotado
    {
        if (currentErrors < maxErrors)
        {
            if (currentErrors < healthBoxes.Length)
            {
                healthBoxes[currentErrors].GetComponent<Image>().color = brokenColor;
            }
            currentErrors++;
        }
        if (currentErrors >= maxErrors) return true;
        return false;
    }

    public void ResetHealth()
    {
        currentErrors = 0;
        foreach (var box in healthBoxes)
        {
            if (box != null)
            {
                box.SetActive(true);
                var img = box.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = activeColor;
            }
        }
        SetupDifficulty();
    }
}