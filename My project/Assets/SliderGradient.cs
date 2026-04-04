using UnityEngine;
using UnityEngine.UI;

public class SliderGradient : MonoBehaviour
{
    public Slider targetSlider;
    public Image fillImage;
    public KommyController kommy; // Drag Kommy here!
    
    [Header("Colors")]
    public Gradient normalGradient;
    public Gradient slowMoGradient; 

    void Update()
    {
        if (targetSlider != null && fillImage != null)
        {
            // If Kommy is sleeping, use the blue gradient! Otherwise, use normal.
            if (kommy != null && kommy.isAbilityActive)
            {
                fillImage.color = slowMoGradient.Evaluate(targetSlider.normalizedValue);
            }
            else
            {
                fillImage.color = normalGradient.Evaluate(targetSlider.normalizedValue);
            }
        }
    }
}