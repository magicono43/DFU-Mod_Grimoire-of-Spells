using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;

namespace GrimoireofSpells
{
    public class TransmuteLife : BaseEntityEffect
    {
        private static readonly string effectKey = "Transmute-Life";

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;
        }

        #region Text

        public override string GroupName => "Transmute Life";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Drains the caster's life force, in exchange for magicka points.",
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
                "Drains the caster's life force, in exchange for magicka points.");
        }

        #endregion

        public override void MagicRound() // Will potentially change this to an incumbent effect later to reduce maximum health and such, but for now just do this simple implementation.
        {
            base.MagicRound();

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            int minHPDrained = (int)Mathf.Ceil(entityBehaviour.Entity.MaxHealth * 0.10f); // 10% of max HP
            int maxHPDrained = (int)Mathf.Ceil(entityBehaviour.Entity.MaxHealth * 0.20f); // 20% of max HP
            int drainedHP = Random.Range(minHPDrained, maxHPDrained + 1);
            int manaRestored = (int)Mathf.Ceil(drainedHP * 2f); // Values will likely be heavily changed in the future, just place-holder for now.

            // Drain health
            entityBehaviour.Entity.SetHealth(entityBehaviour.Entity.CurrentHealth - drainedHP); // Using SetHealth because "DecreaseHealth" goes through magical shields first, which I don't want here.

            // Restore magic points
            entityBehaviour.Entity.IncreaseMagicka(manaRestored);

            UnityEngine.Debug.LogFormat("{0} drained {1}'s health by {2} points, but in return restored {3} magicka points.", Key, entityBehaviour.EntityType.ToString(), drainedHP, manaRestored);
        }
    }
}