using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        // Properties
        private int m_PageNumber = 0;
        public string PageNumber
        {
            get => m_PageNumber.ToString();
            set
            {
                if (m_PageNumber.ToString() == value)
                    return;

                m_PageNumber = Int32.Parse(value);
                OnPropertyChanged(nameof(PageNumber));
            }
        }
        
        private int m_PageSize = 1 << 16;
        public string PageSize
        {
            get => m_PageSize.ToString();
            set
            {
                if (m_PageSize.ToString() == value)
                    return;

                m_PageSize = Int32.Parse(value);
                OnPropertyChanged(nameof(PageSize));
            }
        }

        private bool m_TextWrappingCheckBox = false;
        public bool TextWrappingCheckBox
        {
            get => m_TextWrappingCheckBox;
            set
            {
                m_TextWrappingCheckBox = value;
                OnPropertyChanged(nameof(TextWrappingCheckBox));
                OnPropertyChanged(nameof(TextWrapping));
                OnPropertyChanged(nameof(HorizontalScroll));
            }
        }

        public string TextWrapping => m_TextWrappingCheckBox ? "Wrap" : "WrapWithOverflow";

        public string HorizontalScroll => m_TextWrappingCheckBox ? "Disabled" : "Visible";
        
        private string m_TextContent = String.Empty;
        public string TextContent
        {
            get => m_TextContent;
            set
            {
                if (m_TextContent == value)
                    return;

                m_TextContent = value;
                OnPropertyChanged(nameof(TextContent));
            }
        }
        
        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            myDockPanel.DataContext = this;
        }

        // Click Methods
        private void OnClickOpen(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                this.Title = openFileDialog.FileName;
                LoadText();
            }
        }

        private void OnClickReload(object sender, RoutedEventArgs e)
        {
            LoadText();
        }

        private void OnClickPageDown(object sender, RoutedEventArgs e)
        {
            if (m_PageNumber <= 0)
                return;
            
            --m_PageNumber;
            LoadText();
            OnPropertyChanged(nameof(PageNumber));
        }

        private void OnClickPageUp(object sender, RoutedEventArgs e)
        {
            ++m_PageNumber;
            LoadText();
            OnPropertyChanged(nameof(PageNumber));
        }

        // Callbacks
        private void UIElement_OnPreviewPageInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
            LoadText();
        }

        // Private Methods
        private void LoadText()
        {
            if (!File.Exists(Title))
                return;
            
            using (var fileStream = new FileStream(this.Title, FileMode.Open, FileAccess.Read))
            {
                var fileSize = fileStream.Seek(0, SeekOrigin.End);
                var currentPointer = m_PageNumber * m_PageSize;
                
                if (currentPointer > fileSize)
                {
                    
                    TextContent = "<END OF FILE>";
                    return;
                }

                fileStream.Seek(currentPointer, SeekOrigin.Begin);
                var buffer = new byte[m_PageSize];
                fileStream.Read(buffer, 0, m_PageSize);

                var test = Encoding.UTF8.GetString(buffer);

                TextContent = test;
            }
        }
    }
}