using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebServicos.Pages
{
    public class ErroModel : PageModel
    {
        public string Code { get; set; } = "Geral";

        public void OnGet(string? code)
        {
            Code = code ?? "Geral";
        }
    }
}
