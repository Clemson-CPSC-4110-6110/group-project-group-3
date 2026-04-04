using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // For XRSocketInteractor


public class SocketLogic : MonoBehaviour
{
/*
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor dropBox;
    //private GamePlayManager gamePlayManager;

    void Start()
    {
        gamePlayManager = FindObjectOfType<GamePlayManager>();

        dropBox = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

        if (dropBox == null)
        {
            Debug.LogError("XRSocketInteractor component not found on " + gameObject.name);
        }

        gamePlayManager = FindObjectOfType<GamePlayManager>();
        if (gamePlayManager == null)
        {
            Debug.LogError("GamePlayManager not found in the scene. Please ensure it is attached to a GameObject.");
        }
    }
    private void OnEnable() => dropBox.selectEntered.AddListener(OnCardSnapped);
    private void OnDisable() => dropBox.selectEntered.RemoveListener(OnCardSnapped);

    private void OnCardSnapped(SelectEnterEventArgs args)
    {
        // 1. Identify the snapped card
        GameObject snappedCard = args.interactableObject.transform.gameObject;
        string cardName = snappedCard.name;

        if (cardName.Contains("Liberal"))
        {
            gamePlayManager.policyType = "Liberal";
            Debug.Log("<color=blue>Socket Logic: Liberal Policy Loaded</color>");
        }
        else if (cardName.Contains("Fascist"))
        {
            gamePlayManager.policyType = "Fascist";
            Debug.Log("<color=red>Socket Logic: Fascist Policy Loaded</color>");
        }
        else
        {
            Debug.LogError("Socket Logic: Unrecognized card type loaded into socket: " + cardName);
        }
    }
*/
}