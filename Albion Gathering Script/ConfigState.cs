using Ennui.Api;
using Ennui.Api.Builder;
using Ennui.Api.Gui;
using Ennui.Api.Meta;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ennui.Script.Official
{
    public class ConfigState : StateScript
    {
        private IPanel primaryPanel;
        private ILabel tierLabel;
        private IInputField harvestWoodInput;
        private ICheckBox harvestWoodCheckBox;
        private IInputField harvestOreInput;
        private ICheckBox harvestOreCheckBox;
        private IInputField harvestFiberInput;
        private ICheckBox harvestFiberCheckBox;
        private IInputField harvestHideInput;
        private ICheckBox harvestHideCheckBox;
        private IInputField harvestStoneInput;
        private ICheckBox harvestStoneCheckBox;

        private ICheckBox killMobsCheckBox;
        private ICheckBox useWebCheckBox;

        private ILabel resourceClusterLabel;
        private IInputField resourceClusterInput;

        private ILabel cityClusterLabel;
        private IInputField cityClusterInput;

        private ICheckBox autoLoginCheckbox;
        private ILabel characterNameLabel;
        private IInputField characterNameInput;

        private IButton runButton;

        private Configuration config;

        public ConfigState(Configuration config)
        {
            this.config = config;
        }

        private void AddTiers(ResourceType type, string input)
        {
            if (input.Length == 0)
            {
                return;
            }
            
            var tierGroups = input.Replace(" ", "").Split(',');
            foreach (var tierGroup in tierGroups)
            {
                var tierGroupsBroken = tierGroup.Split('.');

                var minTier = 0;
                var maxTier = 0;
                var minRarity = 0;
                var maxRarity = 10;
                if (tierGroupsBroken.Length > 0)
                {
                    minTier = int.Parse(tierGroupsBroken[0]);
                    maxTier = int.Parse(tierGroupsBroken[0]);

                    if (tierGroupsBroken.Length == 2)
                    {
                        minRarity = int.Parse(tierGroupsBroken[1]);
                        maxRarity = int.Parse(tierGroupsBroken[1]);
                    }
                }

                Logging.Log("Adding tier " + type + " " + minTier + "-" + maxTier + " " + minRarity + "-" + maxRarity);

                config.TypeSetsToUse.Add(new TypeSet(minTier, maxTier, type, minRarity, maxRarity));
            }
        }

        private void SelectedStart()
        {
            config.CityClusterName = cityClusterInput.GetText();
            config.ResourceClusterName = resourceClusterInput.GetText();

            if (harvestWoodCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Wood, harvestWoodInput.GetText());
            }
            
            if (harvestOreCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Ore, harvestOreInput.GetText());
            }
            
            if (harvestFiberCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Fiber, harvestFiberInput.GetText());
            }
            
            if (harvestHideCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Hide, harvestHideInput.GetText());
            }
            
            if (harvestStoneCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Rock, harvestStoneInput.GetText());
            }

            config.AttackMobs = killMobsCheckBox.IsSelected();

            UI.InputEnabled = true;
            Hotkeys.Rehook();
            primaryPanel.Destroy();

            config.VaultArea = new MapArea(Api, config.CityClusterName, new Vector3f(288, -8, 296), new Vector3f(302, 0, 307));
            config.RepairArea = new MapArea(Api, config.CityClusterName, new Vector3f(306, -8, 314), new Vector3f(316, 0, 326));
            config.GatherArea = new MapArea(Api, config.ResourceClusterName, new Vector3f(-10000, -10000, -10000), new Vector3f(10000, 10000, 10000));

            if (autoLoginCheckbox.IsSelected())
            {
                config.LoginCharacterName = characterNameInput.GetText();
            }

            parent.EnterState("resolve");
        }

        public override bool OnStart(IScriptEngine se)
        {
            Game.Sync(() =>
            {
                var screenSize = Game.ScreenSize;

                primaryPanel = Factories.CreateGuiPanel();
                GuiScene.Add(primaryPanel);
                primaryPanel.SetSize(300, 390);
                primaryPanel.SetPosition(screenSize.X / 2, (screenSize.Y / 2), 0);
                primaryPanel.SetAnchor(new Vector2f(0.0f, 0.0f), new Vector2f(0.0f, 0.0f));
                primaryPanel.SetPivot(new Vector2f(0.5f, 0.5f));

                tierLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(tierLabel);
                tierLabel.SetPosition(-60, 175, 0);
                tierLabel.SetSize(100, 25);
                tierLabel.SetText("Tier");

                harvestWoodInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestWoodInput);
                harvestWoodInput.SetPosition(-60, 150, 0);
                harvestWoodInput.SetSize(100, 25);

                harvestWoodCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestWoodCheckBox);
                harvestWoodCheckBox.SetPosition(60, 150, 0);
                harvestWoodCheckBox.SetSize(100, 25);
                harvestWoodCheckBox.SetText("Harvest Wood");
                harvestWoodCheckBox.SetSelected(true);

                harvestOreInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestOreInput);
                harvestOreInput.SetPosition(-60, 120, 0);
                harvestOreInput.SetSize(100, 25);

                harvestOreCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestOreCheckBox);
                harvestOreCheckBox.SetPosition(60, 120, 0);
                harvestOreCheckBox.SetSize(100, 25);
                harvestOreCheckBox.SetText("Harvest Ore");
                harvestOreCheckBox.SetSelected(true);

                harvestFiberInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestFiberInput);
                harvestFiberInput.SetPosition(-60, 90, 0);
                harvestFiberInput.SetSize(100, 25);

                harvestFiberCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestFiberCheckBox);
                harvestFiberCheckBox.SetPosition(60, 90, 0);
                harvestFiberCheckBox.SetSize(100, 25);
                harvestFiberCheckBox.SetText("Harvest Fiber");
                harvestFiberCheckBox.SetSelected(true);

                harvestHideInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestHideInput);
                harvestHideInput.SetPosition(-60, 60, 0);
                harvestHideInput.SetSize(100, 25);

                harvestHideCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestHideCheckBox);
                harvestHideCheckBox.SetPosition(60, 60, 0);
                harvestHideCheckBox.SetSize(100, 25);
                harvestHideCheckBox.SetText("Harvest Hide");
                harvestHideCheckBox.SetSelected(true);

                harvestStoneInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestStoneInput);
                harvestStoneInput.SetPosition(-60, 30, 0);
                harvestStoneInput.SetSize(100, 25);

                harvestStoneCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestStoneCheckBox);
                harvestStoneCheckBox.SetPosition(60, 30, 0);
                harvestStoneCheckBox.SetSize(100, 25);
                harvestStoneCheckBox.SetText("Harvest Rock");
                harvestStoneCheckBox.SetSelected(true);

                resourceClusterLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(resourceClusterLabel);
                resourceClusterLabel.SetPosition(-70, -10, 0);
                resourceClusterLabel.SetSize(120, 25);
                resourceClusterLabel.SetText("Resource Cluster");

                resourceClusterInput = Factories.CreateGuiInputField();
                primaryPanel.Add(resourceClusterInput);
                resourceClusterInput.SetPosition(-70, -35, 0);
                resourceClusterInput.SetSize(120, 25);

                cityClusterLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(cityClusterLabel);
                cityClusterLabel.SetPosition(70, -10, 0);
                cityClusterLabel.SetSize(120, 25);
                cityClusterLabel.SetText("City Cluster");

                cityClusterInput = Factories.CreateGuiInputField();
                primaryPanel.Add(cityClusterInput);
                cityClusterInput.SetPosition(70, -35, 0);
                cityClusterInput.SetSize(120, 25);

                killMobsCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(killMobsCheckBox);
                killMobsCheckBox.SetPosition(0, -65, 0);
                killMobsCheckBox.SetSize(125, 25);
                killMobsCheckBox.SetText("Kill Mobs");
                killMobsCheckBox.SetSelected(true);

                autoLoginCheckbox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(autoLoginCheckbox);
                autoLoginCheckbox.SetPosition(0, -100, 0);
                autoLoginCheckbox.SetSize(125, 25);
                autoLoginCheckbox.SetText("Auto Relogin");
                autoLoginCheckbox.SetSelected(true);

                characterNameLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(characterNameLabel);
                characterNameLabel.SetPosition(0, -125, 0);
                characterNameLabel.SetSize(125, 25);
                characterNameLabel.SetText("Character Name");

                characterNameInput = Factories.CreateGuiInputField();
                primaryPanel.Add(characterNameInput);
                characterNameInput.SetPosition(0, -145, 0);
                characterNameInput.SetSize(125, 25);

                runButton = Factories.CreateGuiButton();
                primaryPanel.Add(runButton);
                runButton.SetPosition(0, -175, 0);
                runButton.SetSize(125, 25);
                runButton.SetText("Run");

                runButton.AddActionListener((e) =>
                {
                    Logging.Log("trace 1", LogLevel.Trace);
                    SelectedStart();
                });

                UI.InputEnabled = false;
                Hotkeys.Unhook();
            });

            return true;
        }
    }
}
