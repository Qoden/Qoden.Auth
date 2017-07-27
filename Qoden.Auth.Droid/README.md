<code>
//First of all we need application and OAuth LoginPage
[Application]
public class MyApplication : Application
{
    public BrowserLoginPage LoginPage { get; private set; }

    public static MyApplication Instance { get; private set; }

    public override void OnCreate()
    {        
        base.OnCreate();
        Instance = this;
        //Create browser login page. This page opens system browser to perform login.
        LoginPage = new Qoden.Auth.BrowserLoginPage(this);
    }
}

//Next step - let check user authorization from app business logic 
public class MyAppService
{
    async void DoSomeWork()
    {
        try
        {
//Make sure we have valid credentials, refresh token or run login flow if required.
//Custodian confgirued to work with MyApplication.Instance.LoginPage
            await Custodian.Authenticate();
            var idToken = user[OAuthApi.IdToken] as string;
            SendAuthenticatedRequest(idToken);
        }
        catch (OAuthException e)
        {
//OAuth server replied with error 
            Console.WriteLine(e);
        }
        catch (OperationCancelledException e)
        {
//User canceled login process
            Console.WriteLine(e);
        }
    }
}

//Last piece of configuration - pass authentication results from browser to LoginPage

//Main activity has to have LaunchMode = SingleTop otherwise Android will create new activity instance 
//when browser receive OAuth redirect. In this case old activity never knows what happened.
[Activity(MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
//Intent filters instruct Android to launch this activity when browser receive OAuth redirect
//to app's custom URL schema
[IntentFilter(
    new[] { Android.Content.Intent.ActionView },
    DataScheme = "my.url.scheme",
    Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable }
)]
public class MainActivity : QodenActivity<MainView>
{
//OnNewIntent is called to handle OAuth redirect
    protected override void OnNewIntent(Android.Content.Intent intent)
    {
        base.OnNewIntent(intent);
        if (intent.Data != null)
        {
            try
            {
                var uri = new Uri(intent.Data.ToString());
//Call LoginPage.OnOpenUrl to finish pending login process
                MyApplication.Instance.LoginPage.OnOpenUrl(uri);
            }
            catch
            {
            }
        }
    }

//OnResume called after OnNewIntent to resume activity
    protected override void OnResume()
    {
        base.OnResume();
//Call LoginPage.OnAppActivated to let it know that login flow is finished.
//If OnOpenUrl was not called then this cancels pending login.
        MyApplication.Instance.LoginPage.OnAppActivated();
    }
}
</code>