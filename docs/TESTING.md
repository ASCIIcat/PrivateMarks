# Acceptance checklist

Automated checks exercise the shared production mark store and edge placement
geometry, with no game process required. They cover different worlds with the
same name, repeated symbols, eight-mark limit, replacement at capacity, toggle
clear, temporary cleanup, serialization/restore, invalid identities, extreme
projection coordinates, viewport origins, resize and invalid projections.

The following checks require FFXIV with Dalamud. On 2026-09-21 the author reported
successful in-game testing, working directionals and distance, and extensive PvP
use. See VALIDATION.md. This checklist remains for regression testing; individual
item results were not supplied.

## Ordinary-area smoke test

1. Enable the built development plugin; check `/xllog` for load errors.
2. `/pmark` opens/closes a compact resizable window. Open Settings through the
   plugin installer and `/pmark settings`.
3. While solo, expand Nearby / Test Players and manually mark yourself. Confirm
   all four shapes render. Walk, jump and rotate the camera. This exercises the
   real production renderer, not synthetic actor positions.
4. Mark a nearby consenting test partner. Close the hotbar; their marker should
   remain. Move both players and rotate the camera across every viewport edge.
   Verify left/right arrows with the partner behind the camera. At the exact
   camera-axis degeneracy the arrow may disappear; it must not pick a fake side.
5. Mark up to eight players, including multiple hearts. The ninth assignment
   must show a clear capacity message. Change an existing icon at capacity.
6. Crowd marked players together. Labels should pack vertically and connector
   lines should preserve on-screen association. Check arrows at every corner.
7. Resize the game window, move it between monitors and vary Dalamud UI scale.
   Labels, clipping and padding must remain within the game viewport.
8. Move the partner beyond actor loading range or have them teleport away. Their
   row must say Not located and their overlay/arrow/distance must disappear.
9. Return within range: restore without another click. Kill/raise a test partner
   in suitable content and check the same identity remains marked.
10. Create one remembered mark and one temporary mark. Change territory: only
    the remembered assignment remains. Reload the plugin/restart the game:
    remembered assignment restores and no temporary assignment is serialized.
11. Uncheck Remember on a marked row, reload, and confirm it does not return.
    Clear All, reload, and confirm all assignments remain removed.
12. Hide the game UI; verify overlays disappear. Check loading and cutscenes.
13. Join a party: party members appear with world names, including unlocated
    members. Compare two same-name players on distinct home worlds if available.

## Frontline acceptance

Manually mark friends in the party and loaded alliance players in Nearby. Verify
fast selection/search, motion with many loaded actors, eight marks, behind-camera
arrows, respawn and unloading. Check frame responsiveness and label readability.
Ask a second client to verify that no real target marker is created or visible.
Full alliance roster discovery is deliberately not an acceptance claim for V1.

## Future release validation

For future changes, record the game/Dalamud versions and relevant checklist
results, including any failures and screenshots, in the validation report.
