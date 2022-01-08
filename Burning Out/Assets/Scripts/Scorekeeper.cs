using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scorekeeper : MonoBehaviour
{
    private bool isActive = false;

    [SerializeField] TMP_Text timerText;
    [SerializeField] float baseTimeBonus = 5000f;
    [SerializeField] float timeFactor = 100f;
    private float timeSpent = 0f;
    private float baseTimeDenominator = 1f;
    private int finalTimeScore = -1;

    [SerializeField] float baseFuseBonus = 10000f;
    private int finalFuseScore = -1;

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            timeSpent += Time.deltaTime;
            timerText.text = $"{timeSpent:N1}";
        }
    }

    public void ResetScorekeeper()
    {
        isActive = true;
        timeSpent = 0f;
        finalTimeScore = -1;
        finalFuseScore = -1;
    }

    public void StopScorekeeper()
    {
        isActive = false;
    }

    public void CalculateFinalScore(int fuseRemaining, int maximumFuse)
    {
        float timeRatio = timeFactor / (baseTimeDenominator + timeSpent);
        finalTimeScore = (int)(baseTimeBonus * timeRatio);

        float fuseRatio = (float)fuseRemaining / (float)maximumFuse;
        finalFuseScore = (int)(baseFuseBonus * (1 - fuseRatio));
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
}
