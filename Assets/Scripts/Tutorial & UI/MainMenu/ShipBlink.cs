using System.Collections;
using UnityEngine;

public class ShipBlink : MonoBehaviour
{
    private float _toggleDelay = 1f;
    [SerializeField] GameObject targetGameObject;

    private void Start()
    {
        StartCoroutine(StartBlink());
    }

    private IEnumerator StartBlink()
    {
        while (true)
        {
            targetGameObject.SetActive(!targetGameObject.activeSelf);

            yield return new WaitForSeconds(_toggleDelay);
        }
    }
}
