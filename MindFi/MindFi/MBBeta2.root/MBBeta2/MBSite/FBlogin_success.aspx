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
<font color="white">
<asp:Label ID="lblAccessToken" runat="server"></asp:Label>
<asp:Label ID="lblExpires" runat="server"></asp:Label>
</font>
</div>
    </form>
</asp:Content>
