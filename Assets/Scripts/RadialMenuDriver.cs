using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;

public class RadialMenuDriver : MonoBehaviour
{   
    [Range(2, 10)]
    public int numberOfRadialSectors = 3;
    public GameObject radialSectorPrefab; 
    public Transform radialMenuCanvas;
    public float angleBetweenSector = 10;

    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public Transform centerEyeTransform;

    public GameObject timerRayInteractorLeft;
    public GameObject timerRayInteractorRight;

    [SerializeField, Interface(typeof(IActiveState))]
    private UnityEngine.Object _pinchActiveStateLeft;
    private IActiveState PinchActiveStateLeft;

    [SerializeField, Interface(typeof(IActiveState))]
    private UnityEngine.Object _pinchActiveStateRight;
    private IActiveState PinchActiveStateRight;

    [SerializeField, Interface(typeof(IActiveState))]
    private UnityEngine.Object _timerActiveStateLeft;
    private IActiveState TimerActiveStateLeft;

    [SerializeField, Interface(typeof(IActiveState))]
    private UnityEngine.Object _timerActiveStateRight;
    private IActiveState TimerActiveStateRight;

    [SerializeField, Interface(typeof(IActiveState))]
    private UnityEngine.Object _pointActiveStateLeft;
    private IActiveState PointActiveStateLeft;

    [SerializeField, Interface(typeof(IActiveState))]
    private UnityEngine.Object _pointActiveStateRight;
    private IActiveState PointActiveStateRight;

    [Range(0.01f, 0.1f)]
    public float deadzone = 0.02f; // Deadzone threshold

    public UnityEvent<int> onSectorSelection;

    private List<GameObject> spawnedSectors = new List<GameObject>();
    private int currentSelectedSectorIndex = -1;
    private Vector3 startingHandPosition; // Snapshotting the position of the hand at summon
    private bool pointAndWaitMode = false;
    private bool pointAndPinchMode = false;
    private Transform handTransform = null; // Indicates which hand initiated the interaction; if null, then no interaction is active right now

    void Awake()
    {
        PinchActiveStateLeft  = _pinchActiveStateLeft  as IActiveState;
        PinchActiveStateRight = _pinchActiveStateRight as IActiveState;

        TimerActiveStateLeft  = _timerActiveStateLeft  as IActiveState;
        TimerActiveStateRight = _timerActiveStateRight as IActiveState;

        PointActiveStateLeft  = _pointActiveStateLeft  as IActiveState;
        PointActiveStateRight = _pointActiveStateRight as IActiveState;
    }

    // Update is called once per frame
    void Update()
    {
        // Select active mode and hand
        if (handTransform == null && !pointAndWaitMode && !pointAndPinchMode)
        {
            // Left hand point and pinch mode
            if (PinchActiveStateLeft.Active) {
                handTransform = leftHandTransform;
                pointAndPinchMode = true;
            }

            // Right hand point and pinch mode
            else if (PinchActiveStateRight.Active) {
                handTransform = rightHandTransform;
                pointAndPinchMode = true;
            }

            // Left hand point and wait active
            else if (TimerActiveStateLeft.Active && PointActiveStateLeft.Active)
            {
                handTransform = leftHandTransform;
                timerRayInteractorLeft.SetActive(false);
                timerRayInteractorRight.SetActive(false);
                pointAndWaitMode = true;
            }

            // Right hand point and wait active
            else if (TimerActiveStateRight.Active && PointActiveStateRight.Active)
            {
                handTransform = rightHandTransform;
                timerRayInteractorLeft.SetActive(false);
                timerRayInteractorRight.SetActive(false);
                pointAndWaitMode = true;
            }

            else
            {
                return;
            }

            startingHandPosition = handTransform.position; // Store starting hand position
            SpawnRadialMenu();
        }

        // Point and wait mode, left hand
        else if (handTransform == leftHandTransform && pointAndWaitMode)
        {
            if (!TimerActiveStateLeft.Active && PointActiveStateLeft.Active)
            {
                GetSelectedSector();
            }
            else
            {
                pointAndWaitMode = false;
                handTransform = null;
                HideAndTriggerSelected();
            }
        }

        // Point and wait mode, right hand
        else if (handTransform == rightHandTransform && pointAndWaitMode)
        {
            if (!TimerActiveStateRight.Active && PointActiveStateRight.Active)
            {
                GetSelectedSector();
            }
            else
            {
                pointAndWaitMode = false;
                handTransform = null;
                HideAndTriggerSelected();
            }
        }

        // Point and pinch mode
        else if (handTransform != null && pointAndPinchMode)
        {
            if (PinchActiveStateLeft.Active || PinchActiveStateRight.Active)
            {
                GetSelectedSector();
            }
            else
            {
                pointAndPinchMode = false;
                handTransform = null;
                HideAndTriggerSelected();
            }
        }
    }

