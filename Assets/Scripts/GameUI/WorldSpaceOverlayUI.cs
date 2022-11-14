using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// http://answers.unity.com/answers/1428061/view.html
public class WorldSpaceOverlayUI : MonoBehaviour
{
    private const string ShaderTestMode = "unity_GUIZTestMode"; //The magic property we need to set

    [ SerializeField ]
    CompareFunction desiredUIComparison = CompareFunction.Always; //If you want to try out other effects

    [ Tooltip( "Set to blank to automatically populate from the child UI elements" ) ]
    [ SerializeField ] Graphic[] uiElementsToApplyTo;

    //Allows us to reuse materials
    private readonly Dictionary<Material, Material> _materialMappings = new();
    static readonly int UnityGuizTestMode = Shader.PropertyToID( ShaderTestMode );

    // Invoke with delay to give a moment for the health bar notches to populate
    protected virtual void Start() => Invoke( nameof( MakeGraphicsOverlay ), 1f );

    private void MakeGraphicsOverlay()
    {
        if( uiElementsToApplyTo.Length == 0 )
            uiElementsToApplyTo = gameObject.GetComponentsInChildren<Graphic>();
        
        foreach( var graphic in uiElementsToApplyTo )
        {
            if( graphic == null )   continue;
            var material = graphic.materialForRendering;
            if( material == null )
            {
                Debug.LogError(
                    $"{nameof( WorldSpaceOverlayUI )}: skipping target without material {graphic.name}.{graphic.GetType().Name}" );
                continue;
            }

            if( !_materialMappings.TryGetValue( material, out Material materialCopy ) )
            {
                materialCopy = new Material( material );
                _materialMappings.Add( material, materialCopy );
            }

            materialCopy.SetInt( UnityGuizTestMode, (int) desiredUIComparison );
            graphic.material = materialCopy;
        }
    }

    public void ReRun() => Invoke( nameof( MakeGraphicsOverlay ), 0.25f );
}