﻿using System.ComponentModel;
namespace Xamarin.Forms.OpenTok
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class OpenTokView : View
    {
        public static BindableProperty IsVideoViewRunningProperty = BindableProperty.Create(nameof(IsVideoViewRunning), typeof(bool), typeof(OpenTokView), false, BindingMode.OneWayToSource);

        public bool IsVideoViewRunning
        {
            get => (bool)GetValue(IsVideoViewRunningProperty);
            set => SetValue(IsVideoViewRunningProperty, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetIsVideoViewRunning(bool value) => IsVideoViewRunning = value;
    }
}