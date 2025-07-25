 # DeterministicPose

Dalamud plugin that provides a deterministic pose commands.

## Installation

Installable using my custom repository (instructions here: https://github.com/anya-hichu/DalamudPluginRepo) or from compiled archives.

## Commands

- `/dpose <index>`: cpose with index or display current index with no argument
- `/standup`: assume standup position
- `/untarget`: clear current target
- `/ifproximity <player name>( <range>)?`: sends /macrocancel if not in proximity (placeholders and quote support for arguments). Proximity range defaults to 1 if not provided (decimal support).
- `/localsync <source player name>( <target player name>)?`: Copy animation local time from source to target. Target defaults to local player if not provided (placeholders and quote support for arguments)
- `/remotesync (<player name>( <delay ms>)?|cancel)`: Resent current emote to sync with someone when his cycle finishes with optional positive/negative ms delay. It's possible to abort current task using cancel subcommand (placeholders and quote support for arguments)
- `/walk (enable|disable|toggle)?`: control walking mode (defaults to enable without argument)

### If in that position
Doze, groundsit and sit are considered as "in that position"

- `/ifinthatposition -(v|?|!|$)( [Command])*` (quoting support)

Flags:
 - `-v`: Invert condition
 - `-?`: verbose mode - display in your local chatlog any text (including commands) that are sent to the server
 - `-!`: dry run - the same as verbose mode, except nothing is actually sent to the server
 - `-$`: abort using /macrocancel command

(inspired by https://github.com/PrincessRTFM/TinyCommands)
