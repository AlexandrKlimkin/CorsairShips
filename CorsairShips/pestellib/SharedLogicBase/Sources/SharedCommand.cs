using System;

namespace PestelLib.SharedLogicBase
{
    public class SharedCommand : Attribute
    {
        public string Description { get; private set; }

        public SharedCommand() { }

        public SharedCommand(string description)
        {
            Description = description;
        }
    }
}