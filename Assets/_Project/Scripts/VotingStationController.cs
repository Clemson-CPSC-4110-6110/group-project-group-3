using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VotingStationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Permanent Identity")]
    public IdentityData identity; // ScriptableObject (Liberal, Fascist, or Hitler)

    [Header("Current Role")]
    public PlayerRole currentRole = PlayerRole.Civilian;
    public bool hasVoted = false;

    [Header("UI Links")]
    public TextMeshProUGUI identityTMP;
    public TextMeshProUGUI roleTMP;
    public Button jaButton;
    public Button neinButton;
    public GameObject votedFlag;

    // Use Start to hook up buttons automatically so you don't have to drag them in every time
    void Start()
    {
        if (jaButton != null) jaButton.onClick.AddListener(() => OnVotePressed(true));
        if (neinButton != null) neinButton.onClick.AddListener(() => OnVotePressed(false));
    }

    public void resetStation() {
        hasVoted = false;
        if (votedFlag != null) votedFlag.SetActive(false);
    }

    public void UpdateUI() {
        if (identityTMP == null) 
            identityTMP = transform.Find("VotingTray/VotingScreen/Panel/Player_Information/Identity/Identity_Text")?.GetComponent<TextMeshProUGUI>();
    
        if (roleTMP == null) 
            roleTMP = transform.Find("VotingTray/VotingScreen/Panel/Player_Information/Role/Role_Text")?.GetComponent<TextMeshProUGUI>();

        if (identityTMP != null) {
            identityTMP.text = (identity != null) ? identity.identityType.ToString() : "STILL NULL";
        }

        if (roleTMP != null) {
            roleTMP.text = currentRole.ToString();
        }
    }

    public void OnVotePressed(bool isJa) {
        if (hasVoted){
            Debug.Log("Already voted! Ignoring additional vote.");
            return; 
        }

        if (votedFlag != null) {
            votedFlag.SetActive(true); // Show the "Voted" flag
        }
        hasVoted = true;

        GamePlayManager manager = Object.FindFirstObjectByType<GamePlayManager>();
        if (manager != null) {
            manager.RecordVote(this, isJa);
        }
    }
}
