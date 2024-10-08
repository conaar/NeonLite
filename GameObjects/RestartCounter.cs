﻿using Steamworks;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace NeonLite.GameObjects
{
    internal class RestartCounter : MonoBehaviour
    {
        public static Dictionary<string, int> LevelRestarts { get; private set; }
        public static int CurrentRestarts { get; private set; }
        private static string _currentLevel;
        private static string _path;
        private static readonly string _filename = "restartcounter.json";
        private static bool run = false;

        private readonly Rect _rectTotal = new(10, 40, 100, 70);
        private readonly Rect _rectSession = new(10, 60, 100, 70);
        private readonly GUIStyle _style = new()
        {
            font = Resources.Load("fonts/nova_mono/novamono") as Font,
            fontSize = 20
        };

        public static void Initialize()
        {
            run = SteamManager.Initialized;
            if (!run || (LevelRush.IsLevelRush() && LevelRush.IsHellRush())) return;
            _path = Application.persistentDataPath + "/" + SteamUser.GetSteamID().m_SteamID.ToString() + "/NeonLite/";


            if (LevelRestarts == null)
            {
                if (transferRestarts())
                {
                    if (!File.Exists(_path + _filename))
                    {
                        LevelRestarts = new();
                        RessourcesUtils.SaveToFile<Dictionary<string, int>>(_path, _filename, LevelRestarts);
                    }
                    else
                        LevelRestarts = RessourcesUtils.ReadFile<Dictionary<string, int>>(_path, _filename);
                }
            }
            new GameObject("RestartCounter").AddComponent<RestartCounter>();
        }

        private void Start()
        {
            if (!run) Destroy(this);

            _style.normal.textColor = Color.white;
            string newLevel = NeonLite.Game.GetCurrentLevel().levelID;

            if (_currentLevel != newLevel)
            {
                _currentLevel = newLevel;
                CurrentRestarts = 0;
            }
            CurrentRestarts++;

            if (LevelRestarts.ContainsKey(_currentLevel))
                LevelRestarts[_currentLevel] = LevelRestarts[_currentLevel] + 1;
            else
                LevelRestarts[_currentLevel] = 1;

            RessourcesUtils.SaveToFile<Dictionary<string, int>>(_path, _filename, LevelRestarts);
        }

        private void OnGUI()
        {
            if (NeonLite.s_Setting_RestartsTotal.Value)
                GUI.Label(_rectTotal, "Total Attempts: " + LevelRestarts[_currentLevel], _style);
            if (NeonLite.s_Setting_RestartsSession.Value)
                GUI.Label(_rectSession, "Attempts: " + CurrentRestarts, _style);
        }

        [Obsolete]
        private static bool transferRestarts()
        {
            string basePath = Application.persistentDataPath + "/" + SteamUser.GetSteamID().m_SteamID.ToString();
            string newDirectory = basePath + "/NeonLite";

            if (!File.Exists(basePath + "/restartcounter.txt")) return true;
            Debug.Log("Old restarts file detected! Moving it");

            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);

            StreamReader streamReader = new(basePath + "/restartcounter.txt", Encoding.UTF8);
            string data = streamReader.ReadToEnd();
            DataContractJsonSerializer deserializer = new(typeof(Dictionary<string, int>));
            MemoryStream stream = new(Encoding.UTF8.GetBytes(data));
            LevelRestarts = (Dictionary<string, int>)deserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data)));
            stream.Close();
            streamReader.Close();
            RessourcesUtils.SaveToFile<Dictionary<string, int>>(_path, _filename, LevelRestarts);
            File.Delete(basePath + "/restartcounter.txt");
            return false;
        }
    }
}
