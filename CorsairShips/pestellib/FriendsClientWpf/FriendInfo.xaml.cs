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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FriendsClient;
using FriendsClient.Sources;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for FriendInfo.xaml
    /// </summary>
    public partial class FriendInfo : UserControl
    {
        public static readonly  DependencyProperty FriendBaseDependencyProperty = DependencyProperty.Register("FriendContext", typeof(IFriendContext), typeof(FriendInfo));

        public IFriendContext FriendContext
        {
            get { return (IFriendContext)GetValue(FriendBaseDependencyProperty); }
            set
            {
                SetValue(FriendBaseDependencyProperty, value);
                UpdateBindings();
            }
        }

        private void UpdateBindings()
        {
            lblId.GetBindingExpression(Label.ContentProperty)?.UpdateTarget();
            lblNick.GetBindingExpression(Label.ContentProperty)?.UpdateTarget();
            lblLevel.GetBindingExpression(Label.ContentProperty)?.UpdateTarget();
            lblStatus.GetBindingExpression(Label.ContentProperty)?.UpdateTarget();
            lblNextGift.GetBindingExpression(Label.ContentProperty)?.UpdateTarget();
            lblLastState.GetBindingExpression(Label.ContentProperty)?.UpdateTarget();
        }

        private FriendBase friend {
            get
            {
                if (FriendContext != null)
                    return FriendContext.FriendInfo;
                return null;
            }
        }
        public string Id => FriendContext.FriendInfo.Id;
        public string Status => Helper.GetFriendStatusString(friend);
        public string LastStatus => friend.LastStatus.ToLocalTime().ToString();
        public string NextGift => friend.NextGift.ToLocalTime().ToString();
        public string Level => friend.Profile.Level.ToString();
        public string Nick => friend.Profile.Nick;

        public FriendInfo()
        {
            InitializeComponent();
        }
    }
}
