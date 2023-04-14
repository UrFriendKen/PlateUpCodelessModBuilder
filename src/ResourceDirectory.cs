using CodelessModBuilder.src.Customs;
using CodelessModInterop;
using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using ModName.src.Customs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodelessModBuilder.src
{
    public enum ResourceType
    {
        Unknown,
        JsonMaterial,
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

    public class JsonMaterial : JsonResource<Material>
    {
        public override ResourceType Type => ResourceType.JsonMaterial;
        public JsonMaterial(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
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
            ModdedResourceRegistry.RegisterModdedMaterial(material);
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

        public JsonDecor(string name, string data, ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, DecorData obj)
        {
            Material material = obj.UseCustomMaterial ? GetCustomMaterial(obj.MaterialName) : MaterialUtils.GetExistingMaterial(obj.MaterialName);
            if (material == null)
            {
                Main.LogWarning($"Failed to get material - {obj.MaterialName}.");
                return;
            }

            if (!gameData.TryGet(obj.Type == LayoutMaterialType.Wallpaper ? ApplianceReferences.WallpaperApplicator : ApplianceReferences.FlooringApplicator, out Appliance applicatorAppliance))
            {
                Main.LogWarning($"Failed to get applicator appliance.");
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
            ModdedResourceRegistry.RegisterModdedGDO(Directory.ModName, decor);
            Main.LogInfo($"Successfully registered {obj.Name} ({id})");
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
    }

    public class JsonUnlocKInfo : JsonResource<UnlockInfoData>
    {
        public override ResourceType Type => ResourceType.JsonUnlockInfo;

        public JsonUnlocKInfo(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, UnlockInfoData obj)
        {
            if (obj.TryConvert(out UnlockInfo unlockInfo) && Directory.AddLocalisationItem(obj.Name, unlockInfo))
            {
                Main.LogInfo($"Successfully registered Localisation - {obj.Name}");
            }
        }
    }

    public class JsonUnlockEffect : JsonResource<UnlockEffectData>
    {
        public override ResourceType Type => ResourceType.JsonUnlockEffect;

        public JsonUnlockEffect(string name, string data, in ResourceDirectory resourceDirectory) : base(name, data, resourceDirectory)
        {
        }

        public override void OnRegister(in GameData gameData, UnlockEffectData obj)
        {
            if (Directory.AddUnlockEffectData(obj.Name, obj))
            {
                Main.LogInfo($"Successfully registered UnlockEffect - {obj.Name}");
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

        public override void OnRegister(in GameData gameData, UnlockCardData obj)
        {
            WasInitialised = false;
            if (!Enum.TryParse(obj.UnlockGroup, ignoreCase: true, out UnlockGroup unlockGroup))
            {
                Main.LogWarning($"Failed to parse UnlockGroup.");
                return;
            }

            if (!Enum.TryParse(obj.CardType, ignoreCase: true, out CardType cardType))
            {
                Main.LogWarning($"Failed to parse CardType.");
                return;
            }

            if (!Enum.TryParse(obj.ExpRewardLevel, ignoreCase: true, out Unlock.RewardLevel expReward))
            {
                Main.LogWarning($"Failed to parse ExpRewardLevel.");
                return;
            }

            if (!Enum.TryParse(obj.CustomerMultiplierLevel, ignoreCase: true, out DishCustomerChange customerChange))
            {
                Main.LogWarning($"Failed to parse CustomerMultiplierLevel.");
                return;
            }

            LocalisationObject<UnlockInfo> info = new LocalisationObject<UnlockInfo>();
            foreach (string localisationName in obj.UnlockInfoNames)
            {
                if (!Directory.TryGetLocalisationItem(localisationName, out UnlockInfo unlockInfo))
                {
                    continue;
                }
                info.Add(unlockInfo.Locale, unlockInfo);
            }

            int id = StringUtils.GetInt32HashCode(obj.Name);

            UnlockCard unlockCard = ScriptableObject.CreateInstance<UnlockCard>();
            unlockCard.ID = id;
            unlockId = id;
            unlockCard.name = obj.Name;
            unlockCardName = obj.Name;
            unlockCard.ExpReward = expReward;
            unlockCard.IsUnlockable = obj.IsUnlockable;
            unlockCard.UnlockGroup = unlockGroup;
            unlockCard.CardType = cardType;
            unlockCard.MinimumFranchiseTier = obj.MinimumFranchiseTier;
            unlockCard.IsSpecificFranchiseTier = obj.IsExactFranchiseTier;
            unlockCard.CustomerMultiplier = customerChange;
            unlockCard.SelectionBias = obj.SelectionBias;
            unlockCard.Info = info;
            unlockCard.Localisation = info.Get(Localisation.CurrentLocale);
            unlockCard.Effects = new List<UnlockEffect>();
            unlockEffectNames = obj.UnlockEffectNames;
            unlockCard.Requires = new List<Unlock>();
            requiredUnlockNames = obj.RequiredCards?? new List<string>();
            unlockCard.BlockedBy = new List<Unlock>();
            blockedByUnlockNames = obj.BlockedByCards ?? new List<string>();

            gameData.Objects.Add(id, unlockCard);
            ModdedResourceRegistry.RegisterModdedGDO(Directory.ModName, unlockCard);
            Main.LogInfo($"Successfully registered {obj.Name} ({id})");
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
                    if (!Directory.TryGetUnlockEffectData(unlockEffectName, out UnlockEffectData unlockEffectData))
                    {
                        Main.LogWarning($"Failed to get UnlockEffect - {unlockEffectName}.");
                    }
                    else
                    {
                        unlockCard.Effects.AddRange(unlockEffectData.GetAllUnlockEffects(gameData, Directory));
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
            if (effectTypeData.TryConvert(gameData, out T2 effectType) && Directory.AddEffectType(effectTypeData.Name, effectType))
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

        private readonly Dictionary<string, UnlockEffectData> _unlockEffectDatas;

        private readonly Dictionary<string, Localisation> _localisations;

        private readonly Dictionary<string, IEffectType> _effectTypes;

        private readonly HashSet<IInitialisableGDOResource> _continuousInitialise;

        public readonly string ModGuid;
        public readonly string ModName;

        public ResourceDirectory(string modGuid, string modName)
        {
            ModGuid = modGuid;
            ModName = modName;

            Resources = new Dictionary<ResourceType, HashSet<Resource>>();
            _unlockEffectDatas = new Dictionary<string, UnlockEffectData>();
            _localisations = new Dictionary<string, Localisation>();
            _effectTypes = new Dictionary<string, IEffectType>();
            _continuousInitialise = new HashSet<IInitialisableGDOResource>();
        }

        internal List<ResourceType> EarlyRegisterTypes => new List<ResourceType>()
        {
            ResourceType.JsonMaterial,
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

        public bool AddUnlockEffectData(string name, UnlockEffectData unlockEffectData)
        {
            if (_unlockEffectDatas.ContainsKey(name))
            {
                Main.LogWarning($"ResourceDirectory already contains UnlockEffectData {name}.");
                return false;
            }
            _unlockEffectDatas.Add(name, unlockEffectData);
            return true;
        }

        public bool TryGetUnlockEffectData(string name, out UnlockEffectData unlockEffectData)
        {
            if (!_unlockEffectDatas.TryGetValue(name, out unlockEffectData))
            {
                Main.LogError($"ResourceDirectory does not contain UnlockEffectData {name}.");
                unlockEffectData = null;
                return false;
            }
            return true;
        }

        public bool AddLocalisationItem(string name, Localisation localisation)
        {
            if (_localisations.ContainsKey(name))
            {
                Main.LogWarning($"ResourceDirectory already contains Localisation {name}.");
                return false;
            }
            _localisations.Add(name, localisation);
            return true;
        }

        public bool TryGetLocalisationItem<T>(string name, out T localisation) where T : Localisation
        {
            localisation = null;
            if (!_localisations.TryGetValue(name, out Localisation obj))
            {
                Main.LogError($"ResourceDirectory does not contain {typeof(T).Name} {name}.");
                return false;
            }

            if (!(obj is T))
            {
                Main.LogError($"Localisation {name} does not match type {typeof(T).Name}.");
                return false;
            }
            localisation = obj as T;
            return true;
        }

        public bool AddEffectType(string name, IEffectType effectType)
        {
            if (_effectTypes.ContainsKey(name))
            {
                Main.LogWarning($"ResourceDirectory already contains EffectType {name}.");
                return false;
            }
            _effectTypes.Add(name, effectType);
            return true;
        }

        public bool TryGetEffectType(string name, out IEffectType effectType)
        {
            if (!_effectTypes.TryGetValue(name, out effectType))
            {
                Main.LogError($"ResourceDirectory does not contain EffectType {name}.");
                effectType = default;
                return false;
            }
            return true;
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
            Main.LogInfo($"_continuousInitialise.Count = {_continuousInitialise.Count}");
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
