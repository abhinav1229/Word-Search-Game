using UnityEngine;
using UnityEngine.EventSystems;

public class LetterDragController : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _drawLine;


    private GameObject _lastDraggedLetter;
    private RectTransform _drawLineRect;
    private int _lastDirection = 0;
    private Vector2Int _cellSize;
    private readonly float _cellsGapInX = 10;
    private readonly float _cellsGapInY = 5;
    private Vector2 _drawLineDefaultSize = new(80, 80);
    private string _firstLetterName;

    private void Start()
    {
        _drawLineRect = _drawLine.GetComponent<RectTransform>();
        _cellSize = GameData.Instance.GetLevelBoardCellSize(GameData.UnlockedLevel);
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

        int directionChangeMultiplier = 1;

        Debug.Log("Dragging on direction: " + direction);

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
                else
                {
                    _drawLine.transform.eulerAngles = new Vector3(0, 0, 90);
                    _drawLine.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    dragLineAnchors.y -= _lastDirection == 0 ? 40 : 80;
                }

                _drawLineRect.anchoredPosition = dragLineAnchors;                
                _lastDirection = direction;
            }
            else
            {
                directionChangeMultiplier = -1;
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

        _drawLineRect.sizeDelta = drawLineSizeRect;
        _lastDraggedLetter = currentDraggedLetter;
    }

    private bool HaveCrossedToTheStartLetter(string draggingObjectName, int direction)
    {
        if(direction == 1)
        {
            return int.Parse(draggingObjectName.Split('_')[2]) > int.Parse(_firstLetterName.Split('_')[2]);
        }
        else if(direction == 2)
        {
            return int.Parse(draggingObjectName.Split('_')[2]) < int.Parse(_firstLetterName.Split('_')[2]);
        }
        else if(direction == 3)
        {
            return int.Parse(draggingObjectName.Split('_')[1]) > int.Parse(_firstLetterName.Split('_')[1]);
        }
        else if(direction == 4)
        {
            return int.Parse(draggingObjectName.Split('_')[1]) < int.Parse(_firstLetterName.Split('_')[1]);
        }

        return false;
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
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        _drawLineRect.pivot = new Vector2(0.5f, 0.5f);
        _drawLineRect.sizeDelta = Vector2.zero;
        _lastDirection = 0;
        _lastDraggedLetter = null;
        _drawLine.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public int GetDirection(string last, string current)
    {
        int last_row = int.Parse(last.Split('_')[1]);
        int last_col = int.Parse(last.Split('_')[2]);

        int curr_row = int.Parse(current.Split('_')[1]);
        int curr_col = int.Parse(current.Split('_')[2]);

        // Debug.Log($"Last: ({last_row}, {last_col}) | Current: ({curr_row}, {curr_col}) ");

        if (curr_row == last_row)
        {
            if (curr_col > last_col) return 1;
            if (curr_col < last_col) return 2;
        }

        if (curr_col == last_col)
        {
            if (curr_row > last_row) return 3;
            if (curr_row < last_row) return 4;
        }

        return -1;
    }
}
