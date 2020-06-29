using KSynthesizer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GSynthesizer.Windows.Views
{
    public class PianoKey : UserControl
    {
        private bool attacked = false;

        private Rectangle Rectangle { get; } = new Rectangle()
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        public PianoKey()
        {
            VerticalContentAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            Content = Rectangle;
            Rectangle.Fill = DefaultKeyFill;
            Rectangle.Stroke = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            Rectangle.StrokeThickness = 1;
            MouseLeftButtonDown += PianoKey_MouseLeftButtonDown;
            MouseLeftButtonUp += PianoKey_MouseLeftButtonUp;
            MouseEnter += PianoKey_MouseEnter;
            MouseLeave += PianoKey_MouseLeave;
        }

        private void PianoKey_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                AttackKey();
            }
        }

        private void PianoKey_MouseLeave(object sender, MouseEventArgs e)
        {
            if (attacked)
            {
                ReleaseKey();
            }
        }

        private void PianoKey_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseKey();
        }

        private void PianoKey_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AttackKey();
        }

        private void AttackKey()
        {
            if (AttackCommand?.CanExecute(this) ?? false)
            {
                AttackCommand.Execute(this);
                attacked = true;
                Rectangle.Fill = Sharp ? SharpAttackKeyFill : AttackKeyFill;
            }
        }

        private void ReleaseKey()
        {
            if (ReleaseCommand?.CanExecute(this) ?? false)
            {
                ReleaseCommand.Execute(this);
                attacked = false;
                Rectangle.Fill = Sharp ? SharpDefaultKeyFill : DefaultKeyFill;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (attacked)
            {
                Rectangle.Fill = Sharp ? SharpDefaultKeyFill : DefaultKeyFill;
            }
            else
            {
                Rectangle.Fill = Sharp ? SharpDefaultKeyFill : DefaultKeyFill;
            }
        }

        public Brush DefaultKeyFill
        {
            get { return (Brush)GetValue(DefaultKeyFillProperty); }
            set { SetValue(DefaultKeyFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultKeyFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultKeyFillProperty =
            DependencyProperty.Register("DefaultKeyFill", typeof(Brush), typeof(PianoKey), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(250, 250, 250))));

        public Brush AttackKeyFill
        {
            get { return (Brush)GetValue(AttackKeyFillProperty); }
            set { SetValue(AttackKeyFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AttackKeyFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttackKeyFillProperty =
            DependencyProperty.Register("AttackKeyFill", typeof(Brush), typeof(PianoKey), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(140, 140, 140))));

        public Brush SharpDefaultKeyFill
        {
            get { return (Brush)GetValue(SharpDefaultKeyFillProperty); }
            set { SetValue(SharpDefaultKeyFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SharpDefaultKeyFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SharpDefaultKeyFillProperty =
            DependencyProperty.Register("SharpDefaultKeyFill", typeof(Brush), typeof(PianoKey), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(30, 30, 30))));

        public Brush SharpAttackKeyFill
        {
            get { return (Brush)GetValue(SharpAttackKeyFillProperty); }
            set { SetValue(SharpAttackKeyFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SharpAttackKeyFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SharpAttackKeyFillProperty =
            DependencyProperty.Register("SharpAttackKeyFill", typeof(Brush), typeof(PianoKey), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(100, 100, 100))));

        public IonianScale Scale
        {
            get { return (IonianScale)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(IonianScale), typeof(PianoKey), new PropertyMetadata(IonianScale.C));

        public bool Sharp
        {
            get { return (bool)GetValue(SharpProperty); }
            set { SetValue(SharpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sharp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SharpProperty =
            DependencyProperty.Register("Sharp", typeof(bool), typeof(PianoKey), new PropertyMetadata(false));

        public ICommand AttackCommand
        {
            get { return (ICommand)GetValue(AttackCommandProperty); }
            set { SetValue(AttackCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AttackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttackCommandProperty =
            DependencyProperty.Register("AttackCommand", typeof(ICommand), typeof(PianoKey), new PropertyMetadata(default(ICommand)));

        public ICommand ReleaseCommand
        {
            get { return (ICommand)GetValue(ReleaseCommandProperty); }
            set { SetValue(ReleaseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Release.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReleaseCommandProperty =
            DependencyProperty.Register("ReleaseCommand", typeof(ICommand), typeof(PianoKey), new PropertyMetadata(default(ICommand)));
    }
}
