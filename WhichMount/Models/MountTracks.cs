namespace WhichMount.Models;

public class MountTrackDefinition(string name, uint rewardId, uint[] memberIds)
{
    public string Name { get; } = name;
    public uint RewardId { get; } = rewardId;
    public uint[] MemberIds { get; } = memberIds;
}

public static class MountTracks
{
    public static readonly MountTrackDefinition[] All =
    [
        new("A Realm Reborn", rewardId: 47, [28, 29, 30, 31, 40, 43]), 
        new("Heavensward", rewardId: 105, [75, 76, 77, 78, 90, 98, 104]),
        new("Stormblood", rewardId: 181, [116, 115, 133, 144, 158, 172, 182]), 
        new("Shadowbringers", rewardId: 245, [189, 192, 205, 217, 226, 238, 249]),
        new("Endwalker", rewardId: 328, [261, 262, 293, 306, 315, 325, 332]),
        new("Dawntrail", rewardId: 420, [345, 346, 363, 389, 407, 422, 444])
    ];
}
