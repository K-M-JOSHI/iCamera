
using Android.App;
using Android.Content;
using Android.OS;
using System.Threading;
using System.Threading.Tasks;

namespace iCamera.Droid
{
    [Activity(Label = "iCamera", MainLauncher = true, Theme = "@style/Theme.AppCompat.Light.NoActionBar", NoHistory = true, Icon = "@drawable/icon")]
    public class SplashScreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SplashScreen);
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();

        }

        async void SimulateStartup()
        {
            await Task.Delay(4000); 
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}