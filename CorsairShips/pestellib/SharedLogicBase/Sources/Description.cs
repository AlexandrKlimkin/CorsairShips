using System;

namespace PestelLib.SharedLogicBase
{
    public class Description : Attribute
    {
        public string Text { get; private set; }

        public Description(string text)
        {
            Text = text;
        }
    }
}
