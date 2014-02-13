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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.FamilyShowLib;
using System.ComponentModel;
using System.Collections;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for TagCloud.xaml
    /// </summary>
    public partial class TagCloud : System.Windows.Controls.UserControl
    {
        private static ListCollectionView lcv;

        #region dependency properties

        public static readonly DependencyProperty TagsCollectionProperty =
            DependencyProperty.Register("TagsCollection", typeof(Object), typeof(TagCloud),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(TagsCollectionProperty_Changed)));

        /// <summary>
        /// The Collection that will be used to build the Tag Cloud
        /// </summary>
        public Object TagsCollection
        {
            get { return (Object)GetValue(TagsCollectionProperty); }
            set { SetValue(TagsCollectionProperty, value); }
        }

        public static readonly DependencyProperty SortDescriptorProperty =
            DependencyProperty.Register("SortDescriptor", typeof(string), typeof(TagCloud));

        /// <summary>
        /// The SortDescriptor is used for sorting the TagsCollection
        /// </summary>
        public string SortDescriptor
        {
            get { return (string)GetValue(SortDescriptorProperty); }
            set { SetValue(SortDescriptorProperty, value); }
        }

        public static readonly DependencyProperty GroupDescriptorProperty =
            DependencyProperty.Register("GroupDescriptor", typeof(string), typeof(TagCloud));

        /// <summary>
        /// The GroupDescriptor is usef for goruping the collection into tags
        /// </summary>
        public string GroupDescriptor
        {
            get { return (string)GetValue(GroupDescriptorProperty); }
            set { SetValue(GroupDescriptorProperty, value); }
        }

        #endregion

        #region routed events

        public static readonly RoutedEvent TagSelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "TagSelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagCloud));

        public event RoutedEventHandler TagSelectionChanged
        {
            add { AddHandler(TagSelectionChangedEvent, value); }
            remove { RemoveHandler(TagSelectionChangedEvent, value); }
        }

        #endregion

        public TagCloud()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used as a filter predicate to see if the person should be included 
        /// </summary>
        /// <param name="o">Person object</param>
        /// <returns>True if the person should be included in the filter, otherwise false</returns>
        public static bool FilterPerson(object o)
        {
            Person p = o as Person;
            return (!string.IsNullOrEmpty(p.LastName));
        }

        private static void TagsCollectionProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            // ListCollectionView is used for sorting and grouping
            lcv = new ListCollectionView((IList)args.NewValue);

            TagCloud tagCloud = ((TagCloud)sender);

            // Apply sorting
            if (!string.IsNullOrEmpty(tagCloud.SortDescriptor))
                lcv.SortDescriptions.Add(new SortDescription(tagCloud.SortDescriptor, ListSortDirection.Ascending));

            // Group the collection into tags. The tag cloud will be based on the group Name and ItemCount
            lcv.GroupDescriptions.Add(new PropertyGroupDescription(tagCloud.GroupDescriptor));

            // Exclude people without lastnames
            lcv.Filter = new Predicate<object>(FilterPerson);

            tagCloud.TagCloudListBox.ItemsSource = lcv.Groups;
        }

        private void TagCloudListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewGroup selected = (CollectionViewGroup)((ListBox)sender).SelectedItem;

            if (selected != null)
                RaiseEvent(new RoutedEventArgs(TagSelectionChangedEvent, selected.Name));
        }

        internal static void Refresh()
        {
            lcv.Refresh();
        }

        internal void ClearSelection()
        {
            TagCloudListBox.UnselectAll();
        }
    }
}