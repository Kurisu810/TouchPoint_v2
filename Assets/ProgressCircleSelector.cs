using Oculus.Interaction.Input;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Oculus.Interaction
{
    /// <summary>
    /// Selects an object after hovering for 2 seconds.
    /// </summary>
    public class ProgressCircleSelector : MonoBehaviour, ISelector
    {
        [Tooltip("The hand to check.")]
        [SerializeField] private UnityEngine.Object _hand;
        public IHand Hand { get; private set; }

        private bool _isHovering;
        private float _timer;
        private const float _selectionTime = 2f; // Time to wait before selecting (2 seconds)

        public event Action WhenSelected = delegate { };
        public event Action WhenUnselected = delegate { };

        protected bool _started = false;

        protected virtual void Awake()
        {
            Hand = _hand as IHand;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(Hand, nameof(Hand));
            this.EndStart(ref _started);
        }

        // Modify this method to listen for hover events
        public void OnHover()
        {
            Logger.Instance.LogInfo("Script test: hover started");
            _isHovering = true;
            _timer = 0f; // Reset the timer when hovering starts
            // Trigger hover event if necessary
        }

        // Modify this method to handle hover stop events
        public void OnUnhover()
        {
            Logger.Instance.LogInfo("Script test: hover ended");
            _isHovering = false;
            _timer = 0f; // Reset the timer when hovering stops
            WhenUnselected();
        }

        private void Update()
        {
            if (_isHovering)
            {
                _timer += Time.deltaTime; // Increase timer while hovering

                if (_timer >= _selectionTime)
                {
                    // Trigger selection when the timer reaches 2 seconds
                    WhenSelected();
                    _isHovering = false; // Stop hovering once selected
                }
            }
        }

        #region Inject

        public void InjectAllHoverTimerSelector(IHand hand)
        {
            InjectHand(hand);
        }

        public void InjectHand(IHand hand)
        {
            _hand = hand as UnityEngine.Object;
            Hand = hand;
        }

        #endregion
    }
}
