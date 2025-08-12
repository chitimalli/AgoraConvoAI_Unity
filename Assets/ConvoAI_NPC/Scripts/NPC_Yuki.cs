using UnityEngine;
using System.Collections;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinConvoAIChannelAudio
{
    /// <summary>
    /// Simplified NPC_Yuki component that only handles visual representation
    /// Works with NPCStatusMonitor for status updates and UIPanelManager for UI control
    /// Can be safely disabled/enabled without breaking functionality
    /// </summary>
    public class NPC_Yuki : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField]
        [Tooltip("Animation duration for show/hide transitions")]
        private float _transitionDuration = 0.5f;
        
        [SerializeField]
        [Tooltip("Scale when NPC is active")]
        private Vector3 _activeScale = Vector3.one;
        
        [SerializeField]
        [Tooltip("Scale when NPC is inactive")]
        private Vector3 _inactiveScale = Vector3.zero;
        
        [SerializeField]
        [Tooltip("Show debug logs for NPC status changes")]
        private bool _enableDebugLogs = true;

        // Internal state
        private bool _isCurrentlyShown = false;
        private Coroutine _transitionCoroutine;

        private void Awake()
        {
            // Initialize scale to inactive (hidden) by default
            transform.localScale = _inactiveScale;
            _isCurrentlyShown = false;
        }

        /// <summary>
        /// Shows the NPC with smooth scaling animation
        /// Called by NPCStatusMonitor when agent becomes active
        /// </summary>
        public void ShowNPC()
        {
            if (_isCurrentlyShown) return;
            
            _isCurrentlyShown = true;
            LogDebug("Showing NPC");
            
            // Stop any existing transition
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            // Start show animation
            _transitionCoroutine = StartCoroutine(AnimateScale(_activeScale));
        }

        /// <summary>
        /// Hides the NPC with smooth scaling animation
        /// Called by NPCStatusMonitor when agent becomes inactive
        /// </summary>
        public void HideNPC()
        {
            if (!_isCurrentlyShown) return;
            
            _isCurrentlyShown = false;
            LogDebug("Hiding NPC");
            
            // Stop any existing transition
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            // Start hide animation
            _transitionCoroutine = StartCoroutine(AnimateScale(_inactiveScale));
        }

        /// <summary>
        /// Smoothly animates the NPC's scale
        /// </summary>
        private IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _transitionDuration;
                
                // Use smooth curve for natural animation
                progress = Mathf.SmoothStep(0f, 1f, progress);
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }

            transform.localScale = targetScale;
            _transitionCoroutine = null;
        }

        /// <summary>
        /// Gets whether the NPC is currently shown
        /// </summary>
        public bool IsShown()
        {
            return _isCurrentlyShown;
        }

        /// <summary>
        /// Called by NPCStatusMonitor when ConvoAI agent status changes
        /// This is the bridge method that connects the external monitoring to the visual NPC
        /// </summary>
        /// <param name="isActive">Whether the agent is active</param>
        /// <param name="agentId">The agent ID (can be null)</param>
        public void OnAgentStatusChanged(bool isActive, string agentId)
        {
            LogDebug($"Agent status changed - Active: {isActive}, Agent ID: {agentId ?? "null"}");
            
            if (isActive)
            {
                ShowNPC();
            }
            else
            {
                HideNPC();
            }
        }

        /// <summary>
        /// Logs debug messages if debug logging is enabled
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[NPC_Yuki] {message}");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor validation to ensure reasonable values
        /// </summary>
        private void OnValidate()
        {
            if (_transitionDuration < 0.1f)
                _transitionDuration = 0.1f;
                
            if (_transitionDuration > 5.0f)
                _transitionDuration = 5.0f;
        }
#endif
    }
}
