using UnityEngine;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game;
using FullSerializer;

namespace GrimoireofSpells
{
    /// <summary>
    /// Curse effect base. CHANGE THIS AFTERWARD TO DESCRIBE IT BETTER, PLACEHOLDER FOR NOW.
    /// Provides functionality common to all Curse effects which vary only by properties and stat.
    /// This effect uses an incumbent pattern where future applications of same effect type
    /// will only add to total magnitude of first effect of this type.
    /// Incumbent curse effect persists indefinitely until player heals stat enough for magnitude to reach 0.
    /// </summary>
    public abstract class CurseEffect : IncumbentEffect
    {
        // Str, Int, Wil, Agi, End, Per, Spe, Luc
        protected int[] magStats = { 0, 0, 0, 0, 0, 0, 0, 0 };
        protected bool[] curseChecks = { false, false, false, false, false, false, false, false };
        protected int lastMagnitudeIncreaseAmount = 0;
        int forcedRoundsRemaining = 1;

        public int[] Magnitudes
        {
            get { return magStats; }
        }

        public bool[] CursedStatChecks
        {
            get { return curseChecks; }
        }

        // Curse effects are permanent until healed so we manage our own lifecycle
        protected override int RemoveRound()
        {
            return forcedRoundsRemaining;
        }

        // Always present at least one round remaining so effect system does not remove
        public override int RoundsRemaining
        {
            get { return forcedRoundsRemaining; }
        }

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            PlayerAggro();
        }

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            CurseEffect otherCurse;

            if (other is CurseEffect)
            {
                otherCurse = other as CurseEffect;

                for (int i = 0; i < otherCurse.curseChecks.Length; i++)
                {
                    if (otherCurse.curseChecks[i] && curseChecks[i])
                    {
                        return true;
                    }
                }
            }
            return false;
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
                (incumbent as CurseEffect).IncreaseMagnitude(lastMagnitudeIncreaseAmount);
                ShowPlayerCursed();
            }
        }

        protected int GetMagnitude()
        {
            int statsCurseEffects = 0;

            for (int i = 0; i < curseChecks.Length; i++)
            {
                if (curseChecks[i])
                {
                    statsCurseEffects++;
                }
            }

            if (statsCurseEffects == 2)
                return Random.Range(2, 9);
            else if (statsCurseEffects == 4)
                return Random.Range(1, 5);
            else
                return Random.Range(1, 5);
        }

        public override void HealAttributeDamage(DFCareer.Stats stat, int amount)
        {
            // Can only heal incumbent matching curse
            if (!IsIncumbent)
                return;

            bool pairCheck = false;

            for (int i = 0; i < curseChecks.Length; i++)
            {
                if (curseChecks[i] && stat == (DFCareer.Stats)i)
                    pairCheck = true;
            }

            if (!pairCheck)
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

                int statsCurseEffects = 0;
                int statMagsNowZero = 0;

                for (int i = 0; i < curseChecks.Length; i++)
                {
                    if (curseChecks[i])
                    {
                        statsCurseEffects++;

                        if (magStats[i] == 0)
                            statMagsNowZero++;
                    }
                }

                // When all cursed stats from this effect have had their respective magnitudes reduced to 0, cancel this entire effect. 
                if (statsCurseEffects == statMagsNowZero)
                    forcedRoundsRemaining = 0;
            }
        }

        public override void CureAttributeDamage()
        {
            // Eventually add some alternate methods to remove curse effects, will have to see if this all works at all first anyway, etc.

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
            for (int i = 0; i < curseChecks.Length; i++)
            {
                if (curseChecks[i])
                {
                    // Allow magnitude to reduce stat below 1. So stats reduced by a "curse" effect CAN kill you, unlike the drain effect which is stopped at 1.
                    magStats[i] += amount * 30;

                    // Thinking about making curses attribute damage be 10-30x more "sticky" than normal stat drain effects. So requires either much more "heal" or some other treatment/service to remove.
                    SetStatMod((DFCareer.Stats)i, (int)Mathf.Ceil(-1 * (magStats[i] / 30)));
                }
            }
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