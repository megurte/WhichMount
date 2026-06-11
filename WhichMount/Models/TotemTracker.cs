using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Newtonsoft.Json;
using WhichMount.ComponentInjector;

namespace WhichMount.Models;

public readonly struct TotemCountInfo(int inventory, int saddlebag, int retainers)
{
    public int Inventory => inventory;
    public int Saddlebag => saddlebag;
    public int Retainers => retainers;
    public int Total => Inventory + Saddlebag + Retainers;
}

[InjectFields]
public unsafe class TotemTracker : IPluginComponent, IInitializable
{
    [Inject] private IDalamudPluginInterface _pluginInterface;
    [Inject] private IGameInventory _gameInventory;
    [Inject] private IPlayerState _playerState;
    [Inject] private IPluginLog _pluginLog;
    
    private class CharacterCache
    {
        public Dictionary<uint, int> Saddlebag { get; set; } = new();
        public Dictionary<ulong, Dictionary<uint, int>> Retainers { get; set; } = new();
    }

    private static readonly InventoryType[] SaddlebagContainers =
    [
        InventoryType.SaddleBag1, InventoryType.SaddleBag2,
        InventoryType.PremiumSaddleBag1, InventoryType.PremiumSaddleBag2
    ];

    private static readonly InventoryType[] RetainerContainers =
    [
        InventoryType.RetainerPage1, InventoryType.RetainerPage2, InventoryType.RetainerPage3,
        InventoryType.RetainerPage4, InventoryType.RetainerPage5, InventoryType.RetainerPage6,
        InventoryType.RetainerPage7
    ];

    private static readonly HashSet<uint> TrackedItems = MountTracks.All
        .SelectMany(track => track.Members)
        .Where(member => member.HasTotem)
        .Select(member => member.TotemItemId)
        .ToHashSet();

    private Dictionary<ulong, CharacterCache> _cacheByCharacter = new();

    private string CacheFilePath => Path.Combine(_pluginInterface.GetPluginConfigDirectory(), "TotemCache.json");

    public void Initialize()
    {
        LoadCache();
        _gameInventory.InventoryChanged += OnInventoryChanged;
    }

    public TotemCountInfo GetCount(uint itemId)
    {
        var inventory = InventoryManager.Instance()->GetInventoryItemCount(itemId);
        var cache = GetCharacterCache(create: false);
        var saddlebag = cache?.Saddlebag.GetValueOrDefault(itemId) ?? 0;
        var retainers = cache?.Retainers.Values.Sum(items => items.GetValueOrDefault(itemId)) ?? 0;
        return new TotemCountInfo(inventory, saddlebag, retainers);
    }

    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        var touchedSaddlebag = false;
        var touchedRetainer = false;

        foreach (var args in events)
        {
            var container = (InventoryType)(int)args.Item.ContainerType;
            touchedSaddlebag |= SaddlebagContainers.Contains(container);
            touchedRetainer |= RetainerContainers.Contains(container);
        }

        if (!touchedSaddlebag && !touchedRetainer)
            return;

        var cache = GetCharacterCache(create: true);
        if (cache == null)
            return;

        if (touchedSaddlebag && AnyContainerLoaded(SaddlebagContainers))
            cache.Saddlebag = ScanContainers(SaddlebagContainers);

        if (touchedRetainer && AnyContainerLoaded(RetainerContainers))
        {
            var retainerId = RetainerManager.Instance()->LastSelectedRetainerId;
            if (retainerId != 0)
                cache.Retainers[retainerId] = ScanContainers(RetainerContainers);
        }

        SaveCache();
    }

    private bool AnyContainerLoaded(InventoryType[] containers)
    {
        var manager = InventoryManager.Instance();
        foreach (var type in containers)
        {
            var container = manager->GetInventoryContainer(type);
            if (container != null && container->IsLoaded)
                return true;
        }
        return false;
    }

    private Dictionary<uint, int> ScanContainers(InventoryType[] containers)
    {
        var counts = new Dictionary<uint, int>();
        var manager = InventoryManager.Instance();

        foreach (var type in containers)
        {
            var container = manager->GetInventoryContainer(type);
            if (container == null || !container->IsLoaded)
                continue;

            for (var i = 0; i < container->Size; i++)
            {
                var item = container->GetInventorySlot(i);
                if (item == null || !TrackedItems.Contains(item->ItemId))
                    continue;

                counts[item->ItemId] = counts.GetValueOrDefault(item->ItemId) + item->Quantity;
            }
        }

        return counts;
    }

    private CharacterCache? GetCharacterCache(bool create)
    {
        var contentId = _playerState.IsLoaded ? _playerState.ContentId : 0;
        if (contentId == 0)
            return null;

        if (_cacheByCharacter.TryGetValue(contentId, out var cache))
            return cache;

        if (!create)
            return null;

        cache = new CharacterCache();
        _cacheByCharacter[contentId] = cache;
        return cache;
    }

    private void LoadCache()
    {
        try
        {
            if (File.Exists(CacheFilePath))
                _cacheByCharacter = JsonConvert.DeserializeObject<Dictionary<ulong, CharacterCache>>(File.ReadAllText(CacheFilePath)) 
                                    ?? new Dictionary<ulong, CharacterCache>();
        }
        catch (Exception e)
        {
            _pluginLog.Warning(e, "Failed to load totem cache, starting with an empty one");
            _cacheByCharacter = new Dictionary<ulong, CharacterCache>();
        }
    }

    private void SaveCache()
    {
        try
        {
            File.WriteAllText(CacheFilePath, JsonConvert.SerializeObject(_cacheByCharacter, Formatting.Indented));
        }
        catch (Exception e)
        {
            _pluginLog.Warning(e, "Failed to save totem cache");
        }
    }

    public void Release()
    {
        _gameInventory.InventoryChanged -= OnInventoryChanged;
    }
}
