using Microsoft.AspNetCore.Mvc;

namespace Contacts.api.Controllers;

[ApiController]
public class ContactsController: ControllerBase
{
    public ContactsController()
    {
    }
    
    public async Task<ActionResult<ICollection<ContactListModel>>> GetContacts()
    {
        return Ok();
    }
}