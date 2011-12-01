<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site1.master" AutoEventWireup="true"
    CodeBehind="FBlogin_success.aspx.cs" Inherits="MBSite.FBlogin_success" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <form id="form1" runat="server">
<font color="black">
<p>
<asp:Label ID="lblResult" runat="server">Login successful!</asp:Label>
</p>
</font>
<div>
    Welcome to Social Rewind, thanks for participating in our Beta program. This 
    window will automatically close soon.</div>
    </form>
</asp:Content>
