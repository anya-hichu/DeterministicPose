# DeterministicPose

Dalamud plugin that provides a deterministic pose commands.

## Installation

Installable using my custom repository (https://github.com/anya-hichu/DalamudPluginRepo) or from compiled archives.

## Commands

### DPose

`/dpose [Index]`: cpose with index

### Idle

- `/idle`: assume idle pose (standing up)

### If in that position (taken from game naming)

`/ifinthatposition -(?|!|$|v)( [Command])*` (quoting support)

Flags:
 - `-v`: Invert condition
 - `-?`: verbose mode - display in your local chatlog any text (including commands) that are sent to the server
 - `-!`: dry run - the same as verbose mode, except nothing is actually sent to the server
 - `-$`: abort the currently running macro when true (by using the /macrocancel command)
 - `-@`: abort when false

(inspired by https://github.com/PrincessRTFM/TinyCommands)