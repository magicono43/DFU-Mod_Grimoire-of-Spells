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
    public class TransmuteBody : CurseEffect
    {
        private static readonly string effectKey = "Transmute-Body";

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = true;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;

            curseChecks[0] = true; // Strength
            curseChecks[3] = true; // Agility
            curseChecks[4] = true; // Endurance
            curseChecks[6] = true; // Speed
        }

        #region Text

        public override string GroupName => "Transmute"; // Tomorrow, work on the text part to explain how this effect works, then basically copy/paste the others and possibly do testing afterward.
        public override string SubGroupName => "Body";
        const string effectDescription = "Purifies target of most afflictions and magical effects.";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                effectDescription,
                "Duration: Instantaneous.",
                "Chance: % Chance purification will succeed.",
                "Magnitude: N/A");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Duration: Instantaneous.",
                "Chance: %bch + %ach per %clc level(s)",
                "Magnitude: N/A",
                effectDescription);
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