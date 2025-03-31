using UnityEngine;
using UnityEngine.UI;

public class GridResizer
{
    private RectTransform gridTransform;
    private int gridWidth;
    private int gridHeight;

    public GridResizer(RectTransform grid, int width, int height)
    {
        this.gridTransform = grid;
        this.gridWidth = width;
        this.gridHeight = height;
    }

    public void ResizeGrid()
    {
        if (gridTransform == null) return;

        float newWidth = 5 + gridWidth*20;
        float newHeight = 5 + gridHeight*20;

        gridTransform.sizeDelta = new Vector2(newWidth, newHeight);

        //Debug.Log($"Grid resized to: {newWidth} x {newHeight}");
    }
}