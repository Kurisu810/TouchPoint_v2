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
    public float radialMenuDistance = 1f;

    public UnityEvent<int> onSectorSelection;

    private List<GameObject> spawnedSectors = new List<GameObject>();
    private int currentSelectedSectorIndex = -1;
    private Vector3 startingHandPosition;
    private bool pointAndWaitMode = false;
    private bool pointAndPinchMode = false;
    private Transform handTransform = null;

    public Light directionalLight; // Reference to the directional light
    private int currentColorMode = 0; // Tracks the current color mode (0 = white, 1 = red, 2 = yellow, 3 = blue)
    private readonly Color[] colorModes = { Color.white, Color.red, Color.yellow, Color.blue };

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
                timerRayInteractorLeft.SetActive(false);
                timerRayInteractorRight.SetActive(false);
                pointAndWaitMode = true;
            }
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
                if (i == currentSelectedSectorIndex)
                {
                    spawnedSectors[i].GetComponent<Image>().color = Color.yellow;
                    spawnedSectors[i].transform.localScale = 1.1f * Vector3.one;
                }
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
        onSectorSelection.Invoke(currentSelectedSectorIndex);

        Logger.Instance.LogInfo("Selected sector index = " + currentSelectedSectorIndex);

        AdjustLight(currentSelectedSectorIndex);

        radialMenuCanvas.gameObject.SetActive(false);
    }

    private void AdjustLight(int sectorIndex)
    {
        switch (sectorIndex)
        {
            case 0: // Top-right: Switch to next color mode
                currentColorMode = (currentColorMode + 1) % colorModes.Length;
                directionalLight.color = colorModes[currentColorMode];
                break;
            case 1: // Bottom-right: Switch to previous color mode
                currentColorMode = (currentColorMode - 1 + colorModes.Length) % colorModes.Length;
                directionalLight.color = colorModes[currentColorMode];
                break;
            case 2: // Bottom-left
                directionalLight.enabled = false;
                break;
            case 3: // Top-left: Disable light
                directionalLight.enabled = true;
                break;
            default:
                break;
        }
    }
}
