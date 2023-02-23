using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSquareImageSelector : MonoBehaviour
{
    public SquareTextureData squareTextureData;
    public bool updateImageOnReachedTreashold = false;

    private void OnEnable()
    {
        UpdateSquareColorBaseOnCurrentPoints();

        if (updateImageOnReachedTreashold)
            GameEvents.UpdateSquareColor += UpdateSquareColor;
    }

    private void OnDisable()
    {
        if(updateImageOnReachedTreashold)
            GameEvents.UpdateSquareColor -= UpdateSquareColor;
    }


    private void UpdateSquareColorBaseOnCurrentPoints()
    {
        foreach (var squareTexture in squareTextureData.activeSquareTextures)
        {
            if (squareTextureData.currentColor == squareTexture.squareColor)
            {
                GetComponent<Image>().sprite = squareTexture.texture;
            }
        }
    }

    private void UpdateSquareColor(Config.SquareColor color)
    {
        foreach (var squareTexture in squareTextureData.activeSquareTextures)
        {
            if (color == squareTexture.squareColor)
            {
                GetComponent<Image>().sprite = squareTexture.texture;
            }
        }
    }

}
