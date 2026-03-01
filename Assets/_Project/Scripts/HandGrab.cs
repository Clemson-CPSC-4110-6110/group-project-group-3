using UnityEngine;

public class HandGrabber : MonoBehaviour
{
    private Collider handCollider;
    private Transform grabbedObject;
    private Vector3 grabOffset;

    void Start()
    {
        handCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider collision)
    {
        // When hand touches an object, grab it
        if (collision.CompareTag("Grabable"))
        {
            GrabObject(collision.transform);
        }
    }

    void OnTriggerExit(Collider collision)
    {
        // When hand releases, drop it
        if (grabbedObject != null && collision.transform == grabbedObject)
        {
            ReleaseObject();
        }
    }

    void GrabObject(Transform obj)
    {
        grabbedObject = obj;
        
        // Make object follow hand
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics while held
        }
        
        // Calculate offset so object stays in hand
        grabOffset = obj.position - transform.position;
    }

    void ReleaseObject()
    {
        if (grabbedObject == null) return;

        // Re-enable physics when released
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        grabbedObject = null;
    }

    void Update()
    {
        // Make grabbed object follow hand
        if (grabbedObject != null)
        {
            grabbedObject.position = transform.position + grabOffset;
        }
    }
}