using KitchenData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModName.src.Utils
{
    public static class GameDataUtils
    {
        public static bool TryGetExistingGDOByName<T>(this GameData gameData, string name, out T result) where T : GameDataObject
        {
            IEnumerable<T> gdos = gameData.Get<T>();
            Dictionary<string, GameDataObject> dict = new Dictionary<string, GameDataObject>();
            foreach (T gdo in gdos)
            {
                if (!dict.ContainsKey(gdo.name))
                    dict.Add(gdo.name, gdo);
            }

            if (!dict.TryGetValue(name, out GameDataObject gdoResult))
            {
                result = null;
                return false;
            }
            result = gdoResult as T;
            return true;
        }
    }
}
