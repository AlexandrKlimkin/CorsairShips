using UnityEngine;

namespace PestelLib.UI
{
    public interface IGui 
    {
        T Show<T>() where T : Component;
        T Show<T>(GuiScreenType type, bool preload = false) where T : Component;
        T Get<T>() where T : MonoBehaviour;
        void Close<T>() where T : MonoBehaviour;
        void Close(GameObject screen);
        void Hide<T>(bool preload = false) where T : MonoBehaviour;
        void Hide(GameObject screen, bool preload = false);
        Component TopDialog { get; }
        Component TopScreen { get; }
        bool AnyVisibleDialog { get; }
        void GoBack();
    }
}