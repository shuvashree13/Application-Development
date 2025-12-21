using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journal_Entry.Components.Models
{
    public class JournalEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PrimaryMood { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime startDate { get; set; }
        public DateTime EntryDate => CreatedAt.Date;
    }
}
