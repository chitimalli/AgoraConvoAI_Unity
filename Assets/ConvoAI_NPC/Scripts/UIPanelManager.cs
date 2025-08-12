using UnityEngine;
using System.Collections;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinConvoAIChannelAudio
{
    /// <summary>
    /// Direction for UI panel sliding animation
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Standalone UI Panel Manager that works independently of NPC visibility
    /// Attach this to a persistent GameObject (like Canvas or UI Manager)
    /// </summary>
    public class UIPanelManager : MonoBehaviour
    {
        [Header("UI Panel Settings")]
        [SerializeField]
        [Tooltip("UI Panel to control (drag your panel here)")]
        private RectTransform _uiPanel;
        
        [SerializeField]
        [Tooltip("Direction to slide the UI panel when closing")]
        private SlideDirection _slideDirection = SlideDirection.Left;
        
        [SerializeField]
        [Tooltip("Animation duration for UI panel sliding")]
        private float _uiAnimationDuration = 0.3f;
        
        [SerializeField]
        [Tooltip("Animation curve for UI panel sliding")]
        private AnimationCurve _uiAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [SerializeField]
        [Tooltip("Show debug logs for UI panel operations")]
        private bool _enableDebugLogs = true;

        // UI Panel state tracking
        private bool _isUIPanelOpen = true;
        private Vector2 _originalUIPosition;
        private Vector2 _closedUIPosition;
        private Coroutine _uiAnimationCoroutine;
        private bool _isUIAnimating = false;
        private bool _isInitialized = false;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeUIPanel();
        }

        private void OnValidate()
        {
            // Ensure animation duration is reasonable
            if (_uiAnimationDuration < 0.1f)
                _uiAnimationDuration = 0.1f;
        }

        #endregion

        #region UI Panel Management

        /// <summary>
        /// Initialize the UI panel settings
        /// </summary>
        private void InitializeUIPanel()
        {
            if (_uiPanel != null)
            {
                // Store the original position
                _originalUIPosition = _uiPanel.anchoredPosition;
                
                // Calculate the closed position based on slide direction
                CalculateClosedPosition();
                
                // Start with panel open
                _isUIPanelOpen = true;
                _isInitialized = true;
                
                LogDebug($"UI Panel initialized - Original position: {_originalUIPosition}, Closed position: {_closedUIPosition}");
            }
            else
            {
                LogDebug("Warning: No UI Panel assigned in UIPanelManager");
            }
        }
        
        /// <summary>
        /// Calculate the closed position based on slide direction and panel size
        /// </summary>
        private void CalculateClosedPosition()
        {
            if (_uiPanel == null) return;
            
            Vector2 panelSize = _uiPanel.rect.size;
            Canvas canvas = _uiPanel.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas?.GetComponent<RectTransform>();
            
            if (canvasRect == null)
            {
                LogDebug("Warning: Could not find Canvas for UI panel size calculation");
                return;
            }
            
            Vector2 canvasSize = canvasRect.rect.size;
            
            switch (_slideDirection)
            {
                case SlideDirection.Left:
                    _closedUIPosition = new Vector2(-panelSize.x - 50f, _originalUIPosition.y);
                    break;
                case SlideDirection.Right:
                    _closedUIPosition = new Vector2(canvasSize.x + 50f, _originalUIPosition.y);
                    break;
                case SlideDirection.Up:
                    _closedUIPosition = new Vector2(_originalUIPosition.x, canvasSize.y + 50f);
                    break;
                case SlideDirection.Down:
                    _closedUIPosition = new Vector2(_originalUIPosition.x, -panelSize.y - 50f);
                    break;
            }
        }
        
        /// <summary>
        /// Toggle the UI panel open/closed state
        /// </summary>
        public void ToggleUIPanel()
        {
            if (!_isInitialized)
            {
                InitializeUIPanel();
            }
            
            if (_uiPanel == null)
            {
                LogDebug("Warning: No UI Panel assigned to toggle");
                return;
            }
            
            if (_isUIAnimating)
            {
                LogDebug("UI Panel animation already in progress, ignoring toggle request");
                return;
            }
            
            SetUIPanelOpen(!_isUIPanelOpen);
        }
        
        /// <summary>
        /// Open the UI panel
        /// </summary>
        public void OpenUIPanel()
        {
            SetUIPanelOpen(true);
        }
        
        /// <summary>
        /// Close the UI panel
        /// </summary>
        public void CloseUIPanel()
        {
            SetUIPanelOpen(false);
        }
        
        /// <summary>
        /// Set the UI panel open/closed state with animation
        /// </summary>
        /// <param name="open">True to open, false to close</param>
        public void SetUIPanelOpen(bool open)
        {
            if (!_isInitialized)
            {
                InitializeUIPanel();
            }
            
            if (_uiPanel == null)
            {
                LogDebug("Warning: No UI Panel assigned");
                return;
            }
            
            if (_isUIAnimating)
            {
                LogDebug("UI Panel animation already in progress");
                return;
            }
            
            if (_isUIPanelOpen == open)
            {
                LogDebug($"UI Panel already {(open ? "open" : "closed")}");
                return;
            }
            
            _isUIPanelOpen = open;
            
            // Stop any existing animation
            if (_uiAnimationCoroutine != null)
            {
                StopCoroutine(_uiAnimationCoroutine);
                _uiAnimationCoroutine = null;
            }
            
            // Start new animation
            _uiAnimationCoroutine = StartCoroutine(AnimateUIPanel(open));
            
            LogDebug($"UI Panel animation started: {(open ? "Opening" : "Closing")}");
        }
        
        /// <summary>
        /// Animate the UI panel sliding in/out
        /// </summary>
        /// <param name="opening">True if opening, false if closing</param>
        private IEnumerator AnimateUIPanel(bool opening)
        {
            _isUIAnimating = true;
            
            Vector2 startPosition = _uiPanel.anchoredPosition;
            Vector2 targetPosition = opening ? _originalUIPosition : _closedUIPosition;
            
            float elapsed = 0f;
            
            while (elapsed < _uiAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _uiAnimationDuration;
                
                // Apply animation curve
                float curveProgress = _uiAnimationCurve.Evaluate(progress);
                
                // Interpolate position
                Vector2 currentPosition = Vector2.Lerp(startPosition, targetPosition, curveProgress);
                _uiPanel.anchoredPosition = currentPosition;
                
                yield return null;
            }
            
            // Ensure final position is exact
            _uiPanel.anchoredPosition = targetPosition;
            
            _isUIAnimating = false;
            _uiAnimationCoroutine = null;
            
            LogDebug($"UI Panel animation completed: {(opening ? "Opened" : "Closed")}");
        }
        
        /// <summary>
        /// Check if the UI panel is currently open
        /// </summary>
        /// <returns>True if open, false if closed</returns>
        public bool IsUIPanelOpen()
        {
            return _isUIPanelOpen;
        }
        
        /// <summary>
        /// Check if the UI panel is currently animating
        /// </summary>
        /// <returns>True if animating, false if stationary</returns>
        public bool IsUIPanelAnimating()
        {
            return _isUIAnimating;
        }
        
        /// <summary>
        /// Set the UI panel reference (useful for runtime assignment)
        /// </summary>
        /// <param name="panel">The UI panel RectTransform</param>
        public void SetUIPanel(RectTransform panel)
        {
            _uiPanel = panel;
            if (_uiPanel != null)
            {
                InitializeUIPanel();
                LogDebug($"UI Panel set to: {_uiPanel.name}");
            }
        }
        
        /// <summary>
        /// Reset the UI panel to its original position
        /// </summary>
        public void ResetUIPanel()
        {
            if (_uiPanel != null && _isInitialized)
            {
                _uiPanel.anchoredPosition = _originalUIPosition;
                _isUIPanelOpen = true;
                
                // Stop any running animation
                if (_uiAnimationCoroutine != null)
                {
                    StopCoroutine(_uiAnimationCoroutine);
                    _uiAnimationCoroutine = null;
                    _isUIAnimating = false;
                }
                
                LogDebug("UI Panel reset to original position");
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Log debug message if debug logging is enabled
        /// </summary>
        /// <param name="message">Message to log</param>
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[UIPanelManager - {gameObject.name}] {message}");
            }
        }
        
        /// <summary>
        /// Get a singleton instance of UIPanelManager (creates one if none exists)
        /// </summary>
        /// <returns>UIPanelManager instance</returns>
        public static UIPanelManager GetInstance()
        {
            UIPanelManager instance = FindObjectOfType<UIPanelManager>();
            
            if (instance == null)
            {
                GameObject managerObj = new GameObject("UIPanelManager");
                instance = managerObj.AddComponent<UIPanelManager>();
                Debug.Log("[UIPanelManager] Created new UIPanelManager instance");
            }
            
            return instance;
        }

        #endregion
    }
}
