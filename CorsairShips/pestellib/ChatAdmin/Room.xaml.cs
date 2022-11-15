using System;
using System.Windows.Controls;

namespace ChatAdmin
{
    public static class TextExtensions
    {
        public static bool IsScrolledToEnd(this TextBox textBox)
        {
            return Math.Abs(textBox.VerticalOffset + textBox.ViewportHeight - textBox.ExtentHeight) < 0.001;
        }
    }

    /// <summary>
    /// Interaction logic for Room.xaml
    /// </summary>
    public partial class Room : UserControl
    {
        
        public Room()
        {
            InitializeComponent();
        }
    }
}
