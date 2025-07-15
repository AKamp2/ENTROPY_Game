using UnityEngine;
using TMPro;

public class WristMonitor : MonoBehaviour
{
    bool isActive = false;
    [SerializeField] ZeroGravity player;
    [SerializeField] TextMeshProUGUI health;
    public void ToggleWristMonitor()
    {
        isActive = !isActive;
        this.gameObject.SetActive(isActive);
    }

    private void Update()
    {
        health.text = $"Health: {player.PlayerHealth}";
    }
}
