# Agora ConvoAI Unity Integration

A Unity package that integrates Agora's Conversational AI engine with real-time voice communication, enabling developers to create AI-powered voice interactions in their Unity applications.

<img width="966" height="546" alt="Screenshot 2025-08-26 at 1 18 35 PM" src="https://github.com/user-attachments/assets/ccb51428-4101-404e-805f-7608f5c693ba" />

Watch the youtube GamePlay here: https://www.youtube.com/watch?v=4t_2K87Fx9I

## 🚀 Features

- **Real-time Voice Communication**: Powered by Agora RTC SDK
- **AI Conversational Agent**: Integration with Agora's ConvoAI engine
- **Configurable AI Personality**: Customizable system messages and responses
- **Multi-language Support**: Configurable ASR (Automatic Speech Recognition) languages
- **Text-to-Speech**: Integrated TTS with customizable voices
- **Quality Monitoring**: Built-in call quality panels and logging
- **Generic NPC System**: Interface-based architecture for easy integration

## 📋 Prerequisites

Before using this package, you need to obtain the following from Agora:

### Required Agora Credentials
1. **App ID**: Your Agora project App ID
2. **Token**: RTC token for authentication (can be temporary token for testing)
3. **API Key & Secret**: For ConvoAI API access
4. **Project ID**: Your Agora project identifier

### External API Requirements
- **OpenAI API Key**: For LLM integration (or compatible API endpoint)
- **Azure Cognitive Services**: For Text-to-Speech functionality
  - TTS Key
  - TTS Region
  - TTS Voice Name

## 🛠️ Setup Instructions

### 1. Install Dependencies

Ensure you have the following packages in your Unity project:
- **Agora RTC SDK for Unity** (included in `Assets/Agora-RTC-Plugin/`)
- **Unity 2020.3 or later**

### 2. Configure ConvoAI Settings

1. **Create Configuration Asset**:
   - Right-click in your Project window
   - Go to `Create > Agora > ConvoAIConfigs`
   - Name it `ConvoAIConfigs` (or your preferred name)

2. **Fill in Required Settings**:

#### Basic RTC Configuration
```
App ID: [Your Agora App ID]
Token: [Your RTC Token]
Channel Name: [Your channel name]
```

#### ConvoAI API Configuration
```
API Key: [Your Agora API Key]
API Secret: [Your Agora API Secret]
Agent Name: [Custom name for your AI agent]
Agent RTC UID: 8888 (or your preferred UID)
```

#### Language & Speech Configuration
```
ASR Language: en-US (or your preferred language)
LLM URL: https://api.openai.com/v1/chat/completions
LLM API Key: [Your OpenAI API Key]
```

#### AI Personality Configuration
```
System Message: "You are a helpful assistant..." (Define AI behavior)
Greeting Message: "Hello! How can I help you today?"
Failure Message: "I'm sorry, I didn't understand that."
Max History: 32 (conversation context length)
```

#### Text-to-Speech Configuration
```
TTS Key: [Your Azure Cognitive Services key]
TTS Region: [Your Azure region, e.g., "eastus"]
TTS Voice Name: [Voice name, e.g., "en-US-JennyNeural"]
```

### 3. Scene Setup

1. **Add ConvoAI Component**:
   - Create an empty GameObject in your scene
   - Add the `JoinConvoAIChannelAudio` component
   - Assign your `ConvoAIConfigs` asset to the component

2. **UI Setup** (Optional):
   - Add UI Text component for logging (assign to LogText field)
   - Set up quality monitoring panels if needed

### 4. Integration with NPCs

The system uses an interface-based architecture for easy NPC integration:

```csharp
public interface IConvoAIStatusListener
{
    void OnAgentStatusChanged(bool isActive, string agentId);
}
```

**To create an NPC that responds to AI agent status**:

1. Create a script that implements `IConvoAIStatusListener`
2. Implement the `OnAgentStatusChanged` method
3. Add the script to any GameObject in the scene

The system will automatically discover and notify all components implementing this interface.

