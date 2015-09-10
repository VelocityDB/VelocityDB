using System;
using System.Collections.Generic;
using System.Linq;
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
  public partial class Login : System.Web.UI.Page
  {
    static readonly string dataPath = HttpContext.Current.Server.MapPath("~/Database");
    string continueUrl;
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
        Page.Header.Title = "VelocityDb - Login";
        DataCache.MaximumMemoryUse = 1000000000; // 1 GB, set this to what fits your case
        continueUrl = Request.QueryString["ReturnUrl"];
        if (continueUrl == null)
          continueUrl = "/Secure/Issues.aspx";
        RegisterHyperLink.NavigateUrl = "Register.aspx?ReturnUrl=" + HttpUtility.UrlEncode(continueUrl);
      }
    }

    protected void ForgotPasswordLinkButton_Click(object sender, EventArgs e)
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(dataPath, 2000, true, true))
        {
          session.BeginRead();
          Root root = (Root)session.Open(Root.PlaceInDatabase, 1, 1, false);
          if (root == null)
          {
            ErrorMessage.Text = "The entered email address is not registered with this website";
            session.Abort();
            return;
          }
          CustomerContact lookup = new CustomerContact(Email.Text, null);
          if (!root.customersByEmail.TryGetKey(lookup, ref lookup))
          {
            ErrorMessage.Text = "The entered email address is not registered with this website";
            session.Abort();
            return;
          }
          session.Commit();
          MailMessage message = new MailMessage("Support@VelocityDb.com", Email.Text);
          message.Subject = "Your password to VelocityWeb";
          message.Body = "Password is: " + lookup.password;
          SmtpClient client = new SmtpClient("smtpout.secureserver.net", 80);
          System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("Mats@VelocityDb.com", "xxxx");
          client.Credentials = SMTPUserInfo;
          client.Send(message);
          ErrorMessage.Text = "Email with your password was send to: " + Email.Text;
        }
      }
      catch (System.Exception ex)
      {
        ErrorMessage.Text = ex.ToString();
      }
    }
    
    protected void LoginButton_Click(object sender, EventArgs e)
    {
      if (Password.Text.Length == 0)
      {
        ErrorMessage.Text = "Enter your password.";
        return;
      }
      try
      {
        using (SessionNoServer session = new SessionNoServer(dataPath, 2000, true, true))
        {
          session.BeginUpdate();
          Root velocityDbroot = (Root)session.Open(Root.PlaceInDatabase, 1, 1, false);
          if (velocityDbroot == null)
          {
            ErrorMessage.Text = "The entered email address is not registered with this website";
            session.Abort();
            return;
          }
          CustomerContact lookup = new CustomerContact(Email.Text, null);
          if (!velocityDbroot.customersByEmail.TryGetKey(lookup, ref lookup))
          {
            ErrorMessage.Text = "The entered email address is not registered with this website";
            session.Abort();
            return;
          }
          if (lookup.password != Password.Text)
          {
            ErrorMessage.Text = "The entered password does not match the registered password";
            session.Abort();
            return;
          }
          session.Commit();
          Session["UserName"] = lookup.UserName;
          Session["Email"] = Email.Text;
          Session["FirstName"] = lookup.FirstName;
          Session["LastName"] = lookup.LastName;
          FormsAuthentication.RedirectFromLoginPage(Email.Text, false);
        }
      }
      catch (System.Exception ex)
      {
        ErrorMessage.Text = ex.ToString() + ex.Message;
      }
    }
  }
}
