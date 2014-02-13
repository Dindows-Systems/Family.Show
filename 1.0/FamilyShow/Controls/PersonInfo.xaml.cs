using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.FamilyShowLib;
using System.IO;
using System.Globalization;

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
        }

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PersonInfo));

        // Expose the PersonInfo close button click event
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
                // Load the person story into the viewer
                LoadStoryText(StoryViewer.Document);
                StoryViewBorder.Visibility = Visibility.Visible;

                // Display all text in constrast color to the StoryViewer background.
                TextRange textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                // Hide the Story Edit
                StoryEditBorder.Visibility = Visibility.Hidden;

                if (DisplayPhoto.Source == null)
                {
                    TagsStackPanel.Visibility = Visibility.Hidden;
                    PhotoButtonsDockPanel.Visibility = Visibility.Hidden;
                }
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

        private void CancelStoryButton_Click(object sender, RoutedEventArgs e)
        {
            StoryEditBorder.Visibility = Visibility.Hidden;
            StoryViewBorder.Visibility = Visibility.Visible;
        }

        private void EditStoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Load the person story into the editor
            LoadStoryText(StoryRichTextBox.Document);

            // Switch from the view mode to edit mode
            StoryEditBorder.Visibility = Visibility.Visible;
            StoryViewBorder.Visibility = Visibility.Hidden;

            StoryRichTextBox.Focus();
        }

        private void SaveStoryButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            if (person != null)
            {
                // Pass in a TextRange object to save the story     
                TextRange textRange = new TextRange(StoryRichTextBox.Document.ContentStart, StoryRichTextBox.Document.ContentEnd);
                person.Story = new Story();
                string storyFileName = new StringBuilder(person.Name).Append(" {").Append(person.Id).Append("}").Append(".rtf").ToString();
                person.Story.Save(textRange, storyFileName);

                // Display the rich text in the viewer
                LoadStoryText(StoryViewer.Document);

                // Display all text in constrast color to the StoryViewer background.
                TextRange textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
                textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }

            StoryEditBorder.Visibility = Visibility.Hidden;
            StoryViewBorder.Visibility = Visibility.Visible;
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
            }
            else
            {
                // This person doesn't have a story.

                // Load the default lorem ipsum text
                flowDocument.Blocks.Add(new Paragraph(new Run(Properties.Resources.DefaultStory)));
            }
        }

        #endregion

        #region Photos event handlers and helper methods

        private void PhotosListBox_Drop(object sender, DragEventArgs e)
        {
            Person person = (Person)this.DataContext;

            // Retrieve the dropped files
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            // Get the files that is supported and add them to the photos for the person
            foreach (string fileName in fileNames)
            {
                // Handles photo files
                if (IsFileSupported(fileName))
                {
                    Photo photo = new Photo(fileName);

                    // Make the first photo added the person's avatar
                    if (person.Photos.Count == 0)
                    {
                        photo.IsAvatar = true;
                        PhotosListBox.SelectedIndex = 0;
                    }

                    // Associate the photo with the person.
                    person.Photos.Add(photo);

                    // Setter for property change notification
                    person.Avatar = "";
                }
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

                PhotoButtonsDockPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Clear the display photo
                DisplayPhoto.Source = new BitmapImage();

                // Hide the photos and tags
                PhotoButtonsDockPanel.Visibility = Visibility.Hidden;
                TagsStackPanel.Visibility = Visibility.Hidden;

                // Clear tags and caption
                TagsListBox.ItemsSource = null;
                CaptionTextBlock.Text = string.Empty;
            }
        }

        /// <summary>
        /// Set the display photo
        /// </summary>
        private void SetDisplayPhoto(String path)
        {
            DisplayPhoto.Source = new BitmapImage(new Uri(path));

            // Make sure the photo supports meta data before retrieving and displaying it
            if (HasMetaData(path))
            {
                // Extract the photo's metadata
                BitmapMetadata metadata = (BitmapMetadata)BitmapFrame.Create(new Uri(path)).Metadata;

                // Display the photo's tags
                TagsStackPanel.Visibility = Visibility.Visible;
                TagsListBox.ItemsSource = metadata.Keywords;

                // Display the photo's comment
                CaptionTextBlock.Text = metadata.Title;
            }
            else
            {
                // Clear tags and caption
                TagsStackPanel.Visibility = Visibility.Hidden;
                TagsListBox.ItemsSource = null;
                CaptionTextBlock.Text = string.Empty;
            }
        }

        private void SetPrimaryButton_Click(object sender, RoutedEventArgs e)
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

        private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = (Person)this.DataContext;

            Photo photo = (Photo)PhotosListBox.SelectedItem;
            if (photo != null)
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
            }
        }

        /// <summary>
        /// Only allow the most common photo formats (JPEG, PNG, and GIF)
        /// </summary>
        private static bool IsFileSupported(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);

            if (string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".png", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".gif", true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
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

        #endregion
    }
}