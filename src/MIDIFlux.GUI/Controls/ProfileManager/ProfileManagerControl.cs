using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.GUI.Controls.Common;
using MIDIFlux.GUI.Controls.ProfileEditor;
using MIDIFlux.GUI.Dialogs;
using MIDIFlux.GUI.Helpers;
using MIDIFlux.GUI.Models;
using MIDIFlux.GUI.Services;
using MIDIFlux.GUI.Services.Import;
using MIDIFlux.GUI.Services.Import.Models;

namespace MIDIFlux.GUI.Controls.ProfileManager
{
    /// <summary>
    /// User control for managing profiles
    /// </summary>
    public partial class ProfileManagerControl : BaseTabUserControl
    {
        private readonly ActionConfigurationLoader _configLoader;
        private readonly ImageList _imageList;
        private MidiProcessingServiceProxy _midiProcessingServiceProxy;
        private ProfileModel? _activeProfile;
        private string _searchText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileManagerControl"/> class
        /// </summary>
        /// <param name="logger">The logger to use for this control</param>
        /// <param name="configurationService">The configuration service</param>
        /// <param name="actionConfigurationLoader">The action configuration loader</param>
        public ProfileManagerControl(ILogger<ProfileManagerControl> logger, ConfigurationService configurationService, ActionConfigurationLoader actionConfigurationLoader) : base(logger)
        {
            try
            {
                _logger.LogDebug("Initializing ProfileManagerControl");

                // Initialize the component
                InitializeComponent();

                _logger.LogDebug("InitializeComponent completed successfully");
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error in InitializeComponent: {Message}", ex.Message);

                // Rethrow the exception to be handled by the parent form
                throw;
            }

            // Set the tab title
            TabTitle = "Profile Manager";

            // Store the injected dependencies
            _configLoader = actionConfigurationLoader ?? throw new ArgumentNullException(nameof(actionConfigurationLoader));

            // MidiProcessingServiceProxy will be initialized in OnLoad when the parent form is available
            // If this fails, the control should not work and should show clear error messages
            _midiProcessingServiceProxy = null!;

            // Create an image list for the tree view with standard Windows icons
            _imageList = new ImageList();
            _imageList.Images.Add(System.Drawing.SystemIcons.Information.ToBitmap()); // Index 0: Folder
            _imageList.Images.Add(System.Drawing.SystemIcons.WinLogo.ToBitmap()); // Index 1: Profile
            _imageList.Images.Add(System.Drawing.SystemIcons.Shield.ToBitmap()); // Index 2: Active profile

            // Set up the tree view
            profileTreeView.ImageList = _imageList;
            profileTreeView.AfterSelect += ProfileTreeView_AfterSelect;
            profileTreeView.NodeMouseDoubleClick += ProfileTreeView_NodeMouseDoubleClick;

            // Set up the search box
            searchTextBox.TextChanged += SearchTextBox_TextChanged;

            // Set up the buttons
            newButton.Click += NewButton_Click;
            duplicateButton.Click += DuplicateButton_Click;
            deleteButton.Click += DeleteButton_Click;
            editButton.Click += EditButton_Click;
            activateButton.Click += ActivateButton_Click;
            openFolderButton.Click += OpenFolderButton_Click;
            importMidiKey2KeyButton.Click += ImportMidiKey2KeyButton_Click;

            // Add refresh button functionality
            refreshButton.Click += RefreshButton_Click;

            // Note: LoadProfiles() will be called in OnLoad after the proxy is initialized
        }

        /// <summary>
        /// Override OnLoad to initialize the MidiProcessingServiceProxy when the parent form is available
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Try to get the configured proxy from the parent form
            InitializeMidiProcessingServiceProxy();

            // Now that the proxy is initialized, load the profiles
            LoadProfiles();
        }

