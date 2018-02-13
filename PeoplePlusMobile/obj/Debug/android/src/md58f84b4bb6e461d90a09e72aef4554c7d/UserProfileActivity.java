package md58f84b4bb6e461d90a09e72aef4554c7d;


public class UserProfileActivity
	extends md58f84b4bb6e461d90a09e72aef4554c7d.BaseActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onBackPressed:()V:GetOnBackPressedHandler\n" +
			"";
		mono.android.Runtime.register ("PeoplePlusMobile.UserProfileActivity, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", UserProfileActivity.class, __md_methods);
	}


	public UserProfileActivity ()
	{
		super ();
		if (getClass () == UserProfileActivity.class)
			mono.android.TypeManager.Activate ("PeoplePlusMobile.UserProfileActivity, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onBackPressed ()
	{
		n_onBackPressed ();
	}

	private native void n_onBackPressed ();

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
