/*
 * Represents the family data view. Contains a filter control, an editable list
 * and chart controls.
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


using Microsoft.FamilyShowLib;

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

        string ageFilter;
        string surnameFilter;
        string birthdateFilter;
        string livingFilter;
        string genderFilter;

        public FamilyData()
        {
            InitializeComponent();

            // Get the data that is bound to the list.
            CollectionViewSource source = new CollectionViewSource();
            source.Source = App.Family;

            FamilyEditorGrid.ItemsSource = source.View;
            FamilyEditor.ItemsSource = source.View;

            // When the family changes we'll update things in this view
            App.Family.ContentChanged += new EventHandler<Microsoft.FamilyShowLib.ContentChangedEventArgs>(OnFamilyContentChanged);

            // Setup the binding to the chart controls.
            ListCollectionView tagCloudView = CreateView("LastName", "LastName");
            tagCloudView.Filter = new Predicate<object>(TagCloudFilter);
            TagCloudControl.View = tagCloudView;

            ListCollectionView histogramView = CreateView("AgeGroup", "AgeGroup");
            histogramView.Filter = new Predicate<object>(HistogramFilter);
            AgeDistributionControl.View = histogramView;
            AgeDistributionControl.CategoryLabels.Add(AgeGroup.Youth, Properties.Resources.AgeGroupYouth);
            AgeDistributionControl.CategoryLabels.Add(AgeGroup.Adult, Properties.Resources.AgeGroupAdult);
            AgeDistributionControl.CategoryLabels.Add(AgeGroup.MiddleAge, Properties.Resources.AgeGroupMiddleAge);
            AgeDistributionControl.CategoryLabels.Add(AgeGroup.Senior, Properties.Resources.AgeGroupSenior);

            BirthdaysControl.PeopleCollection = App.Family;

            //Gender bar chart
            ListCollectionView histogramView3 = CreateView("Gender", "Gender");
            GenderDistributionControl1.View = histogramView3;
            GenderDistributionControl1.CategoryLabels.Add(Gender.Male, Properties.Resources.Male);
            GenderDistributionControl1.CategoryLabels.Add(Gender.Female, Properties.Resources.Female);


            //Living bar chart
            ListCollectionView histogramView2 = CreateView("IsLiving", "IsLiving");
            LivingDistributionControl1.View = histogramView2;
            LivingDistributionControl1.CategoryLabels.Add(false, Properties.Resources.Deceased);
            LivingDistributionControl1.CategoryLabels.Add(true, Properties.Resources.Living);

            //Ensure all column widths are wide enough to display the header and the sort icon.
            LoadColumnsSettings();
            UpdateColumnWidths();

        }

        private static ListCollectionView CreateView(string group, string sort)
        {
            ListCollectionView view = new ListCollectionView(App.Family);

            // Apply sorting
            if (!string.IsNullOrEmpty(sort))
                view.SortDescriptions.Add(new SortDescription(sort, ListSortDirection.Ascending));

            // Group the collection into tags. The tag cloud will be based on the group Name and ItemCount
            PropertyGroupDescription groupDescription = new PropertyGroupDescription();
            if (!string.IsNullOrEmpty(group))
                groupDescription.PropertyName = group;
            view.GroupDescriptions.Add(groupDescription);

            return view;
        }

        /// <summary>
        /// Used as a filter predicate to see if the person should be included 
        /// </summary>
        /// <param name="o">Person object</param>
        /// <returns>True if the person should be included in the filter, otherwise false</returns>
        public static bool TagCloudFilter(object o)
        {
            Person p = o as Person;
            return (!string.IsNullOrEmpty(p.LastName));
        }

        /// <summary>
        /// Used as a filter predicate to see if the person should be included 
        /// </summary>
        /// <param name="o">Person object</param>
        /// <returns>True if the person should be included in the filter, otherwise false</returns>
        public static bool HistogramFilter(object o)
        {
            Person p = o as Person;
            return (p.AgeGroup != AgeGroup.Unknown);
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
            TagCloudControl.Refresh();
            AgeDistributionControl.Refresh();
            SharedBirthdays.Refresh();
            LivingDistributionControl1.Refresh();
            GenderDistributionControl1.Refresh();
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
            if (e.OriginalSource is TextBox || e.OriginalSource is CheckBox)
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
        /// Updates the column widths
        /// </summary>
        private void UpdateColumnWidths()
        {
            CalculateColumnWidth(NamesMenu, Names);
            CalculateColumnWidth(CitationMenu, Citation);
            CalculateColumnWidth(PhotosMenu, Photo);
            CalculateColumnWidth(NotesMenu, Note);
            CalculateColumnWidth(AttachmentsMenu, Attachment);
            CalculateColumnWidth(RestrictionMenu, Restriction);
            CalculateColumnWidth(SurnameMenu, Surname);
            CalculateColumnWidth(SuffixMenu, Suffix);
            CalculateColumnWidth(AgeMenu, Age);
            CalculateColumnWidth(ImagesMenu, Image);
            CalculateColumnWidth(BirthDateMenu, BirthDate);
            CalculateColumnWidth(BirthPlaceMenu, BirthPlace);
            CalculateColumnWidth(DeathDateMenu, DeathDate);
            CalculateColumnWidth(DeathPlaceMenu, DeathPlace);
            CalculateColumnWidth(IsLivingMenu, IsLiving);
            CalculateColumnWidth(OccupationMenu, Occupation);
            CalculateColumnWidth(EducationMenu, Education);
            CalculateColumnWidth(ReligionMenu, Religion);
            CalculateColumnWidth(BurialPlaceMenu, BurialPlace);
            CalculateColumnWidth(BurialDateMenu, BurialDate);
            CalculateColumnWidth(CremationPlaceMenu, CremationPlace);
            CalculateColumnWidth(CremationDateMenu, CremationDate);
            CalculateColumnWidth(IDMenu, ID);
        }

        /// <summary>
        /// Update which columns are visible.
        /// </summary>
        private void UpdateColumnsVisible(object sender, RoutedEventArgs e)
        {
            UpdateColumnsSettings();
            UpdateColumnWidths();
        }

        /// <summary>
        /// Update user settings for columns displayed in the list view.
        /// </summary>
        private void UpdateColumnsSettings()
        {

            Properties.Settings.Default.ColumnName = NamesMenu.IsChecked;
            Properties.Settings.Default.ColumnCitations = CitationMenu.IsChecked;
            Properties.Settings.Default.ColumnNotes = NotesMenu.IsChecked;
            Properties.Settings.Default.ColumnRestriction = RestrictionMenu.IsChecked;
            Properties.Settings.Default.ColumnAttachments = AttachmentsMenu.IsChecked;
            Properties.Settings.Default.ColumnImage = ImagesMenu.IsChecked;
            Properties.Settings.Default.ColumnPhotos = PhotosMenu.IsChecked;
            Properties.Settings.Default.ColumnSurname = SurnameMenu.IsChecked;
            Properties.Settings.Default.ColumnDateOfBirth = BirthDateMenu.IsChecked;
            Properties.Settings.Default.ColumnDateOfDeath = DeathDateMenu.IsChecked;
            Properties.Settings.Default.ColumnDeathPlace = DeathPlaceMenu.IsChecked;
            Properties.Settings.Default.ColumnBirthPlace = BirthPlaceMenu.IsChecked;
            Properties.Settings.Default.ColumnCremationPlace = CremationPlaceMenu.IsChecked;
            Properties.Settings.Default.ColumnCremationDate = CremationDateMenu.IsChecked;
            Properties.Settings.Default.ColumnID = IDMenu.IsChecked;
            Properties.Settings.Default.ColumnAge = AgeMenu.IsChecked;
            Properties.Settings.Default.ColumnLiving = IsLivingMenu.IsChecked;
            Properties.Settings.Default.ColumnSuffix = SuffixMenu.IsChecked;
            Properties.Settings.Default.ColumnReligion = ReligionMenu.IsChecked;
            Properties.Settings.Default.ColumnEducation = EducationMenu.IsChecked;
            Properties.Settings.Default.ColumnOccupation = OccupationMenu.IsChecked;
            Properties.Settings.Default.ColumnBurialPlace = BurialDateMenu.IsChecked;
            Properties.Settings.Default.ColumnBurialDate = BurialPlaceMenu.IsChecked;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Load user settings for columns displayed in the list view.
        /// </summary>
        private void LoadColumnsSettings()
        {
            NamesMenu.IsChecked = Properties.Settings.Default.ColumnName;
            RestrictionMenu.IsChecked = Properties.Settings.Default.ColumnRestriction;
            CitationMenu.IsChecked = Properties.Settings.Default.ColumnCitations;
            NotesMenu.IsChecked = Properties.Settings.Default.ColumnNotes;
            AttachmentsMenu.IsChecked = Properties.Settings.Default.ColumnAttachments;
            ImagesMenu.IsChecked = Properties.Settings.Default.ColumnImage;
            PhotosMenu.IsChecked = Properties.Settings.Default.ColumnPhotos;
            SurnameMenu.IsChecked = Properties.Settings.Default.ColumnSurname;
            BirthDateMenu.IsChecked = Properties.Settings.Default.ColumnDateOfBirth;
            DeathDateMenu.IsChecked = Properties.Settings.Default.ColumnDateOfDeath;
            DeathPlaceMenu.IsChecked = Properties.Settings.Default.ColumnDeathPlace;
            BirthPlaceMenu.IsChecked = Properties.Settings.Default.ColumnBirthPlace;
            CremationPlaceMenu.IsChecked = Properties.Settings.Default.ColumnCremationPlace;
            CremationDateMenu.IsChecked = Properties.Settings.Default.ColumnCremationDate;
            IDMenu.IsChecked = Properties.Settings.Default.ColumnID;
            AgeMenu.IsChecked = Properties.Settings.Default.ColumnAge;
            IsLivingMenu.IsChecked = Properties.Settings.Default.ColumnLiving;
            SuffixMenu.IsChecked = Properties.Settings.Default.ColumnSuffix;
            ReligionMenu.IsChecked = Properties.Settings.Default.ColumnReligion;
            EducationMenu.IsChecked = Properties.Settings.Default.ColumnEducation;
            OccupationMenu.IsChecked = Properties.Settings.Default.ColumnOccupation;
            BurialPlaceMenu.IsChecked = Properties.Settings.Default.ColumnBurialPlace;
            BurialDateMenu.IsChecked = Properties.Settings.Default.ColumnBurialDate;
        }

        /// <summary>
        /// Sets the column width based on the length of the title.
        /// Required for localization.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private static double CalculateColumnWidth(MenuItem menu, SortListViewColumn columnName)
        {
            if (menu.IsChecked && columnName.Header != null)
            {
                string s = columnName.Header.ToString();
                int i = s.Length;
                int ii = 8;
                columnName.Width = (i * ii) + 25;
                return columnName.Width;
            }
            else if (menu.IsChecked && columnName.Header == null)
            {
                columnName.Width = 24;
                return 24;
            }
            else
            {
                columnName.Width = 0;
                return 0;
            }

        }

        /// <summary>
        /// The filter text changed, update the list based on the new filter.
        /// </summary>
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FamilyEditor.FilterList(FilterTextBox.Text);
            if (FamilyEditor.Items.Count > 0)
            {
                FamilyEditor.ScrollIntoView(FamilyEditor.Items[0]);
            }

            UpdateControls(FilterTextBox.Text);
        }

        /// <summary>
        /// Allow the analytic user controls to reset their selections when the filter is reset
        /// </summary>
        void FilterTextBox_ResetFilter(object sender, RoutedEventArgs e)
        {
            TagCloudControl.ClearSelection();
            AgeDistributionControl.ClearSelection();
            BirthdaysControl.ClearSelection();
            GenderDistributionControl1.ClearSelection();
            LivingDistributionControl1.ClearSelection();

            surnameFilter = null;
            ageFilter = null;
            birthdateFilter = null;
            livingFilter = null;
            genderFilter = null;
            scrollToTop();

        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void TagCloudControl_TagSelectionChanged(object sender, RoutedEventArgs e)
        {

            string filter = e.OriginalSource as string;
            surnameFilter = filter;
            if (filter != null)
                UpdateFilter(filter);
        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void AgeDistributionControl_CategorySelectionChanged(object sender, RoutedEventArgs e)
        {

            string filter = e.OriginalSource as string;
            ageFilter = filter;
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
                birthdateFilter = date.ToShortDateString();
                UpdateFilter(date.ToShortDateString());
            }
        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void GenderDistributionControl1_CategorySelectionChanged(object sender, RoutedEventArgs e)
        {

            string filter = e.OriginalSource as string;
            genderFilter = filter;
            if (filter != null)
                UpdateFilter(filter);
        }

        /// <summary>
        /// Selection changed in the chart, update the filter.
        /// </summary>
        void LivingDistributionControl1_CategorySelectionChanged(object sender, RoutedEventArgs e)
        {

            string filter = e.OriginalSource as string;
            livingFilter = filter;
            if (filter != null)
                UpdateFilter(filter);
        }

        /// <summary>
        /// Update the list based on the filter.
        /// </summary>
        private void UpdateFilter(string filter)
        {
            FilterTextBox.Text = filter;
            UpdateControls(filter);
        }

        /// <summary>
        /// Remove the highlight from the data panels controls when the filter text changes
        /// </summary>
        private void UpdateControls(string filter)
        {
            if (ageFilter != filter)
                AgeDistributionControl.ClearSelection();
            if (surnameFilter != filter)
                TagCloudControl.ClearSelection();
            if (birthdateFilter != filter)
                BirthdaysControl.ClearSelection();
            if (livingFilter != filter)
                LivingDistributionControl1.ClearSelection();
            if (genderFilter != filter)
                GenderDistributionControl1.ClearSelection();
        }

        /// <summary>
        /// Try to scroll to the top of the list on filter text change
        /// Don't use autoscroll option in xaml as this disables the 
        /// virtual list slowing the program significantly.
        /// </summary>
        private void scrollToTop()
        {
            try
            {
                FamilyEditor.ScrollIntoView(FamilyEditor.Items[0]);
            }
            catch { }
        }

    }
}
