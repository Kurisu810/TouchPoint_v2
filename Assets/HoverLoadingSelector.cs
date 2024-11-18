using UnityEngine;
using UnityEngine.UI;

public class HoverLoadingCircle : MonoBehaviour
{
    [SerializeField]
    private GameObject loadingCirclePrefab; // Prefab for the loading circle

    [SerializeField]
    private Transform rayOrigin; // The origin of the ray (e.g., RayInteractor's transform)

    private GameObject loadingCircleInstance;
    private Image loadingCircleImage;

    // This function can be called by the Unity Event Wrapper on hover
    public void StartHovering()
    {
        if (loadingCircleInstance == null)
        {
            // Instantiate the loading circle prefab and get its image component
            loadingCircleInstance = Instantiate(loadingCirclePrefab);
            loadingCircleImage = loadingCircleInstance.GetComponentInChildren<Image>();

            // Make the loading circle visible
            loadingCircleInstance.SetActive(true);
        }

        // Update the position of the loading circle at the cursor
        UpdateLoadingCirclePosition();
    }

    public void StopHovering()
    {
        // Hide and destroy the loading circle when hover ends
        if (loadingCircleInstance != null)
        {
            loadingCircleInstance.SetActive(false);
            Destroy(loadingCircleInstance);
        }
    }

    private void UpdateLoadingCirclePosition()
    {
        // Perform a raycast from the ray origin to find the cursor position
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Position the loading circle at the hit point
            loadingCircleInstance.transform.position = hit.point;
            loadingCircleInstance.transform.forward = -rayOrigin.forward; // Face the user
        }
    }
}
