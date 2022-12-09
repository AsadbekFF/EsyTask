using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EsyTask.Models
{
    public class ChangeInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<Changes> Changes { get; set; } = new List<Changes>();
        public DateTime When { get; set; } = DateTime.Now;
        public string Info { get; set; }
    }
}
