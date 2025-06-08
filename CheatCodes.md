# Cheat Codes Documentation

This document lists all available cheat codes for Echo Nomads game.

## How to Use Cheat Codes

Simply type the cheat code while playing the game. The system will automatically detect when you've typed a valid cheat code and activate it immediately.

- **Input Method**: Type letters directly during gameplay
- **Case Sensitivity**: Not case sensitive (codes are converted to lowercase)
- **Feedback**: Debug messages and audio feedback when codes are activated

## Available Cheat Codes

### 1. `superspeed`
**Effect**: Activates super speed mode for the player character
- Increases sprint speed from normal (6 units) to cheat speed (60 units)
- Applies enhanced grounding physics to prevent issues at high speeds
- Modifies gravity and ground detection for better control

**Usage**: Type `superspeed` during gameplay
**Audio**: Plays "CheatActivated" sound effect

### 2. `normalspeed`
**Effect**: Deactivates super speed mode and returns to normal movement
- Restores sprint speed to normal values
- Restores original grounding physics
- Disables super speed modifications

**Usage**: Type `normalspeed` during gameplay (only works when super speed is active)
**Audio**: Plays "CheatActivated" sound effect

### 3. `reset`
**Effect**: Teleports the player to the nearest checkpoint
- Uses CheckpointManager to find and teleport to the closest checkpoint
- Includes fallback system to manually find nearest checkpoint if CheckpointManager is unavailable
- Useful for getting unstuck or quickly returning to a safe location

**Usage**: Type `reset` during gameplay
**Audio**: Plays "CheatActivated" sound effect

### 4. `clearsave`
**Effect**: Clears all save data for a fresh start
- Clears checkpoint save data
- Clears player position save data
- Clears puzzle save data (ResonancePuzzle objects)
- Clears level save data (LevelPuzzleManager objects)
- Game will restart fresh on next load

**Usage**: Type `clearsave` during gameplay
**Audio**: Plays "CheatActivated" sound effect

### 5. `autosolve`
**Effect**: Automatically activates resonators within the player's range

- Finds all resonators in the current scene
- Checks if the player is within each resonator's activation radius
- Automatically activates any resonators that are in range and not already activated
- Uses advanced reflection techniques to access private resonator methods
- Useful for quickly solving puzzles or testing puzzle completion

**Usage**: Type `autosolve` during gameplay when near resonators
**Audio**: Plays "CheatActivated" sound effect
**Note**: Only works on resonators within their activation radius from the player

## Game Systems Documentation

### Scene Transition System

### Scene Transition System

The game includes multiple components for handling scene transitions when the player touches specific areas or objects.

#### Available Scene Transition Components

1. **SimpleSceneTransition**:
   - Easiest to use for basic scene changes
   - Attach to any GameObject with a trigger collider
   - Player touches trigger → scene changes after delay
   - Includes audio feedback

2. **SceneTransitionTrigger**:
   - Advanced scene transition with many options
   - Supports loading screens, fade effects, and player requirements
   - Can save game progress before transition
   - Includes visual effects and audio feedback

3. **SceneTransitionManager**:
   - Singleton manager for scene transitions with fade effects
   - Handles fade in/out animations
   - Supports loading bars and progress display
   - Persists between scenes

#### How to Set Up Scene Transitions

##### Method 1: Simple Setup (Recommended for beginners)

1. Create a plane or invisible trigger area where you want the transition
2. Add `SimpleSceneTransition` component
3. Set the `Target Scene Name` to your destination scene
4. Ensure the plane has a Collider with "Is Trigger" checked
5. Test by walking your player into the trigger area

##### Method 2: Advanced Setup (For complex transitions)

1. Create your trigger object (plane, invisible box, etc.)
2. Add `SceneTransitionTrigger` component
3. Configure all the settings (scene name, delays, effects, etc.)
4. Optionally add `SceneTransitionManager` to your scene for fade effects
5. Set up loading screens if desired

#### Use Cases for Scene Transitions

- **Level Progression**: Moving between game levels
- **Area Transitions**: Entering buildings, caves, or new regions
- **Teleporters**: Magic portals or technological transport
- **Story Progression**: Cutscene transitions or story beats
- **Menu Systems**: Returning to main menu or loading screens

### Depth Checker System

The game includes a depth checking system that automatically respawns the player if they fall below a certain depth (default: -50 units). This system can be configured and disabled in specific areas of the map.

#### Zone-Based Depth Check Disabling

You can disable depth checking in specific areas using several methods:

1. **DepthCheckDisableZone Component**:
   - Attach to any GameObject to create a disable zone
   - Supports Box, Sphere, and Trigger zone types
   - Automatically registers with the DepthChecker
   - Visual gizmos show zone boundaries in editor

2. **DepthCheckDisableTrigger Component**:
   - Simple trigger-based solution
   - Attach to any GameObject with a trigger collider
   - Automatically disables depth checking when player enters

3. **Manual Control**:
   - Use `DepthChecker.DisableDepthCheckingInArea(true/false)` in scripts
   - Useful for cutscenes or special gameplay sections

#### Use Cases for Disabling Depth Checking

- **Swimming Areas**: Deep water sections where falling is intentional
- **Cave Systems**: Underground areas with legitimate deep sections
- **Puzzle Areas**: Sections where controlled falling is part of gameplay
- **Cutscenes**: Story moments where depth checking would interfere
- **Special Gameplay**: Areas with unique mechanics like flying or teleportation

## Technical Details

### Super Speed Physics

When super speed is active, the system applies special physics modifications:

- **Extra Gravity**: -50 units (increased downward force)
- **Ground Max Distance**: 0.1 units (tighter ground detection)
- **Slope Handling**: Additional grounding forces on slopes to maintain contact
- **Velocity Limiting**: Prevents excessive vertical velocity (clamped to 2 units upward)

### Input System

- **Buffer Length**: 15 characters maximum
- **Character Filtering**: Only letters are processed
- **Continuous Checking**: Checks for valid codes after each letter input
- **Rolling Buffer**: Maintains last 15 characters for pattern matching

### Depth Checker Configuration

- **Death Depth**: Default -50 units (configurable)
- **Check Interval**: 0.5 seconds (configurable for performance)
- **Zone System**: Supports overlapping disable zones
- **Debug Mode**: Visual gizmos and detailed logging available

### Error Handling

- Graceful fallbacks when required components are missing
- Debug warnings for missing managers or components
- Multiple fallback systems for critical functions like checkpoint teleportation
- Automatic player re-finding if references are lost

### AutoSolve Cheat Implementation

- **Range Detection**: Uses each resonator's individual `activationRadius` property
- **Reflection System**: Accesses private `isActivated` field and `ActivateResonator()` method
- **Safety Checks**: Validates resonator components and methods before attempting activation
- **Smart Activation**: Only activates resonators that are not already activated
- **Debug Logging**: Comprehensive logging for troubleshooting and feedback

## Notes

- All cheat codes require the CheatCodeManager to be present in the scene
- Some cheats require specific game components (CheckpointManager, AudioManager, etc.)
- Cheat codes are persistent across scene changes due to singleton pattern
- Super speed cheat includes sophisticated physics handling to prevent common issues with high-speed movement
- Depth checking can be globally disabled or selectively disabled in specific map areas
- Multiple disable zones can overlap and are managed with a counter system
- **Scene transition system is fully implemented and functional** - all components work together seamlessly with proper checkpoint and player position saving, no compilation errors
