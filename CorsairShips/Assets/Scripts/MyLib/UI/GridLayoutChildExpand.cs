using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridLayoutChildExpand : MonoBehaviour
{
    private GridLayoutGroup _gridLayout;
    private RectTransform _rectTransform;

    [SerializeField]
    private bool _HorizontalChildExpand;
    [SerializeField]
    private bool _VerticalChildExpand;

    private void OnValidate()
    {
        UpdateSpacing();
    }

// #if UNITY_EDITOR
//     [ExecuteAlways]
//     void Update()
//     {
//         UpdateSpacing();
//     }
// #endif
    
    void OnRectTransformDimensionsChange()
    {
        UpdateSpacing();
    }

    private void UpdateSpacing()
    {
        if(!_gridLayout)
            _gridLayout = GetComponent<GridLayoutGroup>();
        if(!_rectTransform)
            _rectTransform = _gridLayout.transform as RectTransform;
        if (_gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount) {
            var columnCount = _gridLayout.constraintCount;
            if (_HorizontalChildExpand) {
                var cellWidth = _gridLayout.cellSize.x;
                var cellsSumWidth = cellWidth * columnCount;
                var leftPlace = _rectTransform.rect.width - cellsSumWidth;
                // if (leftPlace < 0)
                // {
                //     Debug.LogError("Cant expand grid layout by width, because sum of cells width is more then");
                // }
                var space = leftPlace / columnCount;

                _gridLayout.padding.left = (int)(space / 2f);
                _gridLayout.spacing = new Vector2(space, _gridLayout.spacing.y);
            }
            if (_VerticalChildExpand) {
                var rowCount = _gridLayout.transform.childCount / columnCount;
                var cellHeight = _gridLayout.cellSize.y;
                var cellsSumHeight= cellHeight * rowCount;
                var upPlace = _rectTransform.rect.height - cellsSumHeight;
                
                var space = upPlace / rowCount;

                _gridLayout.padding.top = (int)(space / 2f);
                _gridLayout.spacing = new Vector2(_gridLayout.spacing.x, space);
            }
        }
        LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
    }
}
