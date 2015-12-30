using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance { get; private set; }

    private static AudioClip LastPlayClip_ = null;

    private static float LastPlayClipTime_ = 0;

    // 背景音
    private bool PlayMusic_ = true;

    // 音效
    private bool PlaySound_ = true;

    public bool PlayMusic {
        get {
            //_PlayMusic = PlayerSys.GetCustomSetting ( "Music" );
            return PlayMusic_;
        }
        set {
            PlayMusic_ = value;
            //PlayerSys.SetCustomSetting ( "Music", _PlayMusic ? "1" : "0" );
            ModifyMusicVolume ( PlayMusic_ ? 1 : 0 );
        }
    }

    public bool PlaySound {
        get {
            return PlaySound_;
        }
        set {
            PlaySound_ = value;
        }
    }

    private void Awake () {
        Instance = this;
    }

#region private member func

    private void InitListener () {
        GameObject go = CameraManager.Instance.MainCamera;
        if ( go.GetComponent<AudioListener> () == null )
            go.AddComponent<AudioListener> ();
    }

    private void InitAudioSource( SceneManager.SceneType sceneType ) {
        string path = string.Empty;
        if( sceneType == SceneManager.SceneType.Login ) {
            path = "res/audio/login.audio";
        }
        else if( sceneType == SceneManager.SceneType.Deploy ) {
            path = "res/audio/battle.audio";
        }
        if ( string.IsNullOrEmpty ( path ) )
            return;
        Object res = AssetLoader.GetAsset( path );
        InitAudioClip( res );
    }

    private void InitAudioClip ( Object clip ) {
        GameObject go = CameraManager.Instance.MainCamera;
        AudioSource Source = go.GetComponent<AudioSource> ();
        if ( Source == null )
            Source = go.AddComponent<AudioSource> ();
        if ( clip != null ) {
            Source.clip = clip as AudioClip;
            Source.loop = true;
            Source.volume = PlayMusic ? 1 : 0;
            Source.Play ();
        }
    }

    private IEnumerator ModifyVolume ( AudioSource source, float target ) {
        if ( source == null ) yield break;
        if( source.volume == target ) yield break;
        float nowTime = 0f;
        float endTime = 1f;
        bool bAddVolume = source.volume < target;        
        yield return null;
        while ( nowTime < endTime ) {
            if ( source == null ) yield break;
            float temp = Time.deltaTime;
            float modify = 0;
            if ( bAddVolume )
                modify = ( source.volume < ( temp / endTime ) ? ( temp / endTime ) : source.volume );
            else
                modify = 0 - ( source.volume > ( temp / endTime ) ? ( temp / endTime ) : source.volume );

            source.volume += modify;

            if ( bAddVolume )
                source.volume = source.volume > target ? target : source.volume;
            else
                source.volume = source.volume < target ? target : source.volume;

            nowTime += temp;
            nowTime = nowTime > endTime ? endTime : nowTime;
            yield return null;
        }
    }

#endregion

#region interface

    public static void InitMusic( SceneManager.SceneType sceneType ) {
        if( Instance == null ) return;
        Instance.InitListener();
        Instance.InitAudioSource( sceneType );
    }

    public void ModifyMusicVolume ( float target ) {
        GameObject go = CameraManager.Instance.MainCamera;
        AudioSource source = go.GetComponent<AudioSource> ();
        StartCoroutine ( ModifyVolume ( source, target ) );
    }

    public static void PlaySoundByName(string soundName, Vector3 position, bool threeD) {
        string path = string.Format ( "res/audio/music/{0}.audio", soundName );
        PlaySoundByPath(path, position, threeD);
    }

    public static void PlaySoundByPath(string path, Vector3 position, bool threeD) {
        Object res = AssetLoader.GetAsset ( path );
        if ( !res ) return;
        PlaySoundByClip(res as AudioClip, position, threeD);
    }

    public static void PlaySoundByClip ( AudioClip clip, Vector3 position, bool threeD = false ) {
        if ( LastPlayClip_ == clip && ( Time.time - LastPlayClipTime_ ) < ( clip.length / 2 ) ) {
            return;
        }
        LastPlayClip_ = clip;
        LastPlayClipTime_ = Time.time;
        if ( !Instance.PlaySound ) return;

        //For Unity 4.x
        if( !threeD )
            AudioSource.PlayClipAtPoint( clip, position );
        else {
            //For Unity 5.x
            GameObject attach = new GameObject( "__audio_" + clip.name );
            attach.transform.position = position;
            AudioSource source = attach.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = threeD ? 1f : 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.maxDistance = 320;//如果区域变化大，这个属性是需要动态调整的
            source.Play();
            Instance.StartCoroutine( DestroyAudioGameobject( attach, clip.length ) );
        }
    }

    private static IEnumerator DestroyAudioGameobject(GameObject attach, float duration) {
        yield return new WaitForSeconds(duration);
        Destroy(attach);
        //Debugger.Log("DestroyAudioGameobject()..." + attach.name + " duration : " + duration);
    }

#endregion    
}