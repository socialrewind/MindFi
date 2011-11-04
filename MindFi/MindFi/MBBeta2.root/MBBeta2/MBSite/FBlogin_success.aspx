<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FBlogin_success.aspx.cs" Inherits="MBSite.FBlogin_success" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body bgcolor="white">
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
</body>
</html>
