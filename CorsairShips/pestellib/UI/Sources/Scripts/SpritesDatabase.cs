using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace PestelLib.UI
{ 
	public class SpritesDatabase : MonoBehaviour
	{
	    [SerializeField] private Sprite _defaultSprite;
	    [SerializeField] private SpriteAtlas[] _atlases;
	    [SerializeField] public List<Sprite> Sprites = new List<Sprite>();
	
	    private readonly Dictionary<string, Sprite> _spritesDict = new Dictionary<string, Sprite>();

	    private bool _initDone = false;
	
	    public Sprite GetSprite(string n)
	    {
	        if (!_initDone)
	        {
	            Init();
	        }
		    
		    for (var i = 0; i < _atlases.Length; i++)
		    {
			    if(_atlases[i].GetSprite(n) == null) continue;

			    return _atlases[i].GetSprite(n);
		    }

	        if (_spritesDict.ContainsKey(n))
	        {
	            return _spritesDict[n];
	        }

            var lower = n.ToLower();
            if (_spritesDict.ContainsKey(lower))
            {
                return _spritesDict[lower];
            }

            Debug.LogErrorFormat("Can't find sprite with name: '{0}'", n);
	        return _defaultSprite;
	    }
	
	    private void Init()
	    {
	        for (var i = 0; i < Sprites.Count; i++)
	        {
                if (Sprites[i] == null) continue;

                _spritesDict[Sprites[i].name] = Sprites[i];
                var lower = Sprites[i].name.ToLower();
                if (lower != Sprites[i].name) {
                    _spritesDict[lower] = Sprites[i];
                }
	        }
	        _initDone = true;
        }
	}
}