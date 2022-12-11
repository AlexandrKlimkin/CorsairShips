namespace UI.Screens.ShipSelection {
    public struct ShipSelectedSignal {
        public string SelectedId;
        public ShipSelectedSignal(string selectedId) {
            SelectedId = selectedId;
        }
    }
}