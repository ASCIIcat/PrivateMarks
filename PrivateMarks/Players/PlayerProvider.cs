using Dalamud.Game.ClientState.Objects.SubKinds;
using PrivateMarks.Core;

namespace PrivateMarks.Players;

internal sealed class PlayerProvider
{
    public List<PlayerRow> Rows { get; } = new();
    public Dictionary<PlayerIdentity, PlayerRow> Loaded { get; } = new();
    public System.Numerics.Vector3? LocalPosition { get; private set; }
    public void Clear() { Rows.Clear(); Loaded.Clear(); LocalPosition = null; }

    // Called inside UiBuilder.Draw: never retain game-object wrappers between frames.
    public void Refresh()
    {
        Clear();
        var local = Services.Objects.LocalPlayer;
        if (local == null) return;
        LocalPosition = local.Position;
        foreach (var actor in Services.Objects.OfType<IPlayerCharacter>())
        {
            var id = new PlayerIdentity(actor.Name.TextValue, actor.HomeWorld.RowId);
            if (!id.IsValid || !EdgeGeometry.Finite(actor.Position)) continue;
            Loaded[id] = new(id, actor.HomeWorld.Value.Name.ToString(), "Nearby", actor.Position);
        }
        var seen = new HashSet<PlayerIdentity>();
        foreach (var member in Services.Party)
        {
            var id = new PlayerIdentity(member.Name.TextValue, member.World.RowId);
            if (!id.IsValid || !seen.Add(id)) continue;
            Loaded.TryGetValue(id, out var actor);
            Rows.Add(new(id, member.World.Value.Name.ToString(), "Party", actor?.Position));
        }
        // V1 intentionally avoids address-based alliance enumeration. Alliance players
        // still appear under Nearby when loaded; no stale roster positions are consumed.
        foreach (var row in Loaded.Values.OrderBy(r => r.Identity.Name))
            if (seen.Add(row.Identity)) Rows.Add(row);
    }
}
