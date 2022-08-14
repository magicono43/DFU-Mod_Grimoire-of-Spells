using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;

namespace GrimoireofSpells
{
    public class Invigorate : IncumbentEffect
    {
        private static readonly string effectKey = "Invigorate";

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.CasterOnly | TargetTypes.ByTouch; // This might not work, but will have to see.
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker; // Will probably add potion maker as well later, but for now just spells probably.
            properties.MagicSkill = DFCareer.MagicSkills.Restoration;
            properties.DisableReflectiveEnumeration = true;
            properties.SupportChance = true;
            properties.ChanceCosts = MakeEffectCosts(8, 100, 200); // These values will have to be adjusted heavily, basically just placeholder for now.
        }

        #region Text

        public override string GroupName => "Invigorate"; // Also remember to add potion effects for these later, this one will possibly change the vanilla purification potions if possible.
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

        public override void MagicRound() // Will have to decide later if I should also heal all attributes as well or not, might make another effect or something for that.
        {
            base.MagicRound();

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Implement effect
            manager.CureAllPoisons();
            manager.CureAllDiseases();
            manager.EndIncumbentEffect<Paralyze>();
            manager.ClearSpellBundles();

            Debug.LogFormat("Purified entity of all poisons, diseases, paralysis, and magic effects");
        }

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            throw new System.NotImplementedException();
        }

        protected override void AddState(IncumbentEffect incumbent)
        {
            throw new System.NotImplementedException();
        }
    }
}