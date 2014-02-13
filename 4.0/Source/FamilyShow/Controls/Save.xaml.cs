using System;
using System.Windows;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Save.xaml
    /// </summary>
    public partial class Save : System.Windows.Controls.UserControl
    {
        public Save()
        {
            InitializeComponent();
            Clear();
        }

        #region routed events

        public static readonly RoutedEvent SaveButtonClickEvent = EventManager.RegisterRoutedEvent(
            "SaveButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Save));

        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Save));

        // Expose this event for this control's container
        public event RoutedEventHandler SaveButtonClick
        {
            add { AddHandler(SaveButtonClickEvent, value); }
            remove { RemoveHandler(SaveButtonClickEvent, value); }
        }

        public event RoutedEventHandler CancelButtonClick
        {
            add { AddHandler(CancelButtonClickEvent, value); }
            remove { RemoveHandler(CancelButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SaveButtonClickEvent));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent));
        }

        private void Ancestors_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Option4.IsChecked = true;
        }

        private void Descendants_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Option4.IsChecked = true;
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Get the selected options
        /// </summary>
        public string Options()
        {
            {
                string choice = "0";
                if (Option1.IsChecked == true)
                    choice = "1";
                if (Option2.IsChecked == true)
                    choice = "2";
                if (Option3.IsChecked == true)
                    choice = "3";
                if (Option4.IsChecked == true)
                    choice = "4";
                return choice;
            }
        }

        public decimal Ancestors()
        {
            return Convert.ToDecimal(AncestorsComboBox.Text);
        }

        public decimal Descendants()
        {
            return Convert.ToDecimal(DescendantsComboBox.Text);
        }

        public bool Privacy()
        {
            if (PrivacySave.IsChecked == true)
                return true;
            else
                return false;
        }

        public void Clear()
        {
            DescendantsComboBox.SelectedIndex = 0;
            AncestorsComboBox.SelectedIndex = 0;
            PrivacySave.IsChecked = false;
            Option1.IsChecked = true;
        }

        #endregion

    }
}