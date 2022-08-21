using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace VideoHandler.Models
{
    public class SaveVideoResponse
    {
        public Video Video { get; set; }
        public bool AlreadyExists { get; set; }
        public string Message { get; set; }
    }
}
