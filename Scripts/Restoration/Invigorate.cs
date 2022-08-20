using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;

namespace GrimoireofSpells
{
    public class Invigorate : IncumbentEffect
    {
        public static readonly string effectKey = "Invigorate";

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_All;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker; // Will probably add potion maker as well later, but for now just spells probably.
            properties.MagicSkill = DFCareer.MagicSkills.Restoration;
            properties.DisableReflectiveEnumeration = true;
            properties.SupportDuration = true;
            properties.SupportMagnitude = true;
            properties.DurationCosts = MakeEffectCosts(75, 15); // These values will have to be adjusted heavily, basically just placeholder for now.
            properties.MagnitudeCosts = MakeEffectCosts(6, 6);
        }

        #region Text

        public override string GroupName => "Invigorate"; // Also remember to add potion effects for these later.
        const string effectDescription = "Target regenerates Fatigue Points each round.";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                "Invigorate",
                effectDescription,
                "Duration: Rounds target regenerates.",
                "Chance: N/A",
                "Magnitude: Number of points regenerated each round.");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                "Invigoration",
                "Duration: %bdr + %adr per %cld level(s)",
                "Chance: N/A",
                "Magnitude: %1bm - %2bm + %1am - %2am per %clm level(s)",
                effectDescription);
        }

        #endregion

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            // Output "You feel invigorated." if the host manager is player
            if (manager.EntityBehaviour == GameManager.Instance.PlayerEntityBehaviour)
            {
                DaggerfallUI.AddHUDText("You feel invigorated.", 1.5f);
            }
        }

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            return (other is Invigorate);
        }

        protected override void AddState(IncumbentEffect incumbent)
        {
            // Stack my rounds onto incumbent
            incumbent.RoundsRemaining += RoundsRemaining;
        }

        public override void MagicRound()
        {
            base.MagicRound();

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Increase target fatigue
            entityBehaviour.Entity.IncreaseFatigue(GetMagnitude(caster), true);
        }
    }
}