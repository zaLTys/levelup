using Web.UI.Controllers;
using System.Collections.Generic;

namespace Web.UI.ViewModels
{
    public class IndexViewModel
    {
        public string Content { get; set; } = "Index content";

        public IndexViewModel()
        {
            
        }
        public IndexViewModel(string content)
        {
            Content = content;
        }
    }
}
