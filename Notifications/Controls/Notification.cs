﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.ComponentModel;

namespace Notifications.Controls
{
    public class Notification : ContentControl, INotifyPropertyChanged
    {
        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationClosingEvent = EventManager.RegisterRoutedEvent(
           "NotificationClosing", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        private Duration expirationTime;

        public event RoutedEventHandler NotificationClosed
        {
            add { AddHandler(NotificationClosedEvent, value); }
            remove { RemoveHandler(NotificationClosedEvent, value); }
        }

        public event RoutedEventHandler NotificationCloseInvoked
        {
            add { AddHandler(NotificationCloseInvokedEvent, value); }
            remove { RemoveHandler(NotificationCloseInvokedEvent, value); }
        }

        public event RoutedEventHandler NotificationClosing
        {
            add { AddHandler(NotificationClosingEvent, value); }
            remove { RemoveHandler(NotificationClosingEvent, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Duration ExpirationTime
        {
            get { return expirationTime; }
            set
            {
                expirationTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExpirationTime"));
            }
        }

        public bool IsClosing { get; set; }

        public virtual async Task CloseAsync(TimeSpan expirationTime)
        {
            ExpirationTime = expirationTime;

            RaiseEvent(new RoutedEventArgs(NotificationClosingEvent));

            if (expirationTime == TimeSpan.MaxValue)
            {
                return;
            }
            await Task.Delay(expirationTime);

            await InternalCloseAsync();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var storyboards = Style.Triggers.OfType<EventTrigger>().FirstOrDefault(t => t.RoutedEvent == NotificationCloseInvokedEvent)?.Actions.OfType<BeginStoryboard>().Select(a => a.Storyboard);
            _closingAnimationTime = new TimeSpan(storyboards?.Max(s => Math.Min((s.Duration.HasTimeSpan ? s.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero) : TimeSpan.MaxValue).Ticks, s.Children.Select(ch => ch.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero)).Max().Ticks)) ?? 0);
        }

        internal async Task InternalCloseAsync()
        {
            if (IsClosing)
            {
                return;
            }
            IsClosing = true;

            RaiseEvent(new RoutedEventArgs(NotificationCloseInvokedEvent));
            await Task.Delay(_closingAnimationTime);
            RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }
    }
}