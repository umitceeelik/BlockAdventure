using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class SquareTextureData : ScriptableObject
{

    [System.Serializable]
    public class TextureData
    {
        public Sprite texture;
        public Config.SquareColor squareColor;
    }

    public int tresholdVal = 10;
    private const int StartTresholdVal = 100;
    public List<TextureData> activeSquareTextures;

    public Config.SquareColor currentColor;
    private Config.SquareColor nextColor;

    public int GetCurrentColorIndex()
    {
        var currentIndex = 0;

        for (int index = 0; index < activeSquareTextures.Count; index++)
        {
            if (activeSquareTextures[index].squareColor == currentColor)
            {
                currentIndex = index;
            }
        }

        return currentIndex;
    }

    public void UpdateColors(int currentScore)
    {
        currentColor = nextColor;
        var currentColorIndex = GetCurrentColorIndex();

        if (currentColorIndex == activeSquareTextures.Count - 1)
            nextColor = activeSquareTextures[0].squareColor;
        else
            nextColor = activeSquareTextures[currentColorIndex + 1].squareColor;

        tresholdVal = StartTresholdVal + currentScore;
    }

    public void SetStartColor()
    {
        tresholdVal = StartTresholdVal;
        currentColor = activeSquareTextures[0].squareColor;
        nextColor = activeSquareTextures[1].squareColor;
    }

    private void Awake()
    {
        SetStartColor();
    }

    private void OnEnable()
    {
        SetStartColor();
    }
}
