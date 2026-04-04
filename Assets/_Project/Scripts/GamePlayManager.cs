using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class GamePlayManager : MonoBehaviour{

    public static GamePlayManager Instance;

    // ScriptableObjects for each identity (in inspector)
    public List<IdentityData> availableIdentities; 

    private int yesVotes = 0;
    private int noVotes = 0;
    private int numPlayers;

    // Policy type determined by active govt panel
    //public string policyType;
    private bool activeGovt = false;

    // Board Indicators to unhide when policies are enacted
    private GameObject[] LiberalProgressTicks;
    private GameObject[] FascistProgressTicks;
    private int currentLiberalProgress = 0;
    private int currentFascistProgress = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null){
            Instance = this;
            Debug.Log("GamePlayManager instance set.");
        }
        else{
            Debug.Log("Warning: Multiple instances of GamePlayManager detected. Destroying duplicate.");
            Destroy(gameObject);
        }

        // FInd the Board Pieces
        LiberalProgressTicks = GameObject.FindGameObjectsWithTag("LiberalTick");
        FascistProgressTicks = GameObject.FindGameObjectsWithTag("FascistTick");

        // Find the voting stations (trifold and Ipads)
        GameObject[] stationObjects = GameObject.FindGameObjectsWithTag("VotingStation");
        numPlayers = stationObjects.Length;

        Debug.Log("Found " + numPlayers + " voting stations via tags.");

        // Assign Ids and Roles
        if (stationObjects.Length > 0) {
            initializePlayers(stationObjects);
        } else {
            Debug.LogError("No voting stations found! Ensure they are tagged correctly.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckGameEnd();
    }

    private void CheckGameEnd(){
        // Check if either boards are full (game over)
        if (FascistProgressTicks[5].transform.GetChild(1).gameObject.activeSelf == true)
        {
            Debug.Log("Fascists Win!");
        }
        else if (LiberalProgressTicks[4].transform.GetChild(1).gameObject.activeSelf == true)
        {
            Debug.Log("Liberals Win!");
        }
    }

    private void initializePlayers(GameObject[] stationObjects) {
        
        // Sort the stations for future assignment
        stationObjects = stationObjects.OrderBy(go => go.name).ToArray();
        numPlayers = stationObjects.Length;

        Debug.Log($"[Manager] Initializing {numPlayers} players. Sorted stations: {string.Join(", ", stationObjects.Select(s => s.name))}");

        // Create a pool of identities based on number of players
        List<identityType> identityPool = new List<identityType>();
        identityPool.Add(identityType.Hitler);
        identityPool.Add(identityType.Fascist);
        for (int i = 0; i < numPlayers - 2; i++){
            identityPool.Add(identityType.Liberal);
        }

        // "Shuffle cards"
        for (int i = 0; i < identityPool.Count; i++) {
            int randomIndex = Random.Range(i, identityPool.Count);
            identityType temp = identityPool[i];
            identityPool[i] = identityPool[randomIndex];
            identityPool[randomIndex] = temp;
        }

        Debug.Log(stationObjects.Length + " stations found. Identity pool after shuffling: " + string.Join(", ", identityPool));

        // Assign identities and roles to each station
        for (int i = 0; i < stationObjects.Length; i++) {
            VotingStationController controller = stationObjects[i].GetComponentInChildren<VotingStationController>();

            if (controller != null) {
                identityType assignedType = identityPool[i];

                // this looks through the list assigned in the Inspector (Identity Data Scriptables)
                controller.identity = availableIdentities.Find(x => x != null && x.identityType == assignedType);

                // Assign Roles for the start of the game
                if (i == 0){
                    controller.currentRole = PlayerRole.President;
                }
                else if (i == 1){
                    controller.currentRole = PlayerRole.Chancellor;
                }
                else{
                    controller.currentRole = PlayerRole.Civilian;
                }

                // update the UI with roles and identities
                controller.UpdateUI();
            }
        }
    }

    
    public void RecordVote(VotingStationController station, bool isJa){
        if (isJa){
            yesVotes++; 
        }
        else{
            noVotes++;
        }

        // Once everyone has voted
        if (yesVotes + noVotes >= numPlayers){
            if (yesVotes > noVotes){
                Debug.Log("Vote Passed with " + yesVotes + " Ja votes and " + noVotes + " Nein votes!");
                Debug.Log("Activating government for policy selection.");
                activeGovt = true;
            }
            else{
                Debug.Log("Vote Failed with " + yesVotes + " Ja votes and " + noVotes + " Nein votes!");
                activeGovt = false;
                assignRoles(); // Reassign roles for next round if vote fails
            }
            // Reset vote counts for next round
            yesVotes = 0;
            noVotes = 0;
        }
    }

    private void assignRoles(){
        // Random President
        int currentPresidentIndex = Random.Range(0, numPlayers); 
    
        // Random Chancellor
        int nextChancellorIndex;
        do {
            nextChancellorIndex = Random.Range(0, numPlayers);
        } while (nextChancellorIndex == currentPresidentIndex);

        // Get Voting Stations (players)
        GameObject[] stations = GameObject.FindGameObjectsWithTag("VotingStation").OrderBy(s => s.name).ToArray();

        for (int i = 0; i < stations.Length; i++){
            VotingStationController controller = stations[i].GetComponentInChildren<VotingStationController>();
            if (controller != null){
                controller.hasVoted = false;

                switch (i){
                    case var _ when i == currentPresidentIndex:
                        controller.currentRole = PlayerRole.President;
                        Debug.Log($"[Manager] Player at station {stations[i].name} is the new President.");
                        break;
                    case var _ when i == nextChancellorIndex:
                        controller.currentRole = PlayerRole.Chancellor;
                        Debug.Log($"[Manager] Player at station {stations[i].name} is the new Chancellor.");
                        break;
                    default:
                        controller.currentRole = PlayerRole.Civilian;
                        Debug.Log($"[Manager] Player at station {stations[i].name} is a Civilian.");
                        break;
                }

                controller.UpdateUI(); // Refresh UI with new roles
                controller.resetStation(); // Reset voted flag
            }
        }
    }

    public void EnactPolicy(identityType policy){
        GameObject CurrentIndicator;

        if (activeGovt == false){
            Debug.LogError("Attempted to enact a policy without an active government!");
            return;
        }
        if (policy == identityType.Liberal){
            if (currentLiberalProgress < LiberalProgressTicks.Length){
                CurrentIndicator = LiberalProgressTicks[currentLiberalProgress].transform.GetChild(1).gameObject; // Get the child GameObject (the indicator)
                CurrentIndicator.SetActive(true); // Activate the indicator
                currentLiberalProgress++;
                Debug.Log("Enacted a Liberal policy. Current Liberal progress: " + currentLiberalProgress);
            }
        }
        else if (policy == identityType.Fascist){
            if (currentFascistProgress < FascistProgressTicks.Length){
                CurrentIndicator = FascistProgressTicks[currentFascistProgress].transform.GetChild(1).gameObject; // Get the child GameObject (the indicator)
                CurrentIndicator.SetActive(true); // Activate the indicator
                currentFascistProgress++;
                Debug.Log("Enacted a Fascist policy. Current Fascist progress: " + currentFascistProgress);
            }
        }
        else {
            Debug.LogError("Invalid policy type: " + policy.ToString());
        }

        // Reset for next round
        activeGovt = false; 
        assignRoles();
    }

    public bool getGovtStatus(){
        Debug.Log($"Current government status: activeGovt = {activeGovt}");
        return activeGovt;
    }
}