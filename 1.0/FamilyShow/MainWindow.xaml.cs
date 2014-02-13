using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.IO;
using System.IO.Packaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region fields

        // The list of people, this is a global list shared by the application.
        People familyCollection = App.FamilyCollection;
        PeopleCollection family = App.Family;

        #endregion

        #region menu routed commands

        public static readonly RoutedCommand ImportGedcomCommand = new RoutedCommand("ImportGedcom", typeof(MainWindow));
        public static readonly RoutedCommand ExportGedcomCommand = new RoutedCommand("ExportGedcom", typeof(MainWindow));
        public static readonly RoutedCommand WhatIsGedcomCommand = new RoutedCommand("WhatIsGedcom", typeof(MainWindow));
        public static readonly RoutedCommand ExportXpsCommand = new RoutedCommand("ExportXps", typeof(MainWindow));

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            family.CurrentChanged +=new EventHandler(People_CurrentChanged);

            // Setup menu command bindings
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, this.NewFamily));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, this.OpenFamily));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, this.SaveFamily));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, this.SaveFamilyAs));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, this.PrintFamily));
            this.CommandBindings.Add(new CommandBinding(MainWindow.ImportGedcomCommand, this.ImportGedcom));
            this.CommandBindings.Add(new CommandBinding(MainWindow.ExportGedcomCommand, this.ExportGedcom));
            this.CommandBindings.Add(new CommandBinding(MainWindow.WhatIsGedcomCommand, this.WhatIsGedcom));
            this.CommandBindings.Add(new CommandBinding(MainWindow.ExportXpsCommand, this.ExportXps));

            // Build the Open Menu, recent opened files are part of the open menu
            BuildOpenMenu();

            // The welcome screen is the initial view
            ShowWelcomeScreen();
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

        private void NewUserControl_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            HideNewUserControl();
            ShowDetailsPane();
        }

        private void DetailsControl_PersonInfoClick(object sender, RoutedEventArgs e)
        {
            // Uses an animation to show the Person Info Control
            ((Storyboard)this.Resources["ShowPersonInfo"]).Begin(this);

            PersonInfoControl.DataContext = family.Current;
        }

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

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ShowPersonInfo_StoryboardCompleted(object sender, EventArgs e)
        {
            PersonInfoControl.SetDefaultFocus();
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void HidePersonInfo_StoryboardCompleted(object sender, EventArgs e)
        {
            DetailsControl.SetDefaultFocus();
        }

        private void Vertigo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Open the Vertigo website in the user's default browser
            System.Diagnostics.Process.Start("http://www.vertigo.com");
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

        private void WelcomeUserControl_OpenRecentFileButtonClick(object sender, RoutedEventArgs e)
        {
            Button item = (Button)e.OriginalSource;
            string file = item.CommandParameter as string;

            if (!string.IsNullOrEmpty(file))
            {
                // Load the selected family file
                familyCollection.Load(file);

                ShowDetailsPane();

                // This will tell the diagram to redraw and the details panel to update.
                family.OnContentChanged();

                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(file);
                App.RecentFiles.Insert(0, file);
                BuildOpenMenu();
                family.IsDirty = false;
            }
        }

        #endregion

        #region menu events

        /// <summary>
        /// Command handler for New Command in the menu.
        /// </summary>
        private void NewFamily(object sender, RoutedEventArgs e)
        {
            PromptToSave();
        
            family.Clear();
            familyCollection.FullyQualifiedFilename = null;
            family.OnContentChanged();

            ShowNewUserControl();
            family.IsDirty = false;
        }

        /// <summary>
        /// Command handler for Open Command in the menu.
        /// </summary>
        private void OpenFamily(object sender, RoutedEventArgs e)
        {
            PromptToSave();
        
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
            dialog.Title = Properties.Resources.Open;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                // Load the selected family file
                familyCollection.Load(dialog.FileName);

                ShowDetailsPane();

                // This will tell the diagram to redraw and the details panel to update.
                family.OnContentChanged();

                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                BuildOpenMenu();
                family.IsDirty = false;
            }
        }

        /// <summary>
        /// Command handler for Open Recent Command in the menu.
        /// </summary>
        private void OpenRecentFile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string file = item.CommandParameter as string;

            if (!string.IsNullOrEmpty(file))
            {
                PromptToSave();
            
                // Load the selected family file
                familyCollection.Load(file);

                ShowDetailsPane();

                // This will tell the diagram to redraw and the details panel to update.
                family.OnContentChanged();

                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(file);
                App.RecentFiles.Insert(0, file);
                BuildOpenMenu();
                family.IsDirty = false;
            }
        }

        /// <summary>
        /// Command handler for Save Command in the menu.
        /// </summary>
        private void SaveFamily(object sender, RoutedEventArgs e)
        {
            // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
            if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename))
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = People.ApplicationFolderPath;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyExtension));
                dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
                dialog.Title = Properties.Resources.SaveAs;
                dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
                dialog.ShowSave();

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    familyCollection.Save(dialog.FileName);

                    // Remove the file from its current position and add it back to the top/most recent position.
                    App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                    App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                    BuildOpenMenu();
                }
            }
            else
            {
                familyCollection.Save();

                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                BuildOpenMenu();
            }
        }

        /// <summary>
        /// Command handler for Save As Command in the menu.
        /// </summary>
        private void SaveFamilyAs(object sender, RoutedEventArgs e)
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
            dialog.Title = Properties.Resources.SaveAs;
            dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                familyCollection.Save(dialog.FileName);

                // Remove the file from its current position and add it back to the top/most recent position.
                App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
                App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
                BuildOpenMenu();
            }
        }

        /// <summary>
        /// Command handler for Print Command in the menu.
        /// </summary>
        private void PrintFamily(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();

            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                // Hide the zoom control before the diagram is saved
                DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;

                // Send the diagram to the printer
                dlg.PrintVisual(DiagramBorder, familyCollection.FullyQualifiedFilename);

                // Show the zoom control again
                DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
            }            
        }

        /// <summary>
        /// Command handler for ImportGedcomCommand in the menu.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ImportGedcom(object sender, EventArgs e)
        {
            PromptToSave();

            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
            dialog.Title = Properties.Resources.Import;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                try
                {
                    GedcomImport ged = new GedcomImport();
                    ged.Import(family, dialog.FileName);
                    familyCollection.FullyQualifiedFilename = string.Empty;

                    ShowDetailsPane();
                    family.IsDirty = false;
                }
                catch
                {
                    // Could not import the GEDCOM for some reason. Handle
                    // all exceptions the same, display message and continue
                    /// without importing the GEDCOM file.
                    MessageBox.Show(this, Properties.Resources.GedcomFailedMessage, 
                        Properties.Resources.GedcomFailed, MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Command handler for ExportGedcomCommand in the menu.
        /// </summary>
        private void ExportGedcom(object sender, EventArgs e)
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
            dialog.Title = Properties.Resources.Export;
            dialog.DefaultExtension = Properties.Resources.DefaultGedcomExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                GedcomExport ged = new GedcomExport();
                ged.Export(family, dialog.FileName);
            }
        }

        /// <summary>
        /// Command handler for ExportGedcomCommand in the menu.
        /// </summary>
        private void WhatIsGedcom(object sender, EventArgs e)
        {
            // Open the Wikipedia entry about GEDCOM in the user's default browser
            System.Diagnostics.Process.Start("http://en.wikipedia.org/wiki/GEDCOM");
        }
        
        /// <summary>
        /// Command handler for ExportXPSCommand in the menu.
        /// </summary>
        private void ExportXps(object sender, EventArgs e)
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.XpsFiles, Properties.Resources.XpsExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
            dialog.Title = Properties.Resources.Export;
            dialog.DefaultExtension = Properties.Resources.DefaultXpsExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                // Create the XPS document from the window's main container (in this case, a grid) 
                Package package = Package.Open(dialog.FileName, FileMode.Create);
                XpsDocument xpsDoc = new XpsDocument(package);
                XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);

                // Hide the zoom control before the diagram is saved
                DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;

                // Since DiagramBorder derives from FrameworkElement, the XpsDocument writer knows
                // how to output it's contents. The border is used instead of the DiagramControl
                // so that the diagram background is output as well as the digram control itself.
                xpsWriter.Write(DiagramBorder);
                xpsDoc.Close();
                package.Close(); 

                // Show the zoom control again
                DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Command handler for Clear Recent Files Command in the menu.
        /// </summary>
        private void ClearRecentFiles_Click(object sender, RoutedEventArgs e)
        {
            App.RecentFiles.Clear();
            App.SaveRecentFiles();
            BuildOpenMenu();
        }

        /// <summary>
        /// Command handler for WelcomeScreenCommand in the menu.
        /// </summary>
        private void WelcomeScreen(object sender, EventArgs e)
        {
            ShowWelcomeScreen();
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Displays the details pane
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

            HideNewUserControl();
            HideWelcomeScreen();

            NewMenu.IsEnabled = true;
            OpenMenu.IsEnabled = true;
            SaveMenu.IsEnabled = true;
            GedcomMenu.IsEnabled = true;
        }

        /// <summary>
        /// Hides the details pane
        /// </summary>
        private void HideDetailsPane()
        {
            DetailsPane.Visibility = Visibility.Collapsed;

            // Remove the cloned columns from layers 0
            if (DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
                DiagramPane.ColumnDefinitions.Remove(column1CloneForLayer0);

            NewMenu.IsEnabled = false;
            OpenMenu.IsEnabled = false;
            SaveMenu.IsEnabled = false;
            GedcomMenu.IsEnabled = false;
        }

        /// <summary>
        /// Hides the New User Control.
        /// </summary>
        private void HideNewUserControl()
        {
            NewUserControl.Visibility = Visibility.Hidden;
            DiagramControl.Visibility = Visibility.Visible;

            if (family.Current != null)
                DetailsControl.DataContext = family.Current;
        }

        /// <summary>
        /// Show the New User Control.
        /// </summary>
        private void ShowNewUserControl()
        {
            HideDetailsPane();
            DiagramControl.Visibility = Visibility.Collapsed;
            WelcomeUserControl.Visibility = Visibility.Collapsed;

            NewUserControl.Visibility = Visibility.Visible;
            NewUserControl.ClearInputFields();
            NewUserControl.SetDefaultFocus();
        }

        /// <summary>
        /// Show the Welcome Screen.
        /// </summary>
        private void ShowWelcomeScreen()
        {
            HideDetailsPane();
            HideNewUserControl();

            DiagramControl.Visibility = Visibility.Hidden;
            WelcomeUserControl.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the Welcome Screen.
        /// </summary>
        private void HideWelcomeScreen()
        {
            WelcomeUserControl.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Builds the Recent Files Menu
        /// </summary>
        private void BuildOpenMenu()
        {
            // Clear existing menu items
            OpenMenu.Items.Clear();

            // MenuItem for opening files
            MenuItem openMenuItem = new MenuItem();
            openMenuItem.Header = "Open";
            openMenuItem.Command = ApplicationCommands.Open;
            OpenMenu.Items.Add(openMenuItem);

            // Add the recent files to the menu as menu items
            if (App.RecentFiles.Count > 0)
            {
                // Separator between the open menu and the recent files
                OpenMenu.Items.Add(new Separator());

                foreach (string file in App.RecentFiles)
                {
                    MenuItem item = new MenuItem();
                    item.Header = System.IO.Path.GetFileName(file);
                    item.CommandParameter = file;
                    item.Click += new RoutedEventHandler(OpenRecentFile_Click);

                    OpenMenu.Items.Add(item);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Make sure the file is saved before the app is closed.
            PromptToSave();
            base.OnClosing(e);
        }

        private void PromptToSave()
        {
            if (!family.IsDirty)
                return;

            MessageBoxResult result = MessageBox.Show(Properties.Resources.NotSavedMessage,
                Properties.Resources.NotSaved, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = People.ApplicationFolderPath;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyExtension));
                dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
                dialog.Title = Properties.Resources.SaveAs;
                dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
                dialog.ShowSave();

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    familyCollection.Save(dialog.FileName);

                    if (!App.RecentFiles.Contains(familyCollection.FullyQualifiedFilename))
                    {
                        App.RecentFiles.Add(familyCollection.FullyQualifiedFilename);
                        BuildOpenMenu();
                    }
                }
            }
        }

        #endregion
    }
}
