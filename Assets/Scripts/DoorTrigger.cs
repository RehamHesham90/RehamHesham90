using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorOpenerNewInput doorScript; // drag Door GameObject (with script) here
    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            doorScript.SetPlayerNearby(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            doorScript.SetPlayerNearby(false);
        }
    }
}
