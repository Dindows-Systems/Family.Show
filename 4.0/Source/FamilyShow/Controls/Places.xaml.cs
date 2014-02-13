using System;
using System.IO;
using System.Windows;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Places.xaml
    /// </summary>
    public partial class Places: System.Windows.Controls.UserControl
    {

        #region fields

        People familyCollection = App.FamilyCollection;
        PeopleCollection family = App.Family;
        SourceCollection source = App.Sources;
        RepositoryCollection repository = App.Repositories;
        public int minYear = DateTime.Now.Year;

        #endregion

        public Places()
        {
            InitializeComponent();
            Option1.IsChecked = true;  //set the default choice to be All people
        }

        #region routed events

        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Places));

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

        private void Option3_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Option3.IsChecked == true)
            {
                BirthsCheckBox.IsEnabled = false;
                DeathsCheckBox.IsEnabled = false;
                MarriagesCheckBox.IsEnabled = false;
                CremationsCheckBox.IsEnabled = false;
                BurialsCheckBox.IsEnabled = false;
            }
            else
            {
                BirthsCheckBox.IsEnabled = true;
                DeathsCheckBox.IsEnabled = true;
                MarriagesCheckBox.IsEnabled = true;
                CremationsCheckBox.IsEnabled = true;
                BurialsCheckBox.IsEnabled = true;
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
                return choice;
            }
        }

        private bool Privacy()
        {
            if (PrivacyPlaces.IsChecked == true)
                return true;
            else
                return false;
        }

        private bool Births()
        {
            if (BirthsCheckBox.IsChecked == true)
                return true;
            else
                return false;
        }

        private bool Deaths()
        {
            if (DeathsCheckBox.IsChecked == true)
                return true;
            else
                return false;
        }

        private bool Marriages()
        {
            if (MarriagesCheckBox.IsChecked == true)
                return true;
            else
                return false;
        }

        private bool Burials()
        {
            if (BurialsCheckBox.IsChecked == true)
                return true;
            else
                return false;
        }

        private bool Cremations()
        {
            if (CremationsCheckBox.IsChecked == true)
                return true;
            else
                return false;
        }


       
        private void Clear()
        {    
            PrivacyPlaces.IsChecked = false;
            Option1.IsChecked = true;

            BirthsCheckBox.IsEnabled = true;
            DeathsCheckBox.IsEnabled = true;
            MarriagesCheckBox.IsEnabled = true;
            CremationsCheckBox.IsEnabled = true;
            BurialsCheckBox.IsEnabled = true;
        }

        private void Export()
        {

            if (Options() != "0") //only run if cancel not clicked
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = People.ApplicationFolderPath;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.htmlFiles, Properties.Resources.kmlExtension));
                dialog.Title = Properties.Resources.Export;
                dialog.DefaultExtension = Properties.Resources.DefaultkmlExtension;
                dialog.ShowSave();

                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    //return without doing anything if no file name is input
                }
                else
                {
                    if (!string.IsNullOrEmpty(dialog.FileName))
                    {
                        PlacesExport places = new PlacesExport();

                        string filename = dialog.FileName;

                        string[] summary = null;

                        if (Options() == "1")
                           summary = places.ExportPlaces(family,filename,Privacy(),false,false,true,Burials(),Deaths(),Cremations(),Births(),Marriages());
                        if (Options() == "2")
                            summary = places.ExportPlaces(family, filename, Privacy(), true, false, false, Burials(), Deaths(), Cremations(), Births(), Marriages());
                        if (Options() == "3")
                            summary = places.ExportPlaces(family, filename, Privacy(), false, true, false, Burials(), Deaths(), Cremations(), Births(), Marriages());


                        if (summary[1] == "No file")
                        {
                            MessageBoxResult result = MessageBox.Show(summary[0],
                             Properties.Resources.ExportResult, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show(summary[0] + "\n\n" + Properties.Resources.PlacesMessage,
                             Properties.Resources.ExportResult, MessageBoxButton.YesNo, MessageBoxImage.Information);

                            if (result == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    System.Diagnostics.Process.Start(summary[1]);
                                }
                                catch
                                {
                                    //no viewer or other error
                                }
                            }
                        } 
                    }
                }
            }
        }

        #endregion

    }
}