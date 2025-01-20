using UnityEngine;
using TMPro;

public class TextHoverEffect : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    private TMP_Text textMesh;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        textMesh.color = normalColor;
    }

    public void OnPointerEnter()
    {
        textMesh.color = hoverColor;
    }

    public void OnPointerExit()
    {
        textMesh.color = normalColor;
    }
}