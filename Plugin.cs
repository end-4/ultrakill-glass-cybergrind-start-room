using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlassCybergrindStartRoom;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin {
    // Logger
    internal static ManualLogSource? Log;

    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.glassCybergrindStartRoom";
    public const string PluginName = "GlassCybergrindStartRoom";
    public const string PluginVersion = "1.0.0";

    // internal static GameObject RoomPrefab;
    internal static GameObject RoomCeilingPrefab;
    internal static GameObject RoomWallsPrefab;
    internal static GameObject RoomFloorPrefab;
    internal static GameObject HallwayPrefab;
    internal static GameObject HallwayWallsPrefab;

    private static readonly string BundlePath =
        Path.Combine(workingDir, "assets", "stained_glass_cybergrind_start_room.bundle");

    internal static GameObject FindNestedObject(GameObject baseObject, string path) {
        Transform t = baseObject.transform;
        string[] pathItems = path.Split("/");
        for (int i = 0; i < pathItems.Length; i++) {
            string itemStr = pathItems[i];
            t = t.transform.Find(itemStr);
            if (t == null) {
                Log.LogWarning(itemStr + " not found for object path " + baseObject.name + "/" + path);
                return null;
            }
        }

        return t.gameObject;
    }

    private void LoadObjects() {
        AssetBundle bundle = AssetBundle.LoadFromFile(BundlePath);
        if (bundle == null) {
            Log.LogError("Couldn't load asset bundle. Aborting.");
            return;
        }

        // RoomPrefab = bundle.LoadAsset<GameObject>("Room");
        RoomCeilingPrefab = bundle.LoadAsset<GameObject>("Ceiling");
        RoomWallsPrefab = bundle.LoadAsset<GameObject>("Walls");
        RoomFloorPrefab = bundle.LoadAsset<GameObject>("Floor");
        HallwayWallsPrefab = bundle.LoadAsset<GameObject>("HallwayWalls");
        // bundle.Unload(false); // ?
    }

    private void Awake() {
        Log = Logger;
        LoadObjects();

        // Add scene load replacement
        SceneManager.sceneLoaded += (_, _) => {
            if (SceneHelper.CurrentScene != "Endless") return;
            GameObject firstroom = SceneManager.GetActiveScene().GetRootGameObjects()
                .FirstOrDefault(obj => obj.name == "FirstRoom");
            if (firstroom) {
                Transform room = Plugin.FindNestedObject(firstroom, "Room/Room").transform;
                Transform hallway = Plugin.FindNestedObject(firstroom, "Room/Hallway").transform;
                GameObject firstRoomFloor = Plugin.FindNestedObject(room.gameObject, "Floor");
                GameObject firstRoomCeiling = Plugin.FindNestedObject(room.gameObject, "Ceiling");
                GameObject firstRoomWalls = Plugin.FindNestedObject(room.gameObject, "Walls");
                GameObject hallwayWalls = Plugin.FindNestedObject(hallway.gameObject, "Walls");
                firstRoomFloor.SetActive(false);
                firstRoomCeiling.SetActive(false);
                firstRoomWalls.SetActive(false);
                hallwayWalls.SetActive(false);
                GameObject frf = Object.Instantiate(Plugin.RoomFloorPrefab, room, true);
                GameObject frc = Object.Instantiate(Plugin.RoomCeilingPrefab, room, true);
                GameObject frw = Object.Instantiate(Plugin.RoomWallsPrefab, room, true);
                GameObject hw = Object.Instantiate(Plugin.HallwayWallsPrefab, hallway, true);

                frf.transform.localPosition = firstRoomFloor.transform.localPosition;
                frc.transform.localPosition = firstRoomCeiling.transform.localPosition;
                frw.transform.localPosition = firstRoomWalls.transform.localPosition;
                hw.transform.localPosition = hallwayWalls.transform.localPosition;
            }
        };

        // Done
        Log.LogInfo($"{PluginName} loaded!");
    }
}
