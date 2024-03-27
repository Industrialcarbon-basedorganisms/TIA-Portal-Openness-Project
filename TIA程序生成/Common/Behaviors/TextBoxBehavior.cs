using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace TIA程序生成.Common.Behaviors
{
    public class TextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty BaseProperty =
            DependencyProperty.Register("Base", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public string Base
        {
            get { return (string)GetValue(BaseProperty); }
            set { SetValue(BaseProperty, value); }
        }

        public static readonly DependencyProperty HEXProperty =
            DependencyProperty.Register("HEX", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public string HEX
        {
            get { return (string)GetValue(HEXProperty); }
            set { SetValue(HEXProperty, value); }
        }

        public static readonly DependencyProperty DECProperty =
            DependencyProperty.Register("DEC", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public string DEC
        {
            get { return (string)GetValue(DECProperty); }
            set { SetValue(DECProperty, value); }
        }

        public static readonly DependencyProperty OCTProperty =
           DependencyProperty.Register("OCT", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public string OCT
        {
            get { return (string)GetValue(OCTProperty); }
            set { SetValue(OCTProperty, value); }
        }

        public static readonly DependencyProperty BINProperty =
          DependencyProperty.Register("BIN", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public string BIN
        {
            get { return (string)GetValue(BINProperty); }
            set { SetValue(BINProperty, value); }
        }

        public static readonly DependencyProperty FloatProperty =
          DependencyProperty.Register("Float", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null));

        public string Float
        {
            get { return (string)GetValue(FloatProperty); }
            set { SetValue(FloatProperty, value); }
        }

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
                switch (Base)
                {
                    case "HEX":
                        // 尝试解析输入文本为 16进制
                        if (Regex.IsMatch(e.Text, @"^[0-9A-Fa-f]+$") && textBox.Text.Length < 16)
                        {

                        }
                        else
                        {
                            // 输入不是有效的 Byte 值，阻止事件传播
                            e.Handled = true;
                        }

                        break;
                    case "DEC":
                        // 尝试解析输入文本为 10进制
                        if (Regex.IsMatch(e.Text, @"^[0-9]+$") && textBox.Text.Length < 19)
                        {
                            // 输入是有效的 Byte 值
                        }
                        else
                        {
                            // 输入不是有效的 Byte 值，阻止事件传播
                            e.Handled = true;
                        }
                        break;
                    case "OCT":
                        // 尝试解析输入文本为 8进制
                        if (Regex.IsMatch(e.Text, @"^[0-7]+$") && textBox.Text.Length < 22)
                        {
                            // 输入是有效的 Byte 值
                        }
                        else
                        {
                            // 输入不是有效的 Byte 值，阻止事件传播
                            e.Handled = true;
                        }
                        break;
                    case "BIN":
                        // 尝试解析输入文本为 10进制
                        if (Regex.IsMatch(e.Text, @"^[0-1]+$") && textBox.Text.Length < 64)
                        {
                            // 输入是有效的 Byte 值
                        }
                        else
                        {
                            // 输入不是有效的 Byte 值，阻止事件传播
                            e.Handled = true;
                        }
                        break;
                    case "Float":
                        if (Regex.IsMatch(e.Text, @"^[0-9-.]*$"))
                        {
                            string inputText = textBox.Text + e.Text;
                            // 允许输入负号作为第一个字符
                            if (inputText == "-" && textBox.SelectionStart == 0) { return; }
                            // 尝试解析输入文本为 Real
                            if (float.TryParse(inputText, out float resultReal)) { }
                            else { e.Handled = true; }

                        }
                        else { e.Handled = true; }
                        break;
                }
            }
            else { e.Handled = true; }
        }
    }
}
