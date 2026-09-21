using Dalamud.Configuration;
using PrivateMarks.Core;

namespace PrivateMarks;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public float MarkerSize = 28;
    public float VerticalOffset = 2.5f;
    public bool ShowName = true;
    public bool ShowDistance = true;
    public bool ShowOffscreen = true;
    public float EdgePadding = 30;
    public float Opacity = 0.95f;
    public float BackgroundOpacity = 0.8f;
    public bool PersistNewMarks = true;
    public List<Mark> SavedMarks = new();
    public void Sanitize()
    {
        MarkerSize = Clamp(MarkerSize, 16, 64, 28);
        VerticalOffset = Clamp(VerticalOffset, 0, 8, 2.5f);
        EdgePadding = Clamp(EdgePadding, 8, 160, 30);
        Opacity = Clamp(Opacity, 0.2f, 1, 0.95f);
        BackgroundOpacity = Clamp(BackgroundOpacity, 0, 1, 0.8f);
        SavedMarks ??= new();
    }
    private static float Clamp(float value, float min, float max, float fallback) => float.IsFinite(value) ? Math.Clamp(value, min, max) : fallback;
}
