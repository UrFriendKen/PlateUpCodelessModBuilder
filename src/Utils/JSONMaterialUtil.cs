using KitchenLib.Customs;
using KitchenLib;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using System.IO;

namespace CodelessModBuilder.src.Utils
{
    internal class JSONMaterialUtil
    {
        public static bool LoadMaterialFromJSON(string json, out Material material)
        {
            BaseJson jsonObj = null;
            material = null;
            try
            {
                JObject jObject = JObject.Parse(json);
                if (jObject.TryGetValue("Type", out var value))
                {
                    JsonType key = value.ToObject<JsonType>();
                    object obj = jObject.ToObject(JSONManager.keyValuePairs[key]);
                    jsonObj = obj as BaseJson;
                }
            }
            catch (Exception ex)
            {
                Main.LogWarning("Could not load json material.");
                Main.LogWarning(ex.Message);
            }
            if (jsonObj != null)
            {
                if (jsonObj is CustomMaterial)
                {
                    CustomMaterial customMaterial = jsonObj as CustomMaterial;
                    customMaterial.Deserialise();
                    customMaterial.ConvertMaterial(out material);
                }
                if (jsonObj is CustomBaseMaterial)
                {
                    CustomBaseMaterial customBaseMaterial = jsonObj as CustomBaseMaterial;
                    customBaseMaterial.ConvertMaterial(out material);
                }
            }
            return material != null;
        }
    }
}
