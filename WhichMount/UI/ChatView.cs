using Dalamud.Plugin.Services;
using WhichMount.Models;

namespace WhichMount.UI;

public class ChatView : IViewBinder
{
    private readonly IChatGui _chatGui;
    private readonly Configuration _configuration;

    public ChatView(IChatGui chatGui, Configuration configuration)
    {
        _chatGui = chatGui;
        _configuration = configuration;
    }
    
    public void BindModel(MountModel model)
    {
        _chatGui.Print("===================");
        _chatGui.Print($"{model.Owner}'s mount: {model.Name}");
        _chatGui.Print($"Acquired by: {model.GetDataByTable(TargetData.AcquiredBy)}");
        
        if (_configuration.ShowMountId)
            _chatGui.Print($"Mount ID: {model.Id}");
        if (_configuration.ShowSeats) 
            _chatGui.Print($"Number of seats: {model.NumberSeats}");
        if (_configuration.ShowHasActions)
            _chatGui.Print($"Has actions: {(model.HasActions ? "Yes" : "No")}");
        if (_configuration.ShowHasUniqueMusic)
            _chatGui.Print($"Has unique music: {(model.HasUniqueMusic ? "Yes" : "No")}");
        if (_configuration.ShowAvailability)
            _chatGui.Print($"Is currently obtainable: {(model.GetDataByTable(TargetData.IsObtainable) == "1" ? "Yes" : "No")}");
        if (_configuration.AddedInPatch)
            _chatGui.Print($"Added in patch {model.GetDataByTable(TargetData.Patch)}");
        _chatGui.Print("===================");
    }
}
