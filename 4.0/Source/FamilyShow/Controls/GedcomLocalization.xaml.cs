using System.Windows;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for GedcomLocalization.xaml
    /// </summary>
    public partial class GedcomLocalization : System.Windows.Controls.UserControl
    {
        public GedcomLocalization()
        {
            InitializeComponent();
        }

        #region routed events

        public static readonly RoutedEvent ContinueButtonClickEvent = EventManager.RegisterRoutedEvent(
            "ContinueButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GedcomLocalization));


        // Expose this event for this control's container
        public event RoutedEventHandler ContinueButtonClick
        {
            add { AddHandler(ContinueButtonClickEvent, value); }
            remove { RemoveHandler(ContinueButtonClickEvent, value); }
        }

        #endregion

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ContinueButtonClickEvent));            
        }
    }
}