// Project:         GrimoireofSpells mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.21
// Created On: 	    8/9/2022, 11:00 PM
// Last Edit:		8/22/2022, 9:00 PM
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

            RegisterDisplacementSpells();

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

        private void RegisterDisplacementSpells()
        {
            EntityEffectBroker effectBroker = GameManager.Instance.EntityEffectBroker;

            // Register Blink
            Blink BlinkTemplateEffect = new Blink();
            effectBroker.RegisterEffectTemplate(BlinkTemplateEffect);

            EffectSettings BlinkEffectSettings = new EffectSettings() // Just testing values for now.
            {
                MagnitudeBaseMin = 5,
                MagnitudeBaseMax = 5,
                MagnitudePlusMin = 0,
                MagnitudePlusMax = 0,
                MagnitudePerLevel = 1,
            };

            EffectEntry BlinkEffectEntry = new EffectEntry()
            {
                Key = BlinkTemplateEffect.Properties.Key,
                Settings = BlinkEffectSettings,
            };

            EffectBundleSettings BlinkSpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Blink",
                IconIndex = 12, // Change this to a custom spell icon later, most likely.
                Effects = new EffectEntry[] { BlinkEffectEntry },
            };

            CustomSpellBundleOffer BlinkOffer = new CustomSpellBundleOffer()
            {
                Key = "Blink-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale, // Will add more places this can be found later, such as enchantments and such.
                BundleSetttings = BlinkSpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(BlinkOffer);

            // Register Dislocate
            Dislocate DislocateTemplateEffect = new Dislocate();
            effectBroker.RegisterEffectTemplate(DislocateTemplateEffect);

            EffectEntry DislocateEffectEntry = new EffectEntry()
            {
                Key = DislocateTemplateEffect.Properties.Key,
            };

            EffectBundleSettings DislocateSpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.SingleTargetAtRange,
                ElementType = ElementTypes.Magic,
                Name = "Dislocate",
                IconIndex = 13, // Change this to a custom spell icon later, most likely.
                Effects = new EffectEntry[] { DislocateEffectEntry },
            };

            CustomSpellBundleOffer DislocateOffer = new CustomSpellBundleOffer()
            {
                Key = "Dislocate-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale, // Will add more places this can be found later, such as enchantments and such.
                BundleSetttings = DislocateSpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(DislocateOffer);
        }

        private void RegisterTransmuteSpells() // I will want to consolidate this registration code more later if possible, atm kind of messy.
        {
            EntityEffectBroker effectBroker = GameManager.Instance.EntityEffectBroker;

            // Important Notes: 8/21/2022, 5:15 PM:
            // So for the magnitudes not going down quickly enough issue. It's due to a parameter from a earlier calling method in the EntityEffectManager returning a modified version of a number.
            // I can't think of anyway to get around this without changing that method unfortunately, but I know the issue there at least.
            // so what I'm going to do is leave that for now and deal with it later when I inevitably have to try and put some PRs out to fix these little issues here and there.
            // Such as for this above mentioned issue and the lycanthropy tag work-around one and such.
            // For the one mentioned above, goto "EntityEffectManager.cs" the "HealAttribute" method around line 695-730.
            // Something will have to be changed in this method so the unaltered "amount" value can be given to the calling methods that use this raw value, such as I need in this case.
            // Not sure exactly how I'll do it, but will hopefully figure out a good method that can be merged into the code-base without issues when I need it. Same with the 0 cost lycanthropy tag.
            // Also, Trasmute Energy probably need to have it's return taken down a bit, but I'll see.
            // I'll still have to do some testing when all is said and done to 100% make sure this works as intended, but for now it's good enough and will finish later on for polish stuff.
            // Such as making sure the mana returned is the expected amounts and such, as well as implementing the other methods to cure the curses and such that don't currently exist, etc.

            // Register Transmute Body
            TransmuteBody TransmuteBodyTemplateEffect = new TransmuteBody();
            effectBroker.RegisterEffectTemplate(TransmuteBodyTemplateEffect);

            EffectEntry TransmuteBodyEffectEntry = new EffectEntry()
            {
                Key = TransmuteBodyTemplateEffect.Properties.Key,
            };

            EffectBundleSettings TransmuteBodySpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Potion, // This is an attempted work around to disallow "dispel magic" effects from removing these "curses".
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Body",
                IconIndex = 3, // Change this to a custom spell icon later, most likely.
                MinimumCastingCost = true,
                Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                Effects = new EffectEntry[] { TransmuteBodyEffectEntry },
            };

            CustomSpellBundleOffer TransmuteBodyOffer = new CustomSpellBundleOffer()
            {
                Key = "TransmuteBody-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = TransmuteBodySpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(TransmuteBodyOffer);

            // Register Transmute Mind
            TransmuteMind TransmuteMindTemplateEffect = new TransmuteMind();
            effectBroker.RegisterEffectTemplate(TransmuteMindTemplateEffect);

            EffectEntry TransmuteMindEffectEntry = new EffectEntry()
            {
                Key = TransmuteMindTemplateEffect.Properties.Key,
            };

            EffectBundleSettings TransmuteMindSpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Potion, // This is an attempted work around to disallow "dispel magic" effects from removing these "curses".
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Mind",
                IconIndex = 4, // Change this to a custom spell icon later, most likely.
                MinimumCastingCost = true,
                Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                Effects = new EffectEntry[] { TransmuteMindEffectEntry },
            };

            CustomSpellBundleOffer TransmuteMindOffer = new CustomSpellBundleOffer()
            {
                Key = "TransmuteMind-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = TransmuteMindSpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(TransmuteMindOffer);

            // Register Transmute Soul
            TransmuteSoul TransmuteSoulTemplateEffect = new TransmuteSoul();
            effectBroker.RegisterEffectTemplate(TransmuteSoulTemplateEffect);

            EffectEntry TransmuteSoulEffectEntry = new EffectEntry()
            {
                Key = TransmuteSoulTemplateEffect.Properties.Key,
            };

            EffectBundleSettings TransmuteSoulSpell = new EffectBundleSettings()
            {
                Version = CurrentSpellVersion,
                BundleType = BundleTypes.Potion, // This is an attempted work around to disallow "dispel magic" effects from removing these "curses".
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Soul",
                IconIndex = 5, // Change this to a custom spell icon later, most likely.
                MinimumCastingCost = true,
                Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                Effects = new EffectEntry[] { TransmuteSoulEffectEntry },
            };

            CustomSpellBundleOffer TransmuteSoulOffer = new CustomSpellBundleOffer()
            {
                Key = "TransmuteSoul-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = TransmuteSoulSpell,
            };

            effectBroker.RegisterCustomSpellBundleOffer(TransmuteSoulOffer);

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
                BundleType = BundleTypes.Potion, // This is an attempted work around to disallow "dispel magic" effects from removing these "curses".
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Life",
                IconIndex = 6, // Change this to a custom spell icon later, most likely.
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
                BundleType = BundleTypes.Potion, // This is an attempted work around to disallow "dispel magic" effects from removing these "curses".
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Transmute Energy",
                IconIndex = 7, // Change this to a custom spell icon later, most likely.
                MinimumCastingCost = true,
                Tag = "lycanthrope", // I'll probably have to do another work-around method for this to try and make it 0 mana cost, this will be removed if Lycanthropy is cured and other stuff.
                Effects = new EffectEntry[] { transmuteEnergyEffectEntry },
            };

            CustomSpellBundleOffer transmuteEnergyOffer = new CustomSpellBundleOffer()
            {
                Key = "TransmuteEnergy-CustomOffer",
                Usage = CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = transmuteEnergySpell,
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