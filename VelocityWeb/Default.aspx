<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="VelocityWeb.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="steelbluebox">
<div class="steelblueboxtitle">
<h1>Sample Web Site using VelocityDB for all persistent storage.</h1>
</div>
<div class="boxcontents">
<p>This is a <a href="http://www.VelocityDB.com/VelocityWeb">web site</a> with some code shared with VelocityDB.com. It shows how to store user information in one set of databases and issues/bugs tracking in a another set of databases. The web site we cloned, VelocityDB.com, also have code for license generation/management, charts and other tricks.
We hope you can learn how to build your own web sites with VelocityDB by looking at this sample code. Another sample web site using VelocityDB for data storage is <a href="http://www.OnlineStoreFinder.com">OnlineStoreFinder.com</a>. If anyone is intrested in building a shopping site managing, querying and comparing millions of products, that is the sample to study.
Send us an <a href="mailto:Mats@VelocityDB.com">email</a> to discuss it.
</p>
</div>
</div>
<div class="steelbluebox">
<div class="steelblueboxtitle">
<h1>IssueTracker brief introduction</h1>
</div>
<div class="boxcontents">
<p>The IssueTracker consists of issues, users, projects, components and versions. Any user can create issues but only an admin user can create/modify projects, components and users. The very first user to connect to the issue tracker becomes admin automaticly. An admin user can decide what other users are admin users. The code is not elegant but a developers first attempt in using Ajax controls for a rather complex application. I am sure you can do better!</p>
<p>Email notification is build in but you need to update the code with your own credentials, search for NetworkCredential in the source code for places to update.</p>
</div>
</div>
</asp:Content>
