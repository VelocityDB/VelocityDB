<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="VelocityWeb.Contact" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="steelbluebox">
<div class="steelblueboxtitle">
<h1 class="boxtitleH1">Contact VelocityDB, Inc. - <small><asp:Label ID="month" runat="server"/></small></h1>
</div>
<div class="boxcontents">
<h3>Email Support</h3>
<p><a href="mailto:support@velocitydb.com">support@VelocityDB.com</a></p>
<h3>Email Sales</h3>
<p><a href="mailto:sales@velocitydb.com">sales@VelocityDB.com</a></p>
<h3>Email Mats Persson (founder)</h3>
<p><a href="mailto:Mats@VelocityDB.com">Mats@VelocityDB.com</a></p>
<h3>Mailing address</h3>
<p>2034 Cordoba Pl</p>
<p>Carlsbad, CA 92008-3710</p>
<h3>Skype</h3>
<p>VelocityDB</p>
<h3>Phone</h3>
<p>+1 760 845 4368</p>
<h3>Mobile</h3>
<p>+1 408 596 0973</p></div>
</div>
<%--<script type="text/javascript" src="http://cdn.dev.skype.com/uri/skype-uri.js"></script>
<div id="genSkypeCall_01">
    <script type="text/javascript">
        Skype.ui({
            name: "call",
            element: "genSkypeCall_01",
            participants: ["VelocityDB"],
            imageSize: 32,
            imageColor: "skype"
        });
    </script>
</div>--%>

<%--<h3>Phone number</h3>--%>
<%--<p>858 246 6700</p>--%>
</asp:Content>
