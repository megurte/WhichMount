using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using WhichMount.ComponentInjector;
using WhichMount.Models;
using WhichMount.Utils;

namespace WhichMount.UI;

public unsafe class MountInfoTooltip : IInitializable, IPluginComponent
{
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly CashContainer _cashContainer;
    
    private Hook<AgentHUD.Delegates.UpdateTargetInfo> _updateTargetInfoHook;

    public MountInfoTooltip(IGameInteropProvider gameInteropProvider, CashContainer cashContainer)
    {
        _gameInteropProvider = gameInteropProvider;
        _cashContainer = cashContainer;
    }

    public void Initialize()
    {
        _updateTargetInfoHook = _gameInteropProvider.HookFromAddress<AgentHUD.Delegates.UpdateTargetInfo>(
            AgentHUD.MemberFunctionPointers.UpdateTargetInfo,
            TargetInfoHookHandle);
        _updateTargetInfoHook.Enable();
    }
    
    private void TargetInfoHookHandle(AgentHUD* thisPtr)
    {
        _updateTargetInfoHook.Original(thisPtr);
        UpdateMountIconStatus();
    }

    private void UpdateMountIconStatus()
    {
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null)
            return;

        var chara = TargetUtils.GetPlayerTarget();
        if (chara == null) return;

        if (chara->Mount.MountId != 0)
        {
            var sb = new SeStringBuilder();
            var mountName = _cashContainer.GetCachedData(chara->Mount.MountId, TargetData.Name);

            sb.Append($"{mountName}\n");

            if (chara->EntityId != localPlayer->EntityId)
            {
                var isUnlocked = PlayerState.Instance()->IsMountUnlocked(chara->Mount.MountId);
                sb.Append(isUnlocked ? "Unlocked" : "Locked");
            }

            TargetStatusUtils.AddPermanentStatus(0, 216201, 0, 0, default, sb.ToString());
        }
    }

    public void Release()
    {
        _updateTargetInfoHook.Disable();
    }
}

