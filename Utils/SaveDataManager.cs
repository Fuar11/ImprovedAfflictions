using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModData;
using MelonLoader;


namespace ImprovedAfflictions.Utils
{
    internal class SaveDataManager
    {

        ModDataManager dm = new ModDataManager("Improved Afflictions", false);

        public void Save(string data, string suffix)
        {
            dm.Save(data, suffix);
        }

        public string LoadData(string suffix)
        {
            string? data = dm.Load(suffix);
            return data;
        }

    }
}
