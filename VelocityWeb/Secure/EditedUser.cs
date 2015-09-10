using System;

namespace VelocityWeb
{
  [System.ComponentModel.DataObject(true)]
  public class EditedUser
  {
    public EditedUser() { }

    public UInt64 Id { get; set; }
    public string Oid { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string CreatedBy { get; set; }
    public DateTime DateTimeCreated { get; set; } 
  }
}