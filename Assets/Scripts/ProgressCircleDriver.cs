using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Oculus.Interaction;

public class ProgressCircleDriver : MonoBehaviour
{
    [SerializeField] private RectTransform circleUI; // UI element (circle image)
    [SerializeField] private Camera userCamera; // Reference to the VR headset camera
    [SerializeField] private float displayDistance = 0.7f; // Distance from interactable
    [SerializeField] private float selectionTime = 1f; // Time required for full progress

    private Coroutine fillCoroutine;
    private Vector3 headsetPosition;
    private Vector3 targetPosition;

    void Start()
    {
        if (userCamera == null)
        {
            userCamera = Camera.main;
        }

        // Hide the circle UI initially
        circleUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when the interactable is hovered.
    /// </summary>
    public void OnHoverEnter(GameObject object1)
    {
        // Capture the user's headset position
        headsetPosition = userCamera.transform.position;

        // Calculate the target position for the circle UI element
        // Target position is a set distance away from the center of the interactable
        // and always facing the camera
        Vector3 interactableCenter = object1.transform.position;
        Vector3 interactionVector = (headsetPosition - interactableCenter).normalized;    
        targetPosition = interactableCenter + interactionVector * displayDistance;

        // Position and orient the circle UI
        circleUI.position = targetPosition;
        circleUI.LookAt(userCamera.transform);
        circleUI.rotation = Quaternion.Euler(0f, circleUI.rotation.eulerAngles.y + 180f, 0f);

        // Show the circle UI and start the fill coroutine
        circleUI.gameObject.SetActive(true);
        fillCoroutine = StartCoroutine(FillCircle());
    }

    /// <summary>
    /// Called when the interactable is unhovered.
    /// </summary>
    public void OnHoverExit()
    {
        // Stop the fill coroutine and reset the circle UI
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
        ResetCircle();
    }

    private IEnumerator FillCircle()
    {
        float timer = 0f;
        Image circleImage = circleUI.GetComponent<Image>();

        while (timer < selectionTime)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / selectionTime);
            circleImage.fillAmount = progress;

            yield return null;
        }

        // Optional: Trigger any action when progress is complete
        Logger.Instance.LogInfo("Selection Complete!");
        ResetCircle();
    }

    private void ResetCircle()
    {
        Image circleImage = circleUI.GetComponent<Image>();
        circleImage.fillAmount = 0f;
        circleUI.gameObject.SetActive(false);
    }
}
