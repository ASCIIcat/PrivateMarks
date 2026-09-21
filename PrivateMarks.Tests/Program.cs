using System.Numerics;
using System.Text.Json;
using PrivateMarks.Core;

var checks = 0;
void Check(bool passed, string name) { if (!passed) throw new Exception(name); checks++; }
var marks = new MarkStore();
var a = new PlayerIdentity("Same Name", 1);
var b = new PlayerIdentity("Same Name", 2);
Check(marks.Toggle(a, MarkerKind.Heart, true), "assign");
Check(marks.Toggle(b, MarkerKind.Heart, false) && marks.All.Count == 2, "same name different world, duplicate marker");
marks.Toggle(a, MarkerKind.Star, false);
Check(marks.Get(a) is { Kind: MarkerKind.Star, Persistent: true }, "replacing icon keeps persistence");
marks.ClearTemporary();
Check(marks.Get(b) == null && marks.Get(a) != null, "zone cleanup");
var json = JsonSerializer.Serialize(marks.All);
var restored = new MarkStore();
restored.Restore(JsonSerializer.Deserialize<List<Mark>>(json)!);
Check(restored.Get(a) == marks.Get(a), "serialized identity and persistence restoration");
var dalamudJson = Newtonsoft.Json.JsonConvert.SerializeObject(marks.All);
var dalamudRestored = new MarkStore();
dalamudRestored.Restore(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Mark>>(dalamudJson)!);
Check(dalamudRestored.Get(a) == marks.Get(a), "Dalamud Newtonsoft serialization round trip");
for (uint i = 2; i <= 8; i++) marks.Toggle(new("Player", i), MarkerKind.Circle, false);
Check(marks.All.Count == 8 && !marks.Toggle(new("Ninth", 1), MarkerKind.Heart, true), "capacity");
Check(marks.Toggle(a, MarkerKind.Diamond, true), "can change while full");
Check(marks.Toggle(a, MarkerKind.Diamond, true) && marks.Get(a) == null, "toggle clears");
Check(!marks.Toggle(new("", 0), MarkerKind.Heart, true), "invalid identity");
marks.ClearAll(); Check(marks.All.Count == 0, "clear all");
var origin = new Vector2(100, 50);
var size = new Vector2(1920, 1080);
var center = origin + size / 2;
foreach (var delta in new[] { new Vector2(2000, 0), new(0, -2000), new(-2000, 500), new(1e30f, 1e30f), new(0, 2000) })
{
    Check(EdgeGeometry.TryPlace(center + delta, origin, size, new(80, 60), out var point, out var direction), "valid edge");
    Check(point.X >= origin.X + 80 - .01f && point.X <= origin.X + size.X - 80 + .01f && point.Y >= origin.Y + 60 - .01f && point.Y <= origin.Y + size.Y - 60 + .01f, "edge bounds");
    Check(Vector2.Dot(Vector2.Normalize(delta / MathF.Max(MathF.Abs(delta.X), MathF.Abs(delta.Y))), direction) > .999f, "direction retained, including behind-camera signed projection");
}
Check(!EdgeGeometry.TryPlace(center, origin, size, new(30), out _, out _), "ambiguous axis suppressed");
Check(!EdgeGeometry.TryPlace(new(float.NaN, 0), origin, size, new(30), out _, out _), "NaN suppressed");
Check(!EdgeGeometry.TryPlace(new(float.PositiveInfinity, 0), origin, size, new(30), out _, out _), "infinity suppressed");
Check(EdgeGeometry.TryPlace(new(2000, 1000), Vector2.Zero, new(320, 240), new(500), out var small, out _) && small.X <= 320 && small.Y <= 240, "small viewport and oversized padding");
Console.WriteLine($"PASS: {checks} checks (assignment, persistence, cleanup, capacity, projection geometry).");
