using System.Collections;
using UnityEngine;
using System.Reflection; // For MethodInfo and BindingFlags
//using GameManager gm;

public class VotingButtonClick : MonoBehaviour
{
    private GamePlayManager GamePlayManager; // Reference to the GamePlayManager script

    public float clickCooldown = .05f; // Cooldown time in seconds
    public bool canBeClicked = true; // Flag to check if the button can be clicked

    private void Start()
    {
        // Find the GamePlayManager in the scene and get the reference
        GamePlayManager = FindObjectOfType<GamePlayManager>();
        if (GamePlayManager == null)
        {
            Debug.LogError("GamePlayManager not found in the scene. Please ensure it is attached to a GameObject.");
        }
        // This part "scans" the script for its functions
        MethodInfo[] methods = typeof(GamePlayManager).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        string methodList = "Functions found in GamePlayManager: \n";
        foreach (var method in methods)
        {
            methodList += "- " + method.Name + "()\n";
        }
        Debug.Log(methodList);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canBeClicked)
        {
            if (other.CompareTag("YesButton"))
            {
                // Handle the button click logic here
                Debug.Log("Yes Button clicked!");
                GamePlayManager.AddVotes(true); // Cast a "Yes" vote
            }
            else if (other.CompareTag("NoButton"))
            {
                // Handle the button click logic here
                Debug.Log("No Button clicked!");
                GamePlayManager.AddVotes(false); // Cast a "No" vote
            }
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("YesButton"))
        {
            // Start the cooldown when the button is released
            StartCoroutine(ButtonCooldown());
        }
        else if (other.CompareTag("NoButton"))
        {
            // Start the cooldown when the button is released
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
