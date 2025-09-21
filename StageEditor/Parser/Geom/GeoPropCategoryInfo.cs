using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haven.Parser.Geom
{
    public class GeoPropCategoryInfo
    {
        public string CategoryName { get; }
        public bool IsMini { get; }

        public GeoPropCategoryInfo(string categoryName, bool isMini)
        {
            CategoryName = categoryName;
            IsMini = isMini;
        }

        public static GeoPropCategoryInfo Default { get; } = new GeoPropCategoryInfo("Other Props", false);
    }
}