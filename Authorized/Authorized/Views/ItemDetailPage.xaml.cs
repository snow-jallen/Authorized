using Authorized.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Authorized.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}