package md58f84b4bb6e461d90a09e72aef4554c7d;


public class MyDialogFragment
	extends android.app.DialogFragment
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onCreateView:(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;:GetOnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_Handler\n" +
			"n_onActivityResult:(IILandroid/content/Intent;)V:GetOnActivityResult_IILandroid_content_Intent_Handler\n" +
			"";
		mono.android.Runtime.register ("PeoplePlusMobile.MyDialogFragment, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MyDialogFragment.class, __md_methods);
	}


	public MyDialogFragment ()
	{
		super ();
		if (getClass () == MyDialogFragment.class)
			mono.android.TypeManager.Activate ("PeoplePlusMobile.MyDialogFragment, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public MyDialogFragment (md58f84b4bb6e461d90a09e72aef4554c7d.BaseActivity p0)
	{
		super ();
		if (getClass () == MyDialogFragment.class)
			mono.android.TypeManager.Activate ("PeoplePlusMobile.MyDialogFragment, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "PeoplePlusMobile.BaseActivity, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public android.view.View onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2)
	{
		return n_onCreateView (p0, p1, p2);
	}

	private native android.view.View n_onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2);


	public void onActivityResult (int p0, int p1, android.content.Intent p2)
	{
		n_onActivityResult (p0, p1, p2);
	}

	private native void n_onActivityResult (int p0, int p1, android.content.Intent p2);

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
