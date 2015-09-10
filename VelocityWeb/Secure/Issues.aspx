<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Issues.aspx.cs" Inherits="VelocityWeb.Secure.Issues" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ToolkitScriptManager ID="ToolkitScriptManager" runat="server" CombineScripts="false" ></asp:ToolkitScriptManager>
  <asp:ObjectDataSource ID="VelocityDbDataSourceIssues" runat="server" SelectMethod="AllIssues" DeleteMethod="DeleteIssue" InsertMethod="InsertIssue" TypeName="VelocityWeb.Secure.Issues"
        SortParameterName="sortExpression"></asp:ObjectDataSource>
  <asp:ObjectDataSource ID="VelocityDbDataSourceIssue" runat="server" InsertMethod="InsertIssue" SelectMethod="SelectIssue" UpdateMethod="UpdateIssue" TypeName="VelocityWeb.Secure.Issues"
        DataObjectTypeName="VelocityWeb.EditedIssue" OnSelecting="VelocityDbDataSourceIssue_Selecting">
                <SelectParameters>
          <asp:Parameter Name="id" Type="Int32" />
        </SelectParameters>
      </asp:ObjectDataSource>
  <asp:Label ID="errorLabel" runat="server" ForeColor="Red"></asp:Label>
  <asp:MultiView ID="MultiView1" runat="server">
    <asp:View ID="IssuesView" runat="server">
             <script type="text/javascript" language="javascript">
                 //  attach to the pageLoaded event
                 Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);

                 function pageLoaded(sender, args) {

                     //  the data key is the control's ID
                     var dataKey = '<%= this.IssuesGridView.ClientID %>';
                     var updatedRowIndex = args.get_dataItems()[dataKey];

                     //  if there is a datakey for the grid, use it to
                     //  identify the row that was updated
                     if (updatedRowIndex) {
                         //  get the row that was updated
                         var tr = $get(dataKey).rows[parseInt(updatedRowIndex) + 1];
                         //  add the 'updated' css class
                         Sys.UI.DomElement.addCssClass(tr, 'updated');

                         //  remove the css class in 1.5 seconds
                         window.setTimeout(function () {
                             Sys.UI.DomElement.removeCssClass(
                                    tr,
                                    'updated'
                                );
                         }, 1500);
                     }
                 }
      </script>
      <script type="text/javascript">
          function UseIcons_CheckedChanged() {
              var x = document.getElementById("MainContent_UseIcons");
              var issuesDataGrid = document.getElementById("MainContent_IssuesGridView");
              var numRows = issuesDataGrid.rows.length - 1;
              for (var i = 0; i < numRows; i++) {
                  var priorityImage = document.getElementById("MainContent_IssuesGridView_PriorityImage_" + i);
                  var priorityLabel = document.getElementById("MainContent_IssuesGridView_PriorityLabel_" + i);
                  var statusImage = document.getElementById("MainContent_IssuesGridView_StatusImage_" + i);
                  var statusLabel = document.getElementById("MainContent_IssuesGridView_StatusLabel_" + i);
                  var categoryImage = document.getElementById("MainContent_IssuesGridView_CategoryImage_" + i);
                  var categoryLabel = document.getElementById("MainContent_IssuesGridView_CategoryLabel_" + i);
                  var resolutionImage = document.getElementById("MainContent_IssuesGridView_ResolutionImage_" + i);
                  var resolutionLabel = document.getElementById("MainContent_IssuesGridView_ResolutionLabel_" + i);
                  if (x.checked) {
                      priorityImage.style.visibility = "visible";
                      priorityLabel.style.visibility = "hidden";
                      statusImage.style.visibility = "visible";
                      statusLabel.style.visibility = "hidden";
                      categoryImage.style.visibility = "visible";
                      categoryLabel.style.visibility = "hidden";
                      resolutionImage.style.visibility = "visible";
                      resolutionLabel.style.visibility = "hidden";
                  }
                  else {
                      priorityImage.style.visibility = "hidden";
                      priorityLabel.style.visibility = "visible";
                      statusImage.style.visibility = "hidden";
                      statusLabel.style.visibility = "visible";
                      categoryImage.style.visibility = "hidden";
                      categoryLabel.style.visibility = "visible";
                      resolutionImage.style.visibility = "hidden";
                      resolutionLabel.style.visibility = "visible";
                  }
              }
          }
      </script> 
      <div class="newitem"><asp:CheckBox ID="UseIcons" runat="server" OnCheckedChanged="UseIcons_CheckedChange" AutoPostBack="true" Checked="true" Text="Use icons instead of text state strings"/></div>
      <div class="datagrid">
      <asp:GridView ID="IssuesGridView" runat="server" DataSourceID="VelocityDbDataSourceIssues" DataKeyNames="Id" GridLines="None" AllowSorting="True" AutoGenerateColumns="False" OnSelectedIndexChanged="IssuesGridView_SelectedIndexChanged" SortedAscendingHeaderStyle-CssClass="sortedASC" SortedDescendingHeaderStyle-CssClass="sortedDESC">
            <Columns>
                <asp:BoundField DataField="OidShort" HeaderText="Id" SortExpression="0" />
                <asp:TemplateField HeaderText="Priority" SortExpression="1" >
                  <ItemTemplate>
                    <asp:Image ID="PriorityImage" runat="server" ToolTip='<%# Bind("Priority") %>' ImageUrl='<%# GetPriorityImage(Container.DataItem) %>'>
                    </asp:Image>
                    <asp:Label ID="PriorityLabel" runat="server" Visible="false" Text='<%# Bind("Priority") %>'></asp:Label>
                  </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="DateTimeCreated" HeaderText="Created" ReadOnly="True" SortExpression="2" DataFormatString="{0:dd MMMM yyyy}" />
                <asp:TemplateField HeaderText="Status" SortExpression="3"
                  ItemStyle-VerticalAlign="Middle">
                  <ItemTemplate>
                    <asp:Image ID="StatusImage" runat="server" ToolTip='<%# Bind("Status") %>' ImageUrl='<%# GetStatusImage(Container.DataItem) %>'>
                    </asp:Image>
                    <asp:Label ID="StatusLabel" runat="server" Visible="false" Text='<%# Bind("Status") %>'></asp:Label>
                  </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Resolution" SortExpression="4" ItemStyle-VerticalAlign="Middle">
                <ItemTemplate>
                    <asp:Image ID="ResolutionImage" runat="server" ToolTip='<%# Bind("FixResolution") %>' ImageUrl='<%# GetResolutionImage(Container.DataItem) %>'>
                    </asp:Image>
                    <asp:Label ID="ResolutionLabel" runat="server" Visible="false" Text='<%# Bind("FixResolution") %>'></asp:Label>
                  </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Summary" HeaderText="Summary" ReadOnly="True" SortExpression="5" ItemStyle-Width="200"/>
                <asp:BoundField DataField="Component" HeaderText="Component" ReadOnly="True" SortExpression="6" />
                <asp:BoundField DataField="Version" HeaderText="Version" ReadOnly="True" SortExpression="7" />
                <asp:TemplateField HeaderText="Category" SortExpression="8">
                  <ItemTemplate>
                    <asp:Image ID="CategoryImage" runat="server" ToolTip='<%# Bind("Category") %>' ImageUrl='<%# GetCategoryImage(Container.DataItem) %>'>
                    </asp:Image>
                    <asp:Label ID="CategoryLabel" runat="server" Visible="false" Text='<%# Bind("Category") %>'></asp:Label>
                  </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ReportedBy" HeaderText="ReportedBy" ReadOnly="True" SortExpression="9" />
                <asp:BoundField DataField="LastUpdatedBy" HeaderText="LastUpdatedBy" ReadOnly="True" SortExpression="10" />
                <asp:BoundField DataField="DateTimeLastUpdated" HeaderText="LastUpdated" ReadOnly="True" SortExpression="11" DataFormatString="{0:dd MMMM yyyy}" />
                <asp:BoundField DataField="AssignedTo" HeaderText="AssignedTo" ReadOnly="True" SortExpression="12" />
                <asp:BoundField DataField="DueDate" HeaderText="DueDate" ReadOnly="True" SortExpression="13" DataFormatString="{0:dd MMMM yyyy}" />
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="IssueViewDetails" runat="server" Text="Edit" CommandName="Select" ClientIDMode="AutoID" />
                  <asp:LinkButton ID="DeleteIssueLinkButton" runat="server" OnClientClick="return confirm('Are you sure you want to delete this Issue?');"
                    CommandName="Delete" Text="Delete" />
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
      </div>
    </asp:View>
    <asp:View ID="IssuesFilter" runat="server">
    <div class="clear"></div>
    <asp:RadioButtonList ID="FilterOnOrOff" runat="server" AutoPostBack="true" RepeatDirection="Horizontal"
        OnSelectedIndexChanged="FilterOnOrOff_SelectedIndexChanged">
                      <asp:ListItem>On</asp:ListItem>
                      <asp:ListItem Selected="True">Off</asp:ListItem>
       </asp:RadioButtonList>
        <asp:TreeView RootNodeStyle-CssClass="treeview" NodeStyle-VerticalPadding="0" CssClass="treeview" ID="IssueFilterTree" runat="server">
        <Nodes>
          <asp:TreeNode Text="Filter" Value="0" Expanded="true">
            <asp:TreeNode Text="Id" Value="0" Expanded="True" ShowCheckBox="True">
          </asp:TreeNode>
            <asp:TreeNode Text="Date Time Created" Value="2" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
               <asp:TreeNode Text="Summary" Value="4" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
               <asp:TreeNode Text="Project" Value="5" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
               <asp:TreeNode Text="Component" Value="6" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
               <asp:TreeNode Text="Version" Value="7" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
                  <asp:TreeNode Text="Reported By" Value="9" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
                  <asp:TreeNode Text="Last Updated By" Value="10" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
                  <asp:TreeNode Text="DateTime Last Updated" Value="11" Expanded="False" ShowCheckBox="True"></asp:TreeNode>
                  <asp:TreeNode Text="Assigned To" Value="12" Expanded="False" ShowCheckBox="True">
               </asp:TreeNode>
                  <asp:TreeNode Text="Due Date" Value="13" Expanded="False" ShowCheckBox="True"></asp:TreeNode></asp:TreeNode>
              </Nodes>
        </asp:TreeView>
        <asp:TreeView RootNodeStyle-CssClass="treeview" NodeStyle-VerticalPadding="0" CssClass="treeview" ID="IssuePriorityTree" runat="server">
            <Nodes>
            <asp:TreeNode Text="Priority" Value="1" Expanded="True" ShowCheckBox="False">
              <asp:TreeNode Text="Blocker" Value="0" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/block32.png"></asp:TreeNode>
              <asp:TreeNode Text="Critical" Value="1" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/critical32.jpg"></asp:TreeNode>
              <asp:TreeNode Text="Major" Value="2" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/priorityMajor32.jpg"></asp:TreeNode>
              <asp:TreeNode Text="Minor" Value="3" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/minor32.jpg"></asp:TreeNode>
              <asp:TreeNode Text="Trivial" Value="4" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/trivial32.jpg"></asp:TreeNode>
            </asp:TreeNode>
           </Nodes>
        </asp:TreeView>
        <div class="treeview">
        <asp:TreeView RootNodeStyle-CssClass="treeview" NodeStyle-VerticalPadding="0" CssClass="treeview" ID="IssueStatusTree" runat="server">
        <Nodes>
            <asp:TreeNode Text="Status" Value="3" Expanded="True" ShowCheckBox="False">
              <asp:TreeNode Text="Open" Value="0" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/open32.jpg"></asp:TreeNode>
              <asp:TreeNode Text="InProgress" Value="1" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/inprogress32.jpg"></asp:TreeNode>
              <asp:TreeNode Text="Resolved" Value="2" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/resolved32.jpg"></asp:TreeNode>
              <asp:TreeNode Text="Reopened" Value="3" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/reopen32.png"></asp:TreeNode>
              <asp:TreeNode Text="Closed" Value="4" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/closed32.jpg"></asp:TreeNode>
            </asp:TreeNode>
              </Nodes>
        </asp:TreeView></div>
        <div class="treeview"><asp:TreeView RootNodeStyle-CssClass="treeview" NodeStyle-VerticalPadding="0" CssClass="treeview" ID="IssueCategoryTree" runat="server">
        <Nodes>
               <asp:TreeNode Text="Category" Value="8" Expanded="True" ShowCheckBox="False">
                  <asp:TreeNode Text="Bug" Value="0" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/bug32.png"></asp:TreeNode>
                    <asp:TreeNode Text="Improvement" Value="1" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/improvement32.png"></asp:TreeNode>
                    <asp:TreeNode Text="NewFeature" Value="2" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/newfeature32.png"></asp:TreeNode>
                    <asp:TreeNode Text="Task" Value="3" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/task32.png"></asp:TreeNode>
                    <asp:TreeNode Text="CustomIssue" Value="4" Expanded="False" ShowCheckBox="True" ImageUrl="~/images/genericissue32.png"></asp:TreeNode>
                  </asp:TreeNode>
              </Nodes>
        </asp:TreeView></div>
         <div class="clear"></div>
        <div class="table">
              <table>
                <tr>
                  <td>
                    <asp:Label ID="IdFromLabel" runat="server" Text="Id"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="IdFromDropDownList" runat="server" DataValueField="Id" DataTextField="Oid"
                      AutoPostBack="true" OnSelectedIndexChanged="IdFromDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="IdToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="IdToDropDownList" runat="server" DataValueField="Id" DataTextField="Oid"
                      AutoPostBack="true" OnSelectedIndexChanged="IdToDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="ProjectFromLabel" runat="server" Text="Project"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="ProjectFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="ProjectToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="ProjectToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="ComponentFromLabel" runat="server" Text="Component"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="ComponentFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="ComponentToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="ComponentToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="VersionLabel" runat="server" Text="Version"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="VersionFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="VersionToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="VersionToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="ReportedByFromLabel" runat="server" Text="ReportedBy"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="ReportedByFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="ReportedByToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="ReportedByToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="AssignedToFromLabel" runat="server" Text="AssignedTo"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList DataMember="AssignedTo" DataTextField="AssignedTo" ID="AssignedToFromDropDownList"
                      runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="AssignedToToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="AssignedToToDropDownList" DataMember="AssignedTo" DataTextField="AssignedTo"
                      runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="LastUpdatedByFrom" runat="server" Text="LastUpdatedBy"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="LastUpdatedByFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="LastUpdatedByTo" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="LastUpdatedByToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="DateTimeCreatedFromLabel" runat="server" Text="Created"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="DateTimeCreatedFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="DateTimeCreatedToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="DateTimeCreatedToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
                <tr>
                  <td>
                    <asp:Label ID="DateTimeLastUpdatedFromLabel" runat="server" Text="Last Updated"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="DateTimeLastUpdatedFromDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                  <td>
                    <asp:Label ID="DateTimeLastUpdatedToLabel" runat="server" Text="To"></asp:Label>
                  </td>
                  <td>
                    <asp:DropDownList ID="DateTimeLastUpdatedToDropDownList" runat="server">
                    </asp:DropDownList>
                  </td>
                </tr>
              </table></div>
       <asp:Button ID="SetFilterAttributesButton" runat="server" Text="Set Filters" OnClick="SetFiltersButton_Click" />
     </asp:View>
    <asp:View ID="IssueEdit" runat="server">
      <asp:Panel ID="IssuePanel" runat="server" CssClass="detail">
            <asp:DetailsView ID="IssueDetailsView" runat="server" DefaultMode="Edit" CssClass="detailgrid" Width="100%" DataSourceID="VelocityDbDataSourceIssue" AutoGenerateRows="false">
              <FieldHeaderStyle Width="100px" />
              <Fields>
                <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true"/>
                <asp:BoundField HeaderText="Oid" DataField="Oid" ReadOnly="true" />
                <asp:BoundField HeaderText="DateTimeCreated" DataField="DateTimeCreated" ReadOnly="true"  DataFormatString="{0:G}" />
                <asp:BoundField HeaderText="LastUpdated" DataField="DateTimeLastUpdated" ReadOnly="True" DataFormatString="{0:G}" />
                <asp:TemplateField HeaderText="DueDate">
                  <EditItemTemplate>
                    <asp:TextBox ID="DueDateEditTextBox" Text='<%# Bind("DueDate") %>' DataFormatString="{0:dd MMMM yyyy}" runat="server" />
                    <asp:CalendarExtender ID="DueDateCalender" runat="server" TargetControlID="DueDateEditTextBox" Format="MMMM d, yyyy"></asp:CalendarExtender>
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Summary">
                  <EditItemTemplate>
                    <asp:TextBox ID="IssueSummaryTextBox" runat="server" Text='<%# Bind("Summary") %>' Width="90%"/>
                    <asp:RequiredFieldValidator ID="rfvIssueSummary" runat="server" ControlToValidate="IssueSummaryTextBox" ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                 <asp:TemplateField HeaderText="Fix Message">
                  <EditItemTemplate>
                    <asp:TextBox ID="IssueFixMessageTextBox" runat="server" Text='<%# Bind("FixMessage") %>' Width="90%"/>
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description">
                  <EditItemTemplate>
                    <asp:TextBox ID="IssueDescriptionEditTextBox" runat="server" Text='<%# Bind("Description") %>' TextMode="MultiLine" Rows="10" Width="90%" />
                    <asp:RequiredFieldValidator ID="rfvIssueDescription" runat="server" ControlToValidate="IssueDescriptionEditTextBox" ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
