# Dalamud Mount Search Plugin
A Final Fantasy XIV Dalamud plugin that shows information about other players' mounts and their acquisition methods via the target's context menu button.

The plugin has a configuration that is controlled by the Dalamud QL XIV environment.
* Type `/mountsconfig` in the in-game chat command to open the configuration window or use the main Dalamud plugin window with the `Settings` button.

Available options:
* **Show Obtainable**: Indicates whether it is currently possible to obtain this mount in the game.
* **Show Number of Seats**: Displays the number of people that can ride the mount.

The code uses data from the [FFXIV wiki](https://ffxiv.consolegameswiki.com/wiki/FF14_Wiki) to get detailed information about the searched mount.

To Install the plugin itself use raw json as experimental link
`https://raw.githubusercontent.com/megurte/WhichMount/master/WhichMount/WhichMount.json`
