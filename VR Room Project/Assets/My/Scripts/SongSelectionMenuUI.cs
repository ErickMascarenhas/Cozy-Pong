using UnityEngine;
using TMPro;

public class SongSelectionMenuUI : MonoBehaviour
{
    [Tooltip("ID no ServeManager dessa musica")]
    [HideInInspector] public string songID;
    public TextMeshProUGUI maxScoreText;
    public TextMeshProUGUI maxGradeText;

    private void Awake()
    {
        songID = gameObject.name;
    }

    private void OnEnable()
    {
        LoadSongStats();
    }

    public void LoadSongStats()
    {
        // Ver ScoreResultUI: nenhum recorde e exibido durante o experimento.
        if (ExperimentMode.IsActive)
        {
            if (maxScoreText) maxScoreText.text = "";
            if (maxGradeText) maxGradeText.text = "";
            return;
        }

        int hasPlayed = PlayerPrefs.GetInt(songID + "_Played", 0);

        if (hasPlayed == 0)
        {
            if (maxScoreText) maxScoreText.text = "";
            if (maxGradeText) maxGradeText.text = "";
        }
        else
        {
            int score = PlayerPrefs.GetInt(songID + "_Score", -1);

            if (score == -1)
            {
                if (maxScoreText) maxScoreText.text = "...";
                if (maxGradeText) maxGradeText.text = "";
            }
            else
            {
                if (maxScoreText) maxScoreText.text = score.ToString("N0");
                if (maxGradeText)
                {
                    maxGradeText.text = PlayerPrefs.GetString(songID + "_Grade", "");
                    string savedColorHex = PlayerPrefs.GetString(songID + "_GradeColor", "#FFFFFF");
                    if (ColorUtility.TryParseHtmlString(savedColorHex, out Color gradeColor)) maxGradeText.color = gradeColor;
                }
            }
        }
    }
}