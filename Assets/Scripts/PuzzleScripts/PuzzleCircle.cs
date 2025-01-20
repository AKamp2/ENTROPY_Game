using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleCircle
{
    public GameObject circle;
    public PuzzleShape parentShape;

    public PuzzleCircle(GameObject circle)
    {
        this.circle = circle;
        parentShape = null;
    }


    public void SetColor(Color color)
    {
        circle.GetComponent<Image>().color = color;
    }

    public Vector2 Position()
    {
        RectTransform circleRect = circle.GetComponent<RectTransform>();
        Vector2 position = new Vector2(circleRect.position.x, circleRect.position.y);
        return position;
    }
}
