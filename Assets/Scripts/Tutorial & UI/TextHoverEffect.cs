using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextHoverEffect : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    private TMP_Text textMesh;
    private UIAudioManager uiAudio;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        textMesh.color = normalColor;

        uiAudio = FindObjectOfType<UIAudioManager>();
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //public void OnPointerExit(PointerEventData eventData)

    public void OnPointerEnter()
    {
        uiAudio.PlayHoverSound();
        textMesh.color = hoverColor;
    }

    public void OnPointerExit()
    {
        uiAudio.PlayExitHoverSound();
        textMesh.color = normalColor;
    }
}