## 🎮 Usage

### Simple Integration

The easiest way to integrate ConvoAI into your project:

1. **Attach the Component**:
   - Add the `JoinConvoAIChannelAudio` script to any GameObject in your scene
   - Assign your `ConvoAIConfigs` asset to the component's configuration field

2. **Control the Agent Programmatically**:
   ```csharp
   // Get reference to the component
   JoinConvoAIChannelAudio convoAI = GetComponent<JoinConvoAIChannelAudio>();
   
   // Start the AI agent (joins the same channel as configured)
   convoAI.CreateConvoAIAgent();
   
   // Stop the AI agent when done
   convoAI.StopConvoAIAgent();
   
   // Update agent configuration on the fly
   convoAI.UpdateConvoAIAgent();
   
   // Check agent status
   convoAI.GetConvoAIAgentStatus();
   
   // Get list of all agents
   convoAI.GetConvoAIAgentList();
   ```

3. **Agent Lifecycle**:
   - Call `CreateConvoAIAgent()` to start the AI agent
   - The agent automatically joins the same channel specified in your `ConvoAIConfigs`
   - The agent will use all the settings from your configuration (LLM, TTS, ASR, etc.)
   - Call `StopConvoAIAgent()` to stop the agent and free resources

### Automatic Scene Setup
1. Ensure all configuration values are properly set in your `ConvoAIConfigs` asset
2. The system will automatically:
   - Connect to Agora RTC using your App ID and Token
   - Join the specified channel
   - When you call `CreateConvoAIAgent()`, the AI agent joins the same channel
   - Start listening for voice input and provide AI responses

### Voice Interaction Flow
1. **User speaks** → ASR converts speech to text
2. **Text sent to LLM** → AI generates response based on your system message
3. **Response converted to speech** → TTS plays audio using your configured voice
4. **NPCs notified** → Any listening NPCs receive status updates through the interface system

## 🔧 API Configuration Examples

### OpenAI Configuration
```
LLM URL: https://api.openai.com/v1/chat/completions
LLM API Key: sk-your-openai-api-key
System Message: You are a helpful RPG character who speaks in medieval style.
```

### Azure OpenAI Configuration
```
LLM URL: https://your-resource.openai.azure.com/openai/deployments/your-model/chat/completions?api-version=2023-12-01-preview
LLM API Key: your-azure-openai-key
```

### Custom LLM Endpoint
Any OpenAI-compatible API endpoint can be used by setting the appropriate URL and API key.

## 📊 Monitoring & Debugging

The system includes comprehensive logging and quality monitoring:

- **Console Logs**: Detailed connection and API call logs
- **Quality Panels**: Real-time audio quality metrics
- **Status Events**: NPC notification system for game integration

## 🔑 Important Notes

### Security
- **Never commit API keys** to version control
- Use environment variables or secure configuration for production
- Implement proper token refresh mechanisms for production use

### Performance
- Adjust `Max History` based on your needs (affects API costs)
- Monitor `Idle Timeout` to manage agent lifecycle
- Use appropriate audio quality settings for your target platform

### Language Support
Supported ASR languages include:
- `en-US` (English - US)
- `en-GB` (English - UK)
- `zh-CN` (Chinese - Simplified)
- `ja-JP` (Japanese)
- `ko-KR` (Korean)
- And many more (check Agora documentation for full list)

## 🤝 Contributing

This is a generic ConvoAI system designed to work with any Unity project. Feel free to extend the interface system to add custom NPC behaviors and integrations.

## 📄 License

This project integrates with Agora services. Please ensure you comply with Agora's terms of service and licensing requirements.

## 🆘 Support

For issues related to:
- **Agora RTC SDK**: Check [Agora Documentation](https://docs.agora.io/en/voice-calling/overview/product-overview?platform=unity)
- **ConvoAI API**: Contact Agora support
- **Unity Integration**: Check Unity console logs and ensure all dependencies are properly installed

---

**Happy coding! 🎉**
