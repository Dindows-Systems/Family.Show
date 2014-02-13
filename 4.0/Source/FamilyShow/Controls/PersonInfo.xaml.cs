using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.FamilyShowLib;
using System.Windows.Markup;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for PersonInfo.xaml
    /// </summary>
    public partial class PersonInfo : System.Windows.Controls.UserControl
    {
        public PersonInfo()
        {
            InitializeComponent();

            // Set the language of the spell checker to be the language used by the OS.
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
                FontsComboBox.Items.Add(fontFamily.Source);
        }

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PersonInfo));

        // Expose the PersonInfo Close Button click event
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #region event handlers

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                // Show the story, hide the editor
                StoryViewBorder.Visibility = Visibility.Visible;
                StoryEditBorder.Visibility = Visibility.Hidden;

                // Load the person story into the viewer
                LoadStoryText(StoryViewer.Document);

                // Display all text in constrast color to the StoryViewer background.
                TextRange textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));

                // Hide the photo tags and photo edit buttons if there is no main photo.
                // Hide the photo tags and photo edit buttons if there is no main photo.
                if (DisplayPhoto.Source == null)
                {
                    TagsStackPanel.Visibility = Visibility.Hidden;
                    CaptionTextBlock.Visibility = Visibility.Hidden;
                }

                // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
                StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
                StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Page;
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
            StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;  //makes the file print full page
            StoryViewer.Print();
        }

        private void SaveStoryButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

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
                LoadStoryText(StoryViewer.Document);

                // Display all text in constrast color to the StoryViewer background.
                TextRange textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));
            }

            // Switch to view mode
            StoryEditBorder.Visibility = Visibility.Hidden;
            StoryViewBorder.Visibility = Visibility.Visible;

            // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
            StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
            StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Page;
        }

        private void LoadStoryText(FlowDocument flowDocument)
        {
            // Ignore null cases
            if (flowDocument == null || flowDocument.Blocks == null || this.DataContext == null)
                return;

            // Clear out any existing text in the viewer 
            flowDocument.Blocks.Clear();

            Person person = (Person)this.DataContext;

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
            //Update the toolbar controls based on the current selected text.
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

        #region Photos event handlers and helper methods

        /// <summary>
        /// Link an existing photo.  
        /// If not existing photo or photo is not in "Family Data\Images" then return warning.
        /// If file not supported, return warning.
        /// </summary>
        private void Link_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                App.ApplicationFolderName);
            appLocation = Path.Combine(appLocation, App.AppDataFolderName);

            // Absolute path to the photos folder
            string photoLocation = Path.Combine(appLocation, Photo.PhotosFolderName);

            int photoCount = 0;

            if (Directory.Exists(photoLocation))
                photoCount = Directory.GetFiles(photoLocation).Length;

            if (photoCount > 0)
            {
                CommonDialog dialog = new CommonDialog();
                dialog.InitialDirectory = photoLocation;
                dialog.Filter.Add(new FilterEntry(Properties.Resources.ImageFiles, Properties.Resources.ImageExtension));
                dialog.Title = Properties.Resources.Link;
                dialog.ShowOpen();

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    if (File.Exists(Path.Combine(photoLocation, Path.GetFileName(dialog.FileName))))  //only link files which are in the temp directory
                    {
                        if (App.IsPhotoFileSupported(Path.GetFileName(dialog.FileName)))
                        {
                            int i = 0;

                            foreach (Photo p in person.Photos)
                            {
                                if (p.RelativePath.ToString() == Path.Combine(Photo.PhotosFolderName, Path.GetFileName(dialog.FileName)))
                                    i++;
                            }

                            if (i == 0)
                            {
                                Photo photo = new Photo();
                                photo.RelativePath = Path.Combine(Photo.PhotosFolderName, Path.GetFileName(dialog.FileName));
                                // Associate the photo with the person.
                                person.Photos.Add(photo);

                                if (person.Photos.Count == 0)
                                {
                                    PhotosListBox.SelectedIndex = 0;
                                }

                                // Setter for property change notification
                                person.Avatar = "";
                            }
                            else
                            {
                                MessageBox.Show(Properties.Resources.PhotoExistsMessage,
                                    Properties.Resources.LinkFailed, MessageBoxButton.OK, MessageBoxImage.Warning);

                            }
                        }
                    }
                    else
                    {
                            MessageBox.Show(Properties.Resources.LinkAddMessage,
                                    Properties.Resources.LinkFailed, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                person.OnPropertyChanged("HasPhoto");
            }
            else
            {
                MessageBox.Show(Properties.Resources.NoExistingPhotos,
                        Properties.Resources.LinkFailed, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        /// <summary>
        /// Add a photo.  
        /// If the photo name exists, append (#) to the file name.
        /// If the file is not supported inform the user.
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            string appLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appLocation = Path.Combine(appLocation, App.AppDataFolderName);

            // Absolute path to the photos folder
            string photoLocation = Path.Combine(appLocation, Photo.PhotosFolderName);
            CommonDialog dialog = new CommonDialog();
            dialog.Filter.Add(new FilterEntry(Properties.Resources.ImageFiles,Properties.Resources.ImageExtension));
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            dialog.Title = Properties.Resources.Open;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                if (App.IsPhotoFileSupported(Path.GetFileName(dialog.FileName)))
                {
                    Photo photo = new Photo(dialog.FileName);
                    photo.RelativePath = Path.Combine(Photo.PhotosFolderName, Path.GetFileName(dialog.FileName));

                    // Associate the photo with the person.
                    person.Photos.Add(photo);

                    if (person.Photos.Count == 0)
                    {
                        PhotosListBox.SelectedIndex = 0;
                    }

                    // Setter for property change notification
                    person.Avatar = "";
                }
            }

            person.OnPropertyChanged("HasPhoto");

        }

        /// <summary>
        /// Handle dropped files.
        /// </summary>
        private void PhotosListBox_Drop(object sender, DragEventArgs e)
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

                        // Make the first photo added the person's avatar
                        if (person.Photos.Count == 0)
                        {
                            //  photo.IsAvatar = true;  //turned this off as I found it became more a hinderance than a help, especially when dragging mulitple files, I may not want the first file to be the primary.
                            PhotosListBox.SelectedIndex = 0;
                        }

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
            }
            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;
        }

        private void PhotosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox photosListBox = sender as ListBox;
            if (photosListBox.SelectedIndex != -1)
            {
                // Get the path to the selected photo
                String path = photosListBox.SelectedItem.ToString();

                // Make sure that the file exists
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                    SetDisplayPhoto(path);

            }
            else
            {
                // Clear the display photo
                DisplayPhoto.Source = null;//new BitmapImage();

                // Hide the photos and tags

                TagsStackPanel.Visibility = Visibility.Hidden;

                //Clear tags and caption
                TagsListBox.ItemsSource = null;
                CaptionTextBlock.Text = string.Empty;
                CaptionTextBlock.ToolTip = null; ;
            }
        }

        /// <summary>
        /// Set the display photo
        /// </summary>
        private void SetDisplayPhoto(String path)
        {
            
            //This code must be used to create the bitmap
            //otherwise the program locks the image.
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelHeight = 280;  //max height of photo in viewer
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            DisplayPhoto.Source = bitmap;

            // Make sure the photo supports meta data before retrieving and displaying it
            if (HasMetaData(path))
            {
                // Extract the photo's metadata
                BitmapMetadata metadata = (BitmapMetadata)BitmapFrame.Create(new Uri(path)).Metadata;

                // Display the photo's tags
                if (metadata.Keywords != null)
                {
                    TagsStackPanel.Visibility = Visibility.Visible;
                    TagsListBox.ItemsSource = metadata.Keywords;
                }
                else
                {
                    TagsStackPanel.Visibility = Visibility.Hidden;
                    TagsListBox.ItemsSource = null;
                }

                // Display the photo's comment
                if (metadata.Title != null)
                {
                    CaptionTextBlock.Visibility = Visibility.Visible;
                    CaptionTextBlock.Text = metadata.Title;
                    CaptionTextBlock.ToolTip = metadata.Title;  //displays the full title if the title won't fit in the box
                }
                else
                {
                    CaptionTextBlock.Visibility = Visibility.Hidden;
                    CaptionTextBlock.Text = string.Empty;
                    CaptionTextBlock.ToolTip = null;
                }
            }
            else
            {
                // Clear tags and caption
                TagsStackPanel.Visibility = Visibility.Hidden;
                TagsListBox.ItemsSource = null;
                CaptionTextBlock.Text = string.Empty;
                CaptionTextBlock.ToolTip = null;
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            if (person.Photos != null && PhotosListBox.SelectedItem != null)
            {
                // Set IsAvatar to false for existing photos
                foreach (Photo existingPhoto in person.Photos)
                {
                    existingPhoto.IsAvatar = false;
                }

                Photo photo = (Photo)PhotosListBox.SelectedItem;
                photo.IsAvatar = true;
                person.Avatar = photo.FullyQualifiedPath;
            }
        }

        /// <summary>
        /// Creates a temporary copy of the file in the family data directory with a GUID and then opens that file.
        /// This prevents problems such as:
        /// 1. On opening a familyx file, the process may fail if another program
        ///    has locked a file in the Photos directory if the new file has a photo of the same name.
        /// Any temp files will be cleaned up on application open or close once the other program has been closed.   
        /// </summary>
        private void OpenPhotoButton_Click(object sender, RoutedEventArgs e)
        {

            Person person = (Person)this.DataContext;
            Photo photo = (Photo)PhotosListBox.SelectedItem;
            string path = photo.FullyQualifiedPath;

            string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationFolderName);
            appLocation = Path.Combine(appLocation, App.AppDataFolderName);

            string fileExtension = Path.GetExtension(path);
            string newFileName = Path.GetFileNameWithoutExtension(path) + Guid.NewGuid().ToString() + fileExtension;
            string tempFilePath = Path.Combine(appLocation, newFileName);

            FileInfo ofi = new FileInfo(path);
            ofi.CopyTo(tempFilePath, true);

            try
            {
                System.Diagnostics.Process.Start(tempFilePath);
            }
            catch { }

        }

        /// <summary>
        /// Deletes the link between a photo and a person 
        /// </summary>
        private void DeletePhotoButton_Click(object sender, RoutedEventArgs e)
        {

            Person person = (Person)this.DataContext;

            Photo photo = (Photo)PhotosListBox.SelectedItem;
            if (photo != null)
            {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDeletePhoto,
                   Properties.Resources.Photo, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    person.Photos.Remove(photo);
                    // Removed photo is an avatar, set a different avatar photo
                    if (photo.IsAvatar && person.Photos.Count > 0)
                    {
                        person.Photos[0].IsAvatar = true;
                        person.Avatar = person.Photos[0].FullyQualifiedPath;
                    }
                    else
                        // Setter for property change notification
                        person.Avatar = "";

                    person.OnPropertyChanged("HasPhoto");
                }
            }


        }

        /// <summary>
        /// Allow a person to have no primary photo
        /// </summary>
        private void NoPrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;
            Photo photo = (Photo)PhotosListBox.SelectedItem;

            if (person.Photos != null && PhotosListBox.SelectedItem != null)
            {
                // Set IsAvatar to false for existing photos
                foreach (Photo existingPhoto in person.Photos)
                    existingPhoto.IsAvatar = false;

                person.Avatar = "";
            }
        }

        /// <summary>
        /// Only JPEG photos support metadata.
        /// </summary>
        private static bool HasMetaData(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);

            if (string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        #endregion

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
            TextRange textRange = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
            textRange.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));
        }

        #endregion

        private void PhotosListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            Person person = (Person)this.DataContext;

            if (person.Restriction != Restriction.Locked)
            {
                if (e.Key == Key.Delete)
                    DeletePhotoButton_Click(sender, e);
            }
        }
    }
}