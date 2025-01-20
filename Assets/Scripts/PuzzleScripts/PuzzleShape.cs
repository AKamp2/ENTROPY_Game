using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleShape
{
    private List<PuzzleCircle> circles = new List<PuzzleCircle>();
    public Color shapeColor;
    public List<PuzzleNode> connectedNodes = new List<PuzzleNode>();

    public PuzzleShape(PuzzleCircle circle)
    {
        shapeColor = new Color(240/255f, 255/255f, 67/255f);
        circles.Add(circle);
        circle.parentShape = this;
        circle.SetColor(shapeColor);
        
    }

    public void AddCircle(PuzzleCircle circle)
    {
        circles.Add(circle);
        circle.parentShape = this;
        circle.SetColor(shapeColor);
    }

    public void AddNode(PuzzleNode node)
    {
        //only add the node if its new to the shape
        if (!connectedNodes.Contains(node))
        {
            connectedNodes.Add(node);
            node.parentShapes.Add(this);
            node.isActivated = true;
            node.SetColor(new Color(255/255f, 253/255f, 204/255f));
        }
        
    }

    public List<PuzzleCircle> GetCircles()
    {
        return circles;
    }

    public void CombineShapes(PuzzleShape otherLine)
    {

        foreach(PuzzleCircle circle in otherLine.GetCircles())
        {
            AddCircle(circle);
        }
        foreach(PuzzleNode node in otherLine.connectedNodes)
        {
            AddNode(node);
        }

    }

    public void ChangeColor(Color color)
    {
        shapeColor = color;
        foreach(PuzzleCircle circle in circles)
        {
            circle.SetColor(color);
        }
    }

    


}
