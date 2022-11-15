using System.Reflection;

namespace PestelLib.GoogleSpreadsheet.Editor
{
    internal struct ImportPageData
    {
        internal readonly string PageName;
        internal readonly object Target;
        internal readonly string PageUri;
        internal readonly FieldInfo DefsFieldInfo;

        internal ImportPageData(string pageUri, string pageName, object target, FieldInfo defsFieldInfo)
        {
            PageName = pageName;
            Target = target;
            PageUri = pageUri;
            DefsFieldInfo = defsFieldInfo;
        }
    }
}