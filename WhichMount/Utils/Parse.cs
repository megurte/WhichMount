using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HtmlAgilityPack;
using Lumina.Excel.GeneratedSheets;

namespace WhichMount.Utils;

public class Parse
{
    public async Task Main(IDataManager dataManager)
    {
                var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync("https://ffxiv.consolegameswiki.com/wiki/Mounts");

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(response);

        var mountTables = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'sortable')]");
        if (mountTables == null || mountTables.Count <= 7)
        {
            Console.WriteLine("Mount table not found.");
            return;
        }

        var mountTable = mountTables[7];
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
                    var name = nameNode.InnerText.Trim();
                    var mount = dataManager.GetExcelSheet<Mount>()?.FirstOrDefault(m => 
                        string.Equals(m.Singular.ToDalamudString().ToString(), name, StringComparison.OrdinalIgnoreCase));
                    var mountId = mount?.RowId.ToString() ?? "Unknown ID";

                    var rowData = new string[cells.Count + 1];
                    rowData[0] = cells[0].InnerText.Trim();
                    rowData[1] = mountId;
                    for (int i = 1; i < cells.Count; i++)
                    {
                        rowData[i + 1] = cells[i].InnerText.Trim();
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

        Console.WriteLine($"Data saved to {csvFile}");
    }
}
