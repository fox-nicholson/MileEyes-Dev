using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace MileEyes.Services.Models
{
    public class Reason : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Default { get; set; }
    }
}