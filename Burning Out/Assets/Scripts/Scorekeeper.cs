using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scorekeeper : MonoBehaviour
{
    private bool isActive = false;

    private int hiScoreTotal;
    private float hiScoreTime;
    private int hiScoreFuse;
    [SerializeField] TMP_Text highScoreText;

    [SerializeField] TMP_Text timerText;
    [SerializeField] float baseTimeBonus = 5000f;
    [SerializeField] float timeFactor = 100f;
    private float timeSpent = 0f;
    private float baseTimeDenominator = 1f;
    private int finalTimeScore = -1;

    private int savedFuseLeft = -1;

    [SerializeField] float baseFuseBonus = 10000f;
    private int finalFuseScore = -1;

    [SerializeField] GameObject resultsPanel;
    [SerializeField] TMP_Text timeResultText;
    [SerializeField] TMP_Text fuseResultText;
    [SerializeField] TMP_Text totalResultText;

    void Awake()
    {
        if (PlayerPrefs.GetInt("hiScoreTotal", -1) != -1)
        {
            hiScoreTotal = PlayerPrefs.GetInt("hiScoreTotal");
        }
        else
        {
            hiScoreTotal = 0;
            PlayerPrefs.SetInt("hiScoreTotal", hiScoreTotal);
        }

        if (PlayerPrefs.GetFloat("hiScoreTime", -1f) != -1f)
        {
            hiScoreTime = PlayerPrefs.GetFloat("hiScoreTime");
        }
        else
        {
            hiScoreTime = 999f;
            PlayerPrefs.SetFloat("hiScoreTime", hiScoreTime);
        }

        if (PlayerPrefs.GetInt("hiScoreFuse", -1) != -1)
        {
            hiScoreFuse = PlayerPrefs.GetInt("hiScoreFuse");
        }
        else
        {
            hiScoreFuse = 999;
            PlayerPrefs.SetInt("hiScoreFuse", hiScoreFuse);
        }

        highScoreText.text = $"HIGH SCORE: {hiScoreTotal} pts ({hiScoreTime:N1} sec) ({hiScoreFuse} fuse)";
    }

    void Update()
    {
        if (isActive)
        {
            timeSpent += Time.deltaTime;
            timerText.text = $"{timeSpent:N1}";
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            hiScoreTotal = 0;
            PlayerPrefs.SetInt("hiScoreTotal", hiScoreTotal);
            hiScoreTime = 999f;
            PlayerPrefs.SetFloat("hiScoreTime", hiScoreTime);
            hiScoreFuse = 999;
            PlayerPrefs.SetInt("hiScoreFuse", hiScoreFuse);
            highScoreText.text = $"HIGH SCORE: {hiScoreTotal} pts ({hiScoreTime:N1} sec) ({hiScoreFuse} fuse)";
        }
    }

    public void ResetScorekeeper()
    {
        timeSpent = 0f;
        timerText.text = $"{timeSpent:N1}";
        finalTimeScore = -1;
        finalFuseScore = -1;
    }

    public void StartScorekeeper()
    {
        isActive = true;
    }

    public void StopScorekeeper()
    {
        isActive = false;
    }

    public void ShowResultsScreen()
    {
        timeResultText.text = $"TIME BONUS ({timeSpent:N1} sec):\n{finalTimeScore} pts";
        fuseResultText.text = $"FUSE BONUS ({savedFuseLeft} left):\n{finalFuseScore} pts";
        totalResultText.text = $"TOTAL SCORE:\n{finalFuseScore + finalTimeScore} pts";
        resultsPanel.SetActive(true);
    }

    public void HideResultsScreen()
    {
        resultsPanel.SetActive(false);
    }

    public void CalculateFinalScore(int fuseRemaining, int maximumFuse)
    {
        float timeRatio = timeFactor / (baseTimeDenominator + timeSpent);
        finalTimeScore = (int)(baseTimeBonus * timeRatio);

        float fuseRatio = (float)fuseRemaining / (float)maximumFuse;
        finalFuseScore = (int)(baseFuseBonus * (1 - fuseRatio));

        savedFuseLeft = fuseRemaining;

        if (((finalTimeScore + finalFuseScore) > hiScoreTotal) || (timeSpent < hiScoreTime) || (savedFuseLeft < hiScoreFuse))
        {
            hiScoreTotal = (finalTimeScore + finalFuseScore);
            hiScoreTime = timeSpent;
            hiScoreFuse = savedFuseLeft;

            PlayerPrefs.SetInt("hiScoreTotal", hiScoreTotal);
            PlayerPrefs.SetFloat("hiScoreTime", hiScoreTime);
            PlayerPrefs.SetInt("hiScoreFuse", hiScoreFuse);
            highScoreText.text = $"HIGH SCORE: {hiScoreTotal} pts ({hiScoreTime:N1} sec) ({hiScoreFuse} fuse)";
        }
    }

    public int getTimeScore()
    {
        return finalTimeScore;
    }

    public int getFuseScore()
    {
        return finalFuseScore;
    }

    public int getTotalScore()
    {
        return finalTimeScore + finalFuseScore;
    }

    public bool getIsActive()
    {
        return isActive;
    }
}
