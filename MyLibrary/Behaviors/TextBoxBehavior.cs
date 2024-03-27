using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyLibrary.Behaviors
{
    public class TextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty MODEProperty =
          DependencyProperty.Register("MODE", typeof(byte), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public byte MODE
        {
            get { return (byte)GetValue(MODEProperty); }
            set { SetValue(MODEProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.PreviewTextInput += TextBox_PreviewTextInput;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= TextBox_PreviewTextInput;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!string.IsNullOrEmpty(e.Text))
            {
                if (textBox != null)
                {
                    // 禁用输入法，确保只能输入键盘上的按键，不会输入中文
                    InputMethod.Current.ImeState = InputMethodState.Off;
                    if (Regex.IsMatch(e.Text, @"^[0-9]*$"))
                    {
                        string inputText = textBox.Text + e.Text;

                        if (!string.IsNullOrEmpty(inputText))
                        {
                            uint number = (uint)Convert.ToUInt32(inputText);
                            if (MODE == 0)
                            {
                                bool inRange = false;

                                if (number >= 1 && number <= 9999)
                                {
                                    inRange = true;
                                }
                                else if (number >= 10001 && number <= 19999)
                                {
                                    inRange = true;
                                }
                                else if (number >= 30001 && number <= 39999)
                                {
                                    inRange = true;
                                }
                                else if (number >= 40000 && number <= 49999)
                                {
                                    inRange = true;
                                }
                                else if (number >= 400001 && number <= 465535)
                                {
                                    inRange = true;
                                }

                                if (inRange)
                                {

                                }
                                else
                                {
                                    e.Handled = true; // 阻止事件继续传播
                                }
                            }


                        }
                    }
                    else
                    {
                        e.Handled = true; // 阻止事件继续传播
                    }
                }
            }
            else
            { e.Handled = true; textBox.Text = string.Empty; }


        }
    }
}
