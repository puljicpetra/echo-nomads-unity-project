# Checkpoint System Documentation

## Overview

The Checkpoint System provides a comprehensive save/respawn mechanism for Unity games, allowing players to save their progress at designated checkpoints and automatically respawn when falling below a certain depth. The system includes cheat code integration for easy testing and debugging.

## Components

### 1. Checkpoint.cs
Individual checkpoint script that handles player detection, activation, and respawning.

**Key Features:**
- Automatic player detection within activation radius
- Visual feedback with active/inactive indicators
- Particle effects and audio feedback on activation
- Safe player teleportation with component management
- Starting checkpoint designation
- Editor visualization with gizmos

**Inspector Settings:**
- `checkpointId`: Unique identifier for the checkpoint
- `isStartingCheckpoint`: Mark as the default spawn point
- `activationRadius`: Detection radius for player activation
- `playerLayer`: Layer mask for player detection
- `activeIndicator`: GameObject shown when checkpoint is active
- `inactiveIndicator`: GameObject shown when checkpoint is inactive
- `activationEffect`: Particle system played on activation
- `activationSoundName`: Audio clip name for activation sound

### 2. CheckpointManager.cs
Singleton manager that coordinates all checkpoints in the scene.

**Key Features:**
- Automatic checkpoint registration and management
- Current active checkpoint tracking
- Nearest checkpoint finding
- Event system for other components to listen to
- Starting checkpoint setup
- Debug visualization and logging

**Public Methods:**
- `RegisterCheckpoint(Checkpoint)`: Manually register a checkpoint
- `SaveCheckpoint(Checkpoint)`: Set a checkpoint as the current save point
- `RespawnAtCurrentCheckpoint()`: Respawn player at current checkpoint
- `TeleportToNearestCheckpoint()`: Teleport to closest checkpoint
- `FindNearestCheckpoint(Vector3)`: Find nearest checkpoint to position

### 3. DepthChecker.cs
Monitors player Y position and triggers respawn when falling too deep.

**Key Features:**
- Configurable death depth threshold
- Performance-optimized interval checking
- Audio feedback for fall death and respawn
- Automatic respawn prevention during ongoing respawn
- Debug visualization of death depth plane

**Inspector Settings:**
- `deathDepth`: Y position that triggers respawn (-50f default)
- `enableDepthChecking`: Toggle depth monitoring
- `checkInterval`: How often to check player depth (0.5s default)
- `fallDeathSoundName`: Audio clip for falling death
- `respawnSoundName`: Audio clip for respawn
- `debugMode`: Enable debug visualization

### 4. CheatCodeManager.cs (Enhanced)
Extended the existing cheat code system with checkpoint functionality.

**New Cheat Code:**
- `"reset"`: Teleports player to nearest checkpoint

**Usage:** Type "reset" during gameplay to instantly teleport to the nearest checkpoint.

### 5. CheckpointSystemSetup.cs
Utility script for easy system setup and validation.

**Features:**
- Automatic system component creation
- System validation and error checking
- Sample checkpoint creation
- Custom Unity Editor interface

## Setup Instructions

### Quick Setup (Recommended)
1. Add `CheckpointSystemSetup` script to any GameObject in your scene
2. In the Inspector, click "Setup Checkpoint System"
3. Click "Create Sample Checkpoint" to create an example
4. Move the checkpoint to desired location
5. Click "Validate System" to ensure everything is working

### Manual Setup
1. Create an empty GameObject named "CheckpointManager"
2. Add the `CheckpointManager` component to it
3. Create another empty GameObject named "DepthChecker"
4. Add the `DepthChecker` component to it
5. Configure the death depth and other settings as needed

### Creating Checkpoints
1. Create an empty GameObject where you want the checkpoint
2. Add the `Checkpoint` component
3. Set up visual indicators:
   - Create child objects for active/inactive states
   - Assign them to the checkpoint's indicator fields
4. Optionally add a ParticleSystem for activation effects
5. Configure audio settings if using the AudioManager

## Integration with Existing Systems

