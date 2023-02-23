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
    public SquareTextureData squareTextureData;

    private Vector2 offset = new Vector2(0, 0);
    private List<GameObject> gridSquares = new List<GameObject>();

    private LineIndicator lineIndicator;

    private Config.SquareColor currentActiveSquareColor = Config.SquareColor.NotSet;
    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
        GameEvents.UpdateSquareColor += OnUpdateSquareColor;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
        GameEvents.UpdateSquareColor -= OnUpdateSquareColor;
    }



    // Start is called before the first frame update
    void Start()
    {
        lineIndicator = GetComponent<LineIndicator>();
        CreateGrid();
        currentActiveSquareColor = squareTextureData.activeSquareTextures[0].squareColor;
    }

    private void OnUpdateSquareColor(Config.SquareColor color)
    {
        currentActiveSquareColor = color;
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
                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().SetImage(lineIndicator.GetGridSquareIndex(squareIndex) % 2 == 0);
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
                gridSquares[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard(currentActiveSquareColor);
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

            CheckIfAnyLineIsCompleted();
        }
        else
        {
            GameEvents.MoveShapeToStartPosition();
        }
    }


    //Adds Lines and squares to the list to check the all line grids are occupied to deactive them.  
    private void CheckIfAnyLineIsCompleted()
    {
        List<int[]> lines = new List<int[]>();

        //columns
        foreach (var column in lineIndicator.columnIndexes)
        {
            lines.Add(lineIndicator.GetVerticalLine(column));
        }

        //rows
        for (int row = 0; row < 9; row++)
        {
            List<int> data = new List<int>(9);

            for (int index = 0; index < 9; index++)
            {
                data.Add(lineIndicator.lineData[row, index]);
            }

            lines.Add(data.ToArray());
        }

        //squares
        for (int square = 0; square < 9; square++)
        {
            List<int> data = new List<int>(9);
            for (int index = 0; index < 9; index++)
            {
                data.Add(lineIndicator.squareData[square, index]);
            }
            lines.Add(data.ToArray());
        }

        var completedLines = CheckIfSquaresAreCompleted(lines);

        if (completedLines > 2)
        {
            //TODO: Play bonus anim..
        }

        var totalScores = 10 * completedLines;
        GameEvents.AddScores(totalScores);
        CheckIfPlayerLost();
    }

    private int CheckIfSquaresAreCompleted(List<int[]> data)
    {
        List<int[]> completedLines = new List<int[]>();

        var linesCompleted = 0;

        foreach (var line in data)
        {
            var lineCompleted = true;
            foreach (var squareIndex in line)
            {
                var comp = gridSquares[squareIndex].GetComponent<GridSquare>();
                if (comp.SquareOccupied == false)
                {
                    lineCompleted = false;
                }
            }

            if (lineCompleted)
            {
                completedLines.Add(line);
            }
        }

        foreach (var line in completedLines)
        {
            var completed = false;

            foreach (var squareIndex in line)
            {
                var comp = gridSquares[squareIndex].GetComponent<GridSquare>();
                comp.Deactivate();
                completed = true;
            }

            foreach (var squareIndex in line)
            {
                var comp = gridSquares[squareIndex].GetComponent<GridSquare>();
                comp.ClearOccupied();
            }

            if (completed)
            {
                linesCompleted++;
            }
        }

        return linesCompleted; 
    }

    private void CheckIfPlayerLost()
    {
        var validShapes = 0;

        for (int index = 0; index < shapeStorage.shapeList.Count; index++)
        {
            var isShapeActive = shapeStorage.shapeList[index].IsAnyOfShapeSquareActive();

            if (CheckIfShapeCanBePlacedOnGrid(shapeStorage.shapeList[index]) && isShapeActive)
            {
                shapeStorage.shapeList[index]?.ActivateShape();
                validShapes++;
            }
        }

        if (validShapes == 0)
        {
            //GameOver
            GameEvents.GameOver(false);
            //Debug.Log("Game Over");
        }
    }

    private bool CheckIfShapeCanBePlacedOnGrid(Shape currentShape)
    {
        var currentShapeData = currentShape.CurrentShapeData;
        var shapeColumns = currentShapeData.columns;
        var shapeRows = currentShapeData.rows;

        //All indexes of filled up squares.
        List<int> originalShapeFilledUpSquares = new List<int>();
        var squareIndex = 0;

        for (int rowIndex = 0; rowIndex < shapeRows; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < shapeColumns; columnIndex++)
            {
                if (currentShapeData.board[rowIndex].column[columnIndex])
                {
                    originalShapeFilledUpSquares.Add(squareIndex);
                }
                squareIndex++;
            }
        }

        if (currentShape.TotalSquareNumber != originalShapeFilledUpSquares.Count)
            Debug.LogError("Number of filled up squares are not the same as the original shape have.");

        var squareList = GetAllSquaresCombination(shapeColumns, shapeRows);

        bool canBePlaced = false;

        foreach (var number in squareList)
        {
            bool shapeCanBePlacedOnTheBoard = true;
            foreach (var squareIndexToCheck in originalShapeFilledUpSquares)
            {
                var comp = gridSquares[number[squareIndexToCheck]].GetComponent<GridSquare>();

                if (comp.SquareOccupied)
                {
                    shapeCanBePlacedOnTheBoard = false;
                }
            }

            if (shapeCanBePlacedOnTheBoard)
            {
                canBePlaced = true;
            }
        }

        return canBePlaced;
    }

    private List<int[]> GetAllSquaresCombination(int columns, int rows)
    {
        var squareList = new List<int[]>();
        var lastColumnIndex = 0;
        var lastRowIndex = 0;

        int safeIndex = 0;

        while(lastRowIndex + (rows - 1) < 9)
        {
            var rowData = new List<int>();

            for (int row = lastRowIndex; row < lastRowIndex + rows; row++)
            {
                for (int column = lastColumnIndex; column < lastColumnIndex + columns; column++)
                {
                    rowData.Add(lineIndicator.lineData[row, column]);
                }
            }

            squareList.Add(rowData.ToArray());

            lastColumnIndex++;

            if (lastColumnIndex + (columns - 1) >= 9)
            {
                lastRowIndex++;
                lastColumnIndex = 0;
            }

            safeIndex++;
            if (safeIndex > 100)
            {
                break;
            }

        }
        return squareList;
    }
}
