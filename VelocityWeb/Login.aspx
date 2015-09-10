<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="VelocityWeb.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p><asp:Label ID="ErrorMessage" runat="server" ForeColor="Red"></asp:Label></p>
<p>Requested page requires that you Log In to your VelocityWeb account, or <asp:HyperLink ID="RegisterHyperLink" NavigateUrl="~/Register.aspx" runat="server" EnableViewState="false" Text="Register"/> (free) for an account.</p>
<table>
 <tr>
  <td>
    <div class="label">E-Mail:</div><div class="textbox"><asp:TextBox ID="Email" runat="server" Width="250"></asp:TextBox>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Email" ErrorMessage="Email" ForeColor="Red">Enter your registered E-Mail address</asp:RequiredFieldValidator></div>
  </td>
 </tr>
 <tr>
  <td>
   <div class="label">Password:</div><div class="textbox"><asp:TextBox ID="Password" runat="server" TextMode="Password" Width="250"></asp:TextBox>
   <br />
   </div>
  </td>
 </tr>
</table>
<div class="button"><asp:Button ID="LoginButton" runat="server" Text="Login" onclick="LoginButton_Click" /></div>
<p><asp:LinkButton ID="ForgotPassword" runat="server" OnClick="ForgotPasswordLinkButton_Click">Forgot your Password?</asp:LinkButton></p>
</asp:Content>
