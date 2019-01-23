using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FoodChain
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }


        public void Btn_nextPage_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MovePage());
        }

        
    }
}
