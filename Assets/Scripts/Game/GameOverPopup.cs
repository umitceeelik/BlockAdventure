using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPopup : MonoBehaviour
{
    public GameObject gameOverPopUp;
    public GameObject loosePopup;
    public GameObject newBestScorePopUp;


    void Start()
    {
        gameOverPopUp.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.GameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.GameOver -= OnGameOver;
    }

    private void OnGameOver(bool newBestScore)
    {
        gameOverPopUp.SetActive(true);
        loosePopup.SetActive(false);
        newBestScorePopUp.SetActive(true);
    }
}
