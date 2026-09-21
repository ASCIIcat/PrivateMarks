# Validation report — V1 release

## In-game validation — 2026-09-21

AdelaideTheBun reports successful in-game testing, confirms directional indicators
and distance work, and reports extensive PvP use. The author considers V1 ready
for release. This is user-reported runtime validation, not an automated game test.
Individual checklist results and exact runtime game/Dalamud versions were not
recorded. Full alliance roster support remains intentionally deferred.

## Build and automated validation

- Official downloaded Dalamud references: 15.0.3.5, API 15.
- .NET SDK: 10.0.401; Dalamud.NET.Sdk: 15.0.0.
- Final Release build: **PASS**, zero warnings and zero errors.
- Core regression executable: **PASS**, 30 assertions.
- Persistence round trips checked with both System.Text.Json and Newtonsoft.Json
  (the serializer family used by Dalamud).
- Generated manifest: `PrivateMarks`, assembly 1.0.0.0, DalamudApiLevel 15.
- Package: `PrivateMarks/bin/Release/PrivateMarks/latest.zip`.
- Runtime code uses no networking, unsafe blocks, memory access, target setters,
  game marker calls, external services, or combat actions.

The acceptance checklist in TESTING.md remains available for future regression
testing; the runtime report above does not assert every checklist item was tested.
