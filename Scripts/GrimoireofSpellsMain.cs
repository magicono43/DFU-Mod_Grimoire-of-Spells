// Project:         GrimoireofSpells mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.21
// Created On: 	    8/9/2022, 11:00 PM
// Last Edit:		8/9/2022, 11:00 PM
// Modifier:
// Special Thanks:  DunnyOfPenwick, Kab the Bird Ranger, Hazelnut, Interkarma

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Serialization;
using System;

namespace GrimoireofSpells
{
    public class GrimoireofSpellsMain : MonoBehaviour, IHasModSaveData
    {
        static GrimoireofSpellsMain instance;

        public static GrimoireofSpellsMain Instance
        {
            get { return instance ?? (instance = FindObjectOfType<GrimoireofSpellsMain>()); }
        }

        static Mod mod;

        // Options
        public static int ProgressDisplayType { get; set; }
        public static bool GovernAttributeText { get; set; }

        // Attached To SaveData
        public static byte[] notifiedSkillsList = new byte[35];

        // Global Variables
        public static int FixedUpdateCounter { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            instance = new GameObject("GrimoireofSpells").AddComponent<GrimoireofSpellsMain>(); // Add script to the scene.
            mod.SaveDataInterface = instance;

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Grimoire of Spells");

            mod.LoadSettings();

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.CharacterSheet, typeof(VSPCharacterSheetOverride));
            Debug.Log("GrimoireofSpells Registered Override For DaggerfallCharacterSheetWindow");

            Debug.Log("Finished mod init: Grimoire of Spells");
        }

        #region Settings

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            ProgressDisplayType = mod.GetSettings().GetValue<int>("GeneralSettings", "DisplayType");
            GovernAttributeText = mod.GetSettings().GetValue<bool>("GeneralSettings", "ShowGovAttributeText");
        }

        #endregion

        private void FixedUpdate()
        {
            if (!SkillReadyNotifications)
                return;

            if (SaveLoadManager.Instance.LoadInProgress)
                return;

            if (GameManager.IsGamePaused)
                return;

            FixedUpdateCounter++; // Increments the FixedUpdateCounter by 1 every FixedUpdate.

            if (FixedUpdateCounter >= 50 * NotificationCheckFrequency) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                FixedUpdateCounter = 0;

                if (playerSkills.Count > 0)
                {
                    for (int i = 0; i < playerSkills.Count; i++)
                    {
                        float cT = VSPCharacterSheetOverride.CurrentTallyCount(playerSkills[i]);
                        float aT = VSPCharacterSheetOverride.TallysNeededToAdvance(playerSkills[i]);
                        if (cT > aT)
                            cT = aT;

                        if (notifiedSkillsList[i] == 1 && cT < aT)
                            notifiedSkillsList[i] = 0;

                        if (notifiedSkillsList[i] == 0 && cT >= aT)
                        {
                            notifiedSkillsList[i] = 1;

                            NotifyPlayer(playerSkills[i]);
                        }
                    }
                }
            }
        }

        #region SaveData Junk

        public Type SaveDataType
        {
            get { return typeof(GrimoireofSpellsSaveData); }
        }

        public object NewSaveData()
        {
            return new GrimoireofSpellsSaveData
            {
                NotifiedSkillsList = new byte[35]
            };
        }

        public object GetSaveData()
        {
            return new GrimoireofSpellsSaveData
            {
                NotifiedSkillsList = notifiedSkillsList
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var GrimoireofSpellsSaveData = (GrimoireofSpellsSaveData)saveData;
            notifiedSkillsList = GrimoireofSpellsSaveData.NotifiedSkillsList;
        }
    }

    [FullSerializer.fsObject("v1")]
    public class GrimoireofSpellsSaveData
    {
        public byte[] NotifiedSkillsList;
    }

    #endregion

}