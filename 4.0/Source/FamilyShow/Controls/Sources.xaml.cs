using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Sources.xaml
    /// </summary>
    public partial class Sources : System.Windows.Controls.UserControl
    {

        #region fields

        People familyCollection = App.FamilyCollection;
        PeopleCollection family = App.Family;
        SourceCollection source = App.Sources;
        RepositoryCollection repository = App.Repositories;

        #endregion

        public Sources()
        {
            InitializeComponent();
            SourceRepository.Content = String.Empty;  //remove the place holder text on load
        }

        #region routed events

        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Sources));

        public event RoutedEventHandler CancelButtonClick
        {
            add { AddHandler(CancelButtonClickEvent, value); }
            remove { RemoveHandler(CancelButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Add();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent));
        }

        private void ExportSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            Export();
        }

        private void SourcesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Change();
        }

        #endregion

        #region helper methods

        private void Change()
        {
            if (SourcesCombobox.SelectedItem != null)
            {
                Source s = (Source)SourcesCombobox.SelectedItem;
                SourceNameEditTextBox.Text = s.SourceName;
                SourceAuthorEditTextBox.Text = s.SourceAuthor;
                SourcePublisherEditTextBox.Text = s.SourcePublisher;
                //SourceNoteEditTextBox.Text = s.SourceNote;
                SourceRepositoryEditTextBox.Text = s.SourceRepository;

                try
                {
                    SourceRepository.Content = "(" + repository.Find(SourceRepositoryEditTextBox.Text).RepositoryName + ")";
                }
                catch
                {
                    SourceRepository.Content = string.Empty;
                }
            }
        }

        private void Export()
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.htmlFiles, Properties.Resources.htmlExtension));
            dialog.Title = Properties.Resources.Export;
            dialog.DefaultExtension = Properties.Resources.DefaulthtmlExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                SourcesExport sources = new SourcesExport();
                sources.ExportSources(dialog.FileName, Path.GetFileName(this.familyCollection.FullyQualifiedFilename), source);
            }

            if (File.Exists(dialog.FileName))
            {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.SourcesExportMessage,
               Properties.Resources.ExportResult, MessageBoxButton.YesNo, MessageBoxImage.Question);

                try
                {
                    if (result == MessageBoxResult.Yes)
                        System.Diagnostics.Process.Start(dialog.FileName);
                }
                catch { }
            }
        }

        private void Clear()
        {
        
            SourcesCombobox.SelectedIndex = -1;

            SourceNameEditTextBox.Text=string.Empty;
            SourceAuthorEditTextBox.Text=string.Empty;
            SourcePublisherEditTextBox.Text=string.Empty;
            //SourceNoteEditTextBox.Text = string.Empty;
            SourceRepositoryEditTextBox.Text = string.Empty;
            SourceRepository.Content = string.Empty;
        }

        private void Add()
        {
            int y = 0;

            string oldSourceIDs = string.Empty;

            foreach (Source s in source)
            {
                oldSourceIDs += s.Id + "E";
            }

            do
            {
                y++;
            }
            while (oldSourceIDs.Contains("S" + y.ToString() + "E"));

            string sourceID = "S" + y.ToString();

            Source newSource = new Source(sourceID, "", "", "", "", "");
            source.Add(newSource);
            source.OnContentChanged();
        }

        private void Save()
        {

            if (SourcesCombobox.Items != null && SourcesCombobox.Items.Count > 0 && SourcesCombobox.SelectedItem != null)
            {
                Source s = (Source)SourcesCombobox.SelectedItem;
                
                s.SourceName = SourceNameEditTextBox.Text;
                s.SourceAuthor = SourceAuthorEditTextBox.Text;
                s.SourcePublisher = SourcePublisherEditTextBox.Text;
                //s.SourceNote = SourceNoteEditTextBox.Text;
                s.SourceRepository = SourceRepositoryEditTextBox.Text;
                s.OnPropertyChanged("SourceNameAndId");
            }
        }

        private void Delete()
        {
            if (SourcesCombobox.Items != null && SourcesCombobox.Items.Count > 0 && SourcesCombobox.SelectedItem != null)
            {
                Source sourceToRemove = (Source)SourcesCombobox.SelectedItem;

                bool deletable = true;

                foreach (Person p in family)
                {

                    if (deletable == true)
                    {
                        if (p.HasSpouse)
                        {
                            foreach (Relationship rel in p.Relationships)
                            {
                                if (rel.RelationshipType == RelationshipType.Spouse)
                                {
                                    SpouseRelationship spouseRel = (SpouseRelationship)rel;

                                    if (spouseRel.MarriageSource != null)
                                    {
                                        if(spouseRel.MarriageSource == sourceToRemove.Id)
                                            deletable=false;
                                    }

                                    if (spouseRel.DivorceSource != null)
                                    {
                                        if(spouseRel.DivorceSource == sourceToRemove.Id)
                                            deletable=false;
                                    }

                                }
                            }

                        }

                        if (p.BirthSource == sourceToRemove.Id ||
                            p.DeathSource == sourceToRemove.Id ||
                            p.EducationSource == sourceToRemove.Id ||
                            p.EducationSource == sourceToRemove.Id ||
                            p.OccupationSource == sourceToRemove.Id ||
                            p.ReligionSource == sourceToRemove.Id ||
                            p.BurialCitation == sourceToRemove.Id ||
                            p.CremationSource == sourceToRemove.Id
                            )
                            deletable = false;
                    }
                    else { }
  
                    
                }

                if (deletable == true)
                {
                    MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeleteSource,
               Properties.Resources.Source, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        source.Remove(sourceToRemove);
                        source.OnContentChanged();
                        Clear();
                    }
                }
                else
                    MessageBox.Show(Properties.Resources.UnableDeleteSource1 + " " + sourceToRemove.Id + " " + Properties.Resources.UnableDeleteSource2 ,Properties.Resources.Source, MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }

        private void SourceRepositoryEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SourceRepository.Content = "("+ repository.Find(SourceRepositoryEditTextBox.Text).RepositoryName + ")";
            }
            catch
            {
                SourceRepository.Content= string.Empty;
            }
        }

        #endregion
    }
}