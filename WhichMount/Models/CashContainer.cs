using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using WhichMount.ComponentInjector;

namespace WhichMount.Models;

public class CashContainer : IPluginComponent
{
    public List<MountModel> MountModels => _mountModelList;
    
    private HashSet<uint> _bgmMountCash;
    private Dictionary<uint, Dictionary<TargetData, string>> _tableData = new();
    private ExcelSheet<Mount>? _excelSheet;
    private readonly IDataManager _dataManager;
    private readonly List<MountModel> _mountModelList = new();
    
    public CashContainer(IDataManager dataManager)
    {
        _dataManager = dataManager;
        InitCashedData();
    }

    private void InitCashedData()
    {
        _bgmMountCash = _dataManager.Excel
                                    .GetSheet<Mount>()
                                    .GroupBy(mount => mount.RideBGM.RowId)
                                    .Where(group => group.Count() == 1)
                                    .Select(group => group.First().RowId)
                                    .ToHashSet();
        
        _excelSheet = _dataManager.GetExcelSheet<Mount>();
        
        foreach (var mount in _excelSheet)
        {
            var model = new MountModel(_dataManager, this, mount.RowId, "N/A");
            if (model.TryInitData())
            {
                _mountModelList.Add(model);
                CacheTableData(model.Id);
            }
        }
    }
    
    public void CacheTableData(uint mountId)
    {
        if (_tableData.ContainsKey(mountId))
            return;

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("WhichMount.Resources.MountList.csv");

        if (stream is not { CanRead: true }) return;
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            var columns = line.Split('|');

            if (columns.Length <= (int)TargetData.Id) continue;
            if (!uint.TryParse(columns[(int)TargetData.Id], out var id)) continue;
            if (id != mountId) continue;

            var dict = new Dictionary<TargetData, string>();
            foreach (TargetData type in Enum.GetValues(typeof(TargetData)))
            {
                if ((int)type < columns.Length)
                    dict[type] = columns[(int)type];
            }

            _tableData[mountId] = dict;
            break;
        }
    }

    public string GetCachedData(uint mountId, TargetData targetData)
        => _tableData.TryGetValue(mountId, out var data) && data.TryGetValue(targetData, out var value)
               ? value
               : "Unknown";

    public bool HasUniqueMusic(uint mountId)
    {
        return _bgmMountCash.Contains(mountId);
    }

    public void Release()
    {
        _bgmMountCash.Clear();
        _tableData.Clear();
    }
}
