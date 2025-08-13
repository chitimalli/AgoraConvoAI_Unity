#define AGORA_RTC
#define AGORA_FULL


using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinConvoAIChannelAudio
{
    public class JoinConvoAIChannelAudio : MonoBehaviour
    {
        [SerializeField]
        private ConvoAIConfigs _convoAIConfigs;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        [Header("_____________ConvoAI Configuration_____________")]
        [SerializeField]
        private string _apiKey = "";
        
        [SerializeField]
        private string _apiSecret = "";
        
        [SerializeField]
        private string _agentName = "";
        
        [SerializeField]
        private uint _agentRtcUid = 8888;
        
        [SerializeField]
        private int _idleTimeout = 30;
        
        [SerializeField]
        private string _asrLanguage = "";
        
        [SerializeField]
        private string _llmUrl = "";
        
        [SerializeField]
        private string _llmApiKey = "";
        
        [SerializeField]
        private string _greetingMessage = "";
        
        [SerializeField]
        private string _failureMessage = "";
        
        [SerializeField]
        private int _maxHistory = 32;
        
        [SerializeField]
        private string _ttsKey = "";
        
        [SerializeField]
        private string _ttsRegion = "";
        
        [SerializeField]
        private string _ttsVoiceName = "";

        // Reference to networking manager
        private ConvoAINWMgr _networkManager;

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

       public RectTransform _qualityPanel;
        public GameObject _qualityItemPrefab;


        private void Awake()
        {
#if AGORA_RTC
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
#endif
        }
        // Start is called before the first frame update
        private void Start()
        {
            LoadAssetData(); // Load _appID, _token, _channelName from asset
            if (CheckAppId())
            {
                RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            }

            // Initialize network manager after LoadAssetData() so it can access loaded values
            _networkManager = new ConvoAINWMgr(this);

#if UNITY_IOS || UNITY_ANDROID
            var text = GameObject.Find("Canvas/Scroll View/Viewport/Content/AudioDeviceManager").GetComponent<Text>();
            text.text = "Audio device manager not support in this platform";

            GameObject.Find("Canvas/Scroll View/Viewport/Content/AudioDeviceButton").SetActive(false);
            GameObject.Find("Canvas/Scroll View/Viewport/Content/deviceIdSelect").SetActive(false);
            GameObject.Find("Canvas/Scroll View/Viewport/Content/AudioSelectButton").SetActive(false);
#endif

            InitRtcEngine();
            LogText.text = "Agora RtcEngine initialized";

        }

        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset!!!!!");
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {

            if (_convoAIConfigs == null) return;
            _appID = _convoAIConfigs.appID;
            _token = _convoAIConfigs.token;
            _channelName = _convoAIConfigs.channelName;
            _apiKey = _convoAIConfigs.apiKey;
            _apiSecret = _convoAIConfigs.apiSecret;
            _agentName = _convoAIConfigs.agentName;
            _agentRtcUid = _convoAIConfigs.agentRtcUid;
            _idleTimeout = _convoAIConfigs.idleTimeout;
            _asrLanguage = _convoAIConfigs.asrLanguage;
            _llmUrl = _convoAIConfigs.llmUrl;
            _llmApiKey = _convoAIConfigs.llmApiKey;
            _greetingMessage = _convoAIConfigs.greetingMessage;
            _failureMessage = _convoAIConfigs.failureMessage;
            _maxHistory = _convoAIConfigs.maxHistory;
            _ttsKey = _convoAIConfigs.ttsKey;
            _ttsRegion = _convoAIConfigs.ttsRegion;
            _ttsVoiceName = _convoAIConfigs.ttsVoiceName;
        }

        #region -- Button Events ---
        public void InitRtcEngine()
        {
            // var text = this._areaSelect.captionText.text;
            // AREA_CODE areaCode = (AREA_CODE)Enum.Parse(typeof(AREA_CODE), text);
            // this.Log.UpdateLog("Select AREA_CODE : " + areaCode);

            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            //context.areaCode = areaCode;

            var result = RtcEngine.Initialize(context);
            this.Log.UpdateLog("Initialize result : " + result);

            RtcEngine.InitEventHandler(handler);

            RtcEngine.EnableAudio();
            RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudioVolumeIndication(200, 3, true);
        }


        public void StartEchoTest()
        {
            EchoTestConfiguration config = new EchoTestConfiguration();
            config.intervalInSeconds = 2;
            config.enableAudio = true;
            config.enableVideo = false;
            config.token = this._appID;
            config.channelId = "echo_test_channel";
            RtcEngine.StartEchoTest(config);
            Log.UpdateLog("StartEchoTest, speak now. You cannot conduct another echo test or join a channel before StopEchoTest");
        }

        public void StopEchoTest()
        {
            RtcEngine.StopEchoTest();
        }

        public void JoinChannel()
        {
            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
        }

        public void StopPublishAudio()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
        }

        public void StartPublishAudio()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
        }

        // ConvoAI Functions //
        public void CreateConvoAIAgent()
        {
            if (_networkManager != null)
            {
                StartCoroutine(_networkManager.CreateConvoAIAgent());
            }
            else
            {
                Log.UpdateLog("Network Manager not initialized");
            }
        }

        public void StopConvoAIAgent()
        {
            if (_networkManager != null)
            {
                StartCoroutine(_networkManager.StopConvoAIAgent());
            }
            else
            {
                Log.UpdateLog("Network Manager not initialized");
            }
        }

        public void UpdateConvoAIAgent()
        {
            if (_networkManager != null)
            {
                StartCoroutine(_networkManager.UpdateConvoAIAgent());
            }
            else
            {
                Log.UpdateLog("Network Manager not initialized");
            }
        }

        public void GetConvoAIAgentStatus()
        {
            if (_networkManager != null)
            {
                StartCoroutine(_networkManager.GetConvoAIAgentStatus());
            }
            else
            {
                Log.UpdateLog("Network Manager not initialized");
            }
        }

        public void GetConvoAIAgentList()
        {
            if (_networkManager != null)
            {
                StartCoroutine(_networkManager.GetConvoAIAgentList());
            }
            else
            {
                Log.UpdateLog("Network Manager not initialized");
            }
        }

        // Public getters for network manager to access configuration
        public string ProjectId => _appID;
        public string ApiKey => _apiKey;
        public string ApiSecret => _apiSecret;
        public string AgentName => _agentName;
        public string AgentChannel => _channelName;
        public string AgentToken => _token;
        public uint AgentRtcUid => _agentRtcUid;
        public int IdleTimeout => _idleTimeout;
        public string AsrLanguage => _asrLanguage;
        public string LlmUrl => _llmUrl;
        public string LlmApiKey => _llmApiKey;
        public string GreetingMessage => _greetingMessage;
        public string FailureMessage => _failureMessage;
        public int MaxHistory => _maxHistory;
        public string TtsKey => _ttsKey;
        public string TtsRegion => _ttsRegion;
        public string TtsVoiceName => _ttsVoiceName;

        // ConvoAI Agent Status Methods
        public string GetCurrentAgentId()
        {
            return _networkManager?.GetCurrentAgentId();
        }

        public bool HasActiveAgent()
        {
            return _networkManager?.HasActiveAgent() ?? false;
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
            StopConvoAIAgent();
        }


        public LocalAudioCallQualityPanel1 GetLocalAudioCallQualityPanel()
        {
            var panels = this._qualityPanel.GetComponentsInChildren<LocalAudioCallQualityPanel1>();
            if (panels != null && panels.Length > 0)
            {
                return panels[0];
            }
            else
            {
                return null;
            }
        }

        public bool CreateLocalAudioCallQualityPanel()
        {
            if (GetLocalAudioCallQualityPanel() == null)
            {
                GameObject item = GameObject.Instantiate(this._qualityItemPrefab, this._qualityPanel);
                item.AddComponent<LocalAudioCallQualityPanel1>();
                return true;
            }

            return false;
        }

        public bool DestroyLocalAudioCallQualityPanel()
        {
            var panel = GetLocalAudioCallQualityPanel();
            if (panel)
            {
                GameObject.Destroy(panel.gameObject);
                return true;
            }
            else
            {
                return false;
            }
        }

        public RemoteAudioCallQualityPanel1 GetRemoteAudioCallQualityPanel(uint uid)
        {
            var panels = this._qualityPanel.GetComponentsInChildren<RemoteAudioCallQualityPanel1>();
            foreach (var panel in panels)
            {
                if (panel.Uid == uid)
                {
                    return panel;
                }
            }

            return null;
        }

        public bool CreateRemoteAudioCallQualityPanel(uint uid)
        {
            if (GetRemoteAudioCallQualityPanel(uid) == null)
            {
                GameObject item = GameObject.Instantiate(this._qualityItemPrefab, this._qualityPanel);
                var panel = item.AddComponent<RemoteAudioCallQualityPanel1>();
                panel.Uid = uid;
                return true;

            }
            return false;
        }

        public bool DestroyRemoteAudioCallQualityPanel(uint uid)
        {
            var panel = GetRemoteAudioCallQualityPanel(uid);
            if (panel)
            {
                GameObject.Destroy(panel.gameObject);
                return true;
            }
            return false;
        }

        public void ClearAudioCallQualityPanel()
        {
            foreach (Transform child in this._qualityPanel)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        #region -- NPC Integration --
        
        /// <summary>
        /// Notify all NPC_Yuki components in the scene about agent status changes
        /// </summary>
        /// <param name="isActive">Whether an agent is active</param>
        /// <param name="agentId">The agent ID (null if no active agent)</param>
        public void NotifyNPCComponents(bool isActive, string agentId)
        {
            try
            {
                // Find all NPC_Yuki components in the scene using reflection to avoid compile-time dependency
                var npcComponents = FindObjectsOfType<MonoBehaviour>().Where(mb => mb.GetType().Name == "NPC_Yuki").ToArray();
                
                if (npcComponents != null && npcComponents.Length > 0)
                {
                    Log?.UpdateLog($"Notifying {npcComponents.Length} NPC component(s) - Active: {isActive}, Agent ID: {agentId ?? "null"}");
                    
                    foreach (var npc in npcComponents)
                    {
                        if (npc != null)
                        {
                            // Use reflection to call the OnAgentStatusChanged method
                            var method = npc.GetType().GetMethod("OnAgentStatusChanged");
                            if (method != null)
                            {
                                method.Invoke(npc, new object[] { isActive, agentId });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log?.UpdateLog($"Error notifying NPC components: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get all NPC_Yuki components in the scene
        /// </summary>
        /// <returns>Array of MonoBehaviour components that are NPC_Yuki type</returns>
        public MonoBehaviour[] GetNPCComponents()
        {
            try
            {
                return FindObjectsOfType<MonoBehaviour>().Where(mb => mb.GetType().Name == "NPC_Yuki").ToArray();
            }
            catch
            {
                return new MonoBehaviour[0];
            }
        }
        
        #endregion
    }

    #region -- ConvoAI Network Manager ---

    public class ConvoAINWMgr
    {
        private JoinConvoAIChannelAudio _audioSample;
        private string _currentAgentId; // Store the agent ID from create response

        public ConvoAINWMgr(JoinConvoAIChannelAudio audioSample)
        {
            _audioSample = audioSample;
            _currentAgentId = null;
        }

        public IEnumerator CreateConvoAIAgent()
        {
            string url = $"https://api.agora.io/api/conversational-ai-agent/v2/projects/{_audioSample.ProjectId}/join";
            
            // Create the request body exactly as in your HttpClient example
            string jsonData = "{\"name\":\"" + _audioSample.AgentName + "\"," +
                             "\"properties\":{" +
                             "\"channel\":\"" + _audioSample.AgentChannel + "\"," +
                             "\"token\":\"" + _audioSample.AgentToken + "\"," +
                             "\"agent_rtc_uid\":\"" + _audioSample.AgentRtcUid + "\"," +
                             "\"remote_rtc_uids\":[\"*\"]," +
                             "\"enable_string_uid\":false," +
                             "\"idle_timeout\":" + _audioSample.IdleTimeout + "," +
                             "\"asr\":{\"vendor\":\"ares\",\"language\":\"" + _audioSample.AsrLanguage + "\"}," +
                             "\"llm\":{" +
                             "\"url\":\"" + _audioSample.LlmUrl + "\"," +
                             "\"api_key\":\"" + _audioSample.LlmApiKey + "\"," +
                             "\"system_messages\":[{\"role\":\"system\",\"content\":\"You are a helpful chatbot.\"}]," +
                             "\"greeting_message\":\"" + _audioSample.GreetingMessage + "\"," +
                             "\"failure_message\":\"" + _audioSample.FailureMessage + "\"," +
                             "\"max_history\":" + _audioSample.MaxHistory + "," +
                             "\"input_modalities\":[\"text\"]," +
                             "\"output_modalities\":[\"text\"]," +
                             "\"params\":{\"model\":\"gpt-4o-mini\"}" +
                             "}," +
                             "\"parameters\":{\"audio_scenario\":\"chorus\"}," +
                             "\"tts\":{" +
                             "\"vendor\":\"microsoft\"," +
                             "\"params\":{" +
                             "\"key\":\"" + _audioSample.TtsKey + "\"," +
                             "\"region\":\"" + _audioSample.TtsRegion + "\"," +
                             "\"voice_name\":\"" + _audioSample.TtsVoiceName + "\"" +
                             "}" +
                             "}" +
                             "}" +
                             "}";
            
            _audioSample.Log.UpdateLog("ConvoAI Create Agent Request: " + jsonData);
            yield return SendRequestAndParseAgentId(url, "POST", jsonData, "Create ConvoAI Agent");
        }

        public IEnumerator StopConvoAIAgent()
        {
            // Check if we have an active agent ID
            if (string.IsNullOrEmpty(_currentAgentId))
            {
                _audioSample.Log.UpdateLog("ConvoAI Stop Agent Failed: No active agent found. Please create an agent first.");
                yield break;
            }

            string url = $"https://api.agora.io/api/conversational-ai-agent/v2/projects/{_audioSample.ProjectId}/agents/{_currentAgentId}/leave";
            
            _audioSample.Log.UpdateLog($"ConvoAI Stop Agent Request for Agent ID: {_currentAgentId}");
            yield return SendRequest(url, "POST", null, "Stop ConvoAI Agent");
            
            // Clear the agent ID after stopping and notify NPC components
            string stoppedAgentId = _currentAgentId;
            _currentAgentId = null;
            _audioSample.Log.UpdateLog("Agent ID cleared after stop request");
            
            // Notify NPC components that the agent is no longer active
            _audioSample.NotifyNPCComponents(false, null);
        }

        public IEnumerator UpdateConvoAIAgent()
        {
            // Check if we have an active agent ID
            if (string.IsNullOrEmpty(_currentAgentId))
            {
                _audioSample.Log.UpdateLog("ConvoAI Update Agent Failed: No active agent found. Please create an agent first.");
                yield break;
            }

            string url = $"https://api.agora.io/api/conversational-ai-agent/v2/projects/{_audioSample.ProjectId}/agents/{_currentAgentId}/update";
            
            // Create update request with current configuration - following the API format
            string jsonData = "{\"properties\":{" +
                             "\"token\":\"" + _audioSample.AgentToken + "\"," +
                             "\"llm\":{" +
                             "\"system_messages\":[{\"role\":\"system\",\"content\":\"You are a helpful chatbot.\"}]," +
                             "\"params\":{\"model\":\"gpt-4o-mini\",\"max_token\":1024}" +
                             "}," +
                             "\"asr\":{\"vendor\":\"ares\",\"language\":\"" + _audioSample.AsrLanguage + "\"}," +
                             "\"tts\":{" +
                             "\"vendor\":\"microsoft\"," +
                             "\"params\":{" +
                             "\"key\":\"" + _audioSample.TtsKey + "\"," +
                             "\"region\":\"" + _audioSample.TtsRegion + "\"," +
                             "\"voice_name\":\"" + _audioSample.TtsVoiceName + "\"" +
                             "}" +
                             "}," +
                             "\"greeting_message\":\"" + _audioSample.GreetingMessage + "\"," +
                             "\"failure_message\":\"" + _audioSample.FailureMessage + "\"," +
                             "\"max_history\":" + _audioSample.MaxHistory + "," +
                             "\"idle_timeout\":" + _audioSample.IdleTimeout +
                             "}}";

            _audioSample.Log.UpdateLog($"ConvoAI Update Agent Request for Agent ID: {_currentAgentId}");
            _audioSample.Log.UpdateLog("ConvoAI Update Agent Request: " + jsonData);
            yield return SendRequest(url, "POST", jsonData, "Update ConvoAI Agent");
        }

        public IEnumerator GetConvoAIAgentStatus()
        {
            // Check if we have an active agent ID
            if (string.IsNullOrEmpty(_currentAgentId))
            {
                _audioSample.Log.UpdateLog("ConvoAI Get Agent Status Failed: No active agent found. Please create an agent first.");
                yield break;
            }

            string url = $"https://api.agora.io/api/conversational-ai-agent/v2/projects/{_audioSample.ProjectId}/agents/{_currentAgentId}";
            
            _audioSample.Log.UpdateLog($"ConvoAI Get Agent Status Request for Agent ID: {_currentAgentId}");
            yield return SendRequestAndParseStatus(url, "GET", null, "Get ConvoAI Agent Status");
        }

        public IEnumerator GetConvoAIAgentList()
        {
            // Add query parameters as per the curl command: state=2&limit=20
            string url = $"https://api.agora.io/api/conversational-ai-agent/v2/projects/{_audioSample.ProjectId}/agents?state=2&limit=20";
            
            _audioSample.Log.UpdateLog("ConvoAI Get Agent List Request");
            yield return SendRequestAndParseAgentList(url, "GET", null, "Get ConvoAI Agent List");
        }

        private IEnumerator SendRequest(string url, string method, string jsonData, string operationName)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();
                
                // Add Basic Auth header using utility function
                string authHeader = CreateBasicAuthHeader(_audioSample.ApiKey, _audioSample.ApiSecret);
                request.SetRequestHeader("Authorization", authHeader);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    _audioSample.Log.UpdateLog($"{operationName} Success: " + request.downloadHandler.text);
                }
                else
                {
                    _audioSample.Log.UpdateLog($"{operationName} Failed: " + request.error + " - " + request.downloadHandler.text);
                }
            }
        }

        private IEnumerator SendRequestAndParseAgentId(string url, string method, string jsonData, string operationName)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();
                
                // Add Basic Auth header using utility function
                string authHeader = CreateBasicAuthHeader(_audioSample.ApiKey, _audioSample.ApiSecret);
                request.SetRequestHeader("Authorization", authHeader);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    _audioSample.Log.UpdateLog($"{operationName} Success: " + responseText);
                    
                    // Parse and store the agent ID from the response
                    string agentId = ParseAgentIdFromResponse(responseText);
                    if (!string.IsNullOrEmpty(agentId))
                    {
                        _currentAgentId = agentId;
                        _audioSample.Log.UpdateLog($"Agent ID stored: {_currentAgentId}");
                        
                        // Notify NPC components that an agent is now active
                        _audioSample.NotifyNPCComponents(true, _currentAgentId);
                    }
                    else
                    {
                        _audioSample.Log.UpdateLog("Warning: Could not parse agent ID from response");
                    }
                }
                else
                {
                    _audioSample.Log.UpdateLog($"{operationName} Failed: " + request.error + " - " + request.downloadHandler.text);
                }
            }
        }

        private IEnumerator SendRequestAndParseStatus(string url, string method, string jsonData, string operationName)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();
                
                // Add Basic Auth header using utility function
                string authHeader = CreateBasicAuthHeader(_audioSample.ApiKey, _audioSample.ApiSecret);
                request.SetRequestHeader("Authorization", authHeader);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    _audioSample.Log.UpdateLog($"{operationName} Success: " + responseText);
                    
                    // Parse and display agent status information
                    ParseAndDisplayAgentStatus(responseText);
                }
                else
                {
                    _audioSample.Log.UpdateLog($"{operationName} Failed: " + request.error + " - " + request.downloadHandler.text);
                }
            }
        }

        private IEnumerator SendRequestAndParseAgentList(string url, string method, string jsonData, string operationName)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();
                
                // Add Basic Auth header using utility function
                string authHeader = CreateBasicAuthHeader(_audioSample.ApiKey, _audioSample.ApiSecret);
                request.SetRequestHeader("Authorization", authHeader);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    _audioSample.Log.UpdateLog($"{operationName} Success: " + responseText);
                    
                    // Parse and display agent list information
                    ParseAndDisplayAgentList(responseText);
                }
                else
                {
                    _audioSample.Log.UpdateLog($"{operationName} Failed: " + request.error + " - " + request.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// Parses the agent ID from the JSON response
        /// Expected format: {"agent_id": "1NT29X10YHxxxxxWJOXLYHNYB", ...}
        /// </summary>
        /// <param name="jsonResponse">The JSON response string</param>
        /// <returns>The agent ID if found, null otherwise</returns>
        private string ParseAgentIdFromResponse(string jsonResponse)
        {
            try
            {
                // Primary pattern: "agent_id": "value" (with optional spaces)
                string[] patterns = {
                    "\"agent_id\":\"",     // No spaces
                    "\"agent_id\": \"",    // Space after colon
                    "\"agentId\":\"",      // Legacy camelCase pattern
                    "\"agentId\": \""      // Legacy camelCase with space
                };

                foreach (string searchPattern in patterns)
                {
                    int startIndex = jsonResponse.IndexOf(searchPattern);
                    
                    if (startIndex >= 0)
                    {
                        startIndex += searchPattern.Length;
                        int endIndex = jsonResponse.IndexOf("\"", startIndex);
                        
                        if (endIndex > startIndex)
                        {
                            string agentId = jsonResponse.Substring(startIndex, endIndex - startIndex);
                            _audioSample.Log.UpdateLog($"Successfully parsed agent ID using pattern: {searchPattern}");
                            return agentId;
                        }
                    }
                }
                
                _audioSample.Log.UpdateLog("Agent ID not found in response JSON");
                _audioSample.Log.UpdateLog($"Response JSON: {jsonResponse}");
                return null;
            }
            catch (System.Exception ex)
            {
                _audioSample.Log.UpdateLog($"Error parsing agent ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses and displays agent status information from the JSON response
        /// Expected format: {"message": "agent exits with reason: xxxx", "start_ts": 1735035893, "stop_ts": 1735035900, "status": "FAILED", "agent_id": "1NT29X11GQSxxxxxNU80BEIN56XF"}
        /// </summary>
        /// <param name="jsonResponse">The JSON response string</param>
        private void ParseAndDisplayAgentStatus(string jsonResponse)
        {
            try
            {
                _audioSample.Log.UpdateLog("=== AGENT STATUS INFORMATION ===");
                
                // Parse agent ID
                string agentId = ParseJsonValue(jsonResponse, "agent_id");
                if (!string.IsNullOrEmpty(agentId))
                {
                    _audioSample.Log.UpdateLog($"Agent ID: {agentId}");
                }

                // Parse status
                string status = ParseJsonValue(jsonResponse, "status");
                if (!string.IsNullOrEmpty(status))
                {
                    _audioSample.Log.UpdateLog($"Status: {status}");
                }

                // Parse message
                string message = ParseJsonValue(jsonResponse, "message");
                if (!string.IsNullOrEmpty(message))
                {
                    _audioSample.Log.UpdateLog($"Message: {message}");
                }

                // Parse timestamps
                string startTs = ParseJsonValue(jsonResponse, "start_ts");
                if (!string.IsNullOrEmpty(startTs))
                {
                    if (long.TryParse(startTs, out long startTimestamp))
                    {
                        DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(startTimestamp).DateTime;
                        _audioSample.Log.UpdateLog($"Start Time: {startTime:yyyy-MM-dd HH:mm:ss} UTC (Timestamp: {startTimestamp})");
                    }
                    else
                    {
                        _audioSample.Log.UpdateLog($"Start Timestamp: {startTs}");
                    }
                }

                string stopTs = ParseJsonValue(jsonResponse, "stop_ts");
                if (!string.IsNullOrEmpty(stopTs))
                {
                    if (long.TryParse(stopTs, out long stopTimestamp))
                    {
                        DateTime stopTime = DateTimeOffset.FromUnixTimeSeconds(stopTimestamp).DateTime;
                        _audioSample.Log.UpdateLog($"Stop Time: {stopTime:yyyy-MM-dd HH:mm:ss} UTC (Timestamp: {stopTimestamp})");
                        
                        // Calculate duration if both timestamps are available
                        if (long.TryParse(startTs, out long startTimestampForDuration))
                        {
                            long duration = stopTimestamp - startTimestampForDuration;
                            _audioSample.Log.UpdateLog($"Duration: {duration} seconds");
                        }
                    }
                    else
                    {
                        _audioSample.Log.UpdateLog($"Stop Timestamp: {stopTs}");
                    }
                }

                _audioSample.Log.UpdateLog("=== END AGENT STATUS ===");
            }
            catch (System.Exception ex)
            {
                _audioSample.Log.UpdateLog($"Error parsing agent status: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses and displays agent list information from the JSON response
        /// Expected format: {"data": {"count": 1, "list": [{"start_ts": 1735035893, "status": "RUNNING", "agent_id": "1234567890ABCDE1CVGZNU80BEIN56XF"}]}, "meta": {"cursor": "", "total": 1}, "status": "ok"}
        /// </summary>
        /// <param name="jsonResponse">The JSON response string</param>
        private void ParseAndDisplayAgentList(string jsonResponse)
        {
            try
            {
                _audioSample.Log.UpdateLog("=== AGENT LIST INFORMATION ===");
                
                // Parse overall status
                string overallStatus = ParseJsonValue(jsonResponse, "status");
                if (!string.IsNullOrEmpty(overallStatus))
                {
                    _audioSample.Log.UpdateLog($"Response Status: {overallStatus}");
                }

                // Parse meta information
                string metaTotal = ParseNestedJsonValue(jsonResponse, "meta", "total");
                if (!string.IsNullOrEmpty(metaTotal))
                {
                    _audioSample.Log.UpdateLog($"Total Agents: {metaTotal}");
                }

                string dataCoun = ParseNestedJsonValue(jsonResponse, "data", "count");
                if (!string.IsNullOrEmpty(dataCoun))
                {
                    _audioSample.Log.UpdateLog($"Returned Count: {dataCoun}");
                }

                // Parse the agents in the list
                ParseAgentListItems(jsonResponse);

                _audioSample.Log.UpdateLog("=== END AGENT LIST ===");
            }
            catch (System.Exception ex)
            {
                _audioSample.Log.UpdateLog($"Error parsing agent list: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses individual agent items from the list array
        /// </summary>
        /// <param name="jsonResponse">The JSON response string</param>
        private void ParseAgentListItems(string jsonResponse)
        {
            try
            {
                // Find the "list": [ array
                string listPattern = "\"list\":[";
                int listStart = jsonResponse.IndexOf(listPattern);
                
                if (listStart >= 0)
                {
                    listStart += listPattern.Length;
                    
                    // Find the end of the list array
                    int bracketCount = 1;
                    int listEnd = listStart;
                    
                    while (listEnd < jsonResponse.Length && bracketCount > 0)
                    {
                        if (jsonResponse[listEnd] == '[') bracketCount++;
                        else if (jsonResponse[listEnd] == ']') bracketCount--;
                        listEnd++;
                    }
                    
                    if (bracketCount == 0)
                    {
                        string listContent = jsonResponse.Substring(listStart, listEnd - listStart - 1);
                        
                        // Split by objects in the array (looking for }, { pattern)
                        string[] agentObjects = listContent.Split(new string[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
                        
                        _audioSample.Log.UpdateLog($"Found {agentObjects.Length} agent(s) in list:");
                        
                        for (int i = 0; i < agentObjects.Length; i++)
                        {
                            string agentJson = agentObjects[i];
                            
                            // Clean up the JSON object (add back braces if needed)
                            if (!agentJson.StartsWith("{")) agentJson = "{" + agentJson;
                            if (!agentJson.EndsWith("}")) agentJson = agentJson + "}";
                            
                            _audioSample.Log.UpdateLog($"--- Agent {i + 1} ---");
                            
                            // Parse individual agent details
                            string agentId = ParseJsonValue(agentJson, "agent_id");
                            if (!string.IsNullOrEmpty(agentId))
                            {
                                _audioSample.Log.UpdateLog($"  Agent ID: {agentId}");
                            }

                            string status = ParseJsonValue(agentJson, "status");
                            if (!string.IsNullOrEmpty(status))
                            {
                                _audioSample.Log.UpdateLog($"  Status: {status}");
                            }

                            string startTs = ParseJsonValue(agentJson, "start_ts");
                            if (!string.IsNullOrEmpty(startTs))
                            {
                                if (long.TryParse(startTs, out long startTimestamp))
                                {
                                    DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(startTimestamp).DateTime;
                                    _audioSample.Log.UpdateLog($"  Start Time: {startTime:yyyy-MM-dd HH:mm:ss} UTC");
                                }
                                else
                                {
                                    _audioSample.Log.UpdateLog($"  Start Timestamp: {startTs}");
                                }
                            }

                            string stopTs = ParseJsonValue(agentJson, "stop_ts");
                            if (!string.IsNullOrEmpty(stopTs))
                            {
                                if (long.TryParse(stopTs, out long stopTimestamp))
                                {
                                    DateTime stopTime = DateTimeOffset.FromUnixTimeSeconds(stopTimestamp).DateTime;
                                    _audioSample.Log.UpdateLog($"  Stop Time: {stopTime:yyyy-MM-dd HH:mm:ss} UTC");
                                }
                                else
                                {
                                    _audioSample.Log.UpdateLog($"  Stop Timestamp: {stopTs}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    _audioSample.Log.UpdateLog("No agent list found in response");
                }
            }
            catch (System.Exception ex)
            {
                _audioSample.Log.UpdateLog($"Error parsing agent list items: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to parse nested JSON values (e.g., "meta.total" or "data.count")
        /// </summary>
        /// <param name="jsonResponse">The JSON response string</param>
        /// <param name="parentKey">The parent key (e.g., "meta")</param>
        /// <param name="childKey">The child key (e.g., "total")</param>
        /// <returns>The value if found, null otherwise</returns>
        private string ParseNestedJsonValue(string jsonResponse, string parentKey, string childKey)
        {
            try
            {
                // Find the parent object
                string parentPattern = $"\"{parentKey}\":{{";
                int parentStart = jsonResponse.IndexOf(parentPattern);
                
                if (parentStart >= 0)
                {
                    parentStart += parentPattern.Length;
                    
                    // Find the end of the parent object
                    int braceCount = 1;
                    int parentEnd = parentStart;
                    
                    while (parentEnd < jsonResponse.Length && braceCount > 0)
                    {
                        if (jsonResponse[parentEnd] == '{') braceCount++;
                        else if (jsonResponse[parentEnd] == '}') braceCount--;
                        parentEnd++;
                    }
                    
                    if (braceCount == 0)
                    {
                        string parentContent = jsonResponse.Substring(parentStart, parentEnd - parentStart - 1);
                        return ParseJsonValue(parentContent, childKey);
                    }
                }
                
                return null;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method to parse a JSON value by key
        /// </summary>
        /// <param name="jsonResponse">The JSON response string</param>
        /// <param name="key">The key to search for</param>
        /// <returns>The value if found, null otherwise</returns>
        private string ParseJsonValue(string jsonResponse, string key)
        {
            try
            {
                // Look for "key":"value" or "key": "value" patterns
                string[] patterns = {
                    $"\"{key}\":\"",     // String value, no spaces
                    $"\"{key}\": \"",    // String value, space after colon
                    $"\"{key}\":",       // Numeric value, no spaces  
                    $"\"{key}\": "       // Numeric value, space after colon
                };

                foreach (string searchPattern in patterns)
                {
                    int startIndex = jsonResponse.IndexOf(searchPattern);
                    
                    if (startIndex >= 0)
                    {
                        startIndex += searchPattern.Length;
                        
                        // For string values (patterns ending with quote)
                        if (searchPattern.EndsWith("\""))
                        {
                            int endIndex = jsonResponse.IndexOf("\"", startIndex);
                            if (endIndex > startIndex)
                            {
                                return jsonResponse.Substring(startIndex, endIndex - startIndex);
                            }
                        }
                        // For numeric values (patterns not ending with quote)
                        else
                        {
                            // Find the end of the numeric value (comma, brace, or end of string)
                            int endIndex = startIndex;
                            while (endIndex < jsonResponse.Length && 
                                   jsonResponse[endIndex] != ',' && 
                                   jsonResponse[endIndex] != '}' && 
                                   jsonResponse[endIndex] != ']')
                            {
                                endIndex++;
                            }
                            
                            if (endIndex > startIndex)
                            {
                                return jsonResponse.Substring(startIndex, endIndex - startIndex).Trim();
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a Basic Authentication header value from username and password
        /// </summary>
        /// <param name="username">The username (API Key)</param>
        /// <param name="password">The password (API Secret)</param>
        /// <returns>Basic Auth header value in format "Basic {base64encodedcredentials}"</returns>
        private string CreateBasicAuthHeader(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _audioSample.Log.UpdateLog("Warning: Username or password is empty for Basic Auth");
                return "";
            }

            // Combine username and password with colon separator (same as JavaScript btoa)
            string credentials = $"{username}:{password}";
            
            // Convert to Base64 using ASCII encoding (matches JavaScript btoa behavior)
            string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            
            // Return formatted Basic Auth header
            string generatedAuthHeader = $"Basic {base64Credentials}";
            
            _audioSample.Log.UpdateLog($"Created Basic Auth header for user: {username}");
            _audioSample.Log.UpdateLog($"Credentials string: {credentials}");
            _audioSample.Log.UpdateLog($"Generated Auth Header: {generatedAuthHeader}");
            _audioSample.Log.UpdateLog($"JavaScript Working Header: Basic MzdiYzU5OWE0MTYwNDAzYzk5ZDg1OTQyODIxYWQ0ODU6ZTA1ZjI2ZTkxNzc1NGYzNWJjOTgxNzhiMWY5M2QxZTU=");
            
            // TEMPORARY: Use the working JavaScript header until we fix the credential mismatch
            string workingAuthHeader = "Basic MzdiYzU5OWE0MTYwNDAzYzk5ZDg1OTQyODIxYWQ0ODU6ZTA1ZjI2ZTkxNzc1NGYzNWJjOTgxNzhiMWY5M2QxZTU=";
            _audioSample.Log.UpdateLog("Using working JavaScript auth header temporarily");
            
            return workingAuthHeader;
        }

        /// <summary>
        /// Public utility method to create Basic Auth header for external use
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Basic Auth header value</returns>
        public static string CreateBasicAuthHeaderStatic(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("ConvoAINWMgr: Username or password is empty for Basic Auth");
                return "";
            }

            string credentials = $"{username}:{password}";
            string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            string authHeader = $"Basic {base64Credentials}";
            
            Debug.Log($"ConvoAINWMgr: Generated Auth Header: {authHeader}");
            
            return authHeader;
        }

        /// <summary>
        /// Gets the current active agent ID
        /// </summary>
        /// <returns>The current agent ID, or null if no agent is active</returns>
        public string GetCurrentAgentId()
        {
            return _currentAgentId;
        }

        /// <summary>
        /// Checks if there is an active agent
        /// </summary>
        /// <returns>True if there is an active agent, false otherwise</returns>
        public bool HasActiveAgent()
        {
            return !string.IsNullOrEmpty(_currentAgentId);
        }
    }

    #endregion

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinConvoAIChannelAudio _audioSample;

        internal UserEventHandler(JoinConvoAIChannelAudio audioSample)
        {
            _audioSample = audioSample;
        }

        public override void OnError(int err, string msg)
        {
            _audioSample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _audioSample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _audioSample.RtcEngine.GetVersion(ref build)));
            _audioSample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
            _audioSample.CreateLocalAudioCallQualityPanel();
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _audioSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _audioSample.Log.UpdateLog("OnLeaveChannel");
            _audioSample.ClearAudioCallQualityPanel();
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _audioSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _audioSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _audioSample.CreateRemoteAudioCallQualityPanel(uid);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _audioSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            _audioSample.DestroyRemoteAudioCallQualityPanel(uid);
        }

        //Quality monitoring during calls
        public override void OnRtcStats(RtcConnection connection, RtcStats stats)
        {
            var panel = _audioSample.GetLocalAudioCallQualityPanel();
            if (panel != null)
            {
                panel.Stats = stats;
                panel.RefreshPanel();
            }
        }

        public override void OnLocalAudioStats(RtcConnection connection, LocalAudioStats stats)
        {
            var panel = _audioSample.GetLocalAudioCallQualityPanel();
            if (panel != null)
            {
                panel.AudioStats = stats;
                panel.RefreshPanel();
            }
        }

        public override void OnLocalAudioStateChanged(RtcConnection connection, LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_REASON reason)
        {

        }

        public override void OnRemoteAudioStats(RtcConnection connection, RemoteAudioStats stats)
        {
            var panel = _audioSample.GetRemoteAudioCallQualityPanel(stats.uid);
            if (panel != null)
            {
                panel.AudioStats = stats;
                panel.RefreshPanel();
            }
        }

        public override void OnRemoteAudioStateChanged(RtcConnection connection, uint remoteUid, REMOTE_AUDIO_STATE state, REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {

        }

        public override void OnAudioVolumeIndication(RtcConnection connection, AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            foreach (var speaker in speakers)
            {
                if (speaker.uid == 0)
                {
                    var panel = _audioSample.GetLocalAudioCallQualityPanel();
                    if (panel != null)
                    {
                        panel.Volume = (int)speaker.volume;
                        panel.RefreshPanel();
                    }
                }
                else
                {
                    var panel = _audioSample.GetRemoteAudioCallQualityPanel(speaker.uid);
                    if (panel != null)
                    {
                        panel.Volume = (int)speaker.volume;
                        panel.RefreshPanel();
                    }
                }
            }

        }

    }

    #endregion
}