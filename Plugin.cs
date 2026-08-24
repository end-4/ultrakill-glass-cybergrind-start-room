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
    public const string PluginVersion = "1.0.2";

    // internal static GameObject RoomPrefab;
    internal static GameObject? RoomCeilingPrefab;
    internal static GameObject? RoomWallsPrefab;
    internal static GameObject? RoomFloorPrefab;
    internal static GameObject? HallwayPrefab;
    internal static GameObject? HallwayWallsPrefab;

    private static readonly string BundlePath =
        Path.Combine(workingDir, "assets", "stained_glass_cybergrind_start_room.bundle");

    internal static GameObject? FindNestedObject(GameObject baseObject, string path) {
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
        var bundle = AssetBundle.LoadFromFile(BundlePath);
        if (bundle == null) {
            Log.LogError("Couldn't load asset bundle. Aborting.");
            return;
        }

        // RoomPrefab = bundle.LoadAsset<GameObject>("Room");
        RoomCeilingPrefab = bundle.LoadAsset<GameObject>("Ceiling");
        RoomWallsPrefab = bundle.LoadAsset<GameObject>("Walls");
        RoomFloorPrefab = bundle.LoadAsset<GameObject>("Floor");
        HallwayWallsPrefab = bundle.LoadAsset<GameObject>("HallwayWalls");
        bundle.Unload(false);
    }

    private void Awake() {
        Log = Logger;
        LoadObjects();

        // Add scene load replacement
        SceneManager.sceneLoaded += (_, _) => {
            if (SceneHelper.CurrentScene != "Endless") return;
            var firstroom = SceneManager.GetActiveScene().GetRootGameObjects()
                .FirstOrDefault(obj => obj.name == "FirstRoom");
            if (firstroom != null) {
                Transform room = FindNestedObject(firstroom, "Room/Room")!.transform;
                Transform hallway = FindNestedObject(firstroom, "Room/Hallway")!.transform;
                var firstRoomFloor = FindNestedObject(room.gameObject, "Floor");
                var firstRoomCeiling = FindNestedObject(room.gameObject, "Ceiling");
                var firstRoomWalls = FindNestedObject(room.gameObject, "Walls");
                var hallwayWalls = FindNestedObject(hallway.gameObject, "Walls");
                firstRoomFloor?.SetActive(false);
                firstRoomCeiling?.SetActive(false);
                firstRoomWalls?.SetActive(false);
                hallwayWalls?.SetActive(false);
                var frf = Instantiate(Plugin.RoomFloorPrefab, room, true);
                var frc = Instantiate(Plugin.RoomCeilingPrefab, room, true);
                var frw = Instantiate(Plugin.RoomWallsPrefab, room, true);
                var hw = Instantiate(Plugin.HallwayWallsPrefab, hallway, true);

                if (frf != null && firstRoomFloor != null) frf.transform.localPosition = firstRoomFloor.transform.localPosition;
                if (frc != null && firstRoomCeiling != null) frc.transform.localPosition = firstRoomCeiling.transform.localPosition;
                if (frw != null && firstRoomWalls != null) frw.transform.localPosition = firstRoomWalls.transform.localPosition;
                if (hw != null && hallwayWalls != null) hw.transform.localPosition = hallwayWalls.transform.localPosition;
            }
        };

        // Done
        Log.LogInfo($"{PluginName} loaded!");
    }
}
