using ModName.src;
using KitchenData;
using KitchenLib.Utils;
using System.Collections.Generic;
using ModName.src.Customs;

namespace ModName.src.Utils
{
    public static class GameDataUtils
    {
        public static bool TryGetExistingGDOByName<T>(GameData gameData, string name, out T result, bool warnIfFail = true) where T : GameDataObject
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
                if (warnIfFail)
                    Main.LogWarning($"Failed to find {typeof(T).Name} with name {name}");
                result = null;
                return false;
            }
            result = gdoResult as T;
            return true;
        }
        public static bool TryGetExistingGDOByID<T>(GameData gameData, int id, out T result, bool warnIfFail = true) where T : GameDataObject
        {
            if (!gameData.TryGet(id, out result))
            {
                if (warnIfFail)
                    Main.LogWarning($"Failed to find {typeof(T).Name} with ID {id}");
                result = null;
                return false;
            }
            return true;
        }
    }
}
