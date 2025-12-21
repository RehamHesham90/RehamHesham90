using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene loading


public class winLevel : MonoBehaviour
{

    [SerializeField] private int sceneName; // Name of the scene to load

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering has the "Player" tag
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }



// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
