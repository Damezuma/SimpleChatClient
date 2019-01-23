using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FoodChain
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MovePage : ContentPage
	{
        public App6.ChatListModel model;

		public MovePage ()
		{
            model = App.Current.Resources["chatListModels"] as App6.ChatListModel;
            InitializeComponent ();
		}

        public void Btn_moveMountain_Clicked(object sender, EventArgs e)
        {

            model.Room = App6.Room.Mountain;
            Navigation.PushModalAsync(new Mountain());
            
        }

        public void Btn_moveField_Clicked(object sender, EventArgs e)
        {
            model.Room = App6.Room.Field;
            Navigation.PushModalAsync(new Field());
        }

        public void Btn_moveRiver_Clicked(object sender, EventArgs e)
        {
            model.Room = App6.Room.River;
            Navigation.PushModalAsync(new River());
        }

        public void Btn_moveSky_Clicked(object sender, EventArgs e)
        {
            model.Room = App6.Room.Sky;
            Navigation.PushModalAsync(new Sky());
        }

    }
}