<%--                <asp:TemplateField HeaderText="Project">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# ProjectIndex(DataBinder.Eval(Container.DataItem, "Project")) %>' OnSelectedIndexChanged="ProjectSelected" ID="IssueProjectDropDownList" runat="server" DataSource='<%# AllProjects("") %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>--%>
                <asp:TemplateField HeaderText="Component">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# ComponentIndex(DataBinder.Eval(Container.DataItem, "Component")) %>' OnSelectedIndexChanged="ComponentSelected" ID="IssueComponentDropDownList" runat="server" DataSource='<%# AllComponents("") %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Version">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# VersionIndex(DataBinder.Eval(Container.DataItem, "Version")) %>' OnSelectedIndexChanged="VersionSelected" ID="IssueVersionDropDownList" runat="server" DataSource='<%# AllVersions("") %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="AssignedTo">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# AssignedToIndex(DataBinder.Eval(Container.DataItem, "AssignedTo")) %>' OnSelectedIndexChanged="AssignedToSelected" ID="IssueAssignedToDropDownList" runat="server" DataSource='<%# AllUsers("") %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Status">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# StatusIndex(DataBinder.Eval(Container.DataItem, "Status")) %>' OnSelectedIndexChanged="StatusSelected" ID="IssueStatusDropDownList" runat="server"  DataSource='<%# AllStatus() %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Resolution">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# ResolutionIndex(DataBinder.Eval(Container.DataItem, "FixResolution")) %>' OnSelectedIndexChanged="ResolutionSelected" ID="IssueResolutionDropDownList" runat="server"  DataSource='<%# AllResolution() %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
                 <asp:TemplateField HeaderText="Category">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# CategoryIndex(DataBinder.Eval(Container.DataItem, "Category")) %>' OnSelectedIndexChanged="CategorySelected" ID="IssueCategoryDropDownList" runat="server"  DataSource='<%# AllCategory() %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Priority">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# PriorityIndex(DataBinder.Eval(Container.DataItem, "Priority")) %>' OnSelectedIndexChanged="PrioritySelected" ID="IssuePriorityDropDownList" runat="server"  DataSource='<%# AllPriority() %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField >
                  <asp:TemplateField HeaderText="Attachments">
                  <ItemTemplate>
                  <table>
                   <table>
                    <tr>
                      <td>
                      <asp:ListBox ID="UploadsListBox" runat="server" DataValueField="Id" DataTextField="FileName" DataSource='<%# Bind("AttachmentArray") %>' Width="100%" OnSelectedIndexChanged="UploadsListBox_SelectedIndexChanged" AutoPostBack="true"> </asp:ListBox>
                      </td>
                    </tr>
                    <tr>
                    <td>
                      <asp:FileUpload ID="AttachmentFileUpload" runat="server"   />
                      </td>
                    </tr>
                    </table>
                    <table>
                    <tr>
                      <td>
                        Comment
                      </td>
                      <td >
                        <asp:TextBox ID="UploadTextBox" runat="server" ></asp:TextBox>
                      </td>
                    </tr>
                    </table><table>
                    <tr>
                    <td>
                      <asp:Image ID="UploadImage" runat="server"  Visible="false" />
                    </td>
                    <td>
                    <table>
                      <tr>
                      <td>
                        <asp:LinkButton ID="DeleteAttachmentLinkButton" runat="server" Text="Delete Attachment" OnClick="DeleteAttachmentLinkButton_Click" AutoPostBack="true" />
                      </td></tr>
                      <tr><td>
                        <asp:LinkButton ID="OpenAttachmentLinkButton" runat="server" Text="Open Attachment" OnClick="OpenAttachmentLinkButton_Click" AutoPostBack="true" />
                      </td></tr>    
                    </table> 
                    </td>
                    <td>
                    </tr><tr>
                    <td>
                      <asp:LinkButton ID="UploadButton" runat="server" Text="Add Attachment" OnClick="UploadButton_Click" AutoPostBack="true" />
                      </td>
                    </tr>
                    </table>
                  </ItemTemplate>
                </asp:TemplateField>
              </Fields>
            </asp:DetailsView>
            <div class="footer">
              <asp:LinkButton ID="SaveIssueLinkButton" runat="server" Text="Save" OnClick="SaveIssueLinkButton_Click" CausesValidation="true" AutoPostBack="true" />
              <asp:LinkButton ID="CloseIssueLinkButton" runat="server" Text="Close" CausesValidation="false" OnClick="CloseIssueLinkButton_Click" AutoPostBack="true" />
            </div>
      </asp:Panel>
      <hr />
    </asp:View>
    <asp:View ID="ProjectsView" runat="server">
      <script type="text/javascript" language="javascript">
          //  attach to the pageLoaded event
          Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);

          function pageLoaded(sender, args) {

              //  the data key is the control's ID
              var dataKey = '<%= this.ProjectsGridView.ClientID %>';
              var updatedRowIndex = args.get_dataItems()[dataKey];

              //  if there is a datakey for the grid, use it to
              //  identify the row that was updated
              if (updatedRowIndex) {
                  //  get the row that was updated
                  var tr = $get(dataKey).rows[parseInt(updatedRowIndex) + 1];
                  //  add the 'updated' css class
                  Sys.UI.DomElement.addCssClass(tr, 'updated');

                  //  remove the css class in 1.5 seconds
                  window.setTimeout(function () {
                      Sys.UI.DomElement.removeCssClass(
                                    tr,
                                    'updated'
                                );
                  }, 1500);
              }
          }
      </script>
      <p><asp:LinkButton ID="NewProjectLinkButton" runat="server" Text="New Project" OnClick="NewProjectLinkButton_Click" /></p>
      <asp:UpdatePanel ID="ProjectUpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <asp:GridView ID="ProjectsGridView" runat="server" CssClass="datagrid" DataSourceID="VelocityDbDataSourceProjects"
            DataKeyNames="Id" GridLines="None" AllowSorting="True" AutoGenerateColumns="False"
            OnSelectedIndexChanged="ProjectsGridView_SelectedIndexChanged">
            <Columns>
              <asp:BoundField DataField="Oid" HeaderText="Id" SortExpression="0" />
              <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" SortExpression="1" />
              <asp:BoundField DataField="Description" HeaderText="Description" ReadOnly="True"
                SortExpression="2" />
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="btnViewDetails" runat="server" Text="Edit" CommandName="Select" />
                </ItemTemplate>
              </asp:TemplateField>
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="DeleteProjectLinkButton" runat="server" OnClientClick="return confirm('Are you sure you want to delete this Project?');"
                    CommandName="Delete" Text="Delete" />
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </ContentTemplate>
      </asp:UpdatePanel>
      <asp:Panel ID="ProjectPanel" runat="server" CssClass="detail" Width="500px" Style="display: none;">
        <asp:UpdatePanel ID="ProjectUpdatePanelDetail" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <asp:Button ID="ProjectShowPopupButton" runat="server" Style="display: none" />
            <asp:ModalPopupExtender ID="ProjectModalPopup" runat="server" TargetControlID="ProjectShowPopupButton"
              PopupControlID="ProjectPanel" CancelControlID="CloseProjectLinkButton" BackgroundCssClass="modalBackground" />
            <asp:DetailsView ID="ProjectDetailsView" runat="server" DefaultMode="Edit" Visible="false"
              CssClass="detailgrid" Width="100%" DataSourceID="VelocityDbDataSourceProject" AutoGenerateRows="false">
              <Fields>
                <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true" />
                <asp:BoundField HeaderText="Oid" DataField="Oid" ReadOnly="true" />
                <asp:TemplateField HeaderText="Name">
                  <EditItemTemplate>
                    <asp:TextBox ID="ProjectNameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                    <asp:RequiredFieldValidator ID="rfvProjectName" runat="server" ControlToValidate="ProjectNameTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description">
                  <EditItemTemplate>
                    <asp:TextBox ID="ProjectDescriptionEditTextBox" runat="server" Text='<%# Bind("Description") %>' />
                    <asp:RequiredFieldValidator ID="rfvProjectDescription" runat="server" ControlToValidate="ProjectDescriptionEditTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
              </Fields>
            </asp:DetailsView>
            <div class="footer">
              <asp:LinkButton ID="SaveProjectLinkButton" runat="server" Text="Save" OnClick="SaveProjectLinkButton_Click"
                CausesValidation="true" />
              <asp:LinkButton ID="CloseProjectLinkButton" runat="server" Text="Close" CausesValidation="false" />
            </div>
          </ContentTemplate>
        </asp:UpdatePanel>
      </asp:Panel>
      <asp:ObjectDataSource ID="VelocityDbDataSourceProjects" runat="server" SelectMethod="AllProjects"
        DeleteMethod="DeleteProject" InsertMethod="InsertProject" TypeName="VelocityWeb.Secure.Issues"
        SortParameterName="sortExpression"></asp:ObjectDataSource>
      <asp:ObjectDataSource ID="VelocityDbDataSourceProject" runat="server" InsertMethod="InsertProject"
        SelectMethod="SelectProject" UpdateMethod="UpdateProject" TypeName="VelocityWeb.Secure.Issues"
        DataObjectTypeName="VelocityWeb.EditedProject" OnSelecting="VelocityDbDataSourceProject_Selecting">
        <SelectParameters>
          <asp:Parameter Name="id" Type="Int32" />
        </SelectParameters>
      </asp:ObjectDataSource>
    </asp:View>
    <asp:View ID="ComponentsView" runat="server">
      <script type="text/javascript" language="javascript">
          //  attach to the pageLoaded event
          Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);

          function pageLoaded(sender, args) {

              //  the data key is the control's ID
              var dataKey = '<%= this.ComponentsGridView.ClientID %>';
              var updatedRowIndex = args.get_dataItems()[dataKey];

              //  if there is a datakey for the grid, use it to
              //  identify the row that was updated
              if (updatedRowIndex) {
                  //  get the row that was updated
                  var tr = $get(dataKey).rows[parseInt(updatedRowIndex) + 1];
                  //  add the 'updated' css class
                  Sys.UI.DomElement.addCssClass(tr, 'updated');

                  //  remove the css class in 1.5 seconds
                  window.setTimeout(function () {
                      Sys.UI.DomElement.removeCssClass(
                                    tr,
                                    'updated'
                                );
                  }, 1500);
              }
          }
      </script>
      <p><asp:LinkButton ID="NewComponentLinkButton" runat="server" Text="New Component" OnClick="NewComponentLinkButton_Click" /></p>
      <asp:UpdatePanel ID="ComponentUpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <asp:GridView ID="ComponentsGridView" runat="server" CssClass="datagrid" DataSourceID="VelocityDbDataSourceComponents"
            DataKeyNames="Id" GridLines="None" AllowSorting="True" AutoGenerateColumns="False"
            OnSelectedIndexChanged="ComponentsGridView_SelectedIndexChanged">
            <Columns>
              <asp:BoundField DataField="Oid" HeaderText="Id" SortExpression="0" />
              <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" SortExpression="1" />
              <asp:BoundField DataField="Description" HeaderText="Description" ReadOnly="True" SortExpression="2" />
              <asp:BoundField DataField="Project" HeaderText="Project" ReadOnly="True" SortExpression="3" />
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="LinkButton1" runat="server" Text="Edit" CommandName="Select" />
                </ItemTemplate>
              </asp:TemplateField>
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="DeleteComponentLinkButton" runat="server" OnClientClick="return confirm('Are you sure you want to delete this Component?');"
                    CommandName="Delete" Text="Delete" />
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </ContentTemplate>
      </asp:UpdatePanel>
      <asp:Panel ID="ComponentPanel" runat="server" CssClass="detail" Width="500px" Style="display: none;">
        <asp:UpdatePanel ID="ComponentUpdatePanelDetail" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <asp:Button ID="ComponentShowPopupButton" runat="server" Style="display: none" />
            <asp:ModalPopupExtender ID="ComponentModalPopup" runat="server" TargetControlID="ComponentShowPopupButton"
              PopupControlID="ComponentPanel" CancelControlID="CloseComponentLinkButton" BackgroundCssClass="modalBackground" />
            <asp:DetailsView ID="ComponentDetailsView" runat="server" DefaultMode="Edit" Visible="false"
              CssClass="detailgrid" Width="100%" DataSourceID="VelocityDbDataSourceComponent" AutoGenerateRows="false">
              <Fields>
                <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true"/>
                <asp:BoundField HeaderText="Oid" DataField="Oid" ReadOnly="true" />
                <asp:TemplateField HeaderText="Name">
                  <EditItemTemplate>
                    <asp:TextBox ID="ComponentNameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                    <asp:RequiredFieldValidator ID="rfvComponentName" runat="server" ControlToValidate="ComponentNameTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description">
                  <EditItemTemplate>
                    <asp:TextBox ID="ComponentDescriptionEditTextBox" runat="server" Text='<%# Bind("Description") %>' />
                    <asp:RequiredFieldValidator ID="rfvComponentDescription" runat="server" ControlToValidate="ComponentDescriptionEditTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Project">
                  <EditItemTemplate>                          
                   <asp:DropDownList SelectedIndex='<%# ProjectIndex(DataBinder.Eval(Container.DataItem, "Project")) %>' OnSelectedIndexChanged="ProjectSelected" ID="ComponentProject" runat="server" DataSource='<%# AllProjects("") %>'> </asp:DropDownList>                   
                 </EditItemTemplate>
                </asp:TemplateField>
              </Fields>
            </asp:DetailsView>
            <div class="footer">
              <asp:LinkButton ID="SaveComponentLinkButton" runat="server" Text="Save" OnClick="SaveComponentLinkButton_Click"
                CausesValidation="true" />
              <asp:LinkButton ID="CloseComponentLinkButton" runat="server" Text="Close" CausesValidation="false" />
            </div>
          </ContentTemplate>
        </asp:UpdatePanel>
      </asp:Panel>
      <asp:ObjectDataSource ID="VelocityDbDataSourceComponents" runat="server" SelectMethod="AllComponents"
        DeleteMethod="DeleteComponent" InsertMethod="InsertComponent" TypeName="VelocityWeb.Secure.Issues"
        SortParameterName="sortExpression"></asp:ObjectDataSource>
      <asp:ObjectDataSource ID="VelocityDbDataSourceComponent" runat="server" InsertMethod="InsertComponent"
        SelectMethod="SelectComponent" UpdateMethod="UpdateComponent" TypeName="VelocityWeb.Secure.Issues"
        DataObjectTypeName="VelocityWeb.EditedComponent" OnSelecting="VelocityDbDataSourceComponent_Selecting">
        <SelectParameters>
          <asp:Parameter Name="id" Type="UInt64" />
        </SelectParameters>
      </asp:ObjectDataSource>
    </asp:View>
    <asp:View ID="VersionsView" runat="server">
      <script type="text/javascript" language="javascript">
          //  attach to the pageLoaded event
          Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);

          function pageLoaded(sender, args) {

              //  the data key is the control's ID
              var dataKey = '<%= this.VersionsGridView.ClientID %>';
              var updatedRowIndex = args.get_dataItems()[dataKey];

              //  if there is a datakey for the grid, use it to
              //  identify the row that was updated
              if (updatedRowIndex) {
                  //  get the row that was updated
                  var tr = $get(dataKey).rows[parseInt(updatedRowIndex) + 1];
                  //  add the 'updated' css class
                  Sys.UI.DomElement.addCssClass(tr, 'updated');

                  //  remove the css class in 1.5 seconds
                  window.setTimeout(function () {
                      Sys.UI.DomElement.removeCssClass(
                                    tr,
                                    'updated'
                                );
                  }, 1500);
              }
          }
      </script>
      <p><asp:LinkButton ID="NewVersionLinkButton" runat="server" Text="New Version" OnClick="newVersionLinkButton_Click" /></p>
      <asp:UpdatePanel ID="VersionUpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <asp:GridView ID="VersionsGridView" runat="server" CssClass="datagrid" DataSourceID="VelocityDbDataSourceVersions"
            DataKeyNames="Id" GridLines="None" AllowSorting="True" AutoGenerateColumns="False"
            OnSelectedIndexChanged="VersionsGridView_SelectedIndexChanged">
            <Columns>
              <asp:BoundField DataField="Oid" HeaderText="Id" HeaderStyle-Font-Size="Small" SortExpression="0" />
              <asp:BoundField DataField="Name" HeaderText="Name" HeaderStyle-Font-Size="Small"
                ReadOnly="True" SortExpression="1" />
              <asp:BoundField DataField="Description" HeaderText="Description" HeaderStyle-Font-Size="Small"
                ReadOnly="True" SortExpression="2" />
              <asp:BoundField DataField="ReleaseDate" HeaderText="ReleaseDate" HeaderStyle-Font-Size="Small"
                ReadOnly="True" SortExpression="3" DataFormatString="{0:dd MMMM yyyy}" />
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="LinkButton2" runat="server" Text="Edit" CommandName="Select" />
                </ItemTemplate>
              </asp:TemplateField>
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="DeleteVersionLinkButton" runat="server" OnClientClick="return confirm('Are you sure you want to delete this ProductVersion?');"
                    CommandName="Delete" Text="Delete" />
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </ContentTemplate>
      </asp:UpdatePanel>
      <asp:Panel ID="pnlPopup" runat="server" CssClass="detail" Width="500px" Style="display: none;">
        <asp:UpdatePanel ID="VersionUpdatePanelDetail" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <asp:Button ID="btnShowPopup" runat="server" Style="display: none" />
            <asp:ModalPopupExtender ID="VersionModalPopup" runat="server" TargetControlID="btnShowPopup"
              PopupControlID="pnlPopup" CancelControlID="btnClose" BackgroundCssClass="modalBackground" />
            <asp:DetailsView ID="VersionDetailsView" runat="server" DefaultMode="Edit" Visible="false"
              CssClass="detailgrid" Width="100%" DataSourceID="VelocityDbDataSourceVersion" AutoGenerateRows="false">
              <Fields>
                <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true" />
                <asp:BoundField HeaderText="Oid" DataField="Oid" ReadOnly="true" />
                <asp:TemplateField HeaderText="Name">
                  <EditItemTemplate>
                    <asp:TextBox ID="VersionNameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                    <asp:RequiredFieldValidator ID="rfvVersionName" runat="server" ControlToValidate="VersionNameTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description">
                  <EditItemTemplate>
                    <asp:TextBox ID="DescriptionEditTextBox" runat="server" Text='<%# Bind("Description") %>' />
                    <asp:RequiredFieldValidator ID="rfvVersionDescription" runat="server" ControlToValidate="DescriptionEditTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="ReleaseDate">
                  <EditItemTemplate>
                    <asp:TextBox ID="ReleaseDateEditTextBox" Text='<%# Bind("ReleaseDate") %>' DataFormatString="{0:dd MMMM yyyy}"
                      runat="server" />
                    <asp:CalendarExtender ID="VersionReleaseDateCalender" runat="server" TargetControlID="ReleaseDateEditTextBox"
                      Format="MMMM d, yyyy"></asp:CalendarExtender>
                  </EditItemTemplate>
                </asp:TemplateField>
              </Fields>
            </asp:DetailsView>
            <div class="footer">
              <asp:LinkButton ID="SaveVersionLinkButton" runat="server" Text="Save" OnClick="SaveVersionLinkButton_Click"
                CausesValidation="true" />
              <asp:LinkButton ID="btnClose" runat="server" Text="Close" CausesValidation="false" />
            </div>
          </ContentTemplate>
        </asp:UpdatePanel>
      </asp:Panel>
      <asp:ObjectDataSource ID="VelocityDbDataSourceVersions" runat="server" SelectMethod="AllVersions"
        DeleteMethod="DeleteVersion" InsertMethod="InsertVersion" TypeName="VelocityWeb.Secure.Issues"
        SortParameterName="sortExpression"></asp:ObjectDataSource>
      <asp:ObjectDataSource ID="VelocityDbDataSourceVersion" runat="server" InsertMethod="InsertVersion"
        SelectMethod="SelectVersion" UpdateMethod="UpdateVersion" TypeName="VelocityWeb.Secure.Issues"
        DataObjectTypeName="VelocityWeb.EditedVersion" OnSelecting="VelocityDbDataSourceVersion_Selecting">
        <SelectParameters>
          <asp:Parameter Name="id" Type="Int32" />
        </SelectParameters>
      </asp:ObjectDataSource>
    </asp:View>
    <asp:View ID="UsersView" runat="server">
      <script type="text/javascript" language="javascript">
          //  attach to the pageLoaded event
          Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);

          function pageLoaded(sender, args) {

              //  the data key is the control's ID
              var dataKey = '<%= this.UsersGridView.ClientID %>';
              var updatedRowIndex = args.get_dataItems()[dataKey];

              //  if there is a datakey for the grid, use it to
              //  identify the row that was updated
              if (updatedRowIndex) {
                  //  get the row that was updated
                  var tr = $get(dataKey).rows[parseInt(updatedRowIndex) + 1];
                  //  add the 'updated' css class
                  Sys.UI.DomElement.addCssClass(tr, 'updated');

                  //  remove the css class in 1.5 seconds
                  window.setTimeout(function () {
                      Sys.UI.DomElement.removeCssClass(
                                    tr,
                                    'updated'
                                );
                  }, 1500);
              }
          }
      </script>
      <p><asp:LinkButton ID="NewUserLinkButton" runat="server" Text="New User" OnClick="newUserLinkButton_Click" /></p>
      <asp:UpdatePanel ID="UsersUpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <asp:GridView ID="UsersGridView" runat="server" CssClass="datagrid" DataSourceID="VelocityDbDataSourceUsers"
            DataKeyNames="Id" GridLines="None" AllowSorting="True" AutoGenerateColumns="False"
            OnSelectedIndexChanged="UsersGridView_SelectedIndexChanged">
            <Columns>
              <asp:BoundField DataField="Oid" HeaderText="Id" HeaderStyle-Font-Size="Small" SortExpression="0" />
              <asp:BoundField DataField="FirstName" HeaderText="FirstName" ReadOnly="True" SortExpression="1" />
              <asp:BoundField DataField="LastName" HeaderText="LastName" ReadOnly="True" SortExpression="2" />
              <asp:BoundField DataField="Email" HeaderText="Email" ReadOnly="True" SortExpression="3" /> 
              <asp:BoundField DataField="UserName" HeaderText="UserName" ReadOnly="True" SortExpression="4" />
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="LinkButton3" runat="server" Text="Edit" CommandName="Select" />
                </ItemTemplate>
              </asp:TemplateField>
              <asp:TemplateField>
                <ItemTemplate>
                  <asp:LinkButton ID="DeleteUserLinkButton" runat="server" OnClientClick="return confirm('Are you sure you want to delete this User?');"
                    CommandName="Delete" Text="Delete" />
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </ContentTemplate>
      </asp:UpdatePanel>
      <asp:Panel ID="UserPanel" runat="server" CssClass="detail" Width="500px" Style="display: none;">
        <asp:UpdatePanel ID="UserUpdatePanelDetail" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <asp:Button ID="UserEditButton" runat="server" Style="display: none" />
            <asp:ModalPopupExtender ID="UserModalPopup" runat="server" TargetControlID="UserEditButton"
              PopupControlID="UserPanel" CancelControlID="CloseUserLinkButton" BackgroundCssClass="modalBackground" />
            <asp:DetailsView ID="UserDetailsView" runat="server" DefaultMode="Edit" Visible="false"
              CssClass="detailgrid" Width="100%" DataSourceID="VelocityDbDataSourceUser" AutoGenerateRows="false">
              <Fields>
                <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true"/>
                <asp:BoundField HeaderText="Oid" DataField="Oid" ReadOnly="true" />
                <asp:BoundField HeaderText="DateTimeCreated" DataField="DateTimeCreated" ReadOnly="true"  DataFormatString="{0:G}" />
                <asp:BoundField HeaderText="CreatedBy" DataField="CreatedBy" ReadOnly="True"/>
                <asp:TemplateField HeaderText="FirstName">
                  <EditItemTemplate>
                    <asp:TextBox ID="FirstNameTextBox" runat="server" Text='<%# Bind("FirstName") %>' />
                    <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="FirstNameTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="LastName">
                  <EditItemTemplate>
                    <asp:TextBox ID="LastNameTextBox" runat="server" Text='<%# Bind("LastName") %>' />
                    <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="LastNameTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Email">
                  <EditItemTemplate>
                    <asp:TextBox ID="EmailEditTextBox" runat="server" Text='<%# Bind("Email") %>' />
                    <asp:RequiredFieldValidator ID="rfvVersionEmail" runat="server" ControlToValidate="EmailEditTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="UserName">
                  <EditItemTemplate>
                    <asp:TextBox ID="UserNameEditTextBox" Text='<%# Bind("UserName") %>' runat="server" />
                    <asp:RequiredFieldValidator ID="rfvUserName" runat="server" ControlToValidate="UserNameEditTextBox"
                      ErrorMessage="Required" Display="Static" SetFocusOnError="true" />
                  </EditItemTemplate>
                </asp:TemplateField>
              </Fields>
            </asp:DetailsView>
            <div class="footer">
              <asp:LinkButton ID="SaveUserLinkButton" runat="server" Text="Save" OnClick="SaveUserLinkButton_Click"
                CausesValidation="true" />
              <asp:LinkButton ID="CloseUserLinkButton" runat="server" Text="Close" CausesValidation="false" />
            </div>
          </ContentTemplate>
        </asp:UpdatePanel>
      </asp:Panel>
      <asp:ObjectDataSource ID="VelocityDbDataSourceUsers" runat="server" SelectMethod="AllUsers"
        DeleteMethod="DeleteUser" InsertMethod="InsertUser" TypeName="VelocityWeb.Secure.Issues"
        SortParameterName="sortExpression"></asp:ObjectDataSource>
      <asp:ObjectDataSource ID="VelocityDbDataSourceUser" runat="server" InsertMethod="InsertUser"
        SelectMethod="SelectUser" UpdateMethod="UpdateUser" TypeName="VelocityWeb.Secure.Issues"
        DataObjectTypeName="VelocityWeb.EditedUser" OnSelecting="VelocityDbDataSourceUser_Selecting">
        <SelectParameters>
          <asp:Parameter Name="id" Type="UInt64" />
        </SelectParameters>
      </asp:ObjectDataSource>
    </asp:View>
    <asp:View ID="PermissionsView" runat="server">
    <div class="permissions">
      <table class="permissions">
      <tr>
        <td>Admin Users</td>
        <td>Developer Users</td>
        <td>Regular Users</td>
      </tr>
      <tr>
        <td><asp:ListBox ID="AdminUsers" runat="server" Width="200" 
            SelectionMode="Multiple"></asp:ListBox></td>
        <td><asp:ListBox ID="DeveloperUsers" runat="server" Width="200" 
            SelectionMode="Multiple"></asp:ListBox></td>
        <td><asp:ListBox ID="RegularUsers" runat="server" Width="200" 
            SelectionMode="Multiple"></asp:ListBox></td>
      </tr>
        <tr>
          <td><asp:Button ID="RemoveAdminUsers" runat="server" Text="Remove Selected Users" 
              Width="200px" onclick="RemoveAdminUsers_Click" /></td>
          <td><asp:Button ID="RemoveDeveloperUsers" runat="server" 
              Text="Remove Selected Users" Width="200px" 
              onclick="RemoveDeveloperUsers_Click" /></td>
          <td><asp:Button ID="AddAdminUsers" runat="server" 
              Text="Add Selected to Admin Users" Width="200px" 
              onclick="AddAdminUsers_Click" /></td>
        </tr>
        <tr>
        <td></td>
        <td></td>
        <td><asp:Button ID="AddDeveloperUsers" runat="server" 
            Text="Add Selected to Developer Users" Width="200px" 
            onclick="AddDeveloperUsers_Click" /></td>
        </tr><tr>
        <td><asp:Button ID="SetAdminPermissions" runat="server" 
            Text="Set Admin Permissions" Width="200px" 
            onclick="SetAdminPermissions_Click" /></td>
        <td><asp:Button ID="SetDevelopersPermissions" runat="server" 
            Text="Set Developers Permissions" Width="200px" 
            onclick="SetDevelopersPermissions_Click" /></td>
        <td><asp:Button ID="SetRegularPermissions" runat="server" 
            Text="Set Regular Permissions" Width="200px" 
            onclick="SetRegularPermissions_Click" /></td>
        </tr><tr>
        <td><asp:ListBox ID="AdminPermissions" runat="server" Width="200" SelectionMode="Multiple"></asp:ListBox></td>
        <td><asp:ListBox ID="DevelopersPermissions" runat="server" Width="200" SelectionMode="Multiple"></asp:ListBox></td>       
        <td><asp:ListBox ID="RegularPermissions" runat="server" Width="200" SelectionMode="Multiple"></asp:ListBox></td> 
        </tr></table></div>
    </asp:View>
  </asp:MultiView>
</asp:Content>