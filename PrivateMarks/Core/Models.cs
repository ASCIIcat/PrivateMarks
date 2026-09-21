using System.Numerics;

namespace PrivateMarks.Core;

public readonly record struct PlayerIdentity(string Name, uint HomeWorld)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && HomeWorld != 0;
}

public enum MarkerKind { Heart, Star, Diamond, Circle }
public sealed record Mark(PlayerIdentity Identity, MarkerKind Kind, bool Persistent);
public sealed record PlayerRow(PlayerIdentity Identity, string WorldName, string Group, Vector3? Position);

public sealed class MarkStore
{
    public const int Capacity = 8;
    private readonly Dictionary<PlayerIdentity, Mark> marks = new();
    public IReadOnlyCollection<Mark> All => marks.Values;
    public Mark? Get(PlayerIdentity id) => marks.GetValueOrDefault(id);
    public bool Toggle(PlayerIdentity id, MarkerKind kind, bool persistent)
    {
        if (!id.IsValid || !Enum.IsDefined(kind)) return false;
        if (Get(id)?.Kind == kind) { marks.Remove(id); return true; }
        if (!marks.ContainsKey(id) && marks.Count >= Capacity) return false;
        marks[id] = new(id, kind, Get(id)?.Persistent ?? persistent);
        return true;
    }
    public void Restore(IEnumerable<Mark> saved)
    {
        foreach (var mark in saved.Take(256))
            if (mark is { Persistent: true } && mark.Identity.IsValid && Enum.IsDefined(mark.Kind) && marks.Count < Capacity)
                marks[mark.Identity] = mark;
    }
    public void Clear(PlayerIdentity id) => marks.Remove(id);
    public void ClearAll() => marks.Clear();
    public void ClearTemporary()
    {
        foreach (var id in marks.Values.Where(m => !m.Persistent).Select(m => m.Identity).ToArray()) marks.Remove(id);
    }
    public void SetPersistent(PlayerIdentity id, bool value)
    {
        if (marks.TryGetValue(id, out var mark)) marks[id] = mark with { Persistent = value };
    }
}

public static class EdgeGeometry
{
    public static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    public static bool Finite(Vector3 p) => float.IsFinite(p.X) && float.IsFinite(p.Y) && float.IsFinite(p.Z);
    public static bool TryPlace(Vector2 projected, Vector2 origin, Vector2 size, Vector2 inset,
        out Vector2 point, out Vector2 direction)
    {
        point = direction = default;
        if (!Finite(projected) || !Finite(size) || size.X < 100 || size.Y < 100) return false;
        var half = size / 2;
        var delta = projected - (origin + half);
        var magnitude = MathF.Max(MathF.Abs(delta.X), MathF.Abs(delta.Y));
        // Exactly along the camera axis has no unique screen direction. Do not invent one.
        if (magnitude < 0.01f) return false;
        direction = Vector2.Normalize(delta / magnitude);
        var bounds = Vector2.Max(new Vector2(10), half - Vector2.Min(inset, half - new Vector2(10)));
        var factor = MathF.Min(MathF.Abs(direction.X) < 0.0001f ? float.PositiveInfinity : bounds.X / MathF.Abs(direction.X),
            MathF.Abs(direction.Y) < 0.0001f ? float.PositiveInfinity : bounds.Y / MathF.Abs(direction.Y));
        point = origin + half + direction * factor;
        return Finite(point);
    }
}
