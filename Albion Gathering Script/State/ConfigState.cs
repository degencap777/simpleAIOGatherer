using Ennui.Api.Gui;
using Ennui.Api.Meta;
using Ennui.Api.Script;
using Ennui.Api.Util;
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
        private IInputField harvestStoneInput;
        private ICheckBox harvestStoneCheckBox;

        private ICheckBox killMobsCheckBox;

        private ICheckBox autoLoginCheckbox;
        private ILabel characterNameLabel;
        private IInputField characterNameInput;

        private IButton setVaultAreaButton;
        private IButton setRepairAreaButton;
        private IButton addRoamPointButton;
        private IButton removeRoamPointButton;

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
                    var input = ts.MaxTier + "." + ts.MaxRarity;
                    
                    sets[ts.Type].Add(ts.MaxTier + ((ts.MinRarity > 0) ? ("." + ts.MaxRarity) : ""));
                }
            }

            if (sets[ResourceType.Fiber].Count > 0)
            harvestFiberInput.SetText(string.Join(",", sets[ResourceType.Fiber].ToArray()));

            if (sets[ResourceType.Hide].Count > 0)
                harvestHideInput.SetText(string.Join(",", sets[ResourceType.Hide].ToArray()));

            if (sets[ResourceType.Ore].Count > 0)
                harvestOreInput.SetText(string.Join(",", sets[ResourceType.Ore].ToArray()));

            if (sets[ResourceType.Rock].Count > 0)
                harvestStoneInput.SetText(string.Join(",", sets[ResourceType.Rock].ToArray()));

            if (sets[ResourceType.Wood].Count > 0)
                harvestWoodInput.SetText(string.Join(",", sets[ResourceType.Wood].ToArray()));

            harvestWoodCheckBox.SetSelected(config.GatherWood);
            harvestOreCheckBox.SetSelected(config.GatherOre);
            harvestFiberCheckBox.SetSelected(config.GatherFiber);
            harvestHideCheckBox.SetSelected(config.GatherHide);
            harvestStoneCheckBox.SetSelected(config.GatherStone);

            killMobsCheckBox.SetSelected(config.AttackMobs);
            autoLoginCheckbox.SetSelected(config.AutoRelogin);

            characterNameInput.SetText(config.LoginCharacterName);

            config.TypeSetsToUse.Clear();
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

            if (config.TypeSetsToUse.Count == 0)
            {
                context.State = "No type sets to gather!";
                return;
            }

            config.GatherWood = harvestWoodCheckBox.IsSelected();
            config.GatherOre = harvestOreCheckBox.IsSelected();
            config.GatherFiber = harvestFiberCheckBox.IsSelected();
            config.GatherHide = harvestHideCheckBox.IsSelected();
            config.GatherStone = harvestStoneCheckBox.IsSelected();

            config.AttackMobs = killMobsCheckBox.IsSelected();
            config.AutoRelogin = autoLoginCheckbox.IsSelected();
            config.LoginCharacterName = characterNameInput.GetText();
            config.ResourceArea = new SafeMapArea(config.ResourceClusterName, new Vector3f(-10000, -10000, -10000), new Vector3f(10000, 10000, 10000));
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
                primaryPanel.SetSize(300, 320);
                primaryPanel.SetPosition(155, (screenSize.Y / 2), 0);
                primaryPanel.SetAnchor(new Vector2f(0.0f, 0.0f), new Vector2f(0.0f, 0.0f));
                primaryPanel.SetPivot(new Vector2f(0.5f, 0.5f));

                tierLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(tierLabel);
                tierLabel.SetPosition(-60, 145, 0);
                tierLabel.SetSize(100, 25);
                tierLabel.SetText("Tier");

                harvestWoodInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestWoodInput);
                harvestWoodInput.SetPosition(-70, 125, 0);
                harvestWoodInput.SetSize(120, 25);

                harvestWoodCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestWoodCheckBox);
                harvestWoodCheckBox.SetPosition(60, 125, 0);
                harvestWoodCheckBox.SetSize(100, 25);
                harvestWoodCheckBox.SetText("Harvest Wood");
                harvestWoodCheckBox.SetSelected(true);

                harvestOreInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestOreInput);
                harvestOreInput.SetPosition(-70, 100, 0);
                harvestOreInput.SetSize(120, 25);

                harvestOreCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestOreCheckBox);
                harvestOreCheckBox.SetPosition(60, 100, 0);
                harvestOreCheckBox.SetSize(100, 25);
                harvestOreCheckBox.SetText("Harvest Ore");
                harvestOreCheckBox.SetSelected(true);

                harvestFiberInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestFiberInput);
                harvestFiberInput.SetPosition(-70, 75, 0);
                harvestFiberInput.SetSize(120, 25);

                harvestFiberCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestFiberCheckBox);
                harvestFiberCheckBox.SetPosition(60, 75, 0);
                harvestFiberCheckBox.SetSize(100, 25);
                harvestFiberCheckBox.SetText("Harvest Fiber");
                harvestFiberCheckBox.SetSelected(true);

                harvestHideInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestHideInput);
                harvestHideInput.SetPosition(-70, 50, 0);
                harvestHideInput.SetSize(120, 25);

                harvestHideCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestHideCheckBox);
                harvestHideCheckBox.SetPosition(60, 50, 0);
                harvestHideCheckBox.SetSize(100, 25);
                harvestHideCheckBox.SetText("Harvest Hide");
                harvestHideCheckBox.SetSelected(true);

                harvestStoneInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestStoneInput);
                harvestStoneInput.SetPosition(-70, 25, 0);
                harvestStoneInput.SetSize(120, 25);

                harvestStoneCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestStoneCheckBox);
                harvestStoneCheckBox.SetPosition(60, 25, 0);
                harvestStoneCheckBox.SetSize(100, 25);
                harvestStoneCheckBox.SetText("Harvest Stone");
                harvestStoneCheckBox.SetSelected(true);

                killMobsCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(killMobsCheckBox);
                killMobsCheckBox.SetPosition(-70, -5, 0);
                killMobsCheckBox.SetSize(125, 25);
                killMobsCheckBox.SetText("Kill Mobs");
                killMobsCheckBox.SetSelected(true);

                autoLoginCheckbox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(autoLoginCheckbox);
                autoLoginCheckbox.SetPosition(60, -5, 0);
                autoLoginCheckbox.SetSize(125, 25);
                autoLoginCheckbox.SetText("Auto Relogin");
                autoLoginCheckbox.SetSelected(true);

                characterNameLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(characterNameLabel);
                characterNameLabel.SetPosition(70, -35, 0);
                characterNameLabel.SetSize(125, 25);
                characterNameLabel.SetText("Character Name");

                characterNameInput = Factories.CreateGuiInputField();
                primaryPanel.Add(characterNameInput);
                characterNameInput.SetPosition(70, -55, 0);
                characterNameInput.SetSize(125, 25);

                setVaultAreaButton = Factories.CreateGuiButton();
                primaryPanel.Add(setVaultAreaButton);
                setVaultAreaButton.SetPosition(-70, -35, 0);
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
                        config.VaultClusterName = Game.ClusterName;
                        config.VaultDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.VaultArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
                    }
                });

                setRepairAreaButton = Factories.CreateGuiButton();
                primaryPanel.Add(setRepairAreaButton);
                setRepairAreaButton.SetPosition(-70, -65, 0);
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
                        config.RepairClusterName = Game.ClusterName;
                        config.RepairDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.RepairArea = new SafeMapArea(Game.ClusterName, new Area(area.Start, area.End));
                    }
                });

                addRoamPointButton = Factories.CreateGuiButton();
                primaryPanel.Add(addRoamPointButton);
                addRoamPointButton.SetPosition(-70, -95, 0);
                addRoamPointButton.SetSize(125, 25);
                addRoamPointButton.SetText("Add Roam Point");
                addRoamPointButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.ThreadSafeLocation;
                        Logging.Log("Add roam point " + loc.X + " " + loc.Y + " " + loc.Z);
                        config.ResourceClusterName = Game.ClusterName;
                        config.RoamPoints.Add(new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z)));
                    }
                });

                removeRoamPointButton = Factories.CreateGuiButton();
                primaryPanel.Add(removeRoamPointButton);
                removeRoamPointButton.SetPosition(60, -95, 0);
                removeRoamPointButton.SetSize(125, 25);
                removeRoamPointButton.SetText("Del Roam Point");
                removeRoamPointButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.ThreadSafeLocation;
                        Logging.Log("Delete roam point " + loc.X + " " + loc.Y + " " + loc.Z);
                        for (var i = 0; i < config.RoamPoints.Count; i++)
                        {
                            if (config.RoamPoints[i].RealVector3().Expand(3, 3, 3).Contains(loc))
                            {
                                config.RoamPoints.RemoveAt(i);
                                i -= 1;
                            }
                        }
                    }
                });

                runButton = Factories.CreateGuiButton();
                primaryPanel.Add(runButton);
                runButton.SetPosition(0, -140, 0);
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

        public override int OnLoop(IScriptEngine se)
        {
            var lpo = Players.LocalPlayer;
            if (lpo != null)
            {
                var max = lpo.MaxCarryWeight;
                if (max >= config.MaxHoldWeight)
                {
                    config.MaxHoldWeight = max;
                }
            }
            return 100;
        }
    }
}
