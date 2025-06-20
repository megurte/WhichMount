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
    private readonly Configuration _configuration;
    private readonly IDataManager _dataManager;

    private Hook<AgentHUD.Delegates.UpdateTargetInfo> _updateTargetInfoHook;

    public MountInfoTooltip(IGameInteropProvider gameInteropProvider, CashContainer cashContainer, Configuration configuration, IDataManager dataManager)
    {
        _gameInteropProvider = gameInteropProvider;
        _cashContainer = cashContainer;
        _configuration = configuration;
        _dataManager = dataManager;
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
        if (!_configuration.ShowTooltip) return;
            
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null)
            return;

        var chara = TargetUtils.GetCurrentPlayerCharacterTarget();
        if (chara == null) return;
        
        var mountId = chara->Mount.MountId;
        if (mountId != 0)
        { 
            var sb = new Lumina.Text.SeStringBuilder();
            var mountModel = new MountModel(_dataManager, _cashContainer, mountId, "N/A");
            mountModel.TryInitData();
            sb.Append($"{mountModel.Name}");

            if (chara->EntityId != localPlayer->EntityId)
            {
                if (_configuration.ShowUnlockedTooltip)
                {
                    sb.AppendNewLine();
                    var isUnlocked = PlayerState.Instance()->IsMountUnlocked(mountId);
                    sb.PushColorType(isUnlocked ? 43u : 518);
                    sb.Append(isUnlocked ? "Unlocked" : "Locked");
                    sb.PopColorType();
                }

                if (_configuration.ShowObtainableTooltip)
                {
                    sb.AppendNewLine();
                    var isObtainable = _cashContainer.GetCachedData(mountId, TargetData.IsObtainable);
                    sb.Append(isObtainable == "1" ? "Obtainable" : "Unobtainable");
                }
            }

            StatusUtils.AddPermanentStatus(0, 216201, 0, 0, default, sb.ToSeString());
        }
    }

    public void Release()
    {
        _updateTargetInfoHook.Disable();
        _updateTargetInfoHook.Dispose();
    }
}

