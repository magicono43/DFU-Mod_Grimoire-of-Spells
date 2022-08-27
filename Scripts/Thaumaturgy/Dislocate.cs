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
            properties.AllowedTargets = TargetTypes.CasterOnly;
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
            base.Start(manager, caster); // Alright, so once again I'm just going to have to come back to this later, the projectile thing is confusing and fucking with me too much right now, move on.

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            /*EffectBundleSettings bundleSettings;
            if (bundleSettings.TargetType == TargetTypes.CasterOnly)
            {
                // Spell is readied on player for free
                GameManager.Instance.PlayerEffectManager.SetReadySpell(thisAction.Index, true);
            }
            else
            {
                // Spell is fired at player, at strength of player level, from triggering object
                DaggerfallMissile missile = GameManager.Instance.PlayerEffectManager.InstantiateSpellMissile(bundleSettings.ElementType);
                missile.Payload = new EntityEffectBundle(bundleSettings);
                Vector3 customAimPosition = thisAction.transform.position;
                customAimPosition.y += 40 * MeshReader.GlobalScale;
                missile.CustomAimPosition = customAimPosition;
                missile.CustomAimDirection = Vector3.Normalize(GameManager.Instance.PlayerObject.transform.position - thisAction.transform.position);

                // If action spell payload is "touch" then set to "target at range" (targets player position as above)
                if (missile.Payload.Settings.TargetType == TargetTypes.ByTouch)
                {
                    EffectBundleSettings settings = missile.Payload.Settings;
                    settings.TargetType = TargetTypes.SingleTargetAtRange;
                    missile.Payload.Settings = settings;
                }
            }*/
        }

        // Get missile aim position from player or enemy mobile
        /*Vector3 GetAimPosition()
        {
            // Aim position from custom source
            if (CustomAimPosition != Vector3.zero)
                return CustomAimPosition;

            // Aim position is from eye level for player or origin for other mobile
            // Player must aim from camera position or it feels out of alignment
            Vector3 aimPosition = caster.transform.position;
            if (caster == GameManager.Instance.PlayerEntityBehaviour)
            {
                aimPosition = GameManager.Instance.MainCamera.transform.position;
            }

            return aimPosition;
        }

        // Get missile aim direction from player or enemy mobile
        Vector3 GetAimDirection()
        {
            // Aim direction from custom source
            if (CustomAimDirection != Vector3.zero)
                return CustomAimDirection;

            // Aim direction should be from camera for player or facing for other mobile
            Vector3 aimDirection = Vector3.zero;
            if (caster == GameManager.Instance.PlayerEntityBehaviour)
            {
                aimDirection = GameManager.Instance.MainCamera.transform.forward;
            }
            else if (enemySenses)
            {
                Vector3 predictedPosition;
                if (DaggerfallUnity.Settings.EnhancedCombatAI)
                    predictedPosition = enemySenses.PredictNextTargetPos(MovementSpeed);
                else
                    predictedPosition = enemySenses.LastKnownTargetPos;

                if (predictedPosition == EnemySenses.ResetPlayerPos)
                    aimDirection = caster.transform.forward;
                else
                    aimDirection = (predictedPosition - caster.transform.position).normalized;
            }

            return aimDirection;
        }*/
    }
}