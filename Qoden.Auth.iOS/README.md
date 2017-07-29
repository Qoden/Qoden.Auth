# Configuration

* Enable Entitlements.plist for Simulator in Debug mode. 
1. Open project settings -> iOS Bundle Signing
1. Select 'Configuration' : 'Debug', 'Platform': 'iPhoneSimulator'
1. Make sure 'Custom Entitlement' = 'Entitlements.plist'
* Enable keychain
1. Open Entitlements.plist in IDE
1. Select 'Enable Keychain' checkbox
* Add custom URL schema
1. Open Info.plist
1. Go to 'Advanced' tab (at the bottom of editor)
1. Add URL type with you app schema

# Integrate Login Page

<code>
[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow Window { get; set; }
//This login page displays Login UI inside embedded Safar browser.
    public Qoden.Auth.iOS.EmbeddedSafariLoginPage LoginPage { get; private set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Window = Window ?? new UIWindow();
        Window.RootViewController = new MainController();
        Window.MakeKeyAndVisible();
//Create embedded login page.
        LoginPage = new Qoden.Auth.iOS.EmbeddedSafariLoginPage(Window.RootViewController);
        return true;
    }

    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
//Let Login Page know that user redirected back to application. 
//This should happen before call to UserActivatedApplication since
//UserActivatedApplication cancel login flow.
        LoginPage.UserOpenedUrl(url);
        return true;
    }

    public override void OnActivated(UIApplication uiApplication)
    {
//Let LoginPage know that login flow has finished.
//If UserOpenedUrl was not called then this cancels pending login.
        LoginPage.UserActivatedApplication();
    }

    public override void OnResignActivation(UIApplication application)
    {
//Let LoginPage know than user has left our app. LoginPage maintain internal 
//flag and does not finish login flow if UserActivatedApplication called before 
//UserHasLeftApplication
        LoginPage.UserHasLeftApplication();
    }
}
</code>