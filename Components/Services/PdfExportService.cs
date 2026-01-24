using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Journal_Entry.Components.Models;

namespace Journal_Entry.Components.Services
{
    public class PdfExportService
    {
        public async Task<string> ExportToPdfAsync(JournalEntry entry)
        {
            var fileName = $"Journal_{entry.CreatedAt:yyyyMMdd}.txt";
            var path = Path.Combine(FileSystem.AppDataDirectory, fileName);

            var content =
$"""
{entry.Title}
{entry.Content}

Created: {entry.CreatedAt:yyyy-MM-dd}
Primary Mood: {entry.PrimaryMood}
Secondary Moods: {string.Join(", ", entry.SecondaryMoods)}
Tags: {string.Join(", ", entry.Tags)}
""";

            await File.WriteAllTextAsync(path, content);
            return path;
        }
    }
}
