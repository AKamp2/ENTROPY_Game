using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NodeType
{
    Start,
    Middle,
    End
}

public class PuzzleNode : MonoBehaviour
{
    public List<PuzzleShape> parentShapes = new List<PuzzleShape>();
    public NodeType nodeType;
    public bool isActivated;
    public bool nodeComplete;
    public PuzzleNode previousNode;
    public PuzzleNode nextNode;
    public Color baseColor;

    private Image nodeImage;

    private void Awake()
    {
        // Cache the Image component and set the base color to the current color of the image.
        nodeImage = GetComponent<Image>();
        if (nodeImage != null)
        {
            baseColor = nodeImage.color;
        }
    }

    public void SetColor(Color color)
    {
        if (nodeImage != null)
        {
            nodeImage.color = color;
        }
    }

    public void ResetNode()
    {
        SetColor(baseColor); // Reset to the stored base color
        parentShapes.Clear();
        isActivated = false;
        nodeComplete = false;
    }

    public Vector2 Position()
    {
        RectTransform circleRect = GetComponent<RectTransform>();
        return new Vector2(circleRect.position.x, circleRect.position.y);
    }

    public void CheckComplete()
    {
        List<PuzzleNode> totalNodes = new List<PuzzleNode>();
        foreach (PuzzleShape parent in parentShapes)
        {
            totalNodes.AddRange(parent.connectedNodes);
        }

        // Check if this node is fully connected based on its type
        if (totalNodes.Count > parentShapes.Count) // More connections than parent shapes
        {
            switch (nodeType)
            {
                case NodeType.Start:
                    if (totalNodes.Contains(nextNode))
                    {
                        nodeComplete = true;
                    }
                    break;

                case NodeType.Middle:
                    if (totalNodes.Contains(previousNode) && totalNodes.Contains(nextNode))
                    {
                        nodeComplete = true;
                    }
                    break;

                case NodeType.End:
                    if (totalNodes.Contains(previousNode))
                    {
                        nodeComplete = true;
                    }
                    break;

                default:
                    Debug.Log("NodeType not specified");
                    break;
            }
        }

        if (nodeComplete)
        {
            SetColor(Color.white); // Set color to indicate completion
        }
    }
}