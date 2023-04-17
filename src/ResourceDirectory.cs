using CodelessModInterop;
using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using ModName.src.Customs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModName.src
{
    public enum ResourceType
    {
        Unknown,
        JsonKLMaterial,
        JsonPatienceValues,
        JsonOrderingValues,
        JsonUnlockInfo,
        JsonDecor,
        JsonUnlockCard,
        JsonUnlockEffect,
        JsonCApplianceSpeedModifier,
        JsonCAppliesStatus,
        JsonCTableModifier,
        JsonCQueueModifier
    }

    public interface IInitialisableGDOResource
    {
        bool WasInitialised { get; set; }
        void Initialise(in GameData gameData);
        void ContinuousInitialise(in GameData gameData);
    }

    public interface IEffectTypeResource { }

    public abstract class Resource
    {
        public virtual string Name { get; protected set; }
        public virtual string Data { get; protected set; }
        public abstract ResourceType Type { get; }
        public virtual bool WasRegistered { get; protected set; } = false;

        public readonly ResourceDirectory Directory;

        public Resource(string name, string data, ResourceDirectory resourceDirectory)
        {
            Name = name;
            Data = data;
            Directory = resourceDirectory;
        }

        public abstract void Register(in GameData gameData);
    }

    public abstract class JsonResource<T> : Resource
    {
        public JObject JsonObject { get; protected set; }

        private T _convertedCache;

        protected JsonResource(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
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

        protected virtual bool LoadObjectFromJSON(JObject jsonObject, out T obj)
        {
            obj = default;
            try
            {
                obj = jsonObject.ToObject<T>();
                return true;
            }
            catch
            {
                string className = GetType().Name;
                Main.LogError($"Could not load {(className.ToLower().StartsWith("json") && className.Length > 4? className.Substring(4) : className)}.");
            }
            return false;
        }
    }

    public class JsonKLMaterial : JsonResource<Material>
    {
        public override ResourceType Type => ResourceType.JsonKLMaterial;
        public JsonKLMaterial(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, Material material)
        {
            if (CustomMaterials.CustomMaterialsIndex.ContainsKey(material.name))
            {
                Main.LogWarning($"CustomMaterialIndex already contains {Name}! All references will use previously registered version.");
                return;
            }
            CustomMaterials.CustomMaterialsIndex.Add(material.name, material);
            Directory.AddMaterial(material);
            Main.LogInfo($"Successfully registered Material - {material.name}");
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
                Main.LogWarning("Could not load JsonKLMaterial.");
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

        public JsonDecor(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, DecorData decorData)
        {
            if (!decorData.TryConvert(gameData, Directory, out Decor decor))
                return;

            gameData.Objects.Add(decor.ID, decor);
            Directory.AddDecor(decor);
            Main.LogInfo($"Successfully registered {decorData.Name} ({decor.ID})");
        }
    }

    public abstract class JsonStructData<TStructData, TStruct> : JsonResource<TStructData> where TStructData : StructData<TStruct> where TStruct : struct
    {
        protected JsonStructData(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, TStructData structData)
        {
            if (Directory.AddStructData(structData))
            {
                Main.LogInfo($"Successfully registered {typeof(TStruct).Name} - {structData.Name}");
            }
        }
    }

    public class JsonPatienceValues : JsonStructData<PatienceValuesData, PatienceValues>
    {
        public override ResourceType Type => ResourceType.JsonPatienceValues;
        public JsonPatienceValues(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }
    }

    public class JsonOrderingValues : JsonStructData<OrderingValuesData, OrderingValues>
    {
        public override ResourceType Type => ResourceType.JsonOrderingValues;
        public JsonOrderingValues(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }
    }

    public class JsonUnlocKInfo : JsonResource<UnlockInfoData>
    {
        public override ResourceType Type => ResourceType.JsonUnlockInfo;

        public JsonUnlocKInfo(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, UnlockInfoData unlockInfoData)
        {
            if (unlockInfoData.TryConvert(out UnlockInfo unlockInfo) && Directory.AddLocalisationItem(unlockInfoData.Name, unlockInfo))
            {
                Main.LogInfo($"Successfully registered Localisation - {unlockInfoData.Name}");
            }
        }
    }

    public class JsonUnlockEffect : JsonResource<UnlockEffectData>
    {
        public override ResourceType Type => ResourceType.JsonUnlockEffect;

        public JsonUnlockEffect(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, UnlockEffectData unlockEffectData)
        {
            if (Directory.AddUnlockEffects(unlockEffectData.Name, unlockEffectData.GetAllUnlockEffects(gameData, Directory)))
            {
                Main.LogInfo($"Successfully registered UnlockEffect - {unlockEffectData.Name}");
            }
        }
    }

    public class JsonUnlockCard : JsonResource<UnlockCardData>, IInitialisableGDOResource
    {
        public bool WasInitialised { get; set; } = false;

        public override ResourceType Type => ResourceType.JsonUnlockCard;

        protected string unlockCardName = string.Empty;

        protected int unlockId = 0;

        protected List<string> unlockEffectNames;

        protected List<string> requiredUnlockNames;

        protected List<string> blockedByUnlockNames;

        public JsonUnlockCard(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, UnlockCardData unlockCardData)
        {
            if (!unlockCardData.TryConvert(gameData, Directory, out UnlockCard unlockCard))
                return;

            WasInitialised = false;

            unlockId = unlockCard.ID;
            unlockCardName = unlockCard.name;
            unlockEffectNames = unlockCardData.UnlockEffectNames;
            requiredUnlockNames = unlockCardData.RequiredCards?? new List<string>();
            blockedByUnlockNames = unlockCardData.BlockedByCards ?? new List<string>();

            gameData.Objects.Add(unlockCard.ID, unlockCard);
            Directory.AddUnlockCard(unlockCard);
            Main.LogInfo($"Successfully registered {unlockCardData.Name} ({unlockCard.ID})");
        }

        protected override bool LoadObjectFromJSON(JObject jsonObject, out UnlockCardData unlockCardData)
        {
            unlockCardData = null;
            try
            {
                unlockCardData = jsonObject.ToObject<UnlockCardData>();
                return true;
            }
            catch
            {
                Main.LogError("Could not load UnlockCard.");
            }
            return false;
        }

        public virtual void Initialise(in GameData gameData)
        {
            if (!WasInitialised)
            {
                Directory.AddContinuousInitialise(this);
                Main.LogInfo($"Successfully initialised {unlockCardName} ({unlockId}).");
            }
            WasInitialised = true;
        }

        public virtual void ContinuousInitialise(in GameData gameData)
        {
            if (!gameData.TryGet(unlockId, out UnlockCard unlockCard))
            {
                Main.LogWarning($"Failed to get UnlockCard - {unlockCardName} ({unlockId}).");
                return;
            }

            if (unlockEffectNames != null && unlockEffectNames.Count > 0 && unlockCard.Effects.Count == 0)
            {
                Main.LogInfo($"Updating {unlockCardName} Effects...");
                foreach (string unlockEffectName in unlockEffectNames)
                {
                    if (!Directory.TryGetUnlockEffects(unlockEffectName, out IEnumerable<UnlockEffect> unlockEffects))
                    {
                        Main.LogWarning($"Failed to get UnlockEffect - {unlockEffectName}.");
                    }
                    else
                    {
                        unlockCard.Effects.AddRange(unlockEffects);
                    }
                }
            }

            Dictionary<string, Unlock> unlocksByName = gameData.Get<Unlock>().ToDictionary(x => x.name, x => x);

            if (requiredUnlockNames != null && requiredUnlockNames.Count > 0 && unlockCard.Requires.Count == 0)
            {
                Main.LogInfo($"Updating {unlockCardName} Requires...");
                foreach (string requiredUnlockName in requiredUnlockNames)
                {
                    if (!unlocksByName.TryGetValue(requiredUnlockName, out Unlock unlock))
                    {
                        Main.LogWarning($"Failed to get required Unlock - {requiredUnlockName}.");
                        continue;
                    }
                    unlockCard.Requires.Add(unlock);
                }
            }

            if (blockedByUnlockNames != null && blockedByUnlockNames.Count > 0 && unlockCard.BlockedBy.Count == 0)
            {
                Main.LogInfo($"Updating {unlockCardName} BlockedBy...");
                foreach (string blockedByUnlockName in blockedByUnlockNames)
                {
                    if (!unlocksByName.TryGetValue(blockedByUnlockName, out Unlock unlock))
                    {
                        Main.LogWarning($"Failed to get blocked by Unlock - {blockedByUnlockName}.");
                        continue;
                    }
                    unlockCard.BlockedBy.Add(unlock);
                }
            }
        }
    }

    public abstract class JsonIEffectType<T1, T2> : JsonResource<T1>, IEffectTypeResource where T1 : EffectTypeData<T2> where T2 : IEffectType
    {
        public JsonIEffectType(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, T1 effectTypeData)
        {
            if (effectTypeData.TryConvert(gameData, Directory, out T2 effectType) && Directory.AddEffectType(effectTypeData.Name, effectType))
            {
                Main.LogInfo($"Successfully registered {typeof(T2).Name} - {effectTypeData.Name}");
            }
        }
    }

    public class JsonCApplianceModifier : JsonIEffectType<CApplianceSpeedModifierEffectData, CApplianceSpeedModifier>
    {
        public override ResourceType Type => ResourceType.JsonCApplianceSpeedModifier;
        public JsonCApplianceModifier(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory) { }
    }

    public class JsonCAppliesStatus : JsonIEffectType<CAppliesStatusEffectData, CAppliesStatus>
    {
        public override ResourceType Type => ResourceType.JsonCAppliesStatus;
        public JsonCAppliesStatus(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory) { }
    }

    public class JsonCTableModifier : JsonIEffectType<CTableModifierEffectData, CTableModifier>
    {
        public override ResourceType Type => ResourceType.JsonCTableModifier;

        public JsonCTableModifier(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory) { }
    }

    public class JsonCQueueModifier : JsonIEffectType<CQueueModifierEffectData, CQueueModifier>
    {
        public override ResourceType Type => ResourceType.JsonCQueueModifier;
        public JsonCQueueModifier(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory) { }
    }

    public sealed class ResourceDirectory
    {
        public Dictionary<ResourceType, HashSet<Resource>> Resources { get; private set; }

        private readonly HashSet<IInitialisableGDOResource> _continuousInitialise;

        private readonly StructDataDictionary _structDatas = new StructDataDictionary();  

        public readonly string ModGuid;
        public readonly string ModName;

        public ResourceDirectory(string modGuid, string modName)
        {
            ModGuid = modGuid;
            ModName = modName;

            Resources = new Dictionary<ResourceType, HashSet<Resource>>();
            _continuousInitialise = new HashSet<IInitialisableGDOResource>();
        }

        internal List<ResourceType> EarlyRegisterTypes => new List<ResourceType>()
        {
            ResourceType.JsonKLMaterial,
            ResourceType.JsonPatienceValues,
            ResourceType.JsonOrderingValues,
            ResourceType.JsonUnlockInfo
        };
        internal List<ResourceType> NormalRegisterTypes => new List<ResourceType>()
        {
            ResourceType.JsonDecor,
            ResourceType.JsonUnlockCard
        };
        internal List<ResourceType> LateRegisterTypes => new List<ResourceType>()
        {
            ResourceType.JsonCApplianceSpeedModifier,
            ResourceType.JsonCAppliesStatus,
            ResourceType.JsonCTableModifier,
            ResourceType.JsonCQueueModifier,
            ResourceType.JsonUnlockEffect
        };

        internal List<ResourceType> InitialiseTypes => new List<ResourceType>()
        {
            ResourceType.JsonUnlockCard
        };

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

        public bool TryGetStructData<T>(string name, out T baseStructData) where T : BaseStructData
        {
            return _structDatas.TryGetValue(name, out baseStructData);
        }

        public bool TryGetConvertedStruct<T>(string name, out T convertedStruct) where T : struct
        {
            return _structDatas.TryGetConvertedValue(name, out convertedStruct);
        }

        public bool AddStructData(BaseStructData baseStructData)
        {
            if (_structDatas.ContainsKey(baseStructData.Name))
            {
                Type registeredType = _structDatas[baseStructData.Name].StructType;
                bool sameType = baseStructData.StructType.Name == registeredType.Name;
                if (sameType)
                {
                    Main.LogWarning($"{baseStructData.StructType.Name} with Name {baseStructData.Name} already registered.");
                }
                else
                {
                    Main.LogWarning($"{baseStructData.StructType.Name} with Name {baseStructData.Name} has the same name as already registered {registeredType.Name}.");
                }
                return false;
            }
            _structDatas.Add(baseStructData.Name, baseStructData);
            return true;
        }

        public bool AddMaterial(Material material)
        {
            return ModdedResourceRegistry.RegisterModdedMaterial(material);
        }

        public bool AddUnlockEffects(string name, IEnumerable<UnlockEffect> unlockEffects)
        {
            return ModdedResourceRegistry.RegisterModdedUnlockEffectSet(name, unlockEffects);
        }

        public bool TryGetUnlockEffects(string name, out IEnumerable<UnlockEffect> unlockEffects)
        {
            return ModdedResourceRegistry.TryGetModdedUnlockEffectSet(name, out unlockEffects);
        }

        public bool AddLocalisationItem(string name, Localisation localisation)
        {
            return ModdedResourceRegistry.RegisterModdedLocalisation(name, localisation);
        }

        public bool TryGetLocalisationItem<T>(string name, out T localisation) where T : Localisation
        {
            localisation = null;
            if (!ModdedResourceRegistry.TryGetModdedLocalisation(name, out Localisation loc))
            {
                return false;
            }
            localisation = loc as T;
            return true;
        }

        public bool AddEffectType(string name, IEffectType effectType)
        {
            return ModdedResourceRegistry.RegisterModdedEffectType(name, effectType);
        }

        public bool TryGetEffectType(string name, out IEffectType effectType)
        {
            return ModdedResourceRegistry.TryGetModdedEffectType(name, out effectType);
        }

        public bool AddDecor(Decor decor)
        {
            return ModdedResourceRegistry.RegisterModdedGDO(ModGuid, decor);
        }

        public bool AddUnlockCard(UnlockCard unlockCard)
        {
            return ModdedResourceRegistry.RegisterModdedGDO(ModGuid, unlockCard);
        }

        public bool AddContinuousInitialise(IInitialisableGDOResource resource)
        {
            if (_continuousInitialise.Contains(resource))
                return false;
            _continuousInitialise.Add(resource);
            return true;
        }

        public void EarlyRegister(in GameData gameData)
        {
            PrivateRegister(gameData, EarlyRegisterTypes);
        }

        public void Register(in GameData gameData)
        {
            PrivateRegister(gameData, NormalRegisterTypes);
        }

        public void LateRegister(in GameData gameData)
        {
            PrivateRegister(gameData, LateRegisterTypes);
        }

        public void Initialise(in GameData gameData)
        {
            PrivateInitialise(gameData, InitialiseTypes);
        }

        public void ContinuousInitialise(in GameData gameData)
        {
            PrivateContinuousInitialise(gameData);
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

        private void PrivateInitialise(in GameData gameData, List<ResourceType> resourceTypes)
        {
            foreach (ResourceType resourceType in resourceTypes)
            {
                if (!Resources.TryGetValue(resourceType, out HashSet<Resource> resourcesOfType))
                {
                    Main.LogWarning($"No {resourceType} to initialise.");
                    continue;
                }

                foreach (Resource resource in resourcesOfType)
                {
                    if (resource is IInitialisableGDOResource)
                    {
                        IInitialisableGDOResource initResource = resource as IInitialisableGDOResource;
                        Main.LogInfo($"{(initResource.WasInitialised ? "Reinitialising" : "Initialising")} {resourceType} - {resource.Name}...");
                        initResource.Initialise(in gameData);
                    }
                }
            }
        }

        private void PrivateContinuousInitialise(in GameData gameData)
        {
            foreach (IInitialisableGDOResource resource in _continuousInitialise)
            {
                resource.ContinuousInitialise(gameData);
            }
        }
    }
}
