using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterDragController : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _drawLine;


    private GameObject _lastDraggedLetter;
    private RectTransform _drawLineRect;
    private int _lastDirection = 0;
    private Vector2Int _cellSize;
    private float _cellsGapInX = 10;
    private float _cellsGapInY = 5;
    private Vector2 _drawLineDefaultSize = new(80, 80);
    private string _firstLetterName;

    private List<char> _selectedLetters = new();

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

        // Debug.Log("Dragging on direction: " + direction);
        if (IsTakenWildMove(direction, _lastDirection))
        {
            Debug.Log("Dragging wildly");
            return;
        }

        if (IsTrickyCrossedStartLetter(currentDraggedLetter.name, direction))
        {
            Debug.Log("Dragging Tricky");
            return;
        }

        Debug.Log($"Passed: ({_lastDirection},{direction})");
        if (_lastDirection != direction)
        {
            if (HaveCrossedToTheStartLetter(currentDraggedLetter.name, direction) || _lastDirection == 0)
            {
                Vector3 dragLineAnchors = _drawLineRect.anchoredPosition;

                if (direction == 1)
                {
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.x -= _lastDirection == 0 ? 40 : 80;
                }
                else if (direction == 2)
                {
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                    dragLineAnchors.x += _lastDirection == 0 ? 40 : 80;
                }
                else if (direction == 3)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, 90);
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                    dragLineAnchors.y += _lastDirection == 0 ? 40 : 80;
                }
                else if (direction == 4)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, 90);
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.y -= _lastDirection == 0 ? 40 : 80;
                }

                // diagonals
                else if (direction == 5)
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, -39.3f);
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.y -= _lastDirection == 0 ? 26 : 26;
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

                _lastDirection = 0;
            }
        }


        if (direction == 1)
        {
            drawLineSizeRect.x += (_cellSize.x + _cellsGapInX) * directionChangeMultiplier;
        }
        else if (direction == 2)
        {
            drawLineSizeRect.x += (_cellSize.x + _cellsGapInX) * directionChangeMultiplier;
        }
        else if (direction == 3)
        {
            drawLineSizeRect.x += (_cellSize.y + _cellsGapInY) * directionChangeMultiplier;
        }
        else
        {
            drawLineSizeRect.x += (_cellSize.y + _cellsGapInY) * directionChangeMultiplier;
        }

        if (directionChangeMultiplier == 1)
        {
            _selectedLetters.Add(currentDraggedLetter.GetComponent<Text>().text[0]);
        }
        else
        {
            if (_selectedLetters.Count > 0)
            {
                _selectedLetters.RemoveAt(_selectedLetters.Count - 1);
            }
        }

        _drawLineRect.sizeDelta = drawLineSizeRect;
        _lastDraggedLetter = currentDraggedLetter;
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

        Vector3 letterAnchors = rayCastedObject.GetComponent<RectTransform>().anchoredPosition3D;
        _drawLineRect.sizeDelta = _drawLineDefaultSize;
        _drawLineRect.anchoredPosition = letterAnchors;
        _lastDraggedLetter = rayCastedObject;

        _selectedLetters.Add(rayCastedObject.GetComponent<Text>().text[0]);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        _drawLineRect.pivot = new Vector2(0.5f, 0.5f);
        _drawLineRect.sizeDelta = Vector2.zero;
        _lastDirection = 0;
        _lastDraggedLetter = null;
        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0);

        string selectedWord = String.Empty;
        foreach (var item in _selectedLetters)
        {
            selectedWord += item;
        }

        Debug.Log(selectedWord);

        _selectedLetters.Clear();
    }

    public int GetDirection(string last, string current)
    {
        int last_row = int.Parse(last.Split('_')[1]);
        int last_col = int.Parse(last.Split('_')[2]);

        int curr_row = int.Parse(current.Split('_')[1]);
        int curr_col = int.Parse(current.Split('_')[2]);

        // Debug.Log($"Last: ({last_row}, {last_col}) | Current: ({curr_row}, {curr_col}) ");

        // Horizontal movement
        if (curr_row == last_row)
        {
            if (curr_col > last_col) return 1; // Right
            if (curr_col < last_col) return 2; // Left
        }

        // Vertical movement
        if (curr_col == last_col)
        {
            if (curr_row > last_row) return 3; // Down
            if (curr_row < last_row) return 4; // Up
        }

        // Diagonal movement
        // if (curr_row > last_row && curr_col > last_col) return 5; // Down-Right
        // if (curr_row > last_row && curr_col < last_col) return 6; // Down-Left
        // if (curr_row < last_row && curr_col > last_col) return 7; // Up-Right
        // if (curr_row < last_row && curr_col < last_col) return 8; // Up-Left

        return -1; // No direction matched
    }

}
