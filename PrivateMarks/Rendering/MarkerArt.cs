using System.Numerics;
using Dalamud.Bindings.ImGui;
using PrivateMarks.Core;

namespace PrivateMarks.Rendering;

internal static class MarkerArt
{
    public static Vector4 Color(MarkerKind kind, float alpha = 1) => kind switch
    {
        MarkerKind.Heart => new(1, .38f, .56f, alpha),
        MarkerKind.Star => new(1, .81f, .3f, alpha),
        MarkerKind.Diamond => new(.32f, .85f, 1, alpha),
        _ => new(.55f, .95f, .64f, alpha),
    };
    // Geometry instead of font glyphs: identical icons in the hotbar and overlay.
    public static void Draw(ImDrawListPtr draw, Vector2 center, float size, MarkerKind kind, float alpha, float backgroundOpacity = .9f)
    {
        var color = ImGui.ColorConvertFloat4ToU32(Color(kind, alpha));
        var radius = size * .46f;
        if (backgroundOpacity > 0)
            draw.AddCircleFilled(center, radius + 3, ImGui.ColorConvertFloat4ToU32(new(0.025f, .03f, .05f, alpha * backgroundOpacity)), 32);
        if (kind == MarkerKind.Circle) { draw.AddCircleFilled(center, radius * .75f, color, 32); return; }
        if (kind == MarkerKind.Diamond)
        {
            draw.AddQuadFilled(center + new Vector2(0, -radius), center + new Vector2(radius, 0),
                center + new Vector2(0, radius), center + new Vector2(-radius, 0), color); return;
        }
        if (kind == MarkerKind.Heart)
        {
            draw.AddCircleFilled(center + new Vector2(-radius * .38f, -radius * .28f), radius * .52f, color, 24);
            draw.AddCircleFilled(center + new Vector2(radius * .38f, -radius * .28f), radius * .52f, color, 24);
            draw.AddTriangleFilled(center + new Vector2(-radius * .86f, -radius * .05f), center + new Vector2(radius * .86f, -radius * .05f), center + new Vector2(0, radius), color);
            return;
        }
        for (var i = 0; i < 10; i++)
        {
            Vector2 Point(int n) { var angle = -MathF.PI / 2 + n * MathF.PI / 5; return center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius * (n % 2 == 0 ? 1 : .44f); }
            draw.AddTriangleFilled(center, Point(i), Point(i + 1), color);
        }
    }
}
