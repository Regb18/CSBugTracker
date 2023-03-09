using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSBugTracker.Models.ViewModels
{
    public class AddDevToTicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DevList { get; set; }
        public string? DevId { get; set; }
    }
}
