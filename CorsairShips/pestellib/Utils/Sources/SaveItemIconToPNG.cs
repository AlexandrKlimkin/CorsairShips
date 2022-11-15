using System.Collections;
using UnityEngine;

namespace PestelLib.Utils
{
    /*
     * Использование:
     * Положить в пустую сцену все объекты, для которых нужна иконка
     * Добавить их ссылки в _models
     * Настроить свет
     * Настроить расположение камеры
     * У камеры поставить ClearFlags = Don't clear
     * Запустить PlayMode
     * В папке Application.persistentDataPath появятся иконки в PNG с альфа каналом для каждого объекта
     */
    public class SaveItemIconToPNG : MonoBehaviour
    {
        [SerializeField] private GameObject[] _models;

        [SerializeField] private int _width = 512;
        [SerializeField] private int _height = 400;

        private IEnumerator Start()
        {
            for (var i = 0; i < _models.Length; i++)
            {
                HideAllModels();

                _models[i].SetActive(true);

                yield return null;
                MakeSquarePngFromOurVirtualThingy(_width, _height, _models[i].name);
            }
        }

        public void MakeSquarePngFromOurVirtualThingy(int w, int h, string modelName)
        {
            // capture the virtuCam and save it as a square PNG.

            //int sqr = 512;

            //Camera.main.aspect = 1.0f;
            // recall that the height is now the "actual" size from now on
            // the .aspect property is very tricky in Unity, and bizarrely is NOT shown in the editor
            // the editor will still incorrectly show the frustrum being screen-shaped

            RenderTexture tempRT = new RenderTexture(w, h, 24);
            // the "24" can be 0,16,24 or formats like RenderTextureFormat.Default, ARGB32 etc.

            Camera.main.targetTexture = tempRT;
            Camera.main.Render();

            RenderTexture.active = tempRT;
            Texture2D virtualPhoto = new Texture2D(w, h, TextureFormat.ARGB32, false);
            // false, meaning no need for mipmaps
            virtualPhoto.ReadPixels(new Rect(0, 0, w, h), 0, 0); // you get the center section

            RenderTexture.active = null; // "just in case" 
            Camera.main.targetTexture = null;
            //////Destroy(tempRT); - tricky on android and other platforms, take care

            byte[] bytes;
            bytes = virtualPhoto.EncodeToPNG();

            System.IO.File.WriteAllBytes(OurTempSquareImageLocation(modelName), bytes);
            // virtualCam.SetActive(false); ... not necesssary but take care

            // now use the image somehow...
            //YourOngoingRoutine(OurTempSquareImageLocation());
        }
        private string OurTempSquareImageLocation(string modelName)
        {
            string r = Application.persistentDataPath + string.Format("/{0}.png", modelName);
            return r;
        }

        private void HideAllModels()
        {
            for (var i = 0; i < _models.Length; i++)
            {
                _models[i].SetActive(false);
            }
        }
    }
}