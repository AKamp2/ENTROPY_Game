using System.Collections;
using UnityEngine;

public class AirBreachTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private VentScript vent1;
    [SerializeField] private VentScript vent2;
    [SerializeField] private VentScript vent3;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            StartCoroutine(TriggerVents());

        }
    }

    private IEnumerator TriggerVents()
    {
        vent1.TurnOn();
        yield return new WaitForSeconds(3f);
        vent2.TurnOn();
        yield return new WaitForSeconds(0.5f);
        vent3.TurnOn();
    }
}
