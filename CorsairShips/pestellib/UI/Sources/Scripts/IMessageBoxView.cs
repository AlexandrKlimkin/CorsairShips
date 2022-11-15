namespace PestelLib.UI
{
	public interface IMessageBoxView
	{
        string Caption { set; }
        string Description { set; }
        string Icon { set; }
        string ButtonA { set; }
        string ButtonB { set; }
        bool CantClose { get; set; }
        void UpdateView();
        void ShowDescription(bool show = true);
	}

}