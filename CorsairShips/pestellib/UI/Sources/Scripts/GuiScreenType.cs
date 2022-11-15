using System;

namespace PestelLib.UI
{
    [System.Serializable]
    public class GuiScreenType : IComparable<GuiScreenType>
    {
        public const int Step = 100;

        public readonly int Order;
        public readonly string Name;
        public readonly bool AllowDuplicates;
        public bool Stackable;

        public GuiScreenType(int order, string name, bool allowDuplicates = true, bool stackable = false)
        {
            Order = order;
            Name = name;
            AllowDuplicates = allowDuplicates;
            Stackable = stackable;
        }

        public static readonly GuiScreenType Background = new GuiScreenType(1 * Step, "Background");
        public static readonly GuiScreenType Screen = new GuiScreenType(2 * Step, "Screen", false, true);
        public static readonly GuiScreenType InBattleScreen = new GuiScreenType(3 * Step, "InBattleScreen");
        public static readonly GuiScreenType Permanent = new GuiScreenType(4 * Step, "Permanent");
        public static readonly GuiScreenType Overlay = new GuiScreenType(5 * Step, "Overlay", false);
        public static readonly GuiScreenType Loading = new GuiScreenType(6 * Step, "Loading", false);
        public static readonly GuiScreenType Dialog = new GuiScreenType(7 * Step, "Dialog");
        public static readonly GuiScreenType Achievement = new GuiScreenType(8 * Step, "Achievement");
        public static readonly GuiScreenType ConnectionStatusOverlay = new GuiScreenType(9 * Step, "ConnectionStatusOverlay");
        public static readonly GuiScreenType ServerMessageOverlay = new GuiScreenType(10 * Step, "ServerMessageOverlay");
        public static readonly GuiScreenType Tooltip = new GuiScreenType(11 * Step, "Tooltip", false);

        public int CompareTo(GuiScreenType other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Order.CompareTo(other.Order);
        }
    }
}
