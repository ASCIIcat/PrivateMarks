using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PrivateMarks.Core;
using PrivateMarks.Players;
using PrivateMarks.Rendering;

namespace PrivateMarks.UI;

internal sealed class HotbarWindow : Window
{
    private readonly Configuration config;
    private readonly MarkStore marks;
    private readonly PlayerProvider players;
    private readonly Action save;
    private string filter = "";
    private string feedback = "";
    public bool ShowSettings;

    public HotbarWindow(Configuration config, MarkStore marks, PlayerProvider players, Action save)
        : base("Private Marks###PrivateMarks")
    {
        this.config = config; this.marks = marks; this.players = players; this.save = save;
        Size = new(520, 390); SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new() { MinimumSize = new(400, 240), MaximumSize = new(900, 1000) };
    }

    public override void Draw()
    {
        ImGui.TextColored(new Vector4(.55f, .86f, .95f, 1), $"PRIVATE MARKS   {marks.All.Count}/{MarkStore.Capacity}");
        ImGui.SameLine();
        if (ImGui.SmallButton("Settings")) ShowSettings = !ShowSettings;
        ImGui.TextDisabled("Only on your screen. Click an active mark to clear it.");
        if (ImGui.Button("Clear All")) { marks.ClearAll(); save(); feedback = ""; }
        ImGui.SameLine();
        if (ImGui.Checkbox("Remember new marks", ref config.PersistNewMarks)) save();
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("New marks survive restarts when enabled. Temporary marks clear on territory change or logout.\nExisting marks keep their own Remember setting.");
        if (ShowSettings) DrawSettings();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##search", "Find player or world...", ref filter, 100);
        if (feedback.Length > 0) ImGui.TextColored(new Vector4(1, .75f, .35f, 1), feedback);
        ImGui.Separator();
        if (ImGui.BeginChild("roster", new Vector2(0, 0)))
        {
            Section("MARKED", marks.All.Select(m => players.Rows.FirstOrDefault(r => r.Identity == m.Identity)
                ?? new PlayerRow(m.Identity, $"World {m.Identity.HomeWorld}", "Saved", null)).ToArray(), true);
            Section("PARTY", players.Rows.Where(r => r.Group == "Party").ToArray(), false);
            if (ImGui.CollapsingHeader("NEARBY / TEST PLAYERS"))
            {
                ImGui.TextWrapped("Loaded players, including yourself for a solo rendering test. Alliance members appear here when loaded.");
                Section("", players.Rows.Where(r => r.Group == "Nearby").ToArray(), false);
            }
            if (players.Rows.Count == 0) ImGui.TextDisabled("Log in and enter a territory to find players.");
        }
        ImGui.EndChild();
    }

    private void Section(string title, PlayerRow[] rows, bool saved)
    {
        if (title.Length > 0) { ImGui.Spacing(); ImGui.TextDisabled(title); }
        if (rows.Length == 0 && title == "PARTY") ImGui.TextDisabled("No party. Use Nearby / Test Players below.");
        ImGui.PushID(title.Length == 0 ? "nearby" : title);
        if (ImGui.BeginTable("players", 2, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Player", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Mark", ImGuiTableColumnFlags.WidthFixed, 171 * ImGuiHelpers.GlobalScale);
            foreach (var row in rows)
            {
                if (!($"{row.Identity.Name} {row.WorldName}").Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                ImGui.PushID($"{row.Identity.Name}@{row.Identity.HomeWorld}");
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.TextUnformatted(row.Identity.Name);
                ImGui.TextDisabled($"{row.WorldName} · {(row.Position.HasValue ? "Located" : "Not located")}");
                ImGui.TableNextColumn();
                foreach (var kind in Enum.GetValues<MarkerKind>())
                {
                    if (kind != MarkerKind.Heart) ImGui.SameLine(0, 3 * ImGuiHelpers.GlobalScale);
                    var selected = marks.Get(row.Identity)?.Kind == kind;
                    ImGui.PushStyleColor(ImGuiCol.Button, selected ? MarkerArt.Color(kind, .42f) : new Vector4(.12f, .14f, .18f, 1));
                    var buttonSize = new Vector2(27, 27) * ImGuiHelpers.GlobalScale;
                    if (ImGui.Button($"##{kind}", buttonSize))
                    {
                        if (marks.Toggle(row.Identity, kind, config.PersistNewMarks)) { save(); feedback = ""; }
                        else feedback = "Eight marks assigned. Clear one to add another.";
                    }
                    ImGui.PopStyleColor();
                    MarkerArt.Draw(ImGui.GetWindowDrawList(), (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) / 2, 17 * ImGuiHelpers.GlobalScale, kind, 1);
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip(selected ? $"Clear {kind}" : $"Assign {kind}");
                }
                ImGui.SameLine(0, 3 * ImGuiHelpers.GlobalScale);
                if (ImGui.Button("X", new Vector2(24, 27) * ImGuiHelpers.GlobalScale)) { marks.Clear(row.Identity); save(); feedback = ""; }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Clear Mark");
                if (saved && marks.Get(row.Identity) is { } assignment)
                {
                    var remember = assignment.Persistent;
                    if (ImGui.Checkbox("Remember", ref remember)) { marks.SetPersistent(row.Identity, remember); save(); }
                }
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        ImGui.PopID();
    }

    private void DrawSettings()
    {
        ImGui.Separator();
        var changed = ImGui.SliderFloat("Marker size", ref config.MarkerSize, 16, 64, "%.0f");
        changed |= ImGui.SliderFloat("Height above actor", ref config.VerticalOffset, 0, 8, "%.1f");
        changed |= ImGui.SliderFloat("Edge padding", ref config.EdgePadding, 8, 160, "%.0f");
        changed |= ImGui.SliderFloat("Opacity", ref config.Opacity, .2f, 1, "%.2f");
        changed |= ImGui.SliderFloat("Background opacity", ref config.BackgroundOpacity, 0, 1, "%.2f");
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Set to 0 to remove the marker background and icon backing. Applies to world markers and edge indicators.");
        changed |= ImGui.Checkbox("Player name", ref config.ShowName);
        ImGui.SameLine(); changed |= ImGui.Checkbox("Distance", ref config.ShowDistance);
        changed |= ImGui.Checkbox("Off-screen indicators", ref config.ShowOffscreen);
        if (changed) save();
        ImGui.Separator();
    }
}
