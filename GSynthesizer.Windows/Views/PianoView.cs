using KSynthesizer;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GSynthesizer.Windows.Views
{
    public class PianoView : UserControl
    {
        private StackPanel _toneStack = new StackPanel()
        {
            Orientation = Orientation.Horizontal
        };

        public PianoView()
        {
            Content = _toneStack;
        }

        private DelegateCommand<IonianTone?> _onAttack;
        public DelegateCommand<IonianTone?> OnAttack =>
            _onAttack ?? (_onAttack = new DelegateCommand<IonianTone?>(ExecuteOnAttack));

        void ExecuteOnAttack(IonianTone? tone)
        {
            if (AttackCommand?.CanExecute(tone) ?? false)
            {
                AttackCommand.Execute(tone);
            }
        }

        private DelegateCommand<IonianTone?> _onRelease;
        public DelegateCommand<IonianTone?> OnRelease =>
            _onRelease ?? (_onRelease = new DelegateCommand<IonianTone?>(ExecuteOnRelease));

        void ExecuteOnRelease(IonianTone? tone)
        {
            if (ReleaseCommand?.CanExecute(tone) ?? false)
            {
                ReleaseCommand.Execute(tone);
            }
        }

        public double ToneWidth
        {
            get { return (double)GetValue(ToneWidthProperty); }
            set { SetValue(ToneWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToneWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToneWidthProperty =
            DependencyProperty.Register("ToneWidth", typeof(double), typeof(PianoView), new PropertyMetadata(200.0));

        public ICommand AttackCommand
        {
            get { return (ICommand)GetValue(AttackCommandProperty); }
            set { SetValue(AttackCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AttackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttackCommandProperty =
            DependencyProperty.Register("AttackCommand", typeof(ICommand), typeof(PianoView), new PropertyMetadata(default));

        public ICommand ReleaseCommand
        {
            get { return (ICommand)GetValue(ReleaseCommandProperty); }
            set { SetValue(ReleaseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReleaseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReleaseCommandProperty =
            DependencyProperty.Register("ReleaseCommand", typeof(ICommand), typeof(PianoView), new PropertyMetadata(default));



        public int BaseOctave
        {
            get { return (int)GetValue(BaseOctaveProperty); }
            set { SetValue(BaseOctaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BaseOctave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseOctaveProperty =
            DependencyProperty.Register("BaseOctave", typeof(int), typeof(PianoView), new PropertyMetadata(1));



        public int MaximumOctave
        {
            get { return (int)GetValue(MaximumOctaveProperty); }
            set { SetValue(MaximumOctaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumOctave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumOctaveProperty =
            DependencyProperty.Register("MaximumOctave", typeof(int), typeof(PianoView), new PropertyMetadata(6));



        public bool AutoWidth
        {
            get { return (bool)GetValue(AutoWidthProperty); }
            set { SetValue(AutoWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoWidthProperty =
            DependencyProperty.Register("AutoWidth", typeof(bool), typeof(PianoView), new PropertyMetadata(true));



        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == nameof(ToneWidth))
            {
                ResizeTone();
            }

            if (e.Property.Name == nameof(MaximumOctave))
            {
                RefreshTones();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RefreshTones();
        }

        private void RefreshTones()
        {
            int count;
            if (AutoWidth)
            {
                count = (int)Math.Ceiling(ActualWidth / ToneWidth);
                count = Math.Min(count, MaximumOctave);
            }
            else
            {
                count = MaximumOctave;
            }

            if (_toneStack.Children.Count != count)
            {
                _toneStack.Children.Clear();
                for (int i = 0; i < count; i++)
                {
                    _toneStack.Children.Add(new ToneView()
                    {
                        Width = ToneWidth,
                        AttackCommand = OnAttack,
                        ReleaseCommand = OnRelease,
                        Octave = BaseOctave + i,
                    }) ;
                }
            }
        }

        private void ResizeTone()
        {
            foreach (ToneView child in _toneStack.Children)
            {
                child.Width = ToneWidth;
            }
        }
    }
}
