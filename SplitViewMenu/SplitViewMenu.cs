using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SplitViewMenu
{
    public sealed class SplitViewMenu : Control
    {
        internal static readonly DependencyProperty MenuItemDataTemplateSelectorProperty =
            DependencyProperty.Register("MenuItemDataTemplateSelector", typeof(DataTemplateSelector),
                typeof(SplitViewMenu), new PropertyMetadata(null));

        internal static readonly DependencyProperty NavMenuItemTemplateProperty =
            DependencyProperty.Register("NavMenuItemTemplate", typeof(DataTemplate), typeof(SplitViewMenu),
                new PropertyMetadata(null));

        internal static readonly DependencyProperty NavMenuItemContainerStyleProperty =
            DependencyProperty.Register("NavMenuItemContainerStyle", typeof(Style), typeof(SplitViewMenu),
                new PropertyMetadata(null));

        internal static readonly DependencyProperty InitialPageProperty =
            DependencyProperty.Register("InitialPage", typeof(Type), typeof(SplitViewMenu),
                new PropertyMetadata(null));

        internal static readonly DependencyProperty NavigationItemsProperty =
            DependencyProperty.Register("NavigationItems", typeof(IEnumerable<INavigationMenuItem>),
                typeof(SplitViewMenu),
                new PropertyMetadata(Enumerable.Empty<INavigationMenuItem>(), OnNavigationItemsPropertyChanged));

        private Button _backButton;
        private NavMenuListView _navMenuListView;
        private Frame _pageFrame;

        public SplitViewMenu()
        {
            DefaultStyleKey = typeof(SplitViewMenu);
            Loaded += OnSplitViewMenuLoaded;
        }

        public DataTemplateSelector MenuItemDataTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(MenuItemDataTemplateSelectorProperty); }
            set { SetValue(MenuItemDataTemplateSelectorProperty, value); }
        }

        public DataTemplate NavMenuItemTemplate
        {
            get { return (DataTemplate)GetValue(NavMenuItemTemplateProperty); }
            set { SetValue(NavMenuItemTemplateProperty, value); }
        }

        public Style NavMenuItemContainerStyle
        {
            get { return (Style)GetValue(NavMenuItemContainerStyleProperty); }
            set { SetValue(NavMenuItemContainerStyleProperty, value); }
        }

        public Type InitialPage
        {
            get { return (Type)GetValue(InitialPageProperty); }
            set { SetValue(InitialPageProperty, value); }
        }

        public IEnumerable<INavigationMenuItem> NavigationItems
        {
            get { return (IEnumerable<INavigationMenuItem>)GetValue(NavigationItemsProperty); }
            set { SetValue(NavigationItemsProperty, value); }
        }

        private void OnSplitViewMenuLoaded(object sender, RoutedEventArgs e)
        {
            if (InitialPage == null || _pageFrame == null)
                return;
            _pageFrame.Navigate(InitialPage);
        }

        private static void OnNavigationItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menu = (SplitViewMenu)d;
            if (menu._navMenuListView != null)
                menu._navMenuListView.ItemsSource = e.NewValue;
        }

        protected override void OnApplyTemplate()
        {
            _pageFrame = GetTemplateChild("PageFrame") as Frame;
            _navMenuListView = GetTemplateChild("NavMenuList") as NavMenuListView;
            _backButton = GetTemplateChild("BackButton") as Button;

            if (_navMenuListView != null)
            {
                _navMenuListView.ItemInvoked += OnNavMenuItemInvoked;
                _navMenuListView.ContainerContentChanging += OnContainerContextChanging;
            }

            if (_backButton != null)
            {
                _backButton.Click += OnBackButtonClick;
            }

            if (_pageFrame != null)
            {
                _pageFrame.Navigating += OnNavigatingToPage;
                _pageFrame.Navigated += OnNavigatedToPage;
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            var ignored = false;
            BackRequested(ref ignored);
        }

        private void BackRequested(ref bool handled)
        {
            if (_pageFrame == null)
                return;
            if (!_pageFrame.CanGoBack || handled)
                return;
            handled = true;
            _pageFrame.GoBack();
        }

        private static void OnContainerContextChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item is INavigationMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((INavigationMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            var page = e.Content as Page;
            if (page != null && e.Content != null)
            {
                var control = page;
                control.Loaded += PageLoaded;
            }
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= PageLoaded;
        }

        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back || !NavigationItems.Any())
                return;
            var item = NavigationItems.SingleOrDefault(p => p.DestinationPage == e.SourcePageType);
            if (item == null && _pageFrame.BackStackDepth > 0)
            {
                foreach (var entry in _pageFrame.BackStack.Reverse())
                {
                    item = NavigationItems.SingleOrDefault(p => p.DestinationPage == entry.SourcePageType);
                    if (item != null)
                        break;
                }
            }

            var container = (ListViewItem)_navMenuListView.ContainerFromItem(item);
            if (container != null)
                container.IsTabStop = false;
            _navMenuListView.SetSelectedItem(container);
            if (container != null)
                container.IsTabStop = true;
        }

        private void OnNavMenuItemInvoked(object sender, ListViewItem e)
        {
            var item = (INavigationMenuItem)((NavMenuListView)sender).ItemFromContainer(e);

            if (item?.DestinationPage != null &&
                item.DestinationPage != _pageFrame.CurrentSourcePageType)
            {
                _pageFrame.Navigate(item.DestinationPage, item.Arguments);
            }
        }
    }
}