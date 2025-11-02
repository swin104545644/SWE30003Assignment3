namespace OnlineShop.Models;
using System.ComponentModel.DataAnnotations;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
