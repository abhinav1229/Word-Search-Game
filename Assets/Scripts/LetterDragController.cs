using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterDragController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _drawLine;
    [SerializeField] private GameObject _matchedDrawLine;
    [SerializeField] private GameObject _selecedWordHint;


    private GameObject _lastDraggedLetter;
    private RectTransform _drawLineRect;
    private int _lastDirection = 0;
    private Vector2Int _cellSize;
    private float _cellsGapInX = 10;
    private float _cellsGapInY = 5;
    private Vector2 _drawLineDefaultSize = new(80, 80);
    private string _firstLetterName;

    private Color32 _defaultColor = new Color32(50, 50, 50, 255);
    private Color32 _selectedColor = new Color32(255, 255, 255, 255);

    private List<GameObject> _selectedLetters = new();

    public static LetterDragController Instance;

    [SerializeField] private AudioClip _selectedWordClip, _wrongMatchClip, _completedWordClip;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _drawLineRect = _drawLine.GetComponent<RectTransform>();
        SetCellSize();
    }

    public void SetCellSize()
    {
        _cellSize = GameData.Instance.GetLevelBoardCellSize(GameData.UnlockedLevel);

        if (GameData.UnlockedLevel > 10)
        {
            _cellsGapInY = 0;
        }
    }

    public void ClearAllDrawLines()
    {
        foreach (Transform drawLine in _matchedDrawLine.transform)
        {
            Destroy(drawLine.gameObject);
        }
    }

    private float GetDiagonalLength()
    {
        if (GameData.UnlockedLevel <= 10)
        {
            return 285f;
        }
        else if (GameData.UnlockedLevel <= 25)
        {
            return 180f;
        }
        else if (GameData.UnlockedLevel <= 50)
        {
            return 150f;
        }

        return 180f;
    }

    private float GetDiagonalAngle(int direction)
    {
        float angle = 0;
        if (GameData.UnlockedLevel <= 10)
        {
            if (direction == 5)
            {
                angle = -39f;
            }
            else if (direction == 6)
            {
                angle = -141.5f;
            }
            else if (direction == 7)
            {
                angle = 39.5f;
            }
            else if (direction == 8)
            {
                angle = -219.5f;
            }
        }
        else if (GameData.UnlockedLevel <= 25)
        {
            if (direction == 5)
            {
                angle = -37f;
            }
            else if (direction == 6)
            {
                angle = -143f;
            }
            else if (direction == 7)
            {
                angle = 37f;
            }
            else if (direction == 8)
            {
                angle = 142f;
            }
        }
        else if (GameData.UnlockedLevel <= 50)
        {
            if (direction == 5)
            {
                angle = -41.5f;
            }
            else if (direction == 6)
            {
                angle = 221.5f;
            }
            else if (direction == 7)
            {
                angle = 41.5f;
            }
            else if (direction == 8)
            {
                angle = 138f;
            }
        }

        return angle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        GameObject currentDraggedLetter = eventData.pointerCurrentRaycast.gameObject;
        if (currentDraggedLetter == null || _lastDraggedLetter == null || !currentDraggedLetter.name.Contains("BoardLetter") || _lastDraggedLetter.name.Equals(currentDraggedLetter.name))
        {
            return;
        }

        Vector2 drawLineSizeRect = _drawLineRect.sizeDelta;
        int direction = GetDirection(_lastDraggedLetter.name, currentDraggedLetter.name);
        Debug.Log("--------------------");
        Debug.Log("direction: " + direction);
        if (direction == -1)
        {
            return;
        }

        int directionChangeMultiplier = 1;

        if (IsTakenNonLinearWildMove(currentDraggedLetter.name) && direction != 5 && direction != 6 && direction != 7 && direction != 8)
        {
            Debug.Log("Dragging IsTakenNonLinearWildMove " + direction);
            return;
        }

        int distanceMultiplier = 1;

        float diagonalLength = GetDiagonalLength();

        Debug.Log($"Passed: ({_lastDirection},{direction}) | {HaveCrossedToTheStartLetter(currentDraggedLetter.name, direction)}");

        if (_lastDirection != direction)
        {
            if (HaveCrossedToTheStartLetter(currentDraggedLetter.name, direction) || _lastDirection == 0)
            {
                Vector3 dragLineAnchors = _drawLineRect.anchoredPosition;
                distanceMultiplier = GetDistanceBetweenCurrentAndFirst(currentDraggedLetter.name);

                if (direction == 1)
                {
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.x -= _lastDirection == 0 ? 40 : 80;


                    if (_lastDirection == 3 || _lastDirection == 4)
                    {
                        _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0);
                        dragLineAnchors.y += _lastDirection == 3 ? -40 : 40;
                        dragLineAnchors.x += 40;
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 2)
                    {
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 5)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x += 65;
                        dragLineAnchors.y -= 15;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 6)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x += 3;
                        dragLineAnchors.y -= 20;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 7)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x += 70;
                        dragLineAnchors.y += 25;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 8)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x += 10;
                        dragLineAnchors.y += 30;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);
                }
                else if (direction == 2)
                {
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                    dragLineAnchors.x += _lastDirection == 0 ? 40 : 80;


                    if (_lastDirection == 3 || _lastDirection == 4)
                    {
                        _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 180);
                        dragLineAnchors.y += _lastDirection == 3 ? -40 : 40;
                        dragLineAnchors.x -= 40;
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 1)
                    {
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 5)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x -= 15;
                        dragLineAnchors.y -= 15;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 6)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x -= 77;
                        dragLineAnchors.y -= 20;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 7)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.y += 25;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 8)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0f);
                        dragLineAnchors.x -= 70;
                        dragLineAnchors.y += 30;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);

                }
                else if (direction == 3)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, 90);
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                    dragLineAnchors.y += _lastDirection == 0 ? 40 : 80;

                    if (_lastDirection == 1 || _lastDirection == 2)
                    {
                        dragLineAnchors.y -= 40;
                        dragLineAnchors.x += _lastDirection == 1 ? 40 : -40;
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 4)
                    {
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 5)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 90f);
                        dragLineAnchors.x += 25;
                        dragLineAnchors.y -= 55;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 6)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, -270f);
                        dragLineAnchors.x -= 37;
                        dragLineAnchors.y -= 60;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 7)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, -270f);
                        dragLineAnchors.x += 30;
                        dragLineAnchors.y -= 25;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 8)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, -270f);
                        dragLineAnchors.x -= 30;
                        dragLineAnchors.y -= 5;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);

                }
                else if (direction == 4)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, 90);
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.y -= _lastDirection == 0 ? 40 : 80;

                    if (_lastDirection == 1 || _lastDirection == 2)
                    {
                        dragLineAnchors.y += 40;
                        dragLineAnchors.x += _lastDirection == 1 ? 40 : -40;
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 3)
                    {
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 5)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 90f);
                        dragLineAnchors.x += 25;
                        dragLineAnchors.y += 25;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 6)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, -270f);
                        dragLineAnchors.x -= 37;
                        dragLineAnchors.y += 20;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 7)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 90f);
                        dragLineAnchors.x += 30;
                        dragLineAnchors.y += 60;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 8)
                    {
                        _drawLine.transform.eulerAngles = new Vector3(0, 0, 90f);
                        dragLineAnchors.x -= 30;
                        dragLineAnchors.y += 70;
                        drawLineSizeRect.x -= diagonalLength * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);
                }
                else if (direction == 5)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, GetDiagonalAngle(direction));
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.x -= 25;
                    dragLineAnchors.y += 15;

                    if (_lastDirection == 1 || _lastDirection == 2)
                    {
                        dragLineAnchors.y += _lastDirection == 1 ? 0 : 0;
                        dragLineAnchors.x += _lastDirection == 1 ? 40 : -40;
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    if (_lastDirection == 3 || _lastDirection == 4)
                    {
                        dragLineAnchors.y += _lastDirection == 3 ? -40 : 40;
                        dragLineAnchors.x += _lastDirection == 3 ? 0 : 0;
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);
                }
                else if (direction == 6)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, GetDiagonalAngle(direction));
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.x += 37;
                    dragLineAnchors.y += 20;

                    if (_lastDirection == 1 || _lastDirection == 2)
                    {
                        dragLineAnchors.x += _lastDirection == 2 ? -40 : 40;
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    if (_lastDirection == 3 || _lastDirection == 4)
                    {
                        dragLineAnchors.y += _lastDirection == 3 ? -40 : 40;
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }
                    SetLetterInDirection(direction, distanceMultiplier);
                }
                else if (direction == 7)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, GetDiagonalAngle(direction));
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.x -= 30;
                    dragLineAnchors.y -= 25;

                    if (_lastDirection == 1 || _lastDirection == 2)
                    {
                        dragLineAnchors.x += _lastDirection == 1 ? 40 : -50;
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    if (_lastDirection == 3 || _lastDirection == 4)
                    {
                        dragLineAnchors.y += _lastDirection == 4 ? 45 : -30;
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);
                }
                else if (direction == 8)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, GetDiagonalAngle(direction));
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.x += 30;
                    dragLineAnchors.y -= 30;

                    if (_lastDirection == 1 || _lastDirection == 2)
                    {
                        dragLineAnchors.x += _lastDirection == 2 ? -40 : 40;
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
                    }
                    if (_lastDirection == 3 || _lastDirection == 4)
                    {
                        dragLineAnchors.y += _lastDirection == 4 ? 40 : -45;
                        drawLineSizeRect.x -= (_cellSize.y + 0) * (_selectedLetters.Count - 1);
                    }

                    SetLetterInDirection(direction, distanceMultiplier);
                }
                _drawLineRect.anchoredPosition = dragLineAnchors;
                _lastDirection = direction;
            }
            else
            {

                directionChangeMultiplier = -1;

            }

            if (HaveReachedToTheStartLetter(currentDraggedLetter.name, _firstLetterName))
            {
                Debug.Log("HaveReachedToTheStartLetter");

                _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                _drawLine.transform.eulerAngles = new Vector3(0, 0, 0);
                _drawLineRect.anchoredPosition = currentDraggedLetter.GetComponent<RectTransform>().anchoredPosition;

                drawLineSizeRect.x = 80;
                drawLineSizeRect.y = 80;

                direction = -1;
                _lastDirection = 0;

                foreach (GameObject letter in _selectedLetters)
                {
                    if (letter.name.Contains("_MATCHED") || letter.name.Equals(currentDraggedLetter.name)) continue;
                    letter.GetComponent<Text>().color = _defaultColor;
                }
            }
        }
        else
        {
            distanceMultiplier = GetDistanceBetweenCurrentAndFirst(currentDraggedLetter.name);
            SetLetterInDirection(direction, distanceMultiplier);
            drawLineSizeRect.x = 80;
        }

        Debug.Log($"distanceMultiplier: {distanceMultiplier} | {directionChangeMultiplier}");

        if (direction == 1)
        {
            drawLineSizeRect.x += (_cellSize.x + _cellsGapInX) * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 2)
        {
            drawLineSizeRect.x += (_cellSize.x + _cellsGapInX) * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 3)
        {
            drawLineSizeRect.x += (_cellSize.y + 0) * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 4)
        {
            drawLineSizeRect.x += (_cellSize.y + 0) * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 5)
        {
            drawLineSizeRect.x += diagonalLength * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 6)
        {
            drawLineSizeRect.x += diagonalLength * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 7)
        {
            drawLineSizeRect.x += diagonalLength * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 8)
        {
            drawLineSizeRect.x += diagonalLength * directionChangeMultiplier * distanceMultiplier;
        }

        if (directionChangeMultiplier == 1)
        {
            _selectedLetters.Add(currentDraggedLetter);
            currentDraggedLetter.GetComponent<Text>().color = _selectedColor;
        }
        else
        {
            if (_selectedLetters.Count > 0)
            {
                _selectedLetters.RemoveAt(_selectedLetters.Count - 1);
                if (!_lastDraggedLetter.name.Contains("_MATCHED"))
                {
                    _lastDraggedLetter.GetComponent<Text>().color = _defaultColor;
                }
            }
        }

        if (GameData.UnlockedLevel <= 10)
        {
            drawLineSizeRect.y = 90;
        }

        _drawLineRect.sizeDelta = drawLineSizeRect;
        _lastDraggedLetter = currentDraggedLetter;

        SetHint(drawLineColor);

        StartCoroutine(AudioManager.Instance.PlaySound(_selectedWordClip));
    }

    public int GetDirection(string last, string current)
    {
        int last_row = int.Parse(last.Split('_')[1]);
        int last_col = int.Parse(last.Split('_')[2]);

        int curr_row = int.Parse(current.Split('_')[1]);
        int curr_col = int.Parse(current.Split('_')[2]);

        int first_row = int.Parse(_firstLetterName.Split('_')[1]);
        int first_col = int.Parse(_firstLetterName.Split('_')[2]);

        // Check vertical directions
        if (curr_col == first_col)
        {
            if (curr_row > first_row) return 3; // Down
            if (curr_row < first_row) return 4; // Up
        }

        // Check horizontal directions
        if (curr_row == first_row)
        {
            if (curr_col > first_col) return 1; // Right
            if (curr_col < first_col) return 2; // Left
        }

        int first_ap = int.Parse(first_row + "" + first_col);
        int current_ap = int.Parse(curr_row + "" + curr_col);

        // Check for down-right diagonal (Direction 5)
        if (current_ap >= first_ap && (current_ap - first_ap) % 11 == 0)
        {
            return 5;
        }

        // Check for down-left diagonal (Direction 6)
        if ((curr_row - first_row) == (first_col - curr_col) && curr_row > first_row && curr_col < first_col)
        {
            return 6;
        }

        // Check for up-right diagonal (Direction 7)
        if ((first_row - curr_row) == (curr_col - first_col) && curr_row < first_row && curr_col > first_col)
        {
            return 7;
        }

        // Check for up-left diagonal (Direction 8)
        if ((first_row - curr_row) == (first_col - curr_col) && curr_row < first_row && curr_col < first_col)
        {
            return 8;
        }

        return -1; // No direction matched
    }





    private void SetLetterInDirection(int direction, int distance)
    {
        // Reset colors of previous selections if not matched
        foreach (GameObject letter in _selectedLetters)
        {
            if (!letter.name.Contains("_MATCHED"))
            {
                letter.GetComponent<Text>().color = _defaultColor;
            }
        }
        _selectedLetters.Clear();

        int firstRow = int.Parse(_firstLetterName.Split('_')[1]);
        int firstCol = int.Parse(_firstLetterName.Split('_')[2]);

        // Set row and column offsets based on direction
        int rowOffset = 0, colOffset = 0;
        switch (direction)
        {
            case 1: colOffset = 1; break;    // Right
            case 2: colOffset = -1; break;   // Left
            case 3: rowOffset = 1; break;    // Down
            case 4: rowOffset = -1; break;   // Up
            case 5: rowOffset = 1; colOffset = 1; break;   // Down-right diagonal
            case 6: rowOffset = 1; colOffset = -1; break;  // Down-left diagonal
            case 7: rowOffset = -1; colOffset = 1; break;  // Up-right diagonal
            case 8: rowOffset = -1; colOffset = -1; break; // Up-left diagonal
        }

        // Loop through each distance step in the chosen direction
        for (int i = 0; i < distance; i++)
        {
            int currentRow = firstRow + rowOffset * i;
            int currentCol = firstCol + colOffset * i;

            // Attempt to find the GameObject in the grid; fallback to "_MATCHED" if not found
            GameObject go = transform.Find($"BoardLetter_{currentRow}_{currentCol}")?.gameObject
                            ?? transform.Find($"BoardLetter_{currentRow}_{currentCol}_MATCHED")?.gameObject;
            if (go != null)
            {
                go.GetComponent<Text>().color = _selectedColor;
                _selectedLetters.Add(go);
            }
        }
    }






    private bool IsTakenWildMove(int currDirection, int lastDirection)
    {
        if (_selectedLetters.Count <= 1) return false;

        if ((lastDirection == 3 || lastDirection == 4) && (currDirection == 1 || currDirection == 2))
        {
            return true;
        }
        else if ((lastDirection == 1 || lastDirection == 2) && (currDirection == 3 || currDirection == 4))
        {
            return true;
        }

        return false;
    }

    private bool IsTakenNonLinearWildMove(string currentObject)
    {
        int first_row = int.Parse(_firstLetterName.Split('_')[1]);
        int first_col = int.Parse(_firstLetterName.Split('_')[2]);

        int curr_row = int.Parse(currentObject.Split('_')[1]);
        int curr_col = int.Parse(currentObject.Split('_')[2]);

        if (curr_row != first_row && curr_col != first_col)
        {
            return true;
        }

        return false;
    }


    private bool IsTakenLinearMove(string currentObject)
    {
        int first_row = int.Parse(_firstLetterName.Split('_')[1]);
        int first_col = int.Parse(_firstLetterName.Split('_')[2]);

        int curr_row = int.Parse(currentObject.Split('_')[1]);
        int curr_col = int.Parse(currentObject.Split('_')[2]);

        if (curr_row == first_row || curr_col == first_col)
        {
            return true;
        }

        return false;
    }

    /* Using 'Manhattan distance' formula */
    private int GetDistanceBetweenCurrentAndFirst(string currentObject)
    {
        var firstParts = _firstLetterName.Split('_');

        // Fixed starting point (3, 0)
        int first_row = int.Parse(firstParts[1]);
        int first_col = int.Parse(firstParts[2]);

        // Extract current point from input
        int curr_row = int.Parse(currentObject.Split('_')[1]);
        int curr_col = int.Parse(currentObject.Split('_')[2]);

        // Calculate row and column differences
        int rowDifference = Math.Abs(curr_row - first_row);
        int colDifference = Math.Abs(curr_col - first_col);

        // Return the maximum of row or column difference, counting each diagonal or linear step
        return Math.Max(rowDifference, colDifference);
    }





    private bool IsTrickyCrossedStartLetter(string currentObject, int currDir)
    {
        if (_selectedLetters.Count <= 1) return false;

        int first_row = int.Parse(_firstLetterName.Split('_')[1]);
        int first_col = int.Parse(_firstLetterName.Split('_')[2]);

        int curr_row = int.Parse(currentObject.Split('_')[1]);
        int curr_col = int.Parse(currentObject.Split('_')[2]);

        if (_selectedLetters.Count <= 2)
        {
            if (_lastDirection == 1 && currDir == 2 && curr_col < first_col) return true;
            if (_lastDirection == 2 && currDir == 1 && curr_col > first_col) return true;

            if (_lastDirection == 3 && currDir == 4 && curr_row < first_row) return true;
            if (_lastDirection == 4 && currDir == 3 && curr_row > first_row) return true;
        }
        else
        {
            if (_lastDirection == 1 && currDir == 2 && curr_col <= first_col) return true;
            if (_lastDirection == 2 && currDir == 1 && curr_col >= first_col) return true;

            if (_lastDirection == 3 && currDir == 4 && curr_row <= first_row) return true;
            if (_lastDirection == 4 && currDir == 3 && curr_row >= first_row) return true;
        }

        return false;
    }

    private bool HaveCrossedToTheStartLetter(string draggingObjectName, int direction)
    {
        int draggingRow = int.Parse(draggingObjectName.Split('_')[1]);
        int draggingCol = int.Parse(draggingObjectName.Split('_')[2]);
        int firstRow = int.Parse(_firstLetterName.Split('_')[1]);
        int firstCol = int.Parse(_firstLetterName.Split('_')[2]);

        if (direction == 1)
        {
            return draggingCol > firstCol; // Right
        }
        else if (direction == 2)
        {
            return draggingCol < firstCol; // Left
        }
        else if (direction == 3)
        {
            return draggingRow > firstRow; // Down
        }
        else if (direction == 4)
        {
            return draggingRow < firstRow; // Up
        }
        else if (direction == 5)
        {
            // Down-right diagonal
            return draggingRow > firstRow && draggingCol > firstCol;
        }
        else if (direction == 6)
        {
            // Down-left diagonal
            return draggingRow > firstRow && draggingCol < firstCol;
        }
        else if (direction == 7)
        {
            // Up-right diagonal
            return draggingRow < firstRow && draggingCol > firstCol;
        }
        else if (direction == 8)
        {
            // Up-left diagonal
            return draggingRow < firstRow && draggingCol < firstCol;
        }

        return false;
    }




    public bool HaveReachedToTheStartLetter(string current, string last)
    {
        return current.Equals(last);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Get the GameObject the pointer is over
        GameObject rayCastedObject = eventData.pointerCurrentRaycast.gameObject;

        if (rayCastedObject == null)
        {
            return;
        }
        _firstLetterName = rayCastedObject.name;

        if (!_firstLetterName.Contains("BoardLetter"))
        {
            return;
        }

        rayCastedObject.GetComponent<Text>().color = _selectedColor;

        Vector3 letterAnchors = rayCastedObject.GetComponent<RectTransform>().anchoredPosition3D;
        _drawLineRect.sizeDelta = _drawLineDefaultSize;
        if (GameData.UnlockedLevel <= 10)
        {
            _drawLineRect.sizeDelta = new Vector3(90, 90, 0);
        }
        _drawLineRect.anchoredPosition = letterAnchors;
        drawLineColor = GetRandomColor();
        _drawLine.GetComponent<Image>().color = drawLineColor;

        _lastDraggedLetter = rayCastedObject;

        _selectedLetters.Add(rayCastedObject);

        SetHint(drawLineColor);
    }

    Color drawLineColor = Color.white;
    private void SetHint(Color color)
    {
        string selectedWord = String.Empty;
        foreach (var item in _selectedLetters)
        {
            selectedWord += item.GetComponent<Text>().text;
        }

        int lengthIncremental = 80;
        if (selectedWord.Length > 1)
        {
            lengthIncremental = 80 + 30 * selectedWord.Count();
        }
        else if (selectedWord.Length < 1)
        {
            lengthIncremental = 0;
        }

        _selecedWordHint.GetComponent<Image>().color = color;
        _selecedWordHint.GetComponent<RectTransform>().sizeDelta = new Vector3(lengthIncremental, 80, 0);
        _selecedWordHint.transform.GetChild(0).GetComponent<Text>().text = selectedWord;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        string selectedWord = String.Empty;
        foreach (var item in _selectedLetters)
        {
            selectedWord += item.GetComponent<Text>().text;
            if (!item.name.Contains("_MATCHED"))
            {
                item.GetComponent<Text>().color = _defaultColor;
            }
        }

        Debug.Log(selectedWord);
        string matchedWord = GameManager.Instance.CurrentLevelWords.Find((word) => word.Equals(selectedWord));
        if (matchedWord != null && !GameManager.Instance.IsMarkedTheWord(matchedWord))
        {
            GameManager.Instance.MarkWordAsFound(matchedWord);
            GameObject drawLineForMatchedWord = Instantiate(_drawLine, _matchedDrawLine.transform);
            drawLineForMatchedWord.GetComponent<Canvas>().sortingOrder = 1;

            foreach (var item in _selectedLetters)
            {
                item.name += "_MATCHED";
                selectedWord += item.GetComponent<Text>().text;
                item.GetComponent<Text>().color = _selectedColor;
            }

            StartCoroutine(AudioManager.Instance.PlaySound(_completedWordClip));
        }
        else 
        {
            StartCoroutine(AudioManager.Instance.PlaySound(_wrongMatchClip));
        }

        _drawLineRect.pivot = new Vector2(0.5f, 0.5f);
        _drawLineRect.sizeDelta = Vector2.zero;
        _lastDirection = 0;
        _lastDraggedLetter = null;
        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0);

        _selectedLetters.Clear();
        SetHint(Color.black);
    }



    public static Color32 GetRandomColor()
    {
        Color32[] vibrantColors = new Color32[]
        {
        new Color32(220, 100, 100, 255), // Soft Coral
        new Color32(200, 140, 90, 255),  // Warm Peach
        new Color32(180, 130, 200, 255), // Soft Lavender Purple
        new Color32(100, 180, 200, 255), // Light Teal
        new Color32(140, 200, 100, 255), // Fresh Green
        new Color32(200, 180, 90, 255),  // Goldenrod
        new Color32(150, 100, 200, 255), // Lilac
        new Color32(100, 200, 160, 255), // Aqua Mint
        new Color32(200, 150, 130, 255), // Soft Sandstone
        new Color32(200, 100, 160, 255), // Rose Pink
        new Color32(160, 200, 90, 255),  // Apple Green
        new Color32(200, 120, 150, 255), // Blush Pink
        new Color32(90, 160, 200, 255),  // Sky Blue
        new Color32(200, 90, 130, 255),  // Soft Crimson
        new Color32(120, 200, 140, 255)  // Fresh Mint
        };

        int index = UnityEngine.Random.Range(0, vibrantColors.Length);
        return vibrantColors[index];
    }


}
