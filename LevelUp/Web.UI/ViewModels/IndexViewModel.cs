using Web.UI.Controllers;
using System.Collections.Generic;
using Demo.Web.API;

namespace Web.UI.ViewModels
{
    public class IndexViewModel
    {
        public List<WeatherForecast> Content { get; set; }

        public IndexViewModel(List<WeatherForecast> content)
        {
            Content = content;
        }
    }
}
