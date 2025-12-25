using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CGLCMIV2App
{
    /// <summary>
    /// MessageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageWindow : Window , IMessageWindow
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        public void Show(string message, Window owner)
        {
            Owner = owner;
            txtMessage.Text = message;
            Show();
        }
    }

    public interface IMessageWindow
    {
        void Show(string message, Window owner);
        void Close();
    }
}
