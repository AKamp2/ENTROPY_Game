using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class DrawPuzzle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    //public RectTransform canvas;
    public RectTransform canvas;     // The area where drawing is allowed (usually the entire Canvas)
    public GameObject drawingArea;
    public GameObject circlePrefab;       // The circle prefab to instantiate
    public GameObject nodePrefab;
    private float circleRadius;
    private float nodeRadius;
    public float circleSize = 20f;
    public float minDistance = 10f;       // Minimum distance between circles
    

    private Vector2 lastPosition;
    private PuzzleCircle currentCircle;
    private PuzzleShape currentShape;
    private PuzzleNode currentNode;
    private bool isDrawing = false;
    PuzzleShape completedCircuit = null;

    //game Logic
    
    //lists
    public List<PuzzleNode> nodes = new List<PuzzleNode>();
    private List<PuzzleCircle> circles = new List<PuzzleCircle>();
    public List<PuzzleShape> shapes = new List<PuzzleShape>();

    private bool nodesComplete = false;
    private bool puzzleComplete = false;
    private bool doorOpen = false;
    public GameObject connectedDoor;
    public GameObject connectedPanel;

    //energy
    public int maxEnergy;
    private int currentEnergy;
    public GameObject energySegmentPrefab;
    public GameObject energyParent; // Empty GameObject to hold energy segments
    public float desiredBarHeight = 300f; // Desired height of the energy bar
    public float gap = 1f; // Gap between each segment
    public float squareSize = 30;

    private List<Image> energySegments = new List<Image>();

    //reset
    private Color drawingAreaColor;

    //audio
    public AudioSource audioSource;
    public AudioClip circleCreate;
    public AudioClip puzzleLoss;
    public AudioClip puzzleWin;


    void Start()
    {
   
        // Ensure the canvas is set
        if (canvas == null)
        {
            canvas = GetComponent<RectTransform>();
        }
        RectTransform circleRect = circlePrefab.GetComponent<RectTransform>();
        if (circleRect != null)
        {
            circleRadius = circleRect.localScale.x * circleSize;
        }
        RectTransform nodeRect = nodePrefab.GetComponent<RectTransform>();
        if (nodeRect != null)
        {
            nodeRadius = nodeRect.localScale.x * (nodeRect.sizeDelta.x /2);
        }

        drawingAreaColor = drawingArea.GetComponent<Image>().color;


        //energy
        currentEnergy = maxEnergy;
        InitializeEnergyBar();
        

    }

    void InitializeEnergyBar()
    {
        // Clear previous energy segments
        foreach (Transform child in energyParent.transform)
        {
            Destroy(child.gameObject);
        }
        energySegments.Clear();

        // Calculate canvas scale factor
        float canvasHeight = canvas.rect.height; // Current height of the canvas
        float referenceHeight = canvas.GetComponent<CanvasScaler>().referenceResolution.y; // Reference height
        float scaleFactor = canvasHeight / referenceHeight; // Calculate scale factor based on current height

        // Calculate effective bar height using scale factor
        float effectiveBarHeight = desiredBarHeight * scaleFactor;

        // Get the square size from the prefab
        RectTransform segmentRect = energySegmentPrefab.GetComponent<RectTransform>();
        float squareHeight = squareSize * segmentRect.localScale.x; // Assuming square has equal width and height

        // Calculate the total number of segments that fit within the total height
        int numberOfSegments = Mathf.FloorToInt((effectiveBarHeight + gap) / (squareHeight + gap));


        // Create segments based on calculated number of segments
        for (int i = 0; i < numberOfSegments; i++)
        {
            GameObject segment = Instantiate(energySegmentPrefab, energyParent.transform);
            Image segmentImage = segment.GetComponent<Image>();
            energySegments.Add(segmentImage);

            // Set position for each segment
            RectTransform segmentTransform = segment.GetComponent<RectTransform>();
            float positionY = (squareHeight + gap) * i; // Calculate Y position
            segmentTransform.anchoredPosition = new Vector2(0, positionY); // Set anchored position

            // Adjust size
            segmentTransform.sizeDelta = new Vector2(squareSize, squareSize);
        }

        UpdateEnergyBar();
    }

    void UpdateEnergyBar()
    {
        // Calculate how much energy each segment represents
        float energyPerSegment = (float)maxEnergy / energySegments.Count;

        for (int i = 0; i < energySegments.Count; i++)
        {
            // Determine how much energy has been consumed to calculate transparency
            float energyThreshold = (i + 1) * energyPerSegment; // Threshold for the current segment

            if (currentEnergy >= energyThreshold)
            {
                // Fully visible if current energy exceeds the threshold
                energySegments[i].color = Color.Lerp(Color.red, Color.green, (float)i / energySegments.Count);
            }
            else if (currentEnergy < energyThreshold && currentEnergy >= i * energyPerSegment)
            {
                // Partially transparent if current energy is between the two thresholds
                float alpha = (currentEnergy - (i * energyPerSegment)) / energyPerSegment; // Calculate alpha based on remaining energy
                energySegments[i].color = new Color(1, 1, 1, alpha); // White with calculated transparency
            }
            else
            {
                // Fully transparent if current energy is below the threshold for the segment
                energySegments[i].color = new Color(0, 0, 0, 0); // Transparent if energy is depleted
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (puzzleComplete)
        {
            return;
        }
        if (IsPointerWithinPanel(eventData.position))
        {
            isDrawing = true;
            lastPosition = ScreenToCanvasPosition(eventData.position);
            CreateCircleAtPosition(lastPosition);

            //check to see if circle is starting in an existing shape
            List<PuzzleShape> relatedShapes = GetRelatedShape(currentCircle);
            //Debug.Log(GetRelatedShape(currentCircle));
            if (relatedShapes != null)
            {
                if (relatedShapes.Count == 1)
                {
                    currentShape = relatedShapes[0];
                    relatedShapes[0].AddCircle(currentCircle);
                }
                //if there's more than 1 shape, connect them
                else
                {
                    for (int i = 1; i < relatedShapes.Count; i++)
                    {
                        relatedShapes[0].CombineShapes(relatedShapes[i]);
                    }
                    currentShape = relatedShapes[0];
                    relatedShapes[0].AddCircle(currentCircle);
                }
            }
            //if not, create a new shape
            else
            {
                PuzzleShape newShape = new PuzzleShape(currentCircle);
                shapes.Add(newShape);
                currentShape = newShape;
            }

            // Get the Canvas scale factor
            float canvasScaleFactor = canvas.localScale.x; // Assuming uniform scaling

            // Adjusted collision radius for circles
            float adjustedCircleRadius = circleRadius * canvasScaleFactor;
            float adjustedNodeRadius = nodeRadius * canvasScaleFactor;

            //if circle intersects a node
            foreach (PuzzleNode node in nodes)
            {
                if (Vector2.Distance(currentCircle.Position(), node.Position()) <= (adjustedCircleRadius + adjustedNodeRadius))
                {
                    currentShape.AddNode(node);
                    currentNode = node;

                }

            }

            foreach (PuzzleNode n in currentShape.connectedNodes)
            {
                n.CheckComplete();
            }




        }

    }

    public void OnDrag(PointerEventData eventData)
    {

        if (puzzleComplete)
        {
            return;
        }

        if (isDrawing && IsPointerWithinPanel(eventData.position))
        {
            Vector2 currentPosition = ScreenToCanvasPosition(eventData.position);

            // Check if the current position is far enough from the last position
            if (Vector2.Distance(lastPosition, currentPosition) >= minDistance)
            {
                CreateCircleAtPosition(currentPosition);

                PuzzleShape nearbyShape = GetNearbyShape(currentCircle);
                if (nearbyShape != currentShape)
                {
                    nearbyShape.CombineShapes(currentShape);
                    shapes.Remove(currentShape);
                    currentShape = nearbyShape;
                    nearbyShape.AddCircle(currentCircle);
                    
                }
                //if not, keep adding to current shape
                else
                {
                    currentShape.AddCircle(currentCircle);
                }

                // Get the Canvas scale factor
                float canvasScaleFactor = canvas.localScale.x; // Assuming uniform scaling

                // Adjusted collision radius for circles
                float adjustedCircleRadius = circleRadius * canvasScaleFactor;
                float adjustedNodeRadius = nodeRadius * canvasScaleFactor;

                //if circle intersects a node
                foreach (PuzzleNode node in nodes)
                {

                    if (Vector2.Distance(currentCircle.Position(), node.Position()) <= (adjustedCircleRadius + adjustedNodeRadius - 10))
                    {
                        currentShape.AddNode(node);

                        //stop drawing if the player is connecting to a different node
                        if (node != currentNode)
                        {
                            isDrawing = false;
                        }
                        
                        foreach(PuzzleNode n in currentShape.connectedNodes)
                        {
                            n.CheckComplete();
                        }

                        
                        currentNode = node;

                    }

                }

                //Debug.Log(shapes.Count);

                lastPosition = currentPosition;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    private void CreateCircleAtPosition(Vector2 position)
    {
        if(currentEnergy > 0)
        {
            // Instantiate the circle prefab
            GameObject circle = Instantiate(circlePrefab, canvas);
            circle.transform.localPosition = position;

            circle.transform.SetSiblingIndex(2);  // Moves it to the back in the hierarchy

            RectTransform circleRect = circle.GetComponent<RectTransform>();
            circleRect.sizeDelta = new Vector2(circleSize * 2, circleSize * 2); // Adjust width and height based on radius

            currentCircle = new PuzzleCircle(circle);

            circles.Add(currentCircle);

            audioSource.PlayOneShot(circleCreate);

            currentEnergy--;
            UpdateEnergyBar();
        }
        else
        {
            //out of energy
            isDrawing = false;
            Debug.Log("Out of energy");
            audioSource.PlayOneShot(puzzleLoss);
            HandleEnergyDepletion();
        }

    }

    private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
    {
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPosition, null, out canvasPosition);
        return canvasPosition;
    }

    private bool IsPointerWithinPanel(Vector2 screenPosition)
    {
        // Check if the mouse/touch is within the bounds of the panel
        return RectTransformUtility.RectangleContainsScreenPoint(drawingArea.GetComponent<RectTransform>(), screenPosition, null);
    }

    //method to be used in OnPointerDown, checks to see if the point is intersecting another shape
    private List<PuzzleShape> GetRelatedShape(PuzzleCircle circleToCheck)
    {

        float canvasScaleFactor = canvas.localScale.x; // Assuming uniform scaling
        float adjustedRadius = circleRadius * canvasScaleFactor;
        float collisionBuffer = 3 * canvasScaleFactor; //makes circles a little less sensitive to collision with eachother, higher number = less sensitive
        List<PuzzleShape> relatedShapes = new List<PuzzleShape>();
        

        foreach (PuzzleCircle circle in circles)
        {

            if (circle != circleToCheck)
            {
                
                if (Vector2.Distance(circle.Position(), circleToCheck.Position()) <= (adjustedRadius * 2) + collisionBuffer)
                {
                    //Debug.Log(Vector2.Distance(circle.Position(), circleToCheck.Position()));
                    //Debug.Log(adjustedRadius * 2);
                    if (relatedShapes.Contains(circle.parentShape) == false)
                    {
                        relatedShapes.Add(circle.parentShape);
                    }
                }
            }



        }
        if (relatedShapes.Count > 0)
        {
            return relatedShapes;
        }
        else
        {
            return null;
        }
    }

    //method to be used in OnDrag, checks to see if the current shape is intersecting another shape
    private PuzzleShape GetNearbyShape(PuzzleCircle circleToCheck)
    {

        float canvasScaleFactor = canvas.localScale.x; // Assuming uniform scaling
        float adjustedRadius = circleRadius * canvasScaleFactor;

        foreach (PuzzleCircle circle in circles)
        {

            if (circle != circleToCheck)
            {
                if (Vector2.Distance(circle.Position(), circleToCheck.Position()) <= (adjustedRadius * 2))
                {
                    //Debug.Log(Vector2.Distance(circle.Position(), circleToCheck.Position()));
                    //Debug.Log(adjustedRadius * 2);
                    return circle.parentShape;
                }
            }

        }
        return currentShape;
    }

    private void HandleEnergyDepletion()
    {
        StartCoroutine(FlashColors(new Color(128 / 255f, 0, 0), new Color(66 / 255f, 0, 0)));
        
    }

    private IEnumerator FlashColors(Color color1, Color color2)
    {
        float flashDuration = 1.5f; // Total duration of the flash effect
        //float flashInterval = 0.25f; // Duration of each flash color (red or white)

        float elapsedTime = 0f;

        // Flash color 1
        Image drawingAreaImage = drawingArea.GetComponent<Image>();

        drawingAreaImage.color = color1;
        while (elapsedTime < flashDuration / 3)
        {
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Flash color 2
        drawingAreaImage.color = color2;
        elapsedTime = 0f;
        while (elapsedTime < flashDuration / 3)
        {
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Flash color 1

        drawingAreaImage.color = color1;
        elapsedTime = 0f;
        while (elapsedTime < flashDuration / 3)
        {
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        ResetEnergyAndShapes();



    }


    private void ResetEnergyAndShapes()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();

        // Clear all circles
        foreach (PuzzleCircle circle in circles)
        {
            Destroy(circle.circle);
        }
        circles.Clear();

        // Clear all shapes
        shapes.Clear();

        // Reset node colors
        foreach (PuzzleNode node in nodes)
        {
            node.ResetNode();
        }

        drawingArea.GetComponent<Image>().color = drawingAreaColor;
    }

    private IEnumerator WaitAndTogglePuzzleOff()
    {
        audioSource.PlayOneShot(puzzleWin);
        connectedPanel.GetComponent<TriggerPuzzle>().puzzleComplete = true;
        connectedPanel.GetComponent<TriggerPuzzle>().interactionText.SetActive(false);
        yield return new WaitForSeconds(1f); // Wait for 1 second
        
        
        connectedPanel.GetComponent<TriggerPuzzle>().TogglePuzzle(); // Call your existing method to toggle the puzzle off
    }

    void Update()
    {
        if (puzzleComplete == false)
        {
            //if all the nodes are connected to their previous and next nodes
            if (nodesComplete == false)
            {
                bool allNodesActivated = true;
                foreach (PuzzleNode node in nodes)
                {
                    if (node.nodeComplete == false)
                    {
                        allNodesActivated = false;
                    }

                }
                nodesComplete = allNodesActivated;
            }
            else
            {
                puzzleComplete = true;
                /*
                completedCircuit = nodes[0].parentShape;
                foreach (PuzzleNode node in nodes)
                {
                    if (node.parentShape != completedCircuit)
                    {
                        puzzleComplete = false;
                    }

                }
                */
            }
        }
        else
        {
            if (doorOpen == false)
            {
                doorOpen = true;
                if (ColorUtility.TryParseHtmlString("#0F3827", out Color bgColor))
                {
                    drawingArea.GetComponent<Image>().color = bgColor;
                }
                connectedDoor.GetComponent<DoorScript>().PuzzleComplete();
                StartCoroutine(WaitAndTogglePuzzleOff());
            }

        }
    }
}