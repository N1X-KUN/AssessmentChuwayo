using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 1.0f; // How long the animation takes to finish

    void Start()
    {
        // Kills the object automatically after 'lifetime' seconds
        Destroy(gameObject, lifetime); 
    }
}