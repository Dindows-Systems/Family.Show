using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.FamilyShowLib;
using Microsoft.FamilyShow;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Syncing.xaml
    /// </summary>
    public partial class Merge : System.Windows.Controls.UserControl
    {
        int i = 0;      // A counter for all duplicate people
        int count = 0;  // Index of current person with differing info
        int total = 0;  // Total number of people with differing info
        public string[,] summary;

        public Merge()
        {            
            InitializeComponent();
        }

        #region routed events

        public static readonly RoutedEvent DoneButtonClickEvent = EventManager.RegisterRoutedEvent(
            "DoneButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Merge));
     
        public event RoutedEventHandler DoneButtonClick
        {
            add { AddHandler(DoneButtonClickEvent, value); }
            remove { RemoveHandler(DoneButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            SavePerson();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            NextPerson();  
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.FamilyCollection.NewPeopleCollection != null)
            {
                if (SummaryStar.IsChecked == true)
                {
                    foreach (Person person in App.Family)
                    {
                        foreach (Person newPerson in App.FamilyCollection.NewPeopleCollection)
                        {
                            if (newPerson.Id == person.Id)
                            {
                                person.FirstName = "*" + person.FirstName;
                                break;
                            }

                        }
                    }
                }
            }

            App.FamilyCollection.ExistingPeopleCollection.Clear();
            App.FamilyCollection.DuplicatePeopleCollection.Clear();
            App.FamilyCollection.NewPeopleCollection.Clear();

            SummaryStar.IsChecked = true;
            TaskBar.Current.Restore();

            RaiseEvent(new RoutedEventArgs(DoneButtonClickEvent));
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            MergeDuplicateSummary.Visibility = Visibility.Collapsed;
            MergeDuplicates.Visibility = Visibility.Visible;
            NextPerson();
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Gets and sets the number of people with differing info.
        /// </summary>
        private int CountHighlights()
        {
                bool anyHighlighted = false;

                total = 0;
                i = 0;

                do
                {
                    anyHighlighted = CurrentPerson(i);
                    i++;
                    if (anyHighlighted)
                        total++;
                }
                while (i < App.FamilyCollection.ExistingPeopleCollection.Count);

                return total;

        }

        /// <summary>
        /// Find and display the next person with differing info.
        /// </summary>
        private void NextPerson()
        {
            bool anyHighlighted = false;

            do
            {
                i++;

                if (i < App.FamilyCollection.ExistingPeopleCollection.Count)
                {
                    anyHighlighted = CurrentPerson(i);

                    if (anyHighlighted)
                        count++;

                    if (total != 0)
                    {
                        FileProgressBar.Value = (count * 100) / total;
                        FileProgressText.Text = count +"/" + total;
                        //If Windows 7 we can update progress on the status bar
                        TaskBar.Current.Progress((int)FileProgressBar.Value);

                    }
                }
            }
            while (!anyHighlighted && i < App.FamilyCollection.ExistingPeopleCollection.Count);  //find next differeing record

            if(i >= App.FamilyCollection.ExistingPeopleCollection.Count)
                ShowSummary();
        }

        /// <summary>
        /// Updates an existing person based on the checkboxes set to update.
        /// When event info is updated, the citation info for that event is also updated.
        /// </summary>
        private void SavePerson()
        {

            Person p1 = App.FamilyCollection.ExistingPeopleCollection[i];
            Person p2 = App.FamilyCollection.DuplicatePeopleCollection[i];

            if (p1 != null && p2 != null)
            {
                Person p3 = App.Family.Find(p1.Id);

                if (NameCheck.IsChecked == true)
                    p3.FirstName = p2.FirstName;

                if (LastNameCheck.IsChecked == true)
                    p3.LastName = p2.LastName;

                if (SuffixCheck.IsChecked == true)
                    p3.Suffix = p2.Suffix;

                if (BirthPlaceCheck.IsChecked == true)
                {
                    p3.BirthPlace = p2.BirthPlace;

                        p3.BirthSource = p2.BirthSource;
                        p3.BirthCitation = p2.BirthCitation;
                        p3.BirthCitationActualText = p2.BirthCitationActualText;
                        p3.BirthCitationNote = p2.BirthCitationNote;
                        p3.BirthLink = p2.BirthLink;
                    
                }

                if (BirthDateCheck.IsChecked == true)
                {
                    p3.BirthDate = p2.BirthDate;
                    p3.BirthDateDescriptor = p2.BirthDateDescriptor;

                        p3.BirthSource = p2.BirthSource;
                        p3.BirthCitation = p2.BirthCitation;
                        p3.BirthCitationActualText = p2.BirthCitationActualText;
                        p3.BirthCitationNote = p2.BirthCitationNote;
                        p3.BirthLink = p2.BirthLink;
    
                }

                if (DeathDateCheck.IsChecked == true)
                {
                    p3.DeathDate = p2.DeathDate;
                    p3.DeathDateDescriptor = p3.DeathDateDescriptor;

                        p3.DeathSource = p2.DeathSource;
                        p3.DeathCitation = p2.DeathCitation;
                        p3.DeathCitationActualText = p2.DeathCitationActualText;
                        p3.DeathCitationNote = p2.DeathCitationNote;
                        p3.DeathLink = p2.DeathLink;
                }

                if (DeathPlaceCheck.IsChecked == true)
                {
                    p3.DeathPlace = p2.DeathPlace;

                        p3.DeathSource = p2.DeathSource;
                        p3.DeathCitation = p2.DeathCitation;
                        p3.DeathCitationActualText = p2.DeathCitationActualText;
                        p3.DeathCitationNote = p2.DeathCitationNote;
                        p3.DeathLink = p2.DeathLink;
                    
                }

                if (OccupationCheck.IsChecked == true)
                {
                    p3.Occupation = p2.Occupation;
                   
                        p3.OccupationSource = p2.OccupationSource;
                        p3.OccupationCitation = p2.OccupationCitation;
                        p3.OccupationCitationActualText = p2.OccupationCitationActualText;
                        p3.OccupationCitationNote = p2.OccupationCitationNote;
                        p3.OccupationLink = p2.OccupationLink;
                    
                }

                if (EducationCheck.IsChecked == true)
                {
                    p3.Education = p2.Education;
                   
                        p3.EducationSource = p2.EducationSource;
                        p3.EducationCitation = p2.EducationCitation;
                        p3.EducationCitationActualText = p2.EducationCitationActualText;
                        p3.EducationCitationNote = p2.EducationCitationNote;
                        p3.EducationLink = p2.EducationLink;
                    
                }

                if (ReligionCheck.IsChecked == true)
                {
                    p3.Religion = p2.Religion;
                    
                        p3.ReligionSource = p2.ReligionSource;
                        p3.ReligionCitation = p2.ReligionCitation;
                        p3.ReligionCitationActualText = p2.ReligionCitationActualText;
                        p3.ReligionCitationNote = p2.ReligionCitationNote;
                        p3.ReligionLink = p2.ReligionLink;
                    
                }

                if (CremationPlaceCheck.IsChecked == true)
                {
                    p3.CremationPlace = p2.CremationPlace;
                    
                        p3.CremationSource = p2.CremationSource;
                        p3.CremationCitation = p2.CremationCitation;
                        p3.CremationCitationActualText = p2.CremationCitationActualText;
                        p3.CremationCitationNote = p2.CremationCitationNote;
                        p3.CremationLink = p2.CremationLink;
                    
                }

                if (CremationDateCheck.IsChecked == true)
                {
                    p3.CremationDate = p2.CremationDate;
                    p3.CremationDateDescriptor = p2.CremationDateDescriptor;

                        p3.CremationSource = p2.CremationSource;
                        p3.CremationCitation = p2.CremationCitation;
                        p3.CremationCitationActualText = p2.CremationCitationActualText;
                        p3.CremationCitationNote = p2.CremationCitationNote;
                        p3.CremationLink = p2.CremationLink;
                    
                }

                if (BurialDateCheck.IsChecked == true)
                {
                    p3.BurialDate = p2.BurialDate;
                    p3.BurialDateDescriptor = p2.BurialDateDescriptor;
                    
                        p3.BurialSource = p2.BurialSource;
                        p3.BurialCitation = p2.BurialCitation;
                        p3.BurialCitationActualText = p2.BurialCitationActualText;
                        p3.BurialCitationNote = p2.BurialCitationNote;
                        p3.BurialLink = p2.BurialLink;
                    
                }

                if (BurialPlaceCheck.IsChecked == true)
                {
                    p3.BurialPlace = p2.BurialPlace;
                    
                        p3.BurialSource = p2.BurialSource;
                        p3.BurialCitation = p2.BurialCitation;
                        p3.BurialCitationActualText = p2.BurialCitationActualText;
                        p3.BurialCitationNote = p2.BurialCitationNote;
                        p3.BurialLink = p2.BurialLink;                   
                }

                if (NoteCheck.IsChecked == true)
                {
                    p3.Story = new Story();
                    string storyFileName = new StringBuilder(App.ReplaceEncodedCharacters(p3.FullName + "(" + p3.Id + ")")).Append(".rtf").ToString();
                    p3.Story.Save(p2.Note, storyFileName);           
                    p3.Note = p2.Note;
                }
            }
          
            NextPerson();
        }

        /// <summary>
        /// Looks up the i th person in the duplicate people list and
        /// the corresponding person in the existing people list.
        /// Returns true if there are information differences.
        /// Updates an event citation when the both duplicate and existing 
        /// person event info is the same but only the new person has a citation.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool CurrentPerson(int i)
        {
                int highlightCount = 0;

                #region load the comparison

                Person existingPerson = App.FamilyCollection.ExistingPeopleCollection[i];
                Person newPerson = App.FamilyCollection.DuplicatePeopleCollection[i];

                OldName.Content = existingPerson.FirstName;
                NewName.Content = newPerson.FirstName;

                OldLastName.Content = existingPerson.LastName;
                NewLastName.Content = newPerson.LastName;

                OldSuffix.Content = existingPerson.Suffix;
                NewSuffix.Content = newPerson.Suffix;

                OldBirthPlace.Content = existingPerson.BirthPlace;
                NewBirthPlace.Content = newPerson.BirthPlace;

                OldBirthDate.Content = existingPerson.BirthDateDescriptor + DateToString(existingPerson.BirthDate);
                NewBirthDate.Content = newPerson.BirthDateDescriptor + DateToString(newPerson.BirthDate);

                OldDeathPlace.Content = existingPerson.DeathPlace;
                NewDeathPlace.Content = newPerson.DeathPlace;

                OldDeathDate.Content = existingPerson.DeathDateDescriptor + DateToString(existingPerson.DeathDate);
                NewDeathDate.Content = newPerson.DeathDateDescriptor + DateToString(newPerson.DeathDate);

                OldEducation.Content = existingPerson.Education;
                NewEducation.Content = newPerson.Education;

                OldOccupation.Content = existingPerson.Occupation;
                NewOccupation.Content = newPerson.Occupation;

                OldReligion.Content = existingPerson.Religion;
                NewReligion.Content = newPerson.Religion;

                OldBurialPlace.Content = existingPerson.BurialPlace;
                NewBurialPlace.Content = newPerson.BurialPlace;

                OldBurialDate.Content = existingPerson.BurialDateDescriptor + DateToString(existingPerson.BurialDate);
                NewBurialDate.Content = newPerson.BurialDateDescriptor + DateToString(newPerson.BurialDate);

                OldCremationDate.Content = existingPerson.CremationDateDescriptor + DateToString(existingPerson.CremationDate);
                NewCremationDate.Content = newPerson.CremationDateDescriptor + DateToString(newPerson.CremationDate);

                OldCremationPlace.Content = existingPerson.CremationPlace;
                NewCremationPlace.Content = newPerson.CremationPlace;

                 //Tooltips
                string existingBirthCitation =   citationString(existingPerson.BirthSource,existingPerson.BirthCitation,existingPerson.BirthCitationActualText,existingPerson.BirthCitationNote,existingPerson.BirthLink);
                string existingDeathCitation =   citationString(existingPerson.DeathSource,existingPerson.DeathCitation,existingPerson.DeathCitationActualText,existingPerson.DeathCitationNote,existingPerson.DeathLink);
                string existingCremationCitation =   citationString(existingPerson.CremationSource,existingPerson.CremationCitation,existingPerson.CremationCitationActualText,existingPerson.CremationCitationNote,existingPerson.CremationLink);
                string existingBurialCitation =   citationString(existingPerson.BurialSource,existingPerson.BurialCitation,existingPerson.BurialCitationActualText,existingPerson.BurialCitationNote,existingPerson.BurialLink);
                string existingEducationCitation =   citationString(existingPerson.EducationSource,existingPerson.EducationCitation,existingPerson.EducationCitationActualText,existingPerson.EducationCitationNote,existingPerson.EducationLink);
                string existingOccupationCitation =   citationString(existingPerson.OccupationSource,existingPerson.OccupationCitation,existingPerson.OccupationCitationActualText,existingPerson.OccupationCitationNote,existingPerson.OccupationLink);
                string existingReligionCitation =   citationString(existingPerson.ReligionSource,existingPerson.ReligionCitation,existingPerson.ReligionCitationActualText,existingPerson.ReligionCitationNote,existingPerson.ReligionLink);

                string newBirthCitation =   citationString(newPerson.BirthSource,newPerson.BirthCitation,newPerson.BirthCitationActualText,newPerson.BirthCitationNote,newPerson.BirthLink);
                string newDeathCitation =   citationString(newPerson.DeathSource,newPerson.DeathCitation,newPerson.DeathCitationActualText,newPerson.DeathCitationNote,newPerson.DeathLink);
                string newCremationCitation =   citationString(newPerson.CremationSource,newPerson.CremationCitation,newPerson.CremationCitationActualText,newPerson.CremationCitationNote,newPerson.CremationLink);
                string newBurialCitation =   citationString(newPerson.BurialSource,newPerson.BurialCitation,newPerson.BurialCitationActualText,newPerson.BurialCitationNote,newPerson.BurialLink);
                string newEducationCitation =   citationString(newPerson.EducationSource,newPerson.EducationCitation,newPerson.EducationCitationActualText,newPerson.EducationCitationNote,newPerson.EducationLink);
                string newOccupationCitation =   citationString(newPerson.OccupationSource,newPerson.OccupationCitation,newPerson.OccupationCitationActualText,newPerson.OccupationCitationNote,newPerson.OccupationLink);
                string newReligionCitation =   citationString(newPerson.ReligionSource,newPerson.ReligionCitation,newPerson.ReligionCitationActualText,newPerson.ReligionCitationNote,newPerson.ReligionLink);

                BirthImage.ToolTip = Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingBirthCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newBirthCitation;
                DeathImage.ToolTip = Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingDeathCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newDeathCitation;
                CremationImage.ToolTip = Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingCremationCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newCremationCitation;
                BurialImage.ToolTip =  Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingBurialCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newBurialCitation;

                OccupationImage.ToolTip = Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingOccupationCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newOccupationCitation;
                EducationImage.ToolTip = Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingEducationCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newEducationCitation;
                ReligionImage.ToolTip = Microsoft.FamilyShow.Properties.Resources.ExistingCitation + " \n" + existingReligionCitation + "\n" + Microsoft.FamilyShow.Properties.Resources.NewCitation + " \n" + newReligionCitation;

                //Handle notes

                OldNote.Text = string.Empty;
                NewNote.Text = string.Empty;

                NoteCheck.IsChecked = false;

                OldNote.Background = Brushes.Transparent;
                NewNote.Background = Brushes.Transparent;

                OldNoteScroll.Background = Brushes.Transparent;
                NewNoteScroll.Background = Brushes.Transparent;

                //If existing person has no note, but the new person does, check update box to help user.
                if (newPerson.HasNote && !existingPerson.HasNote)
                {
                    NoteCheck.IsChecked = true;
                    OldNote.Background = Brushes.Orange;
                    NewNote.Background = Brushes.Orange;
                    OldNoteScroll.Background = Brushes.Orange;
                    NewNoteScroll.Background = Brushes.Orange;
                }

                if (existingPerson.Note != null && newPerson.Note != null)
                {

                    //Compare notes but remove spaces and returns.
                    //Often a note is identical but has some 
                    //trailing carriage returns or white 
                    //space which tricks the comparison.

                    string oldNote = existingPerson.Note.Replace(" ", "");
                    oldNote = oldNote.Replace("\n", "");
                    oldNote = oldNote.Replace("\r", "");

                    string newNote = newPerson.Note.Replace(" ", "");
                    newNote = newNote.Replace("\n", "");
                    newNote = newNote.Replace("\r", "");

                    if (oldNote != newNote)
                    {
                        highlightCount++;
                        OldNote.Background = Brushes.Orange;
                        NewNote.Background = Brushes.Orange;
                        OldNoteScroll.Background = Brushes.Orange;
                        NewNoteScroll.Background = Brushes.Orange;

                    }
                }


                if (existingPerson.HasNote)
                    OldNote.Text = existingPerson.Note;
                if (newPerson.HasNote)
                    NewNote.Text = newPerson.Note;

                // Add new photos but keep existing primary image when present
                foreach (Photo p in newPerson.Photos)
                {
                    bool add = true;

                    foreach (Photo existingPhoto in existingPerson.Photos)
                    {

                        if (existingPerson.HasAvatar)
                            p.IsAvatar = false;

                        if (p.RelativePath == existingPhoto.RelativePath)
                            add = false;
                    }

                    if(add)
                    existingPerson.Photos.Add(p);
                    existingPerson.OnPropertyChanged("Photos");
                }

                // Add new attachments
                foreach (Attachment a in newPerson.Attachments)
                {
                    bool add = true;

                    foreach (Attachment existingAttachment in existingPerson.Attachments)
                    {

                        if (a.RelativePath != existingAttachment.RelativePath)
                            add = false;
                    }

                    if(add)
                    existingPerson.Attachments.Add(a);
                    existingPerson.OnPropertyChanged("Attachments");
                }

                #endregion

                #region highlight conflicting information

                //check for conflicting info
                if (Highlight(NameCheck, OldName, NewName))
                    highlightCount++;
                if (Highlight(LastNameCheck, OldLastName, NewLastName))
                    highlightCount++;
                if (Highlight(SuffixCheck, OldSuffix, NewSuffix))
                    highlightCount++;
                if (Highlight(BirthDateCheck, OldBirthDate, NewBirthDate))
                    highlightCount++;
                if (Highlight(BirthPlaceCheck, OldBirthPlace, NewBirthPlace))
                    highlightCount++;
                if (Highlight(DeathDateCheck, OldDeathDate, NewDeathDate))
                    highlightCount++;
                if (Highlight(DeathPlaceCheck, OldDeathPlace, NewDeathPlace))
                    highlightCount++;
                if (Highlight(OccupationCheck, OldOccupation, NewOccupation))
                    highlightCount++;
                if (Highlight(EducationCheck, OldEducation, NewEducation))
                    highlightCount++;
                if (Highlight(ReligionCheck, OldReligion, NewReligion))
                    highlightCount++;
                if (Highlight(CremationPlaceCheck, OldCremationPlace, NewCremationPlace))
                    highlightCount++;
                if (Highlight(CremationDateCheck, OldCremationDate, NewCremationDate))
                    highlightCount++;
                if (Highlight(BurialDateCheck, OldBurialDate, NewBurialDate))
                    highlightCount++;
                if (Highlight(BurialPlaceCheck, OldBurialPlace, NewBurialPlace))
                    highlightCount++;

                if(highlightCitation(existingBirthCitation,newBirthCitation,BirthImageB))
                    highlightCount++;
                if(highlightCitation(existingDeathCitation,newDeathCitation,DeathImageB))
                    highlightCount++;
                if(highlightCitation(existingCremationCitation,newCremationCitation,CremationImageB))
                    highlightCount++;
                if(highlightCitation(existingBurialCitation,newBurialCitation,BurialImageB))
                    highlightCount++;
                if(highlightCitation(existingEducationCitation,newEducationCitation,EducationImageB))
                    highlightCount++;
                if(highlightCitation(existingReligionCitation,newReligionCitation,ReligionImageB))
                    highlightCount++;
                if(highlightCitation(existingOccupationCitation,newOccupationCitation,OccupationImageB))
                    highlightCount++;

                #endregion

                //Allow for skip over non highlighted people
                if (highlightCount > 0)
                    return true;
                else
                    return false;
        }

        private static string citationString(string source, string details, string actualText, string note, string link)
        {

            if(!string.IsNullOrEmpty(source))
                source = App.FamilyCollection.SourceCollection.Find(source).SourceNameAndId;
            
            return  source + "\n" +
                    details + "\n" +
                    actualText + "\n" +
                    note + "\n" +
                    link;
        }

        /// <summary>
        /// Determins whether the content of 2 citations are different and highlights
        /// if a difference is found.
        /// </summary>
        /// <param name="existingCitation"></param>
        /// <param name="newCitation"></param>
        /// <param name="imageBorder"></param>
        /// <returns></returns>
        private static bool highlightCitation(string existingCitation, string newCitation, Border imageBorder)
        {
            //If citations are different, highlight
            if (existingCitation.Trim() != newCitation.Trim())
            {
                imageBorder.Background = Brushes.Orange;
                return true;
            }
            else
            {
                imageBorder.Background = Brushes.Transparent;
                return false;
            }
        }

        /// <summary>
        /// Determins whether the content of 2 labels are different and highlights
        /// if a difference is found.  If information is present in the New Person 
        /// but is not present in the Existing Person, the Update? checkbox is 
        /// automatically checked.  Returns bool indicating highlighted status.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="existingInfo"></param>
        /// <param name="newInfo"></param>
        /// <returns></returns>
        private static bool Highlight(CheckBox c, Label existingInfo, Label newInfo)
        {

            bool highlight = false;
            bool check = false;

            string existingInfoText = string.Empty;
            string newInfoText = string.Empty;

            if (existingInfo.Content != null)
                existingInfoText = existingInfo.Content.ToString();
            if (newInfo.Content != null)
               newInfoText = newInfo.Content.ToString();

            //Flag to highlight upon differences
            if (existingInfoText != newInfoText)
                highlight = true;

            // If information is present in the New Person but is not present in
            // the Existing Person help user by checking the Update? checkbox
            if (string.IsNullOrEmpty(existingInfoText) && !string.IsNullOrEmpty(newInfoText))
                check = true;

            c.IsChecked = check;

            //Highlight the differences
            if (highlight)
            { 
                existingInfo.Background = Brushes.Orange;
                newInfo.Background = Brushes.Orange;
            }
            else
            {
                existingInfo.Background = Brushes.Transparent;
                newInfo.Background = Brushes.Transparent;
            }

            return highlight;
        }

        /// <summary>
        /// Handles showing the summary of the completed merge.
        /// </summary>
        public void ShowSummary()
        {
            i = 0;
            count = 0;
            MergeDuplicateSummary.Visibility = Visibility.Collapsed;
            MergeDuplicates.Visibility = Visibility.Collapsed;
            Summary.Visibility = Visibility.Visible;
            SummaryText.Content = summary[0, 0] + summary[1, 0] + summary[2, 0];
            TaskBar.Current.Progress(100);
        }

        /// <summary>
        /// Handles showing the summary of people to duplicates to merge
        /// </summary>
        public void ShowMergeSummary()
        {
            TaskBar.Current.Progress(2);

            if (CountHighlights() > 0)
            {
                MergeDuplicateSummary.Visibility = Visibility.Visible;
                Summary.Visibility = Visibility.Collapsed;
                MergeDuplicates.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowSummary();
            }

            count = 0;
            i = 0;
         
        }

        /// <summary>
        /// Converts a DateTime to a short string.  If DateTime is null, returns an empty string.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string DateToString(DateTime? date)
        {
            return App.DateToString(date);
        }

        #endregion

    }
}