using CodelessModBuilder.src.Customs;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodelessModBuilder.src
{
    public enum ResourceType
    {
        Unknown = 0,
        JsonMaterial = 1,
        JsonDecor = 2
    }

    public abstract class Resource
    {
        public virtual string Name { get; protected set; }
        public virtual string Data { get; protected set; }
        public abstract ResourceType Type { get; }
        public virtual bool WasRegistered { get; protected set; } = false;

        public Resource(string name, string data)
        {
            Name = name;
            Data = data;
        }

        public abstract void Register(in GameData gameData);
    }

    public abstract class JsonResource<T> : Resource
    {
        public JObject JsonObject { get; protected set; }

        private T _convertedCache;

        protected JsonResource(string name, string data) : base(name, data)
        {
            JsonObject = JObject.Parse(data);
        }

        public sealed override void Register(in GameData gameData)
        {
            _convertedCache = Convert();
            if (_convertedCache == null)
            {
                Main.LogError("Failed to convert JsonResource!");
                return;
            }
            OnRegister(in gameData, _convertedCache);
            WasRegistered = true;
        }

        public abstract void OnRegister(in GameData gameData, T obj);

        protected virtual T Convert()
        {
            if (LoadObjectFromJSON(JsonObject, out T obj))
            {
                return obj;
            }
            return default;
        }

        protected abstract bool LoadObjectFromJSON(JObject jsonObject, out T obj);
    }

    public class JsonMaterial : JsonResource<Material>
    {
        public override ResourceType Type => ResourceType.JsonMaterial;

        public JsonMaterial(string name, string data) : base(name, data)
        {
        }

        public override void OnRegister(in GameData gameData, Material material)
        {
            if (CustomMaterials.CustomMaterialsIndex.ContainsKey(Name))
            {
                Main.LogWarning($"CustomMaterialIndex already contains {Name}! All references will use previously registered version.");
            }
            CustomMaterials.CustomMaterialsIndex.Add(Name, material);
        }

        protected override bool LoadObjectFromJSON(JObject jsonObject, out Material material)
        {
            BaseJson baseJson = null;
            material = null;
            try
            {
                if (jsonObject.TryGetValue("Type", out var value))
                {
                    JsonType key = value.ToObject<JsonType>();
                    object obj = jsonObject.ToObject(JSONManager.keyValuePairs[key]);
                    baseJson = obj as BaseJson;
                }
            }
            catch (Exception ex)
            {
                Main.LogWarning("Could not load JsonMaterial.");
                Main.LogWarning(ex.Message);
            }
            if (baseJson != null)
            {
                if (baseJson is CustomMaterial)
                {
                    CustomMaterial customMaterial = baseJson as CustomMaterial;
                    customMaterial.Deserialise();
                    customMaterial.ConvertMaterial(out material);
                }
                if (baseJson is CustomBaseMaterial)
                {
                    CustomBaseMaterial customBaseMaterial = baseJson as CustomBaseMaterial;
                    customBaseMaterial.ConvertMaterial(out material);
                }
            }
            return material != null;
        }
    }

    public class JsonDecor : JsonResource<DecorData>
    {
        public override ResourceType Type => ResourceType.JsonDecor;

        public JsonDecor(string name, string data) : base(name, data)
        {
        }

        public override void OnRegister(in GameData gameData, DecorData obj)
        {
            Material material = obj.UseCustomMaterial ? GetCustomMaterial(obj.MaterialName) : MaterialUtils.GetExistingMaterial(obj.MaterialName);
            if (material == null)
            {
                Main.LogInfo($"Failed to get material - {obj.MaterialName}.");
                return;
            }

            if (!gameData.TryGet(obj.Type == LayoutMaterialType.Wallpaper ? ApplianceReferences.WallpaperApplicator : ApplianceReferences.FlooringApplicator, out Appliance applicatorAppliance))
            {
                Main.LogInfo($"Failed to get applicator appliance.");
                return;
            }

            string decorName = $"{obj.Type} - {obj.Name}";
            int id = StringUtils.GetInt32HashCode(decorName);

            Decor decor = ScriptableObject.CreateInstance<Decor>();
            decor.name = decorName;
            decor.ID = id;
            decor.Material = material;
            decor.Type = obj.Type;
            decor.ApplicatorAppliance = applicatorAppliance;
            decor.IsAvailable = obj.IsAvailable;

            gameData.Objects.Add(id, decor);
            Main.LogInfo(id);
        }

        private Material GetCustomMaterial(string materialName)
        {
            try
            {
                return MaterialUtils.GetCustomMaterial(materialName);
            }
            catch { }
            return null;
        }

        protected override bool LoadObjectFromJSON(JObject jsonObject, out DecorData decorData)
        {
            decorData = null;
            try
            {
                decorData = jsonObject.ToObject<DecorData>();
                return true;
            }
            catch
            {
                Main.LogError("Could not load DecorData.");
            }
            return false;
        }
    }

    public sealed class ResourceDirectory
    {
        public Dictionary<ResourceType, HashSet<Resource>> Resources { get; private set; }

        public ResourceDirectory()
        {
            Resources = new Dictionary<ResourceType, HashSet<Resource>>();
        }

        internal List<ResourceType> EarlyRegisterTypes => new List<ResourceType>() { ResourceType.JsonMaterial };
        internal List<ResourceType> NormalRegisterTypes => new List<ResourceType>() { ResourceType.JsonDecor };
        internal List<ResourceType> LateRegisterTypes => new List<ResourceType>() { };

        public void Add<T>(T resource) where T : Resource
        {
            if (!Resources.ContainsKey(resource.Type))
            {
                Resources.Add(resource.Type, new HashSet<Resource>());
            }
            Resources[resource.Type].Add(resource);
        }

        public IEnumerable<Resource> Get(ResourceType type)
        {
            if (!Resources.TryGetValue(type, out HashSet<Resource> resourcesOfType))
            {
                return new HashSet<Resource>();
            }
            return resourcesOfType;
        }

        internal void EarlyRegister(in GameData gameData)
        {
            PrivateRegister(gameData, EarlyRegisterTypes);
        }

        internal void Register(in GameData gameData)
        {
            PrivateRegister(gameData, NormalRegisterTypes);
        }

        internal void LateRegister(in GameData gameData)
        {
            PrivateRegister(gameData, LateRegisterTypes);
        }

        private void PrivateRegister(in GameData gameData, List<ResourceType> resourceTypes)
        {
            foreach (ResourceType resourceType in resourceTypes)
            {
                if (!Resources.TryGetValue(resourceType, out HashSet<Resource> resourcesOfType))
                {
                    Main.LogWarning($"No {resourceType} to register.");
                    continue;
                }

                foreach (Resource resource in resourcesOfType)
                {
                    Main.LogInfo($"{(resource.WasRegistered ? "Reregistering" : "Registering")} {resourceType} - {resource.Name}...");
                    resource.Register(in gameData);
                }
            }
        }
    }
}
