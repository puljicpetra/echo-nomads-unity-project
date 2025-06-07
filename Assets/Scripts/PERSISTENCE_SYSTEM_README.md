# Echo Nomads - Persistence System Implementation

## Overview
This persistence system allows the game to save and restore checkpoint discovery states, puzzle completion progress, and player position between game sessions. The system uses Unity's PlayerPrefs for cross-platform compatibility.

## Components

### 1. CheckpointManager.cs (Enhanced)
**Main Features:**
- Saves discovered and activated checkpoint states
- Restores checkpoint progress on game startup
- Maintains active and last activated checkpoint references
- Automatic save when checkpoints are activated

**Key Methods:**
- `SaveCheckpointData()` - Saves all checkpoint states to PlayerPrefs
- `LoadCheckpointData()` - Loads saved states on startup
- `FinalizeCheckpointLoading()` - Completes loading after registration
- `ClearSaveData()` - Clears all checkpoint save data

**Settings:**
- `enablePersistence` - Toggle persistence on/off
- `saveDataPrefix` - Prefix for save keys (default: "EchoNomads_")

### 2. Checkpoint.cs (Enhanced)
**Main Features:**
- Individual checkpoint state tracking
- Discovered vs Activated state separation
- Automatic discovery marking on activation

**Key Properties:**
- `IsDiscovered` - Whether checkpoint has been found
- `IsActivated` - Whether checkpoint has been used as spawn point
- Persistence support through CheckpointManager

### 3. ResonancePuzzle.cs (Enhanced)
**Main Features:**
- Saves puzzle solved state and activation count
- Restores puzzle state on scene load
- Automatic save when puzzle is completed
- Disabled resonators for solved puzzles

**Key Methods:**
- `SavePuzzleState()` - Saves puzzle completion and progress
- `LoadPuzzleState()` - Restores puzzle state on startup
- `ClearSaveData()` - Clears puzzle save data

**Settings:**
- `puzzleId` - Unique identifier for the puzzle
- `enablePersistence` - Toggle persistence on/off

### 4. LevelPuzzleManager.cs (Enhanced)
**Main Features:**
- Saves level completion state
- Tracks number of solved puzzles
- Restores reward object states
- Automatic save on level completion

**Key Methods:**
- `SaveLevelState()` - Saves level progress and completion
- `LoadLevelState()` - Restores level state and activates rewards
- `ClearSaveData()` - Clears level save data

**Settings:**
- `levelId` - Unique identifier for the level
- `enablePersistence` - Toggle persistence on/off

### 5. PlayerPersistence.cs (New)
**Main Features:**
- Saves player position and rotation
- Scene-aware loading (only loads if same scene)
- Automatic save on checkpoint activation
- Safe teleportation with physics handling

**Key Methods:**
- `SavePlayerPosition()` - Saves current player transform
- `LoadPlayerPosition()` - Restores saved player position
- `RestorePlayerPosition()` - Safely teleports player
- `ClearSaveData()` - Clears player position data

**Settings:**
- `enablePersistence` - Toggle persistence on/off
- `saveOnCheckpointActivation` - Auto-save on checkpoint use
- `saveDataPrefix` - Prefix for save keys

### 6. CheatCodeManager.cs (Enhanced)
**Main Features:**
- Added "clearsave" cheat code
- Clears all persistence data across all systems
- Useful for testing and resetting progress

**Usage:**
- Type "clearsave" to clear all save data
- Affects checkpoints, puzzles, levels, and player position

## Save Data Structure

### PlayerPrefs Keys:
```
[Prefix]ActiveCheckpoint - Currently active checkpoint ID
[Prefix]LastActivatedCheckpoint - Last manually activated checkpoint
[Prefix]DiscoveredCheckpoints - Comma-separated discovered checkpoint IDs
[Prefix]ActivatedCheckpoints - Comma-separated activated checkpoint IDs
[Prefix]UseNearestAsActive - Whether nearest mode is enabled
[Prefix]SaveTime - Timestamp of last save

[Prefix]Puzzle_[ID]_Solved - Whether puzzle is solved (0/1)
[Prefix]Puzzle_[ID]_Activations - Number of activations
[Prefix]Puzzle_[ID]_SaveTime - Puzzle save timestamp

[Prefix]Level_[ID]_Completed - Whether level is completed (0/1)
[Prefix]Level_[ID]_SolvedPuzzles - Number of solved puzzles
[Prefix]Level_[ID]_SaveTime - Level save timestamp

[Prefix]Player_PosX/Y/Z - Player position coordinates
[Prefix]Player_RotX/Y/Z - Player rotation angles
[Prefix]Player_Scene - Scene where position was saved
[Prefix]Player_SaveTime - Player position save timestamp
```

## Setup Instructions

### For New Checkpoints:
1. Add Checkpoint component to GameObject
2. Set unique `checkpointId` in inspector
3. CheckpointManager will handle persistence automatically

### For New Puzzles:
1. Add ResonancePuzzle component 
2. Set unique `puzzleId` in inspector
3. Set `enablePersistence = true`
4. Puzzle state will be saved/loaded automatically

### For Level Managers:
1. Add LevelPuzzleManager component
2. Set unique `levelId` in inspector
3. Set `enablePersistence = true`
4. Configure puzzle-reward pairs as usual

### For Player:
1. Add PlayerPersistence component to player GameObject
2. Assign vThirdPersonController reference
3. Set persistence settings as desired
4. Player position will be saved on checkpoint activation

## Testing

### Manual Testing:
- Use Context Menu options in editor:
  - CheckpointManager: "Debug Log Checkpoint Info"
  - ResonancePuzzle: "Save/Load/Clear" options
  - PlayerPersistence: "Save/Load Player Position"

### Cheat Codes:
- `clearsave` - Clears all save data
- `reset` - Teleport to nearest checkpoint

### Debug Logging:
- Enable `debugMode` in CheckpointManager for detailed logs
- All persistence operations are logged to console
- Error handling with try-catch blocks

## Migration Notes

### Existing Save Compatibility:
- New persistence system uses different keys
- Old progress will not be affected
- Players can continue from where they left off

### Performance Considerations:
- PlayerPrefs operations are fast but blocking
- Save operations only occur on state changes
- Loading only happens once per scene start
- All operations include error handling

## Error Handling

### Common Issues:
1. **Missing Components**: Auto-detection and null checks
2. **Save Failures**: Try-catch with error logging  
3. **Load Failures**: Graceful fallback to default state
4. **Scene Mismatches**: Player position only loads in same scene

### Debug Information:
- All save/load operations are logged
- Timestamps included in save data
- Error messages include specific failure reasons
- Context menu options for manual testing

## Future Enhancements

### Possible Additions:
1. **Save Slots**: Multiple save files
2. **Cloud Saves**: Steam Cloud integration
3. **Encrypted Saves**: Save data protection
4. **Compressed Data**: More efficient storage
5. **Version Migration**: Handle save format changes
6. **Auto-Save Intervals**: Periodic saving
7. **Save Validation**: Detect corrupted saves