### AudioManager Integration
The system automatically integrates with your existing AudioManager singleton:
```csharp
// Plays checkpoint activation sound
AudioManager.Instance.Play("CheckpointActivated");

// Plays fall death sound
AudioManager.Instance.Play("PlayerFallDeath");

// Plays respawn sound
AudioManager.Instance.Play("PlayerRespawn");
```

### Invector Third Person Controller
Fully compatible with Invector's vThirdPersonController:
- Safely disables/enables controller during teleportation
- Manages rigidbody states to prevent physics issues
- Clears velocities to prevent unwanted movement

## Events System

### CheckpointManager Events
```csharp
CheckpointManager.Instance.OnCheckpointActivated += (checkpoint) => {
    Debug.Log($"Checkpoint {checkpoint.CheckpointId} was activated!");
};

CheckpointManager.Instance.OnPlayerRespawned += (checkpoint) => {
    Debug.Log($"Player respawned at {checkpoint.CheckpointId}!");
};
```

### DepthChecker Events
```csharp
DepthChecker depthChecker = FindObjectOfType<DepthChecker>();
depthChecker.OnPlayerFellTooDeep += () => {
    Debug.Log("Player fell too deep!");
};

depthChecker.OnPlayerRespawned += () => {
    Debug.Log("Player was respawned!");
};
```

## Best Practices

### Checkpoint Placement
- Place checkpoints at natural progression points
- Ensure adequate spacing between checkpoints
- Always have at least one starting checkpoint
- Test checkpoint activation radius in-game

### Performance Considerations
- Use appropriate check intervals for DepthChecker (0.5s default is good)
- Limit number of checkpoints in single scene (< 50 recommended)
- Use efficient LayerMasks for player detection

### Audio Setup
Make sure your AudioManager has these sound effects:
- "CheckpointActivated" - Played when checkpoint is triggered
- "CheatActivated" - Played when cheat codes are used
- "PlayerFallDeath" - Played when player falls too deep
- "PlayerRespawn" - Played when player respawns

## Debugging

### Visual Debug Features
- **Checkpoint Gizmos**: Shows activation radius and checkpoint markers
- **CheckpointManager Gizmos**: Shows connections between checkpoints
- **DepthChecker Gizmos**: Shows death depth plane and player position

### Debug Methods
```csharp
// Log detailed checkpoint information
CheckpointManager.Instance.DebugLogCheckpointInfo();

// Force respawn for testing
DepthChecker depthChecker = FindObjectOfType<DepthChecker>();
depthChecker.ForceRespawn();

// Get current player depth
float depth = depthChecker.GetCurrentPlayerDepth();
```

### Common Issues and Solutions

**Player not detected by checkpoints:**
- Ensure player has "Player" tag
- Check LayerMask settings on checkpoint
- Verify activation radius is large enough

**Checkpoints not working:**
- Ensure CheckpointManager exists in scene
- Check that player object is found correctly
- Verify checkpoint IDs are unique

**Depth checking not working:**
- Ensure DepthChecker component is active
- Check that enableDepthChecking is true
- Verify player object has "Player" tag

**Cheat codes not working:**
- Ensure CheatCodeManager exists and is active
- Check that input is being registered
- Verify AudioManager is present for sound feedback

## API Reference

### Public Properties
```csharp
// Checkpoint
public string CheckpointId { get; }
public bool IsActivated { get; }
public bool IsStartingCheckpoint { get; }

// CheckpointManager
public static CheckpointManager Instance { get; }

// DepthChecker
public float GetCurrentPlayerDepth()
public float GetDeathDepth()
public bool IsDepthCheckingEnabled()
public bool IsRespawning()
```

### Public Methods
```csharp
// Checkpoint
public void ActivateCheckpoint(bool silent = false)
public void RespawnPlayerHere()
public CheckpointData GetCheckpointData()

// CheckpointManager
public void RespawnAtCurrentCheckpoint()
public void TeleportToNearestCheckpoint()
public Checkpoint FindNearestCheckpoint(Vector3 position)
public Checkpoint GetCurrentCheckpoint()

// DepthChecker
public void SetDeathDepth(float newDepth)
public void EnableDepthChecking(bool enable)
public void ForceRespawn()
```

This checkpoint system provides a robust foundation for save/respawn mechanics in Unity games, with easy integration, comprehensive debugging tools, and flexible configuration options.
