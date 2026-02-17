# Integration for other mods
Modders can add achievements by using the provided wrapper class [available here](/Achievements.cs).

## The basics
Simply include the wrapper: `using Achievements;`

Call `AchievementManager.Init(this);` from the earliest point in your mod, whether that be `OnMenuLoad()`, `DbLoad()` or `OnLoad()`

Register your achievements in the same place you called `Init()` using `AchievementManager.RegisterAchievement("achievement ID", "Achievement name", "Achievement description", maxProgress (optional), isSecret (defaults to false));`

> [!NOTE]
> Achievements registered from outside `OnMenuLoad()` won't display in the achievements UI from the main menu

If `maxProgress` is left `null`, achievement will be registered as a "one shot" achievement that is either locked or unlocked. These achievements can be unlocked either using `AddProgress` or `UnlockAchievement`

## Methods
### `Init()`
Used to initialise the achievement manager

Parameters:
- `mod` - An instance to your mod object

### `RegisterAchievement()`
Used to register an achievement

Parameters:
- `achievementId` - A unique identifier for the achievement
- `name` - The name for the achievement, shown in the UI and in the unlock notification
- `description` - Brief description text for the achievement, shown in the UI
- `maxProgress` - A nullable integer to define the maximum progress for the achievement. Defaults to null. Leave null for "one shot" achievements that are either locked or unlocked
- `isSecret` - A bool to define whether the achievement details should be hidden in the UI until unlocked. Defaults to false

> [!WARNING]
> Any achievements added from `DbLoad()` or `OnLoad()` should be wrapped in a try-catch to ensure an exception isn't thrown when registering again during the same session. (For example, user going back to main menu, then loading another save/starting a new game)

### `AddProgress()`
Used to add progress for a progression achievement, or unlock a one shot achievement

Parameters:
- `achievementId` - The achievement identifier
- `amount` - An integer of how much progress to add. Defaults to 1

If the amount + the existing amount is greater than or equal to the `maxProgress` the achievement will unlock

### `UnlockAchievement()`
Alternative method to unlock one shot achievements. Calling on a progress achievement will throw an `InvalidOperationException` exception

Parameters:
- `achievementId` - The achievement identifier

### `IsUnlocked()`
Check unlock state for a given achievement

Parameters:
- `achievementId` - The achievement identifier

Returns:\
`true` if the achievement is unlocked; otherwise, `false`

### `GetProgress()`
Get the current progress for a given achievement

Parameters:
- `achievementId` - The achievement identifier

Returns:\
An integer representing the current progress
