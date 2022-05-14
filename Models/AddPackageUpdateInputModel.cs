namespace DevTrackR.API.Models
{
    public class AddPackageUpdateInputModel
    {
        public string Status { get; set; }
        public bool Delivered { get; set; }

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

    }
}