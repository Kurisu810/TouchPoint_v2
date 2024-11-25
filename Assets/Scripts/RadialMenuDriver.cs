using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using Oculus.Interaction;

public class RadialMenuDriver : MonoBehaviour
{
    [Range(2, 10)]
    public int numberOfRadialSectors = 4; // Ensure there are 4 sectors
    public GameObject radialSectorPrefab;
    public Transform radialMenuCanvas;
    public float angleBetweenSector = 10;

    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public Transform centerEyeTransform;

    public GameObject timerRayInteractorLeft;
    public GameObject timerRayInteractorRight;

    public GameObject progressCircle;

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
    public float deadzone = 0.02f;

    [Range(0.5f, 5f)]
    public float radialMenuDistance = 1.2f;

    public UnityEvent<int> onSectorSelection;

    private List<GameObject> spawnedSectors = new List<GameObject>();
    private int currentSelectedSectorIndex = -1;
    private Vector3 startingHandPosition;
    private bool pointAndWaitMode = false;
    private bool pointAndPinchMode = false;
    private Transform handTransform = null;

    public Renderer lampShadeRenderer; // Reference to the lamp shade's Renderer
    public Light directionalLight; // Reference to the directional light

    private readonly Color[] sectorColors =
    {
        new Color(0.9f, 0.9f, 0.9f), // Light Gray
        new Color(1.0f, 0.6f, 0.6f), // #FF9999 (Light Red)
        new Color(0.6f, 1.0f, 1.0f), // #99FFFF (Light Blue)
        new Color(1.0f, 0.8f, 0.6f)  // #FFCC99 (Light Yellow)
    };

    void Awake()
    {
        PinchActiveStateLeft = _pinchActiveStateLeft as IActiveState;
        PinchActiveStateRight = _pinchActiveStateRight as IActiveState;

        TimerActiveStateLeft = _timerActiveStateLeft as IActiveState;
        TimerActiveStateRight = _timerActiveStateRight as IActiveState;

        PointActiveStateLeft = _pointActiveStateLeft as IActiveState;
        PointActiveStateRight = _pointActiveStateRight as IActiveState;
    }

    void Update()
    {
        // Select active mode and hand
        if (handTransform == null && !pointAndWaitMode && !pointAndPinchMode)
        {
            if (PinchActiveStateLeft.Active)
            {
                handTransform = leftHandTransform;
                pointAndPinchMode = true;
            }
            else if (PinchActiveStateRight.Active)
            {
                handTransform = rightHandTransform;
                pointAndPinchMode = true;
            }
            else if (TimerActiveStateLeft.Active && PointActiveStateLeft.Active)
            {
                handTransform = leftHandTransform;
                pointAndWaitMode = true;
            }
            else if (TimerActiveStateRight.Active && PointActiveStateRight.Active)
            {
                handTransform = rightHandTransform;
                pointAndWaitMode = true;
            }
            else
            {
                return;
            }
            
            timerRayInteractorLeft.SetActive(false);
            timerRayInteractorRight.SetActive(false);
            progressCircle.SetActive(false);
            startingHandPosition = handTransform.position;
            SpawnRadialMenu();
        }
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
                progressCircle.SetActive(true);
                HideAndTriggerSelected();
            }
        }
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
                progressCircle.SetActive(true);
                HideAndTriggerSelected();
            }
        }
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
                progressCircle.SetActive(true);
                HideAndTriggerSelected();
            }
        }
    }

    public void SpawnRadialMenu()
    {
        radialMenuCanvas.gameObject.SetActive(true);

        // Set menu position at spawn time, fixed relative to centerEyeTransform
        Vector3 targetPosition = centerEyeTransform.position + centerEyeTransform.forward * radialMenuDistance;
        radialMenuCanvas.position = targetPosition;
        radialMenuCanvas.rotation = centerEyeTransform.rotation;

        foreach (var item in spawnedSectors)
        {
            Destroy(item);
        }

        spawnedSectors.Clear();

        for (int i = 0; i < numberOfRadialSectors; i++)
        {
            GameObject radialSector = Instantiate(radialSectorPrefab, radialMenuCanvas);
            spawnedSectors.Add(radialSector);

            float angle = -i * 360 / numberOfRadialSectors - angleBetweenSector / 2;
            Vector3 radialSectorEulerAngles = new Vector3(0, 0, angle);

            radialSector.transform.position = radialMenuCanvas.position;
            radialSector.transform.localEulerAngles = radialSectorEulerAngles;
            radialSector.GetComponent<Image>().fillAmount = 1 / (float)numberOfRadialSectors - (angleBetweenSector / 360);

            // Set sector color
            radialSector.GetComponent<Image>().color = sectorColors[i];
        }
    }

    public void GetSelectedSector()
    {
        Vector3 currentHandPosition = handTransform.position;
        Vector3 handPositionDelta = currentHandPosition - startingHandPosition;
        handPositionDelta.z = 0;

        float angleFrom12 = 90 - Mathf.Atan2(handPositionDelta.y, handPositionDelta.x) * Mathf.Rad2Deg;
        if (angleFrom12 < 0) { angleFrom12 += 360; }

        float XYMovementDelta = handPositionDelta.magnitude;

        if (XYMovementDelta > deadzone)
        {
            currentSelectedSectorIndex = (int)(numberOfRadialSectors * (angleFrom12 / 360));

            for (int i = 0; i < spawnedSectors.Count; i++)
            {
                // Highlight the selected sector by scaling it to 1.5 times
                if (i == currentSelectedSectorIndex)
                {
                    spawnedSectors[i].transform.localScale = 1.5f * Vector3.one;
                }
                else
                {
                    // Reset the size of unselected sectors
                    spawnedSectors[i].transform.localScale = Vector3.one;
                }
            }
        }
        else
        {
            currentSelectedSectorIndex = -1;

            // Reset all sectors if nothing is selected
            foreach (var sector in spawnedSectors)
            {
                sector.transform.localScale = Vector3.one;
            }
        }
    }

    public void HideAndTriggerSelected()
    {
        onSectorSelection.Invoke(currentSelectedSectorIndex);

        AdjustLampShadeColor(currentSelectedSectorIndex);

        radialMenuCanvas.gameObject.SetActive(false);
    }

    private void AdjustLampShadeColor(int sectorIndex)
    {
        if (sectorIndex >= 0 && sectorIndex < sectorColors.Length)
        {
            Color selectedColor = sectorColors[sectorIndex];
            lampShadeRenderer.material.color = selectedColor; // Update lamp shade color
            directionalLight.color = selectedColor;           // Update directional light color
            RenderSettings.ambientLight = selectedColor;      // Update ambient light color
        }
    }
}
