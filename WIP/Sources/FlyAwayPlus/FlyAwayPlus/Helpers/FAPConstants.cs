namespace FlyAwayPlus.Helpers
{
    public static class FapConstants
    {
        public static string DatetimeFormat = "yyyy/MM/dd HH:mm:ss";
        public static int UploadedImageMaxWidthPixcel = 800;
        public static int UploadedImageMaxHeightPixcel = 800;
        public static string[] ImageFileExtensions = { ".jpg", ".png", ".gif", ".jpeg" };
        public static int RecordsPerPage = 20;

        public static int JOIN_ADMIN = 0;
        public static int JOIN_MEMBER = 1;
        public static int JOIN_REQUEST = 2;
    }
}