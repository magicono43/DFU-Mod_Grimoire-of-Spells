// Project:         GrimoireofSpells mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.21
// Created On: 	    8/9/2022, 11:00 PM
// Last Edit:		8/13/2022, 11:00 PM
// Modifier:
// Special Thanks:  DunnyOfPenwick, Kab the Bird Ranger, Hazelnut, Interkarma

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Serialization;
using System;
using DaggerfallWorkshop.Game.MagicAndEffects;
using static DaggerfallWorkshop.Game.MagicAndEffects.EntityEffectBroker;

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

            RegisterSpells();

            RegisterTransmuteSpells();

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.CharacterSheet, typeof(VSPCharacterSheetOverride));
            Debug.Log("GrimoireofSpells Registered Override For DaggerfallCharacterSheetWindow");

            Debug.Log("Finished mod init: Grimoire of Spells");
        }

        private void RegisterSpells()
        {
            EntityEffectBroker effectBroker = GameManager.Instance.EntityEffectBroker;

            Purify purifyTemplateEffect = new Purify();
            effectBroker.RegisterEffectTemplate(purifyTemplateEffect);

            Invigorate invigorateTemplateEffect = new Invigorate();
            effectBroker.RegisterEffectTemplate(invigorateTemplateEffect);

            ThePenwickPapers.Seeking effect = new ThePenwickPapers.Seeking();
            //effect.SetCustomName(ThePenwickPapers.Text.SeekingPotionName.Get());
            effectBroker.RegisterEffectTemplate(effect, true);
            PotionRecipe recipe = effectBroker.GetEffectPotionRecipe(effect);
            //potionOfSeekingRecipeKey = recipe.GetHashCode();
        }

        private void RegisterTransmuteSpells() // I will want to consolidate this registration code more later if possible, atm kind of messy.
        {
            EntityEffectBroker effectBroker = GameManager.Instance.EntityEffectBroker;

            // Register Transmute Body, Mind, and Soul Effects
            for (int i = 0; i < Transmute.totalVariants; i++)
            {
                Transmute transmuteTemplateEffect = new Transmute();
                transmuteTemplateEffect.CurrentVariant = i;
                effectBroker.RegisterEffectTemplate(transmuteTemplateEffect);

                EffectEntry transmuteEffectEntry = new EffectEntry()
                {
                    Key = transmuteTemplateEffect.Properties.Key,
                };

                EffectBundleSettings transmuteVariantSpell = new EffectBundleSettings()
                {
                    Version = CurrentSpellVersion,
                    BundleType = BundleTypes.Spell,
                    TargetType = TargetTypes.CasterOnly,
                    ElementType = ElementTypes.Magic,
                    Name = "Transmute " + Transmute.subGroupKeys[i],
                    IconIndex = 10 + i, // Change this to a custom spell icon later, most likely.
                    MinimumCastingCost = true,
                    Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                    Effects = new EffectEntry[] { transmuteEffectEntry },
                };

                CustomSpellBundleOffer transmuteVariantOffer = new CustomSpellBundleOffer()
                {
                    Key = "Transmute" + Transmute.subGroupKeys[i] + "-CustomOffer",
                    Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                    BundleSetttings = transmuteVariantSpell,
                };

                effectBroker.RegisterCustomSpellBundleOffer(transmuteVariantOffer);
            }

            // Register Transmute Life
            TransmuteLife transmuteLifeTemplateEffect = new TransmuteLife();
            effectBroker.RegisterEffectTemplate(transmuteLifeTemplateEffect);

            EffectEntry transmuteLifeEffectEntry = new EffectEntry()
            {
                Key = transmuteLifeTemplateEffect.Properties.Key,
            };

            EffectBundleSettings transmuteLifeSpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Life",
                IconIndex = 16, // Change this to a custom spell icon later, most likely.
                MinimumCastingCost = true,
                Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                Effects = new EffectEntry[] { transmuteLifeEffectEntry },
            };

            CustomSpellBundleOffer transmuteLifeOffer = new CustomSpellBundleOffer()
            {
                Key = "TransmuteLife-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = transmuteLifeSpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(transmuteLifeOffer);

            // Register Transmute Energy
            TransmuteEnergy transmuteEnergyTemplateEffect = new TransmuteEnergy();
            effectBroker.RegisterEffectTemplate(transmuteEnergyTemplateEffect);

            EffectEntry transmuteEnergyEffectEntry = new EffectEntry()
            {
                Key = transmuteEnergyTemplateEffect.Properties.Key,
            };

            EffectBundleSettings transmuteEnergySpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Energy",
                IconIndex = 17, // Change this to a custom spell icon later, most likely.
                MinimumCastingCost = true,
                Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                Effects = new EffectEntry[] { transmuteEnergyEffectEntry },
            };

            CustomSpellBundleOffer transmuteEnergyOffer = new CustomSpellBundleOffer()
            {
                Key = "TransmuteEnergy-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = transmuteLifeSpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(transmuteEnergyOffer);
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
            if (SaveLoadManager.Instance.LoadInProgress)
                return;

            if (GameManager.IsGamePaused)
                return;

            FixedUpdateCounter++; // Increments the FixedUpdateCounter by 1 every FixedUpdate.

            if (FixedUpdateCounter >= 50 * 1) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                FixedUpdateCounter = 0;


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