<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="VelocityWeb.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <style type="text/css">
    .style1
    {
      width: 100%;
    }
    .style2
    {
      color: #000000;
    }
    .style3
    {
      width: 220px;
      text-align: right;
    }
    .style3s
    {
      width: 220px;
      font-size: x-small;     
      text-align: left;
    }
    .style4
    {
      width: 253px;
      margin-left: 40px;
    }
    .style5
    {
      font-size: large;
    }
    .style6
    {
      width: 145px;
      text-align: right;
      height: 26px;
    }
    .style7
    {
      width: 253px;
      margin-left: 40px;
      height: 26px;
    }
    .style8
    {
      height: 26px;
    }
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <p class="style5">
       <asp:label id="errors" runat="server" ForeColor="Red"></asp:label>
      </p>
  <asp:Panel ID="Panel1" runat="server" DefaultButton="RegisterButton">
  <table class="style1">
     <tr class="style2">
      <td class="style3">
        Company/Organization:</td>
      <td class="style4">
        <asp:TextBox ID="CompanyName" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        First Name:</td>
      <td class="style4">
        <asp:TextBox ID="FirstName" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>*
        <asp:RequiredFieldValidator ID="RequiredFirstNameValidator" runat="server" 
          Display="Dynamic" ErrorMessage="First Name" ForeColor="Red" 
          ControlToValidate="FirstName">Enter your First Name</asp:RequiredFieldValidator>
      </td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Last Name:</td>
      <td class="style4">
        <asp:TextBox ID="LastName" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>*
        <asp:RequiredFieldValidator ID="RequiredLastNameValidator" runat="server" 
          ErrorMessage="RequiredFieldValidator" ForeColor="Red" 
          ControlToValidate="LastName">Enter your Last Name</asp:RequiredFieldValidator>
      </td>
    </tr>
    <tr class="style2">
      <td class="style3">
        E-Mail:</td>
      <td class="style4">
        <asp:TextBox ID="Email" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>*
        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" 
          Display="Dynamic" ErrorMessage="E-Mail Adress" ForeColor="Red"          
          ValidationExpression="^[\w-\.]{1,}\@([\da-zA-Z-]{1,}\.){1,}[\da-zA-Z-]{2,6}$" 
          ControlToValidate="Email">Enter a valid E-Mail address</asp:RegularExpressionValidator>
      </td>
    </tr>
    <tr>
    <td></td>
    <td>
        <asp:Button ID="SendEmailVerificationButton" runat="server" Text="Send Email Verification Number" 
          Width="217px" BackColor="Yellow" CausesValidation="False" 
          onclick="SendEmailVerificationButton_Click" /></td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Email Verification:</td>
      <td class="style4">
        <asp:TextBox ID="EmailVerification" runat="server" Width="250px"></asp:TextBox>
      </td>
       <td>*
        <asp:RequiredFieldValidator ID="EmailVerificationValidator" runat="server" 
          ErrorMessage="RequiredFieldValidator" ForeColor="Red" 
          ControlToValidate="EmailVerification">Enter the email verification number from the email send to your email when clicking the \"Send Email Verification Number\" Button </asp:RequiredFieldValidator>
      </td>
      <td>
        &nbsp;</td>
    </tr>
     <tr class="style2">
      <td class="style3">
        Address:</td>
      <td class="style4">
        <asp:TextBox ID="Address" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>

    <tr class="style2">
      <td class="style3">
        Address line 2:</td>
      <td class="style4">
        <asp:TextBox ID="AddressLine2" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        City:</td>
      <td class="style4">
        <asp:TextBox ID="City" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Zip Code/Postal Code:</td>
      <td class="style4">
        <asp:TextBox ID="ZipCode" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        State/Province:</td>
      <td class="style4">
        <asp:TextBox ID="State" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Country:</td>
      <td class="style4">
        <asp:DropDownList ID=Country runat="server">
          <asp:ListItem>United States</asp:ListItem>
          <asp:ListItem>Canada</asp:ListItem>
          <asp:ListItem>Afghanistan</asp:ListItem>
          <asp:ListItem>Albania</asp:ListItem>
          <asp:ListItem>Algeria</asp:ListItem>
          <asp:ListItem>American Samoa</asp:ListItem>
                 <asp:ListItem>Andorra</asp:ListItem>
          <asp:ListItem>Angola</asp:ListItem>
          <asp:ListItem>Anguilla</asp:ListItem>
          <asp:ListItem>Antarctica</asp:ListItem>
          <asp:ListItem>Antigua and Barbuda</asp:ListItem>
          <asp:ListItem>Argentina</asp:ListItem>
                 <asp:ListItem>Armenia</asp:ListItem>
          <asp:ListItem>Aruba</asp:ListItem>
          <asp:ListItem>Australia</asp:ListItem>
          <asp:ListItem>Austria</asp:ListItem>
          <asp:ListItem>Azerbaijan</asp:ListItem>
          <asp:ListItem>Bahamas</asp:ListItem>
          <asp:ListItem>Bahrain</asp:ListItem>
          <asp:ListItem>Bangladesh</asp:ListItem>
          <asp:ListItem>Barbados</asp:ListItem>
          <asp:ListItem>Belarus</asp:ListItem>
          <asp:ListItem>Belgium</asp:ListItem>
          <asp:ListItem>Belize</asp:ListItem>
          <asp:ListItem>Benin</asp:ListItem>
          <asp:ListItem>Bermuda</asp:ListItem>
          <asp:ListItem>Bhutan</asp:ListItem>
          <asp:ListItem>Bolivia</asp:ListItem>
          <asp:ListItem>Bosnia and Herzegovina</asp:ListItem>
          <asp:ListItem>Botswana</asp:ListItem>
          <asp:ListItem>Bouvet Island</asp:ListItem>
          <asp:ListItem>Brazil</asp:ListItem>
          <asp:ListItem>British Indian Ocean Territory</asp:ListItem>
          <asp:ListItem>Brunei Darussalam</asp:ListItem>
          <asp:ListItem>Bulgaria</asp:ListItem>
          <asp:ListItem>Burkina Faso</asp:ListItem>
          <asp:ListItem>Burundi</asp:ListItem>
          <asp:ListItem>Cambodia</asp:ListItem>
          <asp:ListItem>Cameroon</asp:ListItem>
          <asp:ListItem>Cape Verde</asp:ListItem>
          <asp:ListItem>Cayman Islands</asp:ListItem>
          <asp:ListItem>Central African Republic</asp:ListItem>
          <asp:ListItem>Chad</asp:ListItem>
          <asp:ListItem>Chile</asp:ListItem>
          <asp:ListItem>China</asp:ListItem>
          <asp:ListItem>Christmas Island</asp:ListItem>
          <asp:ListItem>Cocos (Keeling) Islands</asp:ListItem>
          <asp:ListItem>Colombia</asp:ListItem>
          <asp:ListItem>Comoros</asp:ListItem>
          <asp:ListItem>Congo</asp:ListItem>
          <asp:ListItem>Congo, The Democratic Republic</asp:ListItem>
          <asp:ListItem>Cook Islands</asp:ListItem>
          <asp:ListItem>Costa Rica</asp:ListItem>
          <asp:ListItem>Cote D`Ivoire</asp:ListItem>
          <asp:ListItem>Croatia</asp:ListItem>
          <asp:ListItem>Cuba</asp:ListItem>
          <asp:ListItem>Cyprus</asp:ListItem>
          <asp:ListItem>Czech Republic</asp:ListItem>
          <asp:ListItem>Denmark</asp:ListItem>
          <asp:ListItem>Djibouti</asp:ListItem>
          <asp:ListItem>Dominica</asp:ListItem>
          <asp:ListItem>Dominican Republic</asp:ListItem>
          <asp:ListItem>East Timor</asp:ListItem>
          <asp:ListItem>Ecuador</asp:ListItem>
          <asp:ListItem>Egypt</asp:ListItem>
          <asp:ListItem>El Salvador</asp:ListItem>
          <asp:ListItem>Equatorial Guinea</asp:ListItem>
          <asp:ListItem>Eritrea</asp:ListItem>
          <asp:ListItem>Estonia</asp:ListItem>
          <asp:ListItem>Ethiopia</asp:ListItem>
          <asp:ListItem>Falkland Islands (Malvinas)</asp:ListItem>
          <asp:ListItem>Faroe Islands</asp:ListItem>
          <asp:ListItem>Finland</asp:ListItem>
          <asp:ListItem>Fmr Yugoslav Rep of Macedonia</asp:ListItem>
          <asp:ListItem>France</asp:ListItem>
          <asp:ListItem>French Guiana</asp:ListItem>
          <asp:ListItem>French Polynesia</asp:ListItem>
          <asp:ListItem>French Southern Territories</asp:ListItem>
          <asp:ListItem>Gabon</asp:ListItem>
          <asp:ListItem>Gambia</asp:ListItem>
          <asp:ListItem>Georgia</asp:ListItem>
          <asp:ListItem>Germany</asp:ListItem>
          <asp:ListItem>Ghana</asp:ListItem>
          <asp:ListItem>Gibraltar</asp:ListItem>
          <asp:ListItem>Greece</asp:ListItem>
          <asp:ListItem>Greenland</asp:ListItem>
          <asp:ListItem>Grenada</asp:ListItem>
          <asp:ListItem>Guadeloupe</asp:ListItem>
          <asp:ListItem>Guam</asp:ListItem>
          <asp:ListItem>Guatemala</asp:ListItem>
          <asp:ListItem>Guinea</asp:ListItem>
          <asp:ListItem>Guinea-bissau</asp:ListItem>
          <asp:ListItem>Guyana</asp:ListItem>
          <asp:ListItem>Haiti</asp:ListItem>
          <asp:ListItem>Heard and McDonald Islands</asp:ListItem>
          <asp:ListItem>Holy See (Vatican City State)</asp:ListItem>
          <asp:ListItem>Honduras</asp:ListItem>
          <asp:ListItem>Hong Kong</asp:ListItem>
          <asp:ListItem>Hungary</asp:ListItem>
          <asp:ListItem>Iceland</asp:ListItem>
          <asp:ListItem>India</asp:ListItem>
          <asp:ListItem>Indonesia</asp:ListItem>
          <asp:ListItem Value="IR">Iran, Islamic Republic Of</asp:ListItem>
          <asp:ListItem>Iraq</asp:ListItem>
          <asp:ListItem>Ireland</asp:ListItem>
          <asp:ListItem>Israel</asp:ListItem>
          <asp:ListItem>Italy</asp:ListItem>
          <asp:ListItem>Jamaica</asp:ListItem>
          <asp:ListItem>Japan</asp:ListItem>
          <asp:ListItem>Jordan</asp:ListItem>
          <asp:ListItem>Kazakstan</asp:ListItem>
          <asp:ListItem>Kenya</asp:ListItem>
          <asp:ListItem>Kiribati</asp:ListItem>
          <asp:ListItem Value="KP">Korea, Democratic People`s Rep</asp:ListItem>
          <asp:ListItem Value="KR">Korea, Republic of</asp:ListItem>
          <asp:ListItem>Kuwait</asp:ListItem>
          <asp:ListItem>Kyrgyzstan</asp:ListItem>
          <asp:ListItem Value="LA">Lao People`s Democratic Rep</asp:ListItem>
          <asp:ListItem>Latvia</asp:ListItem>
          <asp:ListItem>Lebanon</asp:ListItem>
          <asp:ListItem>Lesotho</asp:ListItem>
          <asp:ListItem>Liberia</asp:ListItem>
          <asp:ListItem>Libyan Arab Jamahiriya</asp:ListItem>
          <asp:ListItem>Liechtenstein</asp:ListItem>
          <asp:ListItem>Lithuania</asp:ListItem>
          <asp:ListItem>Luxembourg</asp:ListItem>
          <asp:ListItem>Macau</asp:ListItem>
          <asp:ListItem>Madagascar</asp:ListItem>
          <asp:ListItem>Malawi</asp:ListItem>
          <asp:ListItem>Malaysia</asp:ListItem>
          <asp:ListItem>Maldives</asp:ListItem>
          <asp:ListItem>Mali</asp:ListItem>
          <asp:ListItem>Malta</asp:ListItem>
          <asp:ListItem>Marshall Islands</asp:ListItem>
          <asp:ListItem>Martinique</asp:ListItem>
          <asp:ListItem>Mauritania</asp:ListItem>
          <asp:ListItem>Mauritius</asp:ListItem>
          <asp:ListItem>Mayotte</asp:ListItem>
          <asp:ListItem>Mexico</asp:ListItem>
          <asp:ListItem>Micronesia, Federated States</asp:ListItem>
          <asp:ListItem>Moldova, Republic of</asp:ListItem>
          <asp:ListItem>Monaco</asp:ListItem>
          <asp:ListItem>Mongolia</asp:ListItem>
          <asp:ListItem>Montserrat</asp:ListItem>
          <asp:ListItem>Morocco</asp:ListItem>
          <asp:ListItem>Mozambique</asp:ListItem>
          <asp:ListItem>Myanmar</asp:ListItem>
          <asp:ListItem>Namibia</asp:ListItem>
          <asp:ListItem>Netherlands Antilles</asp:ListItem>
          <asp:ListItem>New Caledonia</asp:ListItem>
          <asp:ListItem>New Zealand</asp:ListItem>
          <asp:ListItem>Nicaragua</asp:ListItem>
          <asp:ListItem>Niger</asp:ListItem>
          <asp:ListItem>Nigeria</asp:ListItem>
          <asp:ListItem>Niue</asp:ListItem>
          <asp:ListItem>Norfolk Island</asp:ListItem>
          <asp:ListItem Value="MP">Northern Mariana Islands</asp:ListItem>
          <asp:ListItem>Norway</asp:ListItem>
          <asp:ListItem>Oman</asp:ListItem>
          <asp:ListItem>Pakistan</asp:ListItem>
          <asp:ListItem>Palau</asp:ListItem>
          <asp:ListItem Value="PS">Palestinian Territory, Occupied</asp:ListItem>
          <asp:ListItem>Panama</asp:ListItem>
          <asp:ListItem>Papua New Guinea</asp:ListItem>
          <asp:ListItem>Paraguay</asp:ListItem>
          <asp:ListItem>Peru</asp:ListItem>
          <asp:ListItem>Philippines</asp:ListItem>
          <asp:ListItem>Pitcairn</asp:ListItem>
          <asp:ListItem>Poland</asp:ListItem>
          <asp:ListItem>Portugal</asp:ListItem>
          <asp:ListItem>Puerto Rico</asp:ListItem>
          <asp:ListItem>Qatar</asp:ListItem>
          <asp:ListItem>Reunion</asp:ListItem>
          <asp:ListItem>Romania</asp:ListItem>
          <asp:ListItem>Russian Federation</asp:ListItem>
          <asp:ListItem>Rwanda</asp:ListItem>
          <asp:ListItem>Saint Helena</asp:ListItem>
          <asp:ListItem Value="KN">Saint Kitts and Nevis</asp:ListItem>
          <asp:ListItem>Saint Lucia</asp:ListItem>
          <asp:ListItem Value="PM">Saint Pierre And Miquelon</asp:ListItem>
          <asp:ListItem>Samoa</asp:ListItem>
          <asp:ListItem>San Marino</asp:ListItem>
          <asp:ListItem>Sao Tome And Principe</asp:ListItem>
          <asp:ListItem>Saudi Arabia</asp:ListItem>
          <asp:ListItem>Senegal</asp:ListItem>
          <asp:ListItem>Seychelles</asp:ListItem>
          <asp:ListItem>Sierra Leone</asp:ListItem>
          <asp:ListItem>Singapore</asp:ListItem>
          <asp:ListItem>Slovakia</asp:ListItem>
          <asp:ListItem>Slovenia</asp:ListItem>
          <asp:ListItem>Solomon Islands</asp:ListItem>
          <asp:ListItem>Somalia</asp:ListItem>
          <asp:ListItem>South Africa</asp:ListItem>
          <asp:ListItem>Spain</asp:ListItem>
          <asp:ListItem>Sri Lanka</asp:ListItem>
          <asp:ListItem>St Vincent and the Grenadines</asp:ListItem>
          <asp:ListItem>Sth Georgia &amp; Sth Sandwich Is</asp:ListItem>
          <asp:ListItem>Sudan</asp:ListItem>
          <asp:ListItem>Suriname</asp:ListItem>
          <asp:ListItem>Svalbard And Jan Mayen</asp:ListItem>
          <asp:ListItem>Swaziland</asp:ListItem>
          <asp:ListItem>Sweden</asp:ListItem>
          <asp:ListItem>Switzerland</asp:ListItem>
          <asp:ListItem>Syrian Arab Republic</asp:ListItem>
          <asp:ListItem>Taiwan, Province Of China</asp:ListItem>
          <asp:ListItem>Tajikistan</asp:ListItem>
          <asp:ListItem>Tanzania, United Republic Of</asp:ListItem>
          <asp:ListItem>Thailand</asp:ListItem>
          <asp:ListItem>Togo</asp:ListItem>
          <asp:ListItem>Tokelau</asp:ListItem>
          <asp:ListItem>Tonga</asp:ListItem>
          <asp:ListItem>Trinidad And Tobago</asp:ListItem>
          <asp:ListItem>Tunisia</asp:ListItem>
          <asp:ListItem>Turkey</asp:ListItem>
          <asp:ListItem>Turkmenistan</asp:ListItem>
          <asp:ListItem>Turks And Caicos Islands</asp:ListItem>
          <asp:ListItem>United Arab Emirates</asp:ListItem>
          <asp:ListItem>United Kingdom</asp:ListItem>
          <asp:ListItem>Uruguay</asp:ListItem>
          <asp:ListItem>US Minor Outlying Islands</asp:ListItem>
          <asp:ListItem Value="UZ">Uzbekistan</asp:ListItem>
          <asp:ListItem Value="VU">Vanuatu</asp:ListItem>
          <asp:ListItem Value="VE">Venezuela</asp:ListItem>
          <asp:ListItem Value="VN">Viet Nam</asp:ListItem>
          <asp:ListItem Value="VG">Virgin Islands, British</asp:ListItem>
          <asp:ListItem Value="VI">Virgin Islands, U.S.</asp:ListItem>
          <asp:ListItem Value="WF">Wallis And Futuna</asp:ListItem>
          <asp:ListItem Value="EH">Western Sahara</asp:ListItem>
          <asp:ListItem Value="YE">Yemen</asp:ListItem>
          <asp:ListItem Value="CS">Serbia and Montenegro</asp:ListItem>
          <asp:ListItem Value="ZM">Zambia</asp:ListItem>
          <asp:ListItem Value="ZW">Zimbabwe</asp:ListItem>
                 </asp:DropDownList>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Phone:</td>
      <td class="style4">
        <asp:TextBox ID="Phone" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Fax:</td>
      <td class="style4">
        <asp:TextBox ID="Fax" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Mobile Phone:</td>
      <td class="style4">
        <asp:TextBox ID="MobilePhone" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style6">
        Skype Name:</td>
      <td class="style7">
        <asp:TextBox ID="SkypeName" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td class="style8">
        </td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Website:</td>
      <td class="style4">
        <asp:TextBox ID="Website" runat="server" Width="250px"></asp:TextBox>
      </td>
      <td>
        &nbsp;</td>
    </tr>
    <tr class="style2">
      <td class="style3">
        User Name:</td>
      <td class="style4">
        <asp:TextBox ID="UserName" runat="server" Width="250px" MaxLength="10"></asp:TextBox>
      </td>
      <td>*
        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
          ControlToValidate="UserName" ErrorMessage="User Name" ForeColor="Red">Enter a User Name (3 to 10 characters)</asp:RequiredFieldValidator>
        <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" 
          Display="Dynamic" ErrorMessage="User Name" ForeColor="Red" 
          ValidationExpression="\S{3,10}" ControlToValidate="UserName">User Name must be between 3 and 10 characters long</asp:RegularExpressionValidator>
      </td>
    </tr>
    <tr class="style2">
      <td class="style3">
        How did you find VelocityDB?</td>
      <td class="style4">
        <table>
        <tr><td><asp:RadioButtonList ID="HowFoundRadioButtonList" runat="server"></asp:RadioButtonList></td></tr>
        <tr><td><asp:TextBox ID="HowFoundTextBox" runat="server" Width="250px"></asp:TextBox></td></tr>
        </table>
      </td>
      <td>
      </td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Password:</td>
      <td class="style4">
        <asp:TextBox ID="Password" runat="server" Width="250px" TextMode="Password"></asp:TextBox>
      </td>
      <td>*
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
          ControlToValidate="Password" Display="Dynamic" ErrorMessage="Choose a password" 
          ForeColor="Red"></asp:RequiredFieldValidator>
        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" 
          Display="Dynamic" ErrorMessage="Password length" ForeColor="Red" 
          ValidationExpression="\S{6,25}" ControlToValidate="Password">Password must be at least 6 chars long, max 25</asp:RegularExpressionValidator>
      </td>
    </tr>
    <tr class="style2">
      <td class="style3">
        Password (confirm):</td>
      <td class="style4">
        <asp:TextBox ID="PasswordConfirm" runat="server" Width="250px" TextMode="Password"></asp:TextBox>
      </td>
      <td>*
        <asp:CompareValidator ID="CompareValidator1" runat="server" 
          ControlToCompare="Password" ControlToValidate="PasswordConfirm" 
          Display="Dynamic" ErrorMessage="Does not match first password string" 
          ForeColor="Red"></asp:CompareValidator>
      </td>
    </tr>
    <tr class="style2">
      <td class="style3s">* is required
        &nbsp;</td>
      <td class="style4">
        <asp:Button ID="RegisterButton" runat="server" Text="Register" 
          onclick="RegisterButton_Click"/>
      </td>
      <td>
        &nbsp;</td>
    </tr>
  </table>  </asp:Panel>
</asp:Content>

