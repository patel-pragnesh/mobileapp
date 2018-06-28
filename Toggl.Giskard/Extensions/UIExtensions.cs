﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Toggl.Giskard.Adapters;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Extensions
{
    public static class UIExtensions
    {
        public static IObservable<Unit> Tapped(this View button)
            => Observable
                .FromEventPattern(e => button.Click += e, e => button.Click -= e)
                .SelectUnit();

        public static IObservable<string> Text(this EditText editText)
            => Observable
                .FromEventPattern<TextChangedEventArgs>(e => editText.TextChanged += e, e => editText.TextChanged -= e)
                .Select(_ => editText.Text);

        public static Action<bool> BindIsVisible(this View view)
            => isVisible => view.Visibility = isVisible.ToVisibility();

        public static Action<string> BindText(this TextView textView)
            => text =>
            {
                if (textView.Text != text)
                    textView.Text = text;
            };

        public static Action<bool> BindChecked(this CompoundButton compoundButton)
            => isChecked => compoundButton.Checked = isChecked;
        
        public static Action<IList<T>> BindItems<T>(this BaseRecyclerAdapter<T> adapter)
            => collection => adapter.Items = collection;

        public static Action<bool> BindEnabled(this View view)
            => enabled => view.Enabled = enabled;
    }
}
