using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Details.xaml
    /// </summary>

    public partial class Details : System.Windows.Controls.UserControl
    {
        #region fields

        PeopleCollection family = App.Family;
        SourceCollection sources = App.Sources;

        Gender genderFilter = Gender.Male;

        Boolean ResetFilter = false;     //When true, enables quick filter reset
        Boolean ExistingFilter = false;  //When true, enables automatic filtering on gender and relatives
        Boolean ignoreGender = false;    //When true, enables filtering on gender

        private TextWriter tw;

        // Setting the ItemsSource selects the first item which raises the SelectionChanged event.
        // This flag prevents the initialization code from making the selection.
        bool ignoreSelection = true;

        #endregion

        public Details()
        {
            InitializeComponent();
            // These sections are collapsed within code instead of in the xaml 
            // so that they show up as visible in Blend.
            DetailsAdd.Visibility = Visibility.Collapsed;
            DetailsAddIntermediate.Visibility = Visibility.Collapsed;
            DetailsEdit.Visibility = Visibility.Collapsed;
            DetailsEditMore.Visibility = Visibility.Collapsed;
            DetailsEditRelationship.Visibility = Visibility.Collapsed;
            DetailsEditCitations.Visibility = Visibility.Collapsed;
            DetailsEditAttachments.Visibility = Visibility.Collapsed;
            AddExisting.Visibility = Visibility.Collapsed;


            // Bind the Family ListView and turn off the allow the selection 
            // change event to change the selected item.
            FamilyListView.ItemsSource = family;
            ignoreSelection = false;

            // Set the default sort order for the Family ListView to 
            // the person's first name.
            ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(family);
            view.SortDescriptions.Add(new SortDescription("FirstName", ListSortDirection.Ascending));

            // Handle event when the selected person changes so can select 
            // the item in the list.
            family.CurrentChanged += new EventHandler(Family_CurrentChanged);

            ExistingPeopleListBox.ItemsSource = family;

        }

        #region routed events

        public static readonly RoutedEvent PersonInfoClickEvent = EventManager.RegisterRoutedEvent(
            "PersonInfoClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Details));

        /// <summary>
        /// Expose the PersonInfoClick event
        /// </summary>
        public event RoutedEventHandler PersonInfoClick
        {
            add { AddHandler(PersonInfoClickEvent, value); }
            remove { RemoveHandler(PersonInfoClickEvent, value); }
        }

        public static readonly RoutedEvent EveryoneDeletedEvent = EventManager.RegisterRoutedEvent(
            "EveryoneDeleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Details));

        /// <summary>
        /// Expose the EveryoneDeleted event
        /// </summary>
        public event RoutedEventHandler EveryoneDeleted
        {
            add { AddHandler(EveryoneDeletedEvent, value); }
            remove { RemoveHandler(EveryoneDeletedEvent, value); }
        }

        public static readonly RoutedEvent FamilyDataClickEvent = EventManager.RegisterRoutedEvent(
            "FamilyDataClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Details));

        /// <summary>
        /// Expose the FamilyDataClick event
        /// </summary>
        public event RoutedEventHandler FamilyDataClick
        {
            add { AddHandler(FamilyDataClickEvent, value); }
            remove { RemoveHandler(FamilyDataClickEvent, value); }
        }
        #endregion

        #region event handlers

        #region Attachments event handlers

        private void DeleteAttachmentsButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            if (AttachmentsListBox.SelectedItem != null)
            {

                MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeleteAttachment,
                   Properties.Resources.Attachment, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Attachment attachment = new Attachment();

                    foreach (Attachment a in person.Attachments)
                    {
                        if (a == AttachmentsListBox.SelectedItem)
                        {
                            person.Attachments.Remove(a);
                            break;
                        }
                    }

                    person.OnPropertyChanged("HasAttachments");
                    family.OnContentChanged();
                }
            }
        }

        private void AttachmentsListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Person person = (Person)this.DataContext;

            if (person.Restriction != Restriction.Locked)
            {
                if (e.Key == Key.Delete)
                    DeleteAttachmentsButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Creates a temporary copy of the file in the family data directory with a GUID and then opens that file.
        /// This prevents problems such as:
        /// 1. On opening a familyx file, the process may fail if another program
        ///    has locked a file in the Attachments directory if the new file has an attachment of the same name.
        /// Any temp files will be cleaned up on application open or close once the other program has been closed.   
        /// </summary>
        private void LoadSelectedAttachment(object sender, RoutedEventArgs e)
        {
            if (AttachmentsListBox.Items.Count > 0 && AttachmentsListBox.SelectedItem != null)
            {

                string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationFolderName);
                appLocation = Path.Combine(appLocation, App.AppDataFolderName);

                string fullFilePath = AttachmentsListBox.SelectedItem.ToString();

                string fileExtension = Path.GetExtension(fullFilePath);
                string newFileName = Path.GetFileNameWithoutExtension(fullFilePath) + Guid.NewGuid().ToString() + fileExtension;
                string tempFilePath = Path.Combine(appLocation, newFileName);

                FileInfo ofi = new FileInfo(fullFilePath);
                ofi.CopyTo(tempFilePath, true);

                try
                {
                    System.Diagnostics.Process.Start(tempFilePath);
                }
                catch { }
            }
        }

        private void LinkAttachmentsButton_Click(object sender, RoutedEventArgs e)
        {

            Person person = (Person)this.DataContext;

            string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                App.ApplicationFolderName);
            appLocation = Path.Combine(appLocation, App.AppDataFolderName);

            // Absolute path to the attachments folder
            string attachmentLocation = Path.Combine(appLocation, Attachment.AttachmentsFolderName);

            int attachmentCount = 0;

            if (Directory.Exists(attachmentLocation))
                attachmentCount = Directory.GetFiles(attachmentLocation).Length;

            if (attachmentCount > 0)
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = attachmentLocation;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.AttachmentFiles, Properties.Resources.AttachmentExtension));
                dialog.Title = Properties.Resources.Link;
                dialog.ShowOpen();

                int i = 0;

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    foreach (Attachment a in person.Attachments)
                    {
                        if (Path.GetFileName(a.RelativePath) == Path.GetFileName(dialog.FileName))
                        {
                            i++;
                            break;
                        }
                    }

                    if (i == 0)
                    {
                        //only link files which are in the temp directory and which are of allowed file types
                        if (File.Exists(Path.Combine(attachmentLocation, Path.GetFileName(dialog.FileName))))
                        {
                            Attachment attachment = new Attachment();

                            attachment.RelativePath = Path.Combine(Attachment.AttachmentsFolderName, Path.GetFileName(dialog.FileName));
                            // Associate the attachment with the person.
                            person.Attachments.Add(attachment);
                            person.OnPropertyChanged("HasAttachments");

                        }
                        else
                           MessageBox.Show(Properties.Resources.LinkAddMessage, Properties.Resources.LinkFailed, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    else
                        MessageBox.Show(Properties.Resources.AttachmentExists, Properties.Resources.LinkFailed, MessageBoxButton.OK, MessageBoxImage.Warning);

                }
            }
            else
                MessageBox.Show(Properties.Resources.NoExistingAttachments, Properties.Resources.Link, MessageBoxButton.OK, MessageBoxImage.Warning);


        

        }

        private void AddAttachmentsButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filter.Add(new FilterEntry(Properties.Resources.AttachmentFiles, Properties.Resources.AttachmentExtension));
            dialog.Title = Properties.Resources.Open;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                    Attachment attachment = new Attachment(dialog.FileName);
                    // Associate the attachment with the person.
                    person.Attachments.Add(attachment);
                    person.OnPropertyChanged("HasAttachments");       
            }

            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;
        }

        private void AttachmentsListBox_Drop(object sender, DragEventArgs e)
        {
            Person person = (Person)this.DataContext;

            if (person.Restriction != Restriction.Locked)
            {

                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                // Get the files that is supported and add them to the photos for the person
                foreach (string fileName in fileNames)
                {

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if(App.IsAttachmentFileSupported(fileName))
                        {
                        Attachment attachment = new Attachment(fileName);
                        // Associate the attachment with the person.
                        person.Attachments.Add(attachment);
                        }
                        else
                            MessageBox.Show(Properties.Resources.NotSupportedExtension1 + Path.GetExtension(fileName) + " " + Properties.Resources.NotSupportedExtension2 + " " + Properties.Resources.UnsupportedAttachmentMessage, Properties.Resources.Unsupported, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            person.OnPropertyChanged("HasAttachments");
            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;

        }

        #endregion

        #region Details Add event handlers

        /// <summary>
        /// Handles the Family Member Add Button click event
        /// </summary>
        private void FamilyMemberAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(FamilyMemberAddButton.CommandParameter == null))
            {
                disableButtons();
                
                FamilyMemberComboBox.SelectedItem =
                    (FamilyMemberComboBoxValue)(FamilyMemberAddButton.CommandParameter);
            }

        }

        /// <summary>
        /// Shows the Details Add section with the selected family member relationship choice.
        /// </summary>
        private void FamilyMemberComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            disableButtons();
            if (FamilyMemberComboBox.SelectedIndex != -1)
            {
                ClearDetailsAddFields();

                string surname = string.Empty;
                string relationship = string.Empty;
                bool isExisting = false;

                switch ((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue)
                {
                    case FamilyMemberComboBoxValue.Father:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Father;
                        break;
                    case FamilyMemberComboBoxValue.Mother:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Mother;
                        break;
                    case FamilyMemberComboBoxValue.Sister:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Sister;
                        break;
                    case FamilyMemberComboBoxValue.Brother:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Brother;
                        // Assume that the new person has the same last name.
                        surname = family.Current.LastName;
                        break;
                    case FamilyMemberComboBoxValue.Daughter:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Daughter;
                        break;
                    case FamilyMemberComboBoxValue.Son:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Son;
                        // Assume that the new person has the same last name as the husband
                        if ((family.Current.Gender == Gender.Female) && (family.Current.Spouses.Count > 0) && (family.Current.Spouses[0].Gender == Gender.Male))
                            surname = family.Current.Spouses[0].LastName;
                        else
                            surname = family.Current.LastName;
                        break;
                    case FamilyMemberComboBoxValue.Spouse:
                        relationship = Microsoft.FamilyShow.Properties.Resources.Spouse;
                        break;
                    case FamilyMemberComboBoxValue.Existing:
                        isExisting = true;
                        break;
                    default:
                        break;
                }

                if (isExisting)
                {
                    // Use animation to expand the Add Existing section
                    ((Storyboard)this.Resources["ExpandAddExisting"]).Begin(this);
                }
                else
                    // Use animation to expand the Details Add section
                    ((Storyboard)this.Resources["ExpandDetailsAdd"]).Begin(this);


                Relationship.Text = relationship;
                SurnameInputTextBox.Text = surname;
                NamesInputTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles adding new people
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

            if (FamilyMemberComboBox.SelectedItem != null)  //prevents program crashing when user presses enter more than one before add is completed.
            {

                // To make it a little more user friendly, set the next action for the family member button to be the same as the current relationship being added.
                SetNextFamilyMemberAction((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue);
                // The new person to be added
                Person newPerson = new Person(NamesInputTextBox.Text, SurnameInputTextBox.Text);
                newPerson.IsLiving = (IsLivingInputCheckbox.IsChecked == null) ? true : (bool)IsLivingInputCheckbox.IsChecked;

                DateTime birthdate = App.StringToDate(BirthDateInputTextBox.Text);
                if (birthdate != DateTime.MinValue)
                    newPerson.BirthDate = birthdate;

                newPerson.BirthPlace = BirthPlaceInputTextBox.Text;

                bool SelectParent = false;
                ParentSetCollection possibleParents = family.Current.PossibleParentSets;
                int parentsnumber = family.Current.Parents.Count;

                // Perform the action based on the selected relationship
                switch ((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue)
                {
                    case FamilyMemberComboBoxValue.Father:
                        newPerson.Gender = Gender.Male;

                        RelationshipHelper.AddParent(family, family.Current, newPerson);

                        if (family.Current.Parents.Count == 2)
                            // Person has parents, choice another default.
                            SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Brother);
                        else
                            SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Mother);
                        break;

                    case FamilyMemberComboBoxValue.Mother:
                        newPerson.Gender = Gender.Female;

                        RelationshipHelper.AddParent(family, family.Current, newPerson);

                        if (family.Current.Parents.Count == 2)
                            // Person has parents, choice another default.
                            SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Brother);
                        else
                            SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Father);
                        break;

                    case FamilyMemberComboBoxValue.Brother:
                        newPerson.Gender = Gender.Male;

                        // Check to see if there are multiple parents
                        if (possibleParents.Count > 1)
                            SelectParent = true;
                        else
                            RelationshipHelper.AddSibling(family, family.Current, newPerson);
                        break;

                    case FamilyMemberComboBoxValue.Sister:
                        newPerson.Gender = Gender.Female;

                        // Check to see if there are multiple parents
                        if (possibleParents.Count > 1)
                            SelectParent = true;
                        else
                            RelationshipHelper.AddSibling(family, family.Current, newPerson);
                        break;

                    case FamilyMemberComboBoxValue.Spouse:
                        RelationshipHelper.AddSpouse(family, family.Current, newPerson, SpouseModifier.Current);
                        SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Son);
                        break;

                    case FamilyMemberComboBoxValue.Son:
                        newPerson.Gender = Gender.Male;

                        if (family.Current.Spouses.Count > 1)
                        {
                            possibleParents = family.Current.MakeParentSets();
                            SelectParent = true;
                        }
                        else
                            RelationshipHelper.AddChild(family, family.Current, newPerson, ParentChildModifier.Natural);
                        break;

                    case FamilyMemberComboBoxValue.Daughter:
                        newPerson.Gender = Gender.Female;
                        if (family.Current.Spouses.Count > 1)
                        {
                            possibleParents = family.Current.MakeParentSets();
                            SelectParent = true;
                        }
                        else
                            RelationshipHelper.AddChild(family, family.Current, newPerson, ParentChildModifier.Natural);
                        break;
                    case FamilyMemberComboBoxValue.Unrelated:
                        family.Add(newPerson);
                        family.Current = newPerson;
                        family.OnContentChanged();
                        SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Father);
                        break;
                }

                if (SelectParent)
                    ShowDetailsAddIntermediate(possibleParents);
                else
                {
                    // Use animation to hide the Details Add section
                    ((Storyboard)this.Resources["CollapseDetailsAdd"]).Begin(this);

                    FamilyMemberComboBox.SelectedIndex = -1;
                    FamilyMemberAddButton.Focus();
                }
                family.OnContentChanged(newPerson);
                family.OnContentChanged(family.Current);
            }
        }

        /// <summary>
        /// Handles adding new people and choosing the parents within the Intermediate Add section.
        /// </summary>
        private void IntermediateAddButton_Click(object sender, RoutedEventArgs e)
        {

            if (FamilyMemberComboBox.SelectedItem != null)  //prevents program crashing when user presses enter more than one before add is completed.
            {
                Person newPerson = new Person(NamesInputTextBox.Text, SurnameInputTextBox.Text);
                newPerson.IsLiving = (IsLivingInputCheckbox.IsChecked == null) ? true : (bool)IsLivingInputCheckbox.IsChecked;

                DateTime birthdate = App.StringToDate(BirthDateInputTextBox.Text);
                if (birthdate != DateTime.MinValue)
                    newPerson.BirthDate = birthdate;

                newPerson.BirthPlace = BirthPlaceInputTextBox.Text;

                switch ((FamilyMemberComboBoxValue)FamilyMemberComboBox.SelectedValue)
                {
                    case FamilyMemberComboBoxValue.Brother:
                        newPerson.Gender = Gender.Male;
                        RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
                        break;

                    case FamilyMemberComboBoxValue.Sister:
                        newPerson.Gender = Gender.Female;
                        RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
                        break;
                    case FamilyMemberComboBoxValue.Son:
                        newPerson.Gender = Gender.Male;
                        RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
                        break;

                    case FamilyMemberComboBoxValue.Daughter:
                        newPerson.Gender = Gender.Female;
                        RelationshipHelper.AddParent(family, newPerson, (ParentSet)ParentsListBox.SelectedValue);
                        break;
                }

                FamilyMemberComboBox.SelectedIndex = -1;
                FamilyMemberAddButton.Focus();

                // Use animation to hide the Details Add Intermediate section
                ((Storyboard)this.Resources["CollapseDetailsAddIntermediate"]).Begin(this);

                family.OnContentChanged(newPerson);
                family.OnContentChanged(family.Current);

            }
        }

        private void AddExistingButton_Click(object sender, RoutedEventArgs e)
        {

            if (ExistingFamilyMemberComboBox.SelectedItem != null)  //prevents program crashing when user presses enter more than one before add is completed.
            {
                Person existingPerson = (Person)ExistingPeopleListBox.SelectedItem;

                bool PersonAdded = false; //flag when person is added

                if (existingPerson != family.Current && existingPerson != null)
                {
                    // Perform the action based on the selected relationship
                    switch ((ExistingFamilyMemberComboBoxValue)ExistingFamilyMemberComboBox.SelectedValue)
                    {

                        case ExistingFamilyMemberComboBoxValue.Father:
                            if (existingPerson.Gender == Gender.Male)
                                RelationshipHelper.AddExistingParent(family, family.Current, existingPerson, ParentChildModifier.Natural);

                            break;

                        case ExistingFamilyMemberComboBoxValue.Mother:
                            if (existingPerson.Gender == Gender.Female)
                                RelationshipHelper.AddExistingParent(family, family.Current, existingPerson, ParentChildModifier.Natural);

                            break;

                        case ExistingFamilyMemberComboBoxValue.Brother:
                            if (existingPerson.Gender == Gender.Male)
                                RelationshipHelper.AddExistingSibling(family, family.Current, existingPerson);

                            break;

                        case ExistingFamilyMemberComboBoxValue.Sister:

                            if (existingPerson.Gender == Gender.Female)
                                RelationshipHelper.AddExistingSibling(family, family.Current, existingPerson);
                            break;


                        case ExistingFamilyMemberComboBoxValue.Spouse:
                            if (!existingPerson.Spouses.Contains(family.Current))
                                RelationshipHelper.AddExistingSpouse(family, family.Current, existingPerson, SpouseModifier.Current);
                            break;

                        case ExistingFamilyMemberComboBoxValue.Son:

                            if (existingPerson.Gender == Gender.Male)
                                RelationshipHelper.AddExistingChild(family, family.Current, existingPerson, ParentChildModifier.Natural);
                            break;

                        case ExistingFamilyMemberComboBoxValue.Daughter:

                            if (existingPerson.Gender == Gender.Female)
                                RelationshipHelper.AddExistingChild(family, family.Current, existingPerson, ParentChildModifier.Natural);
                            break;
                    }
                    SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Father);
                    PersonAdded = true;
                }
                else
                {
                    MessageBox.Show(Properties.Resources.SelectPersonFirstMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Information);
                }


                if (PersonAdded == true)
                {
                    family.OnContentChanged();
                    // Use animation to hide the Details Add section
                    ((Storyboard)this.Resources["CollapseAddExisting"]).Begin(this);

                    FamilyMemberComboBox.SelectedIndex = -1;
                    FamilyMemberAddButton.Focus();
                    family.OnContentChanged();
                    family.OnContentChanged(family.Current);
                    family.OnContentChanged(existingPerson);
                }

            }         
        }

        /// <summary>
        /// Closes the Details Add section
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            FamilyMemberComboBox.SelectedIndex = -1;
            FilterTextBox.Text = string.Empty;
            enableButtons();
        }

        private void IntermediateCloseButton_Click(object sender, RoutedEventArgs e)
        {
            FamilyMemberComboBox.SelectedIndex = -1;
        }

        private void ParentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Make it easy to add new people to the tree by pressing return
            IntermediateAddButton.Focus();
        }

        private void ExistingFamilyMemberComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ExistingFamilyMemberComboBox.SelectedIndex != -1)
            {
                ignoreGender = false;
                switch ((ExistingFamilyMemberComboBoxValue)ExistingFamilyMemberComboBox.SelectedValue)
                {
                    case ExistingFamilyMemberComboBoxValue.Father:
                        genderFilter = Gender.Male;
                        break;
                    case ExistingFamilyMemberComboBoxValue.Mother:
                        genderFilter = Gender.Female;
                        break;
                    case ExistingFamilyMemberComboBoxValue.Brother:
                        genderFilter = Gender.Male;
                        break;
                    case ExistingFamilyMemberComboBoxValue.Sister:
                        genderFilter = Gender.Female;
                        break;
                    case ExistingFamilyMemberComboBoxValue.Spouse:
                        ignoreGender = true;
                        break;
                    case ExistingFamilyMemberComboBoxValue.Son:
                        genderFilter = Gender.Male;
                        break;
                    case ExistingFamilyMemberComboBoxValue.Daughter:
                        genderFilter = Gender.Female;
                        break;
                }

                UpdateExistingFilter();
            }
        }

        #endregion

        #region Details Edit Citations event handlers

        /// <summary>
        /// Exports a persons citations
        /// </summary>
        private void ExportCitations_Click(object sender, RoutedEventArgs e)
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = People.ApplicationFolderPath;
            dialog.Filter.Add(new FilterEntry(Properties.Resources.htmlFiles, Properties.Resources.htmlExtension));
            dialog.Title = Properties.Resources.Export;
            dialog.DefaultExtension = Properties.Resources.DefaulthtmlExtension;
            dialog.ShowSave();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {

                Person p = this.family.Current;
                string filename = dialog.FileName;

                tw = new StreamWriter(filename);
                //write the necessary html code for a html document
                tw.WriteLine(Header());
                tw.WriteLine(CSS());
                tw.WriteLine(CSSprinting(9));

                tw.WriteLine("</head><body>");
                tw.WriteLine("<h2>Family.Show</h2>");

                #region export citations

                tw.WriteLine("<i>Summary of citations for " + p.FullName + "</i><br/><br/>");

                //write the table headers
                tw.WriteLine("<table id=\"citationtable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
                "<thead>\n" +
                "<tr>\n" +
                "<th width=\"10%\">Field</th>\n" +
                "<th width=\"15%\">Value</th>\n" +
                "<th width=\"15%\">Citation</th>\n" +
                "<th width=\"10%\">Actual Text</th>\n" +
                "<th width=\"10%\">Note</th>\n" +
                "<th width=\"10%\">Link</th>\n" +
                "<th width=\"10%\">Source</th>\n" +
                "</tr>\n" +
                "</thead>");

                string birthLink = string.Empty;
                string occupationLink = string.Empty;
                string educationLink = string.Empty;
                string religionLink = string.Empty;

                //ensure only fields with link values are exported

                if (!string.IsNullOrEmpty(p.BirthLink) && (p.BirthLink.StartsWith(Properties.Resources.www) || p.BirthLink.StartsWith(Properties.Resources.http)))
                    birthLink = "[<a href=\"" + p.BirthLink + "\">Link</a>]";
                if (!string.IsNullOrEmpty(p.OccupationLink) && (p.OccupationLink.StartsWith(Properties.Resources.www) || p.OccupationLink.StartsWith(Properties.Resources.http)))
                    occupationLink = "[<a href=\"" + p.OccupationLink + "\">Link</a>]";
                if (!string.IsNullOrEmpty(p.EducationLink) && (p.EducationLink.StartsWith(Properties.Resources.www) || p.EducationLink.StartsWith(Properties.Resources.http)))
                    educationLink = "[<a href=\"" + p.EducationLink + "\">Link</a>]";
                if (!string.IsNullOrEmpty(p.ReligionLink) && (p.ReligionLink.StartsWith(Properties.Resources.www) || p.ReligionLink.StartsWith(Properties.Resources.http)))
                    religionLink = "[<a href=\"" + p.ReligionLink + "\">Link</a>]";

                tw.WriteLine("<tr><td>Birth</td><td>" + p.BirthDateDescriptor + " " + dateformat(p.BirthDate) + " " + p.BirthPlace + "</td><td>" + p.BirthCitation + "</td><td>" + p.BirthCitationActualText + "</td><td>" + p.BirthCitationNote + "</td><td>" + birthLink + "</td><td>" + p.BirthSource + "</td></tr>");
                tw.WriteLine("<tr><td>Occupation</td><td>" + p.Occupation + "</td><td>" + p.OccupationCitation + "</td><td>" + p.OccupationCitationActualText + "</td><td>" + p.OccupationCitationNote + "</td><td>" + occupationLink + "</td><td>" + p.OccupationSource + "</td></tr>");
                tw.WriteLine("<tr><td>Education</td><td>" + p.Education + "</td><td>" + p.EducationCitation + "</td><td>" + p.EducationCitationActualText + "</td><td>" + p.EducationCitationNote + "</td><td>" + educationLink + "</td><td>" + p.EducationSource + "</td></tr>");
                tw.WriteLine("<tr><td>Religion</td><td>" + p.Religion + "</td><td>" + p.ReligionCitation + "</td><td>" + p.ReligionCitationActualText + "</td><td>" + p.ReligionCitationNote + "</td><td>" + religionLink + "</td><td>" + p.ReligionSource + "</td></tr>");

                foreach (Relationship rel in p.Relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Spouse)
                    {
                        string m1 = string.Empty;  //date descriptor
                        string m2 = string.Empty;  //date
                        string m3 = string.Empty;  //place
                        string m4 = string.Empty;  //citation
                        string m5 = string.Empty;  //source
                        string m6 = string.Empty;  //link
                        string m7 = string.Empty;  //normal
                        string m8 = string.Empty;  //actual text

                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageDateDescriptor))
                            m1 = ((SpouseRelationship)rel).MarriageDateDescriptor.ToString();
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageDate.ToString()))
                            m2 = dateformat(((SpouseRelationship)rel).MarriageDate);
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriagePlace))
                            m3 = ((SpouseRelationship)rel).MarriagePlace;
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageCitation))
                            m4 = ((SpouseRelationship)rel).MarriageCitation;
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageSource))
                            m5 = ((SpouseRelationship)rel).MarriageSource;
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageLink))  //ensure only fields with link values are exported
                        {
                            if (((SpouseRelationship)rel).MarriageLink.StartsWith(Properties.Resources.www) || ((SpouseRelationship)rel).MarriageLink.StartsWith(Properties.Resources.http))
                                m6 = "[<a href=\"" + ((SpouseRelationship)rel).MarriageLink + "\">Link</a>]";
                        }
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageCitationNote))
                            m7 = ((SpouseRelationship)rel).MarriageCitationNote;
                        if (!string.IsNullOrEmpty(((SpouseRelationship)rel).MarriageCitationActualText))
                            m8 = ((SpouseRelationship)rel).MarriageCitationActualText;

                        tw.WriteLine("<tr><td>Marriage</td><td>" + rel.RelationTo + " " + m1 + " " + m2 + " " + m3 + "</td><td>" + m4 + "</td><td>" + m8 + "</td><td>" + m7 + "</td><td>" + m6 + "</td><td>" + m5 + "</td><td></td></tr>");

                        if (((SpouseRelationship)rel).SpouseModifier == SpouseModifier.Former)
                        {
                            string d1 = string.Empty;  //date descriptor
                            string d2 = string.Empty;  //divorce date
                            string d3 = string.Empty;  //divorce citation 
                            string d4 = string.Empty;  //divorce source
                            string d6 = string.Empty;  //divorce link
                            string d7 = string.Empty;  //divorce citation note
                            string d8 = string.Empty;  //divorce citation actual text 

                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceDateDescriptor))
                                d1 = ((SpouseRelationship)rel).DivorceDateDescriptor.ToString();
                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceDate.ToString()))
                                d2 = dateformat(((SpouseRelationship)rel).DivorceDate);
                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceCitation))
                                d3 = ((SpouseRelationship)rel).DivorceCitation;
                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceSource))
                                d4 = ((SpouseRelationship)rel).DivorceSource;
                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceLink))  //ensure only fields with link values are exported
                            {
                                if (((SpouseRelationship)rel).DivorceLink.StartsWith(Properties.Resources.www) || ((SpouseRelationship)rel).DivorceLink.StartsWith(Properties.Resources.http))
                                    d6 = "[<a href=\"" + ((SpouseRelationship)rel).DivorceLink + "\">Link</a>]";
                            }
                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceCitationNote))
                                d7 = ((SpouseRelationship)rel).DivorceCitationNote;
                            if (!string.IsNullOrEmpty(((SpouseRelationship)rel).DivorceCitationActualText))
                                d8 = ((SpouseRelationship)rel).DivorceCitationActualText;

                            tw.WriteLine("<tr><td>Divorce</td><td>" + rel.RelationTo + " " + d1 + " " + d2 + "</td><td>" + d8 + "</td><td>" + d7 + "</td><td>" + d6 + "</td><td>" + d4 + "</td><td></td></tr>");

                        }
                    }
                }

                if (!p.IsLiving)  //only export death related info for people who are dead
                {
                    string deathLink = string.Empty;
                    string burialLink = string.Empty;
                    string cremationLink = string.Empty;

                    //ensure only fields with link values are exported

                    if (!string.IsNullOrEmpty(p.DeathLink) && (p.DeathLink.StartsWith(Properties.Resources.www) || p.DeathLink.StartsWith(Properties.Resources.http)))
                        deathLink = "[<a href=\"" + p.DeathLink + "\">Link</a>]";
                    if (!string.IsNullOrEmpty(p.BurialLink) && (p.BurialLink.StartsWith(Properties.Resources.www) || p.BurialLink.StartsWith(Properties.Resources.http)))
                        burialLink = "[<a href=\"" + p.BurialLink + "\">Link</a>]";
                    if (!string.IsNullOrEmpty(p.CremationLink) && (p.CremationLink.StartsWith(Properties.Resources.www) || p.CremationLink.StartsWith(Properties.Resources.http)))
                        cremationLink = "[<a href=\"" + p.CremationLink + "\">Link</a>]";

                    tw.WriteLine("<tr><td>Death</td><td>" + p.DeathDateDescriptor + " " + dateformat(p.DeathDate) + " " + p.DeathPlace + "</td><td>" + p.DeathCitation + "</td><td>" + p.DeathCitationActualText + "</td><td>" + p.DeathCitationNote + "</td><td>" + deathLink + "</td><td>" + p.DeathSource + "</td></tr>");
                    tw.WriteLine("<tr><td>Burial</td><td>" + p.BurialDateDescriptor + " " + dateformat(p.BurialDate) + " " + p.BurialPlace + "</td><td>" + p.BurialCitation + "</td><td>" + p.BurialCitationActualText + "</td><td>" + p.BurialCitationNote + "</td><td>" + burialLink + "</td><td>" + p.BurialSource + "</td></tr>");
                    tw.WriteLine("<tr><td>Cremation</td><td>" + p.CremationDateDescriptor + " " + dateformat(p.CremationDate) + " " + p.CremationPlace + "</td><td>" + p.CremationCitation + "</td><td>" + p.CremationCitationActualText + "</td><td>" + p.CremationCitationNote + "</td><td>" + cremationLink + "</td><td>" + p.CremationSource + "</td></tr>");
                }
                #endregion

                #region export sources

                if (sources.Count > 0)  //only export sources if there are sources to export
                {
                    tw.WriteLine("</table><br/><br/><i>Summary of sources</i><br/><br/>");
                    //Export column headers
                    tw.WriteLine(NormalSourceColumns());

                    //Export Sources
                    foreach (Source s in sources)
                        tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
                }
                tw.WriteLine(Footer());
                tw.Close();

                #endregion

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

        /// <summary>
        /// Writes the table headers for source export
        /// </summary>
        private static string NormalSourceColumns()
        {
            return "<table id=\"sourcetable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
            "<thead>\n" +
            "<tr>\n" +
            "<th width=\"10%\">Source</th>\n" +
            "<th width=\"15%\">Name</th>\n" +
            "<th width=\"15%\">Author</th>\n" +
            "<th width=\"15%\">Publisher</th>\n" +
            "<th width=\"15%\">Note</th>\n" +
            "<th width=\"10%\">Repository</th>\n" +
            "</tr>\n" +
            "</thead>";
        }

        /// <summary>
        /// Write the header information
        /// </summary>
        private static string Header()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n" +
                    "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n" +
                    "<head>\n" +
                    "<title>Family.Show</title>";
        }

        /// <summary>
        /// Write the CSS information
        /// </summary>
        private static string CSS()
        {
            return "<style type=\"text/css\">\n" +

                    "body { background-color: white; font-family: Calibri, Tahoma, sans-serif; font-size: 12px; line-height: 1.2; padding: 1em; color: #2E2E2E; }\n" +

                    "table { border: 0.5px gray solid; width: 100%; empty-cells: show; }\n" +
                    "th, td { border: 0.5px gray solid; padding: 0.5em; vertical-align: top; }\n" +
                    "td { text-align: left; }\n" +
                    "th { background-color: #F0F8FF; }\n" +
                    "td a { color: navy; text-decoration: none; }\n" +
                    "td a:hover  { text-decoration: underline; }";
        }

        /// <summary>
        /// Write the CSS printing information.  
        /// This method ensures that the correct number of columns is printed.
        /// The argument passed should be one more than the number of columns to print.
        /// </summary>
        private static string CSSprinting(int i)
        {
            string printstyle = "@media print {\n" +
                                "table { border-width: 0px; }\n" +
                                "tr { page-break-inside: avoid; }\n" +
                                "tr >";

            for (int j = 1; j <= i; j++)
            {
                if (i != j)
                    printstyle += "*+";
                else
                    printstyle += "*";
            }

            printstyle += "{display: none; }\n" +
                            "}\n" +
                            "</style>";

            return printstyle;
        }

        /// <summary>
        /// Write the Footer information
        /// </summary>
        private static string Footer()
        {
            //write the software version and the date and time to the file
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string versionlabel = string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            string date = DateTime.Now.ToString();
            return "</table><br/><p><i>Generated by Family.Show Version " + versionlabel + " on " + date + "</i></p></body></html>";
        }

        /// <summary>
        /// Get a date in dd/mm/yyyy format from a full DateTime?
        /// </summary>
        private static string dateformat(DateTime? dates)
        {
            string date = string.Empty;
            if (dates != null)  //don't try if date is null!
            {
                int month = dates.Value.Month;
                int day = dates.Value.Day;
                int year = dates.Value.Year;
                date = day + "/" + month + "/" + year;
            }
            return date;
        }

        private void DoneEditCitationsButton_Click(object sender, RoutedEventArgs e)
        {
            // Let the collection know that it has been updated so that the diagram control will update.
            family.OnContentChanged();
        }

        private void CitationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCitationsCombobox();
        }

        /// <summary>
        /// Updates the citation panel based on the Citations combo box selected item.  
        /// If selected index of Citations combo box is -1 then the control is cleared.
        /// </summary>
        private void UpdateCitationsCombobox()
        {
            if (family.Current == null)
                return;

            Person p = family.Current;
            {
                CitationDetailsEditTextBox.Text = string.Empty;
                CitationActualTextEditTextBox.Text = string.Empty;
                CitationLinkEditTextBox.Text = string.Empty;
                CitationNoteEditTextBox.Text = string.Empty;
                SourceEditTextBox.Text = string.Empty;
                SourceEditTextBox.ToolTip = null;

                CitationDetailsEditTextBox.IsEnabled = false;
                CitationActualTextEditTextBox.IsEnabled = false;
                CitationLinkEditTextBox.IsEnabled = false;
                CitationNoteEditTextBox.IsEnabled = false;
                SourceEditTextBox.IsEnabled = false;

                if (CitationsComboBox.SelectedIndex != -1)
                {
                    CitationDetailsEditTextBox.IsEnabled = true;
                    CitationActualTextEditTextBox.IsEnabled = true;
                    CitationLinkEditTextBox.IsEnabled = true;
                    CitationNoteEditTextBox.IsEnabled = true;
                    SourceEditTextBox.IsEnabled = true;

                    //Enable the correct fields
                    if (p.IsLiving == true && p.Restriction != Restriction.Locked)
                    {
                        if ((CitationsComboBoxValue)CitationsComboBox.SelectedValue == CitationsComboBoxValue.Burial ||
                            (CitationsComboBoxValue)CitationsComboBox.SelectedValue == CitationsComboBoxValue.Death ||
                            (CitationsComboBoxValue)CitationsComboBox.SelectedValue == CitationsComboBoxValue.Cremation)
                        {
                            CitationDetailsEditTextBox.IsEnabled = false;
                            CitationActualTextEditTextBox.IsEnabled = false;
                            CitationLinkEditTextBox.IsEnabled = false;
                            CitationNoteEditTextBox.IsEnabled = false;
                            SourceEditTextBox.IsEnabled = false;
                        }
                    }

                    if (p.Restriction == Restriction.Locked)
                    {
                        CitationDetailsEditTextBox.IsEnabled = false;
                        CitationActualTextEditTextBox.IsEnabled = false;
                        CitationLinkEditTextBox.IsEnabled = false;
                        CitationNoteEditTextBox.IsEnabled = false;
                        SourceEditTextBox.IsEnabled = false;
                    }

                    Source s = new Source();

                    switch ((CitationsComboBoxValue)CitationsComboBox.SelectedValue)
                    {
                        case CitationsComboBoxValue.Birth:

                            if (p.BirthCitation != null)
                                CitationDetailsEditTextBox.Text = p.BirthCitation;
                            if (p.BirthCitationNote != null)
                                CitationNoteEditTextBox.Text = p.BirthCitationNote;
                            if (p.BirthCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.BirthCitationActualText;
                            if (p.BirthLink != null)
                                CitationLinkEditTextBox.Text = p.BirthLink;
                            if (p.BirthSource != null)
                                SourceEditTextBox.Text = p.BirthSource;

                            s = sources.Find(p.BirthSource);

                            if (p.BirthSource != null && s != null)
                                SourceEditTextBox.ToolTip = s.SourceName;
                            break;
                        case CitationsComboBoxValue.Death:
                            if (p.DeathCitation != null)
                                CitationDetailsEditTextBox.Text = p.DeathCitation;
                            if (p.DeathCitationNote != null)
                                CitationNoteEditTextBox.Text = p.DeathCitationNote;
                            if (p.DeathCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.DeathCitationActualText;
                            if (p.DeathLink != null)
                                CitationLinkEditTextBox.Text = p.DeathLink;
                            if (p.DeathSource != null)
                                SourceEditTextBox.Text = p.DeathSource;

                            s = sources.Find(p.DeathSource);

                            if (p.DeathSource != null && s != null)
                            {
                                SourceEditTextBox.ToolTip = s.SourceName;
                            }
                            break;
                        case CitationsComboBoxValue.Education:
                            if (p.EducationCitation != null)
                                CitationDetailsEditTextBox.Text = p.EducationCitation;
                            if (p.EducationCitationNote != null)
                                CitationNoteEditTextBox.Text = p.EducationCitationNote;
                            if (p.EducationCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.EducationCitationActualText;
                            if (p.EducationLink != null)
                                CitationLinkEditTextBox.Text = p.EducationLink;
                            if (p.EducationSource != null)
                                SourceEditTextBox.Text = p.EducationSource;

                            s = sources.Find(p.EducationSource);

                            if (p.EducationSource != null && s != null)
                                SourceEditTextBox.ToolTip = s.SourceName;
                            break;
                        case CitationsComboBoxValue.Occupation:
                            if (p.OccupationCitation != null)
                                CitationDetailsEditTextBox.Text = p.OccupationCitation;
                            if (p.OccupationCitationNote != null)
                                CitationNoteEditTextBox.Text = p.OccupationCitationNote;
                            if (p.OccupationCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.OccupationCitationActualText;
                            if (p.OccupationLink != null)
                                CitationLinkEditTextBox.Text = p.OccupationLink;
                            if (p.OccupationSource != null)
                                SourceEditTextBox.Text = p.OccupationSource;

                            s = sources.Find(p.OccupationSource);

                            if (p.OccupationSource != null && s != null)
                                SourceEditTextBox.ToolTip = s.SourceName;
                            break;
                        case CitationsComboBoxValue.Religion:
                            if (p.ReligionCitation != null)
                                CitationDetailsEditTextBox.Text = p.ReligionCitation;
                            if (p.ReligionCitationNote != null)
                                CitationNoteEditTextBox.Text = p.ReligionCitationNote;
                            if (p.ReligionCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.ReligionCitationActualText;
                            if (p.ReligionLink != null)
                                CitationLinkEditTextBox.Text = p.ReligionLink;
                            if (p.ReligionSource != null)
                                SourceEditTextBox.Text = p.ReligionSource;

                            s = sources.Find(p.ReligionSource);

                            if (p.ReligionSource != null && s != null)
                                SourceEditTextBox.ToolTip = s.SourceName;
                            break;
                        case CitationsComboBoxValue.Burial:
                            if (p.BurialCitation != null)
                                CitationDetailsEditTextBox.Text = p.BurialCitation;
                            if (p.BurialCitationNote != null)
                                CitationNoteEditTextBox.Text = p.BurialCitationNote;
                            if (p.BurialCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.BurialCitationActualText;
                            if (p.BurialLink != null)
                                CitationLinkEditTextBox.Text = p.BurialLink;
                            if (p.BurialSource != null)
                                SourceEditTextBox.Text = p.BurialSource;

                            s = sources.Find(p.BurialSource);

                            if (p.BurialSource != null && s != null)
                                SourceEditTextBox.ToolTip = s.SourceName;
                            break;
                        case CitationsComboBoxValue.Cremation:
                            if (p.CremationCitation != null)
                                CitationDetailsEditTextBox.Text = p.CremationCitation;
                            if (p.CremationCitationNote != null)
                                CitationNoteEditTextBox.Text = p.CremationCitationNote;
                            if (p.CremationCitationActualText != null)
                                CitationActualTextEditTextBox.Text = p.CremationCitationActualText;
                            if (p.CremationLink != null)
                                CitationLinkEditTextBox.Text = p.CremationLink;
                            if (p.CremationSource != null)
                                SourceEditTextBox.Text = p.CremationSource;

                            s = sources.Find(p.CremationSource);

                            if (p.CremationSource != null && s != null)
                                SourceEditTextBox.ToolTip = s.SourceName;

                            break;
                    }
                }
            }
            family.OnContentChanged();
        }
       
        private void CitationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (family.Current == null)
                return;

            Person p = family.Current;

            if (CitationsComboBox.SelectedItem != null)
            {
                switch ((CitationsComboBoxValue)CitationsComboBox.SelectedValue)
                {
                    case CitationsComboBoxValue.Birth:
                        p.BirthCitation = CitationDetailsEditTextBox.Text;
                        p.BirthSource = SourceEditTextBox.Text;
                        p.BirthCitationNote = CitationNoteEditTextBox.Text;
                        p.BirthCitationActualText = CitationActualTextEditTextBox.Text;
                        p.BirthCitation = CitationDetailsEditTextBox.Text;
                        p.BirthLink = CitationLinkEditTextBox.Text;
                        break;
                    case CitationsComboBoxValue.Death:
                        p.DeathCitation = CitationDetailsEditTextBox.Text;
                        p.DeathSource = SourceEditTextBox.Text;
                        p.DeathCitationNote = CitationNoteEditTextBox.Text;
                        p.DeathCitationActualText = CitationActualTextEditTextBox.Text;
                        p.DeathCitation = CitationDetailsEditTextBox.Text;
                        p.DeathLink = CitationLinkEditTextBox.Text;
                        break;
                    case CitationsComboBoxValue.Education:
                        p.EducationCitation = CitationDetailsEditTextBox.Text;
                        p.EducationSource = SourceEditTextBox.Text;
                        p.EducationCitationNote = CitationNoteEditTextBox.Text;
                        p.EducationCitationActualText = CitationActualTextEditTextBox.Text;
                        p.EducationCitation = CitationDetailsEditTextBox.Text;
                        p.EducationLink = CitationLinkEditTextBox.Text;
                        break;
                    case CitationsComboBoxValue.Occupation:
                        p.OccupationCitation = CitationDetailsEditTextBox.Text;
                        p.OccupationSource = SourceEditTextBox.Text;
                        p.OccupationCitationNote = CitationNoteEditTextBox.Text;
                        p.OccupationCitationActualText = CitationActualTextEditTextBox.Text;
                        p.OccupationCitation = CitationDetailsEditTextBox.Text;
                        p.OccupationLink = CitationLinkEditTextBox.Text;
                        break;
                    case CitationsComboBoxValue.Religion:
                        p.ReligionCitation = CitationDetailsEditTextBox.Text;
                        p.ReligionSource = SourceEditTextBox.Text;
                        p.ReligionCitationNote = CitationNoteEditTextBox.Text;
                        p.ReligionCitationActualText = CitationActualTextEditTextBox.Text;
                        p.ReligionCitation = CitationDetailsEditTextBox.Text;
                        p.ReligionLink = CitationLinkEditTextBox.Text;
                        break;
                    case CitationsComboBoxValue.Burial:
                        p.BurialCitation = CitationDetailsEditTextBox.Text;
                        p.BurialSource = SourceEditTextBox.Text;
                        p.BurialCitationNote = CitationNoteEditTextBox.Text;
                        p.BurialCitationActualText = CitationActualTextEditTextBox.Text;
                        p.BurialCitation = CitationDetailsEditTextBox.Text;
                        p.BurialLink = CitationLinkEditTextBox.Text;
                        break;
                    case CitationsComboBoxValue.Cremation:
                        p.CremationCitation = CitationDetailsEditTextBox.Text;
                        p.CremationSource = SourceEditTextBox.Text;
                        p.CremationCitationNote = CitationNoteEditTextBox.Text;
                        p.CremationCitationActualText = CitationActualTextEditTextBox.Text;
                        p.CremationCitation = CitationDetailsEditTextBox.Text;
                        p.CremationLink = CitationLinkEditTextBox.Text;
                        break;
                }
                family.OnContentChanged();

            }
        }

        private void CitationLinkSearchClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.CitationLinkEditTextBox.Text.ToString() != null && this.CitationLinkEditTextBox.Text.ToString().Length > 0)
                {
                    string searchText = this.CitationLinkEditTextBox.Text.ToString(); ;
                    if (searchText.StartsWith(Properties.Resources.www) || searchText.StartsWith(Properties.Resources.http))
                        System.Diagnostics.Process.Start(searchText);
                    else
                        MessageBox.Show(Properties.Resources.InvalidURL, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch { }
        }

        #endregion

        #region Details Edit event handlers

        private void SearchMapBirthPlace(object sender, RoutedEventArgs e)
        {
            SearchMap(this.family.Current.BirthPlace.ToString());
        }

        private void SearchMapDeathPlace(object sender, RoutedEventArgs e)
        {
            SearchMap(this.family.Current.DeathPlace.ToString());
        }

        private void ChangeBirthDescriptorForward(object sender, RoutedEventArgs e)
        {
            this.family.Current.BirthDateDescriptor = forwardDateDescriptor(this.family.Current.BirthDateDescriptor);
        }

        private void ChangeBirthDescriptorBackward(object sender, RoutedEventArgs e)
        {
            this.family.Current.BirthDateDescriptor = backwardDateDescriptor(this.family.Current.BirthDateDescriptor);
        }

        private void ChangeDeathDescriptorForward(object sender, RoutedEventArgs e)
        {
            this.family.Current.DeathDateDescriptor = forwardDateDescriptor(this.family.Current.DeathDateDescriptor);
        }

        private void ChangeDeathDescriptorBackward(object sender, RoutedEventArgs e)
        {
            this.family.Current.DeathDateDescriptor = backwardDateDescriptor(this.family.Current.DeathDateDescriptor);
        }

        private void DoneEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the data binding is updated for fields that update during LostFocus.
            // This is necessary since this method can be invoked when the Enter key is pressed,
            // but the text field has not lost focus yet, so it does not update binding. This
            // manually updates the binding for those fields.
            if (BirthDateEditTextBox.IsFocused)
                BirthDateEditTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (DeathDateEditTextBox.IsFocused)
                DeathDateEditTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            // Let the collection know that it has been updated so that the diagram control will update.
            family.OnContentChanged();

            //This must be called to ensure that if a person's restriction changes
            //the appropriate fields in the relationship citations panel become readonly/editable.
            UpdateRCitationsComboBox();


        }

        #endregion

        #region Details Edit More event handlers

        private void DoneEditMoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the data binding is updated for fields that update during LostFocus.
            // This is necessary since this method can be invoked when the Enter key is pressed,
            // but the text field has not lost focus yet, so it does not update binding. This
            // manually updates the binding for those fields.
            if (BurialDateEditTextBox.IsFocused)
                BurialDateEditTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (CremationDateEditTextBox.IsFocused)
                CremationDateEditTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            // Let the collection know that it has been updated so that the diagram control will update.
            family.OnContentChanged();
        }

        private void ChangeBurialDescriptorForward(object sender, RoutedEventArgs e)
        {
            this.family.Current.BurialDateDescriptor = forwardDateDescriptor(this.family.Current.BurialDateDescriptor);
        }

        private void ChangeBurialDescriptorBackward(object sender, RoutedEventArgs e)
        {
            this.family.Current.BurialDateDescriptor = backwardDateDescriptor(this.family.Current.BurialDateDescriptor);
        }

        private void ChangeCremationDescriptorForward(object sender, RoutedEventArgs e)
        {
            this.family.Current.CremationDateDescriptor = forwardDateDescriptor(this.family.Current.CremationDateDescriptor);
        }

        private void ChangeCremationDescriptorBackward(object sender, RoutedEventArgs e)
        {
            this.family.Current.CremationDateDescriptor = backwardDateDescriptor(this.family.Current.CremationDateDescriptor);
        }

        private void SearchMapBurialPlace(object sender, RoutedEventArgs e)
        {
            SearchMap(this.family.Current.BurialPlace.ToString());
        }

        private void SearchMapCremationPlace(object sender, RoutedEventArgs e)
        {
            SearchMap(this.family.Current.CremationPlace.ToString());
        }

        #endregion

        #region Details Edit Relationships event handlers

        private void DoneEditRelationshipButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RCitationLinkSearchClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.RCitationLinkEditTextBox.Text.ToString() != null && this.RCitationLinkEditTextBox.Text.ToString().Length > 0)
                {
                    string searchText = this.RCitationLinkEditTextBox.Text.ToString(); ;
                    if (searchText.StartsWith(Properties.Resources.www) || searchText.StartsWith(Properties.Resources.http))
                        System.Diagnostics.Process.Start(searchText);
                    else
                        MessageBox.Show(Properties.Resources.InvalidURL, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch { }
        }

        private void SpouseStatusListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (SpouseStatusListbox.SelectedItem != null)
            {

                switch ((SpouseModifier)SpouseStatusListbox.SelectedItem)
                {
                    case SpouseModifier.Former:
                        SpouseModifer.Content = Microsoft.FamilyShow.Properties.Resources.Former;
                        break;
                    case SpouseModifier.Current:
                        SpouseModifer.Content = Microsoft.FamilyShow.Properties.Resources.Current;
                        break;
                }

                RelationshipHelper.UpdateSpouseStatus(family.Current, (Person)SpousesCombobox.SelectedItem, (SpouseModifier)SpouseStatusListbox.SelectedItem);

                //Some fields are only editable for former spouses.
                ToEditTextBox.IsEnabled = ((SpouseModifier)SpouseStatusListbox.SelectedItem == SpouseModifier.Former);
                ToLabel.IsEnabled = ((SpouseModifier)SpouseStatusListbox.SelectedItem == SpouseModifier.Former);

                UpdateRCitationsComboBox();
            }
            family.OnContentChanged();
        }

        private void SpousesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (family.Current == null)
                return;

            if (SpousesCombobox.SelectedItem != null)
            {
                RelationshipsCitationsComboBox.SelectedIndex = 0;

                foreach (Relationship rel in family.Current.Relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Spouse && rel.RelationTo.Equals((Person)SpousesCombobox.SelectedItem))
                    {
                        SpouseRelationship spouseRel = ((SpouseRelationship)rel);
                        SpouseStatusListbox.SelectedItem = spouseRel.SpouseModifier;

                        FromEditTextBox.Text = string.Empty;
                        ToEditTextBox.Text = string.Empty;
                        PlaceEditTextBox.Text = string.Empty;
                        ToDescriptor.Content = string.Empty;
                        FromDescriptor.Content = string.Empty;

                        if (spouseRel.MarriageDate.HasValue)
                            FromEditTextBox.Text = ((DateTime)spouseRel.MarriageDate).ToShortDateString();
                        if (spouseRel.DivorceDate.HasValue)
                            ToEditTextBox.Text = ((DateTime)spouseRel.DivorceDate).ToShortDateString();
                        if (spouseRel.MarriagePlace != null)
                            PlaceEditTextBox.Text = spouseRel.MarriagePlace;
                        if (spouseRel.MarriageDateDescriptor != null)
                            FromDescriptor.Content = spouseRel.MarriageDateDescriptor;
                        if (spouseRel.DivorceDateDescriptor != null)
                            ToDescriptor.Content = spouseRel.DivorceDateDescriptor;

                        UpdateRCitationsComboBox();
                    }
                }
            }
        }

        private void RelationshipsCitationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRCitationsComboBox();
        }

        private void UpdateRCitationsComboBox()
        {
            if (family.Current == null)
                return;

            Person p = (Person)this.DataContext;

            RCitationDetailsEditTextBox.Text = string.Empty;
            RCitationActualTextEditTextBox.Text = string.Empty;
            RCitationLinkEditTextBox.Text = string.Empty;
            RCitationNoteEditTextBox.Text = string.Empty;
            RSourceEditTextBox.Text = string.Empty;
            RSourceEditTextBox.ToolTip = null;

            RCitationDetailsEditTextBox.IsEnabled = true;
            RCitationActualTextEditTextBox.IsEnabled = true;
            RCitationLinkEditTextBox.IsEnabled = true;
            RCitationNoteEditTextBox.IsEnabled = true;
            RSourceEditTextBox.IsEnabled = true;

            if (RelationshipsCitationsComboBox.SelectedIndex != -1 && SpousesCombobox.SelectedItem != null)
            {

                foreach (Relationship rel in family.Current.Relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Spouse && rel.RelationTo.Equals((Person)SpousesCombobox.SelectedItem))
                    {
                        SpouseRelationship spouseRel = ((SpouseRelationship)rel);
                        SpouseStatusListbox.SelectedItem = spouseRel.SpouseModifier;

                        if (RelationshipsCitationsComboBox.SelectedItem != null)
                        {
                            if ((SpouseModifier)SpouseStatusListbox.SelectedItem == SpouseModifier.Current)
                            {
                                if ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue == RCitationsComboBoxValue.Divorce)
                                {
                                    RCitationDetailsEditTextBox.IsEnabled = false;
                                    RCitationActualTextEditTextBox.IsEnabled = false;
                                    RCitationLinkEditTextBox.IsEnabled = false;
                                    RCitationNoteEditTextBox.IsEnabled = false;
                                    RSourceEditTextBox.IsEnabled = false;
                                }
                            }

                            if (p.Restriction == Restriction.Locked)
                            {
                                RCitationDetailsEditTextBox.IsEnabled = false;
                                RCitationActualTextEditTextBox.IsEnabled = false;
                                RCitationLinkEditTextBox.IsEnabled = false;
                                RCitationNoteEditTextBox.IsEnabled = false;
                                RSourceEditTextBox.IsEnabled = false;
                            }

                            switch ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue)
                            {
                                case RCitationsComboBoxValue.Divorce:

                                    if (spouseRel.DivorceCitation != null)
                                        RCitationDetailsEditTextBox.Text = spouseRel.DivorceCitation;
                                    if (spouseRel.DivorceCitationNote != null)
                                        RCitationNoteEditTextBox.Text = spouseRel.DivorceCitationNote;
                                    if (spouseRel.DivorceLink != null)
                                        RCitationLinkEditTextBox.Text = spouseRel.DivorceLink;
                                    if (spouseRel.DivorceSource != null)
                                        RSourceEditTextBox.Text = spouseRel.DivorceSource;
                                    if (spouseRel.DivorceCitationActualText != null)
                                        RCitationActualTextEditTextBox.Text = spouseRel.DivorceCitationActualText;

                                    Source s = sources.Find(spouseRel.DivorceSource);

                                    if (s != null)
                                        RSourceEditTextBox.ToolTip = s.SourceName;
                                    else
                                        RSourceEditTextBox.ToolTip = null;

                                    break;
                                case RCitationsComboBoxValue.Marriage:

                                    if (spouseRel.MarriageCitation != null)
                                        RCitationDetailsEditTextBox.Text = spouseRel.MarriageCitation;
                                    if (spouseRel.MarriageCitationNote != null)
                                        RCitationNoteEditTextBox.Text = spouseRel.MarriageCitationNote;
                                    if (spouseRel.MarriageLink != null)
                                        RCitationLinkEditTextBox.Text = spouseRel.MarriageLink;
                                    if (spouseRel.MarriageSource != null)
                                        RSourceEditTextBox.Text = spouseRel.MarriageSource;
                                    if (spouseRel.MarriageCitationActualText != null)
                                        RCitationActualTextEditTextBox.Text = spouseRel.MarriageCitationActualText;

                                    Source s1 = sources.Find(spouseRel.MarriageSource);

                                    if (s1 != null)
                                        RSourceEditTextBox.ToolTip = s1.SourceName;
                                    else
                                        RSourceEditTextBox.ToolTip = null;

                                    break;

                            }
                        }
                    }
                }
            }
            family.OnContentChanged();
        }

        private void RCitationLinkEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (family.Current == null)
                return;

            if (RelationshipsCitationsComboBox.SelectedItem != null)
            {
                string link = RCitationLinkEditTextBox.Text;

                switch ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue)
                {
                    case RCitationsComboBoxValue.Marriage:
                        RelationshipHelper.UpdateMarriageLink(family.Current, (Person)SpousesCombobox.SelectedItem, link);
                        break;
                    case RCitationsComboBoxValue.Divorce:
                        RelationshipHelper.UpdateDivorceLink(family.Current, (Person)SpousesCombobox.SelectedItem, link);
                        break;
                }
                family.OnContentChanged();

            }
        }

        private void RCitationDetailsEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            if (family.Current == null)
                return;

            if (RelationshipsCitationsComboBox.SelectedItem != null)
            {
                string CitationDetails = RCitationDetailsEditTextBox.Text;

                switch ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue)
                {
                    case RCitationsComboBoxValue.Marriage:
                        RelationshipHelper.UpdateMarriageCitation(family.Current, (Person)SpousesCombobox.SelectedItem, CitationDetails);
                        break;
                    case RCitationsComboBoxValue.Divorce:
                        RelationshipHelper.UpdateDivorceCitation(family.Current, (Person)SpousesCombobox.SelectedItem, CitationDetails);
                        break;
                }
                family.OnContentChanged();

            }

        }

        private void RCitationActualTextEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            if (family.Current == null)
                return;

            if (RelationshipsCitationsComboBox.SelectedItem != null)
            {
                string CitationActualText = RCitationActualTextEditTextBox.Text;

                switch ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue)
                {
                    case RCitationsComboBoxValue.Marriage:
                        RelationshipHelper.UpdateMarriageCitationActualText(family.Current, (Person)SpousesCombobox.SelectedItem, CitationActualText);
                        break;
                    case RCitationsComboBoxValue.Divorce:
                        RelationshipHelper.UpdateDivorceCitationActualText(family.Current, (Person)SpousesCombobox.SelectedItem, CitationActualText);
                        break;
                }
                family.OnContentChanged();
            }

        }

        private void RCitationNoteEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (family.Current == null)
                return;

            if (RelationshipsCitationsComboBox.SelectedItem != null)
            {
                string CitationNote = RCitationNoteEditTextBox.Text;

                switch ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue)
                {
                    case RCitationsComboBoxValue.Marriage:
                        RelationshipHelper.UpdateMarriageCitationNote(family.Current, (Person)SpousesCombobox.SelectedItem, CitationNote);
                        break;
                    case RCitationsComboBoxValue.Divorce:
                        RelationshipHelper.UpdateDivorceCitationNote(family.Current, (Person)SpousesCombobox.SelectedItem, CitationNote);
                        break;
                }
                family.OnContentChanged();
            }
        }

        private void RSourceEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (family.Current == null)
                return;

            Source s = sources.Find(RSourceEditTextBox.Text.ToString());

            if (s != null)
                RSourceEditTextBox.ToolTip = s.SourceName;
            else
                RSourceEditTextBox.ToolTip = null;

            if (RelationshipsCitationsComboBox.SelectedItem != null)
            {
                string Source = RSourceEditTextBox.Text;

                switch ((RCitationsComboBoxValue)RelationshipsCitationsComboBox.SelectedValue)
                {
                    case RCitationsComboBoxValue.Marriage:
                        RelationshipHelper.UpdateMarriageSource(family.Current, (Person)SpousesCombobox.SelectedItem, Source);
                        break;
                    case RCitationsComboBoxValue.Divorce:
                        RelationshipHelper.UpdateDivorceSource(family.Current, (Person)SpousesCombobox.SelectedItem, Source);
                        break;
                }
                family.OnContentChanged();
            }
        }

        private void ParentsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (family.Current == null)
                return;

            foreach (Relationship rel in family.Current.Relationships)
            {
                if (rel.RelationshipType == RelationshipType.Parent && rel.RelationTo.Equals((Person)ParentsCombobox.SelectedItem))
                {
                    ParentRelationship parentRel = ((ParentRelationship)rel);
                    ParentChildListbox.SelectedItem = parentRel.ParentChildModifier;
                }
            }

            family.OnContentChanged();
        }

        private void ParentChildListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Person p = (Person)ParentsCombobox.SelectedItem;
            Person c = family.Current;

            bool needsUpdate = new Boolean();

            foreach (Relationship rel in c.Relationships)
            {
                if (rel.RelationshipType == RelationshipType.Parent && rel.RelationTo.Equals(p))
                {
                    ParentRelationship parentRel = ((ParentRelationship)rel);
                    if (parentRel.ParentChildModifier.ToString() == ParentChildListbox.SelectedItem.ToString())
                        needsUpdate = false;
                    else
                        needsUpdate = true;
                }
            }

            switch ((ParentChildModifier)ParentChildListbox.SelectedItem)
            {
                case ParentChildModifier.Adopted:
                    ParentModifer.Content = Microsoft.FamilyShow.Properties.Resources.Adopted;
                    break;
                case ParentChildModifier.Foster:
                    ParentModifer.Content = Microsoft.FamilyShow.Properties.Resources.Fostered;
                    break;
                case ParentChildModifier.Natural:
                    ParentModifer.Content = Microsoft.FamilyShow.Properties.Resources.Natural;
                    break;
            }

            //only change a relationship descriptor when a user changes the radio button selection, not when they change the parent list combo box
            if (needsUpdate == true)
            {
                if (ParentChildListbox.SelectedItem != null)
                {
                    RelationshipHelper.UpdateParentChildStatus(family, (Person)ParentsCombobox.SelectedValue, family.Current, (ParentChildModifier)ParentChildListbox.SelectedItem);

                    if ((ParentChildModifier)ParentChildListbox.SelectedItem == ParentChildModifier.Adopted || (ParentChildModifier)ParentChildListbox.SelectedItem == ParentChildModifier.Foster)
                    {
                        RelationshipHelper.RemoveSiblingRelationships(family.Current, (Person)ParentsCombobox.SelectedValue);

                        family.Current.OnPropertyChanged("HasSiblings");
                        family.Current.OnPropertyChanged("Siblings");
                        family.Current.OnPropertyChanged("IsDeletable");

                    }
                    if ((ParentChildModifier)ParentChildListbox.SelectedItem == ParentChildModifier.Natural)
                    {
                        RelationshipHelper.UpdateSiblings(family, family.Current);
                        family.Current.OnPropertyChanged("HasSiblings");
                        family.Current.OnPropertyChanged("Siblings");
                        family.Current.OnPropertyChanged("IsDeletable");
                    }
                }
                family.OnContentChanged();
            }
        }

        private void RemoveParentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeleteRelationship,
                    Properties.Resources.Relationship, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {

                RelationshipHelper.RemoveParentChildRelationship(family.Current, (Person)ParentsCombobox.SelectedValue);
                RelationshipHelper.RemoveSiblingRelationships(family.Current, (Person)ParentsCombobox.SelectedValue);  //update to remove only siblings if present in the parent set containing selected parent

                if (family.Current.Parents.Count >= 2)  //There must be at least 2 parents for a parent set
                {
                    family.Current.OnPropertyChanged("PossibleParentSets");
                }

                family.Current.OnPropertyChanged("HasParents");
                family.Current.OnPropertyChanged("Parents");
                family.Current.OnPropertyChanged("HasSiblings");
                family.Current.OnPropertyChanged("Siblings");
                family.Current.OnPropertyChanged("IsDeletable");

                family.OnContentChanged();

            }

        }

        private void RemoveSiblingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeleteRelationship,
                    Properties.Resources.Relationship, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                RelationshipHelper.RemoveSiblingRelationshipsOneToOne(family.Current, (Person)SiblingsCombobox.SelectedValue);
                family.Current.OnPropertyChanged("HasSiblings");
                family.Current.OnPropertyChanged("Siblings");
                family.Current.OnPropertyChanged("IsDeletable");
                family.OnContentChanged();
            }

        }

        private void RemoveSpouseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeleteRelationship,
                    Properties.Resources.Relationship, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                RelationshipHelper.RemoveSpouseRelationship(family.Current, (Person)SpousesCombobox.SelectedValue);
                family.Current.OnPropertyChanged("HasSpouse");
                family.Current.OnPropertyChanged("Spouses");
                family.Current.OnPropertyChanged("IsDeletable");
                family.OnContentChanged();
            }

        }

        private void FromEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Update the spouse Marriage date info
            if (SpousesCombobox.SelectedItem != null)
            {
                #region Perform the businless logic for updating the marriage date

                DateTime marriageDate = App.StringToDate(FromEditTextBox.Text);

                if (marriageDate == DateTime.MinValue)
                    // Clear the marriage date
                    RelationshipHelper.UpdateMarriageDate(family.Current, (Person)SpousesCombobox.SelectedItem, null);
                else
                    RelationshipHelper.UpdateMarriageDate(family.Current, (Person)SpousesCombobox.SelectedItem, marriageDate);

                // Let the collection know that it has been updated
                family.OnContentChanged();

                #endregion
            }
        }

        private void ToEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Update the spouse Divorce date info
            if (SpousesCombobox.SelectedItem != null)
            {
                #region Perform the businless logic for updating the divorce date

                DateTime divorceDate = App.StringToDate(ToEditTextBox.Text);

                if (divorceDate == DateTime.MinValue)
                    // Clear the divorce date
                    RelationshipHelper.UpdateDivorceDate(family.Current, (Person)SpousesCombobox.SelectedItem, null);
                else
                    RelationshipHelper.UpdateDivorceDate(family.Current, (Person)SpousesCombobox.SelectedItem, divorceDate);

                // Let the collection know that it has been updated
                family.OnContentChanged();

                #endregion
            }
        }

        private void PlaceEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Update the spouse marriage info
            if (SpousesCombobox.SelectedItem != null)
            {
                #region Perform the businless logic for updating the marriage place

                string marriagePlace = PlaceEditTextBox.Text;
                RelationshipHelper.UpdateMarriagePlace(family.Current, (Person)SpousesCombobox.SelectedItem, marriagePlace);

                // Let the collection know that it has been updated
                family.OnContentChanged();

                #endregion
            }
        }

        private void ChangeMarriageDescriptorForward(object sender, RoutedEventArgs e)
        {
            string text = FromDescriptor.Content.ToString();
            text = forwardDateDescriptor(text);
            RelationshipHelper.UpdateMarriageDateDescriptor(family.Current, (Person)SpousesCombobox.SelectedItem, text);
            FromDescriptor.Content = text;

            // Let the collection know that it has been updated
            family.OnContentChanged();
        }

        private void ChangeMarriageDescriptorBackward(object sender, RoutedEventArgs e)
        {
            string text = FromDescriptor.Content.ToString();
            text = backwardDateDescriptor(text);
            RelationshipHelper.UpdateMarriageDateDescriptor(family.Current, (Person)SpousesCombobox.SelectedItem, text);
            FromDescriptor.Content = text;

            // Let the collection know that it has been updated
            family.OnContentChanged();
        }

        private void ChangeDivorceDescriptorForward(object sender, RoutedEventArgs e)
        {
            string text = ToDescriptor.Content.ToString();
            text = forwardDateDescriptor(text);
            RelationshipHelper.UpdateDivorceDateDescriptor(family.Current, (Person)SpousesCombobox.SelectedItem, text);
            ToDescriptor.Content = text;

            // Let the collection know that it has been updated
            family.OnContentChanged();
        }

        private void ChangeDivorceDescriptorBackward(object sender, RoutedEventArgs e)
        {
            string text = ToDescriptor.Content.ToString();
            text = backwardDateDescriptor(text);
            RelationshipHelper.UpdateDivorceDateDescriptor(family.Current, (Person)SpousesCombobox.SelectedItem, text);
            ToDescriptor.Content = text;

            // Let the collection know that it has been updated
            family.OnContentChanged();
        }

        /// <summary>
        /// Load a map of the marriage place
        /// </summary>
        private void SearchMapMarriagePlace(object sender, RoutedEventArgs e)
        {
            SearchMap(PlaceEditTextBox.Text.ToString());
        }

        #endregion

        #region other Details event handlers

        /// <summary>
        /// Change the focus to the next person in the people list
        /// </summary>
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            //find the index of the current person
            int i = family.IndexOf((Person)this.DataContext);
            //then get search for the person with the next index
            Person p = family.Next(i);

            //if not null, set to the next person
            if (p != null)
                family.Current = p;
            else  //otherwise the person is at the end of the list so go to the start of the list
                family.Current = family.Next(-1);

        }

        /// <summary>
        /// Change the focus to the previous person in the people list
        /// </summary>
        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            //find the index of the current person
            int i = family.IndexOf((Person)this.DataContext);
            //then get search for the person with the previous index
            Person p = family.Previous(i);

            if (p != null)  //if not null, set to the previous person
                family.Current = p;
            else  //otherwise the person is at the start of the list so go to the end of the list
                family.Current = family.Previous(family.Count);
        }

        /// <summary>
        /// Handle dropped files.
        /// </summary>
        private void AvatarPhoto_Drop(object sender, DragEventArgs e)
        {
            Person person = (Person)this.DataContext;

            if (person.Restriction != Restriction.Locked)
            {
                // Retrieve the dropped files
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                // Get the files that is supported and add them to the photos for the person
                foreach (string fileName in fileNames)
                {
                    // Handles photo files
                    if (App.IsPhotoFileSupported(fileName))
                    {
                        Photo photo = new Photo(fileName);

                        // Make the new photo the person's avatar if 
                        // 1. The user drops one photo.
                        // 2. The user does not have an existing avatar.
                        // Do not change an existing avatar and do not choose avatar when multiple photos are dropped. 
                        if (!person.HasAvatar && fileNames.Length==1)
                            photo.IsAvatar = true;

                        // Associate the photo with the person.
                        person.Photos.Add(photo);

                        // Setter for property change notification
                        person.Avatar = "";
                    }
                    else
                    {
                        //File not supported, warn user
                        MessageBox.Show(Properties.Resources.NotSupportedExtension1 + Path.GetExtension(fileName) + " " + Properties.Resources.NotSupportedExtension2 + " " + Properties.Resources.UnsupportedPhotoMessage, Properties.Resources.Unsupported, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                person.OnPropertyChanged("HasPhoto");
                family.OnContentChanged();
            }
            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(PersonInfoClickEvent));
        }

        private void FamiliyDataButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FamilyDataClickEvent));
        }

        /// <summary>
        /// Changes the primary person selection for diagram and details panel.
        /// </summary>
        private void FamilyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ignoreSelection)
            {
                ignoreSelection = true;
                Person selected = (Person)((ListBox)sender).SelectedItem;
                if (selected != null)
                {
                    family.Current = selected;
                    this.DataContext = family.Current;
                }

                ignoreSelection = false;
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FamilyListView.FilterList(FilterTextBox.Text);

            if (FamilyListView.Items.Count > 0)
                FamilyListView.ScrollIntoView(FamilyListView.Items[0]);
        }

        /// <summary>
        /// Filters the list of people in the ExistingPeopleListBox.  This filter is separated from the FamilyDisplayListView filter box.
        /// </summary>
        private void ExistingFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExistingFilter();
        }

        private void UpdateExistingFilter()
        {
            if (ExistingPeopleListBox.ItemsSource != null)
            {
                //Use collection view to filter the listbox
                ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(family);
                view.Filter = new Predicate<object>(ExistingPeopleFilter);
            }

        }

        /// <summary>
        /// Event handler when the selected node changes. Select the 
        /// current person in the list.
        /// </summary>
        void Family_CurrentChanged(object sender, EventArgs e)
        {
            if (!ignoreSelection)
            {
                ignoreSelection = true;
                //reset the FamilyListView
                FilterTextBox.Text = string.Empty;
                FamilyListView.SelectedItem = family.Current;
                FamilyListView.ScrollIntoView(family.Current);
                ignoreSelection = false;

                //Reset the existing people list 
                UpdateExistingFilter();

                //update the Citations screen when the person is changed
                UpdateCitationsCombobox();

                //update the RCitations screen when the person is changed
                UpdateRCitationsComboBox();
            }
        }

        /// <summary>
        /// Event handler for textbox GotFocus.  Select all of the textbox contents for quick entry.
        /// </summary>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// Event handler for deleting people.  Note that not all people can be deleted.  See "IsDeletable" property on the person class.
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeletePerson,
                Properties.Resources.Person, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {


                //Helper method to select a sensible next person other than family[0]

                Person nextPerson = new Person();

                if (family.Count > 0)
                {


                    if (family.Current.HasSpouse)
                        nextPerson = family.Current.Spouses[0];
                    else if (family.Current.HasSiblings)
                        nextPerson = family.Current.Siblings[0];
                    else if (family.Current.HasParents)
                        nextPerson = family.Current.Parents[0];
                    else
                        nextPerson = null;

                }
                else
                    nextPerson = null;
       

                // Deleting a person requires deleting that person from their relations with other people
                // Call the relationship helper to handle delete.
                RelationshipHelper.DeletePerson(family, family.Current);

                if (family.Count > 0)
                {
                    // Current person is deleted, choose someone else as the current person

                    if (nextPerson != null)
                        family.Current = nextPerson;
                    else
                        family.Current = family[0];

                    family.OnContentChanged();
                    SetDefaultFocus();
                }
                else
                {
                    // Let the container window know that everyone has been deleted
                    RaiseEvent(new RoutedEventArgs(EveryoneDeletedEvent));
                }
            }

        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
                SetFamilyMemberAddButton();
        }

        #endregion

        #region Storyboard completed event handlers

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsAdd_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            NamesInputTextBox.Focus();
        }

        /// <summary>
        /// Make it easy to add new people to the tree by pressing return
        /// </summary>
        private void CollapseDetailsAdd_StoryboardCompleted(object sender, EventArgs e)
        {
            enableButtons();
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsEdit_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            NamesEditTextBox.Focus();
        }

        /// <summary>
        /// Make it easy to add new people to the tree by pressing return
        /// </summary>
        private void CollapseDetailsEdit_StoryboardCompleted(object sender, EventArgs e)
        {
            enableButtons();
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsEditMore_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            BurialDateEditTextBox.Focus();
        }

        private void CollapseDetailsEditMore_StoryboardCompleted(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsEditCitations_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            UpdateCitationsCombobox();
        }

        /// <summary>
        /// Make it easy to add new people to the tree by pressing return
        /// </summary>
        private void CollapseDetailsEditCitations_StoryboardCompleted(object sender, EventArgs e)
        {
            enableButtons();
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsEditAttachments_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
        }

        /// <summary>
        /// Make it easy to add new people to the tree by pressing return
        /// </summary>
        private void CollapseDetailsEditAttachments_StoryboardCompleted(object sender, EventArgs e)
        {
            enableButtons();
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsEditRelationship_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            DoneEditRelationshipButton.Focus();
        }

        /// <summary>
        /// Make it easy to add new people to the tree by pressing return
        /// </summary>
        private void CollapseDetailsEditRelationship_StoryboardCompleted(object sender, EventArgs e)
        {
            enableButtons();
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// The focus can be set only after the animation has stopped playing.
        /// </summary>
        private void ExpandDetailsAddIntermediate_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            // Select the first parent set for quick entry.
            ParentsListBox.SelectedIndex = 0;
            IntermediateAddButton.Focus();
        }

        /// <summary>
        /// Make it easy to add new people to the tree by pressing return
        /// </summary>
        private void CollapseDetailsAddIntermediate_StoryboardCompleted(object sender, EventArgs e)
        {
            enableButtons();
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// Handle filter updates.
        /// </summary>
        private void ExpandAddExisting_StoryboardCompleted(object sender, EventArgs e)
        {
            disableButtons();
            AddExistingButton.Focus();

            ExistingFilter = true;  //start filtering on gender and relatives

            //set default add action as spouse as this is most common
            ExistingFamilyMemberComboBox.SelectedItem = ExistingFamilyMemberComboBoxValue.Spouse;

        }

        /// <summary>
        /// Handle filter updates
        /// </summary>
        private void CollapseAddExisting_StoryboardCompleted(object sender, EventArgs e)
        {

            ExistingFilter = false;  //stop filtering on gender and relatives
            ResetFilter = true;      //allow quick filter reset

            ExistingFamilyMemberComboBox.SelectedIndex = -1;  //set Existing Family Member Combo Box to -1 so it updates when the Existing people panel loads
            FilterTextBox.Text = String.Empty;              //reset all filter textboxes 
            ExistingFilterTextBox.Text = String.Empty;
            UpdateExistingFilter();                         //quick filter update
            ResetFilter = false;

            enableButtons();
            FamilyMemberAddButton.Focus();

            family.OnContentChanged();
        }

        private void enableButtons()
        {
            this.FamilyListView.IsEnabled = true;
            this.FilterTextBox.IsEnabled = true;
            this.DetailsList.Visibility = Visibility.Visible;
        }

        private void disableButtons()
        {
            this.FamilyListView.IsEnabled = false;
            this.FilterTextBox.IsEnabled = false;
            this.DetailsList.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #region helper methods

        /// <summary>
        /// Search for a map of a location string using Bing maps
        /// </summary>
        private static void SearchMap(string s)
        {
            try
            {
                if (!string.IsNullOrEmpty(s))
                {
                    System.Diagnostics.Process.Start("http://www.bing.com/maps/default.aspx?&v=&where1=" + s);
                }
                else
                    System.Diagnostics.Process.Start("http://www.bing.com/maps/");

            }
            catch { }
        }

        /// <summary>
        /// Change a date descriptor forward
        /// </summary>
        private static string forwardDateDescriptor(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "ABT ";
            if (s == "ABT ")
                return "AFT ";
            if (s == "AFT ")
                return "BEF ";
            if (s == "BEF ")
                return "";
            if (s == "")
                return "ABT ";
            else
                return "";
        }

        /// <summary>
        /// Change a date descriptor backward
        /// </summary>
        private static string backwardDateDescriptor(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "BEF ";
            if (s == "BEF ")
                return "AFT ";
            if (s == "AFT ")
                return "ABT ";
            if (s == "ABT ")
                return "";
            if (s == "")
                return "BEF ";
            else
                return "";
        }

        /// <summary>
        /// Clear the input fields
        /// </summary>
        private void ClearDetailsAddFields()
        {
            NamesInputTextBox.Clear();
            SurnameInputTextBox.Clear();
            SuffixInputTextBox.Clear();
            BirthDateInputTextBox.Clear();
            BirthPlaceInputTextBox.Clear();
        }

        public void SetDefaultFocus()
        {
            FamilyMemberAddButton.Focus();
        }

        /// <summary>
        /// Display the Details Add Intermediate section
        /// </summary>
        private void ShowDetailsAddIntermediate(ParentSetCollection possibleParents)
        {
            // Display the Details Add Intermediate section
            ((Storyboard)this.Resources["ExpandDetailsAddIntermediate"]).Begin(this);

            // Bind the possible parents
            ParentsListBox.ItemsSource = possibleParents;
        }

        /// <summary>
        /// Sets the next action for the Add Family Member Button
        /// </summary>
        private void SetNextFamilyMemberAction(FamilyMemberComboBoxValue value)
        {
            FamilyMemberAddButton.CommandParameter = value;

            string Relationship = string.Empty;

            switch (value.ToString())
            {
                case "Father":
                    Relationship = Properties.Resources.Father;
                    break;
                case "Mother":
                    Relationship = Properties.Resources.Mother;
                    break;
                case "Brother":
                    Relationship = Properties.Resources.Brother;
                    break;
                case "Sister":
                    Relationship = Properties.Resources.Sister;
                    break;
                case "Son":
                    Relationship = Properties.Resources.Son;
                    break;
                case "Daughter":
                    Relationship = Properties.Resources.Daughter;
                    break;
                case "Spouse":
                    Relationship = Properties.Resources.Spouse;
                    break;


            }

            FamilyMemberAddButton.Content = Properties.Resources.Add + " " + Relationship;
        }

        /// <summary>
        /// Logic for setting the default command for the family member to add
        /// </summary>
        private void SetFamilyMemberAddButton()
        {
            if (family.Current.Parents.Count == 2)
                // Person has parents, choice another default.
                SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Brother);
            else
                // Default for everything else
                SetNextFamilyMemberAction(FamilyMemberComboBoxValue.Father);
        }

        /// <summary>
        /// A filter which returns based on a person's relationships, gender or name.
        /// Flags can used to skip filtering on relationships/gender or name or both.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ExistingPeopleFilter(object item)
        {
            Person person = item as Person;

            if (ExistingFilter == true)
            {

                //filter the current person
                if (family.Current == person)
                    return false;
                if (family.Current.Parents.Contains(person)) //filter the current person's parents
                    return false;
                if (family.Current.Spouses.Contains(person)) //filter the current person's spouses
                    return false;
                if (family.Current.PreviousSpouses.Contains(person)) //filter the current persons previous spouses
                    return false;
                if (family.Current.Siblings.Contains(person))  //filter the current persons siblings
                    return false;
                if (family.Current.Children.Contains(person)) //filter the current persons children
                    return false;

                if (ignoreGender == false)
                {
                    if (!string.IsNullOrEmpty(ExistingFilterTextBox.Text))
                    {
                        if (person.Gender == genderFilter && person.FullName.ToLower().Contains(ExistingFilterTextBox.Text.ToLower()))
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (person.Gender == genderFilter)
                            return true;
                        else
                            return false;
                    }
                }
                else
                    return (person.FullName.ToLower().Contains(ExistingFilterTextBox.Text.ToLower()));

            }
            else if (ResetFilter == true)  //quick reset
                return true;
            else
                return (person.FullName.ToLower().Contains(ExistingFilterTextBox.Text.ToLower()));
        }

        #endregion

        #region source tooltip methods

        /// <summary>
        /// Updates a text box with a tool tip containing the citation information
        /// </summary>
        /// <param name="box"></param>
        /// <param name="sourceId"></param>
        /// <param name="citation"></param>
        private void UpdateToolTip(TextBox box, string sourceId, string citation)
        {
            if (string.IsNullOrEmpty(box.Text))
                box.ToolTip = Microsoft.FamilyShow.Properties.Resources.Unknown;  //Event is not known
            else
            {
                if (this.sources.Find(sourceId) != null)  //Event has citation
                {
                    box.ToolTip = this.sources.Find(sourceId).SourceNameAndId;  //Use friendly name

                    if (!string.IsNullOrEmpty(citation))
                        box.ToolTip += "\n" + citation;  //Only add the details if there is a valid source iD
                }
                else
                    box.ToolTip = Microsoft.FamilyShow.Properties.Resources.Uncited;  //Event has no citation

            }
        }

        /// <summary>
        /// Updates all the citation tooltips in the details edit panel and details edit more panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolTip_All(object sender, ToolTipEventArgs e)
        {

            UpdateToolTip(BirthDateEditTextBox, family.Current.BirthSource, family.Current.BirthCitation);
            UpdateToolTip(BirthPlaceEditTextBox, family.Current.BirthSource, family.Current.BirthCitation);

            UpdateToolTip(DeathDateEditTextBox, family.Current.DeathSource, family.Current.DeathCitation);
            UpdateToolTip(DeathPlaceEditTextBox, family.Current.DeathSource, family.Current.DeathCitation);

            UpdateToolTip(CremationDateEditTextBox, family.Current.CremationSource, family.Current.CremationCitation);
            UpdateToolTip(CremationPlaceEditTextBox, family.Current.CremationSource, family.Current.CremationCitation);

            UpdateToolTip(BurialDateEditTextBox, family.Current.BurialSource, family.Current.BurialCitation);
            UpdateToolTip(BurialPlaceEditTextBox, family.Current.BurialSource, family.Current.BurialCitation);

            UpdateToolTip(EducationEditTextBox, family.Current.EducationSource, family.Current.EducationCitation);
            UpdateToolTip(OccupationEditTextBox, family.Current.EducationSource, family.Current.EducationCitation);
            UpdateToolTip(ReligionEditTextBox, family.Current.EducationSource, family.Current.EducationCitation);
 
        }

        #endregion

        /// <summary>
        /// Changes the color of a clickable label to indicate that something will happen if it is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            Label s = (Label)sender;
            s.Foreground = System.Windows.Media.Brushes.LightSteelBlue;   
        }

        /// <summary>
        /// Restores the color of a clickable label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            Label s = (Label)sender;
            s.Foreground = System.Windows.Media.Brushes.White;
        }

    }

    /// <summary>
    /// Enum Values for the Family Member ComboBox.
    /// </summary>
    public enum FamilyMemberComboBoxValue
    {
        Father, Mother, Brother, Sister, Spouse, Son, Daughter, Unrelated, Existing
    }

    /// <summary>
    /// Enum Values for the Existing Family Member ComboBox.
    /// </summary>
    public enum ExistingFamilyMemberComboBoxValue
    {
        Father, Mother, Brother, Sister, Spouse, Son, Daughter
    }

    /// <summary>
    /// Enum Values for the Citations ComboBox.
    /// </summary>
    public enum CitationsComboBoxValue
    {
        Birth, Death, Occupation, Education, Religion, Burial, Cremation
    }

    /// <summary>
    /// Enum Values for the Relationship Citations ComboBox.
    /// </summary>
    public enum RCitationsComboBoxValue
    {
        Marriage, Divorce
    }

}