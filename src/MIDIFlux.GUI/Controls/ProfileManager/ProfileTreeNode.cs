using System.Windows.Forms;
using MIDIFlux.GUI.Models;

namespace MIDIFlux.GUI.Controls.ProfileManager
{
    /// <summary>
    /// Represents a node in the profile tree view
    /// </summary>
    public class ProfileTreeNode : TreeNode
    {
        /// <summary>
        /// Gets or sets the profile associated with this node
        /// </summary>
        public ProfileModel? Profile { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node represents a directory
        /// </summary>
        public bool IsDirectory => Profile == null;

        /// <summary>
        /// Gets a value indicating whether this node represents a profile
        /// </summary>
        public bool IsProfile => Profile != null;

        /// <summary>
        /// Creates a new instance of the ProfileTreeNode class for a directory
        /// </summary>
        /// <param name="directoryName">The name of the directory</param>
        public ProfileTreeNode(string directoryName)
            : base(directoryName)
        {
            // Set the directory icon
            ImageIndex = 0;
            SelectedImageIndex = 0;
        }

        /// <summary>
        /// Creates a new instance of the ProfileTreeNode class for a profile
        /// </summary>
        /// <param name="profile">The profile</param>
        public ProfileTreeNode(ProfileModel profile)
            : base(profile.ToString())
        {
            Profile = profile;

            // Set the profile icon
            ImageIndex = 1;
            SelectedImageIndex = 1;

            // Update the node appearance based on the profile state
            UpdateAppearance();
        }

        /// <summary>
        /// Updates the appearance of the node based on the profile state
        /// </summary>
        public void UpdateAppearance()
        {
            if (Profile != null)
            {
                // Update the text
                Text = Profile.ToString();

                // Update the icon based on the active state
                ImageIndex = Profile.IsActive ? 2 : 1;
                SelectedImageIndex = Profile.IsActive ? 2 : 1;

                // Update the node font based on the active state
                if (Profile.IsActive)
                {
                    // Use bold font for active profile
                    NodeFont = new System.Drawing.Font(TreeView?.Font ?? System.Drawing.SystemFonts.DefaultFont, System.Drawing.FontStyle.Bold);

                    // Set a background color to highlight the active profile
                    BackColor = System.Drawing.Color.LightGreen;
                }
                else
                {
                    // Reset font and background color for non-active profiles
                    NodeFont = null;
                    BackColor = System.Drawing.Color.Empty;
                }
            }
        }
    }
}

