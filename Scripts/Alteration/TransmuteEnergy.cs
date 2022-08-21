using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;

namespace GrimoireofSpells
{
    public class TransmuteEnergy : BaseEntityEffect
    {
        private static readonly string effectKey = "Transmute-Energy";

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;
        }

        #region Text

        public override string GroupName => "Transmute Energy";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Saps the caster's energy, in exchange for magicka points.",
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: Unpredictable.");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: Unpredictable.",
                "Saps the caster's energy, in exchange for magicka points.");
        }

        #endregion

        public override void MagicRound() // Will potentially change this to an incumbent effect later to reduce maximum fatigue and such, but for now just do this simple implementation.
        {
            base.MagicRound();

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            int minFatigueDrained = (int)Mathf.Ceil(entityBehaviour.Entity.MaxFatigue * 0.10f); // 10% of max Fatigue
            int maxFatigueDrained = (int)Mathf.Ceil(entityBehaviour.Entity.MaxFatigue * 0.20f); // 20% of max Fatigue
            int drainedFatigue = Random.Range(minFatigueDrained, maxFatigueDrained + 1);
            int manaRestored = (int)Mathf.Ceil(drainedFatigue * 1f); // Values will likely be heavily changed in the future, just place-holder for now.

            // Drain fatigue
            entityBehaviour.Entity.DecreaseFatigue(drainedFatigue, true); // Need to do more testing for this, seems like too much being drained in one cast. Remove "True" later for testing to see if issue.

            // Restore magic points
            entityBehaviour.Entity.IncreaseMagicka(manaRestored);

            UnityEngine.Debug.LogFormat("{0} drained {1}'s fatigue by {2} points, but in return restored {3} magicka points.", Key, entityBehaviour.EntityType.ToString(), drainedFatigue, manaRestored);
        }
    }
}