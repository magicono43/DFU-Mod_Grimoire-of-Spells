using System;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.FallExe;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Weather;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace GrimoireofSpells
{
    public class Polymorph : IncumbentEffect
    {
        private static readonly string effectKey = "Polymorph"; // Will almost definitely have to add save-data for this eventually, but right now not sure how I will or will have to.

        // Variant can be stored internally with any format
        struct VariantProperties
        {
            public string subGroupKey;
            public EffectProperties effectProperties;
            public MobileTypes enemyType;
            public float challengeCostMod;
        }

        private readonly VariantProperties[] variants = new VariantProperties[]
        {
            new VariantProperties()
            {
                subGroupKey = "Rat",
                enemyType = MobileTypes.Rat,
                challengeCostMod = 1.0f, // These challenge values will just be place-holder for now until later polish happens.
            },
            new VariantProperties()
            {
                subGroupKey = "Bat",
                enemyType = MobileTypes.GiantBat,
                challengeCostMod = 1.0f,
            },
            new VariantProperties()
            {
                subGroupKey = "Bear",
                enemyType = MobileTypes.GrizzlyBear,
                challengeCostMod = 1.0f,
            },
            new VariantProperties()
            {
                subGroupKey = "Tiger",
                enemyType = MobileTypes.SabertoothTiger,
                challengeCostMod = 1.0f,
            },
            new VariantProperties()
            {
                subGroupKey = "Spider",
                enemyType = MobileTypes.Spider,
                challengeCostMod = 1.0f,
            },
            new VariantProperties()
            {
                subGroupKey = "Slaughterfish",
                enemyType = MobileTypes.Slaughterfish,
                challengeCostMod = 1.0f,
            },
            new VariantProperties()
            {
                subGroupKey = "Scorpion",
                enemyType = MobileTypes.GiantScorpion,
                challengeCostMod = 1.0f,
            },
            new VariantProperties()
            {
                subGroupKey = "Dragonling",
                enemyType = MobileTypes.Dragonling,
                challengeCostMod = 1.0f,
            },
            /*new VariantProperties()
            {
                subGroupKey = "Random Beast",
                enemyType = MobileTypes.Centaur, // Have to implement the "random roll" aspect of this later.
                challengeCostMod = 1.0f,
            },*/
        };

        // Must override Properties to return correct properties for any variant
        // The currentVariant value is set by magic framework - each variant gets enumerated to its own effect template
        public override EffectProperties Properties
        {
            get { return variants[currentVariant].effectProperties; }
        }


        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_Other;
            properties.AllowedElements = ElementTypes.Magic;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;
            properties.SupportDuration = true;
            properties.DurationCosts = MakeEffectCosts(16, 80, 20);

            // Set variant count so framework knows how many to extract
            variantCount = variants.Length;

            // Set properties unique to each variant
            for (int i = 0; i < variantCount; ++i)
            {
                variants[i].effectProperties = properties; //making a copy of default properties struct
                variants[i].effectProperties.Key = string.Format("{0}-{1}", effectKey, variants[i].subGroupKey);
            }
        }

        #region Text

        public override string GroupName => "Polymorph";
        public override string SubGroupName => variants[currentVariant].subGroupKey;
        public override string DisplayName => "Polymorph: " + SubGroupName;
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                DisplayName,
                "Transforms target into a " + SubGroupName + " for the duration.",
                "Duration: Rounds target is transformed.",
                "Chance: N/A",
                "Magnitude: N/A",
                "School of: Alteration");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                DisplayName,
                "Duration: %bdr + %adr per %cld level(s)",
                "Chance: N/A",
                "Magnitude: N/A", // Fix grammar and text later on.
                "",
                "Transforms target into a " + SubGroupName + " for the duration.",
                "School of: Alteration");
        }

        #endregion

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            // Get peered entity gameobject
            DaggerfallEntityBehaviour targetEntity = GetPeeredEntityBehaviour(manager);
            if (!targetEntity)
                return;

            PlayerAggro();

            VariantProperties variant = variants[currentVariant];

            // Change target enemy
            try
            {
                if (targetEntity.Entity is EnemyEntity)
                {
                    // Get enemy entity - cannot have Wabbajack active already
                    EnemyEntity enemy = (EnemyEntity)targetEntity.Entity;
                    if (enemy == null)
                        return;

                    if (enemy.WabbajackActive)
                    {
                        DaggerfallUI.AddHUDText("You can't polymorph something that is currently polymorphed.", 3.0f);
                        return;
                    }

                    // Get new enemy career and transform
                    MobileTypes enemyType = variants[currentVariant].enemyType;
                    if ((int)enemyType == enemy.CareerIndex)
                    {
                        DaggerfallUI.AddHUDText("You can't polymorph something into itself.", 3.0f);
                        return;
                    }
                    Transform parentTransform = targetEntity.gameObject.transform.parent;

                    // Do not disable enemy if in use by the quest system
                    QuestResourceBehaviour questResourceBehaviour = targetEntity.GetComponent<QuestResourceBehaviour>(); // Continue from here tomorrow most likely.
                    if (questResourceBehaviour && !questResourceBehaviour.IsFoeDead)
                        return;

                    string[] enemyNames = TextManager.Instance.GetLocalizedTextList("enemyNames");
                    if (enemyNames == null)
                        throw new System.Exception("enemyNames array text not found");

                    // Switch entity
                    targetEntity.gameObject.SetActive(false);
                    GameObject gameObject = GameObjectHelper.CreateEnemy(enemyNames[(int)enemyType], enemyType, targetEntity.transform.localPosition, MobileGender.Unspecified, parentTransform);
                    DaggerfallEntityBehaviour newEnemyBehaviour = gameObject.GetComponent<DaggerfallEntityBehaviour>();
                    EnemyEntity newEnemy = (EnemyEntity)newEnemyBehaviour.Entity;
                    newEnemy.WabbajackActive = true;
                    newEnemy.CurrentHealth -= enemy.MaxHealth - enemy.CurrentHealth; // carry over damage to new monster
                }
            }
            catch (Exception e)
            {
                DaggerfallUI.AddHUDText("It appears something went wrong...", 2.0f);
                Debug.LogException(e);
            }

            try
            {
                if (GameManager.Instance.PlayerEnterExit.IsPlayerInside)
                {
                    DaggerfallUI.AddHUDText("You must be outside to alter the weather.", 3.0f);
                }
                else if (GetCurrentWeatherType() == variant.weatherType)
                {
                    DaggerfallUI.AddHUDText("It's already " + variant.subGroupKey + " out...", 3.0f); // Fix inevitable weird grammar.
                }
                else
                {
                    //requires spell component
                    DaggerfallUnityItem ingredient1;
                    DaggerfallUnityItem ingredient2;

                    if (TryGetSpellComponent(out ingredient1, out ingredient2))
                    {
                        GameManager.Instance.WeatherManager.SetWeather(variant.weatherType); // Might have to check for allowed weather types in current climate and season? Possibly will see.
                        Caster.Entity.Items.RemoveOne(ingredient1);
                        Caster.Entity.Items.RemoveOne(ingredient2);
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                DaggerfallUI.AddHUDText("It appears something went wrong...", 3.0f);
                Debug.LogException(e);
            }
        }
    }
}
