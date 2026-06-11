using System.Collections.Generic;
using System.Linq;

namespace WhichMount.Models;

public class MountTrackMemberModel(MountModel mount, uint totemItemId, uint totemIconId)
{
    public MountModel Mount => mount;
    public uint TotemItemId => totemItemId;
    public uint TotemIconId => totemIconId;
    public bool HasTotem => TotemItemId != 0;
}

public class MountTrackModel(string name, MountModel reward, IReadOnlyList<MountTrackMemberModel> members)
{
    public string Name => name;
    public MountModel Reward => reward;
    public IReadOnlyList<MountTrackMemberModel> Members => members;
    public int CollectedCount => Members.Count(member => member.Mount.IsMountUnlocked);
    public bool IsRewardUnlocked => Reward.IsMountUnlocked;
    public bool HasTotems => Members.Any(member => member.HasTotem);
}
