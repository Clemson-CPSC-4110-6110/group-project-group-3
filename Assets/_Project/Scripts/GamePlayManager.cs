using UnityEngine;
using System.Collections;
using System.Linq; // For OrderByDescending
using System.Collections.Generic; // For List<T>

public class GamePlayManager : MonoBehaviour
{
    private int yesVotes = 0;
    private int noVotes = 0;
    private int numPlayers = 1; // Initialze in start based on player count
    private bool activeGovt = false;

    public string policyType; // Initialize policy type based on chancellor card

    private GameObject[] LiberalProgressTicks;
    private GameObject[] FascistProgressTicks;

    private int currentLiberalProgress = 0;
    private int currentFascistProgress = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LiberalProgressTicks = GameObject.FindGameObjectsWithTag("LiberalTick");
        FascistProgressTicks = GameObject.FindGameObjectsWithTag("FascistTick");
    }

    // Update is called once per frame
    void Update()
    {
        CheckGameEnd();
    }

    public void AddVotes(bool VoteResult){
        if (VoteResult){
            Debug.Log("Received a Yes vote. Total Yes votes: " + (yesVotes + 1));
            yesVotes++;
        }
        else{
            Debug.Log("Received a No vote. Total No votes: " + (noVotes + 1));
            noVotes++;
        }
        ProcessVotes();
    }

    private void ProcessVotes(){
        if (yesVotes + noVotes >= numPlayers){
             // All votes are in, determine outcome
            if (yesVotes > noVotes){
                Debug.Log("Vote passed with " + yesVotes + " Yes votes and " + noVotes + " No votes.");
                
                Debug.Log("Government Elected X is the President, Y is the Chancellor");
                EnactPolicy();
            }
            else{
                Debug.Log("Vote failed with " + yesVotes + " Yes votes and " + noVotes + " No votes.");
            }
            // Reset votes for next round
            yesVotes = 0;
            noVotes = 0;
        }
        else{
            // Handle failed vote (e.g., skip policy enactment, trigger special rules, etc.)
            Debug.Log("Not all votes are in yet. Current tally: " + yesVotes + " Yes votes and " + noVotes + " No votes.");
        }
    }

    private void EnactPolicy(){
        GameObject CurrentIndicator;;

        if (policyType == "Liberal"){
            if (currentLiberalProgress < LiberalProgressTicks.Length){
                CurrentIndicator = LiberalProgressTicks[currentLiberalProgress].transform.GetChild(1).gameObject; // Get the child GameObject (the indicator)
                CurrentIndicator.SetActive(true); // Activate the indicator
                currentLiberalProgress++;
                Debug.Log("Enacted a Liberal policy. Current Liberal progress: " + currentLiberalProgress);
            }
        }
        else if (policyType == "Fascist"){
            if (currentFascistProgress < FascistProgressTicks.Length){
                CurrentIndicator = FascistProgressTicks[currentFascistProgress].transform.GetChild(1).gameObject; // Get the child GameObject (the indicator)
                CurrentIndicator.SetActive(true); // Activate the indicator
                currentFascistProgress++;
                Debug.Log("Enacted a Fascist policy. Current Fascist progress: " + currentFascistProgress);
            }
        }
        else {
            Debug.LogError("Invalid policy type: " + policyType);
        }
    }

    private void CheckGameEnd(){
        if (FascistProgressTicks[5].transform.GetChild(1).gameObject.activeSelf == true)
        {
            Debug.Log("Fascists Win!");
        }
        else if (LiberalProgressTicks[4].transform.GetChild(1).gameObject.activeSelf == true)
        {
            Debug.Log("Liberals Win!");
        }
    }
}
