using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace WhichMount.Utils;

public unsafe class TargetUtils
{
    public static BattleChara* GetCurrentPlayerCharacterTarget()
    {
        var target = TargetSystem.Instance()->GetTargetObject();
        if (target == null || target->GetObjectKind() != ObjectKind.Pc)
            return null;
        
        return (BattleChara*)target;
    }
}
