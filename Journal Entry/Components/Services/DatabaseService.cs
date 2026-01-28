using Journal_Entry.Components.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journal_Entry.Components.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            _db = new SQLiteAsyncConnection(path);

            // Create tables if they don't exist
            _db.CreateTableAsync<JournalEntry>().Wait();
            _db.CreateTableAsync<UserSettings>().Wait();
        }

        

        // Get user settings
        public async Task<UserSettings> GetSettingsAsync()
        {
            var settings = await _db.Table<UserSettings>().FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new UserSettings();
                await _db.InsertAsync(settings);
            }
            return settings;
        }

        // Save or update settings
        public Task UpdateSettingsAsync(UserSettings settings)
        {
            return _db.InsertOrReplaceAsync(settings);
        }

        

        // Get all journal entries
        public Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return _db.Table<JournalEntry>().OrderByDescending(e => e.CreatedAt).ToListAsync();
        }

        // Get a single entry by date
        public Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return _db.Table<JournalEntry>()
                      .Where(x => x.CreatedAt.Date == date.Date)
                      .FirstOrDefaultAsync();
        }

        // Insert a new entry
        public Task InsertEntryAsync(JournalEntry entry)
        {
            return _db.InsertAsync(entry);
        }

        // Update an existing entry
        public Task UpdateEntryAsync(JournalEntry entry)
        {
            return _db.UpdateAsync(entry);
        }

        // Delete an entry by ID
        public Task DeleteEntryAsync(int id)
        {
            return _db.DeleteAsync<JournalEntry>(id);
        }
    }
    }
