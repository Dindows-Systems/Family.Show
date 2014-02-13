using System;
using System.Globalization;
using System.Windows;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : System.Windows.Controls.UserControl
    {
        public About()
        {
            InitializeComponent();
            DisplayVersion();
        }

        #region routed events

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(About));

        // Expose this event for this control's container
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Display the application version.
        /// </summary>
        private void DisplayVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Content += string.Format(CultureInfo.CurrentCulture, 
                "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void Homepage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Open the CodePlex website in the user's default browser
            try
            {
                System.Diagnostics.Process.Start("http://familyshow.codeplex.com/");
            }
            catch { }
        }

        private void Discussion_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Open the CodePlex discussion website in the user's default browser
            try
            {
                System.Diagnostics.Process.Start("http://familyshow.codeplex.com/Thread/List.aspx");
            }
            catch { }
        }

        private void People_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Open the CodePlex people in the user's default browser
            try
            {
                System.Diagnostics.Process.Start("http://familyshow.codeplex.com/team/view");
            }
            catch { }
        }

        private void Vertigo_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Open the Vertigo website in the user's default browser
            try
            {
                System.Diagnostics.Process.Start("http://www.vertigo.com/familyshow.aspx");
            }
            catch { }
        }

        #endregion

    }
}