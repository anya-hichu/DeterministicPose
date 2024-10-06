# DeterministicPose

Dalamud plugin that provides a deterministic pose commands.

## Installation

Installable using my custom repository (instructions here: https://github.com/anya-hichu/DalamudPluginRepo) or from compiled archives.

## Commands

### DPose

`/dpose [Index]`: cpose with index

### Standup

`/standup`: assume standup position

### If in that position (taken from game naming)

`/ifinthatposition -(v|?|!|$)( [Command])*` (quoting support)

Flags:
 - `-v`: Invert condition
 - `-?`: verbose mode - display in your local chatlog any text (including commands) that are sent to the server
 - `-!`: dry run - the same as verbose mode, except nothing is actually sent to the server
 - `-$`: abort using /macrocancel command

(inspired by https://github.com/PrincessRTFM/TinyCommands)
