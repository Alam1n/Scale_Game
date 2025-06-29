using TMPro;
using UnityEngine;

public class Scale : MonoBehaviour
{
    public TextMeshPro myUIText;
    private float totalWeight = 0f;

    private void OnTriggerEnter(Collider other)
    {
        pickupObject obj = other.GetComponent<pickupObject>();
        if (obj != null)
        {
            Debug.Log("Was triggered");
            totalWeight += obj.weight;
            UpdateDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pickupObject obj = other.GetComponent<pickupObject>();
        if (obj != null)
        {
            totalWeight -= obj.weight;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        Debug.Log("Colected");
        myUIText.text = $"Total Weight: {totalWeight}";
    }
}
