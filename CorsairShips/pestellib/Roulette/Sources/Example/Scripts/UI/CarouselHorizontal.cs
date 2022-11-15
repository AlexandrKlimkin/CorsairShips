using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class CarouselHorizontal : MonoBehaviour
{
    public bool Initialized { get { return initialized; } }

    GameObject _prefab;
    int numElements;
    Action<GameObject, int> _callback;

    [SerializeField] CarouselScrollRect scrollRect;
    [SerializeField] GridLayoutGroup grid;
    Vector2 lastNormalizedPosition;

    List<GameObject> listItems = new List<GameObject>();
    int minHElements;
    bool initialized;
    int positionIndex;

    public float GridItemWidth { get { return grid.cellSize.x + grid.spacing.x; } }
    public GridLayoutGroup Grid { get { return grid; } }
    public int ElementsCount { get { return minHElements; } }

    public void Init(int elementsCount, GameObject prefab, Action<GameObject, int> callback)
    {
        Clear();
        //scrollRect = GetComponent<CarouselScrollRect>();
        scrollRect.onValueChanged.AddListener(delegate { OnScroll(); });
        //grid = scrollRect.content.GetComponent<GridLayoutGroup>();
        _callback = callback;
        _prefab = prefab;
        numElements = elementsCount;

        minHElements = Mathf.RoundToInt((scrollRect.viewport.rect.width / (grid.cellSize.x + grid.spacing.x))) + 2;

        for (int i = 0; i < Mathf.Min(numElements, minHElements); i++)
        {
            var item = Instantiate(prefab) as GameObject;
            item.SetActive(true);
            item.transform.SetParent(scrollRect.content, false);
            listItems.Add(item);

            _callback(item, i);
            positionIndex = i;
        }

        initialized = true;

        var stepWidth = grid.cellSize.x + grid.spacing.x;
        var contentWidth = (numElements * stepWidth);
        scrollRect.horizontalScrollbar.size = (float)scrollRect.viewport.rect.width / contentWidth;
        scrollRect.horizontalScrollbar.value = 1;
    }

    public void Refresh()
    {
        if (listItems.Count < minHElements)
        {
            for (int i = 0; i < listItems.Count; i++)
            {
                _callback(listItems[i], i);
                positionIndex = i;
            }
            scrollRect.content.anchoredPosition = new Vector2(0, 0);

        }
        else
        {
            if (positionIndex > numElements - 1)
            {
                positionIndex = numElements - 1;
            }
            for (int i = 0; i < listItems.Count; i++)
            {
                _callback(listItems[i], positionIndex - (minHElements - 1) + i);
            }
        }
    }

    public void SetElementsCount(int elementsCount)
    {
        if (!initialized) Debug.LogError("Carousel is not initialized, call Init() first.");

        numElements = elementsCount;

        if ((listItems.Count < minHElements && numElements > minHElements) ||
            (listItems.Count == minHElements && numElements < minHElements))
        {
            for (int i = 0; i < listItems.Count; i++)
            {
                Destroy(listItems[i].gameObject);
            }
            listItems.Clear();

            for (int i = 0; i < Mathf.Min(numElements, minHElements); i++)
            {
                var item = Instantiate(_prefab) as GameObject;
                item.SetActive(true);
                item.transform.SetParent(scrollRect.content, false);
                listItems.Add(item);

                _callback(item, i);
                positionIndex = i;
            }
        }
    }

    void OnScroll()
    {
        if (!initialized || numElements <= minHElements) return;
        var diff = lastNormalizedPosition - scrollRect.normalizedPosition;
        var stepWidth = grid.cellSize.x + grid.spacing.x;
        float contentWidth = (numElements * stepWidth);
        if (diff.x < 0)
        {
            var itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scrollRect.viewport, listItems[listItems.Count - 1].transform);
            if (scrollRect.viewport.rect.width - itemBounds.min.x > stepWidth)
            {
                for (int i = 0; i < (int)Mathf.Abs((scrollRect.viewport.rect.width - itemBounds.min.x) / stepWidth); i++)
                {
                    var toMove = listItems[0];
                    listItems.Remove(toMove);
                    listItems.Add(toMove);
                    toMove.transform.SetAsLastSibling();
                    scrollRect.content.anchoredPosition += new Vector2(stepWidth, 0);

                    positionIndex++;
                    _callback(toMove, positionIndex);
                }
            }
        }

        var viewBounds = new Bounds(scrollRect.viewRect.rect.center, scrollRect.viewRect.rect.size);
        float contentMinX = (scrollRect.content.anchoredPosition.x - scrollRect.content.rect.width);
        float expectedContentExtend = (numElements - positionIndex) * stepWidth;

        scrollRect.horizontalScrollbar.size = (float)scrollRect.viewport.rect.width / contentWidth;
        scrollRect.horizontalScrollbar.value = -(viewBounds.min.x - (contentMinX - expectedContentExtend)) / (viewBounds.size.x - contentWidth);

        lastNormalizedPosition = scrollRect.normalizedPosition;
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Clear()
    {
        scrollRect.onValueChanged.RemoveAllListeners();
        foreach (var item in listItems)
            Destroy(item);
        listItems.Clear();
    }
}
