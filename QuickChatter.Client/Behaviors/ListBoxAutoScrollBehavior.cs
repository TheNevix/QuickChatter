using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace QuickChatter.Client.Behaviors
{
    public static class ListBoxAutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll",
                typeof(bool),
                typeof(ListBoxAutoScrollBehavior),
                new PropertyMetadata(false, OnAutoScrollChanged));

        public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);

        public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox && e.NewValue is true)
            {
                ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (sender, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        listBox.ScrollIntoView(args.NewItems[0]);
                    }
                };
            }
        }
    }
}
