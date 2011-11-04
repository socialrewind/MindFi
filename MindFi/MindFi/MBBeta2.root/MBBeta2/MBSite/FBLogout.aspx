<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FBLogout.aspx.cs" Inherits="MBSite.FBLogout" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Social Rewind Logout</title>
</head>
<body>
    <form id="form1" runat="server">
<div id="fb-root">
<script type="text/javascript" language="javascript">
<!--
    window.fbAsyncInit = function () {
        FB.init({
            appId: '131706850230259', cookie: true,
            status: true, xfbml: true
        });
        FB.getLoginStatus(function (response) {
            if (response.session) {
                FB.logout(function (response) {
                    // user is now logged out
                    alert('Logged out successfully');
                });
            } // if response
        }); // getLogin STatus
    }; // fbAsyncInit
    (function () {
        var e = document.createElement('script'); e.async = true;
        e.src = document.location.protocol +
      '//connect.facebook.net/en_US/all.js';
        document.getElementById('fb-root').appendChild(e);
    } ());

// -->
</script>
</div>
Logging out...
</form>
</body>
</html>