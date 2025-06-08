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

### Error Handling
- Graceful fallbacks when required components are missing
- Debug warnings for missing managers or components
- Multiple fallback systems for critical functions like checkpoint teleportation

## Notes
- All cheat codes require the CheatCodeManager to be present in the scene
- Some cheats require specific game components (CheckpointManager, AudioManager, etc.)
- Cheat codes are persistent across scene changes due to singleton pattern
- Super speed cheat includes sophisticated physics handling to prevent common issues with high-speed movement
