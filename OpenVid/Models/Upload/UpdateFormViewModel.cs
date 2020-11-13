using Database.Models;
using OpenVid.Models;

namespace OpenVid.Models.Upload
{
    public class UpdateFormViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Md5 { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal Size { get; set; }


        public string Tags { get; set; }
    }
}