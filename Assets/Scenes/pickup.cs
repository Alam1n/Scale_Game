using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Transform holdPoint; // Drag your HoldPoint here in Inspector
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    private GameObject heldObject;

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            if (heldObject == null)
            {
                // Try pick up
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
                {
                    if (hit.collider.CompareTag("Pickup"))
                    {
                        PickUpObject(hit.collider.gameObject);
                    }
                }
            }
            else
            {
                // Drop
                DropObject();
            }
        }
    }

    void PickUpObject(GameObject pickObj)
    {
        if (pickObj.GetComponent<Rigidbody>())
        {
            Rigidbody rb = pickObj.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            pickObj.transform.position = holdPoint.position;
            pickObj.transform.parent = holdPoint;
            heldObject = pickObj;
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            heldObject.transform.parent = null;
            heldObject = null;
        }
    }
}
