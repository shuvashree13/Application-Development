using Journal_Entry.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journal_Entry.Components.Services
{
    public class JournalService
    {
        private readonly List<JournalEntry> _entries = new();

        public JournalEntry? GetByDate(DateTime date)
        {
            var dateOnly = date.Date;
            return _entries.FirstOrDefault(e => e.EntryDate == dateOnly);
        }

        public void Save(JournalEntry entry)
        {
            var existing = GetByDate(entry.EntryDate);
            if (existing != null)
            {
                // Update existing entry
                existing.Title = entry.Title;
                existing.Content = entry.Content;
                existing.PrimaryMood = entry.PrimaryMood;
            }
            else
            {
                _entries.Add(entry);
            }
        }

        public List<JournalEntry> GetAll() => _entries;


    }
}
