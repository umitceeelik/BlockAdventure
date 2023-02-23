using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    public Image normalImage;
    public Image hoverImage;
    public Image activeImage;
    public List<Sprite> normalImages;

    private Config.SquareColor currentSquareColor = Config.SquareColor.NotSet;

    public Config.SquareColor GetCurrentColor()
    {
        return currentSquareColor;
    }

    public bool Selected { get; set; }
    public int SquareIndex { get; set; }
    public bool SquareOccupied { get; set; }

    void Start()
    {
        Selected = false;
        SquareOccupied = false;
    }

    // Temp func. Remove it
    public bool CanWeUseThisSquare()
    {
        return hoverImage.gameObject.activeSelf;
    }

    public void PlaceShapeOnBoard(Config.SquareColor color)
    {
        currentSquareColor = color;
        ActivateSquare();
    }

    public void ActivateSquare()
    {
        hoverImage.gameObject.SetActive(false);
        activeImage.gameObject.SetActive(true);
        Selected = true;
        SquareOccupied = true;
    }

    public void Deactivate()
    {
        currentSquareColor = Config.SquareColor.NotSet;
        activeImage.gameObject.SetActive(false);
    }
    public void ClearOccupied()
    {
        currentSquareColor = Config.SquareColor.NotSet;
        Selected = false;
        SquareOccupied = false;
    }

    public void SetImage(bool setFirstImage)
    {
        normalImage.GetComponent<Image>().sprite = setFirstImage ? normalImages[1] : normalImages[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (SquareOccupied == false)
        {
            Selected = true;
            hoverImage.gameObject.SetActive(true);
        }
        else if(collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        Selected = true;

        if (SquareOccupied == false)
        {
            hoverImage.gameObject.SetActive(true);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (SquareOccupied == false)
        {
            Selected = false;
            hoverImage.gameObject.SetActive(false);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().UnSetOccupied();
        }
    }

}
