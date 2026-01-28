using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journal_Entry.Components.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Today;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PrimaryMood { get; set; } = "Neutral";

        // These will be stored as comma-separated strings in the database
        // But converted to List<string> by ParseEntryFields in JournalService
        public List<string> SecondaryMoods { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();

        public int WordCount => string.IsNullOrWhiteSpace(Content)
            ? 0
            : Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}