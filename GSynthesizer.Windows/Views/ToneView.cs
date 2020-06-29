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
    public class ToneView : UserControl
    {
        private Grid grid = new Grid();

        public ToneView()
        {
            VerticalContentAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            Content = grid;

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            AddPianoKeyView(0, 3, IonianScale.C, false);
            AddPianoKeyView(3, 3, IonianScale.D, false);
            AddPianoKeyView(6, 3, IonianScale.E, false);
            AddPianoKeyView(9, 3, IonianScale.F, false);
            AddPianoKeyView(12, 3, IonianScale.G, false);
            AddPianoKeyView(15, 3, IonianScale.A, false);
            AddPianoKeyView(18, 3, IonianScale.B, false);

            AddPianoKeyView(2, 2, IonianScale.C, true);
            AddPianoKeyView(5, 2, IonianScale.D, true);
            AddPianoKeyView(11, 2, IonianScale.F, true);
            AddPianoKeyView(14, 2, IonianScale.G, true);
            AddPianoKeyView(17, 2, IonianScale.A, true);
        }

        private void AddPianoKeyView(int column, int span, IonianScale scale, bool sharp)
        {
            var view = new PianoKey()
            {
                Sharp = sharp,
                Scale = scale,
                AttackCommand = OnAttack,
                ReleaseCommand = OnRelease,
            };
            Grid.SetColumn(view, column);
            Grid.SetColumnSpan(view, span);
            Grid.SetRowSpan(view, sharp ? 1 : 2);
            grid.Children.Add(view);
        }

        private DelegateCommand<PianoKey> _onAttack;
        public DelegateCommand<PianoKey> OnAttack =>
            _onAttack ?? (_onAttack = new DelegateCommand<PianoKey>(ExecuteOnAttack));

        void ExecuteOnAttack(PianoKey view)
        {
            var tone = new IonianTone()
            {
                Scale = view.Scale,
                Sharp = view.Sharp,
                Octave = view.Scale == IonianScale.A || view.Scale == IonianScale.B ? Octave + 1 : Octave,
            };

            if (AttackCommand?.CanExecute(tone) ?? false)
            {
                AttackCommand.Execute(tone);
            }
        }

        private DelegateCommand<PianoKey> _onRelease;
        public DelegateCommand<PianoKey> OnRelease =>
            _onRelease ?? (_onRelease = new DelegateCommand<PianoKey>(ExecuteOnRelease));

        void ExecuteOnRelease(PianoKey view)
        {
            var tone = new IonianTone()
            {
                Scale = view.Scale,
                Sharp = view.Sharp,
                Octave = view.Scale == IonianScale.A || view.Scale == IonianScale.B ? Octave + 1 : Octave,
            };

            if (ReleaseCommand?.CanExecute(tone) ?? false)
            {
                ReleaseCommand.Execute(tone);
            }
        }

        public ICommand AttackCommand
        {
            get { return (ICommand)GetValue(AttackCommandProperty); }
            set { SetValue(AttackCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AttackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttackCommandProperty =
            DependencyProperty.Register("AttackCommand", typeof(ICommand), typeof(ToneView), new PropertyMetadata(default));

        public ICommand ReleaseCommand
        {
            get { return (ICommand)GetValue(ReleaseCommandProperty); }
            set { SetValue(ReleaseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReleaseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReleaseCommandProperty =
            DependencyProperty.Register("ReleaseCommand", typeof(ICommand), typeof(ToneView), new PropertyMetadata(default));

        public int Octave
        {
            get { return (int)GetValue(OctaveProperty); }
            set { SetValue(OctaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Octave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OctaveProperty =
            DependencyProperty.Register("Octave", typeof(int), typeof(ToneView), new PropertyMetadata(4));
    }
}
