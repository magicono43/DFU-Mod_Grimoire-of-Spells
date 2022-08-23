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
    public class Blink : BaseEntityEffect
    {
        public static readonly string effectKey = "Blink"; // Will also maybe want to add unique sound-effect and animation/particles for this effect, during the "polishing" phase and such. 

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Thaumaturgy;
            properties.DisableReflectiveEnumeration = true;
            properties.SupportMagnitude = true;
            properties.MagnitudeCosts = MakeEffectCosts(8, 100, 30); // These values will have to be adjusted heavily, basically just placeholder for now.
        }

        #region Text

        public override string GroupName => "Blink";
        const string effectDescription = "Instantly teleports caster a short distance, where they are looking.";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                effectDescription,
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: Maximum distance will travel, in meters.");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Duration: Instantaneous.",
                "Chance: N/A",
                "Magnitude: %1bm - %2bm + %1am - %2am per %clm level(s) Maximum meters will travel.", // Fix grammar and text later on.
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

            bool forceTeleOnNoHit = true;                //teleport maxDistance even if raycast doesn't hit
            float maxDistance = GetMagnitude();          //max distance
            int step = 0;
            Vector3 dir = Camera.main.transform.up;
            Vector3 loc;

            RaycastHit hitInfo; // Will definitely want to change some of this later on to disallow some stuff like climbing up on falls you blink into, instead to just place you at the bottom if possible.
            Vector3 origin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            Ray ray = new Ray(origin + Camera.main.transform.forward * .2f, Camera.main.transform.forward);
            GameManager.Instance.AcrobatMotor.ClearFallingDamage();
            if (!(Physics.Raycast(ray, out hitInfo, maxDistance)))
            {
                Debug.Log("Didn't hit anything...");
                if (forceTeleOnNoHit)
                {
                    GameManager.Instance.PlayerObject.transform.position = ray.GetPoint(maxDistance);
                    Debug.Log("...teleporting anyways");
                }
            }
            else
            {
                loc = hitInfo.point;
                while (Physics.CheckCapsule(loc, loc + dir, GameManager.Instance.PlayerController.radius + .1f) && step < 50) // Will have to look more into how this works to get better understanding.
                {
                    loc = dir + loc;
                    step++;
                }
                GameManager.Instance.PlayerObject.transform.position = loc;
            }

            Debug.LogFormat("Blink worked successfully...?");
        }
    }
}