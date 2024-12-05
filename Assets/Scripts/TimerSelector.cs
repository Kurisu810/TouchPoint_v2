using Oculus.Interaction.Input;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Oculus.Interaction
{
    /// <summary>
    /// Broadcasts whether the hand is selecting or unselecting. The hand is selecting after hovering over an interactable for a minimum amount of time
    /// </summary>
    public class TimerSelector : MonoBehaviour, ISelector
    {
        [SerializeField]
        public RayInteractor Interactor;

        [SerializeField, Interface(typeof(IActiveState))]
        private UnityEngine.Object _pointActiveState;
        private IActiveState PointActiveState;

        public float selectionTime = 2f;  // Time to wait before selecting
        private float timer = 0f;         // Timer

        public event Action WhenSelected = delegate { };
        public event Action WhenUnselected = delegate { };

        private bool LastSelected = false;

        void Start()
        {
            PointActiveState = _pointActiveState as IActiveState;
        }
        void OnEnable()
        {
            timer = 0f;
        }
        void Update()
        {
            if (LastSelected)
            {
                LastSelected = false;
                WhenUnselected();
            }

            // Check if the interactor is in the hovering state
            else if (Interactor != null && Interactor.State == InteractorState.Hover && PointActiveState.Active)
            {
                timer += Time.deltaTime; // Start timer

                if (timer >= selectionTime)
                {
                    // Trigger selection if timer is reached
                    WhenSelected();
                }
            }
            else
            {
                // Reset timer if not hovering
                timer = 0f;
            }
        }
    }
}