using System;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
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
public unsafe class TotemTracker
{
    [Inject] private IDalamudPluginInterface _pluginInterface;

    public TotemCountInfo GetCount(uint itemId)
    {
        var inventory = InventoryManager.Instance()->GetInventoryItemCount(itemId);
        var finder = ItemFinderModule.Instance();
        
        if (finder == null)
            return new TotemCountInfo(inventory, 0, 0);

        var saddlebag = CountItemsInCollection(finder->SaddleBagItemIds, finder->SaddleBagItemCount, itemId)
                        + CountItemsInCollection(finder->PremiumSaddleBagItemIds, finder->PremiumSaddleBagItemCount, itemId);

        var retainers = 0;
        foreach (var pair in finder->RetainerInventories)
        {
            var retainerInventory = pair.Item2.Value;
            if (retainerInventory != null)
                retainers += CountItemsInCollection(retainerInventory->ItemIds, retainerInventory->ItemCount, itemId);
        }

        return new TotemCountInfo(inventory, saddlebag, retainers);
    }

    private int CountItemsInCollection(Span<uint> itemIds, Span<ushort> itemCounts, uint itemId)
    {
        var total = 0;
        for (var i = 0; i < itemIds.Length; i++)
        {
            if (itemIds[i] == itemId)
                total += itemCounts[i];
        }
        return total;
    }
}
