using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ItemOnLevel
{
    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ItemOnLevel : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "General";
        public const string PluginName = "ItemOnLevel";
        public const string PluginVersion = "1.0.1";

        // A string representing a dictionary of the level to item formatted as "5-Feather,15-Feather"
        public static ConfigEntry<string> LevelToItemString { get; set; }

        // Global dictionary for level to item
        private static Dictionary<float, string> levelToItem = new Dictionary<float, string>();

        // Bool toggled by OnServerConnect to true to determin who the host is
        private static bool canSpawn = false;

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Used to determin who the host is, as clients are unable to spawn items
            On.RoR2.Networking.NetworkManagerSystemSteam.OnServerConnect += (s, u, t) => { canSpawn = true; };

            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            LevelToItemString = Config.Bind<string>(
            "ItemToLevel",
            "Dictionary",
            "5-Feather",
            "A string representing a dictionary of the level to item formatted as \"5-Feather,15-Feather\""
            );

            // Convert the level to item config into a usable dictionary on load
            string[] pairs = LevelToItemString.Value.Split(',');
            foreach (var pair in pairs)
            {
                string[] values = pair.Split("-");
                levelToItem.Add(float.Parse(values[0]), values[1]);
            }

            // Detect player level up
            GlobalEventManager.onCharacterLevelUp += GlobalEventManager_onCharacterLevelUp;
        }

        private void GlobalEventManager_onCharacterLevelUp(CharacterBody body)
        {

            Log.Info($"My current level is {body.level}");
            if (body.isPlayerControlled && canSpawn && levelToItem.ContainsKey(body.level))
            {
                Log.Info($"Spawning {levelToItem[body.level]}");
                
                // Convert the string item into the item def so the ItemIndex can be aquired
                ItemDef id = typeof(RoR2Content.Items).GetFieldValue<ItemDef>(levelToItem[body.level]);
                ItemIndex ii = id.itemIndex;

                // Get the location of the player who leveled up
                var transform = body.transform;

                // Spawns the item on the current player position
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(ii), transform.position, transform.forward);

            }

        }

    }
}