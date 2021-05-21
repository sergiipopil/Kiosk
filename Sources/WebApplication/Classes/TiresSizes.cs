using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Classes
{
    public class TiresSizes
    {
        public IList<string> GetTiresRSize() {
            return new List<string>() { "R10", "R12", "R13", "R14", "R15", "R16", "R17", "R18", "R19", "R20", "R21", "R22", "R23", "R24" };
        }
        public IList<string> GetTiresCnt()
        {
            return new List<string>() { "1X", "2X", "3X", "4X" };
        }
        public IList<string> GetTiresWidth()
        {
            return new List<string>() { "60", "70", "80", "90", "100", "110", "120", "125", "130", "135", "140", "145", "150", "155", "160", "165", "170", "175",
                "180", "185", "190", "195", "200", "205", "210", "215", "220", "225", "230", "235", "240", "245", "250", "255", "260", "265", "270", "275", "280", "285", "290", "295", "300", "305", "310", "315", "320",
                "325", "335", "345", "355", "365", "375", "380", "385", "400", "450", "460", "500", "520", "690", "710", "750", "800" };
        }
        public IList<string> GetTiresHeight()
        {
            return new List<string>() { "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80", "82", "85", "90", "95", "100" };
        }
    }
}
