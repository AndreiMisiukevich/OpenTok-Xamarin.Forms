using System;

namespace Xamarin.Forms.OpenTok.Service
{
    [Flags]
    public enum OpenTokPermission
    {
        None = 0,
        Camera = 1,
        WriteExternalStorage = 2,
        RecordAudio = 4,
        ModifyAudioSettings = 8,
        All = Camera | WriteExternalStorage | RecordAudio | ModifyAudioSettings
    }
}
