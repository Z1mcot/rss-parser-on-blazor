using RSS.Data;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace RSS.Pages.Add
{
    public class AddModel
    {
        [DataType(DataType.Url, ErrorMessage = "Неправильный формат ссылки")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Введите ссылку на rss канал")]
        public string Url { get; set; }
        public Feed Feed { get; set; }
    }
}
