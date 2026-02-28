using System.Collections;
using UnityEngine;

public class VotingButtonClick : MonoBehaviour
{

    public float clickCooldown = 1f; // Cooldown time in seconds
    public bool canBeClicked = true; // Flag to check if the button can be clicked

    private void OnTriggerEnter(Collider other)
    {
        if (canBeClicked && other.CompareTag("YesButton"))
        {
            // Handle the button click logic here
            Debug.Log("Voting Button clicked!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("YesButton"))
        {
            // Start the cooldown when the button is released
            Debug.Log("Voting Button released, starting cooldown.");
            StartCoroutine(ButtonCooldown());
        }
    }

    private IEnumerator ButtonCooldown()
    {
        canBeClicked = false; // Disable clicking
        yield return new WaitForSeconds(clickCooldown); // Wait for the cooldown duration
        canBeClicked = true; // Re-enable clicking
        Debug.Log("Voting Button is now clickable again.");
    }
}
