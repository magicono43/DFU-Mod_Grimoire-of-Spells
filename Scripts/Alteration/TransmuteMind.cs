using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using FullSerializer;
using DaggerfallWorkshop.Game;

namespace GrimoireofSpells
{
    /// <summary>
    /// Curse effect base. CHANGE THIS AFTERWARD TO DESCRIBE IT BETTER, PLACEHOLDER FOR NOW.
    /// Provides functionality common to all Curse effects which vary only by properties and stat.
    /// This effect uses an incumbent pattern where future applications of same effect type
    /// will only add to total magnitude of first effect of this type.
    /// Incumbent curse effect persists indefinitely until player heals stat enough for magnitude to reach 0.
    /// </summary>
    public class TransmuteMind : IncumbentEffect
    {
        public static readonly string effectKey = "Transmute-Mind";

        // Int, Per
        protected int[] magStats = { 0, 0, 0, 0, 0, 0, 0, 0 };
        protected bool[] curseChecks = { false, true, false, false, false, true, false, false };
        protected int lastMagnitudeIncreaseAmount = 0;
        int forcedRoundsRemaining = 3;

        public int[] Magnitudes
        {
            get { return magStats; }
        }

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = true;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;
        }

        #region Text

        public override string GroupName => "Transmute Mind";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription() // Will definitely want to change the descriptions after testing and such, place-holder for now.
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Curses the caster's Intelligence and Personality,",
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
                GroupName,
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: Unpredictable.",
                "Curses the caster's Intelligence and Personality,",
                "in exchange for magicka points.",
                "Curses are a more difficult to remove 'drained' effect.");
        }

        #endregion

        // Curse effects are permanent until healed so we manage our own lifecycle
        protected override int RemoveRound()
        {
            return forcedRoundsRemaining;
        }

        // Always present at least one round remaining so effect system does not remove, 3 in this case so the icon stays solid instead of constantly blinking
        public override int RoundsRemaining
        {
            get { return forcedRoundsRemaining; }
        }

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

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            return other is TransmuteMind;
        }

        protected override void BecomeIncumbent()
        {
            lastMagnitudeIncreaseAmount = GetMagnitude();
            if (lastMagnitudeIncreaseAmount > 0)
            {
                IncreaseMagnitude(lastMagnitudeIncreaseAmount);
                ShowPlayerCursed();
            }
        }

        protected override void AddState(IncumbentEffect incumbent)
        {
            if (forcedRoundsRemaining == 0)
                return;

            lastMagnitudeIncreaseAmount = GetMagnitude();
            if (lastMagnitudeIncreaseAmount > 0)
            {
                (incumbent as TransmuteMind).IncreaseMagnitude(lastMagnitudeIncreaseAmount);
                ShowPlayerCursed();
            }
        }

        protected int GetMagnitude()
        {
            return Random.Range(2, 9);
        }

        public override void HealAttributeDamage(DFCareer.Stats stat, int amount)
        {
            // Can only heal incumbent matching curse
            if (!IsIncumbent)
                return;

            if (!(stat == DFCareer.Stats.Intelligence || stat == DFCareer.Stats.Personality))
                return;

            int magnitude = magStats[(int)stat];

            int beforeStatHeal = (int)Mathf.Ceil(magnitude / 30f);
            int afterStatHeal = (int)Mathf.Ceil((magnitude / 30f) - (amount / 30f));
            int statHealDiff = beforeStatHeal - afterStatHeal;

            if (magnitude <= amount)
            {
                // Heal attribute fully
                base.HealAttributeDamage(stat, (int)Mathf.Ceil(magnitude / 30f));
            }
            else
            {
                // Heal attribute based on remaining magnitude of curse
                base.HealAttributeDamage(stat, Mathf.Abs(statHealDiff));
            }

            // Reduce magnitude and cancel effect only once all other cursed stats for this effect are also reduced to 0
            if (DecreaseMagnitude(amount, stat) == 0)
            {
                if (manager.EntityBehaviour == GameManager.Instance.PlayerEntityBehaviour)
                    DaggerfallUI.AddHUDText("The curse on your " + stat.ToString() + " is lifted.", 1.5f); // The "ToString" thing might not work on an enum value, but will have to see.

                // When all cursed stats from this effect have had their respective magnitudes reduced to 0, cancel this entire effect. 
                if (magStats[(int)DFCareer.Stats.Intelligence] == 0 && magStats[(int)DFCareer.Stats.Personality] == 0)
                    forcedRoundsRemaining = 0;
            }
        }

        public override void CureAttributeDamage()
        {
            // Eventually add some alternate methods to remove curse effects, will have to see if this all works at all first anyway, etc.
            // Also need a way to prevent just using dispel magic effect to remove a curse, maybe could make the effect a potion as an "easy" work-around for that?

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInside)
            {
                // Hopefully temporary hacky method to disallow attributes to be restored by curse effects from the temple "heal attribute" free service thing.
                if (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.Temple)
                {
                    DaggerfallUI.AddHUDText("A curse cannot be lifted this way.", 1.5f);
                    return;
                }
            }

            base.CureAttributeDamage();
        }

        void ShowPlayerCursed()
        {
            // Output "You have been cursed." if the host manager is player
            if (manager.EntityBehaviour == GameManager.Instance.PlayerEntityBehaviour)
                DaggerfallUI.AddHUDText("You have been cursed.");
        }

        public void IncreaseMagnitude(int amount)
        {
            // Allow magnitude to reduce stat below 1. So stats reduced by a "curse" effect CAN kill you, unlike the drain effect which is stopped at 1.
            magStats[(int)DFCareer.Stats.Intelligence] += amount * 30;
            magStats[(int)DFCareer.Stats.Personality] += amount * 30;

            // Thinking about making curses attribute damage be 10-30x more "sticky" than normal stat drain effects. So requires either much more "heal" or some other treatment/service to remove.
            SetStatMod(DFCareer.Stats.Intelligence, (int)Mathf.Ceil(-1 * (magStats[(int)DFCareer.Stats.Intelligence] / 30)));
            SetStatMod(DFCareer.Stats.Personality, (int)Mathf.Ceil(-1 * (magStats[(int)DFCareer.Stats.Personality] / 30)));
        }

        public int DecreaseMagnitude(int amount, DFCareer.Stats stat)
        {
            magStats[(int)stat] -= amount;
            if (magStats[(int)stat] < 0)
                magStats[(int)stat] = 0;

            return magStats[(int)stat];
        }

        #region Serialization

        [fsObject("v1")]
        public struct SaveData_v1
        {
            public int[] magStats;
            public bool[] curseChecks;
            public int forcedRoundsRemaining;
        }

        public override object GetSaveData()
        {
            SaveData_v1 data = new SaveData_v1();
            data.magStats = magStats;
            data.curseChecks = curseChecks;
            data.forcedRoundsRemaining = forcedRoundsRemaining;

            return data;
        }

        public override void RestoreSaveData(object dataIn)
        {
            if (dataIn == null)
                return;

            SaveData_v1 data = (SaveData_v1)dataIn;
            magStats = data.magStats;
            curseChecks = data.curseChecks;
            forcedRoundsRemaining = data.forcedRoundsRemaining;
        }

        #endregion
    }
}