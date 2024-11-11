using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterDragController : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _drawLine;
    [SerializeField] private GameObject _matchedDrawLine;


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

    private void Start()
    {
        _drawLineRect = _drawLine.GetComponent<RectTransform>();
        _cellSize = GameData.Instance.GetLevelBoardCellSize(GameData.UnlockedLevel);

        if (GameData.UnlockedLevel > 20)
        {
            _cellsGapInY = 0;
        }

    }
    public void OnBeginDrag(PointerEventData eventData)
    {

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

        if (direction == -1)
        {
            return;
        }

        int directionChangeMultiplier = 1;

        if (IsTakenNonLinearWildMove(currentDraggedLetter.name))
        {
            // Debug.Log("Dragging IsTakenNonLinearWildMove");
            return;
        }

        int distanceMultiplier = 1;

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
                        drawLineSizeRect.x -= (_cellSize.y + _cellsGapInY) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 2)
                    {
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);
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
                        drawLineSizeRect.x -= (_cellSize.y + _cellsGapInY) * (_selectedLetters.Count - 1);
                    }
                    else if (_lastDirection == 1)
                    {
                        drawLineSizeRect.x -= (_cellSize.x + _cellsGapInX) * (_selectedLetters.Count - 1);

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
                        drawLineSizeRect.x -= (_cellSize.y + _cellsGapInY) * (_selectedLetters.Count - 1);
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
                        drawLineSizeRect.x -= (_cellSize.y + _cellsGapInY) * (_selectedLetters.Count - 1);
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
            drawLineSizeRect.x += (_cellSize.y + _cellsGapInY) * directionChangeMultiplier * distanceMultiplier;
        }
        else if (direction == 4)
        {
            drawLineSizeRect.x += (_cellSize.y + _cellsGapInY) * directionChangeMultiplier * distanceMultiplier;
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

        Debug.Log($"drawLineSizeRect: {drawLineSizeRect}");
        _drawLineRect.sizeDelta = drawLineSizeRect;
        _lastDraggedLetter = currentDraggedLetter;
    }

    private void SetLetterInDirection(int direction, int distance)
    {
        Debug.Log("SetLetterInDirection: " + direction + " | " + distance);

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
        }

        // Loop through each distance step in the chosen direction
        for (int i = 0; i < distance; i++)
        {
            int currentRow = firstRow + rowOffset * i;
            int currentCol = firstCol + colOffset * i;

            // Attempt to find the GameObject in the grid; fallback to "_MATCHED" if not found
            GameObject go = transform.Find($"BoardLetter_{currentRow}_{currentCol}")?.gameObject
                            ?? transform.Find($"BoardLetter_{currentRow}_{currentCol}_MATCHED")?.gameObject;

            Debug.Log("GO:: " + go);

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

    private int GetDistanceBetweenCurrentAndFirst(string currentObject)
    {
        int first_row = int.Parse(_firstLetterName.Split('_')[1]);
        int first_col = int.Parse(_firstLetterName.Split('_')[2]);

        int curr_row = int.Parse(currentObject.Split('_')[1]);
        int curr_col = int.Parse(currentObject.Split('_')[2]);

        return (int)Math.Sqrt((curr_row - first_row) * (curr_row - first_row) + (curr_col - first_col) * (curr_col - first_col));
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
        if (direction == 1)
        {
            return int.Parse(draggingObjectName.Split('_')[2]) > int.Parse(_firstLetterName.Split('_')[2]);
        }
        else if (direction == 2)
        {
            return int.Parse(draggingObjectName.Split('_')[2]) < int.Parse(_firstLetterName.Split('_')[2]);
        }
        else if (direction == 3)
        {
            return int.Parse(draggingObjectName.Split('_')[1]) > int.Parse(_firstLetterName.Split('_')[1]);
        }
        else if (direction == 4)
        {
            return int.Parse(draggingObjectName.Split('_')[1]) < int.Parse(_firstLetterName.Split('_')[1]);
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

        if(!_firstLetterName.Contains("BoardLetter"))
        {
            return;
        }

        rayCastedObject.GetComponent<Text>().color = _selectedColor;

        Vector3 letterAnchors = rayCastedObject.GetComponent<RectTransform>().anchoredPosition3D;
        _drawLineRect.sizeDelta = _drawLineDefaultSize;
        _drawLineRect.anchoredPosition = letterAnchors;
        _drawLine.GetComponent<Image>().color = GetRandomColor();

        _lastDraggedLetter = rayCastedObject;

        _selectedLetters.Add(rayCastedObject);
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
        string matchedWord = GameManager.instance._currentLevelWords.Find((word) => word.Equals(selectedWord));
        if (matchedWord != null && !GameManager.instance.IsMarkedTheWord(matchedWord))
        {
            GameManager.instance.MarkWordAsFound(matchedWord);
            GameObject drawLineForMatchedWord = Instantiate(_drawLine, _matchedDrawLine.transform);
            drawLineForMatchedWord.GetComponent<Canvas>().sortingOrder = 1;

            foreach (var item in _selectedLetters)
            {
                item.name += "_MATCHED";
                selectedWord += item.GetComponent<Text>().text;
                item.GetComponent<Text>().color = _selectedColor;
            }
        }

        _drawLineRect.pivot = new Vector2(0.5f, 0.5f);
        _drawLineRect.sizeDelta = Vector2.zero;
        _lastDirection = 0;
        _lastDraggedLetter = null;
        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0);

        _selectedLetters.Clear();
    }

    public int GetDirection(string last, string current)
    {
        int last_row = int.Parse(last.Split('_')[1]);
        int last_col = int.Parse(last.Split('_')[2]);

        int curr_row = int.Parse(current.Split('_')[1]);
        int curr_col = int.Parse(current.Split('_')[2]);

        int first_row = int.Parse(_firstLetterName.Split('_')[1]);
        int first_col = int.Parse(_firstLetterName.Split('_')[2]);


        // Debug.Log($"Last: ({last_row}, {last_col}) | Current: ({curr_row}, {curr_col}) ");

        // Horizontal movement
        if (curr_row == last_row)
        {
            if (curr_col > last_col) return 1; // Right
            if (curr_col < last_col) return 2; // Left
        }

        // Vertical movement
        else if (curr_col == last_col)
        {
            if (curr_row > last_row) return 3; // Down
            if (curr_row < last_row) return 4; // Up
        }

        else if (curr_col == first_col)
        {
            if (curr_row > first_row) return 3; // Down
            if (curr_row < first_row) return 4; // Up
        }

        else if (curr_row == first_row)
        {
            if (curr_col > first_col) return 1; // Right
            if (curr_col < first_col) return 2; // Left
        }

        // Diagonal movement
        // if (curr_row > last_row && curr_col > last_col) return 5; // Down-Right
        // if (curr_row > last_row && curr_col < last_col) return 6; // Down-Left
        // if (curr_row < last_row && curr_col > last_col) return 7; // Up-Right
        // if (curr_row < last_row && curr_col < last_col) return 8; // Up-Left

        return -1; // No direction matched
    }

    public static Color32 GetRandomColor()
    {
        Color32[] lightColors = new Color32[]
{
    new Color32(180, 180, 180, 255), // Light Gray
    new Color32(200, 200, 200, 255), // Soft Gray
    new Color32(190, 180, 170, 255), // Light Taupe
    new Color32(170, 190, 200, 255), // Light Slate Blue
    new Color32(180, 200, 180, 255), // Soft Green
    new Color32(160, 170, 190, 255), // Light Steel Blue
    new Color32(200, 180, 160, 255), // Light Sand
    new Color32(190, 170, 190, 255), // Soft Lavender
    new Color32(200, 190, 160, 255), // Light Olive
    new Color32(160, 190, 200, 255), // Soft Teal
    new Color32(180, 160, 180, 255), // Soft Lilac
    new Color32(200, 160, 160, 255), // Soft Rose
    new Color32(190, 170, 160, 255), // Light Beige
    new Color32(180, 160, 140, 255), // Light Tan
    new Color32(170, 160, 200, 255)  // Soft Mauve
};



        int index = UnityEngine.Random.Range(0, lightColors.Length);
        return lightColors[index];
    }

}
