using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using HandyControl.Controls;
using KSynthesizer.Sources;

namespace Synthesizer.Windows
{
    public class FunctionComboBox : HandyControl.Controls.ComboBox
    {
        public FunctionType Function { get; set; } = FunctionType.Sin;

        public FunctionComboBox()
        {
            foreach(FunctionType func in Enum.GetValues(typeof(FunctionType)))
            {
                Items.Add(func);
            }
            SelectedItem = Function;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            Function = (FunctionType)SelectedItem;
        }
    }
}
