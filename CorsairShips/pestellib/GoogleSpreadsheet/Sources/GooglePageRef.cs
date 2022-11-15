using System;

namespace PestelLib.Serialization
{ 
	public class GooglePageRefAttribute : Attribute
	{
	    public string PageName;

	    public GooglePageRefAttribute(string pageName)
	    {
	        PageName = pageName;
	    }
	}
}