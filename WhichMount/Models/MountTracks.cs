using WhichMount.Utils;

namespace WhichMount.Models;

public class MountTrackMember(uint mountId, uint totemItemId = 0)
{
    public uint MountId { get; } = mountId;
    public uint TotemItemId { get; } = totemItemId;
    public bool HasTotem => TotemItemId != 0;
}

public class MountTrackDefinition(string name, uint rewardId, MountTrackMember[] members)
{
    public string Name { get; } = name;
    public uint RewardId { get; } = rewardId;
    public MountTrackMember[] Members { get; } = members;
}

public static class MountTracks
{
    public const int TotemCost = 99;

    public static readonly MountTrackDefinition[] All =
    [
        new(Expansions.ARealmReborn, rewardId: 47, 
        [
            new MountTrackMember(28),
            new MountTrackMember(29),
            new MountTrackMember(30), 
            new MountTrackMember(31), 
            new MountTrackMember(40), 
            new MountTrackMember(43)
        ]),
        new(Expansions.Heavensward, rewardId: 105, 
        [
            new MountTrackMember(75, 12672),  
            new MountTrackMember(76, 12673),  
            new MountTrackMember(77, 13627),  
            new MountTrackMember(78, 14300),  
            new MountTrackMember(90, 15421),  
            new MountTrackMember(98, 16188),  
            new MountTrackMember(104, 17461)  
        ]),
        new(Expansions.Stormblood, rewardId: 181, 
        [
            new MountTrackMember(116, 19109), 
            new MountTrackMember(115, 19110), 
            new MountTrackMember(133, 21196), 
            new MountTrackMember(144, 21773), 
            new MountTrackMember(158, 23043), 
            new MountTrackMember(172, 23962), 
            new MountTrackMember(182, 24797)  
        ]),
        new(Expansions.Shadowbringers, rewardId: 245,
        [
            new MountTrackMember(189, 27391), 
            new MountTrackMember(192, 27392), 
            new MountTrackMember(205, 28708), 
            new MountTrackMember(217, 29001), 
            new MountTrackMember(226, 30596), 
            new MountTrackMember(238, 32132), 
            new MountTrackMember(249, 33480)  
        ]),
        new(Expansions.Endwalker, rewardId: 328,
        [
            new MountTrackMember(261, 35815), 
            new MountTrackMember(262, 35816), 
            new MountTrackMember(293, 36809), 
            new MountTrackMember(306, 38374), 
            new MountTrackMember(315, 38949), 
            new MountTrackMember(325, 40295), 
            new MountTrackMember(332, 41053)  
        ]),
        new(Expansions.Dawntrail, rewardId: 420,
        [
            new MountTrackMember(345, 43539), 
            new MountTrackMember(346, 43540), 
            new MountTrackMember(363, 44718), 
            new MountTrackMember(389, 46720), 
            new MountTrackMember(407, 46982), 
            new MountTrackMember(422, 49748), 
            new MountTrackMember(444, 50892)  
        ])
    ];
}
