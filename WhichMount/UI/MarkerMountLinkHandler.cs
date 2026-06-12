using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Chat;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using WhichMount.ComponentInjector;
using WhichMount.Models;
using WhichMount.Utils;

namespace WhichMount.UI;

[InjectFields]
public unsafe class MarkerMountLinkHandler : IPluginComponent, IInitializable
{
    [Inject] private IChatGui _chatGui;
    [Inject] private CacheContainer _cacheContainer;
    
    public Action<uint>? OpenMountRequested { get; set; }
    
    private const ushort LinkColorKey = 500;
    
    private static readonly Regex MarkerPattern = new(@"\[Mount: (.+?) #(\d+)\]", RegexOptions.Compiled);
    private readonly Dictionary<uint, DalamudLinkPayload> _linkPayloads = new();

    public void Initialize()
    {
        _chatGui.ChatMessage += OnChatMessage;
    }

    public void ShareToChat(MountModel mount)
    {
        var text = $"[Mount: {mount.Name} #{mount.Id}]";
        var utf8 = Utf8String.FromString(text);
        UIModule.Instance()->ProcessChatBoxEntry(utf8);
        utf8->Dtor(true);
    }

    public SeString BuildLocalLink(MountModel mount)
    {
        var payloads = new List<Payload>();
        AddLinkPayloads(payloads, mount);
        return new SeString(payloads);
    }

    private DalamudLinkPayload GetOrCreateLink(uint mountId)
    {
        if (_linkPayloads.TryGetValue(mountId, out var payload))
            return payload;

        payload = _chatGui.AddChatLinkHandler(mountId, (id, _) => OpenMountRequested?.Invoke(id));
        _linkPayloads[mountId] = payload;
        return payload;
    }

    private void AddLinkPayloads(List<Payload> payloads, MountModel mount)
    {
        payloads.Add(GetOrCreateLink(mount.Id));
        payloads.Add(new UIForegroundPayload(LinkColorKey));
        payloads.Add(new TextPayload(mount.Name.FormatAsMountLink()));
        payloads.Add(UIForegroundPayload.UIForegroundOff);
        payloads.Add(RawPayload.LinkTerminator);
    }

    private MountModel? FindMount(uint mountId)
    {
        return _cacheContainer.MountModels.FirstOrDefault(m => m.Id == mountId);
    }

    private void OnChatMessage(IHandleableChatMessage message)
    {
        var original = message.Message;
        if (!original.TextValue.Contains("[Mount:"))
            return;

        var payloads = new List<Payload>();
        var changed = false;

        foreach (var payload in original.Payloads)
        {
            if (payload is TextPayload { Text: { } text } && text.Contains("[Mount:"))
                changed |= AppendLinkified(payloads, text);
            else
                payloads.Add(payload);
        }

        if (changed)
            message.Message = new SeString(payloads);
    }

    private bool AppendLinkified(List<Payload> payloads, string text)
    {
        var changed = false;
        var last = 0;

        foreach (Match match in MarkerPattern.Matches(text))
        {
            if (!uint.TryParse(match.Groups[2].Value, out var mountId))
                continue;

            var mount = FindMount(mountId);
            if (mount == null)
                continue;

            if (match.Index > last)
                payloads.Add(new TextPayload(text[last..match.Index]));

            AddLinkPayloads(payloads, mount);

            last = match.Index + match.Length;
            changed = true;
        }

        if (changed && last < text.Length)
            payloads.Add(new TextPayload(text[last..]));

        return changed;
    }

    public void Release()
    {
        _chatGui.ChatMessage -= OnChatMessage;
        _chatGui.RemoveChatLinkHandler();
        _linkPayloads.Clear();
    }
}
