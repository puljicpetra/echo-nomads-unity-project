# Cursor Management Setup Instructions

## Overview
The cursor management system ensures that when you click "Play" in Unity, the mouse cursor is confined to the game window and doesn't escape to interact with Unity's interface.

## Files Created
1. **CursorManager.cs** - Handles all cursor locking and visibility states
2. **GameInitializer.cs** - Ensures core systems are initialized when the game starts

## Setup Instructions

### Step 1: Add GameInitializer to Your Main Scene
1. In your main game scene (probably "Desert scene"), create an empty GameObject
2. Name it "GameInitializer"
3. Add the `GameInitializer` component to it
4. In the inspector, make sure "Enable Cursor Management" is checked

### Step 2: Cursor Behavior Settings
The CursorManager provides several cursor modes:

- **Confined** (Default): Cursor stays within game window but is visible
- **Locked**: Cursor is locked to center and hidden (good for FPS-style camera control)
- **Free**: Cursor can move freely and is visible (good for UI interactions)

### Step 3: Testing
1. Click Play in Unity
2. The cursor should now be confined to the Game window
3. Press **ESC** to toggle between confined and free cursor (for debugging)

### Step 4: Integration with Existing Systems
The system automatically integrates with:
- Your CheatCodeManager (ensures proper cursor state)
- FinalCutsceneController (frees cursor during cutscenes)

## Cursor Controls

### During Gameplay
- Cursor is confined to game window
- Can still interact with game UI elements
- Press ESC to temporarily free cursor for debugging

### During Cutscenes
- Cursor is freed for potential skip buttons or interactions

### For Debugging
- ESC key toggles cursor lock state
- Debug messages in console show cursor state changes

## Customization

### Change Default Behavior
In CursorManager inspector:
- **Gameplay Cursor Lock Mode**: Choose between Confined, Locked, or None
- **Show Cursor During Gameplay**: Toggle cursor visibility
- **Debug Mode**: Enable/disable debug logging

### Manual Control from Code
```csharp
// Confine cursor to game window
CursorManager.Instance.SetGameplayCursorState();

// Free cursor for UI
CursorManager.Instance.SetFreeCursorState();

// Lock cursor to center (for camera control)
CursorManager.Instance.SetLockedCursorState();

// Toggle between states
CursorManager.Instance.ToggleCursorLock();
```

## Troubleshooting

### Cursor Still Escapes Game Window
- Make sure GameInitializer exists in your scene
- Check that CursorManager was created (look for debug messages in console)
- Verify "Enable Cursor Management" is checked in GameInitializer

### Can't Interact with Game UI
- Change cursor lock mode to "Confined" instead of "Locked"
- Enable "Show Cursor During Gameplay" in CursorManager settings

### Need Different Behavior for Different Scenes
- Call `CursorManager.Instance.SetFreeCursorState()` in scenes where you need free cursor
- Call `CursorManager.Instance.SetGameplayCursorState()` to return to game mode

## Notes
- The system preserves original cursor settings and restores them when the game stops
- GameInitializer and CursorManager persist across scene changes
- The system automatically handles window focus events to maintain proper cursor state
