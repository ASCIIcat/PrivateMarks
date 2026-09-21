# API decisions

Verified 2026-09-19 against official source/documentation and the official latest
Dalamud distribution (assembly package 15.0.3.5). Compilation is against the real
assemblies, not substitute stubs.

| Need | API and decision |
| --- | --- |
| Party roster | `IPartyList` enumeration, `IPartyMember.Name` and `World.RowId`. Source maps `World` to `HomeWorld`, not visiting world. |
| Loaded players | Enumerate `IObjectTable`, keep `IPlayerCharacter` values only. Match exact `Name.TextValue` plus `HomeWorld.RowId`. |
| Position | Copy current loaded actor `Position` during Draw. Never retain wrappers between frames; never read `IPartyMember.Position`. |
| Local position | `IObjectTable.LocalPlayer.Position`; finite-value check before displaying distance. |
| Alliance | `IsAlliance` exists, but extra slots require `GetAllianceMemberAddress` / `CreateAllianceMemberReference`. V1 uses party + Nearby and does not touch addresses. |
| Projection | `IGameGui.WorldToScreen(Vector3, out Vector2, out bool)` returns front-of-camera status separately from viewport membership. |
| Overlay | `UiBuilder.Draw`, `ImGui.GetBackgroundDrawList`, main viewport position/size and a clip rectangle. WindowSystem handles the hotbar. |
| Transitions | `IClientState.IsLoggedIn`, `TerritoryType`, `ICondition` BetweenAreas/BetweenAreas51; clear position snapshots on every refresh or suppression. |

The current `GameGui.WorldToScreen` implementation divides clip coordinates by
the **absolute** W. Thus screen-space direction is already sign-preserved behind
the camera; negating it again would point to the wrong side. It also includes the
main viewport's origin. The renderer must not add the viewport origin twice.
On exact W=0 the method returns zero; this sentinel is suppressed. On the camera
axis, no unique edge direction exists and the geometry routine suppresses it.
This behavior should be rechecked when upgrading Dalamud.

Only fresh loaded positions are used, even if the roster still contains an
unloaded member. Visibility outside the viewport and presence in the object
table are distinct concepts. No unsupported camera data is needed.

Official sources:

- [Current sample project / SDK](https://github.com/goatcorp/SamplePlugin/blob/master/SamplePlugin/SamplePlugin.csproj)
- [IPartyList](https://dalamud.dev/api/Dalamud.Plugin.Services/Interfaces/IPartyList/)
- [PartyMember implementation](https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/Party/PartyMember.cs)
- [IPlayerCharacter](https://dalamud.dev/api/Dalamud.Game.ClientState.Objects.SubKinds/Interfaces/IPlayerCharacter/)
- [IGameGui](https://dalamud.dev/api/Dalamud.Plugin.Services/Interfaces/IGameGui/)
- [Projection implementation](https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/Gui/GameGui.cs)

These references describe the API investigated; the checked-in SDK version and
lock file control the build. Runtime game validation remains separate.
