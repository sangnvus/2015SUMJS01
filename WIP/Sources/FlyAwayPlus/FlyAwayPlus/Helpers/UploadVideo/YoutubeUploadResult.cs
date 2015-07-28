using Google.Apis.Upload;

namespace FlyAwayPlus.Helpers.UploadVideo
{
    public class YoutubeUploadResult
    {
        public string VideoId { get; set; }
        public IUploadProgress UploadProgress { get; set; }
    }
}