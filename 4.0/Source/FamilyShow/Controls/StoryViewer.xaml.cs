using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.FamilyShowLib;
using System.Windows.Markup;
using System.Globalization;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for StoryViewer.xaml
    /// </summary>
    public partial class StoryViewer : System.Windows.Controls.UserControl
    {
        public StoryViewer()
        {
            InitializeComponent();

            // Set the language of the spell checker to be the language used by the OS.
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            StorysListBox.ItemsSource = App.Family;

            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
                FontsComboBox.Items.Add(fontFamily.Source);

            // Set the default sort order for the Family ListView to 
            // if a person has a note.
            ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(App.Family);
            view.SortDescriptions.Add(new SortDescription("HasNote", ListSortDirection.Descending));
            
        }

        #region routed events

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StoryViewer));

        // Expose the StoryViewer Close Button click event
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {

                // Reset the filter
                StoryFilterTextBox.Text = string.Empty;
                StorysListBox.FilterList(StoryFilterTextBox.Text);

                // Show the story, hide the editor
                StoryViewBorder.Visibility = Visibility.Visible;
                StoryEditBorder.Visibility = Visibility.Hidden;

                // Load the person story into the viewer
                LoadStoryText(StoryView.Document);

                // Display all text in constrast color to the StoryViewer background.
                TextRange textRange = new TextRange(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
                textRange.Select(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));

                // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
                StoryView.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
                StoryView.ViewingMode = FlowDocumentReaderViewingMode.Page;
            }
        }
        
        /// <summary>
        /// Handler for the CloseButton click event
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise the CloseButtonClickEvent to notify the container to close this control
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            StorysListBox.FilterList(StoryFilterTextBox.Text);
        }

        private void StorysListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (this.Visibility == Visibility.Visible)
            {
                EditStoryButton.Visibility = Visibility.Visible;
                PrintStoryButton.Visibility= Visibility.Visible;

                if (StorysListBox.SelectedItem != null)
                {
                    Person person = (Person)StorysListBox.SelectedItem;
                    Name1.Text = person.FullName;
                    Name2.Text = person.FullName;

                    if (person.Restriction == Restriction.Locked)
                        EditStoryButton.Visibility = Visibility.Collapsed;
                    if (person.Restriction == Restriction.Private)
                        PrintStoryButton.Visibility = Visibility.Collapsed;

                }
                else
                {
                    Name1.Text = string.Empty;
                    Name2.Text = string.Empty;
                }

                // Show the story, hide the editor
                StoryViewBorder.Visibility = Visibility.Visible;
                StoryEditBorder.Visibility = Visibility.Hidden;

                // Load the person story into the viewer
                LoadStoryText(StoryView.Document);

                // Display all text in constrast color to the StoryViewer background.
                TextRange textRange2 = new TextRange(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
                textRange2.Select(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
                textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));

                // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
                StoryView.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
                StoryView.ViewingMode = FlowDocumentReaderViewingMode.Page;
            }

        }

        #endregion

        #region Rich Text event handlers and helper methods

        /// <summary>
        /// Cancels editting a story.
        /// </summary>
        private void CancelStoryButton_Click(object sender, RoutedEventArgs e)
        {
            StoryEditBorder.Visibility = Visibility.Hidden;
            StoryViewBorder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Switch from the view mode to edit mode
        /// </summary>
        private void EditStoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStoryText(StoryRichTextBox.Document);

            StoryEditBorder.Visibility = Visibility.Visible;
            StoryViewBorder.Visibility = Visibility.Hidden;

            StoryRichTextBox.Focus();
        }
		
		/// <summary>
        /// Print the contents of the storyviewer
        /// </summary>
        private void PrintStoryButton_Click(object sender, RoutedEventArgs e)
        {
            StoryView.ViewingMode = FlowDocumentReaderViewingMode.Scroll;  //makes the file print full page
            StoryView.Print();  
        }

        private void SaveStoryButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)StorysListBox.SelectedItem;

            if (person != null)
            {
                   // Pass in a TextRange object to save the story     
                   TextRange textRange = new TextRange(StoryRichTextBox.Document.ContentStart, StoryRichTextBox.Document.ContentEnd);

                    person.Story = new Story();
                    //remove spaces  and {} from history file names
                    //also use unique person Id in the file name so people with same name don't have thier history overwritten
                    string storyFileName = new StringBuilder(App.ReplaceEncodedCharacters(person.FullName + "(" + person.Id + ")")).Append(".rtf").ToString();
                    person.Story.Save(textRange, storyFileName);
                    person.Note = textRange.Text;

                    // Display the rich text in the viewer
                    LoadStoryText(StoryView.Document);

                    // Display all text in constrast color to the StoryViewer background.
                    TextRange textRange2 = new TextRange(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
                    textRange2.Select(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
                    textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));         

            }

            // Switch to view mode
            StoryEditBorder.Visibility = Visibility.Hidden;
            StoryViewBorder.Visibility = Visibility.Visible;

            // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
            StoryView.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
            StoryView.ViewingMode = FlowDocumentReaderViewingMode.Page;

        }

        private void LoadStoryText(FlowDocument flowDocument)
        {

            // Ignore null cases
            if (flowDocument == null || flowDocument.Blocks == null || StorysListBox.SelectedItem==null)
               return;

            // Clear out any existing text in the viewer 
            flowDocument.Blocks.Clear();

            Person person = (Person)StorysListBox.SelectedItem;

            // Load the story into the story viewer
            if (person != null && person.Story != null)
            {
                TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                person.Story.Load(textRange);
				person.Note = textRange.Text;
            }
            else
            {
                // This person doesn't have a story.
                // Load the default story text
                TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Text = Properties.Resources.DefaultStory;

                textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, Properties.Resources.StoryFontFamily);
                textRange.ApplyPropertyValue(TextElement.FontSizeProperty, Properties.Resources.StoryFontSize);
            }
        }

        private void FontsComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            StoryRichTextBox.Selection.ApplyPropertyValue(FontFamilyProperty, FontsComboBox.SelectedValue);
        }

        void StoryRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Update the toolbar controls based on the current selected text.
            UpdateButtons();
        }

        void StoryRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the toolbar controls based on the current selected text.
            UpdateButtons();
        }

        /// <summary>
        /// Update the toolbar controls based on the current selected text.
        /// </summary>
        private void UpdateButtons()
        {
            // Need to use object since GetPropertyValue returns different types
            // depending on the text selected.
            object result;

            // Bold button.
            result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontWeightProperty);
            BoldButton.IsChecked = (result != null && result is FontWeight &&
                (FontWeight)result == FontWeights.Bold);

            // Italic button.
            result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontStyleProperty);
            ItalicButton.IsChecked = (result != null && result is FontStyle &&
                (FontStyle)result == FontStyles.Italic);

            // Font list.
            result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontFamilyProperty);
            if (result != null && result is FontFamily)
                FontsComboBox.SelectedItem = result.ToString();

            // Align buttons.
            result = StoryRichTextBox.Selection.GetPropertyValue(Paragraph.TextAlignmentProperty);
            AlignLeftButton.IsChecked = (result != null && result is TextAlignment
                && (TextAlignment)result == TextAlignment.Left);
            AlignCenterButton.IsChecked = (result != null && result is TextAlignment
                && (TextAlignment)result == TextAlignment.Center);
            AlignRightButton.IsChecked = (result != null && result is TextAlignment
                && (TextAlignment)result == TextAlignment.Right);
            AlignFullButton.IsChecked = (result != null && result is TextAlignment
                && (TextAlignment)result == TextAlignment.Justify);

            // Underline button.
            result = StoryRichTextBox.Selection.GetPropertyValue(Paragraph.TextDecorationsProperty);
            if (result != null && result is TextDecorationCollection)
            {
                TextDecorationCollection decorations = (TextDecorationCollection)result;
                UnderlineButton.IsChecked = (decorations.Count > 0 &&
                    decorations[0].Location == TextDecorationLocation.Underline);
            }
            else
                UnderlineButton.IsChecked = false;

            // bullets
            UpdateBulletButtons();   
        }

        /// <summary>
        /// Update the bullet toolbar buttons.
        /// </summary>
        private void UpdateBulletButtons()
        {
            // The bullet information takes a little more work, need
            // to walk the tree and look for a ListItem element.
            TextElement element = StoryRichTextBox.Selection.Start.Parent as TextElement;
            while (element != null)
            {
                if (element is ListItem)
                {
                    // Found a bullet item, determine the type of bullet.
                    ListItem item = element as ListItem;
                    BulletsButton.IsChecked = (item.List.MarkerStyle != TextMarkerStyle.Decimal);
                    NumberingButton.IsChecked = (item.List.MarkerStyle == TextMarkerStyle.Decimal);
                    return;
                }
                element = element.Parent as TextElement;
            }
            
            // Did not find a bullet item.
            BulletsButton.IsChecked = false;
            NumberingButton.IsChecked = false;
        }

        #endregion

        #region helper methods

        public void SetDefaultFocus()
        {
            CloseButton.Focus();
        }

        /// <summary>
        /// The details of a person changed.
        /// </summary>
        public void OnThemeChange()
        {
            // Display all text in constrast color to the StoryViewer background.
            TextRange textRange = new TextRange(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
            textRange.Select(StoryView.Document.ContentStart, StoryView.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));
        }

        #endregion

    }
}