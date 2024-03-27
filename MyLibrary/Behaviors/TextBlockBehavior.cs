using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyLibrary.Behaviors
{
    public class TextBlockBehavior : Behavior<TextBlock>
    {
        public static readonly DependencyProperty TextBlockCommandProperty =
           DependencyProperty.Register("TextBlockCommand", typeof(ICommand), typeof(TextBlockBehavior), new PropertyMetadata(null));

        public ICommand TextBlockCommand
        {
            get { return (ICommand)GetValue(TextBlockCommandProperty); }
            set { SetValue(TextBlockCommandProperty, value); }
        }

        public static readonly DependencyProperty TextBlockCommandParameterProperty =
            DependencyProperty.Register("TextBlockCommandParameter", typeof(object), typeof(TextBlockBehavior), new PropertyMetadata(null));

        public object TextBlockCommandParameter
        {
            get { return GetValue(TextBlockCommandParameterProperty); }
            set { SetValue(TextBlockCommandParameterProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= TextBlock_MouseLeftButtonDown;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ICommand command = TextBlockCommand;
            object parameter = TextBlockCommandParameter;
            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