        /// <summary>
        /// Initializes the MidiProcessingServiceProxy from the parent ConfigurationForm
        /// </summary>
        private void InitializeMidiProcessingServiceProxy()
        {
            _logger.LogDebug("Attempting to get MidiProcessingServiceProxy from parent form");

            // Get the parent ConfigurationForm
            var configForm = FindForm() as Forms.ConfigurationForm;

            if (configForm == null)
            {
                var errorMessage = "CRITICAL ERROR: ProfileManagerControl is not hosted in a ConfigurationForm. This control can only be used within the Configuration GUI opened from the main MIDIFlux application.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            _logger.LogDebug("Found parent ConfigurationForm");

            // Get the configured proxy from the form
            var configuredProxy = configForm.GetMidiProcessingServiceProxy();
            if (configuredProxy == null)
            {
                var errorMessage = "CRITICAL ERROR: ConfigurationForm does not have a configured MidiProcessingServiceProxy. This indicates the Configuration GUI was not properly initialized by the main MIDIFlux application.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            _logger.LogInformation("Successfully retrieved configured MidiProcessingServiceProxy from parent form");
            _midiProcessingServiceProxy = configuredProxy;
        }

        /// <summary>
        /// Loads all profiles from the profiles directory
        /// </summary>
        private void LoadProfiles()
        {
            // Clear the tree view
            profileTreeView.Nodes.Clear();

            // Get the profiles directory
            string profilesDir = ProfileHelper.GetProfilesDirectory();
            ProfileHelper.EnsureDirectoriesExist();

            // Check for the active profile from the MidiProcessingServiceProxy
            string? activeProfilePath = _midiProcessingServiceProxy.GetActiveConfigurationPath();
            if (!string.IsNullOrEmpty(activeProfilePath) && File.Exists(activeProfilePath))
            {
                _activeProfile = new ProfileModel(activeProfilePath, profilesDir);
            }

            // If no active profile is found, try to load from current.json
            if (_activeProfile == null)
            {
                // Check if current.json exists
                string currentConfigPath = Path.Combine(AppDataHelper.GetAppDataDirectory(), "current.json");
                if (File.Exists(currentConfigPath))
                {
                    // Try to find the original profile
                    var profileFiles = Directory.GetFiles(profilesDir, "*.json", SearchOption.AllDirectories);
                    foreach (var profileFile in profileFiles)
                    {
                        if (profileFile.Equals(currentConfigPath, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Compare the contents of the files
                        if (FilesAreEqual(profileFile, currentConfigPath))
                        {
                            _activeProfile = new ProfileModel(profileFile, profilesDir);
                            _activeProfile.IsActive = true;
                            break;
                        }
                    }
                }
            }

            // Create the root node
            var rootNode = new TreeNode("Profiles");
            rootNode.ImageIndex = 0;
            rootNode.SelectedImageIndex = 0;
            profileTreeView.Nodes.Add(rootNode);

            // Load profiles from the root directory
            LoadProfilesFromDirectory(profilesDir, rootNode);

            // Expand the root node
            rootNode.Expand();

            // Auto-select the active profile if one exists
            SelectActiveProfileInTree();

            // Update the UI
            UpdateUI();
        }

        /// <summary>
        /// Automatically selects the active profile in the tree view to enable buttons
        /// </summary>
        private void SelectActiveProfileInTree()
        {
            if (_activeProfile == null)
                return;

            try
            {
                // Find the active profile node in the tree
                var activeNode = FindProfileNodeInTree(profileTreeView.Nodes, _activeProfile);
                if (activeNode != null)
                {
                    // Select the active profile node
                    profileTreeView.SelectedNode = activeNode;

                    // Ensure the node is visible
                    activeNode.EnsureVisible();

                    _logger.LogDebug("Auto-selected active profile '{ProfileName}' in tree view", _activeProfile.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error auto-selecting active profile in tree view");
            }
        }

        /// <summary>
        /// Recursively finds a profile node in the tree view
        /// </summary>
        /// <param name="nodes">The collection of nodes to search</param>
        /// <param name="targetProfile">The profile to find</param>
        /// <returns>The matching ProfileTreeNode, or null if not found</returns>
        private ProfileTreeNode? FindProfileNodeInTree(TreeNodeCollection nodes, ProfileModel targetProfile)
        {
            foreach (TreeNode node in nodes)
            {
                if (node is ProfileTreeNode profileNode && profileNode.IsProfile && profileNode.Profile != null)
                {
                    // Check if this is the target profile by comparing file paths
                    if (profileNode.Profile.FilePath.Equals(targetProfile.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return profileNode;
                    }
                }

                // Recursively search child nodes
                var foundNode = FindProfileNodeInTree(node.Nodes, targetProfile);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        /// <summary>
        /// Compares two files to see if they have the same content
        /// </summary>
        /// <param name="file1">The first file</param>
        /// <param name="file2">The second file</param>
        /// <returns>True if the files have the same content, false otherwise</returns>
        private bool FilesAreEqual(string file1, string file2)
        {
            try
            {
                // Quick check - if the file sizes are different, they're not equal
                var info1 = new FileInfo(file1);
                var info2 = new FileInfo(file2);
                if (info1.Length != info2.Length)
                {
                    return false;
                }

                // Read the files and compare their contents
                string content1 = File.ReadAllText(file1);
                string content2 = File.ReadAllText(file2);
                return content1 == content2;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads profiles from a directory and adds them to the specified parent node
        /// </summary>
        /// <param name="directoryPath">The directory path</param>
        /// <param name="parentNode">The parent node</param>
        private void LoadProfilesFromDirectory(string directoryPath, TreeNode parentNode)
        {
            try
            {
                // Get all JSON files in the directory
                var profileFiles = Directory.GetFiles(directoryPath, "*.json");

                // Get the profiles directory for calculating relative paths
                string profilesDir = ProfileHelper.GetProfilesDirectory();

                // Add each profile to the tree
                foreach (var profileFile in profileFiles)
                {
                    // Skip the current.json file
                    if (Path.GetFileName(profileFile).Equals("current.json", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Create a profile model
                    var profile = new ProfileModel(profileFile, profilesDir);

                    // Check if this is the active profile
                    if (_activeProfile != null && profile.FilePath.Equals(_activeProfile.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        profile.IsActive = true;
                    }

                    // Create a tree node for the profile
                    var profileNode = new ProfileTreeNode(profile);

                    // Add the profile node to the parent node
                    parentNode.Nodes.Add(profileNode);
                }

                // Get all subdirectories
                var subdirectories = Directory.GetDirectories(directoryPath);

                // Add each subdirectory to the tree
                foreach (var subdirectory in subdirectories)
                {
                    // Get the directory name
                    string directoryName = Path.GetFileName(subdirectory);

                    // Create a tree node for the directory
                    var directoryNode = new ProfileTreeNode(directoryName);

                    // Add the directory node to the parent node
                    parentNode.Nodes.Add(directoryNode);

                    // Recursively load profiles from the subdirectory
                    LoadProfilesFromDirectory(subdirectory, directoryNode);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading profiles: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the UI based on the current selection
        /// </summary>
        private void UpdateUI()
        {
            // Get the selected node
            var selectedNode = profileTreeView.SelectedNode as ProfileTreeNode;

            // Enable/disable buttons based on the selection
            duplicateButton.Enabled = selectedNode?.IsProfile ?? false;
            deleteButton.Enabled = selectedNode?.IsProfile ?? false;
            editButton.Enabled = selectedNode?.IsProfile ?? false;
            activateButton.Enabled = selectedNode?.IsProfile ?? false &&
                                    (selectedNode?.Profile?.IsActive ?? false) == false;

            // Update the status label
            if (_activeProfile != null)
            {
                statusLabel.Text = $"Active Profile: {_activeProfile.Name}";
            }
            else
            {
                statusLabel.Text = "No active profile";
            }
        }

        /// <summary>
        /// Filters the tree view based on the search text
        /// </summary>
        private void FilterTreeView()
        {
            // If the search text is empty, show all nodes
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                // Show all nodes
                ShowAllNodes(profileTreeView.Nodes);
                return;
            }

            // Hide all nodes first
            HideAllNodes(profileTreeView.Nodes);

            // Show nodes that match the search text
            ShowMatchingNodes(profileTreeView.Nodes, _searchText);

            // Expand all nodes
            profileTreeView.ExpandAll();
        }

        /// <summary>
        /// Shows all nodes in the tree view
        /// </summary>
        /// <param name="nodes">The collection of nodes</param>
        private void ShowAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Text = node.Text.Replace("<b>", "").Replace("</b>", "");
                node.ForeColor = SystemColors.WindowText;
                node.BackColor = SystemColors.Window;

                // Show the node
                node.EnsureVisible();

                // Recursively show all child nodes
                ShowAllNodes(node.Nodes);
            }
        }

        /// <summary>
        /// Hides all nodes in the tree view
        /// </summary>
        /// <param name="nodes">The collection of nodes</param>
        private void HideAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                // Reset the node text
                node.Text = node.Text.Replace("<b>", "").Replace("</b>", "");

                // Hide the node
                node.ForeColor = Color.Gray;

                // Recursively hide all child nodes
                HideAllNodes(node.Nodes);
            }
        }

        /// <summary>
        /// Shows nodes that match the search text
        /// </summary>
        /// <param name="nodes">The collection of nodes</param>
        /// <param name="searchText">The search text</param>
        /// <returns>True if any node matches the search text</returns>
        private bool ShowMatchingNodes(TreeNodeCollection nodes, string searchText)
        {
            bool anyMatches = false;

            foreach (TreeNode node in nodes)
            {
                // Check if this node matches the search text
                bool nodeMatches = node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                // Check if any child nodes match the search text
                bool childrenMatch = ShowMatchingNodes(node.Nodes, searchText);

                // If this node or any of its children match, show this node
                if (nodeMatches || childrenMatch)
                {
                    // Show the node
                    node.ForeColor = SystemColors.WindowText;
                    node.BackColor = SystemColors.Window;

                    // If this node matches, highlight the matching text
                    if (nodeMatches)
                    {
                        // Highlight the matching text
                        int index = node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                        string before = node.Text.Substring(0, index);
                        string match = node.Text.Substring(index, searchText.Length);
                        string after = node.Text.Substring(index + searchText.Length);
                        node.Text = before + match + after;

                        // Set the node color
                        node.BackColor = Color.LightYellow;
                    }

                    anyMatches = true;
                }
            }

            return anyMatches;
        }

        #region Event Handlers

        /// <summary>
        /// Handles the AfterSelect event of the ProfileTreeView
        /// </summary>
        private void ProfileTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the NodeMouseDoubleClick event of the ProfileTreeView
        /// </summary>
        private void ProfileTreeView_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            // If the node is a profile, activate it
            if (e.Node is ProfileTreeNode profileNode && profileNode.IsProfile)
            {
                if (profileNode.Profile != null)
                {
                    ActivateProfile(profileNode.Profile);
                }
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the SearchTextBox
        /// </summary>
        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            _searchText = searchTextBox.Text.Trim();
            FilterTreeView();
        }

        /// <summary>
        /// Handles the Click event of the NewButton
        /// </summary>
        private void NewButton_Click(object? sender, EventArgs e)
        {
            // Prompt for a profile name
            string profileName = "New Profile";
            using (var inputDialog = new TextInputDialog("New Profile", "Enter a name for the new profile:", profileName))
            {
                if (inputDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                profileName = inputDialog.InputText;
            }

            // Create a new profile
            CreateNewProfile(profileName);
        }

        /// <summary>
        /// Handles the Click event of the DuplicateButton
        /// </summary>
        private void DuplicateButton_Click(object? sender, EventArgs e)
        {
            // Get the selected node
            var selectedNode = profileTreeView.SelectedNode as ProfileTreeNode;
            if (selectedNode?.IsProfile != true || selectedNode.Profile == null)
            {
                ShowError("Please select a profile to duplicate");
                return;
            }

            // Get the source profile
            var sourceProfile = selectedNode.Profile;

            // Prompt for a profile name
            string profileName = $"Copy of {sourceProfile.Name}";
            using (var inputDialog = new TextInputDialog("Duplicate Profile", "Enter a name for the duplicated profile:", profileName))
            {
                if (inputDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                profileName = inputDialog.InputText;
            }

            // Duplicate the profile
            DuplicateProfile(sourceProfile, profileName);
        }

        /// <summary>
        /// Handles the Click event of the DeleteButton
        /// </summary>
        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            // Get the selected node
            var selectedNode = profileTreeView.SelectedNode as ProfileTreeNode;
            if (selectedNode?.IsProfile != true || selectedNode.Profile == null)
            {
                ShowError("Please select a profile to delete");
                return;
            }

            // Get the profile to delete
            var profileToDelete = selectedNode.Profile;

            // Confirm deletion
            if (!ShowConfirmation($"Are you sure you want to delete the profile '{profileToDelete.Name}'?", "Delete Profile"))
            {
                return;
            }

            // Delete the profile
            DeleteProfile(profileToDelete);
        }

        /// <summary>
        /// Handles the Click event of the ActivateButton
        /// </summary>
        private void ActivateButton_Click(object? sender, EventArgs e)
        {
            // Get the selected node
            var selectedNode = profileTreeView.SelectedNode as ProfileTreeNode;

            // If the node is a profile, activate it
            if (selectedNode?.IsProfile ?? false)
            {
                ActivateProfile(selectedNode.Profile);
            }
        }

        /// <summary>
        /// Handles the Click event of the EditButton
        /// </summary>
        private void EditButton_Click(object? sender, EventArgs e)
        {
            // Get the selected node
            var selectedNode = profileTreeView.SelectedNode as ProfileTreeNode;
            if (selectedNode?.IsProfile != true || selectedNode.Profile == null)
            {
                ShowError("Please select a profile to edit");
                return;
            }

            // Edit the profile
            if (selectedNode.Profile != null)
            {
                EditProfile(selectedNode.Profile);
            }
        }

        /// <summary>
        /// Handles the Click event of the OpenFolderButton
        /// </summary>
        private void OpenFolderButton_Click(object? sender, EventArgs e)
        {
            // Open the profiles directory in Windows Explorer
            try
            {
                var profilesDir = ProfileHelper.GetProfilesDirectory();
                System.Diagnostics.Process.Start("explorer.exe", profilesDir);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open profiles directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Click event of the RefreshButton
        /// </summary>
        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Reload all profiles from the file system
                LoadProfiles();
                ShowMessage("Profile list refreshed successfully");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to refresh profile list: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Click event of the ImportMidiKey2KeyButton
        /// </summary>
        private async void ImportMidiKey2KeyButton_Click(object? sender, EventArgs e)
        {
            try
            {
                using var importDialog = new Dialogs.MidiKey2KeyImportDialog();
                if (importDialog.ShowDialog(this) == DialogResult.OK)
                {
                    await PerformImportAsync(importDialog.SelectedFilePath, importDialog.ProfileName);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error opening import dialog: {ex.Message}", "Import Error", ex);
            }
        }

        #endregion

        /// <summary>
        /// Performs the MIDIKey2Key import operation
        /// </summary>
        /// <param name="iniFilePath">Path to the MIDIKey2Key INI file</param>
        /// <param name="profileName">Name for the imported profile</param>
        private async Task PerformImportAsync(string iniFilePath, string profileName)
        {
            try
            {
                // Show progress message
                statusLabel.Text = "Importing MIDIKey2Key configuration...";
                Application.DoEvents();

                // Create import options
                var options = new ImportOptions
                {
                    ProfileName = profileName,
                    SkipTrainSimulatorFeatures = true,
                    ConvertSysExToWildcards = true,
                    OutputDirectory = ProfileHelper.GetProfilesDirectory()
                };

                // Create importer and perform import
                var importer = new MidiKey2KeyImporter();
                var result = await importer.ImportConfigurationAsync(iniFilePath, options);

                // Handle the result
                if (result.Success)
                {
                    // Show success message with statistics
                    var message = $"Import completed successfully!\n\n" +
                                  $"Profile saved to: {result.OutputFilePath}\n\n" +
                                  $"Statistics:\n" +
                                  $"• Total actions found: {result.Statistics.TotalActionsFound}\n" +
                                  $"• Actions converted: {result.Statistics.ActionsConverted}\n" +
                                  $"• Actions skipped: {result.Statistics.ActionsSkipped}\n" +
                                  $"• Actions failed: {result.Statistics.ActionsFailed}\n" +
                                  $"• Keyboard actions: {result.Statistics.KeyboardActionsCreated}\n" +
                                  $"• Command executions: {result.Statistics.CommandExecutionsCreated}";

                    if (result.Warnings.Count > 0)
                    {
                        message += $"\n\nWarnings ({result.Warnings.Count}):\n";
                        foreach (var warning in result.Warnings.Take(5)) // Show first 5 warnings
                        {
                            message += $"• {warning.Message}\n";
                        }
                        if (result.Warnings.Count > 5)
                        {
                            message += $"• ... and {result.Warnings.Count - 5} more warnings\n";
                        }
                    }

                    ShowMessage(message, "Import Successful");

                    // Reload profiles to show the new imported profile
                    LoadProfiles();
                }
                else
                {
                    // Show error message
                    var errorMessage = "Import failed:\n\n";
                    foreach (var error in result.Errors.Take(5)) // Show first 5 errors
                    {
                        errorMessage += $"• {error.Message}\n";
                    }
                    if (result.Errors.Count > 5)
                    {
                        errorMessage += $"• ... and {result.Errors.Count - 5} more errors\n";
                    }

                    ShowError(errorMessage, "Import Failed");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Unexpected error during import: {ex.Message}", "Import Error", ex);
            }
            finally
            {
                // Reset status
                statusLabel.Text = _activeProfile != null ? $"Active Profile: {_activeProfile.Name}" : "No active profile";
            }
        }

        /// <summary>
        /// Creates a new profile with the specified name
        /// </summary>
        /// <param name="profileName">The name of the profile</param>
        private void CreateNewProfile(string profileName)
        {
            try
            {
                // Get the profiles directory
                string profilesDir = ProfileHelper.GetProfilesDirectory();
                ProfileHelper.EnsureDirectoriesExist();

                // Create a file name from the profile name
                string fileName = profileName.Replace(" ", "_") + ".json";
                string filePath = Path.Combine(profilesDir, fileName);

                // Check if the file already exists
                if (File.Exists(filePath))
                {
                    ShowError($"A profile with the name '{profileName}' already exists");
                    return;
                }

                // Create an empty configuration
                var config = new MappingConfig
                {
                    ProfileName = profileName,
                    Description = $"Empty MIDIFlux profile: {profileName}",
                    MidiDevices = new List<DeviceConfig>()
                };

                // Save the configuration using unified system
                if (!_configLoader.SaveConfiguration(config, filePath))
                {
                    ShowError($"Failed to save profile '{profileName}'");
                    return;
                }

                // Reload the profiles
                LoadProfiles();

                // Show a success message
                ShowMessage($"Created new profile '{profileName}'");
            }
            catch (Exception ex)
            {
                ShowError($"Error creating profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Duplicates the specified profile with a new name
        /// </summary>
        /// <param name="sourceProfile">The source profile</param>
        /// <param name="newProfileName">The new profile name</param>
        private void DuplicateProfile(ProfileModel sourceProfile, string newProfileName)
        {
            try
            {
                // Get the profiles directory
                string profilesDir = ProfileHelper.GetProfilesDirectory();
                ProfileHelper.EnsureDirectoriesExist();

                // Create a file name from the profile name
                string fileName = newProfileName.Replace(" ", "_") + ".json";
                string filePath = Path.Combine(profilesDir, fileName);

                // Check if the file already exists
                if (File.Exists(filePath))
                {
                    ShowError($"A profile with the name '{newProfileName}' already exists");
                    return;
                }

                // Load the source configuration using unified system
                var sourceConfig = _configLoader.LoadConfiguration(sourceProfile.FilePath);
                if (sourceConfig == null)
                {
                    ShowError($"Failed to load source profile '{sourceProfile.Name}'");
                    return;
                }

                // Update the profile name in the duplicated configuration
                sourceConfig.ProfileName = newProfileName;
                sourceConfig.Description = $"Copy of {sourceProfile.Name}";

                // Save the configuration with the new name using unified system
                if (!_configLoader.SaveConfiguration(sourceConfig, filePath))
                {
                    ShowError($"Failed to save profile '{newProfileName}'");
                    return;
                }

                // Reload the profiles
                LoadProfiles();

                // Show a success message
                ShowMessage($"Duplicated profile '{sourceProfile.Name}' as '{newProfileName}'");
            }
            catch (Exception ex)
            {
                ShowError($"Error duplicating profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the specified profile
        /// </summary>
        /// <param name="profile">The profile to delete</param>
        private void DeleteProfile(ProfileModel profile)
        {
            try
            {
                // Check if the profile exists
                if (!profile.Exists)
                {
                    ShowError($"Profile '{profile.Name}' does not exist");
                    return;
                }

                // Check if this is the active profile
                if (profile.IsActive)
                {
                    ShowError($"Cannot delete the active profile '{profile.Name}'");
                    return;
                }

                // Delete the file
                File.Delete(profile.FilePath);

                // Reload the profiles
                LoadProfiles();

                // Show a success message
                ShowMessage($"Deleted profile '{profile.Name}'");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Activates the specified profile
        /// </summary>
        /// <param name="profile">The profile to activate</param>
        private void ActivateProfile(ProfileModel? profile)
        {
            if (profile == null || !profile.Exists)
            {
                ShowError("Cannot activate profile: Profile does not exist");
                return;
            }

            try
            {
                // Check if this profile is already active
                if (_activeProfile != null && _activeProfile.FilePath.Equals(profile.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    ShowMessage($"Profile '{profile.Name}' is already active");
                    return;
                }

                // Perform cleanup for the previously active profile
                if (_activeProfile != null)
                {
                    // Reset the active state
                    _activeProfile.IsActive = false;
                }

                // Load the configuration using unified system
                MappingConfig? config = null;
                try
                {
                    config = _configLoader.LoadConfiguration(profile.FilePath);
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to load profile '{profile.Name}':\n\n{ex.Message}", "MIDIFlux - Profile Load Error", ex);
                    return;
                }

                if (config == null)
                {
                    ShowError($"Failed to load profile '{profile.Name}': The profile file could not be parsed or is invalid.\n\nPlease check the logs for more details.", "MIDIFlux - Profile Load Error");
                    return;
                }

                // Save the configuration as current.json using unified system
                string currentConfigPath = Path.Combine(AppDataHelper.GetAppDataDirectory(), "current.json");
                if (!_configLoader.SaveConfiguration(config, currentConfigPath))
                {
                    ShowError($"Failed to save current configuration");
                    return;
                }

                // No need to save active profile information to a file anymore
                // The MidiProcessingServiceProxy handles this directly

                // Update the active profile
                _activeProfile = profile;
                _activeProfile.IsActive = true;

                // Update the application settings
                ProfileHelper.UpdateLastProfilePath(profile.FilePath);

                // Immediately activate the profile in the main application
                if (_midiProcessingServiceProxy.IsServiceAvailable())
                {
                    if (_midiProcessingServiceProxy.ActivateProfile(profile.FilePath))
                    {
                        ShowMessage($"Activated profile '{profile.Name}' - configuration is now live");
                    }
                    else
                    {
                        ShowWarning($"Profile '{profile.Name}' was saved but could not be activated");
                    }
                }
                else
                {
                    // Fall back to file-based activation
                    if (_midiProcessingServiceProxy.ActivateProfile(profile.FilePath))
                    {
                        ShowMessage($"Activated profile '{profile.Name}' via configuration file");
                    }
                    else
                    {
                        ShowWarning($"Profile '{profile.Name}' was saved but could not be activated");
                    }
                }

                // Reload the profiles to update the UI
                LoadProfiles();
            }
            catch (Exception ex)
            {
                ShowError($"Error activating profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens the profile editor for the specified profile
        /// </summary>
        /// <param name="profile">The profile to edit</param>
        private void EditProfile(ProfileModel profile)
        {
            var logger = LoggingHelper.CreateLoggerForType(GetType());
            try
            {
                // Get the parent form
                var configForm = FindForm() as Forms.ConfigurationForm;
                if (configForm == null)
                {
                    logger.LogError("Could not find parent ConfigurationForm when trying to edit profile: {ProfileName}", profile.Name);
                    ShowError("Could not find parent form");
                    return;
                }

                // Get the MidiProcessingServiceProxy from the parent form
                var midiProcessingServiceProxy = configForm.GetMidiProcessingServiceProxy();
                if (midiProcessingServiceProxy == null)
                {
                    logger.LogError("MidiProcessingServiceProxy is null when trying to edit profile: {ProfileName}", profile.Name);
                    ShowError("Failed to get MIDI processing service proxy");
                    return;
                }

                logger.LogDebug("Creating ProfileEditorControl for profile: {ProfileName}", profile.Name);

                // Create a new unified profile editor control and pass the MidiProcessingServiceProxy
                var profileEditorLogger = LoggingHelper.CreateLogger<Controls.ProfileEditor.ProfileEditorControl>();
                var profileEditorControl = new Controls.ProfileEditor.ProfileEditorControl(profile, profileEditorLogger, _configLoader, midiProcessingServiceProxy);

                // Add it as a tab or activate existing one
                bool newTabCreated = configForm.AddOrActivateProfileEditorTab(profileEditorControl);

                // Show a status message
                if (newTabCreated)
                {
                    statusLabel.Text = $"Editing profile: {profile.Name}";
                    logger.LogInformation("Successfully opened profile editor for: {ProfileName}", profile.Name);
                }
                else
                {
                    statusLabel.Text = $"Activated existing editor for: {profile.Name}";
                    logger.LogInformation("Activated existing profile editor for: {ProfileName}", profile.Name);

                    // Dispose the unused control since we're using an existing tab
                    profileEditorControl.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error opening profile editor for {ProfileName}: {Message}", profile.Name, ex.Message);
                ShowError($"Error opening profile editor: {ex.Message}", "MIDIFlux - Error", ex);
            }
        }

        /// <summary>
        /// Saves any unsaved changes
        /// </summary>
        /// <returns>True if the save was successful, false otherwise</returns>
        public override bool Save()
        {
            // The profile manager doesn't have any unsaved changes
            return true;
        }

        // Note: Error handling methods (ShowError, ShowWarning, ShowMessage, ShowConfirmation, ShowValidationResult)
        // are inherited from BaseUserControl and don't need to be redefined here.
    }
}

