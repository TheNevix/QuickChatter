using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickChatter.Models.Domain
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
