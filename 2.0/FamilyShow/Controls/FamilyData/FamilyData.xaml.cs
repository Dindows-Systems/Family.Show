/*
 * Represents the family data view. Contains a filter control, an editable list
 * and chart controls.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
    public partial class FamilyData : System.Windows.Controls.UserControl
    {
        // Event that is raised when the Back button is clicked.
        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FamilyData));

        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        public FamilyData()
        {
            InitializeComponent();

            // Get the data that is bound to the list.
            CollectionViewSource source = new CollectionViewSource();
            source.Source = App.Family;
            FamilyEditor.ItemsSource = source.View;

            // When the family changes we'll update things in this view
            App.Family.ContentChanged += new EventHandler<Microsoft.FamilyShowLib.ContentChangedEventArgs>(OnFamilyContentChanged);

            // Setup the binding to the chart controls.
            TagCloudControl.TagsCollection = App.Family;
            AgeDistributionControl.SeriesCollection = App.Family;
            BirthdaysControl.PeopleCollection = App.Family;
        }

        /// <summary>
        /// Set focus to the default control.
        /// </summary>
        public void SetDefaultFocus()
        {
            FilterTextBox.Focus();
        }

        /// <summary>
        /// Refresh the chart controls.
        /// </summary>
        public void Refresh()
        {
            TagCloud.Refresh();
            AgeDistributionControl.Refresh();
            SharedBirthdays.Refresh();
        }

        void OnFamilyContentChanged(object sender, Microsoft.FamilyShowLib.ContentChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// A control lost focus. Refresh the chart controls if a cell was updated.
        /// </summary>
        void FamilyEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox)
                Refresh();
        }

        /// <summary>
        /// The back button was clicked, raise event.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Family.OnContentChanged();
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }

        /// <summary>
        /// The filter text changed, update the list based on the new filter.
        /// </summary>
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FamilyEditor.FilterList(FilterTextBox.Text);
        }

        /// <summary>
        /// Allow the analytic user controls to reset their selections when the filter is reset
        /// </summary>
        void FilterTextBox_ResetFilter(object sender, RoutedEventArgs e)
        {
            TagCloudControl.ClearSelection();
            AgeDistributionControl.ClearSelection();
            BirthdaysControl.ClearSelection();
        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void TagCloudControl_TagSelectionChanged(object sender, RoutedEventArgs e)
        {
            string filter = e.OriginalSource as string;
            if (filter != null)
                UpdateFilter(filter);
        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void AgeDistributionControl_CategorySelectionChanged(object sender, RoutedEventArgs e)
        {
            string filter = e.OriginalSource as string;
            if (filter != null)
                UpdateFilter(filter);
        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void BirthdaysControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is DateTime)
            {
                DateTime date = (DateTime)e.OriginalSource;
                UpdateFilter(date.ToShortDateString());
            }
        }

        /// <summary>
        /// Update the list based on the filter.
        /// </summary>
        private void UpdateFilter(string filter)
        {
            FilterTextBox.Text = filter;
        }
    }
}
