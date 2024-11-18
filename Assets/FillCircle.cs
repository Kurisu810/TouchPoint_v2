using UnityEngine;
using UnityEngine.UI;

public class LoadingCircleController : MonoBehaviour
{
    private Image loadingImage;
    private float fillAmount = 0f;
    private bool isFilling = false;

    void Start()
    {
        // Get the Image component
        loadingImage = GetComponent<Image>();
    }

    void Update()
    {
        if (isFilling)
        {
            // Increment the fill amount over time (e.g., 1 second to fill the circle)
            fillAmount += Time.deltaTime;
            loadingImage.fillAmount = Mathf.Clamp01(fillAmount);

            // Stop filling when the circle is full
            if (fillAmount >= 5f)
            {
                isFilling = false;
            }
        }
    }

    // Call this function to start the filling process
    public void StartFilling()
    {
        isFilling = true;
        fillAmount = 0f; // Reset fill amount when starting to fill
    }

    // Call this function to stop the filling process
    public void StopFilling()
    {
        isFilling = false;
    }
}
