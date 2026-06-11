using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using WhichMount.ComponentInjector;

namespace WhichMount.Models;

public class MountTrackContainer : IPluginComponent
{
    public IReadOnlyList<MountTrackModel> Tracks => _tracks;

    private readonly List<MountTrackModel> _tracks = [];

    public MountTrackContainer(CashContainer cashContainer, IDataManager dataManager)
    {
        var mountsById = cashContainer.MountModels.ToDictionary(mount => mount.Id);
        var itemSheet = dataManager.GetExcelSheet<Item>();

        foreach (var definition in MountTracks.All)
        {
            if (!mountsById.TryGetValue(definition.RewardId, out var reward))
                continue;
            
            var members = definition.Members
                                    .Where(member => mountsById.ContainsKey(member.MountId))
                                    .Select(member => new MountTrackMemberModel(mountsById[member.MountId], member.TotemItemId,
                                                member.HasTotem ? itemSheet.GetRow(member.TotemItemId).Icon : 0u))
                                    .ToList();
            _tracks.Add(new MountTrackModel(definition.Name, reward, members));
        }
    }

    public void Release()
    {
        _tracks.Clear();
    }
}
