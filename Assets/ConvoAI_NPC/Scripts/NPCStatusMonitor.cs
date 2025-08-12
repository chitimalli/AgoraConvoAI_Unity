using UnityEngine;
using System.Collections;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinConvoAIChannelAudio
{
    /// <summary>
    /// Persistent NPC Status Monitor that runs independently of NPC visibility
    /// Attach this to a persistent GameObject (like Canvas, UI Manager, or dedicated Manager)
    /// This ensures ConvoAI monitoring continues even when NPCs are hidden
    /// </summary>
    public class NPCStatusMonitor : MonoBehaviour
    {
        [Header("ConvoAI Configuration")]
        [SerializeField]
        [Tooltip("Reference to the JoinChannelAudio component that manages ConvoAI")]
        private JoinConvoAIChannelAudio _convoAIManager;
        
        [SerializeField]
        [Tooltip("How often to check the agent status (in seconds)")]
        private float _statusCheckInterval = 1.0f;
        
        [SerializeField]
        [Tooltip("Show debug logs for status changes")]
        private bool _enableDebugLogs = true;

        // Status tracking
        private string _currentAgentId;
        private bool _hasActiveAgent = false;
        private Coroutine _statusCheckCoroutine;

        // Events for NPC components to listen to
        public System.Action<bool, string> OnAgentStatusChanged;

        #region Unity Lifecycle

        private void Start()
        {
            // Find ConvoAI manager if not assigned
            if (_convoAIManager == null)
            {
                _convoAIManager = FindObjectOfType<JoinConvoAIChannelAudio>();
                if (_convoAIManager == null)
                {
                    LogDebug("Warning: No JoinChannelAudio component found in scene. Please assign one manually.");
                }
                else
                {
                    LogDebug($"Automatically found ConvoAI manager: {_convoAIManager.name}");
                }
            }

            // Start monitoring
            StartStatusMonitoring();
            
            LogDebug("NPC Status Monitor initialized and ready");
        }

        private void OnDestroy()
        {
            StopStatusMonitoring();
        }

        private void OnValidate()
        {
            // Ensure status check interval is reasonable
            if (_statusCheckInterval < 0.1f)
                _statusCheckInterval = 0.1f;
        }

        #endregion

        #region Status Monitoring

        /// <summary>
        /// Start the status monitoring coroutine
        /// </summary>
        private void StartStatusMonitoring()
        {
            StopStatusMonitoring(); // Stop any existing coroutine
            _statusCheckCoroutine = StartCoroutine(MonitorAgentStatus());
            LogDebug("Status monitoring started");
        }

        /// <summary>
        /// Stop the status monitoring coroutine
        /// </summary>
        private void StopStatusMonitoring()
        {
            if (_statusCheckCoroutine != null)
            {
                StopCoroutine(_statusCheckCoroutine);
                _statusCheckCoroutine = null;
                LogDebug("Status monitoring stopped");
            }
        }

        /// <summary>
        /// Coroutine that continuously monitors the ConvoAI agent status
        /// </summary>
        private IEnumerator MonitorAgentStatus()
        {
            while (true)
            {
                if (_convoAIManager != null)
                {
                    bool hasActiveAgent = _convoAIManager.HasActiveAgent();
                    string currentAgentId = _convoAIManager.GetCurrentAgentId();
                    
                    // Check if status changed
                    bool statusChanged = hasActiveAgent != _hasActiveAgent;
                    bool agentIdChanged = currentAgentId != _currentAgentId;
                    
                    if (statusChanged || agentIdChanged)
                    {
                        LogDebug($"Agent status changed - Active: {hasActiveAgent}, Agent ID: {currentAgentId ?? "null"}");
                        
                        // Update internal state
                        _hasActiveAgent = hasActiveAgent;
                        _currentAgentId = currentAgentId;
                        
                        // Notify all NPC components
                        NotifyNPCComponents(hasActiveAgent, currentAgentId);
                        
                        // Trigger event for other systems
                        OnAgentStatusChanged?.Invoke(hasActiveAgent, currentAgentId);
                    }
                }
                
                yield return new WaitForSeconds(_statusCheckInterval);
            }
        }

        /// <summary>
        /// Notify all NPC_Yuki components in the scene about status changes
        /// </summary>
        /// <param name="isActive">Whether an agent is active</param>
        /// <param name="agentId">The agent ID (null if no active agent)</param>
        private void NotifyNPCComponents(bool isActive, string agentId)
        {
            try
            {
                // Find all NPC_Yuki components using reflection to avoid dependency issues
                var npcComponents = FindObjectsOfType<MonoBehaviour>();
                int notifiedCount = 0;
                
                foreach (var component in npcComponents)
                {
                    if (component.GetType().Name == "NPC_Yuki")
                    {
                        // Use reflection to call OnAgentStatusChanged
                        var method = component.GetType().GetMethod("OnAgentStatusChanged");
                        if (method != null)
                        {
                            method.Invoke(component, new object[] { isActive, agentId });
                            notifiedCount++;
                        }
                    }
                }
                
                if (notifiedCount > 0)
                {
                    LogDebug($"Notified {notifiedCount} NPC component(s) - Active: {isActive}, Agent ID: {agentId ?? "null"}");
                }
            }
            catch (System.Exception ex)
            {
                LogDebug($"Error notifying NPC components: {ex.Message}");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Get the current agent ID
        /// </summary>
        /// <returns>Current agent ID or null if no active agent</returns>
        public string GetCurrentAgentId()
        {
            return _currentAgentId;
        }

        /// <summary>
        /// Check if there is an active agent
        /// </summary>
        /// <returns>True if there is an active agent</returns>
        public bool HasActiveAgent()
        {
            return _hasActiveAgent;
        }

        /// <summary>
        /// Manually set the ConvoAI manager reference
        /// </summary>
        /// <param name="manager">The JoinChannelAudio component to monitor</param>
        public void SetConvoAIManager(JoinConvoAIChannelAudio manager)
        {
            _convoAIManager = manager;
            LogDebug($"ConvoAI manager set to: {manager?.name ?? "null"}");
        }

        /// <summary>
        /// Force a status check immediately
        /// </summary>
        public void ForceStatusCheck()
        {
            if (_convoAIManager != null)
            {
                bool hasActiveAgent = _convoAIManager.HasActiveAgent();
                string currentAgentId = _convoAIManager.GetCurrentAgentId();
                
                LogDebug($"Force status check - Active: {hasActiveAgent}, Agent ID: {currentAgentId ?? "null"}");
                
                // Update and notify regardless of whether it changed
                _hasActiveAgent = hasActiveAgent;
                _currentAgentId = currentAgentId;
                
                NotifyNPCComponents(hasActiveAgent, currentAgentId);
                OnAgentStatusChanged?.Invoke(hasActiveAgent, currentAgentId);
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
                Debug.Log($"[NPCStatusMonitor - {gameObject.name}] {message}");
            }
        }
        
        /// <summary>
        /// Get a singleton instance of NPCStatusMonitor (creates one if none exists)
        /// </summary>
        /// <returns>NPCStatusMonitor instance</returns>
        public static NPCStatusMonitor GetInstance()
        {
            NPCStatusMonitor instance = FindObjectOfType<NPCStatusMonitor>();
            
            if (instance == null)
            {
                GameObject managerObj = new GameObject("NPCStatusMonitor");
                instance = managerObj.AddComponent<NPCStatusMonitor>();
                Debug.Log("[NPCStatusMonitor] Created new NPCStatusMonitor instance");
            }
            
            return instance;
        }

        #endregion
    }
}
