# DeterministicPose

Dalamud plugin that provides a deterministic pose commands.

## Installation

Installable using my custom repository (instructions here: https://github.com/anya-hichu/DalamudPluginRepo) or from compiled archives.

## Commands

- `/dpose <index>`: cpose with index
- `/standup`: assume standup position
- `/untarget`: clear current target
- `/ifproximity <player name>`: sends /macrocancel if not in proximity 

### If in that position (taken from enum name)

- `/ifinthatposition -(v|?|!|$)( [Command])*` (quoting support)

Flags:
 - `-v`: Invert condition
 - `-?`: verbose mode - display in your local chatlog any text (including commands) that are sent to the server
 - `-!`: dry run - the same as verbose mode, except nothing is actually sent to the server
 - `-$`: abort using /macrocancel command

(inspired by https://github.com/PrincessRTFM/TinyCommands)
