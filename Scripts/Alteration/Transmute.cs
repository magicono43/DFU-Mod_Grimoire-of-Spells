using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using System.Collections.Generic;

namespace GrimoireofSpells
{
    public class Transmute : CurseEffect
    {
        public const int totalVariants = 3;
        public static readonly string effectKey = "Transmute";
        public static string[] subGroupKeys = { "Body", "Mind", "Soul" };

        static bool[] body = new bool[] { true, false, false, true, true, false, true, false };
        static bool[] mind = new bool[] { false, true, false, false, false, true, false, false };
        static bool[] soul = new bool[] { false, false, true, false, false, false, false, true };

        bool[][] attributeVariants = new bool[][] { body, mind, soul };

        VariantProperties[] variantProperties = new VariantProperties[totalVariants];

        #region Structs & Enums

        // Variant can be stored internally with any format
        // Using a struct here with properties for to effect
        struct VariantProperties
        {
            public bool[] attributeVariant;
            public EffectProperties effectProperties;
        }

        // A friendly name for each variant - could also just reference by index 0-4
        enum VariantTypes
        {
            Body,
            Mind,
            Soul,
        }

        #endregion

        #region Properties

        // Must override Properties to return correct properties for any variant
        // The currentVariant value is set by magic framework - each variant gets enumerated to its own effect template
        public override EffectProperties Properties
        {
            get { return variantProperties[currentVariant].effectProperties; }
        }

        #endregion

        public override void SetProperties()
        {
            // Set properties shared by all variants
            properties.Key = effectKey;
            properties.ShowSpellIcon = true;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;

            // Set variant count so framework knows how many to extract
            variantCount = totalVariants;

            // Set properties unique to each variant
            SetVariantProperties(VariantTypes.Body);
            SetVariantProperties(VariantTypes.Mind);
            SetVariantProperties(VariantTypes.Soul);
        }

        #region Variants

        void SetVariantProperties(VariantTypes variant)
        {
            int variantIndex = (int)variant;
            VariantProperties vp = new VariantProperties();
            vp.attributeVariant = attributeVariants[variantIndex];
            vp.effectProperties = properties;
            vp.effectProperties.Key = string.Format("{0}-{1}", effectKey, subGroupKeys[variantIndex]);
            variantProperties[variantIndex] = vp;
        }

        #endregion

        #region Text

        const string groupName = "Transmute";
        string[] subGroupNames = { "Body", "Mind", "Soul" };
        string[] attributeNameList = { "Strength, Agility, Endurance, and Speed,", "Intelligence and Personality,", "Willpower and Luck," };

        public override string GroupName => groupName;
        public override string SubGroupName => subGroupNames[currentVariant];
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription() // Will definitely want to change the descriptions after testing and such, place-holder for now.
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName + " " + SubGroupName,
                "Curses the caster's " + attributeNameList[currentVariant],
                "in exchange for magicka points.",
                "Curses are a more difficult to remove 'drained' effect.",
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: Unpredictable.");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName + " " + SubGroupName,
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: Unpredictable.",
                "Curses the caster's " + attributeNameList[currentVariant],
                "in exchange for magicka points.",
                "Curses are a more difficult to remove 'drained' effect.");
        }

        #endregion

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Attempt to determine points to restore based on amount of total points "cursed" by the effect, will need to do testing to ensure "lastMagnitudeIncreaseAmount" is accurate here.
            int magnitude = (int)Mathf.Ceil(lastMagnitudeIncreaseAmount * 4f * 7.5f); // Values will likely be heavily changed in the future, just place-holder for now.

            // Restore magic points
            entityBehaviour.Entity.IncreaseMagicka(magnitude);

            UnityEngine.Debug.LogFormat("{0} restored {1}'s magicka by {2} points", Key, entityBehaviour.EntityType.ToString(), magnitude);
        }
    }
}