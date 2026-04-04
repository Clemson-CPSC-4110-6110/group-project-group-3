using UnityEngine;

public enum identityType { Liberal, Fascist, Hitler }
public enum PlayerRole { President, Chancellor, Civilian }  

[CreateAssetMenu(fileName = "IdentityData", menuName = "Scriptable Objects/IdentityData")]
public class IdentityData : ScriptableObject
{
    public identityType identityType;
    //public string roleName;
}
