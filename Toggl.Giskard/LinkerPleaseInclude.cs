﻿using System;
using System.Collections.Specialized;
using System.Windows.Input;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Target;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using V4Space = Android.Support.V4.Widget.Space;
using Newtonsoft.Json.Converters;
using Firebase.Provider;
using MvvmCross.Platforms.Android.Binding.Views;

namespace Toggl.Giskard
{
    // This class is never actually executed, but when Xamarin linking is enabled it does so to ensure types and properties
    // are preserved in the deployed app
    public class LinkerPleaseInclude
    {
        public void Include(Button button)
        {
            button.Click += (s,e) => button.Text = button.Text + "";
        }

        public void Include(CheckBox checkBox)
        {
            checkBox.CheckedChange += (sender, args) => checkBox.Checked = !checkBox.Checked;
        }
        
        public void Include(Switch @switch)
        {
            @switch.CheckedChange += (sender, args) => @switch.Checked = !@switch.Checked;
        }

        public void Include(SwitchCompat @switch)
        {
            @switch.CheckedChange += (sender, args) => @switch.Checked = !@switch.Checked;
        }

        public void Include(View view)
        {
            view.Click += (s, e) => view.ContentDescription = view.ContentDescription + "";
        }

        public void Include(TextView text)
        {
            text.AfterTextChanged += (sender, args) => text.Text = "" + text.Text;
            text.Hint = "" + text.Hint;
        }
        
        public void Include(CheckedTextView text)
        {
            text.AfterTextChanged += (sender, args) => text.Text = "" + text.Text;
            text.Hint = "" + text.Hint;
        }

        public void Include(CompoundButton cb)
        {
            cb.CheckedChange += (sender, args) => cb.Checked = !cb.Checked;
        }

        public void Include(SeekBar sb)
        {
            sb.ProgressChanged += (sender, args) => sb.Progress = sb.Progress + 1;
        }

        public void Include(RadioGroup radioGroup)
        {
            radioGroup.CheckedChange += (sender, args) => radioGroup.Check(args.CheckedId);
        }

        public void Include(EditText editText)
        {
            editText.FocusChange += (sender, args) => editText.Text = args.ToString();
        }

        public void Include(RadioButton radioButton)
        {
            radioButton.CheckedChange += (sender, args) => radioButton.Checked = args.IsChecked;
        }
        
        public void Include(RatingBar ratingBar)
        {
            ratingBar.RatingBarChange += (sender, args) => ratingBar.Rating = 0 + ratingBar.Rating;
        }

        public void Include(Activity act)
        {
            act.Title = act.Title + "";
        }

        public void Include(INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += (s,e) => { var test = $"{e.Action}{e.NewItems}{e.NewStartingIndex}{e.OldItems}{e.OldStartingIndex}"; };
        }

        public void Include(ICommand command)
        {
            command.CanExecuteChanged += (s, e) => { if (command.CanExecute(null)) command.Execute(null); };
        }
        
        public void Include(MvvmCross.IoC.MvxPropertyInjector injector)
        {
            injector = new MvvmCross.IoC.MvxPropertyInjector ();
        } 

        public void Include(System.ComponentModel.INotifyPropertyChanged changed)
        {
            changed.PropertyChanged += (sender, e) =>  {
                var test = e.PropertyName;
            };
        }
        
        public void Include(MvxTaskBasedBindingContext context)
        {
            context.Dispose();
            var context2 = new MvxTaskBasedBindingContext();
            context2.Dispose();
        }

        public void Include(MvxNavigationService service, IMvxViewModelLoader loader)
        {
            service = new MvxNavigationService(null, loader);
        }

        public void Include(FitWindowsLinearLayout linearLayout)
        {
            linearLayout = new FitWindowsLinearLayout(null);
            linearLayout = new FitWindowsLinearLayout(null, null);
        }

        public void Include(CardView cardView)
        {
            cardView = new CardView(null);
            cardView = new CardView(null, null);
            cardView = new CardView(null, null, 0);
        }

        public void Include(MvxCompoundButtonCheckedTargetBinding binding)
        {
            binding = new MvxCompoundButtonCheckedTargetBinding(null, null);
        }

        public void Include(MvxSeekBarProgressTargetBinding binding)
        {
            binding = new MvxSeekBarProgressTargetBinding(null, null);
        }

        public void Include(MvxTimePicker timePicker)
        {
            timePicker = new MvxTimePicker(null);
            timePicker = new MvxTimePicker(null, null);
        }

        public void Include(MvxDatePicker datePicker)
        {
            datePicker = new MvxDatePicker(null);
            datePicker = new MvxDatePicker(null, null);
        }

        public void Include(FitWindowsFrameLayout layout)
        {
            layout = new FitWindowsFrameLayout(null);
            layout = new FitWindowsFrameLayout(null, null);
        }

        public void Include(AlertDialogLayout layout)
        {
            layout = new AlertDialogLayout(null);
            layout = new AlertDialogLayout(null, null);
        }

        public void Include(DialogTitle title)
        {
            title = new DialogTitle(null);
            title = new DialogTitle(null, null);
            title = new DialogTitle(null, null, 0);
        }
        public void Include(V4Space space)
        {
            space = new V4Space(null);
            space = new V4Space(null, null);
            space = new V4Space(null, null, 0);
        }
        public void Include(ButtonBarLayout layout)
        {
            layout = new ButtonBarLayout(null, null);
        }
      
        public void Include(StringEnumConverter converter)
        {
            converter = new StringEnumConverter(true);
        }

        public void Include(ConsoleColor color)
        {
            Console.Write("");
            Console.WriteLine("");
            color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        public void Include(FirebaseInitProvider provider)
        {
            provider = new FirebaseInitProvider();
        }
    }
}
