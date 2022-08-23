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
    public class Dislocate : BaseEntityEffect
    {
        public static readonly string effectKey = "Dislocate"; // Will also maybe want to add unique sound-effect and animation/particles for this effect, during the "polishing" phase and such. 

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.SingleTargetAtRange;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Thaumaturgy;
            properties.DisableReflectiveEnumeration = true;
            // If possible, later maybe add a duration to this that would actually effect how long the projectile can travel before disappearing and fizzling out? Might have to do PR for something like it.
        }

        #region Text

        public override string GroupName => "Dislocate";
        const string effectDescription = "Teleports to whatever the projectile first collides with.";
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                effectDescription,
                "Duration: Will eventually determine how long projectile lasts before disappearing.",
                "Chance: N/A",
                "Magnitude: N/A");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Duration: Will eventually determine how long projectile lasts before disappearing.",
                "Chance: N/A",
                "Magnitude: N/A",
                effectDescription);
        }

        #endregion

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null) // Get this working tomorrow, where projectile hitting is the "trigger" and place to teleport, etc.
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

            RaycastHit hitInfo; // Will definitely want to change some of this later on to disallow some stuff like climbing up on falls you Dislocate into, instead to just place you at the bottom if possible.
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

            Debug.LogFormat("Dislocate worked successfully...?");
        }
    }
}