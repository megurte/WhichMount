using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HtmlAgilityPack;
using Lumina.Excel.Sheets;
using WhichMount.ComponentInjector;

namespace WhichMount.Utils;

public class Parse : IPluginComponent, IInitializable
{
    private readonly IDataManager _dataManager;
    private readonly IChatGui _chatGui;

    public Parse(IDataManager dataManager, IChatGui chatGui)
    {
        _dataManager = dataManager;
        _chatGui = chatGui;
        _chatGui.Print($"Created");
    }
    
    public async void Initialize()
    {
        Main();
    }
    
    public async Task Main()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync("https://ffxiv.consolegameswiki.com/wiki/Mounts");

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(response);

        var mountTables = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'sortable')]");
        if (mountTables == null)
        {
            Console.WriteLine("Mount table not found.");
            return;
        }

        var mountTable = mountTables[9];
        var rows = mountTable.SelectNodes(".//tr").Skip(1); // Skip header row

        var mountsData = new List<string[]>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells != null && cells.Count >= 1)
            {
                var nameNode = cells[0].SelectSingleNode(".//a");
                if (nameNode != null)
                {
                    var name = HtmlEntity.DeEntitize(nameNode.InnerText.Trim()).Replace('\u00A0', ' ');
                    var mount = _dataManager.GetExcelSheet<Mount>()?.FirstOrDefault(m => 
                        string.Equals(m.Singular.ToDalamudString().ToString(), name, StringComparison.OrdinalIgnoreCase));
                    var mountId = mount?.RowId.ToString() ?? "Unknown ID";

                    var rowData = new string[cells.Count + 1];
                    rowData[0] = name.Replace("|", "").Trim();
                    rowData[1] = mountId;                     
                    for (int i = 1; i < cells.Count; i++)
                    {
                        var cellText = HtmlEntity.DeEntitize(cells[i].InnerText.Trim())
                                                 .Replace('\u00A0', ' ')
                                                 .Replace("|", "");
                        rowData[i + 1] = string.IsNullOrEmpty(cellText) ? "" : cellText; 
                    }
                    mountsData.Add(rowData);
                }
            }
        }

        // Save to CSV
        var csvFile = "mounts_with_ids.csv";
        using (var writer = new StreamWriter(csvFile))
        {
            foreach (var data in mountsData)
            {
                writer.WriteLine(string.Join("|", data));
            }
        }

        _chatGui.Print($"Data saved to {csvFile}");
    }

    public void Release() { }
}
