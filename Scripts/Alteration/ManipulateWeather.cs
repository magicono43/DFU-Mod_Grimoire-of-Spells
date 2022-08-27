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

namespace GrimoireofSpells
{
    public class ManipulateWeather : BaseEntityEffect // Will want to refine this more later just the same for the rest, but for now it's working well enough to call it good for now.
    {
        private static readonly string effectKey = "Manipulate Weather";

        // Variant can be stored internally with any format
        struct VariantProperties
        {
            public string subGroupKey;
            public EffectProperties effectProperties;
            public WeatherType weatherType;
            public ItemGroups itemGroup1;
            public ItemGroups itemGroup2;
            public int itemIndex1;
            public int itemIndex2;
        }

        private readonly VariantProperties[] variants = new VariantProperties[]
        {
            new VariantProperties()
            {
                subGroupKey = "Sunny",
                weatherType = WeatherType.Sunny,
                itemGroup1 = ItemGroups.PlantIngredients2, // Currently no idea how these "multi-variables" work with logical operators in struct variables, strange.
                itemIndex1 = (int)PlantIngredients2.Cactus, // place-holder for now since the northern and southern flower crap will probably cause issues.
                itemGroup2 = ItemGroups.MetalIngredients,
                itemIndex2 = (int)MetalIngredients.Gold,
            },
            /*new VariantProperties()
            {
                subGroupKey = "Cloudy",
                weatherType = WeatherType.Cloudy,
                itemGroup1 = ItemGroups.MiscellaneousIngredients1,
                itemIndex1 = (int)MiscellaneousIngredients1.Pure_water, // Cloudy weather is currently not implemented in DFU, yet. 8/26/2022
                itemGroup2 = ItemGroups.MiscellaneousIngredients2,
                itemIndex2 = (int)MiscellaneousIngredients2.Ivory,
            },*/
            new VariantProperties()
            {
                subGroupKey = "Overcast",
                weatherType = WeatherType.Overcast,
                itemGroup1 = ItemGroups.MiscellaneousIngredients1,
                itemIndex1 = (int)MiscellaneousIngredients1.Pure_water,
                itemGroup2 = ItemGroups.MetalIngredients,
                itemIndex2 = (int)MetalIngredients.Tin,
            },
            new VariantProperties()
            {
                subGroupKey = "Fog",
                weatherType = WeatherType.Fog,
                itemGroup1 = ItemGroups.MiscellaneousIngredients1,
                itemIndex1 = (int)MiscellaneousIngredients1.Pure_water,
                itemGroup2 = ItemGroups.CreatureIngredients1,
                itemIndex2 = (int)CreatureIngredients1.Ectoplasm,
            },
            new VariantProperties()
            {
                subGroupKey = "Rain",
                weatherType = WeatherType.Rain,
                itemGroup1 = ItemGroups.MiscellaneousIngredients1,
                itemIndex1 = (int)MiscellaneousIngredients1.Rain_water,
                itemGroup2 = ItemGroups.MetalIngredients,
                itemIndex2 = (int)MetalIngredients.Tin,
            },
            new VariantProperties()
            {
                subGroupKey = "Thunderstorm",
                weatherType = WeatherType.Thunder,
                itemGroup1 = ItemGroups.MiscellaneousIngredients1,
                itemIndex1 = (int)MiscellaneousIngredients1.Rain_water,
                itemGroup2 = ItemGroups.MetalIngredients,
                itemIndex2 = (int)MetalIngredients.Iron,
            },
            new VariantProperties()
            {
                subGroupKey = "Snow",
                weatherType = WeatherType.Snow,
                itemGroup1 = ItemGroups.MiscellaneousIngredients1,
                itemIndex1 = (int)MiscellaneousIngredients1.Rain_water,
                itemGroup2 = ItemGroups.MetalIngredients,
                itemIndex2 = (int)MetalIngredients.Silver,
            },
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
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = ElementTypes.Shock;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DisableReflectiveEnumeration = true;
            // Maybe support duration as well? But not sure right now how I might do that for keeping the weather in a specific state, will have to see later.

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

        public override string GroupName => "Manipulate Weather";
        public override string SubGroupName => variants[currentVariant].subGroupKey;
        public override string DisplayName => "Manipulate Weather: " + SubGroupName;
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private string GetEffectDescription()
        {
            ItemTemplate item1 = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(variants[currentVariant].itemIndex1);
            ItemTemplate item2 = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(variants[currentVariant].itemIndex2);
            return "Changes the current weather to " + SubGroupName + "; requires " + item1.name + " and " + item2.name;
        }

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                DisplayName,
                GetEffectDescription(),
                "Duration: N/A",
                "Chance: N/A",
                "Magnitude: N/A",
                "School of: Alteration");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                DisplayName,
                "Duration: N/A",
                "Chance: N/A",
                "Magnitude: N/A", // Fix grammar and text later on.
                "",
                GetEffectDescription(),
                "School of: Alteration");
        }

        #endregion

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (caster == null)
            {
                return;
            }

            VariantProperties variant = variants[currentVariant];
            bool success = false;

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

            if (!success)
            {
                RefundSpellCost(manager);
                End();
            }
        }

        public WeatherType GetCurrentWeatherType()
        {
            WeatherManager weatherManager = GameManager.Instance.WeatherManager;

            if (weatherManager.IsSnowing)
                return WeatherType.Snow;
            else if (weatherManager.IsStorming)
                return WeatherType.Thunder;
            else if (weatherManager.IsRaining)
                return WeatherType.Rain;
            else if (weatherManager.IsOvercast && weatherManager.currentOutdoorFogSettings.density == weatherManager.HeavyFogSettings.density)
                return WeatherType.Fog;
            else if (weatherManager.IsOvercast)
                return WeatherType.Overcast;
            else
                return WeatherType.Sunny;
        }

        /// <summary>
        /// Checks the caster's inventory for the spell ingredient/component
        /// </summary>
        /// <returns>true if the component was found, false otherwise</returns>
        private bool TryGetSpellComponent(out DaggerfallUnityItem item1, out DaggerfallUnityItem item2)
        {
            VariantProperties variant = variants[currentVariant];

            item1 = Caster.Entity.Items.GetItem(variant.itemGroup1, variant.itemIndex1, false, false, true);
            item2 = Caster.Entity.Items.GetItem(variant.itemGroup2, variant.itemIndex2, false, false, true);

            if (item1 == null)
            {
                ItemTemplate itemTemplate = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(variant.itemIndex1);
                DaggerfallUI.AddHUDText("You need " + itemTemplate.name + " to make it " + variant.subGroupKey + ".", 3.0f);
                return false;
            }

            if (item2 == null)
            {
                ItemTemplate itemTemplate = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(variant.itemIndex2);
                DaggerfallUI.AddHUDText("You need " + itemTemplate.name + " to make it " + variant.subGroupKey + ".", 3.0f);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Refund magicka cost of this effect to the caster
        /// </summary>
        private void RefundSpellCost(EntityEffectManager manager)
        {
            if (manager.ReadySpell != null)
            {
                foreach (EffectEntry entry in manager.ReadySpell.Settings.Effects)
                {
                    if (entry.Key.Equals(Key) && entry.Settings.Equals(Settings))
                    {
                        FormulaHelper.SpellCost cost = FormulaHelper.CalculateEffectCosts(this, Settings, Caster.Entity);
                        Caster.Entity.IncreaseMagicka(cost.spellPointCost);
                        break;
                    }
                }
            }
        }
    }
}
