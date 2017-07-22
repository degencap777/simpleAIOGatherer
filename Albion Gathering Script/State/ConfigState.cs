using Ennui.Api;
using Ennui.Api.Builder;
using Ennui.Api.Gui;
using Ennui.Api.Meta;
using Ennui.Api.Script;
using System;

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

        private ILabel resourceClusterLabel;
        private IInputField resourceClusterInput;

        private ILabel cityClusterLabel;
        private IInputField cityClusterInput;

        private ICheckBox autoLoginCheckbox;
        private ILabel characterNameLabel;
        private IInputField characterNameInput;

        private IButton setVaultAreaButton;
        private IButton setRepairAreaButton;
        private IButton addRoamPointButton;

        private IButton runButton;

        private Configuration config;
        private Context context;

        public ConfigState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        private void AddTiers(ResourceType type, string input)
        {
            if (input.Length == 0)
            {
                return;
            }

            try
            {
                var tierGroups = input.Replace(" ", "").Split(',');
                foreach (var tierGroup in tierGroups)
                {
                    var filtered = tierGroup.Trim(' ', ',');
                    if (filtered.Length == 0)
                    {
                        continue;
                    }

                    var tierGroupsBroken = filtered.Split('.');

                    var tier = 0;
                    var rarity = 0;
                    if (tierGroupsBroken.Length > 0)
                    {
                        tier = 0;
                        if (!int.TryParse(tierGroupsBroken[0], out tier))
                        {
                            Logging.Log("Failed to parse tier " + input);
                        }

                        if (tierGroupsBroken.Length == 2)
                        {
                            rarity = 0;
                            if (!int.TryParse(tierGroupsBroken[0], out rarity))
                            {
                                Logging.Log("Failed to parse rarity " + input);
                            }
                        }
                        else
                        {
                            Logging.Log("Invalid rarity format " + input);
                        }
                    }

                    config.TypeSetsToUse.Add(new TypeSet(tier, tier, type, rarity, rarity));
                }
            }
            catch (Exception e)
            {
                context.State = "Failed to parse tiers " + input;
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
            
            primaryPanel.Destroy();
            
            config.GatherArea = new MapArea(Api, config.ResourceClusterName, new Vector3f(-10000, -10000, -10000), new Vector3f(10000, 10000, 10000));

            if (autoLoginCheckbox.IsSelected())
            {
                config.LoginCharacterName = characterNameInput.GetText();
            }

            parent.EnterState("resolve");
        }
        
        public override bool OnStart(IScriptEngine se)
        {
            context.State = "Configuring...";

            Game.Sync(() =>
            {
                var screenSize = Game.ScreenSize;

                primaryPanel = Factories.CreateGuiPanel();
                GuiScene.Add(primaryPanel);
                primaryPanel.SetSize(300, 390);
                primaryPanel.SetPosition(155, (screenSize.Y / 2), 0);
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

                killMobsCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(killMobsCheckBox);
                killMobsCheckBox.SetPosition(-60, -5, 0);
                killMobsCheckBox.SetSize(125, 25);
                killMobsCheckBox.SetText("Kill Mobs");
                killMobsCheckBox.SetSelected(true);

                autoLoginCheckbox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(autoLoginCheckbox);
                autoLoginCheckbox.SetPosition(70, -5, 0);
                autoLoginCheckbox.SetSize(125, 25);
                autoLoginCheckbox.SetText("Auto Relogin");
                autoLoginCheckbox.SetSelected(true);

                resourceClusterLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(resourceClusterLabel);
                resourceClusterLabel.SetPosition(-70, -35, 0);
                resourceClusterLabel.SetSize(120, 25);
                resourceClusterLabel.SetText("Resource Cluster");

                resourceClusterInput = Factories.CreateGuiInputField();
                primaryPanel.Add(resourceClusterInput);
                resourceClusterInput.SetPosition(-70, -55, 0);
                resourceClusterInput.SetSize(120, 25);

                cityClusterLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(cityClusterLabel);
                cityClusterLabel.SetPosition(70, -35, 0);
                cityClusterLabel.SetSize(120, 25);
                cityClusterLabel.SetText("City Cluster");

                cityClusterInput = Factories.CreateGuiInputField();
                primaryPanel.Add(cityClusterInput);
                cityClusterInput.SetPosition(70, -55, 0);
                cityClusterInput.SetSize(120, 25);

                characterNameLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(characterNameLabel);
                characterNameLabel.SetPosition(70, -85, 0);
                characterNameLabel.SetSize(125, 25);
                characterNameLabel.SetText("Character Name");

                characterNameInput = Factories.CreateGuiInputField();
                primaryPanel.Add(characterNameInput);
                characterNameInput.SetPosition(70, -105, 0);
                characterNameInput.SetSize(125, 25);

                setVaultAreaButton = Factories.CreateGuiButton();
                primaryPanel.Add(setVaultAreaButton);
                setVaultAreaButton.SetPosition(-70, -85, 0);
                setVaultAreaButton.SetSize(125, 25);
                setVaultAreaButton.SetText("Set Vault Loc.");
                setVaultAreaButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.ThreadSafeLocation;
                        var area = loc.Expand(4, 2, 4);
                        Logging.Log("Set vault loc to " + loc.X + " " + loc.Y + " " + loc.Z);
                        cityClusterInput.SetText(Game.Cluster.Name);
                        config.VaultDest = new Vector3f(loc.X, loc.Y, loc.Z);
                        config.VaultArea = new Area(area.Start, area.End);
                    }
                });

                setRepairAreaButton = Factories.CreateGuiButton();
                primaryPanel.Add(setRepairAreaButton);
                setRepairAreaButton.SetPosition(-70, -115, 0);
                setRepairAreaButton.SetSize(125, 25);
                setRepairAreaButton.SetText("Set Repair Loc.");
                setRepairAreaButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.ThreadSafeLocation;
                        var area = loc.Expand(4, 2, 4);
                        Logging.Log("Set repair loc to " + loc.X + " " + loc.Y + " " + loc.Z);
                        cityClusterInput.SetText(Game.Cluster.Name);
                        config.RepairDest = new Vector3f(loc.X, loc.Y, loc.Z);
                        config.RepairArea = new Area(area.Start, area.End);
                    }
                });

                addRoamPointButton = Factories.CreateGuiButton();
                primaryPanel.Add(addRoamPointButton);
                addRoamPointButton.SetPosition(-70, -145, 0);
                addRoamPointButton.SetSize(125, 25);
                addRoamPointButton.SetText("Add Roam Point");
                addRoamPointButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.ThreadSafeLocation;
                        Logging.Log("Add roam point " + loc.X + " " + loc.Y + " " + loc.Z);
                        resourceClusterInput.SetText(Game.Cluster.Name);
                        config.RoamPoints.Add(new Vector3f(loc.X, loc.Y, loc.Z));
                    }
                });

                runButton = Factories.CreateGuiButton();
                primaryPanel.Add(runButton);
                runButton.SetPosition(0, -175, 0);
                runButton.SetSize(125, 25);
                runButton.SetText("Run");
                runButton.AddActionListener((e) =>
                {
                    if (config.VaultDest == null)
                    {
                        context.State = "No vault area set!";
                        return;
                    }

                    if (config.RoamPoints.Count == 0)
                    {
                        context.State = "No roam points added!";
                        return;
                    }

                    SelectedStart();
                });
            });

            return true;
        }
    }
}
