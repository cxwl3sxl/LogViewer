using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogViewer
{
    public class LinkButton : Button
    {
        static LinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkButton),
                new FrameworkPropertyMetadata(typeof(LinkButton)));
        }

        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register(nameof(HighlightBrush), typeof(Brush), typeof(LinkButton));

        public Brush HighlightBrush
        {
            get => (Brush)GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }
    }
}
