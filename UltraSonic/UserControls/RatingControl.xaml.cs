using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public sealed partial class RatingControl
    {
        public static readonly DependencyProperty RatingValueProperty = DependencyProperty.Register("RatingValue", typeof(int), typeof(RatingControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RatingValueChanged));

        private const int MaxValue = 5;

        public int RatingValue
        {
            get { return (int)GetValue(RatingValueProperty); }

            set
            {
                if (value < 0)
                    SetValue(RatingValueProperty, 0);
                else if (value > MaxValue)
                    SetValue(RatingValueProperty, MaxValue);
                else
                    SetValue(RatingValueProperty, value);
            }
        }

        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingControl parent = sender as RatingControl;
            int ratingValue = (int)e.NewValue;

            if (parent == null) return;

            UIElementCollection children = ((Grid)(parent.Content)).Children;
            ToggleButton button;

            for (int i = 0; i < ratingValue; i++)
            {
                button = children[i] as ToggleButton;

                if (button != null)
                    button.IsChecked = true;
            }

            for (int i = ratingValue; i < children.Count; i++)
            {
                button = children[i] as ToggleButton;

                if (button != null)
                    button.IsChecked = false;
            }
        }

        public RatingControl()
        {
            InitializeComponent();
        }

        private static readonly RoutedEvent _clickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RatingControl));

        public event RoutedEventHandler Click
        {
            add { AddHandler(_clickEvent, value); }
            remove { RemoveHandler(_clickEvent, value); }
        }

        public void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(_clickEvent);
            RaiseEvent(newEventArgs);
        }

        private void RatingButtonClickEventHandler(Object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            if (button != null)
            {
                int newRating = int.Parse((String)button.Tag);

                if (button.IsChecked.HasValue && (button.IsChecked.Value || newRating < RatingValue))
                    RatingValue = newRating;
                else
                    RatingValue = newRating - 1;

                RaiseClickEvent();
            }

            e.Handled = true;
        }
    }

}
