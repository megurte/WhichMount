using System.Collections.Generic;
using System.Linq;
using WhichMount.ComponentInjector;

namespace WhichMount.Models;

public class MountTrackContainer : IPluginComponent
{
    public IReadOnlyList<MountTrackModel> Tracks => _tracks;

    private readonly List<MountTrackModel> _tracks = [];

    public MountTrackContainer(CashContainer cashContainer)
    {
        var mountsById = cashContainer.MountModels.ToDictionary(mount => mount.Id);

        foreach (var definition in MountTracks.All)
        {
            if (!mountsById.TryGetValue(definition.RewardId, out var reward))
                continue;

            var members = definition.MemberIds
                                    .Where(mountsById.ContainsKey)
                                    .Select(id => mountsById[id])
                                    .ToList();
            _tracks.Add(new MountTrackModel(definition.Name, reward, members));
        }
    }

    public void Release()
    {
        _tracks.Clear();
    }
}
