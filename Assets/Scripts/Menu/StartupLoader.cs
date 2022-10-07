using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class StartupLoader : MonoBehaviour
    {
        [ SerializeField ] Image loadingBar;

        void Start() => StartCoroutine( LoadMenu() );

        IEnumerator LoadMenu()
        {
            var async = SceneManager.LoadSceneAsync( "Menu" );
            while( !async.isDone )
            {
                SetLoadingBarProgress( async.progress );
                yield return null;
            }
        }

        void SetLoadingBarProgress( float progress ) =>
            loadingBar.rectTransform.localScale = new Vector3( progress * 10f / 9f, 1f, 1f );
    }
}