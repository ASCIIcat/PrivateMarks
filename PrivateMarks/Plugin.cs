using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using PrivateMarks.Core;
using PrivateMarks.Players;
using PrivateMarks.Rendering;
using PrivateMarks.UI;

namespace PrivateMarks;

public sealed class Plugin : IDalamudPlugin
{
    private readonly Configuration config;
    private readonly MarkStore marks = new();
    private readonly PlayerProvider players = new();
    private readonly WindowSystem windows = new("PrivateMarks");
    private readonly HotbarWindow hotbar;
    private readonly WorldMarkerRenderer renderer;
    private bool wasLoggedIn;
    private uint territory;
    private DateTime lastError;
    private int pendingReset;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();
        config = pluginInterface.GetPluginConfig() as Configuration ?? new();
        config.Sanitize();
        marks.Restore(config.SavedMarks);
        hotbar = new(config, marks, players, Save);
        renderer = new(config, marks, players);
        windows.AddWindow(hotbar);
        Services.Commands.AddHandler("/pmark", new CommandInfo(Command)
        { HelpMessage = "Toggle Private Marks. /pmark clearall clears all marks; /pmark settings opens settings." });
        pluginInterface.UiBuilder.Draw += Draw;
        pluginInterface.UiBuilder.OpenMainUi += Open;
        pluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
        Services.Client.Logout += OnLogout;
        Services.Client.TerritoryChanged += OnTerritoryChanged;
    }

    private void Save()
    {
        config.SavedMarks = marks.All.Where(m => m.Persistent).ToList();
        Services.PluginInterface.SavePluginConfig(config);
    }
    private void Open() => hotbar.IsOpen = true;
    private void OpenSettings() { hotbar.ShowSettings = true; Open(); }
    // Queue lifecycle resets even while Dalamud suppresses Draw. Consume them on
    // the draw thread so the UI and event handlers never mutate the store concurrently.
    private void OnLogout(int type, int code) => Interlocked.Exchange(ref pendingReset, 1);
    private void OnTerritoryChanged(uint value) => Interlocked.Exchange(ref pendingReset, 1);
    private void Command(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "clearall": marks.ClearAll(); Save(); break;
            case "settings": OpenSettings(); break;
            default: hotbar.Toggle(); break;
        }
    }
    private void Draw()
    {
        try
        {
            if (Interlocked.Exchange(ref pendingReset, 0) != 0) { marks.ClearTemporary(); players.Clear(); }
            var loggedIn = Services.Client.IsLoggedIn;
            var currentTerritory = Services.Client.TerritoryType;
            if ((wasLoggedIn && !loggedIn) || (territory != 0 && territory != currentTerritory)) marks.ClearTemporary();
            wasLoggedIn = loggedIn; territory = currentTerritory;
            var changing = Services.Condition[ConditionFlag.BetweenAreas] || Services.Condition[ConditionFlag.BetweenAreas51];
            if (!loggedIn || changing) players.Clear(); else players.Refresh();
            // Input runs first so a click is reflected in the very same draw frame.
            windows.Draw();
            if (loggedIn && !changing && !Services.GameGui.GameUiHidden) renderer.Draw();
        }
        catch (Exception ex)
        {
            players.Clear();
            if (DateTime.UtcNow - lastError > TimeSpan.FromSeconds(10))
            { Services.Log.Error(ex, "Private Marks could not draw this frame."); lastError = DateTime.UtcNow; }
        }
    }
    public void Dispose()
    {
        Services.PluginInterface.UiBuilder.Draw -= Draw;
        Services.PluginInterface.UiBuilder.OpenMainUi -= Open;
        Services.PluginInterface.UiBuilder.OpenConfigUi -= OpenSettings;
        Services.Commands.RemoveHandler("/pmark");
        Services.Client.Logout -= OnLogout;
        Services.Client.TerritoryChanged -= OnTerritoryChanged;
        windows.RemoveAllWindows(); players.Clear();
    }
}
