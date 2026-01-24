using UnityEngine;
using UnityEngine.Video;

public class TimelineVideoControl : MonoBehaviour
{
    [SerializeField] private VideoPlayer vp;
    [SerializeField] private bool freezeOnFirstFrameOnStart = true;

    private void Reset()
    {
        vp = GetComponent<VideoPlayer>();
    }

    private void Awake()
    {
        if (vp == null) vp = GetComponent<VideoPlayer>();
        if (vp == null) { Debug.LogError("VideoPlayer mancante"); return; }

        // Impostazioni consigliate
        vp.playOnAwake = false;
        vp.waitForFirstFrame = true;

        if (freezeOnFirstFrameOnStart)
            PrepareAndFreezeFirstFrame();
    }

    public void PrepareAndFreezeFirstFrame()
    {
        if (vp == null) return;

        // Evita doppie subscription
        vp.prepareCompleted -= OnPreparedFreeze;
        vp.prepareCompleted += OnPreparedFreeze;

        vp.Prepare();
    }

    private void OnPreparedFreeze(VideoPlayer source)
    {
        source.prepareCompleted -= OnPreparedFreeze;

        // Porta a frame 0 e ferma: resta visibile sulla RenderTexture
        source.frame = 0;
        source.Pause();
    }

    public void Play()
    {
        if (vp == null) return;

        // Se non è pronto, preparalo e poi play appena pronto
        if (!vp.isPrepared)
        {
            vp.prepareCompleted -= OnPreparedPlay;
            vp.prepareCompleted += OnPreparedPlay;
            vp.Prepare();
            return;
        }

        vp.Play();
    }

    private void OnPreparedPlay(VideoPlayer source)
    {
        source.prepareCompleted -= OnPreparedPlay;
        source.Play();
    }

    public void StopAndFreezeFirstFrame()
    {
        if (vp == null) return;

        vp.Stop();
        // dopo Stop spesso frame torna “non valido” finché non prepari:
        PrepareAndFreezeFirstFrame();
    }
}
