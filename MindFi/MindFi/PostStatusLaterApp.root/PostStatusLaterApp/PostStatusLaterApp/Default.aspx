<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="PostStatusLaterApp._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        POST YOUR STATUS IN FACEBOOK WHILE YOU ARE OUT</h2>
    <p>
        <asp:Label ID="lblError" runat="server" Text="Error"></asp:Label>
    </p>
    <p>
        Status you want to post:
        <asp:TextBox ID="txtStatus" runat="server" Width="714px">what will you be thinking?</asp:TextBox>
    </p>
    <p>
        Day/time you want it posted (your time zone is
        <asp:HyperLink ID="HyperLink1" runat="server">currently UDT</asp:HyperLink>
        )</p>
    <p>
        <asp:Calendar ID="calPost" runat="server" 
            onselectionchanged="Calendar1_SelectionChanged"></asp:Calendar>
        at
        <asp:DropDownList ID="lstHour" runat="server">
        </asp:DropDownList>
        :<asp:DropDownList ID="lstMinute" runat="server">
        </asp:DropDownList>
&nbsp;hrs</p>
    <p>
        <asp:Button ID="btnPost" runat="server" Text="Program the post" />
    </p>
</asp:Content>
