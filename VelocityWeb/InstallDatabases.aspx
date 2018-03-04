<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="InstallDatabases.aspx.cs" Inherits="VelocityWeb.InstallDatabases" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="steelbluebox">
<div class="steelblueboxtitle">
<h1>If you move databases from one server to another, you need to install the databases with local host name and paths.</h1>
</div>
<div class="boxcontents">
<p> The first textbox contains a secret password you have made up and is hard coded in the source code for this page. The second text box contains success/failure messages after you press Button. The third box contains an optional host name field, enter the domain of your web site there when you use install on your hosting we site, like VelocityWeb.com
</p>
</div>
</div>
<div class="steelbluebox">
<div class="steelblueboxtitle">
    <div>
      <asp:TextBox ID="DoUpdatePassword" runat="server" TextMode="Password"></asp:TextBox>
      <asp:TextBox ID="Results" runat="server" Width=500></asp:TextBox>
      <asp:Button ID="DoIt" runat="server" Text="Button" OnClick="DoItClick" />    
      <br />
      <asp:TextBox ID="HostName" runat="server" Width=200 Text=""></asp:TextBox>
    </div>
  </div>
</div>
</asp:Content>