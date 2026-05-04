using UnityEngine;

public class VFXSelfDestruct : MonoBehaviour
{
    void Start()
    {
        // Destroys this explosion object after 1 second (adjust time if your animation is longer!)
        Destroy(gameObject, 1f); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
