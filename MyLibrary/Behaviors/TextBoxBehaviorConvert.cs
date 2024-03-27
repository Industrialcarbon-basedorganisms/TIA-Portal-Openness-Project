using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyLibrary.Behaviors
{
    public class TextBoxBehaviorConvert : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewTextInput += TextBox_PreviewTextInput;
            AssociatedObject.TextChanged += TextBox_TextChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= TextBox_PreviewTextInput;
            AssociatedObject.TextChanged -= TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            // 禁用输入法，确保只能输入键盘上的按键，不会输入中文
            InputMethod.Current.ImeState = InputMethodState.Off;

            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                // 尝试解析输入文本为 16进制
                if (Regex.IsMatch(e.Text, @"^[0-9A-Fa-f]+$") && textBox.Text.Length < 16)
                {

                }
                else
                {
                    // 输入不是有效的 Byte 值，阻止事件传播
                    e.Handled = true;
                }
            }
        }

    }
}

