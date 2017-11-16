using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Cracker
{
    [ContentProperty("Child")]
    public partial class RowContainer : Grid
    {
        #region Field
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty ChildProperty;
        public static readonly DependencyProperty ShowStarProperty;
        private TextBlock m_TextBlockTitle;
        private ContentControl m_ContentControl;
        private TextBlock m_TextBlockStar;
        #endregion

        #region Constructor
        static RowContainer()
        {
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(RowContainer), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnTitleChanged)));
            ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(RowContainer), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnChildChanged)));
            ShowStarProperty = DependencyProperty.Register("ShowStar", typeof(bool), typeof(RowContainer), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnShowStarChanged)));
        }

        public RowContainer()
        {
            m_TextBlockTitle = new TextBlock() { Visibility = Visibility.Collapsed };
            m_ContentControl = new ContentControl() { Margin = new Thickness(6, 0, 6, 0) };
            m_TextBlockStar = new TextBlock() { Text = "*", Visibility = Visibility.Collapsed };

            Grid.SetColumn(m_TextBlockTitle, 0);
            Grid.SetColumn(m_ContentControl, 1);
            Grid.SetColumn(m_TextBlockStar, 2);
            
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            this.Children.Add(m_TextBlockTitle);
            this.Children.Add(m_ContentControl);
            this.Children.Add(m_TextBlockStar);
            this.MinHeight = 23;
            this.Margin = new Thickness(3);
        }
        #endregion

        #region Property
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public bool ShowStar
        {
            get { return (bool)GetValue(ShowStarProperty); }
            set { SetValue(ShowStarProperty, value); }
        }
        #endregion

        #region Method
        private void OnTitleChanged()
        {
            m_TextBlockTitle.Text = Title;
            if (string.IsNullOrEmpty(Title))
                m_TextBlockTitle.Visibility = Visibility.Collapsed;
            else
                m_TextBlockTitle.Visibility = Visibility.Visible;
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RowContainer dd = (RowContainer)d;
            dd.OnTitleChanged();
        }

        private void OnChildChanged()
        {
            m_ContentControl.Content = Child;
        }

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RowContainer dd = (RowContainer)d;
            dd.OnChildChanged();
        }

        private void OnShowStarChanged()
        {
            if (ShowStar)
                m_TextBlockStar.Visibility = Visibility.Visible;
            else
                m_TextBlockStar.Visibility = Visibility.Collapsed;
        }

        private static void OnShowStarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RowContainer dd = (RowContainer)d;
            dd.OnShowStarChanged();
        }
        #endregion
    }
}
