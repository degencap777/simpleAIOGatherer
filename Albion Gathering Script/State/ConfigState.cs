using Ennui.Api;
using Ennui.Api.Builder;
using Ennui.Api.Gui;
using Ennui.Api.Meta;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

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
        private IInputField harvestRockInput;
        private ICheckBox harvestRockCheckBox;

        private ICheckBox killMobsCheckBox;

        private ILabel resourceClusterLabel;
        private ILabel resourceClusterInput;

        private ILabel cityClusterLabel;
        private ILabel cityClusterInput;

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

        public void UpdateForConfig()
        {
            var sets = new Dictionary<ResourceType, List<string>>();
            sets.Add(ResourceType.Fiber, new List<string>());
            sets.Add(ResourceType.Hide, new List<string>());
            sets.Add(ResourceType.Ore, new List<string>());
            sets.Add(ResourceType.Rock, new List<string>());
            sets.Add(ResourceType.Wood, new List<string>());
            
            foreach (var ts in config.TypeSetsToUse)
            {
                if (sets.ContainsKey(ts.Type))
                {
                    Logging.Log("Adding typeset " + (ts.MaxTier + ts.MaxRarity > 0 ? ("." + ts.MaxRarity) : ""));
                    sets[ts.Type].Add(ts.MaxTier + ((ts.MinRarity > 0) ? ("." + ts.MaxRarity) : ""));
                }
            }

            foreach (var s in sets[ResourceType.Fiber]) Logging.Log(s);
            foreach (var s in sets[ResourceType.Hide]) Logging.Log(s);
            foreach (var s in sets[ResourceType.Ore]) Logging.Log(s);
            foreach (var s in sets[ResourceType.Rock]) Logging.Log(s);
            foreach (var s in sets[ResourceType.Wood]) Logging.Log(s);

            if (sets[ResourceType.Fiber].Count > 0)
            harvestFiberInput.SetText(string.Join(",", sets[ResourceType.Fiber].ToArray()));

            if (sets[ResourceType.Hide].Count > 0)
                harvestHideInput.SetText(string.Join(",", sets[ResourceType.Hide].ToArray()));

            if (sets[ResourceType.Ore].Count > 0)
                harvestOreInput.SetText(string.Join(",", sets[ResourceType.Ore].ToArray()));

            if (sets[ResourceType.Rock].Count > 0)
                harvestRockInput.SetText(string.Join(",", sets[ResourceType.Rock].ToArray()));

            if (sets[ResourceType.Wood].Count > 0)
                harvestWoodInput.SetText(string.Join(",", sets[ResourceType.Wood].ToArray()));

            killMobsCheckBox.SetSelected(config.AttackMobs);
            characterNameInput.SetText(config.LoginCharacterName);
            resourceClusterInput.SetText(config.ResourceClusterName);
            cityClusterInput.SetText(config.CityClusterName);
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

                    var targetInfo = filtered.Split('.');

                    var tier = 0;
                    if (targetInfo.Length >= 1)
                    {
                        if (!int.TryParse(targetInfo[0], out tier))
                        {
                            Logging.Log("Failed to parse tier " + input);
                        }
                    }

                    var rarity = 0;
                    if (targetInfo.Length >= 2)
                    {
                        if (!int.TryParse(targetInfo[1], out rarity))
                        {
                            Logging.Log("Failed to parse rarity " + input);
                        }
                    }

                    config.TypeSetsToUse.Add(new SafeTypeSet(tier, tier, type, rarity, rarity));
                }
            }
            catch (Exception e)
            {
                context.State = "Failed to parise tiers " + input;
                context.State = "Failed to parse tiers " + input;
            }
        }

        private void SaveConfig()
        {
            try
            {
                if (Files.Exists("simple-aio-gatherer.json"))
                {
                    Files.Delete("simple-aio-gatherer.json");
                }
                Files.WriteText("simple-aio-gatherer.json", Codecs.ToJson(config));
            }
            catch (Exception e)
            {
                Logging.Log("Failed to save config " + e, LogLevel.Error);
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

            if (harvestRockCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Rock, harvestRockInput.GetText());
            }

            if (config.TypeSetsToUse.Count == 0)
            {
                context.State = "No type sets to gather!";
                return;
            }

            config.AttackMobs = killMobsCheckBox.IsSelected();
            config.GatherArea = new SafeMapArea(config.ResourceClusterName, new Vector3f(-10000, -10000, -10000), new Vector3f(10000, 10000, 10000));
            if (autoLoginCheckbox.IsSelected())
            {
                config.LoginCharacterName = characterNameInput.GetText();
            }

            SaveConfig();

            primaryPanel.Destroy();
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
                harvestWoodInput.SetPosition(-70, 150, 0);
                harvestWoodInput.SetSize(120, 25);

                harvestWoodCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestWoodCheckBox);
                harvestWoodCheckBox.SetPosition(60, 150, 0);
                harvestWoodCheckBox.SetSize(100, 25);
                harvestWoodCheckBox.SetText("Harvest Wood");
                harvestWoodCheckBox.SetSelected(true);

                harvestOreInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestOreInput);
                harvestOreInput.SetPosition(-70, 120, 0);
                harvestOreInput.SetSize(120, 25);

                harvestOreCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestOreCheckBox);
                harvestOreCheckBox.SetPosition(60, 120, 0);
                harvestOreCheckBox.SetSize(100, 25);
                harvestOreCheckBox.SetText("Harvest Ore");
                harvestOreCheckBox.SetSelected(true);

                harvestFiberInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestFiberInput);
                harvestFiberInput.SetPosition(-70, 90, 0);
                harvestFiberInput.SetSize(120, 25);

                harvestFiberCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestFiberCheckBox);
                harvestFiberCheckBox.SetPosition(60, 90, 0);
                harvestFiberCheckBox.SetSize(100, 25);
                harvestFiberCheckBox.SetText("Harvest Fiber");
                harvestFiberCheckBox.SetSelected(true);

                harvestHideInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestHideInput);
                harvestHideInput.SetPosition(-70, 60, 0);
                harvestHideInput.SetSize(120, 25);

                harvestHideCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestHideCheckBox);
                harvestHideCheckBox.SetPosition(60, 60, 0);
                harvestHideCheckBox.SetSize(100, 25);
                harvestHideCheckBox.SetText("Harvest Hide");
                harvestHideCheckBox.SetSelected(true);

                harvestRockInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestRockInput);
                harvestRockInput.SetPosition(-70, 30, 0);
                harvestRockInput.SetSize(120, 25);

                harvestRockCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestRockCheckBox);
                harvestRockCheckBox.SetPosition(60, 30, 0);
                harvestRockCheckBox.SetSize(100, 25);
                harvestRockCheckBox.SetText("Harvest Rock");
                harvestRockCheckBox.SetSelected(true);

                killMobsCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(killMobsCheckBox);
                killMobsCheckBox.SetPosition(-70, -5, 0);
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

                resourceClusterInput = Factories.CreateGuiLabel();
                primaryPanel.Add(resourceClusterInput);
                resourceClusterInput.SetPosition(-70, -55, 0);
                resourceClusterInput.SetSize(120, 25);

                cityClusterLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(cityClusterLabel);
                cityClusterLabel.SetPosition(70, -35, 0);
                cityClusterLabel.SetSize(120, 25);
                cityClusterLabel.SetText("City Cluster");

                cityClusterInput = Factories.CreateGuiLabel();
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
                        cityClusterInput.SetText(Game.ClusterName);
                        config.VaultDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.VaultArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
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
                        cityClusterInput.SetText(Game.ClusterName);
                        config.RepairDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.RepairArea = new SafeMapArea(Game.ClusterName, new Area(area.Start, area.End));
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
                        resourceClusterInput.SetText(Game.ClusterName);
                        config.RoamPoints.Add(new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z)));
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

                UpdateForConfig();
            });

            return true;
        }
    }
}
