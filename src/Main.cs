﻿using CodelessModBuilder.src.Customs;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Utils;
using KitchenMods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace CodelessModBuilder.src
{
    public sealed class Main : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public static string Mod_Guid = "IcedMilo.PlateUp.CodelessModBuilder";
        public static string Mod_Name = "CodelessModBuilder";
        public static string Mod_Version = "0.1.0";
        public static string Mod_Author = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.5";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.5" current and all future
        // e.g. ">=1.1.5 <=1.2.3" for all from/until

        public readonly string AssemblyName;
        public string MetadataResourceName;
        public static readonly Dictionary<string, ResourceType> FOLDER_MAPPINGS = new Dictionary<string, ResourceType>()
        {
            { "materials", ResourceType.JsonMaterial },
            { "decors", ResourceType.JsonDecor }
        };

        public static readonly HashSet<string> ALLOWED_FILE_EXTENSIONS = new HashSet<string>()
        {
            "json"
        };

        public Main() : base(Mod_Guid, Mod_Name, Mod_Author, Mod_Version, MOD_GAMEVERSION, Assembly.GetExecutingAssembly())
        {
            AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            MetadataResourceName = $"{AssemblyName}.Resources.metadata.json";

            var assembly = Assembly.GetExecutingAssembly();
            string[] allResourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in allResourceNames)
            {
                LogInfo(resourceName);
            }
            using (var stream = assembly.GetManifestResourceStream(MetadataResourceName))
            {
                if (stream == null)
                {
                    LogError($"\"{MetadataResourceName}\" not found!");
                    return;
                }

                using (var reader = new StreamReader(stream))
                {
                    LogWarning($"\"{MetadataResourceName}\" found. Updating mod metadata...");
                    var text = reader.ReadToEnd();
                    ModMetadata metadata = JsonConvert.DeserializeObject<ModMetadata>(text);

                    Mod_Author = metadata.Author;
                    Mod_Name = metadata.ModName.IsNullOrEmpty()? AssemblyName : metadata.ModName;
                    Mod_Guid = $"{Mod_Author.Replace(" ", "_").ToLower()}:plateup:{AssemblyName.Replace(" ", "_").Replace(".", "").ToLower()}";
                    Mod_Version = metadata.ModVersion;

                    ModID = Mod_Guid;
                    ModName = Mod_Name;
                    ModVersion = Mod_Version;
                    ModAuthor = Mod_Author;
                }
            };
        }

        protected override void OnInitialise()
        {
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(Mod mod)
        {
            LogWarning($"{ModID} v{ModVersion} in use!");

            var assembly = Assembly.GetExecutingAssembly();

            ResourceDirectory resourceDirectory = new ResourceDirectory();

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName == MetadataResourceName)
                    continue;

                if (!resourceName.StartsWith(AssemblyName))
                    continue;

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception("Resource not found.");
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd();
                        string[] pathStructure = resourceName.Split('.');
                        LogInfo(string.Join(".", pathStructure));
                        string filename = null;
                        string extension = null;

                        ResourceType resourceType = ResourceType.Unknown;
                        bool success = false;
                        for (int i = 0; i < pathStructure.Length; i++)
                        {
                            string item = pathStructure[i];
                            string itemLower = item.ToLower();

                            if (i == 0 && itemLower != AssemblyName.ToLower() ||
                                i == 1 && itemLower != "resources")
                            {
                                break;
                            }

                            if (i == pathStructure.Length - 3)
                            {
                                if (!FOLDER_MAPPINGS.TryGetValue(itemLower, out ResourceType mappedResourceType))
                                {
                                    LogError($"Error parsing embedded resource! Incorrect folder name. Resources must be placed in folder matching their type. ({resourceName})");
                                    break;
                                }
                                resourceType = mappedResourceType;
                                continue;
                            }

                            if (i == pathStructure.Length - 2)
                            {
                                filename = item;
                                continue;
                            }

                            if (i == pathStructure.Length - 1)
                            {
                                if (!ALLOWED_FILE_EXTENSIONS.Contains(pathStructure.Last()))
                                {
                                    LogError($"Error parsing embedded resource! Invalid file extension. ({resourceName})");
                                    break;
                                }
                                extension = itemLower;

                                success = true;
                            }
                        }

                        if (!success)
                        {
                            continue;
                        }

                        switch (resourceType)
                        {
                            case ResourceType.JsonMaterial:
                                resourceDirectory.Add(new JsonMaterial(filename, text));
                                break;
                            case ResourceType.JsonDecor:
                                resourceDirectory.Add(new JsonDecor(filename, text));
                                break;
                            case ResourceType.Unknown:
                            default:
                                break;
                        }
                    }
                }
            }

            Events.BuildGameDataPreSetupEvent += delegate (object _, BuildGameDataEventArgs args)
            {
                resourceDirectory.EarlyRegister(args.gamedata);
            };

            Events.BuildGameDataEvent += delegate (object _, BuildGameDataEventArgs args)
            {
                resourceDirectory.Register(args.gamedata);
            };

            Events.BuildGameDataPostViewInitEvent += delegate (object _, BuildGameDataEventArgs args)
            {
                resourceDirectory.LateRegister(args.gamedata);
            };
        }
        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{Mod_Name}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{Mod_Name}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{Mod_Name}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}