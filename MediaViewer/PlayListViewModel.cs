using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class PlayListViewModel
    {
        public string Song { get; set; }
        public string Path { get; set; }
        public string Length { get; set; }
        public string File { get; set; }
        public bool Selected { get; set; }
    }
}
