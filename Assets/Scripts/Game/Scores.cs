using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class BestScoreData
{
    public int score = 0;
}

public class Scores : MonoBehaviour
{
    public SquareTextureData squareTextureData;
    public TextMeshProUGUI scoreText;

    private bool newBestScore = false;
    private BestScoreData bestScore = new BestScoreData();
    private int currentScores;

    private string bestScoreKey = "bsdat";

    private void Awake()
    {
        if (BinaryDataStream.Exist(bestScoreKey))
        {
            StartCoroutine(ReadDataFile());
        }
    }

    private IEnumerator ReadDataFile()
    {
        bestScore = BinaryDataStream.Read<BestScoreData>(bestScoreKey);
        yield return new WaitForEndOfFrame();
        GameEvents.UpdateBestScoreBar(currentScores, bestScore.score);
    }

    void Start()
    {
        currentScores = 0;
        newBestScore = false;
        squareTextureData.SetStartColor();
        UpdateScoreText();
    }

    private void OnEnable()
    {
        GameEvents.AddScores += AddScores;
        GameEvents.GameOver += SaveBestScores;
    }


    private void OnDisable()
    {
        GameEvents.AddScores -= AddScores;
        GameEvents.GameOver -= SaveBestScores;
    }

    public void SaveBestScores(bool newBestScore)
    {
        BinaryDataStream.Save<BestScoreData>(bestScore, bestScoreKey);
    }

    private void AddScores(int scores)
    {
        currentScores += scores;
        if (currentScores > bestScore.score)
        {
            newBestScore = true;
            bestScore.score = currentScores;
            SaveBestScores(true);
        }
        UpdateSquareColor();
        GameEvents.UpdateBestScoreBar(currentScores, bestScore.score);
        UpdateScoreText();
    }

    private void UpdateSquareColor()
    {
        if (GameEvents.UpdateSquareColor != null && currentScores >= squareTextureData.tresholdVal)
        {
            squareTextureData.UpdateColors(currentScores);
            GameEvents.UpdateSquareColor(squareTextureData.currentColor);
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = currentScores.ToString();
    }
}
