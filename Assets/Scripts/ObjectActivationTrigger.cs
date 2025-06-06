using UnityEngine;

public class ObjectActivationTrigger : MonoBehaviour
{
    public GameObject objectsToActivate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectsToActivate != null)
            {
                objectsToActivate.SetActive(true);
            }
        }
    }
}