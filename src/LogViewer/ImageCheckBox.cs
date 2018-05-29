using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogViewer
{
    public class ImageCheckBox : CheckBox
    {
        public static readonly DependencyProperty BorderStyleProperty =
            DependencyProperty.Register(nameof(BorderStyle), typeof(Style), typeof(ImageCheckBox));
        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register(nameof(CheckedImage), typeof(ImageSource), typeof(ImageCheckBox));
        public static readonly DependencyProperty UnCheckedImageProperty =
            DependencyProperty.Register(nameof(UnCheckedImage), typeof(ImageSource), typeof(ImageCheckBox));
        public static readonly DependencyProperty CheckedToolTipProperty =
            DependencyProperty.Register(nameof(CheckedToolTip), typeof(string), typeof(ImageCheckBox));
        public static readonly DependencyProperty UnCheckedToolTipProperty =
            DependencyProperty.Register(nameof(UnCheckedToolTip), typeof(string), typeof(ImageCheckBox));
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(nameof(ImageWidth), typeof(double), typeof(ImageCheckBox));

        static ImageCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageCheckBox),
                new FrameworkPropertyMetadata(typeof(ImageCheckBox)));
        }

        public Style BorderStyle
        {
            get => (Style)GetValue(BorderStyleProperty);
            set => SetValue(BorderStyleProperty, value);
        }

        public ImageSource CheckedImage
        {
            get => (ImageSource)GetValue(CheckedImageProperty);
            set => SetValue(CheckedImageProperty, value);
        }

        public ImageSource UnCheckedImage
        {
            get => (ImageSource)GetValue(UnCheckedImageProperty);
            set => SetValue(UnCheckedImageProperty, value);
        }

        public string UnCheckedToolTip
        {
            get => (string)GetValue(UnCheckedToolTipProperty);
            set => SetValue(UnCheckedToolTipProperty, value);
        }

        public string CheckedToolTip
        {
            get => (string)GetValue(CheckedToolTipProperty);
            set => SetValue(CheckedToolTipProperty, value);
        }

        public double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }
    }
}
