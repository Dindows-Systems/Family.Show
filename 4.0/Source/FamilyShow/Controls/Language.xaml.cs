using System.Windows;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Language.xaml
    /// </summary>
    public partial class Language : UserControl
    {
        #region Properties

        public IList<LanguagePair> Languages { get; private set; } 

        #endregion

        #region Nested Types

        public class LanguagePair
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        #endregion

        #region Initialization

        public Language()
        {
            LoadLanguages();
            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region Dependency Properties

        public string SelectedLanguage
        {
            get { return (string)GetValue(SelectedLanguageProperty); }
            set { SetValue(SelectedLanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLanguage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLanguageProperty =
            DependencyProperty.Register(
                "SelectedLanguage",
                typeof(string),
                typeof(Language),
                new UIPropertyMetadata("en-US", SelectedLanguagePropertyChanged));

        private static void SelectedLanguagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var languageCode = e.NewValue as string;            
            if (languageCode != Properties.Settings.Default.Language)
            {
                Properties.Settings.Default.Language = languageCode;
                Properties.Settings.Default.Save();
                var control = d as Language;
                control.RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));

                MessageBox.Show(Properties.Resources.LanguageChangeMessage, Properties.Resources.LanguageHeader, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        #endregion

        #region Routed Events

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Language));

        // Expose this event for this control's container
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #endregion

        #region Event Handlers

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }

        #endregion

        #region Private Methods

        private void LoadLanguages()
        {
            InitializeDefaultLanguage();

            if (File.Exists(Properties.Settings.Default.LanguagesFileName))
            {
                try
                {
                    var document = XDocument.Load(Properties.Settings.Default.LanguagesFileName);
                    var languages = from language in document.Descendants("Language")
                                    select new LanguagePair
                                    {
                                        Name = language.Attribute("Name").Value,
                                        Code = language.Attribute("Code").Value
                                    };

                    var languageList = languages.ToList();
                    if (languageList != null && languageList.Count != 0)
                    {
                        Languages = languageList;
                    }
                }
                catch { }
            }

            // Read the language setting and select the appropriate language in the dropdown box.
            var userLanguage = Languages.FirstOrDefault(
                pair => string.Equals(
                    pair.Code, Properties.Settings.Default.Language)) ?? Languages[0];
            SelectedLanguage = userLanguage.Code;
        }

        private void InitializeDefaultLanguage()
        {
            Languages = new List<LanguagePair>
            {
                new LanguagePair
                {
                    Name = "English (United States)",
                    Code = "en-US"
                }
            };
        }
        
        #endregion
    }
}