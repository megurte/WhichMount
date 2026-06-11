using System.Collections.Generic;
using System.Linq;

namespace WhichMount.Models;

public class MountTrackModel(string name, MountModel reward, IReadOnlyList<MountModel> members)
{
    public string Name { get; } = name;
    public MountModel Reward { get; } = reward;
    public IReadOnlyList<MountModel> Members { get; } = members;

    public int CollectedCount => Members.Count(mount => mount.IsMountUnlocked);
    public bool IsRewardUnlocked => Reward.IsMountUnlocked;
}
