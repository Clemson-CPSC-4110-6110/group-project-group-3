using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VotingStationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Permanent Identity")]
    public IdentityData identity; // ScriptableObject (Liberal, Fascist, or Hitler)

    [Header("Current Turn State")]
    public PlayerRole currentRole = PlayerRole.Civilian;
    public bool hasVoted = false;

    [Header("UI Links")]
    public TextMeshProUGUI identityTMP;
    public TextMeshProUGUI roleTMP;
    public Button jaButton;
    public Button neinButton;

    // Use Start to hook up buttons automatically so you don't have to drag them in every time
    void Start()
    {
        if (jaButton != null) jaButton.onClick.AddListener(() => OnVotePressed(true));
        if (neinButton != null) neinButton.onClick.AddListener(() => OnVotePressed(false));
    }

    public void SetTurn(PlayerRole newRole) {
        currentRole = newRole;
        hasVoted = false;
        UpdateUI();
    }

public void UpdateUI() {
    // The path must exactly match the names in your Hierarchy window
    if (identityTMP == null) 
        identityTMP = transform.Find("VotingTray/VotingScreen/Panel/Player_Information/Identity/Identity_Text")?.GetComponent<TextMeshProUGUI>();
    
    if (roleTMP == null) 
        roleTMP = transform.Find("VotingTray/VotingScreen/Panel/Player_Information/Role/Role_Text")?.GetComponent<TextMeshProUGUI>();

    // Update the mesh text
    if (identityTMP != null) {
        identityTMP.text = (identity != null) ? identity.identityType.ToString() : "STILL NULL";
    }

    if (roleTMP != null) {
        roleTMP.text = currentRole.ToString();
    }
}

    public void OnVotePressed(bool isJa) {
        if (hasVoted) return; // Prevent double-clicking

        hasVoted = true;
        UpdateUI();
        
        // Tell the manager to record the vote
        GamePlayManager manager = Object.FindFirstObjectByType<GamePlayManager>();
        if (manager != null) {
            manager.RecordVote(this, isJa);
        }
    }
}
