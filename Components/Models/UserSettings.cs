using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Journal_Entry.Components.Models
{
    public class UserSettings
    {
        
        public int Id { get; set; } = 1;

        public string Theme { get; set; } = "Light";

        public bool IsPasswordEnabled { get; set; }

        public string PasswordHash { get; set; } = string.Empty;
    }
}
