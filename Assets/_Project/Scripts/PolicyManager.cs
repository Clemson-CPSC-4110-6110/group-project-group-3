using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PolicyManager : MonoBehaviour
{
    public GameObject[] cardButtons; // Assign 3 buttons in the Inspector
    private int cardsDiscarded = 0;

    private List<identityType> currentOptions = new List<identityType>();

    void Start() {
        GenerateThreePolicies();
    }

    public void GenerateThreePolicies() {
        cardsDiscarded = 0;
        currentOptions.Clear();

        Color lightBlue = new Color(0.4f, 0.7f, 1.0f); // for liberal policies
        Color lightRed = new Color(1.0f, 0.4f, 0.4f); // for fascist policies

        for (int i = 0; i < 3; i++) {
            identityType p = (Random.value > 0.3f) ? identityType.Fascist : identityType.Liberal;
            currentOptions.Add(p);
    
            cardButtons[i].SetActive(true);

            //asign text and color
            TMP_Text btnText = cardButtons[i].GetComponentInChildren<TMP_Text>(true); 
            Image btnImage = cardButtons[i].GetComponent<Image>();
    
            if (btnText != null && btnImage != null) {
                btnText.text = p.ToString(); 
                btnText.color = Color.black;
                btnImage.color = (p == identityType.Liberal) ? lightBlue : lightRed;
                Debug.Log($"Button {i} text set to: {p}");
            } else {
                Debug.LogError($"Button {i} is missing a Text component in its children!");
            }   
        }
    }

    public void OnCardClicked(int index) {
        Debug.Log($"Card {index} clicked. Current options: {string.Join(", ", currentOptions)}. Cards discarded: {cardsDiscarded}");
        if (!GamePlayManager.Instance.getGovtStatus()) {
            Debug.LogWarning("No active government. Ignoring card click.");
            return;
        }
        if (cardsDiscarded < 2) {
            Debug.Log($"Discarding card {index} with policy {currentOptions[index]}");
            cardButtons[index].SetActive(false); // Make it "disappear"
            cardsDiscarded++;

            if (cardsDiscarded == 2) {
                FinalizePolicy();
                GenerateThreePolicies();
            }
        }
    }

    private void FinalizePolicy() {
        // Find the one button that is still active
        for (int i = 0; i < cardButtons.Length; i++) {
            if (cardButtons[i].activeSelf) {
                identityType enactedPolicy = currentOptions[i];
                Debug.Log($"Policy Enacted: {enactedPolicy}");
                
                // Call your Game Manager to update the board
                GamePlayManager.Instance.EnactPolicy(enactedPolicy);
                
                // Optional: Clear UI or prep for next round
                cardButtons[i].SetActive(false); 
                break;
            }
        }
    }
}
