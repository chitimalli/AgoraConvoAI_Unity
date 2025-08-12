# ConvoAI NPC System Setup Instructions

## Overview
The system has been redesigned to solve the issue where disabled GameObjects break script functionality. Now the system uses persistent manager components that stay active even when NPCs are hidden.

## Architecture Components

### 1. JoinChannelAudio.cs (Main Controller)
- **Purpose**: Central ConvoAI integration hub
- **Status**: Updated with networking and NPC notification system
- **Key Features**: 
  - ConvoAI REST API integration
  - Automatic NPC component discovery via reflection
  - Agent lifecycle management

### 2. ConvoAINWMgr.cs (Networking Manager)
- **Purpose**: Complete ConvoAI REST API implementation
- **Status**: Fully functional with working authentication
- **Endpoints**: /join, /leave, /update, /status, /list
- **Features**: JSON parsing, error handling, NPC notifications

### 3. NPCStatusMonitor.cs (Status Monitor) - NEW
- **Purpose**: Persistent ConvoAI status monitoring
- **Status**: Ready to use
- **Features**: 
  - Continues running when NPCs are hidden
  - Automatic NPC component notification
  - Event system for other components

### 4. UIPanelManager.cs (UI Controller) - NEW
- **Purpose**: Standalone UI panel management
- **Status**: Ready to use
- **Features**: 
  - Independent of NPC visibility
  - Customizable slide directions and animations
  - Singleton pattern for easy access

### 5. NPC_Yuki.cs (Visual NPC) - SIMPLIFIED
- **Purpose**: Visual NPC representation only
- **Status**: Simplified and safe to disable
- **Features**: 
  - Show/hide animations
  - No longer handles status monitoring or UI panels
  - Works with external managers

## Setup Instructions

### Step 1: Create Manager GameObjects
Create two new empty GameObjects in your scene:

1. **"ConvoAI Status Monitor"**
   - Add the `NPCStatusMonitor` component
   - This GameObject must stay active at all times

2. **"UI Panel Manager"** 
   - Add the `UIPanelManager` component
   - This GameObject must stay active at all times

### Step 2: Configure NPCStatusMonitor
On the NPCStatusMonitor component:
- Set **Status Check Interval** (default: 1.0 seconds)
- Enable **Debug Logs** if you want to see status updates
- The component will automatically find JoinChannelAudio

### Step 3: Configure UIPanelManager
On the UIPanelManager component:
- Drag your **UI Panel** (RectTransform) to the UI Panel field
- Set **Slide Direction** (Left, Right, Up, Down)
- Set **Animation Duration** (default: 0.3 seconds)
- Adjust **Animation Curve** for custom easing

### Step 4: Configure NPC_Yuki (Visual NPC)
On your NPC GameObject with NPC_Yuki component:
- Set **Transition Duration** for show/hide animations
- Set **Active Scale** (default: 1,1,1)
- Set **Inactive Scale** (default: 0,0,0)
- Enable **Debug Logs** if needed

### Step 5: Connect UI Panel Buttons
For your UI panel's Open/Close buttons:

**Close Button:**
- OnClick() → UIPanelManager → CloseUIPanel()

**Open Button:**
- OnClick() → UIPanelManager → OpenUIPanel()

**Toggle Button:**
- OnClick() → UIPanelManager → ToggleUIPanel()

## How It Works

### Agent Activation Flow:
1. User calls `CreateConvoAIAgent()` on JoinChannelAudio
2. JoinChannelAudio creates agent via ConvoAINWMgr
3. ConvoAINWMgr notifies all NPC components of status change
4. NPCStatusMonitor detects active agent in its monitoring loop
5. NPCStatusMonitor calls `ShowNPC()` on all NPC_Yuki components
6. NPC_Yuki components animate to show the NPC

### Agent Deactivation Flow:
1. User calls `StopConvoAIAgent()` on JoinChannelAudio
2. JoinChannelAudio stops agent via ConvoAINWMgr
3. ConvoAINWMgr notifies all NPC components of status change
4. NPCStatusMonitor detects no active agent
5. NPCStatusMonitor calls `HideNPC()` on all NPC_Yuki components
6. NPC_Yuki components animate to hide the NPC

### UI Panel Operation:
- UIPanelManager operates independently of NPC visibility
- UI panels can be opened/closed even when NPCs are hidden
- Buttons call UIPanelManager methods directly
- No dependency on NPC GameObject state

## Benefits of New Architecture

1. **Persistent Functionality**: UI controls and status monitoring work even when NPCs are hidden
2. **Separation of Concerns**: Each component has a single responsibility
3. **Scalability**: Easy to add multiple NPCs or UI panels
4. **Reliability**: No broken references when GameObjects are disabled
5. **Maintainability**: Simpler, focused components are easier to debug

## Troubleshooting

### NPCs Not Showing/Hiding:
- Check that NPCStatusMonitor GameObject is active
- Verify JoinChannelAudio is in the scene
- Check ConvoAI authentication is working
- Look at debug logs from NPCStatusMonitor

### UI Panels Not Working:
- Check that UIPanelManager GameObject is active
- Verify UI Panel is assigned in UIPanelManager
- Check button OnClick events are connected
- Look at debug logs from UIPanelManager

### Authentication Issues:
- Verify ConvoAI credentials in JoinChannelAudio
- Check network connectivity
- Look at ConvoAINWMgr debug logs

## API Reference

### NPCStatusMonitor Public Methods:
- `StartMonitoring()` - Begin status monitoring
- `StopMonitoring()` - Stop status monitoring
- `GetCurrentAgentStatus()` - Get current agent status

### UIPanelManager Public Methods:
- `OpenUIPanel()` - Open the UI panel with animation
- `CloseUIPanel()` - Close the UI panel with animation
- `ToggleUIPanel()` - Toggle panel open/closed
- `IsUIPanelOpen()` - Check if panel is currently open

### NPC_Yuki Public Methods:
- `ShowNPC()` - Show the NPC with animation
- `HideNPC()` - Hide the NPC with animation
- `IsShown()` - Check if NPC is currently shown
