using UnityEngine;
using System.Collections;
using System.Linq; // For OrderByDescending
using System.Collections.Generic; // For List<T>

public class GamePlayManager : MonoBehaviour{

    public List<IdentityData> availableIdentities; // Assign in Inspector with ScriptableObjects for each identity

    private int yesVotes = 0;
    private int noVotes = 0;
    private int numPlayers = 1; // Initialze in start based on player count
    private bool activeGovt = false;

    public string policyType; // Initialize policy type based on chancellor card

    private GameObject[] LiberalProgressTicks;
    private GameObject[] FascistProgressTicks;

    private int currentLiberalProgress = 0;
    private int currentFascistProgress = 0;

    private int currentPresidentIndex = 0; // Start at -1 so first increment in ReassignRoles sets to 0

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        // 1. Sort by name so Station (0) is always the first index
        stationObjects = stationObjects.OrderBy(go => go.name).ToArray();
        numPlayers = stationObjects.Length;

        Debug.Log($"[Manager] Initializing {numPlayers} players. Sorted stations: {string.Join(", ", stationObjects.Select(s => s.name))}");

        // 2. Build the Identity Pool
        List<identityType> identityPool = new List<identityType>();
        identityPool.Add(identityType.Hitler);
        identityPool.Add(identityType.Fascist);
        for (int i = 0; i < numPlayers - 2; i++){
            identityPool.Add(identityType.Liberal);
        }

        // 3. Shuffle
        for (int i = 0; i < identityPool.Count; i++) {
            int randomIndex = Random.Range(i, identityPool.Count);
            identityType temp = identityPool[i];
            identityPool[i] = identityPool[randomIndex];
            identityPool[randomIndex] = temp;
        }

        Debug.Log(stationObjects.Length + " stations found. Identity pool after shuffling: " + string.Join(", ", identityPool));

        // 4. Assign to Stations
        for (int i = 0; i < stationObjects.Length; i++) {

            Debug.Log($"[Manager] Assigning player {i} at station {stationObjects[i].name} the role of {identityPool[i]}");

            VotingStationController controller = stationObjects[i].GetComponentInChildren<VotingStationController>();

            if (controller != null) {
                Debug.Log("Inside Controller Check");

                identityType assignedType = identityPool[i];
                Debug.Log($"[Manager] Looking for IdentityData with type {assignedType} in AvailableIdentities...");

                // SEARCH: This looks through the list you assigned in the Inspector
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
                Debug.Log("Vote Passed! Majority said Ja.");
                activeGovt = true;
                EnactPolicy();
            }
            else{
                Debug.Log("Vote Failed! Tie or majority said Nein.");
                activeGovt = false;
            }

            yesVotes = 0;
            noVotes = 0;
        }
    }

    private void assignRoles(){
        // Random President
        currentPresidentIndex = Random.Range(0, numPlayers); 
    
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

            // Force the refresh
            controller.UpdateUI();
            }
        }
    }

    private void EnactPolicy(){
        GameObject CurrentIndicator;

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

        assignRoles();
    }
}