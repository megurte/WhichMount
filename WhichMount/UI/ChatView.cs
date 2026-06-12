using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using WhichMount.Models;

namespace WhichMount.UI;

public class ChatView : IViewBinder
{
    private readonly IChatGui _chatGui;
    private readonly Configuration _configuration;
    private readonly MountChatLinks _chatLinks;
    private readonly CacheContainer _cacheContainer;

    public ChatView(IChatGui chatGui, Configuration configuration, MountChatLinks chatLinks, CacheContainer cacheContainer)
    {
        _chatGui = chatGui;
        _configuration = configuration;
        _chatLinks = chatLinks;
        _cacheContainer = cacheContainer;
    }
    
    public void BindModel(MountModel model)
    {
        _chatGui.Print(BuildHeader(model));
        _chatGui.Print($"Acquired by: {GetData(model, TargetData.AcquiredBy)}");

        if (_configuration.ShowMountId)
            _chatGui.Print($"Mount ID: {model.Id}");
        if (_configuration.ShowSeats)
            _chatGui.Print($"Number of seats: {model.NumberSeats}");
        if (_configuration.ShowHasActions)
            _chatGui.Print($"Has actions: {(model.HasActions ? "Yes" : "No")}");
        if (_configuration.ShowHasUniqueMusic)
            _chatGui.Print($"Has unique music: {(model.HasUniqueMusic ? "Yes" : "No")}");
        if (_configuration.ShowMBAvailable)
            _chatGui.Print($"Is available on Market board: {(model.IsMarketBoardAvailable ? "Yes" : "No")}");
        if (_configuration.ShowAvailability)
            _chatGui.Print($"Is currently obtainable: {(GetData(model, TargetData.IsObtainable) == "1" ? "Yes" : "No")}");
        if (_configuration.AddedInPatch)
            _chatGui.Print($"Added in patch {GetData(model, TargetData.Patch)}");
    }

    private SeString BuildHeader(MountModel model)
    {
        var builder = new SeStringBuilder();
        builder.AddText($"{model.Owner}'s mount: ");
        foreach (var payload in _chatLinks.BuildChatLink(model).Payloads)
            builder.Add(payload);
        return builder.Build();
    }

    private string GetData(MountModel model, TargetData targetData)
    {
        return _cacheContainer.GetCachedData(model.Id, targetData);
    }
}
