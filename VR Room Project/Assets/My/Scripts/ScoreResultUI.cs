using UnityEngine;
using TMPro;

public class ScoreResultUI : MonoBehaviour
{
    [Header("Estatisticas")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI okCountText;
    public TextMeshProUGUI badCountText;
    public TextMeshProUGUI missCountText;
    public TextMeshProUGUI maxComboText;

    [Header("Nota")]
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI percentageText;

    [Header("Cores")]
    public Color sGoldColor = new Color(1f, 0.84f, 0f);
    public Color sPinkColor = new Color(1f, 0.41f, 0.71f);
    public Color sSilverColor = new Color(0.75f, 0.75f, 0.75f);

    private void OnEnable()
    {
        DisplayResults();
    }

    public void DisplayResults()
    {
        GameScoreManager mgr = GameScoreManager.Instance;
        if (mgr == null) return;
        if (perfectCountText) perfectCountText.text = mgr.hitCounts[HitType.Perfect].ToString();
        if (okCountText) okCountText.text = mgr.hitCounts[HitType.Ok].ToString();
        if (badCountText) badCountText.text = mgr.hitCounts[HitType.Bad].ToString();
        if (missCountText) missCountText.text = mgr.hitCounts[HitType.Miss].ToString();
        if (maxComboText) maxComboText.text = mgr.maxCombo.ToString();
        if (totalScoreText) totalScoreText.text = mgr.totalScore.ToString("N0");
        int totalNotes = mgr.totalHits + mgr.totalMisses;
        float percentage = 0f;
        if (totalNotes > 0)
        {
            float maxPossibleScore = CalculateMaxPossibleScore(totalNotes);
            percentage = (mgr.totalScore / maxPossibleScore) * 100f;
            percentage = Mathf.Clamp(percentage, 0f, 100f);
        }
        if (percentageText) percentageText.text = $"{percentage:0.00}%";
        CalculateAndDisplayGrade(percentage, mgr.totalMisses == 0);
    }

    private void CalculateAndDisplayGrade(float percentage, bool isFullCombo)
    {
        if (gradeText == null) return;
        string gradeStr = "";
        Color gradeCol = Color.white;
        if (percentage >= 100f)
        {
            gradeStr = "S";
            gradeCol = sGoldColor;
        }
        else if (percentage >= 92f)
        {
            gradeStr = "S";
            gradeCol = sPinkColor;
        }
        else if (percentage >= 85f)
        {
            gradeStr = "S";
            gradeCol = sSilverColor;
        }
        else if (percentage >= 70f || isFullCombo)
        {
            gradeStr = "A";
            gradeCol = new Color(0.6f, 0f, 1f); // roxo
        }
        else if (percentage >= 50f)
        {
            gradeStr = "B";
            gradeCol = new Color(0f, 0.4f, 1f); // azul
        }
        else if (percentage >= 35f)
        {
            gradeStr = "C";
            gradeCol = new Color(0.1f, 1f, 0.1f); // verde
        }
        else
        {
            gradeStr = "D";
            gradeCol = new Color(0.5f, 0.5f, 0.5f); // cinza
        }
        gradeText.text = gradeStr;
        gradeText.color = gradeCol;

        // Recorde e um estado que sobrevive entre participantes. Ver o recorde
        // de quem jogou antes e pressao de desempenho, e o experimento mede
        // justamente isso: no modo experimento nada e lido nem gravado.
        if (ExperimentMode.UsesConditionConfig) return;

        GameScoreManager mgr = GameScoreManager.Instance;
        if (mgr != null && !string.IsNullOrEmpty(mgr.currentSongID))
        {
            PlayerPrefs.SetInt(mgr.currentSongID + "_Played", 1);
            int currentHighScore = PlayerPrefs.GetInt(mgr.currentSongID + "_Score", -1);
            if (mgr.totalScore > currentHighScore)
            {
                PlayerPrefs.SetInt(mgr.currentSongID + "_Score", mgr.totalScore);
                PlayerPrefs.SetString(mgr.currentSongID + "_Grade", gradeStr);
                PlayerPrefs.SetString(mgr.currentSongID + "_GradeColor", "#" + ColorUtility.ToHtmlStringRGBA(gradeCol));
                PlayerPrefs.Save();
            }
        }
    }

    private int CalculateMaxPossibleScore(int totalNotes)
    {
        int maxScore = 0;
        int simulatedCombo = 0;
        int simulatedMultiplier = 1;
        for (int i = 0; i < totalNotes; i++)
        {
            simulatedCombo++;
            if (simulatedCombo >= 14) simulatedMultiplier = 8;
            else if (simulatedCombo >= 6) simulatedMultiplier = 4;
            else if (simulatedCombo >= 2) simulatedMultiplier = 2;
            else simulatedMultiplier = 1;
            maxScore += 200 * simulatedMultiplier; // 200 = pontuacao do perfect
        }
        return maxScore;
    }
}