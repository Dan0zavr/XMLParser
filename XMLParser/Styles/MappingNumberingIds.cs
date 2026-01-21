using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class MappingNumberingIds
    {
        public StyleCategory Category { get; set; }
        public int CurrentId { get; set; }
        public int? ReplaceId { get; set; }
        
    }
}
