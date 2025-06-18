using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using WhichMount.Models;
using WhichMount.Utils;

namespace WhichMount.UI;

public unsafe class MountInfoTooltip : IDisposable, IInitializable
{
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly IChatGui _chatGui;
    private readonly CashContainer _cashContainer;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ITextureProvider _textureProvider;
    private readonly IClientState _clientState;
    
    private Hook<AgentHUD.Delegates.UpdateTargetInfo> _updateTargetInfoHook;

    public MountInfoTooltip(
        IDalamudPluginInterface pluginInterface,
        ITextureProvider textureProvider,
        IClientState clientState,
        IGameInteropProvider gameInteropProvider,
        IChatGui chatGui,
        CashContainer cashContainer)
    {
        _pluginInterface = pluginInterface;
        _textureProvider = textureProvider;
        _clientState = clientState;
        _gameInteropProvider = gameInteropProvider;
        _chatGui = chatGui;
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
                sb.Append(isUnlocked ? "Unlocked" : "");
            }

            TargetStatusUtils.AddPermanentStatus(0, 216201, 0, 0, default, sb.ToString());
        }
    }

    public void Dispose()
    {
        _updateTargetInfoHook.Disable();
    }
}

