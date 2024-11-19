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

        public float selectionTime = 2f;  // Time to wait before selecting
        private float timer = 0f;         // Timer

        public event Action WhenSelected = delegate { };
        public event Action WhenUnselected = delegate { };

        void Update()
        {
            // Check if the interactor is in the hovering state
            if (Interactor != null && Interactor.State == InteractorState.Hover)
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
                WhenUnselected();
            }
        }
    }
}