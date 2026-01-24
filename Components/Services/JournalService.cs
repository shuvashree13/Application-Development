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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Journal_Entry.Components.Services
{
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
            var entries = (await db.QueryAsync<JournalEntry>("SELECT * FROM JournalEntry")).ToList();
            entries.ForEach(ParseEntryFields);
            return entries.OrderByDescending(e => e.CreatedAt).ToList();
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            using var db = Connection;
            var entry = await db.QueryFirstOrDefaultAsync<JournalEntry>(
                "SELECT * FROM JournalEntry WHERE date(CreatedAt)=date(@date)", new { date = date.ToString("yyyy-MM-dd") });
            if (entry != null) ParseEntryFields(entry);
            return entry;
        }

        public async Task SaveEntryAsync(JournalEntry entry)
        {
            using var db = Connection;

            var secondaryMoods = string.Join(",", entry.SecondaryMoods ?? new List<string>());
            var tags = string.Join(",", entry.Tags ?? new List<string>());

            if (entry.Id == 0)
            {
                await db.ExecuteAsync(@"INSERT INTO JournalEntry 
                    (Title, Content, PrimaryMood, SecondaryMoods, Tags, CreatedAt)
                    VALUES (@Title,@Content,@PrimaryMood,@SecondaryMoods,@Tags,@CreatedAt)",
                    new { entry.Title, entry.Content, entry.PrimaryMood, SecondaryMoods = secondaryMoods, Tags = tags, entry.CreatedAt });
            }
            else
            {
                await db.ExecuteAsync(@"UPDATE JournalEntry SET
                    Title=@Title, Content=@Content, PrimaryMood=@PrimaryMood,
                    SecondaryMoods=@SecondaryMoods, Tags=@Tags
                    WHERE Id=@Id",
                    new { entry.Title, entry.Content, entry.PrimaryMood, SecondaryMoods = secondaryMoods, Tags = tags, entry.Id });
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
            var sb = new System.Text.StringBuilder();

            foreach (var e in all.OrderByDescending(x => x.CreatedAt))
            {
                sb.AppendLine($"# {e.Title}");
                sb.AppendLine($"*Date:* {e.CreatedAt:yyyy-MM-dd}");
                sb.AppendLine($"*Mood:* {e.PrimaryMood}" + (e.SecondaryMoods.Any() ? $" | Secondary: {string.Join(", ", e.SecondaryMoods)}" : ""));
                sb.AppendLine($"*Tags:* {string.Join(", ", e.Tags)}");
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
        private void ParseEntryFields(JournalEntry entry)
        {
            entry.SecondaryMoods = string.IsNullOrWhiteSpace(entry.SecondaryMoods?.ToString())
                ? new List<string>()
                : entry.SecondaryMoods.ToString().Split(',').ToList();

            entry.Tags = string.IsNullOrWhiteSpace(entry.Tags?.ToString())
                ? new List<string>()
                : entry.Tags.ToString().Split(',').ToList();
        }
    }
}
