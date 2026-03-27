using UnityEngine;
using UnityEngine.UI;

public class SliderGradient : MonoBehaviour
{
    public Slider targetSlider;
    public Image fillImage;
    public Gradient gradient;

    void Update()
    {
        if (targetSlider != null && fillImage != null)
        {
            // Changes the color based on the percentage of the slider!
            fillImage.color = gradient.Evaluate(targetSlider.normalizedValue);
        }
    }
}