using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public ShapeStorage shapeStorage;
    public int columns = 0;
    public int rows = 0;
    public float squaresGap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startPosition = new Vector2(0, 0);
    public float squareScale = 0.5f;
    public float everySquareOffset = 0.0f;

    private Vector2 offset = new Vector2(0, 0);
    private List<GameObject> gridSquares = new List<GameObject>();

    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
    }



    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
    }


    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
    }



    private void SpawnGridSquares()
    {
        // 0, 1, 2, 3, 4,
        // 5, 6, 7, 8, 9

        int squareIndex = 0;

        for(var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                gridSquares.Add(Instantiate(gridSquare) as GameObject);

                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().SquareIndex = squareIndex;
                gridSquares[gridSquares.Count - 1].transform.SetParent(this.transform);
                gridSquares[gridSquares.Count - 1].transform.localScale = squareScale * Vector3.one;
                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().SetImage(squareIndex % 2 == 0);
                squareIndex++;                
            }
        }
    }

    private void SetGridSquaresPositions()
    {
        int columnNo = 0;
        int rowNo = 0;
        Vector2 squareGapNo = Vector2.zero;
        bool rowMoved = false;

        var squareRect = gridSquares[0].GetComponent<RectTransform>();

        offset.x = squareRect.rect.width * squareRect.transform.localScale.x + everySquareOffset;
        offset.y = squareRect.rect.height * squareRect.transform.localScale.y + everySquareOffset;

        foreach (GameObject square in gridSquares)
        {
            if (columnNo + 1 > columns)
            {
                squareGapNo.x = 0;
                // go to the next column
                columnNo = 0;
                rowNo++;
                rowMoved = false;
            }

            var posXOffset = offset.x * columnNo + (squareGapNo.x * squaresGap);
            var posYOffset = offset.y * rowNo + (squareGapNo.y * squaresGap);

            if (columnNo > 0 && columnNo % 3 == 0)
            {
                squareGapNo.x++;
                posXOffset += squaresGap;
            }

            if (rowNo > 0 && rowNo % 3 == 0 && rowMoved == false) 
            {
                rowMoved = true;
                squareGapNo.y++;
                posYOffset += squaresGap;
            }

            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + posXOffset, startPosition.y - posYOffset);
            
            square.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x + posXOffset, startPosition.y - posYOffset, 0.0f);

            columnNo++;
        }
    }

    private void CheckIfShapeCanBePlaced()
    {
        var squareIndexes = new List<int>();

        foreach (var square in gridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if (gridSquare.Selected && !gridSquare.SquareOccupied)
            {
                squareIndexes.Add(gridSquare.SquareIndex);
                gridSquare.Selected = false;
                //gridSquare.ActivateSquare();
            }
        }

        var currentSelectedShape = shapeStorage.GetCurrentSelectedShape();
        if (currentSelectedShape == null) return; // there is no selected shape.

        if (currentSelectedShape.TotalSquareNumber == squareIndexes.Count)
        {
            foreach (var squareIndex in squareIndexes)
            {
                gridSquares[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard();
            }

            var shapeLeft = 0;

            foreach (var shape in shapeStorage.shapeList)
            {
                if (shape.IsOnStartPosition() && shape.IsAnyOfShapeSquareActive())
                {
                    shapeLeft++;
                }
            }

            if(shapeLeft == 0)
            {
                GameEvents.RequestNewShapes();
            }
            else
            {
                GameEvents.SetShapeInactive();
            }

        }
        else
        {
            GameEvents.MoveShapeToStartPosition();
        }
    }

}
