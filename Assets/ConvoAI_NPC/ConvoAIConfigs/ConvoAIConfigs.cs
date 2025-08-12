using UnityEngine;
using System;
using UnityEngine.Serialization;



namespace io.agora.rtc.demo
{
    [CreateAssetMenu(menuName = "Agora/ConvoAIConfigs", fileName = "ConvoAIConfigs", order = 1)]
    [Serializable]
    public class ConvoAIConfigs : ScriptableObject
    {
        public string apiKey = "";
        
        public string apiSecret = "";
        
        public string agentName = "";
        
        public uint agentRtcUid = 8888;
        
        public int idleTimeout = 30;
        
        public string asrLanguage = "";
        
        public string llmUrl = "";
        
        public string llmApiKey = "";
        
        public string greetingMessage = "";
        
        public string failureMessage = "";
        
        public int maxHistory = 32;
        
        public string ttsKey = "";
        
        public string ttsRegion = "";
        
        public string ttsVoiceName = "";
    }
}
