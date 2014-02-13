using System;
using System.IO;
using System.Windows;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Html.xaml
    /// </summary>
    public partial class Html: System.Windows.Controls.UserControl
    {

        #region fields

        People familyCollection = App.FamilyCollection;
        PeopleCollection family = App.Family;
        SourceCollection source = App.Sources;
        RepositoryCollection repository = App.Repositories;
        public int minYear = DateTime.Now.Year;

        #endregion

        public Html()
        {
            InitializeComponent();
            searchfield.SelectedIndex = 0;  //set name as default filter
            Option1.IsChecked = true;  //set the default choice to be All people
        }

        #region routed events

        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Html));

        // Expose this event for this control's container
        public event RoutedEventHandler CancelButtonClick
        {
            add { AddHandler(CancelButtonClickEvent, value); }
            remove { RemoveHandler(CancelButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent));
            Export();
            Clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent));
            Clear();
        }

        private void Ancestors_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Option4.IsChecked = true;
        }

        private void Descendants_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Option4.IsChecked = true;
        }

        private void searchfield_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Option5.IsChecked = true;
        }

        private void searchtext_TextChanged(object sender, RoutedEventArgs e)
        {
            Option5.IsChecked = true; 
        }

        private void Option6_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Option6.IsChecked == true)
            {
                SourcesHtml.IsEnabled = false;
            }
            else
            {
                SourcesHtml.IsEnabled = true;
            }
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Get the selected options
        /// </summary>
        private string Options()
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
                if (Option5.IsChecked == true)
                    choice = "5";
                if (Option6.IsChecked == true)
                    choice = "6";
                return choice;
            }
        }

        private bool Privacy()
        {
            if (PrivacyHtml.IsChecked == true)
                return true;
            else
                return false;
        }

        private bool Sources()
        {
            if (SourcesHtml.IsChecked == true)
                return true;
            else
                return false;
        }

        private decimal Ancestors()
        {
            return Convert.ToDecimal(AncestorsComboBox.Text);
        }

        private decimal Descendants()
        {
            return Convert.ToDecimal(DescendantsComboBox.Text);
        }

        private string searchtextvalue()
        {
            return searchtext.Text;
        }

        private string searchfieldvalue()
        {
            return searchfield.Text;
        }

        private int searchfieldindex()
        {
            return searchfield.SelectedIndex;
        }

        private void Clear()
        {
            DescendantsComboBox.SelectedIndex = 0;
            AncestorsComboBox.SelectedIndex = 0;
            searchfield.SelectedIndex = 0;
            searchtext.Clear();
            PrivacyHtml.IsChecked = false;
            SourcesHtml.IsChecked = false;
            Option1.IsChecked = true;
        }

        private void Export()
        {

            if (Options() != "0") //only run if cancel not clicked
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = People.ApplicationFolderPath;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.htmlFiles, Properties.Resources.htmlExtension));
                dialog.Title = Properties.Resources.Export;
                dialog.DefaultExtension = Properties.Resources.DefaulthtmlExtension;
                dialog.ShowSave();

                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    //return without doing anything if no file name is input
                }
                else
                {
                    if (!string.IsNullOrEmpty(dialog.FileName))
                    {
                        HtmlExport html = new HtmlExport();

                        int start = minYear;
                        int end = DateTime.Now.Year;

                        string filename = dialog.FileName;
                        if (Options() == "1")
                            html.ExportAll(family, source, repository, dialog.FileName, Path.GetFileName(familyCollection.FullyQualifiedFilename), Privacy(), Sources());  //Export the all individuals
                        if (Options() == "2")
                            html.ExportCurrent(family, source, repository, dialog.FileName, Path.GetFileName(familyCollection.FullyQualifiedFilename), Privacy(), Sources());
                        if (Options() == "3")
                            html.ExportDirect(family, source, repository, dialog.FileName, Path.GetFileName(familyCollection.FullyQualifiedFilename), Privacy(), Sources());     //Export current person and immediate family relatives 
                        if (Options() == "4")
                            html.ExportGenerations(family, source, repository, Ancestors(), Descendants(), dialog.FileName, Path.GetFileName(familyCollection.FullyQualifiedFilename), Privacy(), Sources());
                        if (Options() == "5")
                            html.ExportFilter(family, source, repository, searchtextvalue(), searchfieldvalue(), searchfieldindex(), dialog.FileName, Path.GetFileName(familyCollection.FullyQualifiedFilename), Privacy(), Sources());
                        if (Options() == "6")
                            html.ExportEventsByDecade(family, source, repository, dialog.FileName, Path.GetFileName(familyCollection.FullyQualifiedFilename), Privacy(), start, end);
                        
                        MessageBoxResult result = MessageBox.Show(Properties.Resources.SourcesExportMessage, Properties.Resources.ExportResult, MessageBoxButton.YesNo, MessageBoxImage.Question);

                        try
                        {
                            if (result == MessageBoxResult.Yes)
                                System.Diagnostics.Process.Start(filename);
                        }
                        catch { }

                    }
                }
            }
        }

        #endregion

    }
}