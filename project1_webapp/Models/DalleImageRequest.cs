namespace project1_webapp.Models {
    public class DalleImageRequest {

        public string model { get; set; } = "dall-e-3";
        public int n { get; set; } = 1;
        public string size { get; set; } = "1024x1024";
        public string prompt { get; set; }

    }
}
