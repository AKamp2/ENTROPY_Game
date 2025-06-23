using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindCustomization : MonoBehaviour
{
    [Header ("Binding")]
    [SerializeField] InputActionReference playerAction;
    [SerializeField] Key binding;

    [Header("UI")]
    [SerializeField] TMP_Text keybindLabel;
    [SerializeField] TMP_Text keybindText;
    [SerializeField] TMP_Text rebindText;

    public void Start()
    {
        keybindLabel.text = playerAction.name;
    }

    public void StartInteractiveRebind()
    {
        playerAction.action.Disable();
        playerAction.action.ApplyBindingOverride(userInput);
    }

}
