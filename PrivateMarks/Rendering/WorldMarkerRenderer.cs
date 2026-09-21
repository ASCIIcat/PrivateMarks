using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using PrivateMarks.Core;
using PrivateMarks.Players;

namespace PrivateMarks.Rendering;

internal sealed class WorldMarkerRenderer(Configuration config, MarkStore marks, PlayerProvider players)
{
    public void Draw()
    {
        var viewport = ImGui.GetMainViewport();
        var draw = ImGui.GetBackgroundDrawList();
        var scale = ImGuiHelpers.GlobalScale;
        var size = config.MarkerSize * scale;
        var occupied = new List<(Vector2 Min, Vector2 Max)>();
        draw.PushClipRect(viewport.Pos, viewport.Pos + viewport.Size, true);
        try
        {
            foreach (var mark in marks.All)
            {
                if (!players.Loaded.TryGetValue(mark.Identity, out var player) || player.Position is not { } world) continue;
                var anchor = world + new Vector3(0, config.VerticalOffset, 0);
                var front = Services.GameGui.WorldToScreen(anchor, out var screen, out var inView);
                if (!EdgeGeometry.Finite(screen) || (!front && screen == Vector2.Zero)) continue;
                var distance = players.LocalPosition is { } local ? Vector3.Distance(local, world) : float.NaN;
                var label = config.ShowName ? mark.Identity.Name.Split(' ')[0] : "";
                if (config.ShowDistance && float.IsFinite(distance)) label += (label.Length > 0 ? " · " : "") + $"{distance:0}y";
                var textSize = ImGui.CalcTextSize(label);
                var boxSize = new Vector2(MathF.Max(size + 12 * scale, textSize.X + 14 * scale), size + (label.Length > 0 ? textSize.Y + 8 * scale : 4 * scale));
                var edge = !front || !inView;
                Vector2 direction = default;
                if (edge)
                {
                    if (!config.ShowOffscreen || !EdgeGeometry.TryPlace(screen, viewport.Pos, viewport.Size,
                        boxSize / 2 + new Vector2((config.EdgePadding + 16) * scale), out screen, out direction)) continue;
                }
                var intended = screen;
                // Eight marks maximum: simple deterministic vertical packing keeps labels readable.
                var min = screen - boxSize / 2;
                for (var attempt = 0; attempt < 16; attempt++)
                {
                    min = Vector2.Clamp(min, viewport.Pos, viewport.Pos + Vector2.Max(Vector2.Zero, viewport.Size - boxSize));
                    if (!occupied.Any(r => min.X < r.Max.X && min.X + boxSize.X > r.Min.X && min.Y < r.Max.Y && min.Y + boxSize.Y > r.Min.Y)) break;
                    min.Y = intended.Y - boxSize.Y / 2 + (attempt % 2 == 0 ? 1 : -1) * (attempt / 2 + 1) * (boxSize.Y + 5 * scale);
                }
                occupied.Add((min, min + boxSize));
                var center = min + boxSize / 2;
                var ink = ImGui.ColorConvertFloat4ToU32(MarkerArt.Color(mark.Kind, config.Opacity));
                if (!edge && Vector2.Distance(center, intended) > 8 * scale) draw.AddLine(intended, center, ink, 1.5f * scale);
                if (config.BackgroundOpacity > 0)
                    draw.AddRectFilled(min, min + boxSize, ImGui.ColorConvertFloat4ToU32(new(.025f, .03f, .05f, config.Opacity * config.BackgroundOpacity)), 6 * scale);
                MarkerArt.Draw(draw, new(center.X, min.Y + size / 2 + 2 * scale), size, mark.Kind, config.Opacity, config.BackgroundOpacity);
                if (label.Length > 0) draw.AddText(new(center.X - textSize.X / 2, min.Y + size + 4 * scale), ImGui.ColorConvertFloat4ToU32(new(1, 1, 1, config.Opacity)), label);
                if (edge) OffScreenIndicatorRenderer.Draw(draw, center, boxSize, direction, ink, scale);
            }
        }
        finally { draw.PopClipRect(); }
    }
}

internal static class OffScreenIndicatorRenderer
{
    public static void Draw(ImDrawListPtr draw, Vector2 center, Vector2 box, Vector2 direction, uint color, float scale)
    {
        var tip = center + direction * (MathF.Abs(direction.X) * box.X / 2 + MathF.Abs(direction.Y) * box.Y / 2 + 12 * scale);
        var side = new Vector2(-direction.Y, direction.X) * 5 * scale;
        draw.AddTriangleFilled(tip, tip - direction * 10 * scale + side, tip - direction * 10 * scale - side, color);
    }
}
