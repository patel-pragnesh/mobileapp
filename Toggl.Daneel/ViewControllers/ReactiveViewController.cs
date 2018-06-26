﻿using System;
using System.Reactive.Disposables;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Views;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Daneel.ViewControllers
{
    public abstract class ReactiveViewController<TViewModel> : MvxViewController<TViewModel>, IReactiveBindingHolder
        where TViewModel : class, IMvxViewModel
    {
        public CompositeDisposable DisposeBag { get; private set; } = new CompositeDisposable();

        protected ReactiveViewController(string nibName)
            : base(nibName, null) { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            DisposeBag?.Dispose();
        }
    }
}
