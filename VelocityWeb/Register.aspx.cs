using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.VelocityWeb;

namespace VelocityWeb
{
  public partial class Register : System.Web.UI.Page
  {
    static readonly string dataPath = HttpContext.Current.Server.MapPath("~/Database");
    static List<string> UsaStates = new List<string>()
{ "Alabama", "AL", "Alaska", "AK", "Arizona", "AZ", "Arkansas", "AR", "California", "CA", "Colorado", "CO","Connecticut", "CT", "Delaware", "DE", "Florida", "FL", "Georgia", "GA",
"Hawaii", "HI","Idaho", "ID","Illinois", "IL","Indiana", "IN","Iowa", "IA","Kansas", "KS","Kentucky", "KY","Louisiana", "LA","Maine", "ME","Maryland", "MD","Massachusetts", "MA","Michigan", "MI","Minnesota", "MN",
"Mississippi", "MS", "Missouri", "MO",
"Montana", "MT", "Nebraska", "NE",
"Nevada", "NV", "New Hampshire", "NH",
"New Jersey", "NJ", "New Mexico", "NM",
"New York", "NY", "North Carolina", "NC",
"North Dakota", "ND", "Ohio", "OH",
"Oklahoma", "OK", "Oregon", "OR", "Pennsylvania", "PA",
"Rhode Island", "RI","South Carolina", "SC",
"South Dakota", "SD", "Tennessee", "TN",
"Texas", "TX", "Utah", "UT",
"Vermont", "VT", "Virginia", "VA",
"Washington", "WA", "West Virginia", "WV",
"Wisconsin", "WI", "Wyoming", "WY" 
};
    string continueUrl;
    CustomerContact existingCustomer = null;
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
        Session["EmailVerification"] = -1;
        Session["EmailVerificationEmail"] = "none";
        Page.Header.Title = "VelocityDB - Register";
        continueUrl = Request.QueryString["ReturnUrl"];
        HowFoundRadioButtonList.DataSource = AllHowFound();
        HowFoundRadioButtonList.SelectedIndex = 0;
        HowFoundRadioButtonList.DataBind();
        DataCache.UnauthorizedPerformanceCounter = true;
        try
        {
          using (SessionNoServer session = new SessionNoServer(dataPath, 2000, true, true))
          {
            session.BeginUpdate();
            Root velocityDbroot = (Root)session.Open(Root.PlaceInDatabase, 1, 1, false);
            if (velocityDbroot == null)
            {
              Placement placementRoot = new Placement(Root.PlaceInDatabase, 1, 1, 1000, 1000, true);
              velocityDbroot = new Root(session, 10000);
              velocityDbroot.Persist(placementRoot, session);
            }
            else
            {
              string user = this.User.Identity.Name;
              if (user != null && user.Length > 0)
              {
                CustomerContact lookup = new CustomerContact(user, null);
                if (velocityDbroot.customersByEmail.TryGetKey(lookup, ref existingCustomer))
                {
                  EmailVerificationValidator.Enabled = false;
                  CompanyName.Text = existingCustomer.company;
                  FirstName.Text = existingCustomer.firstName;
                  LastName.Text = existingCustomer.lastName;
                  Email.Text = existingCustomer.email;
                  Address.Text = existingCustomer.address;
                  AddressLine2.Text = existingCustomer.addressLine2;
                  City.Text = existingCustomer.city;
                  ZipCode.Text = existingCustomer.zipCode;
                  State.Text = existingCustomer.state;
                  Country.SelectedValue = existingCustomer.countryCode;
                  Phone.Text = existingCustomer.phone;
                  Fax.Text = existingCustomer.fax;
                  MobilePhone.Text = existingCustomer.mobile;
                  SkypeName.Text = existingCustomer.skypeName;
                  Website.Text = existingCustomer.webSite;
                  UserName.Text = existingCustomer.userName;
                  Password.Text = existingCustomer.password;
                  PasswordConfirm.Text = existingCustomer.password;
                  HowFoundRadioButtonList.SelectedIndex = (int)existingCustomer.howFoundVelocityDb;
                  HowFoundTextBox.Text = existingCustomer.howFoundOther;
                }
                else if (Request.IsLocal)
                  EmailVerificationValidator.Enabled = false;
              }
            }
            session.Commit();
          }
        }
        catch (System.Exception ex)
        {
          errors.Text = ex.ToString();
        }
      }
    }

    protected void RegisterButton_Click(object sender, EventArgs e)
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(dataPath, 2000, true, true))
        {
          session.BeginUpdate();
          CustomerContact customer = new CustomerContact(CompanyName.Text, FirstName.Text, LastName.Text, Email.Text, Address.Text, AddressLine2.Text,
            City.Text, ZipCode.Text, State.Text, Country.SelectedItem.Text, Country.SelectedItem.Value, Phone.Text, Fax.Text, MobilePhone.Text,
            SkypeName.Text, Website.Text, UserName.Text, Password.Text, HowFoundTextBox.Text, HowFoundRadioButtonList.SelectedIndex, session);
          Root root = (Root)session.Open(Root.PlaceInDatabase, 1, 1, false);
          CustomerContact lookup;
          string user = this.User.Identity.Name;
          if (user != null && user.Length > 0)
          {
            lookup = new CustomerContact(user, null);
            root.customersByEmail.TryGetKey(lookup, ref existingCustomer);
          }
          else
          {
            lookup = new CustomerContact(customer.email, customer.userName);
            if (!root.customersByEmail.TryGetKey(lookup, ref existingCustomer))
              root.customersByUserName.TryGetKey(lookup, ref existingCustomer);
          }
          if (existingCustomer != null)
          {
            existingCustomer.Update();

            if (existingCustomer.email != customer.email)
            {
              string verifiedEmail = (string)Session["EmailVerificationEmail"];
              int emailVerification = (int)Session["EmailVerification"];
              if (Request.IsLocal == false)
              {
                if (emailVerification < 0 || verifiedEmail != customer.email)
                {
                  errors.Text = "Email was not verified for new user registration";
                  session.Abort();
                  return;
                }
                int enteredVerificationNumber;
                int.TryParse(EmailVerification.Text, out enteredVerificationNumber);
                if (emailVerification != enteredVerificationNumber)
                {
                  errors.Text = "Entered Email Verification number is " + enteredVerificationNumber + " does match the emailed verification number: " + emailVerification;
                  Session["EmailVerification"] = -1;
                  session.Abort();
                  return;
                }
              }
              if (existingCustomer.password != customer.password && user != existingCustomer.email)
              {
                errors.Text = "Entered Email address already registered";
                session.Abort();
                return;
              }
              existingCustomer.priorVerifiedEmailSet.Add(existingCustomer.email);
              root.customersByEmail.Remove(existingCustomer);
              existingCustomer.email = customer.email;
              root.customersByEmail.Add(existingCustomer);
            }
            if (existingCustomer.userName != customer.userName)
            {
              lookup = new CustomerContact(user, customer.userName);
              if (root.customersByUserName.TryGetKey(lookup, ref lookup))
              {
                errors.Text = "Entered User Name is already in use";
                session.Abort();
                return;
              }
              // remove and add to get correct sorting order
              root.customersByUserName.Remove(existingCustomer);
              existingCustomer.userName = customer.userName;
              root.customersByUserName.Add(existingCustomer);
            }
            existingCustomer.company = customer.company;
            existingCustomer.firstName = customer.firstName;
            existingCustomer.lastName = customer.lastName;
            existingCustomer.address = customer.address;
            existingCustomer.addressLine2 = customer.addressLine2;
            existingCustomer.city = customer.city;
            existingCustomer.zipCode = customer.zipCode;
            existingCustomer.state = customer.state;
            existingCustomer.country = Country.SelectedItem.Text;
            existingCustomer.countryCode = Country.SelectedItem.Value;
            existingCustomer.phone = customer.phone;
            existingCustomer.fax = customer.fax;
            existingCustomer.mobile = customer.mobile;
            existingCustomer.skypeName = customer.skypeName;
            existingCustomer.webSite = customer.webSite;
            existingCustomer.password = customer.password;
            existingCustomer.howFoundOther = customer.howFoundOther;
            existingCustomer.howFoundVelocityDb = customer.howFoundVelocityDb;
          }
          else
          {
            if (Request.IsLocal == false)
            {
              int emailVerification = (int)Session["EmailVerification"];
              string verifiedEmail = (string)Session["EmailVerificationEmail"];
              if (emailVerification < 0 || verifiedEmail != customer.email)
              {
                errors.Text = "Email was not verified for new user registration";
                session.Abort();
                return;
              }
              int enteredVerificationNumber;
              int.TryParse(EmailVerification.Text, out enteredVerificationNumber);
              if (emailVerification != enteredVerificationNumber)
              {
                errors.Text = "Entered Email Verification number is " + enteredVerificationNumber + " does match the emailed verification number: " + emailVerification;
                Session["EmailVerification"] = -1;
                session.Abort();
                return;
              }
            }
            Placement placementCustomer = new Placement(customer.PlacementDatabaseNumber);
            customer.idNumber = root.NewCustomerNumber();
            customer.Persist(placementCustomer, session);
            root.customersByEmail.Add(customer);
            root.customersByUserName.Add(customer);
          }
          session.Commit();
          string redirectUrl = FormsAuthentication.GetRedirectUrl(Email.Text, false);
          try
          {
            string path;
            MailMessage message = new MailMessage("Mats@VelocityDB.com", "Sales@VelocityDB.com");
            if (existingCustomer == null)
            {
              path = HttpContext.Current.Server.MapPath("~/Save") + "/new" + DateTime.Now.Ticks + ".txt";
              message.Subject = "VelocityWeb new prospect: " + customer.Email;
            }
            else
            {
              customer = existingCustomer;
              path = HttpContext.Current.Server.MapPath("~/Save") + "/updated" + DateTime.Now.Ticks + ".txt";
              message.Subject = "VelocityWeb updated prospect: " + customer.Email;
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
              file.Write("{0}\t", "Email");
              file.Write("{0}\t", "FirstName");
              file.Write("{0}\t", "LastName");
              file.Write("{0}\t", "Address");
              file.Write("{0}\t", "Address2");
              file.Write("{0}\t", "City");
              file.Write("{0}\t", "Company");
              file.Write("{0}\t", "Country");
              file.Write("{0}\t", "countryCode");
              file.Write("{0}\t", "Fax");
              file.Write("{0}\t", "HowFoundUs");
              file.Write("{0}\t", "HowFoundUsOther");
              file.Write("{0}\t", "Mobile");
              file.Write("{0}\t", "Password");
              file.Write("{0}\t", "Phone");
              file.Write("{0}\t", "Skype");
              file.Write("{0}\t", "State");
              file.Write("{0}\t", "UserName");
              file.Write("{0}\t", "WebSite");
              file.WriteLine("{0}\t", "ZipCode");
              file.Write("{0}\t", customer.Email);
              file.Write("{0}\t", customer.FirstName);
              file.Write("{0}\t", customer.LastName);
              file.Write("{0}\t", customer.Address);
              file.Write("{0}\t", customer.Address2);
              file.Write("{0}\t", customer.City);
              file.Write("{0}\t", customer.Company);
              file.Write("{0}\t", customer.Country);
              file.Write("{0}\t", customer.countryCode);
              file.Write("{0}\t", customer.Fax);
              file.Write("{0}\t", customer.HowFoundUs);
              file.Write("{0}\t", customer.HowFoundUsOther);
              file.Write("{0}\t", customer.Mobile);
              file.Write("{0}\t", customer.Password);
              file.Write("{0}\t", customer.Phone);
              file.Write("{0}\t", customer.Skype);
              file.Write("{0}\t", customer.State);
              file.Write("{0}\t", customer.UserName);
              file.Write("{0}\t", customer.WebSite);
              file.WriteLine("{0}\t", customer.ZipCode);             
            }
            Session["UserName"] = customer.UserName;
            Session["Email"] = Email.Text;
            Session["FirstName"] = customer.FirstName;
            Session["LastName"] = customer.LastName;
            message.Body = path;
            Attachment data = new Attachment(path);
            message.Attachments.Add(data);
            SmtpClient client = new SmtpClient("smtpout.secureserver.net", 80);
            System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("Mats@VelocityDB.com", "xxxx");        // Add credentials if the SMTP server requires them.
            client.Credentials = SMTPUserInfo;
            client.Send(message);
          }
          catch (System.Exception ex)
          {
            string errorPath = HttpContext.Current.Server.MapPath("~/Errors");
            using (StreamWriter outfile = new StreamWriter(errorPath + @"\errors.txt", true))
            {
              outfile.Write(ex.ToString());
            }
          }
          if (redirectUrl == null || redirectUrl.Length == 0)
            Response.Redirect("~/Secure/Issues.aspx");
          FormsAuthentication.RedirectFromLoginPage(Email.Text, false);
        }
      }
      catch (System.Exception ex)
      {
        errors.Text = ex.ToString();
      }
    }

    public System.Array AllHowFound()
    {
      return Enum.GetValues(typeof(CustomerContact.HowFound));
    }

    protected void SendEmailVerificationButton_Click(object sender, EventArgs e)
    {
      string email = Email.Text;
      // create mail message object
      if (email.Length > 0)
      {
        MailMessage message = new MailMessage("Support@VelocityDB.com", email);
        Random rand = new Random();
        int random = rand.Next();
        message.Subject = "VelocityWeb Email verification number: " + random;
        Session["EmailVerification"] = random;
        message.Body = "Your email verification number is: " + random;
         SmtpClient client = new SmtpClient("smtpout.secureserver.net", 80);
        System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("Mats@VelocityDB.com", "xxxx");        // Add credentials if the SMTP server requires them.
        client.Credentials = SMTPUserInfo;
        Session["EmailVerificationEmail"] = Email.Text;
        client.Send(message);
        errors.Text = "Email with your verification number was send to: " + Email.Text;
      }
    }
  }
}