# Fall Death Trigger System

## Overview

The Fall Death Trigger system replaces the global depth checking approach with efficient, zone-based triggers. Instead of constantly monitoring the player's Y position across the entire level, you now place specific trigger zones where you want fall death detection to occur.

## Key Benefits

✅ **Better Performance**: No constant global position checking  
✅ **More Flexible**: Place triggers exactly where needed  
✅ **Easier Setup**: Visual trigger zones in the editor  
✅ **Precise Control**: Different behaviors per zone  
✅ **Backwards Compatible**: Legacy system still available  

## Quick Setup

### 1. Add FallDeathTriggerSetup Component
- Add `FallDeathTriggerSetup` to any GameObject in your scene
- Click "Setup Fall Death System" in the inspector
- This ensures you have a DepthChecker for trigger management

### 2. Create Fall Death Triggers
Use the context menu options on `FallDeathTriggerSetup`:

- **"Create Fall Death Trigger Here"** - Creates a trigger at the setup object's position
- **"Create Fall Death Trigger Below Scene"** - Automatically finds the lowest point and creates a large trigger below it
- **"Create Multiple Fall Death Triggers"** - Creates a grid of triggers
- **"Convert Legacy DepthChecker"** - Converts your old depth-based system to trigger-based

### 3. Configure Triggers
Each `FallDeathTrigger` component has these settings:

- **Trigger Name**: Identifier for debugging
- **Teleport To Nearest Checkpoint**: When enabled, teleports to closest checkpoint
- **Specific Respawn Point**: Alternative to nearest checkpoint (optional)
- **Audio Settings**: Fall death and respawn sound names
- **Visual Settings**: Gizmo appearance in editor

## Components

### FallDeathTrigger.cs
Individual trigger zones that detect when players fall into them.

**Key Features:**
- Automatic teleportation to nearest checkpoint
- Optional specific respawn points
- Audio feedback on fall and respawn
- Visual gizmos for easy placement
- Prevention of multiple triggers

**Usage:**
```csharp
// Get reference to trigger
FallDeathTrigger trigger = GetComponent<FallDeathTrigger>();

// Configure programmatically
trigger.SetTeleportToNearest(true);
trigger.SetTriggerSize(new Vector3(20f, 5f, 20f));
trigger.EnableTrigger(true);

// Listen to events
trigger.OnPlayerFellIntoTrigger += (player) => {
    Debug.Log("Player fell into trigger!");
};
```

### DepthChecker.cs (Reworked)
Now manages fall death triggers instead of global depth monitoring.

**New Functionality:**
- Automatically finds and registers all FallDeathTrigger components
- Manages trigger events and coordination
- Provides backwards compatibility with legacy depth checking
- Centralized respawn management

**Migration:**
- Old depth checking is now called "Legacy" and disabled by default
- All trigger zones are automatically managed
- Events still work the same way for other systems

### FallDeathTriggerSetup.cs
Editor utility for easy trigger creation and system setup.

**Features:**
- One-click system setup
- Multiple trigger creation patterns
- Legacy system conversion
- System validation and debugging

## Best Practices

### Trigger Placement
- Place triggers in natural fall areas (cliffs, platforms, water)
- Use larger triggers in open areas, smaller ones for specific hazards
- Overlap triggers slightly to ensure coverage
- Test trigger sizes during gameplay

### Performance Tips
- Use fewer, larger triggers instead of many small ones
- Place triggers only where players can actually fall
- Avoid triggers in safe areas where players can't reach

### Audio Setup
Make sure your AudioManager has these sounds:
- "PlayerFallDeath" - Played when falling into trigger
- "PlayerRespawn" - Played after respawn

## Migration from Old System

### Automatic Migration
1. Add `FallDeathTriggerSetup` to your scene
2. Right-click and select "Convert Legacy DepthChecker"
3. This creates a large trigger at your old death depth
4. Legacy depth checking is automatically disabled

### Manual Migration
1. Note your current death depth value
2. Create triggers at strategic fall locations
3. Set trigger Y positions slightly above your old death depth
4. Disable legacy depth checking in DepthChecker inspector
5. Test all fall areas in your level

## Debugging

### Visual Debugging
- Enable "Debug Mode" on DepthChecker to see trigger connections
- FallDeathTrigger gizmos show trigger boundaries
- Red wireframes indicate trigger areas

### Context Menu Tools
Use these on `FallDeathTriggerSetup`:
- **"Find All Fall Death Triggers"** - Lists all triggers in console
- **"Validate Fall Death System"** - Checks system integrity
- **"Setup Fall Death System"** - Ensures proper setup

### Common Issues

**Player not teleporting:**
- Check if CheckpointManager exists and has checkpoints
- Verify player has "Player" tag
- Ensure trigger collider is marked as trigger
- Check if trigger is enabled

**Multiple respawns:**
- This is prevented automatically by the trigger system
- Each trigger waits 2 seconds before allowing another fall

**Audio not playing:**
- Check AudioManager exists in scene
- Verify sound names match AudioManager clips
- Test audio outside of fall death system

## Events

The system provides events for integration with other systems:

```csharp
// DepthChecker events (same as before)
DepthChecker depthChecker = FindObjectOfType<DepthChecker>();
depthChecker.OnPlayerFellTooDeep += () => {
    Debug.Log("Player fell!");
};

// Individual trigger events
FallDeathTrigger trigger = GetComponent<FallDeathTrigger>();
trigger.OnPlayerFellIntoTrigger += (player) => {
    Debug.Log($"Player {player.name} fell into {trigger.GetTriggerName()}");
};

trigger.OnPlayerRespawned += (player) => {
    Debug.Log($"Player {player.name} respawned");
};
```

## Example Scenarios

### Cliff Areas
Create a long, thin trigger along cliff edges:
```
Size: (100, 5, 5)
Position: At cliff base
```

### Water Areas
Create a flat trigger just below water surface:
```
Size: (50, 2, 50)  
Position: Slightly below water level
```

### Platforming Sections
Create multiple small triggers under platforms:
```
Size: (10, 5, 10)
Position: Below each platform cluster
```

### Open World Fallback
Create one large trigger far below the playable area:
```
Size: (1000, 10, 1000)
Position: Y = -100 (or below lowest terrain)
```

This new system gives you much more control and better performance while maintaining the same functionality your game already expects!