    public void SpawnRadialMenu()
    {   
        // Enable radial menu game object
        radialMenuCanvas.gameObject.SetActive(true);

        // Set radial menu position and rotation to in front of the player
        Vector3 targetPosition = centerEyeTransform.position;
        targetPosition.z += 1; // Move the radial menu to in front of the player
        radialMenuCanvas.position = targetPosition;
        radialMenuCanvas.rotation = centerEyeTransform.rotation;

        // Clear spawned sectors list
        foreach (var item in spawnedSectors)
        {
            Destroy(item);
        }

        spawnedSectors.Clear();

        // Spawn each radial sector
        for (int i = 0; i < numberOfRadialSectors; i++)
        {
            // Instantiate new radial sector
            GameObject radialSector = Instantiate(radialSectorPrefab, radialMenuCanvas);
            spawnedSectors.Add(radialSector); // Add to spawned sectors list

            // Compute rotation angle
            float angle = -i * 360 / numberOfRadialSectors - angleBetweenSector/2;
            Vector3 radialSectorEulerAngles = new Vector3(0, 0, angle);

            // Configure radial sector
            radialSector.transform.position = radialMenuCanvas.position;  // Set position to center of canvas
            radialSector.transform.localEulerAngles = radialSectorEulerAngles;  // Set rotation angles
            radialSector.GetComponent<Image>().fillAmount = 1 / (float) numberOfRadialSectors - (angleBetweenSector/360);  // Set fill amount
        }
    }

    public void GetSelectedSector()
    {   
        // Get current hand position
        Vector3 currentHandPosition = handTransform.position;

        // Get a vector from current to starting hand position
        Vector3 handPositionDelta = currentHandPosition - startingHandPosition;
        handPositionDelta.z = 0; // Ignore Z differences

        // Calculate the clock face angle from starting hand position
        float angleFrom12 = 90 - Mathf.Atan2(handPositionDelta.y, handPositionDelta.x) * Mathf.Rad2Deg;
        if (angleFrom12 < 0) {angleFrom12 += 360;} // Translate the angle to between 0 to 360

        // Check deadzone threshold
        float XYMovementDelta = handPositionDelta.magnitude;

        if (XYMovementDelta > deadzone) {
            // Determine the selected sector
            currentSelectedSectorIndex = (int) (numberOfRadialSectors * (angleFrom12 / 360));

            for (int i = 0; i < spawnedSectors.Count; i++)
            {   
                // Set the selected sector's color to yellow and slightly increase its size
                if (i == currentSelectedSectorIndex)
                {
                    spawnedSectors[i].GetComponent<Image>().color = Color.yellow;
                    spawnedSectors[i].transform.localScale = 1.1f * Vector3.one;
                }
                
                // Reset all sectors not selected
                else
                {
                    spawnedSectors[i].GetComponent<Image>().color = Color.white;
                    spawnedSectors[i].transform.localScale = Vector3.one;
                }
            }
        }

        else
        {
            currentSelectedSectorIndex = -1;
        }
    }

    public void HideAndTriggerSelected()
    {
        // Trigger selected radial sector
        onSectorSelection.Invoke(currentSelectedSectorIndex);

        Logger.Instance.LogInfo("Selected sector index = " + currentSelectedSectorIndex);

        // Despawn radial menu
        radialMenuCanvas.gameObject.SetActive(false);
    }
}
