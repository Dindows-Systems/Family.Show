using System;
using System.IO;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Microsoft.FamilyShowLib;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.FamilyShow;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region fields

        // The list of people, sources and repositories. This is a global list shared by the application.
        People familyCollection = App.FamilyCollection;
        PeopleCollection family = App.Family;
        SourceCollection source = App.Sources;
        RepositoryCollection repository = App.Repositories;

        bool hideDiagramControls = false;
        private Properties.Settings appSettings = Properties.Settings.Default;

        #endregion

        public MainWindow()
        {
            InitializeComponent();            
            BuildOpenMenu();
            BuildThemesMenu();
            family.CurrentChanged += new EventHandler(People_CurrentChanged);
            ProcessCommandLines();
        }        

        #region event handlers

        /// <summary>
        /// Event handler when the primary person has changed.
        /// </summary>
        private void People_CurrentChanged(object sender, EventArgs e)
        {
            if (family.Current != null)
                DetailsControl.DataContext = family.Current;  
        }

        private void NewUserControl_AddButtonClick(object sender, RoutedEventArgs e)
        {
            HideNewUserControl();
            ShowDetailsPane();
        }

        private void DetailsControl_PersonInfoClick(object sender, RoutedEventArgs e)
        {
            PersonInfoControl.DataContext = family.Current;
            // Uses an animation to show the Person Info Control
            ((Storyboard)this.Resources["ShowPersonInfo"]).Begin(this);
        }

        private void DetailsControl_FamilyDataClick(object sender, RoutedEventArgs e)
        {
            FamilyDataControl.Refresh();
            // Uses an animation to show the Family Data Control
            ((Storyboard)this.Resources["ShowFamilyData"]).Begin(this);
        }

        /// <summary>
        /// Event handler when all people in the people collection have been deleted.
        /// </summary>
        private void DetailsControl_EveryoneDeleted(object sender, RoutedEventArgs e)
        {
            // Everyone was deleted show the create new user control
            NewFamily(sender, e);
        }

        private void PersonInfoControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            // Uses an animation to hide the Person Info Control
            ((Storyboard)this.Resources["HidePersonInfo"]).Begin(this);
        }

        private void FamilyDataControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            // Uses an animation to hide the Family Data Control
            HideFamilyDataControl();
        }

        private void ShowPersonInfo_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            PersonInfoControl.SetDefaultFocus();
        }

        private void HidePersonInfo_StoryboardCompleted(object sender, EventArgs e)
        {
            this.family.OnContentChanged();
            DetailsControl.SetDefaultFocus();
            enableButtons();
        }

        private void ShowFamilyData_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            FamilyDataControl.SetDefaultFocus();
        }

        private void HideFamilyData_StoryboardCompleted(object sender, EventArgs e)
        {
            DetailsControl.SetDefaultFocus();
            enableButtons();
        }

        private void WelcomeUserControl_NewButtonClick(object sender, RoutedEventArgs e)
        {
            NewFamily(sender, e);
        }

        private void WelcomeUserControl_OpenButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFamily(sender, e);
        }

        private void WelcomeUserControl_ImportButtonClick(object sender, RoutedEventArgs e)
        {
            ImportGedcom(sender, e);
        }

        private void AboutControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            AboutControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void LanguageControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            LanguageControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void StatisticsControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            StatisticsControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void PhotoViewerControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            PhotoViewerControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void AttachmentViewerControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            AttachmentViewerControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void StoryViewerControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            StoryViewerControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void MergeControl_DoneButtonClick(object sender, RoutedEventArgs e)
        {
            MergeControl.Visibility = Visibility.Hidden;
            
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
            dialog.Title = Properties.Resources.SaveAs;
            dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
            dialog.ShowSave();
            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                TaskBar.Current.Loading();
                familyCollection.Save(dialog.FileName);
                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                BuildOpenMenu();
            }
            else
                this.familyCollection.FullyQualifiedFilename = string.Empty;

            this.family.OnContentChanged();
            UpdateStatus();
            TaskBar.Current.Restore();
            removeControlFocus();

        }

        private void SaveControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            SaveControl.Visibility = Visibility.Hidden;
            removeControlFocus();
            SaveControl.Clear();
        }

        private void SaveControl_SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveControl.Visibility = Visibility.Hidden;
            removeControlFocus();
            SaveFamilyAs();
            SaveControl.Clear();
        }

        private void GedcomLocalizationControl_ContinueButtonClick(object sender, RoutedEventArgs e)
        {
            GedcomLocalizationControl.Visibility = Visibility.Hidden;
            appSettings.EnableUTF8 = (bool)GedcomLocalizationControl.EnableUTF8CheckBox.IsChecked;
            appSettings.Save();
            ImportGedcom();
        }

        private void SourcesControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            SourcesControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void DateCalculatorControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DateCalculatorControl.Visibility = Visibility.Hidden;
            removeControlFocus();
        }

        private void RepositoriesControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            RepositoriesControl.Visibility = Visibility.Hidden;
            removeControlFocus();       
        }

        private void HtmlControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            HtmlControl.Visibility = Visibility.Hidden;
            removeControlFocus();
            UpdateStatus();
        }

        private void PlacesControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            PlacesControl.Visibility = Visibility.Hidden;
            removeControlFocus();
            UpdateStatus();
        }

        private void ExtractControl_CancelButtonClick(object sender, RoutedEventArgs e)
        {
            ExtractControl.Visibility = Visibility.Hidden;
            removeControlFocus();
            UpdateStatus();
        }

        private void WelcomeUserControl_OpenRecentFileButtonClick(object sender, RoutedEventArgs e)
        {
            Button item = (Button)e.OriginalSource;
            string file = item.CommandParameter as string;

            if (!string.IsNullOrEmpty(file))
            {
                // Load the selected family file
                
                bool fileLoaded = LoadFamily(file);

                if (fileLoaded)
                {
                    ShowDetailsPane();
                    // This will tell the diagram to redraw and the details panel to update.
                    family.OnContentChanged();
                    // Remove the file from its current position and add it back to the top/most recent position.
                    App.RecentFiles.Remove(file);
                    App.RecentFiles.Insert(0, file);
                    BuildOpenMenu();
                    family.IsDirty = false;
                    UpdateStatus();
                }
                else
                {
                    Title = Properties.Resources.FamilyShow;
                }
            }
        }

        #endregion

        #region menu command handlers

        #region new menu

        private void NewFamily(object sender, RoutedEventArgs e)
        {
            NewFamily();
        }

        #endregion

        #region open menu

        private void OpenFamily(object sender, RoutedEventArgs e)
        {
            OpenFamily();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ImportGedcom(object sender, EventArgs e)
        {
            App.canExecuteJumpList = false;
            removeControlFocus();
            GedcomLocalizationImport();
        }

        private void Merge(object sender, RoutedEventArgs e)
        {

            App.canExecuteJumpList = false;

            string oldFilePath = string.Empty;

            #region prompt to save before merging

            if (family.IsDirty)
            {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.SaveBeforeMerge, Properties.Resources.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if(result == MessageBoxResult.Yes)
                {

                    if (!string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
                    {
                        oldFilePath = familyCollection.FullyQualifiedFilename;
                        familyCollection.Save(familyCollection.FullyQualifiedFilename);
                    }
                    else
                    {

                        CommonDialog dialog = new CommonDialog();
                        dialog.InitialDirectory = People.ApplicationFolderPath;
                        dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
                        dialog.Title = Properties.Resources.SaveAs;
                        dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
                        dialog.ShowSave();

                        if (!string.IsNullOrEmpty(dialog.FileName))
                        {
                            oldFilePath = dialog.FileName;
                            familyCollection.Save(dialog.FileName);
                        }
                    }


                }

                if (result == MessageBoxResult.Cancel)
                {
                    App.canExecuteJumpList = true;
                    return;
                }
            }

            #endregion

            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.MergingStatus;  //Update status bar

            CommonDialog mergedialog = new CommonDialog();
            mergedialog.InitialDirectory = People.ApplicationFolderPath;
            mergedialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
            mergedialog.Title = Properties.Resources.Merge;
            mergedialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
            mergedialog.ShowOpen();

            if (!string.IsNullOrEmpty(mergedialog.FileName))
            {

                Title = Properties.Resources.FamilyShow;
                string[,] summary = MergeFamily(mergedialog.FileName);

                if (summary != null)
                {
                    try
                    {
                        giveControlFocus();

                        MergeControl.summary = summary;

                        if (App.FamilyCollection.ExistingPeopleCollection != null && App.FamilyCollection.DuplicatePeopleCollection != null)
                        {
                            if (App.FamilyCollection.ExistingPeopleCollection.Count > 0 && App.FamilyCollection.DuplicatePeopleCollection.Count > 0)
                            {
                                MergeControl.Visibility = Visibility.Visible;
                                MergeControl.ShowMergeSummary();
                            }
                            else
                            {
                                MergeControl.Visibility = Visibility.Visible;
                                MergeControl.summary = summary;
                                MergeControl.ShowSummary();
                            }
                        }

                        else
                        {
                            MergeControl.Visibility = Visibility.Visible;
                            MergeControl.summary = summary;
                            MergeControl.ShowSummary();
                        }
                        family.OnContentChanged();
                    }
                    catch 
                    {
                        MergeControl.Visibility = Visibility.Hidden;
                        MessageBox.Show(Properties.Resources.MergeExistingError, Properties.Resources.Merge, MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowWelcomeScreen();
                        UpdateStatus();  
                    }

                }
                else
                {
                    //if the merge fails, reload the original file and continue. Prompt the user.
                    if (LoadFamily(oldFilePath))
                        MessageBox.Show(Properties.Resources.MergeFailed1, Properties.Resources.Merge, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        MessageBox.Show(Properties.Resources.MergeFailed2, Properties.Resources.Merge, MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowWelcomeScreen();
                        UpdateStatus();
                    }
                }  
            }
            else
               UpdateStatus();
        }

        private void OpenRecentFile(object sender, RoutedEventArgs e)
        {
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.LoadingStatus;
            MenuItem item = (MenuItem)sender;
            string file = item.CommandParameter as string;

            if (!string.IsNullOrEmpty(file))
            {
                PromptToSave();
                LoadFamily(file);
                ShowDetailsPane();
                // This will tell the diagram to redraw and the details panel to update.
                family.OnContentChanged();
                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(file);
                App.RecentFiles.Insert(0, file);
                BuildOpenMenu();
                family.IsDirty = false;
            }
            UpdateStatus();
            e.Handled = true;
        }
        
        private void ClearRecentFiles(object sender, RoutedEventArgs e)
        {
            App.RecentFiles.Clear();
            App.SaveRecentFiles();
            BuildOpenMenu();
        }

        #endregion 

        #region save menu

        private void SaveFamily(object sender, RoutedEventArgs e)
        {
            App.canExecuteJumpList = false;

            Title = Title = Properties.Resources.FamilyShow + " " + Properties.Resources.SavingStatus;  //Update status bar
            // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
            if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = People.ApplicationFolderPath;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
                dialog.Title = Properties.Resources.SaveAs;
                dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
                dialog.ShowSave();

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    TaskBar.Current.Loading();
                    familyCollection.Save(dialog.FileName);
                    // Remove the file from its current position and add it back to the top/most recent position.
                    App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                    App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                    BuildOpenMenu();
                }
            }
            else
            {
                TaskBar.Current.Loading();
                familyCollection.Save(false);
                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                BuildOpenMenu();
            }
            App.canExecuteJumpList = true;
            TaskBar.Current.Restore();
            UpdateStatus();
        }

        private void SaveFamilyAs(object sender, RoutedEventArgs e)
        {
            giveControlFocus();
            SaveControl.Visibility = Visibility.Visible;
        }

        private void ExportGedcom(object sender, EventArgs e)
        {
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.ExportingStatus;
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
            dialog.Title = Properties.Resources.Export;
            dialog.DefaultExtension = Properties.Resources.DefaultGedcomExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                GedcomExport ged = new GedcomExport();
                try
                {

                    ged.Export(family, source, repository, dialog.FileName, familyCollection.FullyQualifiedFilename, Properties.Resources.Language);
                    MessageBox.Show(this, Properties.Resources.GedcomExportSucessfulMessage,
                        Properties.Resources.Export, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show(this, Properties.Resources.GedcomExportFailedMessage,
                        Properties.Resources.Export, MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            UpdateStatus();
        }

        private void ExportHtml(object sender, RoutedEventArgs e)
        {
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.ExportingStatus;
            giveControlFocus();
            HtmlControl.Visibility = Visibility.Visible;
            HtmlControl.minYear = (int)DiagramControl.TimeSlider.Minimum;
        }

        private void ExportPlaces(object sender, RoutedEventArgs e)
        {
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.ExportingStatus;
            giveControlFocus();
            PlacesControl.Visibility = Visibility.Visible;
        }

        private void ExtractFiles(object sender, RoutedEventArgs e)
        {
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.ExtractingStatus;
            giveControlFocus();
            ExtractControl.Visibility = Visibility.Visible;
        }

        #endregion

        #region tools menu

        private void EditRepositories(object sender, RoutedEventArgs e)
        {
            giveControlFocus();
            RepositoriesControl.Visibility = Visibility.Visible;
            RepositoriesControl.RepositoriesCombobox.ItemsSource = repository;
        }

        private void EditSources(object sender, RoutedEventArgs e)
        {
            giveControlFocus();
            SourcesControl.Visibility = Visibility.Visible;
            SourcesControl.SourcesCombobox.ItemsSource = source;
        }

        private void Statistics(object sender, EventArgs e)
        {
            giveControlFocus();
            StatisticsControl.Visibility = Visibility.Visible;
            StatisticsControl.DisplayStats(family, source, repository);
        }

        private void Photos(object sender, EventArgs e)
        {
            giveControlFocus();
            enableMenus();
            PhotoViewerControl.Visibility = Visibility.Visible; 
            PhotoViewerControl.LoadPhotos(family);

        }

        private void Dates(object sender, EventArgs e)
        {
            giveControlFocus();
            enableMenus();
            DateCalculatorControl.Visibility = Visibility.Visible;
        }

        private void Attachments(object sender, EventArgs e)
        {
            giveControlFocus();
            enableMenus();
            AttachmentViewerControl.Visibility = Visibility.Visible;
            AttachmentViewerControl.LoadAttachments(family);
        }

        private void Storys(object sender, EventArgs e)
        {
            giveControlFocus();
            enableMenus();
            StoryViewerControl.Visibility = Visibility.Visible;
        }


        #endregion

        #region print menu

        private void Print(object sender, RoutedEventArgs e)
        {
                PrintDialog pd = new PrintDialog();

                if ((bool)pd.ShowDialog().GetValueOrDefault())
                {
                    // Hide the zoom control and time control before the diagram is saved
                    DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;
                    DiagramControl.TimeSliderPanel.Visibility = Visibility.Hidden;

                    //Make a stackpanel to hold the contents.
                    StackPanel pageArea = new StackPanel();

                    double padding = 20;
                    double titleheight = 25;
                    double heightActual = 0;
                    double widthActual = 0;

                    //Diagram
                    VisualBrush diagramFill = new VisualBrush();
                    System.Windows.Shapes.Rectangle diagram = new System.Windows.Shapes.Rectangle();

                    //Print background when black theme is used because diagram has white text
                    if (appSettings.Theme == @"Themes\Black\BlackResources.xaml")
                    {
                        heightActual = this.DiagramBorder.ActualHeight;
                        widthActual = this.DiagramBorder.ActualWidth;
                        diagramFill = new VisualBrush(DiagramBorder);
                        diagram.Margin = new Thickness(0, 0, 0, 0);
                        diagram.Fill = diagramFill;
                    }
                    else
                    {
                        heightActual = this.DiagramBorder.ActualHeight;
                        widthActual = this.DiagramBorder.ActualWidth;
                        diagramFill = new VisualBrush(DiagramControl);
                        diagram.Stroke = Brushes.Black;
                        diagram.StrokeThickness = 0.5;
                        diagram.Margin = new Thickness(0, 0, 0, 0);
                        diagram.Fill = diagramFill;
                    }

                    //Titles
                    TextBlock titles = new TextBlock();
                    titles.Height = titleheight;
                    titles.Text = Properties.Resources.ReportHeader1 + " " + App.Family.Current.FullName + " " + Properties.Resources.ReportHeader2 + " " + DiagramControl.YearFilter.Content.ToString();

                    //Scale
                    double scale = Math.Min((pd.PrintableAreaWidth - padding - padding) / widthActual, (pd.PrintableAreaHeight - padding - padding - titleheight) / heightActual);

                    diagram.Width = scale * widthActual;
                    diagram.Height = scale * heightActual;

                    //Page Area
                    pageArea.Margin = new Thickness(padding);
                    pageArea.Children.Add(titles);
                    pageArea.Children.Add(diagram);
                    pageArea.Measure(new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight));
                    pageArea.Arrange(new Rect(new Point(0, 0), pageArea.DesiredSize));

                    pd.PrintVisual(pageArea, App.Family.Current.FullName);
       
                    // Show the zoom control and time control again
                    if (hideDiagramControls == false)
                    {
                        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
                        DiagramControl.TimeSliderPanel.Visibility = Visibility.Visible;
                    }

                }
        }

        private void ExportXps(object sender, EventArgs e)
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.XpsFiles, Properties.Resources.XpsExtension));
            dialog.Title = Properties.Resources.Export;
            dialog.DefaultExtension = Properties.Resources.DefaultXpsExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                try
                {
                    // Create the XPS document from the window's main container (in this case, a grid) 
                    Package package = Package.Open(dialog.FileName, FileMode.Create);
                    XpsDocument xpsDoc = new XpsDocument(package);
                    XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);

                    // Hide the zoom control and time control before the diagram is saved
                    DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;
                    DiagramControl.TimeSliderPanel.Visibility = Visibility.Hidden;

                    // Since DiagramBorder derives from FrameworkElement, the XpsDocument writer knows
                    // how to output it's contents. The border is used instead of the DiagramControl
                    // so that the diagram background is output as well as the digram control itself.

                    xpsWriter.Write(DiagramBorder);
                    xpsDoc.Close();
                    package.Close();

                }

                catch
                { 
                //save as xps fails if saving as an existing file which is open.
                }

                // Show the zoom control and time control again
                if (hideDiagramControls == false)
                {
                    DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
                    DiagramControl.TimeSliderPanel.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region themes menu

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {

            MenuItem item = (MenuItem)sender;
            string theme = item.CommandParameter as string;

            ResourceDictionary rd = new ResourceDictionary();
            rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(theme, UriKind.Relative)) as ResourceDictionary);
            Application.Current.Resources = rd;

            // Save the theme setting
            appSettings.Theme = theme;
            appSettings.Save();

            family.OnContentChanged();
            PersonInfoControl.OnThemeChange();
            UpdateStatus();
            this.DiagramControl.TimeSlider.Value = DateTime.Now.Year;

        }

        #endregion

        #region help menu

        private void About(object sender, EventArgs e)
        {
            giveControlFocus();
            AboutControl.Visibility = Visibility.Visible;
        }

        private void Languages(object sender, EventArgs e)
        {
            giveControlFocus();
            LanguageControl.Visibility = Visibility.Visible;
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).ToString() + @"\" + Microsoft.FamilyShow.Properties.Resources.HelpFileName);
            }
            catch { }
        }

        #endregion

        #endregion

        #region menu command helper methods

        /// <summary>
        /// Starts a new family.
        /// </summary>
        private void NewFamily()
        {
            giveControlFocus();
            ReleasePhotos();

            // Do not prompt for fully saved or welcome screen new families.
            if (!family.IsDirty || (family.IsDirty && family.Count == 0))
            {

                family.Clear();
                source.Clear();
                repository.Clear();

                familyCollection.FullyQualifiedFilename = null;
                family.OnContentChanged();
                ShowNewUserControl();
                family.IsDirty = false;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.NotSavedMessage,
                    Properties.Resources.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {

                    // Prompt to save if the file has not been saved before.
                    if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
                    {
                        CommonDialog dialog = new CommonDialog();
                        dialog.InitialDirectory = People.ApplicationFolderPath;
                        dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
                        dialog.Title = Properties.Resources.SaveAs;
                        dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
                        dialog.ShowSave();

                        if (string.IsNullOrEmpty(dialog.FileName))
                        {
                            // Return without doing anything.
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(dialog.FileName))
                            {
                                familyCollection.Save(dialog.FileName);
                                // Remove the file from its current position and add it back to the top/most recent position.
                                App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                                App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                                BuildOpenMenu();

                                family.Clear();
                                source.Clear();
                                repository.Clear();

                                familyCollection.FullyQualifiedFilename = null;
                                family.OnContentChanged();

                                ShowNewUserControl();
                                family.IsDirty = false;
                            }
                        }

                    }

                    // Otherwise just save to the existing file.
                    else
                    {
                        familyCollection.Save(false);
                        // Remove the file from its current position and add it back to the top/most recent position.
                        App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                        App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                        BuildOpenMenu();

                        family.Clear();
                        source.Clear();
                        repository.Clear();

                        familyCollection.FullyQualifiedFilename = null;
                        family.OnContentChanged();

                        ShowNewUserControl();
                        family.IsDirty = false;
                    }
                }

                if (result == MessageBoxResult.No)
                {
                    family.Clear();
                    source.Clear();
                    repository.Clear();

                    familyCollection.FullyQualifiedFilename = null;
                    family.OnContentChanged();

                    ShowNewUserControl();
                    family.IsDirty = false;
                }

                if (result == MessageBoxResult.Cancel)
                {
                    removeControlFocus();
                }
            }

            UpdateStatus();
        }

        /// <summary>
        /// Opens a familyx file.
        /// </summary>
        private void OpenFamily()
        {
            App.canExecuteJumpList = false;
            bool loaded = true;
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.LoadingStatus;
            PromptToSave();

            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFilesAll, Properties.Resources.FamilyxExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyShowFiles, Properties.Resources.FamilyShowExtensions));
            dialog.Title = Properties.Resources.Open;
            dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                if (Path.GetExtension(dialog.FileName) == Properties.Resources.DefaultFamilyxExtension)
                {
                    loaded = LoadFamily(dialog.FileName);
                }
                else if (Path.GetExtension(dialog.FileName) == Properties.Resources.DefaultFamilyExtension)
                {
                    loaded = LoadVersion2(dialog.FileName);
                }
               
                if (!loaded)
                {
                    ShowWelcomeScreen();
                    UpdateStatus();
                }
                else
                {
                    CollapseDetailsPanels();
                    ShowDetailsPane();
                    family.OnContentChanged();
                }

                TaskBar.Current.Restore();

                // Do not add non default files to recent files list.
                if (familyCollection.FullyQualifiedFilename.EndsWith(Properties.Resources.DefaultFamilyxExtension))
                {
                    // Remove the file from its current position and add it back to the top/most recent position.
                    App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                    App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                    BuildOpenMenu();
                    family.IsDirty = false;
                }
                   
            }

            if (family.Count==0)
                ShowWelcomeScreen();

            UpdateStatus();
            App.canExecuteJumpList = true;

        }

        /// <summary>
        /// Saves a family with and prompts for a file name
        /// </summary>
        private void SaveFamilyAs()
        {
            App.canExecuteJumpList = false;

            if (SaveControl.Options() != "0")
            {
                Title = Title = Properties.Resources.FamilyShow + " " + Properties.Resources.SavingStatus;  //Update status bar
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = People.ApplicationFolderPath;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension)); ;
                dialog.Title = Properties.Resources.SaveAs;
                dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
                dialog.ShowSave();

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    TaskBar.Current.Loading();

                    bool privacy = SaveControl.Privacy();

                    if (SaveControl.Options() == "1")
                        familyCollection.SavePrivacy(dialog.FileName, privacy);
                    if (SaveControl.Options() == "2")
                        familyCollection.SaveCurrent(dialog.FileName, privacy);
                    if (SaveControl.Options() == "3")
                        familyCollection.SaveDirect(dialog.FileName, privacy);
                    if (SaveControl.Options() == "4")
                        familyCollection.SaveGenerations(dialog.FileName, SaveControl.Ancestors(), SaveControl.Descendants(), privacy);     //then save and load the new family
                }
            }

            if (!string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
            {
                if (familyCollection.FullyQualifiedFilename.EndsWith(Properties.Resources.DefaultFamilyxExtension))
                {
                    // Remove the file from its current position and add it back to the top/most recent position.
                    App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                    App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                    BuildOpenMenu();
                    family.IsDirty = false;
                }
            }

            family.OnContentChanged();
            UpdateStatus();
            TaskBar.Current.Restore();
            App.canExecuteJumpList = true;
        }

        /// <summary>
        /// Prompts the user to select encoding option for GEDCOM import.
        /// </summary>
        private void GedcomLocalizationImport()
        {
            App.canExecuteJumpList = false;
            Title = Properties.Resources.FamilyShow + " " + Properties.Resources.ImportingStatus;
            PromptToSave();

            giveControlFocus();
            GedcomLocalizationControl.Visibility = Visibility.Visible;
            GedcomLocalizationControl.EnableUTF8CheckBox.IsChecked = appSettings.EnableUTF8;
        }

        /// <summary>
        /// Imports a selected GEDCOM file.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ImportGedcom()
        {
            App.canExecuteJumpList = false;
            bool loaded = true;

            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
            dialog.Title = Properties.Resources.HeaderImport;
            dialog.DefaultExtension = Properties.Resources.DefaultGedcomExtension;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {

                TaskBar.Current.Loading();
                ReleasePhotos();

                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.ApplicationFolderName);
                tempFolder = Path.Combine(tempFolder, Microsoft.FamilyShowLib.App.AppDataFolderName);

                People.RecreateDirectory(tempFolder);
                People.RecreateDirectory(Path.Combine(tempFolder, Photo.PhotosFolderName));
                People.RecreateDirectory(Path.Combine(tempFolder, Story.StoriesFolderName));
                People.RecreateDirectory(Path.Combine(tempFolder, Attachment.AttachmentsFolderName));

                try
                {
                    GedcomImport ged = new GedcomImport();
                    loaded = ged.Import(family, source, repository, dialog.FileName, appSettings.EnableUTF8);
                    familyCollection.FullyQualifiedFilename = string.Empty;  //file name must be familyx, this ensures user is prompted to save file to familyx
                    family.IsDirty = false;
                }
                catch
                {
                    // Could not import the GEDCOM for some reason. Handle
                    // all exceptions the same, display message and continue
                    // without importing the GEDCOM file.
                    MessageBox.Show(this, Properties.Resources.GedcomFailedMessage, Properties.Resources.GedcomFailed, MessageBoxButton.OK, MessageBoxImage.Error);
                    loaded = false;
                }
            }

            CollapseDetailsPanels();
            ShowDetailsPane();
            family.OnContentChanged();
            TaskBar.Current.Restore();
            UpdateStatus();
            App.canExecuteJumpList = true;

            if (!loaded || family.Count == 0)
            {
                ShowWelcomeScreen();
                UpdateStatus();
            }
        }

        /// <summary>
        /// Load the selected familyx file.
        /// Returns true on sucessful load.
        /// </summary>
        private bool LoadFamily(string fileName)
        {
            giveControlFocus();
            TaskBar.Current.Loading();
            ReleasePhotos();

            familyCollection.FullyQualifiedFilename = fileName;
            bool fileLoaded = familyCollection.LoadOPC();

            if (fileLoaded)
                familyCollection.FullyQualifiedFilename = fileName;
            else
                familyCollection.FullyQualifiedFilename = string.Empty;

            UpdateStatus();

            removeControlFocus();
            TaskBar.Current.Restore();

            return fileLoaded;
        }

        /// <summary>
        /// Load the selected family file.
        /// Returns true on sucessful load.
        /// </summary>
        private bool LoadVersion2(string fileName)
        {
            giveControlFocus();
            TaskBar.Current.Loading();
            ReleasePhotos();

            MessageBox.Show(Properties.Resources.OldVersionMessage, Properties.Resources.Compatability, MessageBoxButton.OK, MessageBoxImage.Information);

            familyCollection.FullyQualifiedFilename = fileName;
            bool fileLoaded = familyCollection.LoadVersion2();

            if (fileLoaded)
            {
                familyCollection.FullyQualifiedFilename = Path.ChangeExtension(fileName, Properties.Resources.DefaultFamilyxExtension);
                SaveFamilyAs();
            }
            else
                familyCollection.FullyQualifiedFilename = string.Empty;


            UpdateStatus();

            removeControlFocus();
            TaskBar.Current.Restore();

            return fileLoaded;
        }

        /// <summary>
        /// Merge the selected familyx file and return a summary on success.
        /// </summary>
        private string[,] MergeFamily(string fileName)
        {
            string[,] summary = familyCollection.MergeOPC(fileName);
            return summary;
        }

        #endregion

        #region diagram commands

        /// <summary>
        /// Command handler for FullScreen On
        /// </summary>
        private void FullScreen_Checked(object sender, EventArgs e)
        {
            DetailsPane.Visibility = Visibility.Collapsed;

            // Remove the cloned columns from layers 0
            if (DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
                DiagramPane.ColumnDefinitions.Remove(column1CloneForLayer0);
        }

        /// <summary>
        /// Command handler for FullScreen Off
        /// </summary>
        private void FullScreen_Unchecked(object sender, EventArgs e)
        {
            if (WelcomeUserControl.Visibility != Visibility.Visible)
            {
                if (!DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
                    DiagramPane.ColumnDefinitions.Add(column1CloneForLayer0);

                if (family.Current != null)
                    DetailsControl.DataContext = family.Current;


                DetailsPane.Visibility = Visibility.Visible;
            }

            DetailsControl.SetDefaultFocus();
        }

        /// <summary>
        /// Command handler for hide controls checked.
        /// </summary>
        private void HideControls_Checked(object sender, EventArgs e)
        {
            if (DiagramBorder.Visibility == Visibility.Visible)
            {
                this.DiagramControl.TimeSliderPanel.Visibility = Visibility.Hidden;
                this.DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;
                hideDiagramControls = true;
            }
        }

        /// <summary>
        /// Command handler for hide controls unchecked.
        /// </summary>
        private void HideControls_Unchecked(object sender, EventArgs e)
        {
            if (DiagramBorder.Visibility == Visibility.Visible)
            {
                this.DiagramControl.TimeSliderPanel.Visibility = Visibility.Visible;
                this.DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
                hideDiagramControls = false;
            }
        }

        #endregion

        #region command line handlers

        /// <summary>
        /// Handles the command line arguments.
        /// This allows *.familyx files to be opened via double click if 
        /// Family.Show is registered as the file handler for *.familyx extensions.
        /// The method also handles the Windows 7 JumpList "Tasks".
        /// </summary>
        public void ProcessCommandLines()
        {
            if (App.canExecuteJumpList)
            {

                if (App.args != "/x")
                {
                    if ((App.args.EndsWith(Properties.Resources.DefaultFamilyxExtension) || App.args.EndsWith(Properties.Resources.DefaultFamilyExtension)) && File.Exists(App.args))
                    {

                        bool loaded = true;

                        if (App.args.EndsWith(Properties.Resources.DefaultFamilyxExtension))
                            loaded = LoadFamily(App.args);
                        else if (App.args.EndsWith(Properties.Resources.DefaultFamilyExtension))
                            loaded = LoadVersion2(App.args);

                        if (!loaded)
                        {
                            ShowWelcomeScreen();
                            UpdateStatus();
                        }
                        else
                        {
                            CollapseDetailsPanels();
                            ShowDetailsPane();
                            family.OnContentChanged();
                        }

                        // Do not add non default files to recent files list
                        if (familyCollection.FullyQualifiedFilename.EndsWith(Properties.Resources.DefaultFamilyxExtension))
                        {
                            // Remove the file from its current position and add it back to the top/most recent position.
                            App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                            App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                            BuildOpenMenu();
                            family.IsDirty = false;
                        }

                        if (family.Count == 0)
                            ShowWelcomeScreen();

                        UpdateStatus();
                    }

                    else
                    {
                        switch (App.args)
                        {
                            case "/n":
                                NewCommandLine();
                                break;
                            case "/i":
                                ImportCommandLine();
                                break;
                            case "/o":
                                HideWelcomeScreen();
                                HideNewUserControl();
                                OpenCommandLine();
                                break;
                            default:
                                ShowWelcomeScreen();
                                break;
                        }
                    }
                }
                else
                    ShowWelcomeScreen();
            }
            
        }       

        /// <summary>
        /// Handles loading a file from command line.
        /// </summary>
        public void LoadCommandLine(string fileName)
        {
            LoadFamily(fileName);
        }

        /// <summary>
        /// Handles opening a familyx file from command line.
        /// </summary>
        public void OpenCommandLine()
        {
            OpenFamily();
        }

        /// <summary>
        /// Handles starting a new family from command line.
        /// </summary>
        public void NewCommandLine()
        {
            NewFamily();
        }

        /// <summary>
        /// Handles importing a GEDCOM file from command line.
        /// </summary>
        public void ImportCommandLine()
        {
            GedcomLocalizationImport();
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Displays the Details Pane.
        /// </summary>
        private void ShowDetailsPane()
        {
            // Add the cloned column to layer 0:
            if (!DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
                DiagramPane.ColumnDefinitions.Add(column1CloneForLayer0);

            if (family.Current != null)
                DetailsControl.DataContext = family.Current;

            DetailsPane.Visibility = Visibility.Visible;
            DetailsControl.SetDefaultFocus();

            PersonInfoControl.Visibility = Visibility.Collapsed;

            HideNewUserControl();
            HideWelcomeScreen();

            FullScreen.IsChecked = false;
            
            enableMenus();

        }

        /// <summary>
        /// Hides any visible Details Panels.
        /// </summary>
        private void CollapseDetailsPanels()
        {
            if (DetailsControl.DetailsEdit.Visibility == Visibility.Visible)
                ((Storyboard)DetailsControl.Resources["CollapseDetailsEdit"]).Begin(DetailsControl);
            if (DetailsControl.DetailsEditMore.Visibility == Visibility.Visible)
            {
                ((Storyboard)DetailsControl.Resources["CollapseDetailsEditMore"]).Begin(DetailsControl);
                ((Storyboard)DetailsControl.Resources["CollapseDetailsEdit"]).Begin(DetailsControl);
            }

            if(DetailsControl.DetailsEditRelationship.Visibility==Visibility.Visible)
                ((Storyboard)DetailsControl.Resources["CollapseDetailsEditRelationship"]).Begin(DetailsControl);

            if(DetailsControl.DetailsEditAttachments.Visibility==Visibility.Visible)
                ((Storyboard)DetailsControl.Resources["CollapseDetailsEditAttachments"]).Begin(DetailsControl);

            if(DetailsControl.DetailsEditCitations.Visibility==Visibility.Visible)
                ((Storyboard)DetailsControl.Resources["CollapseDetailsEditCitations"]).Begin(DetailsControl);

            if (this.PersonInfoControl.Visibility == Visibility.Visible)
                ((Storyboard)this.Resources["HidePersonInfo"]).Begin(this);
            if (this.FamilyDataControl.Visibility == Visibility.Visible)
                ((Storyboard)this.Resources["HideFamilyData"]).Begin(this);

        }

        /// <summary>
        /// Disables all buttons on the Details Controls.
        /// </summary>
        private void disableButtons()
        {
            DetailsControl.EditButton.IsEnabled = false;
            DetailsControl.InfoButton.IsEnabled = false;
            DetailsControl.FamilyMemberAddButton.IsEnabled = false;
            DetailsControl.FamilyDataButton.IsEnabled = false;
            DetailsControl.EditAttachmentsButton.IsEnabled = false;
            DetailsControl.EditRelationshipsButton.IsEnabled = false;
            DetailsControl.EditCitationsButton.IsEnabled = false;
            DetailsControl.EditMoreButton.IsEnabled = false;
        }

        /// <summary>
        /// Enables all buttons on the Details Controls.
        /// </summary>
        private void enableButtons()
        {
            DetailsControl.InfoButton.IsEnabled = true;
            DetailsControl.FamilyDataButton.IsEnabled = true;
            DetailsControl.EditAttachmentsButton.IsEnabled = true;
            DetailsControl.EditRelationshipsButton.IsEnabled = true;
            DetailsControl.EditCitationsButton.IsEnabled = true;
            DetailsControl.EditButton.IsEnabled = true;
            DetailsControl.EditMoreButton.IsEnabled = true;
            DetailsControl.FamilyMemberAddButton.IsEnabled = true;
        }

        /// <summary>
        /// Enables all menus.
        /// </summary>
        private void enableMenus()
        {
            App.canExecuteJumpList = true;
            NewMenu.IsEnabled = true;
            OpenMenu.IsEnabled = true;
            SaveMenu.IsEnabled = true;
            PrintMenu.IsEnabled = true;
            MediaMenu.IsEnabled = true;
            ThemesMenu.IsEnabled = true;
            HelpMenu.IsEnabled = true;
        }

        /// <summary>
        /// Disables all menus.
        /// </summary>
        private void disableMenus()
        {
            App.canExecuteJumpList = false;
            NewMenu.IsEnabled = false;
            OpenMenu.IsEnabled = false;
            SaveMenu.IsEnabled = false;
            MediaMenu.IsEnabled = false;
            PrintMenu.IsEnabled = false;
            ThemesMenu.IsEnabled = false;
            HelpMenu.IsEnabled = false;
        }

        /// <summary>
        /// Give a control focus.
        /// </summary>
        private void giveControlFocus()
        {
            disableMenus();
            disableButtons();
            HideFamilyDataControl();
            HidePersonInfoControl();
            HideDetailsPane();
            HideWelcomeScreen();
            
            PhotoViewerControl.Visibility = Visibility.Hidden;
            StoryViewerControl.Visibility = Visibility.Hidden;
            AttachmentViewerControl.Visibility = Visibility.Hidden;
            DiagramControl.Visibility = Visibility.Hidden;

        }

        /// <summary>
        /// Remove focus from a control.
        /// </summary>
        private void removeControlFocus()
        {
            enableMenus();
            enableButtons();
            ShowDetailsPane();
            DiagramControl.Visibility = Visibility.Visible;

            if (family.Current != null)
                DetailsControl.DataContext = family.Current;
        }

        /// <summary>
        /// Displays the current file name in the window Title.
        /// </summary>
        private void UpdateStatus()
        {
            // The current file name
            string filename = Path.GetFileName(familyCollection.FullyQualifiedFilename);

            // Default value for Title
            Title = Properties.Resources.FamilyShow;

            // If the Welcome Control is visible, set Family.Show as window Title.
            if (WelcomeUserControl.Visibility == Visibility.Visible)
            {
                family.IsDirty = false;
                Title = Properties.Resources.FamilyShow;
            }
            // In every other case, display the file name as the window Title and "Unsaved" if the file is not saved.
            else
            {
                if (string.IsNullOrEmpty(filename))
                    Title = Properties.Resources.FamilyShow + " " + Properties.Resources.UnsavedStatus;
                else
                    Title = filename + " - " + Properties.Resources.FamilyShow;
            }
        }

        /// <summary>
        /// Hides the Details Pane.
        /// </summary>
        private void HideDetailsPane()
        {
            DetailsPane.Visibility = Visibility.Collapsed;

            // Remove the cloned columns from layers 0
            if (DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
                DiagramPane.ColumnDefinitions.Remove(column1CloneForLayer0);

            disableMenus();
        }

        /// <summary>
        /// Hides the Family Data Control.
        /// </summary>
        private void HideFamilyDataControl()
        {
            // Uses an animation to hide the Family Data Control
            if (FamilyDataControl.IsVisible)
                ((Storyboard)this.Resources["HideFamilyData"]).Begin(this);
        }

        /// <summary>
        /// Hides the Person Info Control.
        /// </summary>
        private void HidePersonInfoControl()
        {
            // Uses an animation to hide the Family Data Control
            if (PersonInfoControl.IsVisible)
                ((Storyboard)this.Resources["HidePersonInfo"]).Begin(this);
        }

        /// <summary>
        /// Hides the New User Control.
        /// </summary>
        private void HideNewUserControl()
        {
            NewUserControl.Visibility = Visibility.Hidden;
            DiagramControl.Visibility = Visibility.Visible;
            enableButtons();

            if (family.Current != null)
                DetailsControl.DataContext = family.Current;
        }

        /// <summary>
        /// Shows the New User Control.
        /// </summary>
        private void ShowNewUserControl()
        {
            HideFamilyDataControl();
            HideDetailsPane();
            DiagramControl.Visibility = Visibility.Collapsed;
            WelcomeUserControl.Visibility = Visibility.Collapsed;

            if (PersonInfoControl.Visibility == Visibility.Visible)
                ((Storyboard)this.Resources["HidePersonInfo"]).Begin(this);

            NewUserControl.Visibility = Visibility.Visible;
            NewUserControl.ClearInputFields();
            NewUserControl.SetDefaultFocus();

            // Delete to clear existing files and re-create the necessary folders.
            string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.ApplicationFolderName);
            tempFolder = Path.Combine(tempFolder, Microsoft.FamilyShowLib.App.AppDataFolderName);

            People.RecreateDirectory(tempFolder);
            People.RecreateDirectory(Path.Combine(tempFolder, Photo.PhotosFolderName));
            People.RecreateDirectory(Path.Combine(tempFolder, Story.StoriesFolderName));
            People.RecreateDirectory(Path.Combine(tempFolder, Attachment.AttachmentsFolderName));


        }

        /// <summary>
        /// Shows the Welcome Screen user control and hides the other controls.
        /// </summary>
        private void ShowWelcomeScreen()
        {
            HideDetailsPane();
            HideNewUserControl();
            DiagramControl.Visibility = Visibility.Hidden;
            WelcomeUserControl.Visibility = Visibility.Visible;
            App.canExecuteJumpList = true;
        }

        /// <summary>
        /// Hides the Welcome Screen.
        /// </summary>
        private void HideWelcomeScreen()
        {
            WelcomeUserControl.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Builds the Recent Files Menu.
        /// </summary>
        private void BuildOpenMenu()
        {
            // Store the menu items with icons
            MenuItem open = new MenuItem();
            MenuItem import = new MenuItem();
            MenuItem merge = new MenuItem();

            foreach (object element in OpenMenu.Items)
            {
                var item = element as MenuItem;
                if (item != null)
                {
                    if (item.Header.ToString() == Properties.Resources.OpenMenu)
                        open = item;
                    if (item.Header.ToString() == Properties.Resources.MergeMenu)
                        merge = item;
                    if (item.Header.ToString() == Properties.Resources.GedcomMenu)
                        import = item;
                }                
            }

            // Clear existing menu item

            OpenMenu.Items.Clear();

            // Restore menu items with icons
            OpenMenu.Items.Add(open);
            OpenMenu.Items.Add(import);
            OpenMenu.Items.Add(merge);
            
            // Add the recent files to the menu as menu items
            if (App.RecentFiles.Count > 0)
            {
                OpenMenu.Items.Add(new Separator());

                int i = 1;

                foreach (string file in App.RecentFiles)
                {
                    MenuItem item = new MenuItem();
                    item.Header = i + ". " + System.IO.Path.GetFileName(file);
                    item.CommandParameter = file;
                    item.Click += new RoutedEventHandler(OpenRecentFile);
                    OpenMenu.Items.Add(item);
                    i++;
                }

                OpenMenu.Items.Add(new Separator());

                MenuItem openMenuItem4 = new MenuItem();
                openMenuItem4.Header = Properties.Resources.ClearRecentFilesMenu;
                openMenuItem4.Click += new RoutedEventHandler(ClearRecentFiles);
                OpenMenu.Items.Add(openMenuItem4);
       
            }

        }

        /// <summary>
        /// Builds the Themes Menu.
        /// </summary>
        private void BuildThemesMenu()
        {
            MenuItem theme1 = new MenuItem();
            MenuItem theme2 = new MenuItem();

            theme1.Header = Properties.Resources.Black;
            theme1.CommandParameter = @"Themes\Black\BlackResources.xaml";
            theme1.Click += new RoutedEventHandler(ChangeTheme);

            theme2.Header = Properties.Resources.Silver;
            theme2.CommandParameter = @"Themes\Silver\SilverResources.xaml";
            theme2.Click += new RoutedEventHandler(ChangeTheme);

            ThemesMenu.Items.Add(theme1);
            ThemesMenu.Items.Add(theme2);
        }

        /// <summary>
        /// Releases any images the application has loaded.
        /// These must be released before opening a new file.
        /// </summary>
        private void ReleasePhotos()
        {
            PhotoViewerControl.DisplayPhoto.Source = null;
            PersonInfoControl.DisplayPhoto.Source = null;
        }

        /// <summary>
        /// Prompts the user upon closing the application to save the current family if it has been changed.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Make sure the file is saved before the app is closed.  
            // Allows user to cancel close request, save the file or to close without saving.

            if (!family.IsDirty)
                return;

            MessageBoxResult result = MessageBox.Show(Properties.Resources.NotSavedMessage,
                Properties.Resources.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
                if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
                {
                    CommonDialog dialog = new CommonDialog();
                    dialog.InitialDirectory = People.ApplicationFolderPath;
                    dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
                    dialog.Title = Properties.Resources.SaveAs;
                    dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
                    dialog.ShowSave();

                    if (!string.IsNullOrEmpty(dialog.FileName))
                    {
                        familyCollection.Save(dialog.FileName);
                        // Remove the file from its current position and add it back to the top/most recent position.
                        App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                        App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                    }
                }
                else
                {
                    familyCollection.Save(false);

                    // Remove the file from its current position and add it back to the top/most recent position.
                    App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                    App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);

                }
                base.OnClosing(e);
            }

            // Continue with close and don't save.
            if (result == MessageBoxResult.No)
            {
                base.OnClosing(e);
            }

            // Cancel the close if user no longer wants to close.
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Prompts the user to save the current family if it has been changed.
        /// </summary>
        public void PromptToSave()
        {
            if (!family.IsDirty)
                return;

            MessageBoxResult result = MessageBox.Show(Properties.Resources.NotSavedMessage,
                    Properties.Resources.Save, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
                if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
                {
                    CommonDialog dialog = new CommonDialog();
                    dialog.InitialDirectory = People.ApplicationFolderPath;
                    dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyxFiles, Properties.Resources.FamilyxExtension));
                    dialog.Title = Properties.Resources.SaveAs;
                    dialog.DefaultExtension = Properties.Resources.DefaultFamilyxExtension;
                    dialog.ShowSave();

                    if (!string.IsNullOrEmpty(dialog.FileName))
                    {
                        familyCollection.Save(dialog.FileName);

                        if (!App.RecentFiles.Contains(familyCollection.FullyQualifiedFilename))
                        {
                            App.RecentFiles.Add(familyCollection.FullyQualifiedFilename);
                        }
                    }
                    else
                    {
                        familyCollection.Save(false);

                        if (!App.RecentFiles.Contains(familyCollection.FullyQualifiedFilename))
                        {
                            App.RecentFiles.Add(familyCollection.FullyQualifiedFilename);
                        }
                    }
                }
            }
        }

        #endregion

    }
}
