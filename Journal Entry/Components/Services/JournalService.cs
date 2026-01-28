using Journal_Entry.Components.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;

namespace Journal_Entry.Components.Services
{
    // Helper class for Dapper to map database results
    internal class JournalEntryDb
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PrimaryMood { get; set; } = "Neutral";
        public string SecondaryMoods { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }

    public class JournalService
    {
        private readonly string dbPath;

        public JournalService()
        {
            dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            InitializeDatabase();
        }

        private IDbConnection Connection => new SqliteConnection($"Data Source={dbPath}");

        private void InitializeDatabase()
        {
            using var db = Connection;
            db.Execute(@"
                CREATE TABLE IF NOT EXISTS JournalEntry (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT,
                    Content TEXT,
                    PrimaryMood TEXT,
                    SecondaryMoods TEXT,
                    Tags TEXT,
                    CreatedAt TEXT
                );

                CREATE TABLE IF NOT EXISTS UserSettings (
                    Id INTEGER PRIMARY KEY,
                    Theme TEXT,
                    IsPasswordEnabled INTEGER,
                    PasswordHash TEXT
                );
            ");
        }

        // ===============================
        // JOURNAL ENTRIES
        // ===============================

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            using var db = Connection;
            var dbEntries = (await db.QueryAsync<JournalEntryDb>("SELECT * FROM JournalEntry")).ToList();
            var entries = dbEntries.Select(ConvertFromDb).ToList();
            return entries.OrderByDescending(e => e.CreatedAt).ToList();
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            using var db = Connection;
            var dbEntry = await db.QueryFirstOrDefaultAsync<JournalEntryDb>(
                "SELECT * FROM JournalEntry WHERE date(CreatedAt)=date(@date)",
                new { date = date.ToString("yyyy-MM-dd") });

            return dbEntry != null ? ConvertFromDb(dbEntry) : null;
        }

        public async Task SaveEntryAsync(JournalEntry entry)
        {
            using var db = Connection;

            var secondaryMoods = entry.SecondaryMoods != null && entry.SecondaryMoods.Any()
                ? string.Join(",", entry.SecondaryMoods)
                : string.Empty;
            var tags = entry.Tags != null && entry.Tags.Any()
                ? string.Join(",", entry.Tags)
                : string.Empty;

            if (entry.Id == 0)
            {
                await db.ExecuteAsync(@"INSERT INTO JournalEntry 
                    (Title, Content, PrimaryMood, SecondaryMoods, Tags, CreatedAt)
                    VALUES (@Title,@Content,@PrimaryMood,@SecondaryMoods,@Tags,@CreatedAt)",
                    new
                    {
                        entry.Title,
                        entry.Content,
                        entry.PrimaryMood,
                        SecondaryMoods = secondaryMoods,
                        Tags = tags,
                        entry.CreatedAt
                    });
            }
            else
            {
                await db.ExecuteAsync(@"UPDATE JournalEntry SET
                    Title=@Title, Content=@Content, PrimaryMood=@PrimaryMood,
                    SecondaryMoods=@SecondaryMoods, Tags=@Tags
                    WHERE Id=@Id",
                    new
                    {
                        entry.Title,
                        entry.Content,
                        entry.PrimaryMood,
                        SecondaryMoods = secondaryMoods,
                        Tags = tags,
                        entry.Id
                    });
            }
        }

        public async Task DeleteEntryAsync(int id)
        {
            using var db = Connection;
            await db.ExecuteAsync("DELETE FROM JournalEntry WHERE Id=@id", new { id });
        }

        // Filter entries by title/content/tags
        public async Task<List<JournalEntry>> FilterAsync(string searchTerm)
        {
            var all = await GetAllEntriesAsync();
            if (string.IsNullOrWhiteSpace(searchTerm)) return all;

            searchTerm = searchTerm.ToLower();
            return all.Where(e =>
                    (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(e.Content) && e.Content.ToLower().Contains(searchTerm)) ||
                    (e.Tags != null && e.Tags.Any(t => t.ToLower().Contains(searchTerm))))
                .ToList();
        }

        // Filter entries by date range
        public async Task<List<JournalEntry>> FilterAsync(DateTime? startDate, DateTime? endDate)
        {
            var all = await GetAllEntriesAsync();

            if (startDate.HasValue && endDate.HasValue)
            {
                return all.Where(e => e.CreatedAt.Date >= startDate.Value.Date &&
                                     e.CreatedAt.Date <= endDate.Value.Date).ToList();
            }
            else if (startDate.HasValue)
            {
                return all.Where(e => e.CreatedAt.Date >= startDate.Value.Date).ToList();
            }
            else if (endDate.HasValue)
            {
                return all.Where(e => e.CreatedAt.Date <= endDate.Value.Date).ToList();
            }

            return all;
        }

        // Export all entries to markdown format
        public async Task<string> ExportToMarkdownAsync()
        {
            var all = await GetAllEntriesAsync();
            var sb = new StringBuilder();

            foreach (var e in all.OrderByDescending(x => x.CreatedAt))
            {
                sb.AppendLine($"# {e.Title}");
                sb.AppendLine($"*Date:* {e.CreatedAt:yyyy-MM-dd}");
                sb.AppendLine($"*Mood:* {e.PrimaryMood}" + (e.SecondaryMoods != null && e.SecondaryMoods.Any() ? $" | Secondary: {string.Join(", ", e.SecondaryMoods)}" : ""));
                sb.AppendLine($"*Tags:* {(e.Tags != null ? string.Join(", ", e.Tags) : "")}");
                sb.AppendLine();
                sb.AppendLine(e.Content);
                sb.AppendLine("\n---\n");
            }

            return sb.ToString();
        }

        // ===============================
        // USER SETTINGS
        // ===============================

        public async Task<UserSettings> GetSettingsAsync()
        {
            using var db = Connection;
            var settings = await db.QueryFirstOrDefaultAsync<UserSettings>("SELECT * FROM UserSettings WHERE Id=1");
            if (settings == null)
            {
                settings = new UserSettings();
                await UpdateSettingsAsync(settings);
            }
            return settings;
        }

        public async Task UpdateSettingsAsync(UserSettings settings)
        {
            using var db = Connection;
            await db.ExecuteAsync(@"
                INSERT INTO UserSettings (Id, Theme, IsPasswordEnabled, PasswordHash)
                VALUES (@Id,@Theme,@IsPasswordEnabled,@PasswordHash)
                ON CONFLICT(Id) DO UPDATE SET Theme=@Theme, IsPasswordEnabled=@IsPasswordEnabled, PasswordHash=@PasswordHash",
                settings);
        }

        // ===============================
        // HELPERS
        // ===============================

        private JournalEntry ConvertFromDb(JournalEntryDb dbEntry)
        {
            return new JournalEntry
            {
                Id = dbEntry.Id,
                CreatedAt = dbEntry.CreatedAt,
                Title = dbEntry.Title,
                Content = dbEntry.Content,
                PrimaryMood = dbEntry.PrimaryMood,
                SecondaryMoods = string.IsNullOrWhiteSpace(dbEntry.SecondaryMoods)
                    ? new List<string>()
                    : dbEntry.SecondaryMoods.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .ToList(),
                Tags = string.IsNullOrWhiteSpace(dbEntry.Tags)
                    ? new List<string>()
                    : dbEntry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .ToList()
            };
        }
    }
}