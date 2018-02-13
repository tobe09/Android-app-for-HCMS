package md58f84b4bb6e461d90a09e72aef4554c7d;


public class ItemViewHolder
	extends android.support.v7.widget.RecyclerView.ViewHolder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PeoplePlusMobile.ItemViewHolder, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ItemViewHolder.class, __md_methods);
	}


	public ItemViewHolder (android.view.View p0)
	{
		super (p0);
		if (getClass () == ItemViewHolder.class)
			mono.android.TypeManager.Activate ("PeoplePlusMobile.ItemViewHolder, PeoplePlusMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Views.View, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
	}

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
